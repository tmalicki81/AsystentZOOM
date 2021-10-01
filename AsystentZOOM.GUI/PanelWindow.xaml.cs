﻿using AsystentZOOM.GUI.View;
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

namespace AsystentZOOM.GUI
{
    /// <summary>
    /// Interaction logic for PanelWindow.xaml
    /// </summary>
    public partial class PanelWindow : Window, IViewModel<MainVM>
    {
        private MainOutputWindow _mainOutputWindow;
        private MainBorderWindow _mainBorderWindow;

        public MainVM ViewModel => (MainVM)DataContext;

        public PanelWindow()
        {
            InitializeComponent();

            MainVM.Dispatcher = Dispatcher;

            Height = ViewModel.PanelWindowHeight;
            Width = ViewModel.PanelWindowWidth;

            if (Environment.UserDomainName.ToUpper().Contains("KAMSOFT"))
                Title = "Entrypoint";//Process.GetCurrentProcess().MainModule.ModuleName;
            Title = Title + $"   ( wersja: {App.Version} / {MainVM.Version})";

            _mainBorderWindow = new MainBorderWindow();
            _mainOutputWindow = new MainOutputWindow();

            EventAggregator.Subscribe<bool>(nameof(MainVM) + "_Close", CloseMainOutputWindow, (p) => true);
            EventAggregator.Subscribe(nameof(MainVM) + "_Open", OpenMainOutputWindow, () => true);
            EventAggregator.Subscribe(nameof(MainVM) + "_Reset", ResetMainOutputWindow, () => true);
            EventAggregator.Subscribe(nameof(MainVM) + "_ActivatePanel", () => Activate(), () => true);
            EventAggregator.Subscribe<MessageBoxParameters>("MessageBox_Show", MessageBox_Show, (x) => true);
            EventAggregator.Subscribe<OpenFileDialogParameters>("OpenFile_Show", OpenFile_Show, (x) => true);
            EventAggregator.Subscribe<SaveFileDialogParameters>("SaveFile_Show", SaveFile_Show, (x) => true);

            EventAggregator.Subscribe<double>($"{nameof(MainVM)}_Change_{nameof(ViewModel.PanelWindowWidth)}", (w) => Width = w, (p) => true);
            EventAggregator.Subscribe<double>($"{nameof(MainVM)}_Change_{nameof(ViewModel.PanelWindowHeight)}", (h) => Height = h, (p) => true);
            EventAggregator.Subscribe<Size>($"{typeof(ILayerVM)}_ChangePanelSize", ChangePanelSize, (s) => true);

            Loaded += PanelWindow_Loaded;
        }

        private void PanelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainBorderWindow.Show();
            _mainOutputWindow.Show();
            _mainOutputWindow.Owner = _mainBorderWindow;
        }

        private void ChangePanelSize(Size size)
        {
            double proportions = size.Height / size.Width;
            Height = Width * proportions;
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
                InitialDirectory = p.InitialDirectory
            };
            p.Result = fileDialog.ShowDialog();
            if (p.Result != true) return;
            p.FileName = fileDialog.FileName;
            p.FileNames = fileDialog.FileNames;
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
        }

        private void OpenMainOutputWindow()
        {
            _mainOutputWindow = new MainOutputWindow();
            _mainOutputWindow.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Application.Current.ShutdownMode != ShutdownMode.OnExplicitShutdown)
            {
                MessageBoxResult dr = MessageBox.Show("Czy na pewno zamknąć aplikację?", "Asystent ZOOM",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                e.Cancel = dr == MessageBoxResult.No;
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mainOutputWindow.Close();
            SingletonVMFactory.SaveAllSingletons();
            SingletonVMFactory.DisposeAllSingletons();
            Application.Current?.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                ViewModel.PanelWindowHeight = e.NewSize.Height;
                ViewModel.PanelWindowWidth = e.NewSize.Width;
            }
        }
    }
}
