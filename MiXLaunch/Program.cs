using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;

namespace MiXLaunch
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            string AppFolder = ApplicationData.Current.LocalSettings.Values["AppFolder"] as string;
            string AppExecutable = ApplicationData.Current.LocalSettings.Values["AppExecutable"] as string;
            string AppArguments = ApplicationData.Current.LocalSettings.Values["AppArguments"] as string;

//            MessageBox.Show(AppExecutable+" "+AppArguments,"@"+AppFolder);

            Process cmd = new Process();
            cmd.StartInfo.FileName = AppExecutable;
            cmd.StartInfo.Arguments = AppArguments;
            cmd.StartInfo.WorkingDirectory = AppFolder;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            ApplicationData.Current.LocalSettings.Values["AppExecutable"] = "";
/*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
*/
        }
    }
}
