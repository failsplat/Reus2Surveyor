using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Reus2Surveyor.Properties;

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
            formMain.LastSpotCheckDir = Settings.Default.LastSpotCheckDir;
            Application.Run(formMain);
            if (formMain.profileDirOK) Settings.Default.StartProfileDir = formMain.ProfileDir;
            Settings.Default.LastSpotCheckDir = formMain.LastSpotCheckDir;
            Settings.Default.Save();
        }

    }

    
}
