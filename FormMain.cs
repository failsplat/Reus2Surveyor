
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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Reus2Surveyor
{
    public partial class FormMain : Form
    {
        static readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Decoded");

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(outputDir);
        }

        private void testDecodeButton_Click(object sender, EventArgs e)
        {
            string defFolder = Path.Combine(baseDir, "Defs");
            Glossaries g = new(defFolder);

            /*OpenFileDialog openTestFileDialog = new();
            if (openTestFileDialog.ShowDialog() == DialogResult.OK) 
            {
                string fp = openTestFileDialog.FileName;
                string res = PlanetFileUtil.DecompressEncodedFile(fp);
                string[] dirs = fp.Split(Path.DirectorySeparatorChar);
                
                var obj_a = JsonConvert.DeserializeObject(res);
                Dictionary<string,object> obj_b = (Dictionary<string,object>)PlanetFileUtil.ObjectToDictionary(obj_a);

                string res_c = JsonConvert.SerializeObject(obj_b, Formatting.Indented);
                string dst2 = Path.Combine(outputDir, dirs[dirs.Length - 2] + ' ' + Path.GetFileNameWithoutExtension(fp) + ".json");
                File.WriteAllText(dst2, res_c);

                Planet planet;
                List<object> refTokens;
                //File.WriteAllText(dst1, res);
                if (Path.GetExtension(fp) == ".deux")
                {
                    refTokens = (List<object>)obj_b["referenceTokens"];
                    planet = new(refTokens);
                }
            }*/
        }
    }
}
