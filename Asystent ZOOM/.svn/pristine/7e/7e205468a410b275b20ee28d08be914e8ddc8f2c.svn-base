using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System.Windows.Controls;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for VideoPanelView.xaml
    /// </summary>
    public partial class VideoPanelView : UserControl, IPanelView<VideoVM>
    {
        public VideoPanelView() => InitializeComponent();
        public string PanelName => "Video";
        public int PanelNr => 4;
        public VideoVM ViewModel => (VideoVM)DataContext;
        public void ChangeVisible(bool visible) { }
    }
}