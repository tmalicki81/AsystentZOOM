using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for MainBorderWindow.xaml
    /// </summary>
    public partial class MainBorderWindow : FlexibleWindow, IViewModel<MainVM>
    {
        public MainBorderWindow()
        {
            InitializeComponent();
            EventAggregator.Subscribe<Size>($"{typeof(ILayerVM)}_ChangeOutputSize", ChangeOutputSize, (s) => !ViewModel.ProgressInfo.ProgressBarVisibility);
        }

        private class Offsets
        {
            internal const double Height = 40;
            internal const double Width = 20;
            internal const double Top = 30;
            internal const double Left = 10;
        }

        private void ChangeOutputSize(Size size)
        {
            double proportions = size.Height / size.Width;
            Height = Width * proportions + Offsets.Top;
        }

        public MainVM ViewModel => SingletonVMFactory.Main;

        protected override void OnClosing(CancelEventArgs e)
        {
            EventAggregator.UnSubscribe($"{typeof(ILayerVM)}_ChangeOutputSize");
            base.OnClosing(e);
        }
    }
}