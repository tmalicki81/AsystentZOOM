using AsystentZOOM.GUI.Converters;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for MainOutputWindow.xaml
    /// </summary>
    public partial class MainOutputWindow : Window, IViewModel<MainVM>
    {
        public MainVM ViewModel => (MainVM)DataContext;

        public MainOutputWindow()
        {
            InitializeComponent();
            Height = ViewModel.OutputWindowHeight;
            Width = ViewModel.OutputWindowWidth;

            if (Environment.UserDomainName.ToUpper().Contains("KAMSOFT"))
                Title = "Entrypoint";//Process.GetCurrentProcess().MainModule.ModuleName;
            FillGrid();

            EventAggregator.Subscribe<double>($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowWidth)}", (w) => Width = w, (p) => true);
            EventAggregator.Subscribe<double>($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowHeight)}", (h) => Height = h, (p) => true);
            EventAggregator.Subscribe<Size>($"{typeof(ILayerVM)}_ChangeOutputSize", ChangeOutputSize, (s) => !ViewModel.ProgressInfo.ProgressBarVisibility);
        }

        private void ChangeOutputSize(Size size)
        {
            double proportions = size.Height / size.Width;
            Height = Width * proportions;
        }

        private void FillGrid() 
        {
            Type[] types = GetType()
                .Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && t.GetInterfaces().Any(i => i == typeof(ILayerOutputView)))
                .ToArray();
            ILayerVM vM;
            foreach (Type t in types)
            {
                var layerControl = (FrameworkElement)Activator.CreateInstance(t);
                var contentControl = new ContentControl { Content = layerControl };
                contentControl.SetBinding(
                    VisibilityProperty, 
                    new Binding
                    { 
                        Path = new PropertyPath(nameof(vM.IsEnabled)),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Source =  layerControl.DataContext,
                        Converter = new BoolToVisibilityConverter() 
                    });
                grid.Children.Add(contentControl);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                ViewModel.OutputWindowHeight = e.NewSize.Height;
                ViewModel.OutputWindowWidth = e.NewSize.Width;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            EventAggregator.UnSubscribe($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowWidth)}");
            EventAggregator.UnSubscribe($"{nameof(MainVM)}_Change_{nameof(ViewModel.OutputWindowHeight)}");
            EventAggregator.UnSubscribe($"{typeof(ILayerVM)}_ChangeOutputSize");
            base.OnClosing(e);
        }
    }
}
