using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System.Windows.Controls;
namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for VideoPanelView.xaml
    /// </summary>
    public partial class ImagePanelView : UserControl, IPanelView<ImageVM>
    {
        public ImagePanelView()
        {
            InitializeComponent();
        }

        public int PanelNr => 5;
        public string PanelName => "Obraz";
        public ImageVM ViewModel => (ImageVM)DataContext;
        public void ChangeVisible(bool visible)
        {
        }
    }
}