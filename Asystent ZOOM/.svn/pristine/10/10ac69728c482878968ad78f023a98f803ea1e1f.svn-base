using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Interfaces.Sortable;
using AsystentZOOM.VM.Model;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for MediaFileInfoControl.xaml
    /// </summary>
    public partial class MediaFileInfoControl : UserControl, IViewModel<BaseMediaFileInfo>
    {
        public MediaFileInfoControl()
        {
            InitializeComponent();
            DataContextChanged += MediaFileInfoControl_DataContextChanged;
        }

        private void MediaFileInfoControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (e.OldValue as ISortableItemVM)?.Sorter;
            var newValue = (e.NewValue as ISortableItemVM)?.Sorter;

            if (oldValue != null)
                oldValue.PropertyChanged -= Sorter_PropertyChanged;
            if (newValue != null)
                newValue.PropertyChanged += Sorter_PropertyChanged;
        }

        private string _previousTitle;
        private void Sorter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISortableItemProvider.IsEditing))
            {
                var sorter = (sender as ISortableItemProvider);
                if (sorter.IsEditing)
                {
                    foreach (var d in sorter.ContainerItemsSource.Cast<ISortableItemVM>())
                    {
                        if (d.Sorter != sorter && d.Sorter.IsEditing)
                            d.Sorter.IsEditing = false;
                    }
                    var viewModel = sorter.SelectedItem as BaseMediaFileInfo;
                    _previousTitle = viewModel?.Title;
                }
            }
        }

        public BaseMediaFileInfo ViewModel 
            => DataContext as BaseMediaFileInfo;        

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.MeetingPoint.Meeting.StopAllCommand.Execute(false);
            ViewModel.PlayCommand.Execute();
        }

        #region Edycja tytułu pliku

        private void txtMiediaFileTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Escape)
                return;
            if (e.Key == Key.Escape)
                ViewModel.Title = _previousTitle;
            ViewModel.Sorter.IsEditing = false;
        }

        #endregion Edycja tytułu pliku

        private void SortableItemsContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.Sorter.IsEditing == true)
            {
                txtMediaFileTitle.Focus();
                txtMediaFileTitle.SelectAll();
            }
        }
    }
}
