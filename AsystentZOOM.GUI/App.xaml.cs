using AsystentZOOM.GUI.Controls;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.ViewModel;
using BinaryHelper;
using BinaryHelper.FileRepositories;
using System;
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
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            if (e.Args.Any())
                MeetingVM.StartupFileName = e.Args.First();

            base.OnStartup(e);
        }

        private async void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            int dr = await DialogHelper.ShowMessageBoxAsync(
                    $"{e.Exception.Message}{Environment.NewLine}Czy skopiować treść do schowka?",
                    "Nieobsłużony błąd", ImageEnum.Error, 3,
                    new MsgBoxButtonVM<int>[]
                    {
                        new(1, "Tak, skopiuj do schowka", ImageEnum.Yes),
                        new(2, "Nie kopiuj",              ImageEnum.No),
                        new(3, "Resetuj aplikację",       ImageEnum.Warning),
                    });

            if (dr == 1)
                Clipboard.SetText(e.Exception.ToString());
            bool reset = dr == 3;

            await SingletonVMFactory.Main.Shutdown(reset);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception ex)
                return;

            var w = new Window
            {
                Owner = Current.MainWindow,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
            };
            var dc = new MsgBoxVM<bool>(
                $"{ex.Message}{Environment.NewLine}Czy skopiować treść do schowka?",
                "Nieobsłużony błąd", ImageEnum.Error, true,
                new MsgBoxButtonVM<bool>[]
                {
                    new(true,  "Tak, skopiuj do schowka", ImageEnum.Yes),
                    new(false, "Nie kopiuj",              ImageEnum.No),
                });
            dc.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(dc.ResultObj))
                    w.Close();
            };
            w.Content = new MsgBoxControl
            {
                DataContext = dc,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            w.ShowDialog();
            if (dc.Result)
                Clipboard.SetText(ex.ToString());
        }

        /// <summary>
        /// Czy zamknąć aplikację w trybie Force
        /// </summary>
        public static bool ForceShutdown { get; set; }
    }
}