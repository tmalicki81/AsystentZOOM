using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.GUI.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Model;
using System.Threading.Tasks;
using System;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for MeetingPanelView.xaml
    /// </summary>
    public partial class MeetingPanelView : UserControl, IPanelView<MeetingVM>
    {
        #region ILayerPanel

        public MeetingVM ViewModel => (MeetingVM)DataContext;
        public int PanelNr => 1;
        public string PanelName => "Spotkanie";
        public void ChangeVisible(bool visible) 
        {
            if (visible)
            {
                var g = VisualTreeHelperExt.GetChild<DataGrid>(this);
                g.SelectAll();
            }
        }

        #endregion ILayerPanel

        public MeetingPanelView()
        {
            InitializeComponent();
            Loaded += MeetingPanelView_Loaded;
        }

        private bool _isLoaded;

        private async void MeetingPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;

            try
            {
                if (!string.IsNullOrEmpty(MeetingVM.StartupFileName))
                {
                    await SingletonVMFactory.Meeting.OpenFromLocal(MeetingVM.StartupFileName);
                }
                else
                {
                    SingletonVMFactory.SetSingletonValues(MeetingVM.CreateMeeting);
                    using (var progress = new ShowProgressInfo("Pobieranie multimediów z chmury", false, "Inicjalizacja"))
                    {
                        await ViewModel.DownloadAndFillMetadata(progress);
                    }
                }
            }
            catch (Exception ex)
            {
                await ViewModel.HandleException(ex);
            }
            _isLoaded = true;
        }

        private void mainListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem draggedItem)
            {
                if (e.ClickCount == 2)
                    e.Handled = false;
                else
                    DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void mainListView_Drop(object sender, DragEventArgs e)
        {
            MeetingPointVM droppedData = (MeetingPointVM)e.Data.GetData(typeof(MeetingPointVM));
            MeetingPointVM targetData = ((ListBoxItem)(sender)).DataContext as MeetingPointVM;
            if (droppedData == targetData || droppedData == null || targetData == null)
                return;

            var list = ViewModel.MeetingPointList;
            int droppedIndex = list.IndexOf(droppedData);
            int targetIndex = list.IndexOf(targetData);
            list.Remove(droppedData);
            list.Insert(targetIndex, droppedData);
            ViewModel.MeetingPointList = list;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (sender as ListView).SelectedIndex = -1;
        }

        private string _previousMeetingTitle;
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsEditing = true;
            _previousMeetingTitle = ViewModel.MeetingTitle;
            e.Handled = true;
            txtMeetingTitle.SelectAll();
            txtMeetingTitle.Focus();
        }

        private void txtMeetingTitle_LostFocus(object sender, RoutedEventArgs e)
            => ViewModel.IsEditing = false;

        private void txtMeetingTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Escape)
                return;
            if (e.Key == Key.Escape)
                ViewModel.MeetingTitle = _previousMeetingTitle;
            ViewModel.IsEditing = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.ContextMenu.IsOpen = true;
            button.ContextMenu.DataContext = button.DataContext;
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if(sender is FrameworkElement control && control.DataContext is MeetingPointVM vm)
                vm.Sorter.IsMouseOver = true;            
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement control && control.DataContext is MeetingPointVM vm)
                vm.Sorter.IsMouseOver = false;
        }

        private void meetingHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            meetingImageHeader.Height = e.NewSize.Height;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
