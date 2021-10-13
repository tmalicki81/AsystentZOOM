using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    [Serializable]
    public abstract class PlayerVM : SingletonBaseVM
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetValue(ref _isEnabled, value, nameof(IsEnabled));
        }

        private PlayerStateEnum _afterEndBookmark = PlayerStateEnum.Paused;
        public PlayerStateEnum AfterEndBookmark
        {
            get => _afterEndBookmark;
            set
            {
                SetValue(ref _afterEndBookmark, value, nameof(AfterEndBookmark));
                CallChangeToParent(this, $"Zmieniono zachowanie po dotarciu do pozycji zdefiniowanej przez zakadki");
            }
        }

        private List<PlayerStateEnum> _afterEndBookmarkList;
        public List<PlayerStateEnum> AfterEndBookmarkList
            => _afterEndBookmarkList ??= new List<PlayerStateEnum>
            {
                 PlayerStateEnum.Played,
                 PlayerStateEnum.Paused,
                 PlayerStateEnum.Stopped
            };

        private static bool _showBookmarks;
        public bool ShowBookmarks
        {
            get => _showBookmarks;
            set => SetValue(ref _showBookmarks, value, nameof(ShowBookmarks));
        }

        private bool _IsMuted = false;
        public bool IsMuted
        {
            get => _IsMuted;
            set => SetValue(ref _IsMuted, value, nameof(IsMuted));
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                SetValue(ref _duration, value, nameof(Duration));
                RaisePropertyChanged(nameof(PercentComplette));
                RaisePropertyChanged(nameof(TimeToEnd));
                if (FileInfo != null)
                    FileInfo.Duration = value;
                else
                { 
                }
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
                RaisePropertyChanged(nameof(TimeToEnd));

                if (FileInfo != null)
                    FileInfo.Position = value;
                else
                { 
                }
                var bookmarks = FileInfo?.Bookmarks;
                if (bookmarks?.Any() == true)
                {
                    var playingBookmark = bookmarks.LastOrDefault(b => b.Position <= Position);
                    var prevBookmark = bookmarks.LastOrDefault(b => b.Position < playingBookmark?.Position);
                    var nextBookmark = bookmarks.FirstOrDefault(b => b.Position > playingBookmark?.Position);

                    bool isNewPlayingBookmark = (!playingBookmark?.IsPlaying) ?? false;
                    if (PlayerState == PlayerStateEnum.Played && isNewPlayingBookmark)
                    {
                        switch (AfterEndBookmark)
                        {
                            case PlayerStateEnum.Paused:
                                PauseCommand.Execute();
                                break;
                            case PlayerStateEnum.Stopped:
                                StopCommand.Execute();
                                break;
                        }
                    }
                    foreach (var bookmark in bookmarks)
                        bookmark.IsPlaying = bookmark == playingBookmark;
                    IsSelectionRangeEnabled = playingBookmark != null;
                    if (IsSelectionRangeEnabled)
                    {
                        SelectionStart = playingBookmark.Position.TotalSeconds;
                        SelectionEnd = nextBookmark?.Position.TotalSeconds ?? Duration.TotalSeconds;
                    }
                }
                TimeSpan finishBefore = FileInfo?.FinishBefore ?? TimeSpan.Zero;
                if (finishBefore > TimeSpan.Zero && Position > Duration - finishBefore)
                    StopCommand.Execute();
            }
        }
        private TimeSpan _position;

        private double _selectionStart;
        public double SelectionStart
        {
            get => _selectionStart;
            set => SetValue(ref _selectionStart, value, nameof(SelectionStart));
        }

        private double _selectionEnd;
        public double SelectionEnd
        {
            get => _selectionEnd;
            set => SetValue(ref _selectionEnd, value, nameof(SelectionEnd));
        }

        private bool _isSelectionRangeEnabled;
        public bool IsSelectionRangeEnabled
        {
            get => _isSelectionRangeEnabled;
            set => SetValue(ref _isSelectionRangeEnabled, value, nameof(IsSelectionRangeEnabled));
        }

        public TimeSpan TimeToEnd
        {
            get
            {
                TimeSpan timeToEnd = Duration - Position;
                bool isAlert =
                    PlayerState == PlayerStateEnum.Played &&
                    Duration > TimeSpan.Zero &&
                    Position > TimeSpan.Zero &&
                    (timeToEnd - FileInfo.FinishBefore).TotalSeconds < 15;
                EventAggregator.Publish($"{nameof(MainVM)}_IsAlertChanged", isAlert);
                return timeToEnd;
            }
        }

        public int PercentComplette
            => Duration.TotalMilliseconds > 0
                ? (int)(100 * Position.TotalSeconds / Duration.TotalSeconds)
                : 0;

        private double _bufferingProgress;
        public double BufferingProgress
        {
            get => _bufferingProgress;
            set => SetValue(ref _bufferingProgress, value, nameof(BufferingProgress));
        }

        private double _volume = 100;
        public double Volume
        {
            get => _volume;
            set => SetValue(ref _volume, value, nameof(Volume));
        }

        private string _source = "https://youtu.be/C2KYSxcxnR4";
        public string Source
        {
            get => _source;
            set => SetValue(ref _source, value, nameof(Source));
        }

        [Parent(typeof(IBaseMediaFileInfo), typeof(IMovable))]
        public IMovable FileInfo
        {
            get => _fileInfo;
            set
            {
                SetValue(ref _fileInfo, value, nameof(FileInfo));
                if (FileInfo != null)
                {
                    FileInfo.Duration = Duration;
                    FileInfo.Position = Position;
                }
            }
        }
        private IMovable _fileInfo;

        private PlayerStateEnum _playerState = PlayerStateEnum.Stopped;
        public PlayerStateEnum PlayerState
        {
            get => _playerState;
            set => SetValue(ref _playerState, value, nameof(PlayerState));
        }

        private RelayCommand _playCommand;
        public RelayCommand PlayCommand
            => _playCommand ??= new RelayCommand(() =>
                {
                    EventAggregator.Publish($"{GetType().Name}_Play");
                    PlayerState = PlayerStateEnum.Played;
                },
                () => PlayerState != PlayerStateEnum.Played);

        private RelayCommand _pauseCommand;
        public RelayCommand PauseCommand
            => _pauseCommand ??= new RelayCommand(() =>
                {
                    EventAggregator.Publish($"{GetType().Name}_Pause");
                    PlayerState = PlayerStateEnum.Paused;
                },
                () => PlayerState != PlayerStateEnum.Paused && PlayerState != PlayerStateEnum.Stopped);

        private RelayCommand _stopCommand;
        public RelayCommand StopCommand
            => _stopCommand ??= new RelayCommand(() =>
                {
                    EventAggregator.Publish($"{GetType().Name}_Stop");
                    EventAggregator.Publish($"{nameof(MainVM)}_IsAlertChanged", false);
                    PlayerState = PlayerStateEnum.Stopped;
                    Position = TimeSpan.Zero;
                },
                () => PlayerState != PlayerStateEnum.Stopped);

        private RelayCommand _restartCommand;
        public RelayCommand RestartCommand
            => _restartCommand ??= new RelayCommand(() =>
                {
                    EventAggregator.Publish($"{GetType().Name}_Restart");
                    PlayerState = PlayerStateEnum.Played;
                },
                () => true);

        private IBookmarkVM GetSelectedBookmark()
            => FileInfo?.SelectedBookmark;

        private void SetSelectedBookmark(IBookmarkVM bookmark)
            => FileInfo.SelectedBookmark = bookmark;

        public ObservableCollection<IBookmarkVM> GetBookmarks()
            => FileInfo?.Bookmarks;

        private void SetBookmarks(IEnumerable<IBookmarkVM> bookmarks)
            => FileInfo.Bookmarks = new ObservableCollection<IBookmarkVM>(bookmarks);

        private RelayCommand _setPositionFromBookmarkCommand;
        public RelayCommand SetPositionFromBookmarkCommand
            => _setPositionFromBookmarkCommand ??= new RelayCommand(
                SetPositionFromBookmark,
                () => GetSelectedBookmark() != null);
        private void SetPositionFromBookmark()
        {
            var selectedBookmark = GetSelectedBookmark();
            double position = selectedBookmark.Position.TotalSeconds;
            EventAggregator.Publish($"{GetType().Name}_PositionChanged", position);
        }

        private RelayCommand _deleteBookmarkCommand;
        public RelayCommand DeleteBookmarkCommand
            => _deleteBookmarkCommand ??= new RelayCommand(
                DeleteBookmark,
                () => GetSelectedBookmark() != null);
        private void DeleteBookmark()
        {
            var selectedBookmark = GetSelectedBookmark();
            var dr = DialogHelper.ShowMessageBox($"Czy usunąć zakładkę {selectedBookmark.Name}?", "Usuwanie zakładki", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (dr != MessageBoxResult.Yes)
                return;
            var bookmarks = GetBookmarks();
            bookmarks.Remove(selectedBookmark);
            if (!bookmarks.Any())
                IsSelectionRangeEnabled = false;
            SetBookmarks(bookmarks);
            CallChangeToParent(this, $"Usunieto zakładkę {selectedBookmark.Name}");
        }

        private TimeSpan TrimPosition()
            => TimeSpan.FromSeconds((int)Position.TotalSeconds);

        private RelayCommand _replaceBookmarkCommand;
        public RelayCommand ReplaceBookmarkCommand
            => _replaceBookmarkCommand ??= new RelayCommand(
                ReplaceBookmark,
                () => GetSelectedBookmark() != null);
        private void ReplaceBookmark()
        {
            var bookmarks = GetBookmarks();
            var selectedBookmark = GetSelectedBookmark();
            selectedBookmark.Position = TrimPosition();
            SetBookmarks(bookmarks.OrderBy(b => b.Position));
            SetSelectedBookmark(selectedBookmark);
            CallChangeToParent(this, $"Podmieniono pozycję zakładki {selectedBookmark.Name} na {selectedBookmark.Position}");
        }

        private RelayCommand _addBookmarkCommand;
        public RelayCommand AddBookmarkCommand
            => _addBookmarkCommand ??= new RelayCommand(AddBookmark);
        private void AddBookmark()
        {
            var newBookmark = new BookmarkVM { FileInfo = FileInfo };
            var bookmarks = GetBookmarks();
            newBookmark.Position = TrimPosition();
            newBookmark.Color = ColorsHelper.ColorsDictionary.FirstOrDefault(d => !bookmarks.Any(b => d.Key == b.Color)).Key;
            newBookmark.Name = $"{BookmarkVM.NewBookmarkName} {bookmarks.Where(b => b.Name.StartsWith(BookmarkVM.NewBookmarkName)).Count() + 1}";
            bookmarks.Add(newBookmark);

            SetBookmarks(bookmarks.OrderBy(b => b.Position));
            SetSelectedBookmark(newBookmark);
            CallChangeToParent(this, "Dodano zakładkę");
        }
    }
}