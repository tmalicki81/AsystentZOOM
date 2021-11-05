using AsystentZOOM.VM.Common;
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

            string[] eArgs = e.Args;

            //eArgs = new string[] { @"C:\Users\tmali\OneDrive\Dokumenty\Asystent ZOOM\2021-09-29.meeting" };
            //MessageBox.Show("AsystentZOOM.GUI: " + eArgs.FirstOrDefault());

            /*
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
            */

            if (eArgs.Any())
                MeetingVM.StartupFileName = eArgs.First();

            base.OnStartup(e);
        }

        private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                try
                {
                    string fileName = "c:\\Log.txt";
                    if (!File.Exists(fileName))
                        File.Create(fileName);
                    File.AppendAllText(fileName, ex.ToString() + Environment.NewLine);
                }
                catch { }

                bool dr = await DialogHelper.ShowMessagePanelAsync(
                    $"{ex.Message}{Environment.NewLine}Czy skopiować treść do schowka?",
                    "Nieobsłużony błąd", ImageEnum.Question, true,
                    new MsgBoxButtonVM<bool>[]
                    {
                        new(true,  "Tak, skopiuj do schowka", ImageEnum.Yes),
                        new(false, "Nie kopiuj",              ImageEnum.No),
                    });

                if (dr)
                    Clipboard.SetText(ex.ToString());
            }
        }
    }
}