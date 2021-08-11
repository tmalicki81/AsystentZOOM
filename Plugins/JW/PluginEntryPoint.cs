using AsystentZOOM.Plugins.JW;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace JW
{
    public class PluginEntryPoint : IPluginEntryPoint
    {
        private MainVM _mainVM;

        public void Execute(MainVM mainVM, ItemCollection miAddins)
        {
            _mainVM = mainVM;
            var miWednesdayMeetingCreator = new MenuItem { Header = "Zebranie w tygodniu" };
            miWednesdayMeetingCreator.Click += miWednesdayMeetingCreator_Click;
            miAddins.Add(miWednesdayMeetingCreator);
        }

        private void miWednesdayMeetingCreator_Click(object sender, RoutedEventArgs e)
        {
            var window = new WednesdayMeetingCreatorWindow();
            window.ShowDialog();
        }
    }
}
