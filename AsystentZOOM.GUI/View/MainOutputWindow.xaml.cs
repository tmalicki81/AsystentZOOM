using AsystentZOOM.GUI.Common;
using AsystentZOOM.GUI.Common.Mouse;
using AsystentZOOM.GUI.Converters;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var currentPoint = PointFromScreen(MouseHelper.GetMousePosition());
                double deltaX = _grabPoint.X - currentPoint.X;
                double deltaY = _grabPoint.Y - currentPoint.Y;

                switch (_resizeButtonEnum)
                {
                    case ResizeButtonEnum.Left:
                        Left = Left - deltaX;
                        Width = Width + deltaX;
                        break;
                    case ResizeButtonEnum.Right:
                        Width = Width - deltaX;
                        break;
                }
            });
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
                Grid.SetColumnSpan(contentControl, 3);
                Grid.SetRowSpan(contentControl, 3);
                grid.Children.Add(contentControl);
            }
        }

        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonDown(e);
        //    DragMove();
        //}

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

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private Point _grabPoint;
        private System.Timers.Timer _timer;
        private ResizeButtonEnum _resizeButtonEnum = ResizeButtonEnum.None;

        private void ResizeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _resizeButtonEnum = (ResizeButtonEnum)((FrameworkElement)sender).Tag;
            _grabPoint = PointFromScreen(MouseHelper.GetMousePosition());
            _timer.Enabled = true;
            _timer.Start();
        }        

        private void ResizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Enabled = false;
            _timer.Stop();
            _resizeButtonEnum = ResizeButtonEnum.None;
        }
    }
}
