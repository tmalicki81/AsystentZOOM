using AsystentZOOM.GUI.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for SliderWithPopup.xaml
    /// </summary>
    public partial class SliderWithPopup : Slider
    {
        public delegate (FrameworkElement Control, Action Destructor) ControlConverterDelegate(object dataContext, double position);

        #region LabelConverter

        public static DependencyProperty LabelConverterProperty = DependencyProperty.Register(
            nameof(LabelConverter), 
            typeof(Func<double, string>), 
            typeof(SliderWithPopup));

        public Func<double, string> LabelConverter
        {
            get => (Func<double, string>)GetValue(LabelConverterProperty);
            set => SetValue(LabelConverterProperty, value);
        }

        #endregion LabelConverter

        #region ControlConverter

        public static DependencyProperty ControlConverterProperty = DependencyProperty.Register(
            nameof(ControlConverter),
            typeof(ControlConverterDelegate),
            typeof(SliderWithPopup));

        public ControlConverterDelegate ControlConverter
        {
            get => (ControlConverterDelegate)GetValue(ControlConverterProperty);
            set => SetValue(ControlConverterProperty, value);
        }

        #endregion ControlConverter

        private Binding _valuePropertyBiding;
        private Popup _myLabelPopup;
        private Popup _myControlPopup;

        public SliderWithPopup()
        {
            InitializeComponent();
            Loaded += SliderWithPopup_Loaded;
            DataContextChanged += SliderWithPopup_DataContextChanged;
        }

        private void SliderWithPopup_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (LabelConverter == null)
                throw new Exception($"Nie ustawiono wartości {nameof(LabelConverter)}");
        }

        private void SliderWithPopup_Loaded(object sender, RoutedEventArgs e)
        {
            _valuePropertyBiding = GetBindingExpression(ValueProperty).ParentBinding;
            _myLabelPopup = (Popup)Resources["myLabelPopup"];
            _myLabelPopup.PlacementTarget = this;

            _myControlPopup = (Popup)Resources["myControlPopup"];
            _myControlPopup.PlacementTarget = this;
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Slider slider)
            {
                var value = slider.Value;
                BindingOperations.ClearBinding(slider, ValueProperty);
                slider.Value = value;
            }
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Slider slider)
            {
                var value = slider.Value;
                slider.SetBinding(ValueProperty, _valuePropertyBiding);
                slider.Value = value;
                if (LabelConverter != null)
                    _myLabelPopup.IsOpen = false;
                if (ControlConverter != null)
                {
                    _myControlPopup.IsOpen = false;
                    _previousControlPopupDestructor?.Invoke();
                }
            }
        }

        

        private Action _previousControlPopupDestructor;

        private void Slider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Slider slider)
            {
                double sliderWidth = slider.ActualWidth;

                if (LabelConverter != null)
                {
                    ((_myLabelPopup.Child as Border).Child as ContentControl).Content = LabelConverter(Value);
                    _myLabelPopup.IsOpen = true;
                    
                    double labelPopupWidth = (_myLabelPopup.Child as FrameworkElement).ActualWidth;
                    double labelHorizontalOffset = e.GetPosition(slider).X - labelPopupWidth / 2;

                    if (labelHorizontalOffset < 0)
                        labelHorizontalOffset = 0;
                    if (labelHorizontalOffset > sliderWidth - labelPopupWidth)
                        labelHorizontalOffset = sliderWidth - labelPopupWidth;
                    _myLabelPopup.HorizontalOffset = labelHorizontalOffset;
                }
                if (ControlConverter != null)
                {
                    var controlPopupManager = ControlConverter(slider.DataContext, Value);
                    lock (this)
                    {
                        _previousControlPopupDestructor?.Invoke();
                        _previousControlPopupDestructor = controlPopupManager.Destructor;
                    }
                    ((_myControlPopup.Child as Border).Child as ContentControl).Content = controlPopupManager.Control;
                    _myControlPopup.IsOpen = true;

                    double controlPopupWidth = (_myControlPopup.Child as FrameworkElement).ActualWidth;
                    double controlHorizontalOffset = e.GetPosition(slider).X - controlPopupWidth / 2;

                    if (controlHorizontalOffset < 0)
                        controlHorizontalOffset = 0;
                    if (controlHorizontalOffset > sliderWidth - controlPopupWidth)
                        controlHorizontalOffset = sliderWidth - controlPopupWidth;
                    _myControlPopup.HorizontalOffset = controlHorizontalOffset;
                }
            }
        }
    }
}
