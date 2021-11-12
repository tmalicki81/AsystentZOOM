using AsystentZOOM.GUI.Common.Sortable;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for MeetingPointControl.xaml
    /// </summary>
    public partial class MeetingPointControl : UserControl, IViewModel<MeetingPointVM>
    {
        public MeetingPointVM ViewModel => (MeetingPointVM)DataContext;

        public static Func<double, string> SecondsToTimeSpanFormat
            => (value) => $"{TimeSpan.FromSeconds(value):hh\\:mm\\:ss}";

        public static DependencyProperty IsPrimitiveProperty = DependencyProperty.Register(
            nameof(IsPrimitive), 
            typeof(bool), 
            typeof(MeetingPointControl), 
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MeetingPointControl meetingPoint)
            {
                var visibility = (bool)e.NewValue
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                meetingPoint.btnHand.Visibility = visibility;
                meetingPoint.btnMenu.Visibility = visibility;
                meetingPoint.lblDuration.Visibility = visibility;
                if ((bool)e.NewValue)
                {
                    meetingPoint.ContextMenu = null;
                }
            }
        }

        public bool IsPrimitive
        {
            get => (bool)GetValue(IsPrimitiveProperty);
            set => SetValue(IsPrimitiveProperty, value);
        }

        public MeetingPointControl()
            => InitializeComponent();

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            btnMenu.ContextMenu.IsOpen = true;
            btnMenu.ContextMenu.DataContext = ((FrameworkElement)sender).DataContext;
        }

        #region Edycja tytułu punktu

        private string _previousPointTitle;
        private void lblPointTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsPrimitive)
                return;

            ViewModel.Sorter.IsEditing = true;
            _previousPointTitle = ViewModel.PointTitle;

            e.Handled = true;
            txtPointTitle.SelectAll();
            txtPointTitle.Focus();
        }

        private void SortableItemsContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.Sorter.IsEditing == true)
            {
                _previousPointTitle = ViewModel.PointTitle;
                txtPointTitle.Focus();
                txtPointTitle.SelectAll();
            }
        }

        private void txtPointTitle_LostFocus(object sender, RoutedEventArgs e)
            => ViewModel.Sorter.IsEditing = false;

        private void txtPointTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Escape)
                return;
            if (e.Key == Key.Escape)
                ViewModel.PointTitle = _previousPointTitle;
            ViewModel.Sorter.IsEditing = false;
        }

        #endregion Edycja tytułu punktu

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsPrimitive)
                SortableItemControlExtended.MouseMove(sender, e);
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (IsPrimitive)
                return;

            e.Handled = true;

            await _semaphore.WaitAsync();
            try
            {
                await SortableItemControlExtended.Drop(sender, e, ViewModel.GetDataFromFileDrop);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!IsPrimitive)
                SortableItemControlExtended.DragOver(sender, e);
        }

        private void SliderWithPopup_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is SliderWithPopup slider)
            {
                var newValue = TimeSpan.FromSeconds(slider.Value);
                ViewModel.BeginTime = DateTime.Now - newValue;
            }
        }
    }
}