using AsystentZOOM.Finisher.ViewModel;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsystentZOOM.Finisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewModel<FinisherVM>
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainVM.Dispatcher = Dispatcher;
            ViewModel.ExecuteAllTask();
        }

        public FinisherVM ViewModel => (FinisherVM)DataContext;

        private void DataGridRow_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is not DataGridRow dataGridRow)
                return;
            dgMain.SelectedItem = dataGridRow.Item;
            dgMain.Focus();
            dataGridRow.Focus();
        }
    }
}
