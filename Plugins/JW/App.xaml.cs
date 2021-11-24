using AsystentZOOM.GUI;
using System;
using System.IO;
using System.Windows;

namespace JW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            new PanelWindow().ShowDialog();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception ex)
                return;

            var dr = MessageBox.Show(
                $"{ex.Message}{Environment.NewLine}Czy skopiować treść do schowka?",
                "Nieobsłużony błąd", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);
            
            if (dr == MessageBoxResult.Yes)
                Clipboard.SetText(ex.ToString());
        }
    }
}
