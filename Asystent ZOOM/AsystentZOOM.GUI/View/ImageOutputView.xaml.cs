using AsystentZOOM.GUI.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for VideoOutputView.xaml
    /// </summary>
    public partial class ImageOutputView : UserControl, ILayerOutput<ImageVM>
    {
        public ImageOutputView()
        {
            InitializeComponent();
        }

        public ImageVM ViewModel => (ImageVM)DataContext; 
    }
}
