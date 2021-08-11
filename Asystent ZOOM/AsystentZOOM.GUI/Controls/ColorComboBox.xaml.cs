using AsystentZOOM.VM.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KVP = System.Collections.Generic.KeyValuePair<System.Windows.Media.Color, string>;

namespace AsystentZOOM.GUI.Controls
{
    /// <summary>
    /// Interaction logic for ColorComboBox.xaml
    /// </summary>
    public partial class ColorComboBox : ComboBox
    {
        public static DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), 
            typeof(Color), 
            typeof(ColorComboBox), 
            new PropertyMetadata(Colors.Black));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static DependencyProperty ShowColorNamesProperty = DependencyProperty.Register(
            nameof(ShowColorNames),
            typeof(bool),
            typeof(ColorComboBox),
            new PropertyMetadata(true, PropertyChangedCallback));

        public bool ShowColorNames
        {
            get => (bool)GetValue(ShowColorNamesProperty);
            set => SetValue(ShowColorNamesProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ColorComboBox)d;
            if (e.Property.Name == nameof(ShowColorNames))
            {
                bool value = (bool)e.NewValue;
                control.Width = value ? 200 : 40;
            }
        }

        public static DependencyProperty FavoritesProperty = DependencyProperty.Register(
            nameof(Favorites),
            typeof(Dictionary<Color, string>),
            typeof(ColorComboBox));

        public Dictionary<Color, string> Favorites
        {
            get => (Dictionary<Color, string>)GetValue(FavoritesProperty);
            set => SetValue(FavoritesProperty, value);
        }

        public ColorComboBox()
        {
            InitializeComponent();

            var colorsDictionary = ColorsHelper.ColorsDictionary.ToList();
            colorsDictionary.Insert(0, new KVP(Colors.Transparent, "Z ULUBIONYCH"));
            ItemsSource = colorsDictionary.ToDictionary(k => k.Key, v => v.Value);

            Favorites = ((Dictionary<Color, string>)ItemsSource)
                .Where(x => x.Key != Colors.Transparent)
                .ToList()
                .Take(10)
                .ToDictionary(k => k.Key, v => v.Value);
        }

        private RelayCommand<KVP> _setColorFromFavoritesCommand;
        public RelayCommand<KVP> SetColorFromFavoritesCommand
            => _setColorFromFavoritesCommand ??= new RelayCommand<KVP>(SetColorFromFavorites, (x) => true);
        private void SetColorFromFavorites(KVP obj)
            => SelectedColor = obj.Key;
    }
}
