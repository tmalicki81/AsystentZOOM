using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Model
{
    public interface IBookmarkVM
    {
        IMovable FileInfo { get; set; }
        Color Color { get; set; }
        bool IsPlaying { get; set; }
        bool IsSelected { get; set; }
        string Name { get; set; }
        TimeSpan Position { get; set; }
    }

    [Serializable]
    public class BookmarkVM : BaseVM, IBookmarkVM
    {
        //public override void CallChangeToParent(IBaseVM child)
        //    => (FileInfo as BaseVM)?.CallChangeToParent(child);

        [XmlIgnore]
        public IMovable FileInfo
        {
            get => _fileInfo;
            set => SetValue(ref _fileInfo, value, nameof(FileInfo));
        }
        private IMovable _fileInfo;

        private TimeSpan _position = TimeSpan.Zero;
        public TimeSpan Position
        {
            get => _position;
            set
            {
                SetValue(ref _position, value, nameof(Position));
                CallChangeToParent(this);
            }
        }

        public const string NewBookmarkName = "Nowa zakładka";

        private string _name = NewBookmarkName;
        public string Name
        {
            get => _name;
            set
            {
                SetValue(ref _name, value, nameof(Name));
                CallChangeToParent(this);
            }
        }

        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                SetValue(ref _color, value, nameof(Color));
                CallChangeToParent(this);
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetValue(ref _isSelected, value, nameof(IsSelected));
        }
        private bool _isSelected;

        [XmlIgnore]
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetValue(ref _isPlaying, value, nameof(IsPlaying));
        }
        private bool _isPlaying;

#if (DEBUG)
        public override string ToString() => $"{Position} => {Name}";
#endif
    }
}
