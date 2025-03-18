
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
        public static readonly Glossaries GameGlossaries = new(Path.Combine(baseDir, "Glossaries"));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastSpotCheckDir { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool WriteDecodedSetting { get; private set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SpotCheckWriteSetting { get; private set; } = true;

        public StatCollector PlanetStatCollector;

        public FormMain()
        {
            InitializeComponent();
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

            if (Path.GetFileName(value).StartsWith("profile_"))
            {
                this.ProfileDir = value;
                this.profileFolderTextBox.Text = this.ProfileDir;
                this.profileFolderTextBox.BackColor = this.profileFolderTextBox.BackColor;
                this.profileFolderTextBox.ForeColor = Color.Green;
                this.profileDirOK = true;
                this.decodeReadyStatusLabel.Text = "Ready";
                this.InitializePlanetGridView();
            }
            else
            {
                this.ProfileDir = value;
                this.profileFolderTextBox.Text = this.ProfileDir;
                this.profileFolderTextBox.BackColor = this.profileFolderTextBox.BackColor;
                this.profileFolderTextBox.ForeColor = Color.Red;
                this.profileDirOK = false;
                this.decodeReadyStatusLabel.Text = "Not Ready";
            }
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

        public void InitializePlanetGridView()
        {

        }

        private void decodeButton_Click(object sender, EventArgs e)
        {
            if (this.profileDirOK)
            {
                this.PlanetStatCollector = new(GameGlossaries);

                List<string> allPlanetPaths = [.. Directory.GetDirectories(Path.Combine(this.ProfileDir, "sessions"))];
                //List<string> completedPlanetPaths = [.. allPlanetPaths.Where(path => Path.Exists(Path.Combine(path, "auto_complete.deux")))];

                List<string> completedPlanetPaths = [.. allPlanetPaths.Select(x => Path.Exists(Path.Combine(x, "auto_complete.deux")) ? Path.Combine(x, "auto_complete.deux") : null)];
                completedPlanetPaths = [.. completedPlanetPaths.Where(x => x is not null)];
                int completedPlanetCount = completedPlanetPaths.Count();

                List<string> incompletePlanetPaths = [.. allPlanetPaths.Select(x => Path.Exists(Path.Combine(x, "auto_complete.deux")) ? null : x)];
                incompletePlanetPaths = [.. incompletePlanetPaths.Where(x => x is not null)];

                int readPlanetCount = 0;
                this.decodeProgressBar.Maximum = completedPlanetCount;
                this.updateDecodeProgress(readPlanetCount, 0, completedPlanetCount);

                this.LoopThroughPlanetSaves(completedPlanetPaths);

                this.exportStatsButton.Enabled = true;
                this.exportReadyLabel.Text = "Ready";
            }
            else
            {
                this.decodeReadyStatusLabel.ForeColor = Color.Red;
            }
        }

        private void LoopThroughPlanetSaves(List<string> pathsToSaveFiles)
        {
            this.planetList.Clear();

            int i = -1;
            int ok = 0;
            foreach (string path in pathsToSaveFiles)
            {
                if (path is null)
                {
                    // TODO: Update the table for a skipped file
                    continue;
                }
                i++;

                List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
                bool readPlanetOK = false;
                (Planet newPlanet, Dictionary<string, object> resAsDict) = PlanetFileUtil.ReadPlanetFromFile(path);
                if (newPlanet is not null)
                {
                    this.planetList.Add(newPlanet);
                    readPlanetOK = true;
                    ok++;
                    newPlanet.SetGlossaryThenLookup(GameGlossaries);



                    // Write decoded file
                    if (this.WriteDecodedSetting)
                    {
                        pathParts.Reverse();
                        string dst = Path.Combine(decodedDir, pathParts[1] + "." + pathParts[0] + ".json");
                        string outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                        File.WriteAllText(dst, outputText);
                    }
                }
                else
                {
                    this.planetList.Add(newPlanet);
                }

                this.updateDecodeProgress(i + 1, ok, pathsToSaveFiles.Count);
                if (readPlanetOK)
                {

                }
                else
                {
                    Trace.TraceError("Failed to read planet file: " + pathParts[1] + "/" + pathParts[0]);
                }

            }
        }

        private void updateDecodeProgress(int tried, int ok, int total)
        {
            this.decodeProgressLabel.Text = String.Format("Planets ({0}/{1}), {2} OK", tried, total, ok);
            this.decodeProgressLabel.Refresh();

            if (tried < total) this.decodeProgressBar.Value = tried; else this.decodeProgressBar.Value = this.decodeProgressBar.Maximum;
        }

        private void exportStatsButton_Click(object sender, EventArgs e)
        {
            int i = -1;
            this.PlanetStatCollector = new(GameGlossaries);
            foreach (Planet planet in this.planetList)
            {
                i++;
                this.PlanetStatCollector.ConsumePlanet(planet, i);
            }
            this.PlanetStatCollector.FinalizeStats();

            string dst = Path.Combine(outputDir, DateTime.Now.ToString("yyyyMMdd HHmm") + ".xlsx");
            this.PlanetStatCollector.WriteToExcel(dst);
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
                (Planet testPlanet, Dictionary<string, object> resAsDict) = PlanetFileUtil.ReadPlanetFromFile(path);
                testPlanet.SetGlossaryThenLookup(GameGlossaries);

                this.LastSpotCheckDir = Path.GetDirectoryName(path);

                List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
                pathParts.Reverse();

                if (this.SpotCheckWriteSetting)
                {
                    string dst = Path.Combine(decodedDir, pathParts[1] + "." + pathParts[0] + ".json");
                    string outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                    File.WriteAllText(dst, outputText);
                }

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

                StatCollector sc = new(GameGlossaries);
                sc.ConsumePlanet(testPlanet, 0);
                sc.FinalizeStats();
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
    }
}
