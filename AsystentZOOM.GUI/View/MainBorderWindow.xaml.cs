using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Windows;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for MainBorderWindow.xaml
    /// </summary>
    public partial class MainBorderWindow : Window
    {
        public MainBorderWindow()
        {
            InitializeComponent();
            Loaded += MainBorderWindow_Loaded;
        }

        private class Offsets
        {
            internal const double Height = 40;
            internal const double Width = 20;
            internal const double Top = 30;
            internal const double Left = 10;
        }

        private MainVM _main => SingletonVMFactory.Main;

        private void MainBorderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Height = _main.OutputWindowHeight + Offsets.Height;
            Width = _main.OutputWindowWidth + Offsets.Width;
            Top = _main.OutputWindowTop - Offsets.Top;
            Left = _main.OutputWindowLeft - Offsets.Left;

            SizeChanged += MainBorderWindow_SizeChanged;
            LocationChanged += MainBorderWindow_LocationChanged;
        }

        private void MainBorderWindow_LocationChanged(object sender, EventArgs e)
        {
            _main.OutputWindowTop = Top + Offsets.Top;
            _main.OutputWindowLeft = Left + Offsets.Left;
        }

        private void MainBorderWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _main.OutputWindowWidth = e.NewSize.Width - Offsets.Width;
            _main.OutputWindowHeight = e.NewSize.Height - Offsets.Height;
        }
    }
}