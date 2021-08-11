using System;
using System.Windows;

namespace AsystentZOOM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowVM)DataContext;
            vm.OnQuit += MainWindow_Quit;
            vm.RunUpdatedAppCommand.ExecuteAsync();
        }

        private void MainWindow_Quit(object sender, EventArgs e)
            => Dispatcher.Invoke(Close);
    }
}