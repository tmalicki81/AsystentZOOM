using AsystentZOOM.GUI.Common.Sortable;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Interfaces.Sortable;
using AsystentZOOM.VM.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for ParametersControl.xaml
    /// </summary>
    public partial class ParametersControl : UserControl, IViewModel<ParameterVM>, ISortableContainerView
    {
        public ParameterVM ViewModel
            => (ParameterVM)DataContext;

        public ParametersControl()
            => InitializeComponent();

        private void ButtonWithContextMenu_Click(object sender, RoutedEventArgs e)
            => SortableItemControlExtended.ButtonWithContextMenu_Click(this, sender, e);

        private void DataGridRow_ContextMenuOpening(object sender, ContextMenuEventArgs e)
            => SortableItemControlExtended.DataGridRow_ContextMenuOpening(this, sender, e);

        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
            => SortableItemControlExtended.DataGridRow_MouseEnter(this, sender, e);

        private void DataGridRow_MouseLeave(object sender, MouseEventArgs e)
            => SortableItemControlExtended.DataGridRow_MouseLeave(this, sender, e);
    }
}