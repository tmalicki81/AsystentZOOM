using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System.Windows.Controls;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for VideoPanelView.xaml
    /// </summary>
    public partial class AudioPanelView : UserControl, IPanelView<AudioVM>
    {
        public AudioPanelView() => InitializeComponent();
        public string PanelName => "Audio";
        public int PanelNr => 5;
        public AudioVM ViewModel => (AudioVM)DataContext;
        public void ChangeVisible(bool visible) { }
    }
}