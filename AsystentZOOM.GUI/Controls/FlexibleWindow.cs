using System.Windows;

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
            SizeChanged += FlexibleWindow_SizeChanged;
        }

        private void FlexibleWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CurrentWidth = e.NewSize.Width;
            CurrentHeight = e.NewSize.Height;
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
            var window = (Window)d;
            if (e.Property.Name == nameof(CurrentWidth))
                window.Width = (double)e.NewValue;
            else if (e.Property.Name == nameof(CurrentHeight))
                window.Height = (double)e.NewValue;
        }
    }
}