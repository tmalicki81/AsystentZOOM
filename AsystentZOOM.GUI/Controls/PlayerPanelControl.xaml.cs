using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static AsystentZOOM.GUI.Controls.SliderWithPopup;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for PlayerPanelControl.xaml
    /// </summary>
    public partial class PlayerPanelControl : UserControl
    {
        public static DependencyProperty MediaNameProperty = DependencyProperty.Register(
            nameof(MediaName), typeof(string), typeof(PlayerPanelControl));

        public string MediaName
        {
            get => (string)GetValue(MediaNameProperty);
            set => SetValue(MediaNameProperty, value);
        }

        public static DependencyProperty AdditionalButtonsProperty = DependencyProperty.Register(
            nameof(AdditionalButtons),
            typeof(Control),
            typeof(PlayerPanelControl),
            new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PlayerPanelControl control))
                throw new ArgumentException(nameof(d));
            if (e.Property == AdditionalButtonsProperty)
            {
                var content = (Control)e.NewValue;
                control.icAdditionalButtons.Content = content;
            }
        }

        public Control AdditionalButtons
        {
            get => (Control)GetValue(AdditionalButtonsProperty);
            set => SetValue(AdditionalButtonsProperty, value);
        }

        public Func<double, string> SecondsToTimeSpanFormat
            => (value) => $"{TimeSpan.FromSeconds(value):hh\\:mm\\:ss}";

        public static Func<double, string> PercentFromUnit
            => (value) => $"{(int)(value * 100)} %";

        public ControlConverterDelegate PositionToImage => (dataContext, position) =>
        {
            var viewModel = dataContext as VideoVM;
            if (viewModel == null)
                return new(null, null);

            var control = new MediaElement
            {
                LoadedBehavior = MediaState.Manual,
                Height = 0,
                Width = 100
            };
            control.MediaOpened += (s, e) =>
            {
                if (s is MediaElement sender && sender.NaturalVideoWidth != 0)
                {
                    double proportions = sender.NaturalVideoHeight / (double)sender.NaturalVideoWidth;
                    sender.Height = sender.Width * proportions;
                }
            };
            string source = viewModel.Source.Contains(":")
                ? viewModel.Source
                : Path.Combine(MediaLocalFileRepositoryFactory.Videos.RootDirectory, viewModel.Source);
            control.Source = new Uri(source);
            control.Volume = 0;
            control.IsMuted = true;
            control.Position = TimeSpan.FromSeconds(position);
            control.Play();
            Action destructor = () => control.Close();
            return (control, destructor);
        };

        public PlayerPanelControl()
        {
            InitializeComponent();
            DataContextChanged += AudioVideoPanelControl_DataContextChanged;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void AudioVideoPanelControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is PlayerVM dataContext && dataContext != null)
                dataContext.PropertyChanged += ViewModel_PropertyChanged;
        }

        public PlayerVM ViewModel
            => DataContext as PlayerVM;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectionStart))
            {
                Color rangeColor = ViewModel.GetBookmarks()?.FirstOrDefault(b => b.IsPlaying)?.Color ?? Colors.Blue;
                positionSlider.Resources[SystemColors.HighlightBrushKey] = new SolidColorBrush(rangeColor);
            }
        }

        private void SliderWithPopup_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is SliderWithPopup sliderWithPopup)
            {
                string dataContextName = ViewModel.GetType().Name;
                EventAggregator.Publish($"{dataContextName}_PositionChanged", sliderWithPopup.Value);
            }
        }

        private void TimePiecePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Popup popup)
                popup.IsOpen = false;
        }
    }
}
