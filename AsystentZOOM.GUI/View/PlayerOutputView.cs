#define tmalicki_debug

using AsystentZOOM.GUI.Converters;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using EventAggregator = AsystentZOOM.VM.Common.EventAggregator;
using SubscribeInfo = AsystentZOOM.VM.Common.EventAggregator.SubscribeInfo;

namespace AsystentZOOM.GUI.View
{
    public abstract class PlayerOutputView : UserControl
    {
        private DispatcherTimer _timer;
        private List<SubscribeInfo> _subscribesList;
        private static bool _isRestarting;
        private readonly MediaElement _meMain;
        private readonly TextBlock _textBlock;

        public PlayerOutputView()
        {
            var grid = new Grid();
            AddChild(grid);

            _meMain = new MediaElement
            {
                Name = nameof(_meMain), 
                Volume = .99
            };
            grid.Children.Add(_meMain);

#if (tmalicki_debug)
            _textBlock = new TextBlock { Foreground = new SolidColorBrush(Colors.Yellow) };
            BindingOperations.SetBinding(_textBlock, TextBlock.TextProperty, new Binding
            {
                Path = new PropertyPath(nameof(_meMain.Volume)),
                Source = _meMain
            });
            grid.Children.Add(_textBlock);
#endif
            DataContextChanged += AudioVideoOutputView_DataContextChanged;
        }

        private void AudioVideoOutputView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            _meMain.LoadedBehavior = MediaState.Manual;
            _meMain.MediaOpened += meMain_MediaOpened;
            _meMain.MediaEnded += meMain_MediaEnded;
            _meMain.SetBinding(MediaElement.VolumeProperty, new Binding
            {
                Path = new PropertyPath(nameof(ViewModel.Volume)),
                Mode = BindingMode.TwoWay,
                Source = ViewModel
            });
            _meMain.SetBinding(MediaElement.IsMutedProperty, new Binding
            {
                Path = new PropertyPath(nameof(ViewModel.IsMuted)),
                Mode = BindingMode.OneWay,
                Source = ViewModel
            });
            _meMain.SetBinding(MediaElement.SourceProperty, new Binding
            {
                Path = new PropertyPath(nameof(ViewModel.Source)),
                Mode = BindingMode.OneWay,
                Source = ViewModel,
                Converter = new SourceFileNameConverter()
            });

            if (_isRestarting)
            {
                if (ViewModel.PlayerState == PlayerStateEnum.Played)
                {
                    _meMain.Position = ViewModel.Position;
                    _meMain.Play();
                }
                _isRestarting = false;
            }

            string dataContextName = ViewModel.GetType().Name;
            _subscribesList = new List<SubscribeInfo>
            {
                EventAggregator.Subscribe($"{dataContextName}_Play", Play, () => true),
                EventAggregator.Subscribe($"{dataContextName}_Stop", Stop, () => true),
                EventAggregator.Subscribe($"{dataContextName}_Pause", Pause, () => true),
                EventAggregator.Subscribe($"{dataContextName}_Restart", Restart, () => true),
                EventAggregator.Subscribe<double>($"{dataContextName}_PositionChanged", PositionChanged, (p) => true),
                //EventAggregator.Subscribe<bool>(nameof(MainVM) + "_Close", (x) => CloseMainOutputWindow(), (p) => true),
                EventAggregator.Subscribe(nameof(MainVM) + "_Reset", CloseMainOutputWindow, () => true)
            };
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += _timer_Tick;
        }

        private void CloseMainOutputWindow()
        {
            _timer.Stop();
            _meMain.Close();
            _subscribesList?.ForEach(si => EventAggregator.UnSubscribe(si));
            _isRestarting = true;
        }

        protected virtual void MediaOpened(MediaElement mediaElement)
        {
            if (_meMain.NaturalDuration.HasTimeSpan)
                ViewModel.Duration = _meMain.NaturalDuration.TimeSpan;
        }

        private void _timer_Tick(object sender, EventArgs e)
            => ViewModel.Position = _meMain.Position;

        private void PositionChanged(double position)
        {
            _meMain.Position = TimeSpan.FromSeconds(position);
            ViewModel.Position = _meMain.Position;
        }

        private void meMain_MediaOpened(object sender, RoutedEventArgs e)
            => MediaOpened(sender as MediaElement);

        private void meMain_MediaEnded(object sender, RoutedEventArgs e)
            => EventAggregator.Publish($"{typeof(ILayerVM)}_Finished", ViewModel);

        private PlayerVM ViewModel
            => (PlayerVM)DataContext;

        private void Play()
        {
            _meMain.Play();
            _timer.Start();
        }

        private void Stop()
        {
            _meMain.Close();
            _timer.Stop();
        }

        private void Pause() => _meMain.Pause();

        private void Restart()
        {
            Stop();
            Play();
        }
    }
}
