using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Model
{
    public abstract class PlayerFileInfo<TContent> : BaseMediaFileInfo<TContent>, IMovable
        where TContent : class, ILayerVM
    {
        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                SetValue(ref _duration, value, nameof(Duration));
                RaisePropertyChanged(nameof(PercentComplette));
            }
        }

        [XmlIgnore]
        public TimeSpan Position
        {
            get => _position;
            set
            {
                SetValue(ref _position, value, nameof(Position));
                RaisePropertyChanged(nameof(PercentComplette));
            }
        }
        private TimeSpan _position;

        public TimeSpan FinishBefore
        {
            get => _finishBefore;
            set => SetValue(ref _finishBefore, value, nameof(FinishBefore));
        }
        private TimeSpan _finishBefore;

        public int PercentComplette
            => Duration.TotalMilliseconds > 0
                ? (int)(100 * Position.TotalSeconds / Duration.TotalSeconds)
                : 0;

        [XmlIgnore]
        public BookmarkVM SelectedBookmark
        {
            get => _selectedBookmark;
            set
            {
                SetValue(ref _selectedBookmark, value, nameof(SelectedBookmark));
                foreach (var bookmark in Bookmarks)
                    bookmark.IsSelected = bookmark == SelectedBookmark;
            }
        }
        private BookmarkVM _selectedBookmark;

        private ObservableCollection<BookmarkVM> _bookmarks = new ObservableCollection<BookmarkVM>();
        public ObservableCollection<BookmarkVM> Bookmarks
        {
            get => _bookmarks;
            set => SetValue(ref _bookmarks, value, nameof(Bookmarks));
        }

        public override void OnDeserialized(object sender)
        {
            base.OnDeserialized(sender);
            if (Bookmarks?.Any() == true)
            {
                foreach (var bookmark in Bookmarks)
                    bookmark.FileInfo = this;
            }
        }

#if (DEBUG)
        public override string ToString()
            => !string.IsNullOrEmpty(Title) ? Title : FileName;
#endif
    }
}