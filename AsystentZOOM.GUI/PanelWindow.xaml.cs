using AsystentZOOM.GUI.View;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace AsystentZOOM.GUI
{
    /// <summary>
    /// Interaction logic for PanelWindow.xaml
    /// </summary>
    public partial class PanelWindow : FlexibleWindow, IViewModel<MainVM>
    {
        private MainOutputWindow _mainOutputWindow;
        private MainBorderWindow _mainBorderWindow;

        public MainVM ViewModel => (MainVM)DataContext;

        public PanelWindow()
        {
            InitializeComponent();

            MainVM.Dispatcher = Dispatcher;

            Title = Title + $"   ( wersja: {App.Version} / {MainVM.Version})";

            _mainBorderWindow = new MainBorderWindow();
            _mainOutputWindow = new MainOutputWindow();

            //EventAggregator.Subscribe<bool>(nameof(MainVM) + "_Close", CloseMainOutputWindow, (p) => true);
            EventAggregator.Subscribe(nameof(MainVM) + "_Open", OpenMainOutputWindow, () => true);
            EventAggregator.Subscribe(nameof(MainVM) + "_Reset", ResetMainOutputWindow, () => true);
            EventAggregator.Subscribe(nameof(MainVM) + "_ActivatePanel", () => Activate(), () => true);
            EventAggregator.Subscribe<MessageBoxParameters>("MessageBox_Show", MessageBox_Show, (x) => true);
            EventAggregator.Subscribe<OpenFileDialogParameters>("OpenFile_Show", OpenFile_Show, (x) => true);
            EventAggregator.Subscribe<SaveFileDialogParameters>("SaveFile_Show", SaveFile_Show, (x) => true);

            Loaded += PanelWindow_Loaded;
        }

        private void PanelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainBorderWindow.Show();
            _mainOutputWindow.Show();
            _mainBorderWindow.Owner = this;
            _mainOutputWindow.Owner = _mainBorderWindow;

            ((UIElement)Content).Focus();
        }

        private void MessageBox_Show(MessageBoxParameters p)
            => p.Result = MessageBox.Show(p.MessageBoxText, p.Caption, p.Button, p.Icon, p.DefaultResult);

        private void OpenFile_Show(OpenFileDialogParameters p)
        {
            var fileDialog = new OpenFileDialog
            {
                Multiselect = p.Multiselect,
                Title = p.Title,
                Filter = p.Filter,
                AddExtension = p.AddExtension,
                DefaultExt = p.DefaultExt,
                InitialDirectory = p.InitialDirectory
            };
            p.Result = fileDialog.ShowDialog();
            if (p.Result != true) return;
            p.FileName = fileDialog.FileName;
            p.FileNames = fileDialog.FileNames;
        }

        private void SaveFile_Show(SaveFileDialogParameters p)
        {
            var fileDialog = new SaveFileDialog
            {
                Title = p.Title,
                Filter = p.Filter,
                AddExtension = p.AddExtension,
                DefaultExt = p.DefaultExt,
                InitialDirectory = p.InitialDirectory,
                FileName = p.FileName
            };
            p.Result = fileDialog.ShowDialog();
            if (p.Result != true)
            {
                p.FileName = null;
                p.FileNames = null;
            }
            else
            {
                p.FileName = fileDialog.FileName;
                p.FileNames = fileDialog.FileNames;
            }
        }

        private void CloseMainOutputWindow(bool warnBeforeStoppingSharing)
        {
            if (warnBeforeStoppingSharing)
            {
                MessageBox.Show(
                    "Przed zmianą trybu udostępniania zawartości nastąpi zatrzymanie udostępniania zawartości.",
                    "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            _mainOutputWindow.Close();
            _mainOutputWindow = null;
        }

        private void ResetMainOutputWindow() 
        {
            CloseMainOutputWindow(false);
            OpenMainOutputWindow();
            DialogHelper.ShowMessageBar("Zresetowano okno prezentacji");
        }

        private void OpenMainOutputWindow()
        {
            _mainOutputWindow = new MainOutputWindow();
            _mainOutputWindow.Show();
            _mainOutputWindow.Owner = _mainBorderWindow;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Application.Current.ShutdownMode != ShutdownMode.OnExplicitShutdown)
            {
                MessageBoxResult dr = MessageBox.Show("Czy na pewno zamknąć aplikację?", "Asystent ZOOM",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                e.Cancel = dr == MessageBoxResult.No;
            }

            if (!e.Cancel)
                SingletonVMFactory.DisposeAllSingletons();

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mainOutputWindow.Close();
            SingletonVMFactory.SaveAllSingletons();
            Application.Current?.Shutdown();
        }
    }
}
