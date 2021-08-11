using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System.Windows.Controls;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for BackgroundOutputView.xaml
    /// </summary>
    public partial class BackgroundOutputView : UserControl, ILayerOutput<BackgroundVM>
    {
        public BackgroundOutputView()
        {
            InitializeComponent();
        }

        public BackgroundVM ViewModel 
            => (BackgroundVM)DataContext;
    }
}
