
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Reus2Surveyor
{
    public partial class FormMain : Form
    {
        public static readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string decodedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decoded");
        public static readonly string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProfileDir { get; private set; } = "";
        public bool profileDirOK = false;

        private List<Planet> planetList = [];
        private int planetsTried, planetsOk, planetsTotal = 0;

        public static readonly Glossaries GameGlossaries = new(Path.Combine(baseDir, "Glossaries"));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastSpotCheckDir { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool WriteDecodedSetting { get; private set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SpotCheckWriteSetting { get; private set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HeatmapsEnabledSetting { get; private set; } = false;

        public StatCollector PlanetStatCollector;

        private Dictionary<int, PlanetFileUtil.SaveSlotManager> planetsInProfile = [];
        private Dictionary<int, string> filesToProcess { get; set; } = [];
        private DateTime decodeStartTime;

        public FormMain()
        {
            InitializeComponent();
            this.planetGridView.RowTemplate.Height = 25;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(decodedDir);
            Directory.CreateDirectory(outputDir);
        }

        private void findProfileButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new()
            {
                Description = "Locate your Reus 2 save folder."
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dialog.SelectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                if (this.profileDirOK) dialog.SelectedPath = this.ProfileDir;
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.SetAndCheckProfilePath(dialog.SelectedPath);
            }
        }

        public void SetAndCheckProfilePath(string value)
        {
            if (value == this.ProfileDir)
            {
                return;
            }

            this.planetGridView.Rows.Clear();
            this.ResetPlanetList();
            this.updateDecodeProgress();

            if (Path.GetFileName(value).StartsWith("profile_"))
            {
                this.ProfileDir = value;
                this.profileFolderTextBox.Text = this.ProfileDir;
                this.profileFolderTextBox.BackColor = this.profileFolderTextBox.BackColor;
                this.profileFolderTextBox.ForeColor = System.Drawing.Color.Green;
                this.profileDirOK = true;
                this.decodeReadyStatusLabel.Text = "Ready";
                this.GetPlanetsInProfile();
                this.InitializePlanetGridView();
            }
            else
            {
                this.ProfileDir = value;
                this.profileFolderTextBox.Text = this.ProfileDir;
                this.profileFolderTextBox.BackColor = this.profileFolderTextBox.BackColor;
                this.profileFolderTextBox.ForeColor = System.Drawing.Color.Red;
                this.profileDirOK = false;
                this.decodeReadyStatusLabel.Text = "Not Ready";
            }
        }

        public void ResetPlanetList()
        {
            this.planetList.Clear();
            this.planetsOk = 0;
            this.planetsTotal = 0;
            this.planetsTried = 0;
            this.InitialPlanetListLockouts();
        }

        public void InitialPlanetListLockouts()
        {
            this.decodeReadyStatusLabel.Text = "Not Ready";
            this.decodeReadyStatusLabel.ForeColor = System.Drawing.Color.Black;
            this.decodeReadyStatusLabel.Refresh();
            this.exportReadyLabel.Text = "Not Ready";
            this.exportReadyLabel.ForeColor = System.Drawing.Color.Black;
            this.exportStatsButton.Enabled = false;
        }

        public void SetCheckWriteDecodedSetting(bool value)
        {
            this.WriteDecodedSetting = value;
            this.writeDecodedCheckBox.Checked = value;
        }

        public void SetSpotCheckWriteSetting(bool value)
        {
            this.SpotCheckWriteSetting = value;
            this.spotCheckWriteCheckBox.Checked = value;
        }

        public void SetHeatmapsEnabledSetting(bool value)
        {
            this.HeatmapsEnabledSetting = value;
            this.heatmapCheckbox.Checked = value;
        }

        public void GetPlanetsInProfile()
        {
            List<string> allPlanetPaths = [.. Directory.GetDirectories(Path.Combine(this.ProfileDir, "sessions"))];
            allPlanetPaths.Sort();
            List<PlanetFileUtil.SaveSlotManager> availablePlanetSaves = [.. allPlanetPaths.Select(x => new PlanetFileUtil.SaveSlotManager(x))];
            this.planetsInProfile = availablePlanetSaves.Select((x, ind) => new { x, ind }).ToDictionary(x => x.ind, x => x.x);
            this.planetCountLabel.Text = this.planetsInProfile.Count.ToString();
        }

        public void InitializePlanetGridView()
        {
            this.planetGridView.Rows.Clear();
            foreach (KeyValuePair<int, PlanetFileUtil.SaveSlotManager> kv in this.planetsInProfile)
            {
                this.AddPlanetGridViewRow(kv.Value);
            }
        }

        public void AddPlanetGridViewRow(PlanetFileUtil.SaveSlotManager ssm)
        {
            int index = this.planetGridView.Rows.Add();
            DataGridViewRow thisRow = this.planetGridView.Rows[index];
            thisRow.Cells["NameCol"].Value = PlanetFileUtil.PlanetNameFromPlanetFolderPath(ssm.parentPath);
            if (ssm.Complete.valid)
            {
                thisRow.Cells["CompletionCol"].Value = "Complete";
                thisRow.Cells["ReadOptionCol"].Value = true;
            }
            else
            {
                thisRow.Cells["ReadOptionCol"].ReadOnly = true;
            }
        }

        private void decodeButton_Click(object sender, EventArgs e)
        {
            if (this.profileDirOK)
            {
                this.ResetPlanetList();
                Dictionary<int, string> completedPlanetPaths = this.planetsInProfile.Where(kv => kv.Value.Complete.valid).Select(kv => new KeyValuePair<int, string>(kv.Key, kv.Value.Complete.path)).ToDictionary();

                List<int> readOptionOff = [];
                foreach (DataGridViewRow r in this.planetGridView.Rows)
                {
                    r.Cells["SpiritCol"].Value = null;
                    r.Cells["ScoreCol"].Value = null;
                    r.Cells["ReadStatusCol"].Value = null;

                    r.Cells["SpiritIconCol"].Value = null;
                    r.Cells["Giant1Col"].Value = null;
                    r.Cells["Giant2Col"].Value = null;
                    r.Cells["Giant3Col"].Value = null;
                    r.Cells["MinimapCol"].Value = null;

                    bool? readOption = r.Cells["ReadOptionCol"].Value is null ? null : (bool)r.Cells["ReadOptionCol"].Value;
                    if (readOption is null || !(bool)readOption)
                    {
                        readOptionOff.Add(r.Index);
                    }
                }
                foreach (int ro in readOptionOff)
                {
                    completedPlanetPaths.Remove(ro);
                    this.planetGridView.Rows[ro].Cells["SpiritCol"].Value = "Skipped";
                    this.planetGridView.Rows[ro].Cells["ScoreCol"].Value = "Skipped";
                }
                int completedPlanetCount = completedPlanetPaths.Count();

                this.decodeProgressBar.Maximum = completedPlanetCount;
                this.decodeStartTime = DateTime.Now;
                this.updateDecodeProgress();
                this.filesToProcess = completedPlanetPaths;
                this.planetLooperBackgroundWorker.RunWorkerAsync();
            }
            else
            {
                this.decodeReadyStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void LoopThroughPlanetSaves()
        {
            this.planetList = [.. Enumerable.Repeat((Planet)null, this.planetsInProfile.Count)];
            this.planetsTotal = this.filesToProcess.Count;
            foreach ((int index, string path) in this.filesToProcess)
            {
                ProcessPlanet(index, path);
            }
        }

        public void ProcessPlanet(int index, string path)
        {
            if (path is null)
            {
                // TODO: Update the table for a skipped file
                this.planetsTried++;
                return;
            }

            List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
            pathParts.Reverse();
            bool readPlanetOK = false;
            Planet newPlanet = null;
            string planetName = null;
            Dictionary<string, object> resAsDict = null;
            try
            {
                resAsDict = PlanetFileUtil.ReadDictFromFile(path);
                planetName = PlanetFileUtil.PlanetNameFromSaveFilePath(path);
                newPlanet = PlanetFileUtil.InterpretDictAsPlanet(resAsDict, path);
                newPlanet.number = index;
            }
            catch (Exception e)
            {
                newPlanet = null;
                Program.TracePlanetException(e, pathParts[1] + Path.DirectorySeparatorChar + pathParts[0]);
            }

            this.planetsTried++;
            if (newPlanet is not null)
            {
                this.planetList[index] = newPlanet;
                readPlanetOK = true;
                this.planetsOk++;
                newPlanet.SetGlossaryThenLookup(GameGlossaries);

                this.UpdatePlanetGrid(index, newPlanet);

                // Write decoded file
                if (this.WriteDecodedSetting)
                {
                    string dst = Path.Combine(decodedDir, pathParts[1] + "." + pathParts[0] + ".json");
                    string outputText;
                    if (!File.Exists(dst))
                    {
                        outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                        File.WriteAllText(dst, outputText);
                    }
                    else
                    {
                        DateTime dstLastWrite = File.GetLastWriteTimeUtc(dst);
                        DateTime srcLastWrite = File.GetLastWriteTimeUtc(path);
                        if (srcLastWrite > dstLastWrite)
                        {
                            outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                            File.WriteAllText(dst, outputText);
                        }
                    }
                }
            }
            else
            {
                this.planetList[index] = newPlanet;
                this.planetGridView.Rows[index].Cells["ReadStatusCol"].Value = "Failed";
            }

            if (this.planetLooperBackgroundWorker.IsBusy) this.planetLooperBackgroundWorker.ReportProgress(1);
            else this.updateDecodeProgress();
            if (readPlanetOK)
            {

            }
            else
            {
                Trace.TraceError("Failed to read planet file: " + pathParts[1] + "/" + pathParts[0]);
            }
        }

        public void UpdatePlanetGrid(int index, Planet newPlanet)
        {
            string spiritName = GameGlossaries.SpiritNameFromHash(newPlanet.gameSession.selectedCharacterDef);
            DataGridViewRow thisRow = this.planetGridView.Rows[index];

            thisRow.Cells["ScoreCol"].Value = newPlanet.gameSession.turningPointPerformances.Last().scoreTotal.ToString();
            thisRow.Cells["SpiritCol"].Value = spiritName;
            thisRow.Cells["ReadStatusCol"].Value = "OK";

            if (TableGraphics.spiritSquares.TryGetValue(spiritName, out byte[] spiritImage)) { thisRow.Cells["SpiritIconCol"].Value = spiritImage; }
            else thisRow.Cells["SpiritIconCol"].Value = Properties.Resources.ErrorSquare;


            if (TableGraphics.giantSquares.TryGetValue(newPlanet.GiantNames[0], out byte[] giant1Image)) { thisRow.Cells["Giant1Col"].Value = giant1Image; }
            else thisRow.Cells["Giant1Col"].Value = Properties.Resources.ErrorSquare;
            if (TableGraphics.giantSquares.TryGetValue(newPlanet.GiantNames[1], out byte[] giant2Image)) { thisRow.Cells["Giant2Col"].Value = giant2Image; }
            else thisRow.Cells["Giant2Col"].Value = Properties.Resources.ErrorSquare;
            if (TableGraphics.giantSquares.TryGetValue(newPlanet.GiantNames[2], out byte[] giant3Image)) { thisRow.Cells["Giant3Col"].Value = giant3Image; }
            else thisRow.Cells["Giant3Col"].Value = Properties.Resources.ErrorSquare;

            SixLabors.ImageSharp.Image miniMap = TableGraphics.BiomePositionalToMinimap(newPlanet.BiomeSizeMap, GameGlossaries);
            using MemoryStream ms = new MemoryStream();
            miniMap.SaveAsPng(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            thisRow.Cells["MinimapCol"].Value = ms.ToArray();
        }

        private void updateDecodeProgress()
        {
            this.decodeProgressLabel.Text = String.Format("Planets ({0}/{1}), {2} OK", this.planetsTried, this.planetsTotal, this.planetsOk);
            this.decodeProgressLabel.Refresh();

            if (this.planetsTried < this.planetsTotal) this.decodeProgressBar.Value = this.planetsTried;
            else if (this.planetsTotal == 0)
            {
                this.decodeProgressBar.Value = 0;
                this.decodeProgressLabel.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                this.decodeProgressBar.Value = this.decodeProgressBar.Maximum;
                this.decodeProgressLabel.Text += " - Done!";
                double decodeSeconds = (DateTime.Now - this.decodeStartTime).TotalSeconds;
                this.decodeProgressLabel.Text += $" ({decodeSeconds:N2} s)";
                this.decodeProgressLabel.ForeColor = System.Drawing.Color.Green;
            }
        }

        private void exportStatsButton_Click(object sender, EventArgs e)
        {
            DateTime exportStart = DateTime.Now;
            exportStatsButton.Enabled = false;
            int i = -1;
            this.PlanetStatCollector = new(GameGlossaries);
            foreach (Planet planet in this.planetList)
            {
                i++;
                if (planet is null) continue;
                this.PlanetStatCollector.ConsumePlanet(planet, i);
            }
            this.PlanetStatCollector.FinalizeStats();

            string dst = Path.Combine(outputDir, "Reus 2 Stats " + DateTime.Now.ToString("yyyyMMdd HHmm") + ".xlsx");
            this.PlanetStatCollector.WriteToExcel(dst, this.heatmapCheckbox.Checked);
            exportStatsButton.Enabled = true;
            string timeMsg = $"({((DateTime.Now - exportStart).TotalSeconds):N2} s)";
            this.exportReadyLabel.Text = "Export Complete " + timeMsg;
            this.exportReadyLabel.ForeColor = System.Drawing.Color.Green;

            // Ternary plot testing
            //List<(double, double, double)> dataSet = [.. this.PlanetStatCollector.PlanetSummaries.Select(ps => (ps.PPlant ?? 0, ps.PAnimal ?? 0, ps.PMineral ?? 0))];
            //TernaryTileHeatmap tp = new(10, dataSet);
            //Func<double, double, (int, int, int), Color> shader = TernaryTileHeatmap.MakeSimpleShader(Color.DarkMagenta);
            //Image im = tp.DrawStandardPlot(Color.White, shader,
            //    title: "Biotica Types\nAll Planets",
            //    labelA: "Plant",
            //    labelB: "Animal",
            //    labelC: "Mineral"
            //    );
            //im.SaveAsPng("testImage.png");

            // Invention/Luxury spading/debugging
            //File.WriteAllLines("Invention Names.csv", this.PlanetStatCollector.inventionNamesByDef.Select(kv => String.Join(",", [kv.Key, kv.Value])));
        }

        private void spotCheckButton_Click(object sender, EventArgs e)
        {
            if (!Path.Exists(this.LastSpotCheckDir))
            {
                this.LastSpotCheckDir = this.ProfileDir;
            }
            OpenFileDialog openFile = new()
            {
                InitialDirectory = this.LastSpotCheckDir,
                DefaultExt = ".deux"
            };

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string path = openFile.FileName;
                Planet testPlanet = null;
                Dictionary<string, object> resAsDict = null;
                bool planetOK = false;
                string planetName = null;

                resAsDict = PlanetFileUtil.ReadDictFromFile(path);
                planetName = PlanetFileUtil.PlanetNameFromSaveFilePath(path);

                this.LastSpotCheckDir = Path.GetDirectoryName(path);

                List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
                pathParts.Reverse();

                if (this.SpotCheckWriteSetting)
                {
                    string dst = Path.Combine(decodedDir, pathParts[1] + "." + pathParts[0] + ".json");
                    string outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                    File.WriteAllText(dst, outputText);
                }

                testPlanet = PlanetFileUtil.InterpretDictAsPlanet(resAsDict, path);
                testPlanet.SetGlossaryThenLookup(GameGlossaries);
                planetOK = true;

                StatCollector sc;

                if (planetOK)
                {
                    // Counting biotica
                    // For getting definition strings for biotica
                    Dictionary<string, int> bioticaCounter = [];
                    foreach (NatureBioticum nb in testPlanet.natureBioticumDictionary.Values)
                    {
                        string bioName = GameGlossaries.BioticumNameFromHash(nb.definition);
                        if (bioticaCounter.ContainsKey(bioName)) bioticaCounter[bioName] += 1;
                        else bioticaCounter[bioName] = 1;
                    }
                    List<string> singleBiotica = [.. bioticaCounter.Where(kv => kv.Value == 1).Select(kv => kv.Key)];
                    List<string> dualBiotica = [.. bioticaCounter.Where(kv => kv.Value == 2).Select(kv => kv.Key)];
                    List<string> tripleBiotica = [.. bioticaCounter.Where(kv => kv.Value == 3).Select(kv => kv.Key)];
                    string bio1, bio2, bio3;
                    bio1 = singleBiotica.Count > 0 ? singleBiotica[0] : null;
                    bio2 = dualBiotica.Count > 0 ? dualBiotica[0] : null;
                    bio3 = tripleBiotica.Count > 0 ? tripleBiotica[0] : null;
                    string bio123;
                    if (bio1 is not null && bio2 is not null && bio3 is not null)
                    {
                        bio123 = String.Join('\n', [bio1, bio2, bio3]);
                    }

                    sc = new(GameGlossaries);
                    sc.ConsumePlanet(testPlanet, 0);
                    sc.FinalizeStats();
                } // Breakpoint here
            }
        }

        private void spotCheckWriteCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            this.SpotCheckWriteSetting = this.spotCheckWriteCheckBox.Checked;
        }

        private void decodeProgressLabel_Click(object sender, EventArgs e)
        {

        }

        private void writeDecodedCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            this.WriteDecodedSetting = this.writeDecodedCheckBox.Checked;
        }

        private void heatmapCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            this.HeatmapsEnabledSetting = this.heatmapCheckbox.Checked;
        }

        private void planetLooperBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.planetLooperBackgroundWorker.ReportProgress(0);
            this.LoopThroughPlanetSaves();

        }

        private void planetLooperBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            this.exportStatsButton.Enabled = true;
            this.exportReadyLabel.Text = "Ready";

            // Remove Lockout
            this.findProfileButton.Enabled = true;
            this.decodeButton.Enabled = true;
            this.writeDecodedCheckBox.Enabled = true;
            this.planetGridView.Columns["ReadOptionCol"].ReadOnly = false;

            this.readAllButton.Enabled = true;
            this.readNoneButton.Enabled = true;
        }

        private void planetLooperBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                // Lockout
                this.findProfileButton.Enabled = false;
                this.decodeButton.Enabled = false;
                this.writeDecodedCheckBox.Enabled = false;
                this.planetGridView.Columns["ReadOptionCol"].ReadOnly = true;

                this.readAllButton.Enabled = false;
                this.readNoneButton.Enabled = false;

                this.InitialPlanetListLockouts();
            }

            this.updateDecodeProgress();
        }

        private void readAllButton_Click(object sender, EventArgs e)
        {
            foreach ((int index, PlanetFileUtil.SaveSlotManager ssm) in this.planetsInProfile)
            {
                if (ssm.Complete.valid) { this.planetGridView.Rows[index].Cells["ReadOptionCol"].Value = true; }
            }
        }

        private void readNoneButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in this.planetGridView.Rows)
            {
                row.Cells["ReadOptionCol"].Value = false;
            }
        }

        private void debugMenuButton_Click(object sender, EventArgs e)
        {
            this.debugPanel.Visible = !this.debugPanel.Visible;
        }

        private void resetProfileButton_Click(object sender, EventArgs e)
        {
            string temp = this.ProfileDir;
            this.ProfileDir = null;
            this.SetAndCheckProfilePath(temp);
        }

        private void genericTestButton_Click(object sender, EventArgs e)
        {
            int steps = 10;
            TernaryTileHeatmap tpBlank = new(steps, []);
            TernaryTileHeatmap tp = new(steps, [.. tpBlank.TileCounts.Keys]);
            Func<double, double, (int, int, int), Color> simpleShader = TernaryTileHeatmap.MakeSimpleShader(Color.DarkMagenta);
            Func<double, double, (int, int, int), Color> cymShader = TernaryTileHeatmap.MakeCompositionShader(Color.Cyan, Color.Yellow, Color.Magenta);
            Func<double, double, (int, int, int), Color> rgbShader = TernaryTileHeatmap.MakeCompositionShader(Color.Red, Color.Lime, Color.Blue);
            Func<double, double, (int, int, int), Color> rybShader = TernaryTileHeatmap.MakeCompositionShader(Color.Red, Color.Yellow, Color.Blue);

            //Func<double, double, (int, int, int), Color> degenShader = TernaryTileHeatmap.MakeCompositionShader(Color.Blue, Color.Blue, Color.Blue);

            Image imSimple = tp.DrawStandardPlot(Color.White, simpleShader,
                title: "Shader Test\nSimple Shader DarkMagenta",
                labelA: "A",
                labelB: "B",
                labelC: "C",
                blurMult: 0
                );
            imSimple.SaveAsPng("testSimpleShader.png");

            Image imCym = tp.DrawStandardPlot(Color.White, cymShader,
                title: "Shader Test\nCYM Shader",
                labelA: "Cyan",
                labelB: "Yellow",
                labelC: "Magenta",
                blurMult: 0
                );
            imCym.SaveAsPng("testCymShader.png");

            Image imRgb = tp.DrawStandardPlot(Color.White, rgbShader,
                title: "Shader Test\nRGB Shader",
                labelA: "Red",
                labelB: "Green",
                labelC: "Blue",
                blurMult: 0
                );
            imRgb.SaveAsPng("testRgbShader.png");

            Image imRyb = tp.DrawStandardPlot(Color.White, rybShader,
                title: "Shader Test\nRYB Shader",
                labelA: "Red",
                labelB: "Yellow",
                labelC: "Blue",
                blurMult: 0
                );
            imRyb.SaveAsPng("testRybShader.png");

            //Image imDegen = tp.DrawStandardPlot(Color.White, degenShader, title: "a");
        }
    }
}
