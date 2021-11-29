using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System.Windows.Controls;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for VideoOutputView.xaml
    /// </summary>
    public partial class VideoOutputView : PlayerOutputView, ILayerOutput<VideoVM>
    {
        public VideoOutputView()
            => InitializeComponent();

        public VideoVM ViewModel
            => (VideoVM)DataContext;

        protected override void MediaOpened(MediaElement mediaElement)
        {
            base.MediaOpened(mediaElement);

            ViewModel.NaturalVideoWidth = mediaElement.NaturalVideoWidth;
            ViewModel.NaturalVideoHeight = mediaElement.NaturalVideoHeight;
            ViewModel.ChangeOutputSizeCommand.Execute();
        }
    }
}