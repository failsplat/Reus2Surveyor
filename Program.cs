using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Reus2Surveyor.Properties;
using System.IO;

namespace Reus2Surveyor
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TextWriterTraceListener t = new("error.log");
            t.TraceOutputOptions |= TraceOptions.Timestamp;

            Trace.Listeners.Add(t);
            Trace.AutoFlush = true;
            FormMain formMain = new();
            formMain.SetAndCheckProfilePath(Settings.Default.StartProfileDir);
            if (!Path.Exists(Settings.Default.LastSpotCheckDir))
            {
                Settings.Default.LastSpotCheckDir = Settings.Default.StartProfileDir;
            }
            formMain.LastSpotCheckDir = Settings.Default.LastSpotCheckDir;
            formMain.SetSpotCheckWriteSetting(Settings.Default.SpotCheckWrite);
            formMain.SetCheckWriteDecodedSetting(Settings.Default.WriteDecoded);
            Application.Run(formMain);
            if (formMain.profileDirOK) Settings.Default.StartProfileDir = formMain.ProfileDir;
            if (!Path.Exists(Settings.Default.LastSpotCheckDir))
            {
                Settings.Default.LastSpotCheckDir = Settings.Default.StartProfileDir;
            }
            Settings.Default.LastSpotCheckDir = formMain.LastSpotCheckDir;
            Settings.Default.SpotCheckWrite = formMain.SpotCheckWriteSetting;
            Settings.Default.WriteDecoded = formMain.WriteDecodedSetting;
            Settings.Default.Save();
        }

    }

    
}
