using AsystentZOOM.VM.Model;
using System;
using System.Collections.ObjectModel;

namespace AsystentZOOM.VM.Interfaces
{
    public interface IMovable
    {
        TimeSpan Duration { get; set; }
        TimeSpan Position { get; set; }
        TimeSpan FinishBefore { get; set; }
        BookmarkVM SelectedBookmark { get; set; }
        ObservableCollection<BookmarkVM> Bookmarks { get; set; }
    }
}
