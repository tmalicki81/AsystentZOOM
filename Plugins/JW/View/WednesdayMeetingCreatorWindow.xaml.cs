using AsystentZOOM.GUI.Common;
using AsystentZOOM.Plugins.JW.Common;
using AsystentZOOM.Plugins.JW.ViewModel;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using JW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AsystentZOOM.Plugins.JW
{
    /// <summary>
    /// Interaction logic for WednesdayMeetingCreatorWindow.xaml
    /// </summary>
    public partial class WednesdayMeetingCreatorWindow : Window, IViewModel<WednesdayMeetingCreatorVM>
    {
        public WednesdayMeetingCreatorVM ViewModel
            => (WednesdayMeetingCreatorVM)DataContext;

        public WednesdayMeetingCreatorWindow()
        {
            InitializeComponent();
            Loaded += WednesdayMeetingCreatorWindow_Loaded;
        }

        private void WednesdayMeetingCreatorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtHost.Focus();

            var datePicker = VisualTreeHelperExt.GetChild<DatePicker>(this);
            datePicker.DisplayDateStart = DateTime.Now.AddDays(-14);
            datePicker.DisplayDateEnd = DateTime.Now.AddDays(90);
            int year = DateTime.Now.Year;
            for (int month = datePicker.DisplayDateStart.Value.Month-1; 
                 month <= datePicker.DisplayDateEnd.Value.Month; 
                 month++)
            {
                for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                {
                    DateTime dateTime = new DateTime(year, month, day);
                    if (dateTime.DayOfWeek != ViewModel.MeetingDayOfWeek)
                        datePicker.BlackoutDates.Add(new CalendarDateRange(dateTime, dateTime));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
