using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for MainPanelView.xaml
    /// </summary>
    public partial class MainPanelView : UserControl, IViewModel<MainVM>
    {
        public MainVM ViewModel => (MainVM)DataContext;

        private void FillTabs()
        {
            var contents = GetType().Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && t.GetInterfaces().Any(i => i == typeof(IPanelView)))
                .Select(x => (FrameworkElement)Activator.CreateInstance(x))
                .Select(x => new
                {
                    FrameworkElement = x,
                    LayerPanelView = (IPanelView)x
                })
                .OrderBy(x => (x.LayerPanelView.PanelNr))
                .ToArray();
            
            foreach (var content in contents)
            {
                content.FrameworkElement.Margin = new Thickness(10);
                var headerLabel = new Label
                {
                    Content = content.LayerPanelView.PanelName,
                    DataContext = content.FrameworkElement.DataContext
                };

                var newTabItem = new TabItem
                {
                    DataContext = content.FrameworkElement.DataContext,
                    Header = headerLabel,
                    Content = content.FrameworkElement,
                    Padding = new Thickness(3)
                };
                tabControl.Items.Add(newTabItem);

                if (content.FrameworkElement.DataContext is ILayerVM layerVM)
                {
                    layerVM.IsEnabled = false;

                    #region Styl nagłówka zakładki

                    var headerDataTrigger = new DataTrigger
                    {
                        Binding = new Binding
                        {
                            Path = new PropertyPath(nameof(layerVM.IsEnabled)),
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Source = headerLabel.DataContext
                        },
                        Value = true
                    };

                    var headerAnimation = new ColorAnimation(Colors.Orange, Colors.Red, new Duration(TimeSpan.FromSeconds(1)));
                    var sb = new SolidColorBrush(Colors.Yellow);
                    sb.BeginAnimation(SolidColorBrush.ColorProperty, headerAnimation, HandoffBehavior.SnapshotAndReplace);

                    headerDataTrigger.Setters.Add(new Setter(BackgroundProperty, sb));

                    Style headerStyle = new Style(typeof(Label));
                    headerStyle.Triggers.Add(headerDataTrigger);
                    headerLabel.Style = headerStyle;

                    #endregion Styl nagłówka zakładki

                    #region Styl zawartości zakładki

                    var tabItemDataTrigger = new DataTrigger
                    {
                        Binding = new Binding
                        {
                            Path = new PropertyPath(nameof(layerVM.IsEnabled)),
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Source = newTabItem.DataContext
                        },
                        Value = true
                    };
                    tabItemDataTrigger.Setters.Add(new Setter(VisibilityProperty, Visibility.Visible));
                    Style tabItemStyle = new Style(typeof(TabItem));
                    tabItemStyle.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
                    tabItemStyle.Triggers.Add(tabItemDataTrigger);
                    newTabItem.Style = tabItemStyle;

                    #endregion Styl zawartości zakładki
                }


            }
            tabControl.SelectedIndex = 1;
        }

        public MainPanelView()
        {
            InitializeComponent();
            FillTabs();

            cmbWindowMode.ItemsSource = new Dictionary<WindowModeEnum, string>
            {
                { WindowModeEnum.NoBorder,   "Ruchome okno bez krawędzi i tytułu"  },
                { WindowModeEnum.Normal,     "Zwykłe okno z krawędziami i tytułem" },
                { WindowModeEnum.FullScreen, "Pełny ekran"                         }
            };
            ViewModel.WindowMode = WindowModeEnum.NoBorder;
            //ViewModel.WindowMode = WindowModeEnum.Normal;
            Loaded += MainPanelView_Loaded;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 1 && (e.RemovedItems[0] as TabItem)?.Content is IPanelView removedPanelView)
                removedPanelView.ChangeVisible(false); 
            if (e.AddedItems.Count == 1 && (e.AddedItems[0] as TabItem)?.Content is IPanelView addedPanelView)
                addedPanelView.ChangeVisible(true);            
        }

        private void MainPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            EventAggregator.Subscribe<bool>($"{nameof(MainVM)}_IsAlertChanged", IsAlertChanged, (a) => true);
            tabControl.SelectionChanged += TabControl_SelectionChanged;

            string[] pluginsAssemblies = Directory.GetFiles(Directory.GetCurrentDirectory(), "AsystentZOOM.Plugins.*.dll", SearchOption.TopDirectoryOnly);

            if (!pluginsAssemblies.Any())
            {
                (miAddins.Parent as Menu).Items.Remove(miAddins);
            }
            foreach (string pluginAssembly in pluginsAssemblies)
            {
                Assembly asm = Assembly.LoadFrom(pluginAssembly);
                var pluginEntryPointType = asm.GetTypes().FirstOrDefault(t => t.GetInterfaces().Any(i => i == typeof(IPluginEntryPoint)));
                if (pluginEntryPointType != null)
                {
                    IPluginEntryPoint pluginEntryPoint = (IPluginEntryPoint)Activator.CreateInstance(pluginEntryPointType);
                    pluginEntryPoint.Execute(SingletonVMFactory.Main, miAddins.Items);
                }
            }                
        }

        private bool _alertSequenceFlag;

        private void IsAlertChanged(bool isAlert)
        {
            _alertSequenceFlag = !_alertSequenceFlag;
            var color = isAlert ? (_alertSequenceFlag ? Colors.Red : Colors.Orange) : Colors.Transparent;
            Background = new SolidColorBrush(color);
        }

        private void DragDropPopup_DragEnter(object sender, DragEventArgs e)
        {
            if (!(sender is Popup popup)) return;
            Point mousePosition = e.GetPosition(this);
            var deltaX = popup.HorizontalOffset - mousePosition.X;
            var deltaY = popup.VerticalOffset - mousePosition.Y;

            if (popup.Placement == PlacementMode.Top)
                popup.VerticalOffset = mousePosition.Y - deltaY;
            else if (popup.Placement == PlacementMode.Bottom)
                popup.VerticalOffset = mousePosition.Y + deltaY;
            else if (popup.Placement == PlacementMode.Left)
                popup.HorizontalOffset = mousePosition.X - deltaX;
            else if (popup.Placement == PlacementMode.Right)
                popup.HorizontalOffset = mousePosition.X + deltaX;
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            DialogHelper.ShowMessageBar("MenuItem_SubmenuOpened");
        }
    }
}