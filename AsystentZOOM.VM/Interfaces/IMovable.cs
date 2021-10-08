using AsystentZOOM.VM.Model;
using System;
using System.Collections.ObjectModel;

namespace AsystentZOOM.VM.Interfaces
{
    public interface IMovable
    {
        ObservableCollection<IBookmarkVM> Bookmarks { get; set; }
        TimeSpan Duration { get; set; }
        TimeSpan FinishBefore { get; set; }
        int PercentComplette { get; }
        TimeSpan Position { get; set; }
        IBookmarkVM SelectedBookmark { get; set; }
    }
}
