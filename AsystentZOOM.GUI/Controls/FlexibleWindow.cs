using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using System;
using System.Windows;
using System.Windows.Forms;

namespace AsystentZOOM.GUI.View
{
    public class FlexibleWindow : Window
    {
        public FlexibleWindow()
        {
            Loaded += FlexibleWindow_Loaded;
        }

        private void FlexibleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetCurrentScreen();
            SizeChanged += FlexibleWindow_SizeChanged;
            LocationChanged += FlexibleWindow_LocationChanged;
        }

        private void FlexibleWindow_LocationChanged(object sender, EventArgs e)
        {
            SetCurrentScreen();
        }

        private void FlexibleWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CurrentWidth = e.NewSize.Width;
            CurrentHeight = e.NewSize.Height;
            SetCurrentScreen();
        }

        public double CurrentWidth
        {
            get => (double)GetValue(CurrentWidthProperty);
            set => SetValue(CurrentWidthProperty, value);
        }

        public static DependencyProperty CurrentWidthProperty = DependencyProperty.Register(
            nameof(CurrentWidth), typeof(double), typeof(FlexibleWindow),
            new PropertyMetadata(PropertyChangedCallback));

        public double CurrentHeight
        {
            get => (double)GetValue(CurrentHeightProperty);
            set => SetValue(CurrentHeightProperty, value);
        }

        public static DependencyProperty CurrentHeightProperty = DependencyProperty.Register(
            nameof(CurrentHeight), typeof(double), typeof(FlexibleWindow),
            new PropertyMetadata(PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (FlexibleWindow)d;
            if (e.Property.Name == nameof(CurrentWidth))
                window.Width = (double)e.NewValue;
            else if (e.Property.Name == nameof(CurrentHeight))
                window.Height = (double)e.NewValue;
            
            window.SetCurrentScreen();
        }

        public event EventHandler<EventArgs<Screen>> ScreenChanged;

        private Screen _currentScreen;

        private void SetCurrentScreen()
        {
            var currentScreen = Screen.FromPoint(new System.Drawing.Point(
                (int)(Left + Width / 2),
                (int)(Top + Height / 2)));

            if (_currentScreen?.DeviceName != currentScreen.DeviceName)
            {
                if(this is MainOutputWindow)
                {
                }
                ScreenChanged?.Invoke(this, new EventArgs<Screen>(currentScreen));
                _currentScreen = currentScreen;
            }
        }
    }
}