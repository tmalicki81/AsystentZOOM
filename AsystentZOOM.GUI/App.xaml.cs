using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.ViewModel;
using BinaryHelper;
using BinaryHelper.FileRepositories;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace AsystentZOOM.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string _version;
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    try
                    {
                        var buildVersion = BuildVersion.Load(new LocalCodeFileRepository(), BuildVersionHelper.BuildVersionFile);
                        _version = buildVersion.AppVersion;
                    }
                    catch (Exception ex)
                    {
                        _version = $"nieznana {ex.GetType().Name}";
                    }
                }
                return _version;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //string[] e_Args = new string[] { @"C:\Users\tmalicki.KAMSOFT\Documents\Asystent ZOOM\2021-07_28__Zebranie w tygodniu 26 lipca do 1 sierpnia.meeting" };
            //MessageBox.Show("AsystentZOOM.GUI: " + e.Args.FirstOrDefault());

            var currentProcess = Process.GetCurrentProcess();
            Process[] processesToKill = Process.GetProcessesByName(currentProcess.ProcessName)
                .Where(p => p.Id != currentProcess.Id)
                .ToArray();
            foreach (Process processToKill in processesToKill)
            {
                var dr = MessageBox.Show($"Czy zamknąć {processToKill.MainWindowTitle} ?", "Zamykanie", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (dr == MessageBoxResult.Yes)
                {
                    try
                    {
                        processToKill.Kill();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Wystąpił błąd: {ex}", "Zamykanie", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (e.Args.Any())
                MeetingVM.StartupFileName = e.Args.First();

            base.OnStartup(e);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                try
                {
                    File.AppendAllText("c:\\Log.txt", ex.ToString() + Environment.NewLine);
                }
                catch { }
                var dr = DialogHelper.ShowMessageBox(
                    $"{ex.Message}{Environment.NewLine}Czy skopiować treść do schowka?", 
                    "Nieobsłużony błąd", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);
                if (dr == MessageBoxResult.Yes)
                    Clipboard.SetText(ex.ToString());
            }
        }
    }
}
