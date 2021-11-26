using AsystentZOOM.GUI.Common;
using AsystentZOOM.GUI.Common.Mouse;
using AsystentZOOM.GUI.Converters;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
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
    public partial class MainOutputWindow : FlexibleWindow, IViewModel<MainVM>
    {
        public MainVM ViewModel => (MainVM)DataContext;

        public MainOutputWindow()
        {
            InitializeComponent();

            if (Environment.UserDomainName.ToUpper().Contains("KAMSOFT"))
                Title = "Entrypoint";//Process.GetCurrentProcess().MainModule.ModuleName;
            FillGrid();

            ScreenChanged += MainOutputWindow_ScreenChanged;
        }

        private void MainOutputWindow_ScreenChanged(object sender, EventArgs<System.Windows.Forms.Screen> e)
        {
            DialogHelper.ShowMessageBar($"Zmieniono urządzenie wyświetlające {e.Value.DeviceName}");
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
    }
}
