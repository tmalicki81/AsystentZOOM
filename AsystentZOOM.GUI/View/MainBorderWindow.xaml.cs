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
        }

        public MainVM ViewModel => SingletonVMFactory.Main;
    }
}