using AsystentZOOM.VM.Interfaces.Sortable;
using AsystentZOOM.GUI.View;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AsystentZOOM.VM.Common.Dialog;
using System.Threading.Tasks;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.Common;

namespace AsystentZOOM.GUI.Common.Sortable
{
    /// <summary>
    /// Rozszerzenia do kontrolek, które mogą zmieniać swoja pozycję względem elementu nadrzędnego
    /// </summary>
    public static class SortableItemControlExtended
    {
        /// <summary>
        /// Kliknięcie w przycisk wyświetlający menu dla kontrolki podrzędnej
        /// </summary>
        /// <param name="control">Kontener (kontrolka nadrzędna)</param>
        /// <param name="sender">Kontrolka podrzedna</param>
        /// <param name="e">Parametr zdarzenia</param>
        public static void ButtonWithContextMenu_Click(this ISortableContainerView control, object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fElement)
            {
                fElement.ContextMenu.DataContext = fElement.DataContext;
                fElement.ContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Przekazanie kontekstu menu z kontrolki podrzędnej
        /// </summary>
        /// <param name="control"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DataGridRow_ContextMenuOpening(this ISortableContainerView control, object sender, ContextMenuEventArgs e)
        {
            if (sender is FrameworkElement fElement)
                fElement.ContextMenu.DataContext = fElement.DataContext;
        }

        /// <summary>
        /// Najechanie na element podrzedny
        /// </summary>
        /// <param name="control"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DataGridRow_MouseEnter(this ISortableContainerView control, object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement fElement && fElement.DataContext is ISortableItemVM item)
                item.Sorter.IsMouseOver = true;
        }

        /// <summary>
        /// Opuszczenie kursora w elemencie podrzednym
        /// </summary>
        /// <param name="control"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DataGridRow_MouseLeave(this ISortableContainerView control, object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement fElement && fElement.DataContext is ISortableItemVM item)
                item.Sorter.IsMouseOver = false;
        }

        /// <summary>
        /// Opakowanie elementu zmieniającego kolejność w operacji Drag&Drop
        /// </summary>
        /// <param name="draggedItem">Element podrzędny</param>
        /// <returns></returns>
        private static Popup GetDragDropPopup(FrameworkElement draggedItem)
        {
            MainPanelView meetingPanelView = draggedItem.GetParent<MainPanelView>();
            return meetingPanelView.DragDropPopup;
        }

        /// <summary>
        /// Poruszanie elementem zmieniającym kolejność
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void MouseMove(object sender, MouseEventArgs e)
        {
            
            if (e.LeftButton == MouseButtonState.Pressed &&
                sender is FrameworkElement draggedItem &&
                draggedItem.DataContext is ISortableItemVM sortableItem)
            {
                if (sortableItem.Sorter.IsEditing)
                    return;

                var dragDropPopup = GetDragDropPopup(draggedItem);

                try
                {
                    FrameworkElement contentControl = draggedItem.GetChild<UserControl>() ?? draggedItem;
                    var ppp = contentControl.GetType()
                        .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        .Where(p => p.FieldType == typeof(DependencyProperty))
                        .Select(p => p.GetValue(contentControl) as DependencyProperty)
                        .Select(p => contentControl.GetBindingExpression(p))
                        .Where(p => p != null)
                        .ToList();
                    contentControl = (FrameworkElement)Activator.CreateInstance(contentControl.GetType());
                    foreach (var p in ppp)
                        contentControl.SetBinding(p.TargetProperty, p.ParentBinding);

                    contentControl.IsEnabled = false;
                    contentControl.Opacity = .8;
                    dragDropPopup.Child = contentControl;
                    dragDropPopup.DataContext = draggedItem.DataContext;
                    dragDropPopup.IsOpen = true;
                    DragDrop.DoDragDrop(draggedItem, new DataObject(sortableItem.GetType(), sortableItem), DragDropEffects.All);
                    sortableItem.Sorter.SelectedItem = sortableItem;
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowMessageBar(ex.Message, MessageBarLevelEnum.Error);
                }
                finally
                {
                    if (dragDropPopup.IsOpen)
                        dragDropPopup.IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Upuszczenie elementu zmieniającego kolejność
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="additionalImplementation"></param>
        public static async Task Drop(
            object sender, DragEventArgs e, 
            Func<DragEventArgs, IProgressInfoVM, ISortableItemVM[]> additionalImplementation = null)
        {
            e.Handled = true;
            using (var progress = new ShowProgressInfo("Pobieranie pliku", true, null))
            {
                try
                {
                    var targetData = (sender as FrameworkElement)?.DataContext as ISortableItemVM;
                    ISortableItemVM[] droppedData;

                    droppedData = await Task.Run(() => additionalImplementation?.Invoke(e, progress));
                    if (droppedData == null)
                    {
                        string format = e.Data.GetFormats().First();
                        droppedData = new ISortableItemVM[] { e.Data.GetData(format) as ISortableItemVM };
                    }
                    droppedData?
                        .Where(dd => dd != null && targetData != null && dd != targetData)
                        .ToList()
                        .ForEach(x => x.Sorter.DropTo(targetData));
                }
                catch (Exception ex)
                {
                    progress.Dispose();
                    DialogHelper.ShowMessageBar(ex.Message, MessageBarLevelEnum.Error);
                }
            }
        }

        /// <summary>
        /// Najechanie wskaźnikiem na element zmieniający kolejność (podczas operacji Drag&Drop)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DragOver(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement draggedItem)
            {
                var dragDropPopup = GetDragDropPopup(draggedItem);
                dragDropPopup.HorizontalOffset = e.GetPosition(dragDropPopup.PlacementTarget).X;
                dragDropPopup.VerticalOffset = e.GetPosition(dragDropPopup.PlacementTarget).Y;
            }
        }
    }
}
