using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;

namespace AsystentZOOM.VM.Model
{
    public class ImageFileInfo : BaseMediaFileInfo<ImageVM>, IResizable
    {
        public override ILayerVM CreateContent(string fileName)
            => new ImageVM { Source = fileName };

        private double _naturalWidth;
        public double NaturalWidth
        {
            get => _naturalWidth;
            set => SetValue(ref _naturalWidth, value, nameof(NaturalWidth));
        }

        private double _naturalHeight;
        public double NaturalHeight
        {
            get => _naturalHeight;
            set => SetValue(ref _naturalHeight, value, nameof(NaturalHeight));
        }
    }
}