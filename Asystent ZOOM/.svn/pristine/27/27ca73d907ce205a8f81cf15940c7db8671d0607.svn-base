using AsystentZOOM.GUI.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Interfaces.Sortable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for SortableItemsContextMenu.xaml
    /// </summary>
    public partial class SortableItemsContextMenu : ContextMenu
    {
        public SortableItemsContextMenu()
        {
            InitializeComponent();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu && contextMenu.DataContext is ISortableItemVM item)
                foreach (var p in item.Sorter.GetAllSortableItems())
                    if (!p.HoldSelected)
                        p.IsSelected = p == item.Sorter;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu && contextMenu.DataContext is ISortableItemVM item)
                foreach (var p in item.Sorter.GetAllSortableItems())
                    if (!p.HoldSelected)
                        p.IsSelected = false;
        }
    }
}
