using AsystentZOOM.GUI.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for TimePieceOutputView.xaml
    /// </summary>
    public partial class TimePieceOutputView : UserControl, ILayerOutput<TimePieceVM>
    {
        public TimePieceOutputView()
        {
            InitializeComponent();
        }

        public TimePieceVM ViewModel => (TimePieceVM)DataContext;
    }
}
