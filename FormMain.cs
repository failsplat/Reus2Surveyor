
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public static readonly string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decoded");

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProfileDir { get; set; } = "";
        public bool profileDirOK = false;

        private List<Planet> planetList = [];
        public static readonly Glossaries GameGlossaries = new(Path.Combine(baseDir, "Defs"));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastSpotCheckDir { get; set; } = "";

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
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

        public void InitializePlanetGridView()
        {

        }

        private void decodeButton_Click(object sender, EventArgs e)
        {
            if (this.profileDirOK)
            {
                List<string> allPlanetPaths = [.. Directory.GetDirectories(Path.Combine(this.ProfileDir, "sessions"))];
                //List<string> completedPlanetPaths = [.. allPlanetPaths.Where(path => Path.Exists(Path.Combine(path, "auto_complete.deux")))];

                List<string> completedPlanetPaths = [.. allPlanetPaths.Select(x => Path.Exists(Path.Combine(x, "auto_complete.deux")) ? Path.Combine(x, "auto_complete.deux") : null)];
                int completedPlanetCount = completedPlanetPaths.Where(x => x is not null).Count();

                int readPlanetCount = 0;
                this.decodeProgressBar.Maximum = completedPlanetCount;
                this.updateDecodeProgress(readPlanetCount, completedPlanetCount);

                this.LoopThroughPlanetSaves(completedPlanetPaths);
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
            foreach (string path in pathsToSaveFiles)
            {
                i++;
                if (path is null)
                {
                    // TODO: Update the table for a skipped file
                    continue;
                }

                bool readPlanetOK = false;
                (Planet newPlanet, Dictionary<string, object> resAsDict) = PlanetFileUtil.ReadPlanetFromFile(path);
                if (newPlanet is not null)
                {
                    this.planetList.Add(newPlanet);
                }
                else
                {
                    readPlanetOK = true;
                    this.planetList.Add(newPlanet);
                }

                // Write decoded file
                List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
                pathParts.Reverse();

                string dst = Path.Combine(outputDir, pathParts[1] + "." + pathParts[0] + ".json");
                string outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                File.WriteAllText(dst, outputText);

                if (readPlanetOK)
                {
                    // TODO: Update the table for successful file
                }
                else
                {
                    // TODO: Update the table for a skipped file
                }

            }
        }

        private void updateDecodeProgress(int read, int total)
        {
            this.decodeProgressLabel.Text = String.Format("Planets ({0}/{1})", read, total);
            this.decodeProgressBar.Value = read;
        }

        private void exportStatsButton_Click(object sender, EventArgs e)
        {

        }

        private void spotCheckButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new()
            {
                InitialDirectory = this.LastSpotCheckDir,
                DefaultExt = ".deux"
            };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string path = openFile.FileName;
                (Planet testPlanet, Dictionary<string,object> resAsDict) = PlanetFileUtil.ReadPlanetFromFile(path);
                this.LastSpotCheckDir = Path.GetDirectoryName(path);

                List<string> pathParts = [.. path.Split(Path.DirectorySeparatorChar)];
                pathParts.Reverse();

                string dst = Path.Combine(outputDir, pathParts[1] + "." + pathParts[0] + ".json");
                string outputText = JsonConvert.SerializeObject(resAsDict, Formatting.Indented);
                File.WriteAllText(dst, outputText);
            }
        }
    }
}
