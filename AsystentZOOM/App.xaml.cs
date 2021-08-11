using System.Linq;
using System.Windows;

namespace AsystentZOOM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Args = e.Args;
            //MessageBox.Show("AsystentZOOM: " + e.Args.FirstOrDefault());
            base.OnStartup(e);
        }
    }
}
