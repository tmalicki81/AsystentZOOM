using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for TimePiecePicker.xaml
    /// </summary>
    public partial class TimePiecePicker : UserControl
    {
        public static DependencyProperty OffsetProperty = DependencyProperty.Register(
            nameof(Offset),
            typeof(TimeSpan),
            typeof(TimePiecePicker),
            new PropertyMetadata(TimeSpan.Zero, PropertyChangedCallback));

        public TimeSpan Offset
        {
            get => (TimeSpan)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public static DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(TimeSpan),
            typeof(TimePiecePicker),
            new PropertyMetadata(TimeSpan.Zero, PropertyChangedCallback));

        public TimeSpan MinValue
        {
            get => (TimeSpan)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(TimeSpan),
            typeof(TimePiecePicker),
            new PropertyMetadata(TimeSpan.FromHours(24), PropertyChangedCallback));

        public TimeSpan MaxValue
        {
            get => (TimeSpan)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public int Hours
        {
            get => (int)cmbHours.SelectedItem;
            set => cmbHours.SelectedItem = value;
        }

        public static DependencyProperty HoursVisibilityProperty = DependencyProperty.Register(
            nameof(HoursVisibility),
            typeof(bool),
            typeof(TimePiecePicker),
            new PropertyMetadata(true, PropertyChangedCallback));

        public bool HoursVisibility
        {
            get => (bool)GetValue(HoursVisibilityProperty);
            set => SetValue(HoursVisibilityProperty, value);
        }

        public int Minutes
        {
            get => (int)cmbMinutes.SelectedItem;
            set => cmbMinutes.SelectedItem = value;
        }

        public static DependencyProperty MinutesVisibilityProperty = DependencyProperty.Register(
            nameof(MinutesVisibility),
            typeof(bool),
            typeof(TimePiecePicker),
            new PropertyMetadata(true, PropertyChangedCallback));

        public bool MinutesVisibility
        {
            get => (bool)GetValue(MinutesVisibilityProperty);
            set => SetValue(MinutesVisibilityProperty, value);
        }

        public int Seconds
        {
            get => (int)cmbSeconds.SelectedItem;
            set => cmbSeconds.SelectedItem = value;
        }

        public static DependencyProperty SecondsVisibilityProperty = DependencyProperty.Register(
            nameof(SecondsVisibility),
            typeof(bool),
            typeof(TimePiecePicker),
            new PropertyMetadata(true, PropertyChangedCallback));

        public bool SecondsVisibility
        {
            get => (bool)GetValue(SecondsVisibilityProperty);
            set => SetValue(SecondsVisibilityProperty, value);
        }

        private static bool CanChange(TimePiecePicker timePiecePicker)
            => timePiecePicker.MinValue < timePiecePicker.MaxValue;

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timePiecePicker = (TimePiecePicker)d;

            if (e.Property.Name == nameof(Offset))
            {
                var oldValue = (TimeSpan)e.OldValue;
                var newValue = (TimeSpan)e.NewValue;
                if (oldValue == newValue) return;

                if (CanChange(timePiecePicker))
                {
                    if (newValue < timePiecePicker.MinValue)
                    {
                        newValue = timePiecePicker.MinValue;
                        timePiecePicker.Offset = newValue;
                        return;
                    }
                    if (newValue > timePiecePicker.MaxValue)
                    {
                        newValue = timePiecePicker.MaxValue;
                        timePiecePicker.Offset = newValue;
                        return;
                    }
                }
                timePiecePicker._lockUpdateValueFromHoursMinutesSeconds = true;
                
                timePiecePicker.Hours = newValue.Hours;
                timePiecePicker.Minutes = newValue.Minutes;
                timePiecePicker.Seconds = newValue.Seconds;

                timePiecePicker._lockUpdateValueFromHoursMinutesSeconds = false;                
            }
            if (CanChange(timePiecePicker))
            {
                if (e.Property.Name == nameof(MinValue))
                {
                    var oldValue = (TimeSpan)e.OldValue;
                    var newValue = (TimeSpan)e.NewValue;
                    if (oldValue == newValue) return;

                    if (timePiecePicker.Offset < newValue)
                        timePiecePicker.Offset = newValue;
                }
                else if (e.Property.Name == nameof(MaxValue))
                {
                    var oldValue = (TimeSpan)e.OldValue;
                    var newValue = (TimeSpan)e.NewValue;
                    if (oldValue == newValue) return;

                    if (timePiecePicker.Offset > newValue)
                        timePiecePicker.Offset = newValue;
                }
            }
        }

        private bool _lockUpdateValueFromHoursMinutesSeconds;

        private List<int> GetValues(int begin, int end, int step = 1)
        {
            var list = new List<int>();
            for (int i = begin; i <= end; i++)
                if (i % step == 0)
                    list.Add(i);
            return list;
        }

        public TimePiecePicker()
        {
            InitializeComponent();

            cmbHours.ItemsSource = GetValues(0, 23);
            cmbMinutes.ItemsSource = GetValues(0, 59);
            cmbSeconds.ItemsSource = GetValues(0, 59);
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_lockUpdateValueFromHoursMinutesSeconds)
                Offset = new TimeSpan(Hours, Minutes, Seconds);
        }
    }
}
