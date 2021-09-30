using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.ViewModel;
using System;
using System.ComponentModel;
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
            EventAggregator.Subscribe<double>($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowWidth)}", (w) => Width = w + Offsets.Width, (p) => true);
            EventAggregator.Subscribe<double>($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowHeight)}", (h) => Height = h + Offsets.Height, (p) => true);
            Loaded += MainBorderWindow_Loaded;
        }

        private class Offsets
        {
            internal const double Height = 40;
            internal const double Width = 20;
            internal const double Top = 30;
            internal const double Left = 10;
        }

        private MainVM ViewModel => SingletonVMFactory.Main;

        private void MainBorderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Height = ViewModel.OutputWindowHeight + Offsets.Height;
            Width = ViewModel.OutputWindowWidth + Offsets.Width;
            Top = ViewModel.OutputWindowTop - Offsets.Top;
            Left = ViewModel.OutputWindowLeft - Offsets.Left;

            SizeChanged += MainBorderWindow_SizeChanged;
            LocationChanged += MainBorderWindow_LocationChanged;
        }

        private void MainBorderWindow_LocationChanged(object sender, EventArgs e)
        {
            ViewModel.OutputWindowTop = Top + Offsets.Top;
            ViewModel.OutputWindowLeft = Left + Offsets.Left;
        }

        private void MainBorderWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.OutputWindowWidth = e.NewSize.Width - Offsets.Width;
            ViewModel.OutputWindowHeight = e.NewSize.Height - Offsets.Height;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            EventAggregator.UnSubscribe($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowWidth)}");
            EventAggregator.UnSubscribe($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowHeight)}");
            base.OnClosing(e);
        }
    }
}