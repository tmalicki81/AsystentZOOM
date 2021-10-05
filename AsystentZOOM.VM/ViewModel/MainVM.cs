﻿using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    public class MainVM : SingletonBaseVM
    {
        public MainVM()
        {
            ProgressInfo = new ProgressInfoVM();
            EventAggregator.Subscribe<Size>($"{typeof(ILayerVM)}_ChangeOutputSize", ChangeMediaSize, (s) => !ProgressInfo.ProgressBarVisibility);
        }

        private Size _mediaSize;
        private void ChangeMediaSize(Size size)
        {
            _mediaSize = size;
            ChangeOutputSize();
        }

        private enum SizeConvDir : int
        {
            ToBorder = -1,
            ToOutput = 1
        }

        private static class SizeConv
        {
            private const int _margin = 8;
            private const int _top = 30;

            internal static double GetWidth(SizeConvDir direction, double width)
                => width - _margin * 2 * (int)direction;

            internal static double GetHeight(SizeConvDir direction, double height) 
                => height - _top * (int)direction - _margin * (int)direction;

            internal static double GetTop(SizeConvDir direction, double top)
                => top +  _top + (int)direction;

            internal static double GetLeft(SizeConvDir direction, double left)
                => left + _margin * (int)direction;
        }

        private void ChangeOutputSize()
        {
            double calcOutputWindowTop = SizeConv.GetTop(SizeConvDir.ToOutput, BorderWindowTop);
            double calcOutputWindowLeft = SizeConv.GetLeft(SizeConvDir.ToOutput, BorderWindowLeft);
            double calcOutputWindowWidth = SizeConv.GetWidth(SizeConvDir.ToOutput, BorderWindowWidth);
            double calcOutputWindowHeight = SizeConv.GetHeight(SizeConvDir.ToOutput, BorderWindowHeight);

            if (_mediaSize == default)
            {
                // Jeśli nie włączono żadnego medium
                OutputWindowTop = calcOutputWindowTop;
                OutputWindowLeft = calcOutputWindowLeft;
                OutputWindowHeight = calcOutputWindowHeight;
                OutputWindowWidth = calcOutputWindowWidth;
            }
            else
            {
                double proportions = _mediaSize.Height / _mediaSize.Width;
                double targetOutputWindowHeight = calcOutputWindowWidth * proportions;
                double targetOutputWindowWidth = calcOutputWindowHeight / proportions;

                if (targetOutputWindowHeight <= calcOutputWindowHeight)
                {
                    // Jeśli dostosować wysokość
                    OutputWindowHeight = targetOutputWindowHeight;
                    OutputWindowWidth = calcOutputWindowWidth;
                    OutputWindowTop = calcOutputWindowTop + (calcOutputWindowHeight - targetOutputWindowHeight) / 2;
                    OutputWindowLeft = calcOutputWindowLeft;
                }
                else if (targetOutputWindowWidth <= calcOutputWindowWidth)
                {
                    // Jeśli dostosować szerokość
                    OutputWindowHeight = calcOutputWindowHeight;
                    OutputWindowWidth = targetOutputWindowWidth;
                    OutputWindowTop = calcOutputWindowTop;
                    OutputWindowLeft = calcOutputWindowLeft + (calcOutputWindowWidth - targetOutputWindowWidth) / 2;
                }
            }
        }

        private static Dispatcher _dispatcher;
        public static Dispatcher Dispatcher
        {
            get => _dispatcher;
            set => _dispatcher = value;
        }

        public const string Version = "L-1.0.0";

        public MeetingVM Meeting
            => SingletonVMFactory.Meeting;

        [XmlIgnore]
        public List<ILayerVM> Layers
            => SingletonVMFactory.Layers;

        [XmlIgnore]
        private ProgressInfoVM _progressInfo;
        public ProgressInfoVM ProgressInfo
        {
            get => _progressInfo;
            set => SetValue(ref _progressInfo, value, nameof(ProgressInfo));
        }

        private bool _warnBeforeStoppingSharing = false;
        public bool WarnBeforeStoppingSharing
        {
            get => _warnBeforeStoppingSharing;
            set => SetValue(ref _warnBeforeStoppingSharing, value, nameof(WarnBeforeStoppingSharing));
        }

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get => _windowState;
            set => SetValue(ref _windowState, value, nameof(WindowState));
        }

        #region Output

        private double _outputWindowTop;
        public double OutputWindowTop
        {
            get => _outputWindowTop;
            set => SetValue(ref _outputWindowTop, value, nameof(OutputWindowTop));
        }

        private double _outputWindowLeft;
        public double OutputWindowLeft
        {
            get => _outputWindowLeft;
            set => SetValue(ref _outputWindowLeft, value, nameof(OutputWindowLeft));
        }

        private double _outputWindowHeight;
        public double OutputWindowHeight
        {
            get => _outputWindowHeight;
            set => SetValue(ref _outputWindowHeight, value, nameof(OutputWindowHeight));
        }

        private double _outputWindowWidth;
        public double OutputWindowWidth
        {
            get => _outputWindowWidth;
            set => SetValue(ref _outputWindowWidth, value, nameof(OutputWindowWidth));
        }

        #endregion Output

        #region Border

        private double _borderWindowTop = 200;
        public double BorderWindowTop
        {
            get => _borderWindowTop;
            set
            {
                SetValue(ref _borderWindowTop, value, nameof(BorderWindowTop));
                ChangeOutputSize();
            }
        }

        private double _borderWindowLeft = 100;
        public double BorderWindowLeft
        {
            get => _borderWindowLeft;
            set
            {
                SetValue(ref _borderWindowLeft, value, nameof(BorderWindowLeft));
                ChangeOutputSize();
            }
        }

        private double _borderWindowHeight = 600;
        public double BorderWindowHeight
        {
            get => _borderWindowHeight;
            set
            {
                SetValue(ref _borderWindowHeight, value, nameof(BorderWindowHeight));
                ChangeOutputSize();
            }
        }

        private double _borderWindowWidth = 824;
        public double BorderWindowWidth
        {
            get => _borderWindowWidth;
            set
            {
                SetValue(ref _borderWindowWidth, value, nameof(BorderWindowWidth));
                ChangeOutputSize();
            }
        }

        #endregion Border

        #region Panel

        private double _panelWindowTop = 0;
        public double PanelWindowTop
        {
            get => _panelWindowTop;
            set => SetValue(ref _panelWindowTop, value, nameof(PanelWindowTop));
        }

        private double _panelWindowLeft = 0;
        public double PanelWindowLeft
        {
            get => _panelWindowLeft;
            set => SetValue(ref _panelWindowLeft, value, nameof(PanelWindowLeft));
        }

        private double _panelWindowHeight = 600;
        public double PanelWindowHeight
        {
            get => _panelWindowHeight;
            set => SetValue(ref _panelWindowHeight, value, nameof(PanelWindowHeight));
        }

        private double _panelWindowWidth = 824;
        public double PanelWindowWidth
        {
            get => _panelWindowWidth;
            set => SetValue(ref _panelWindowWidth, value, nameof(PanelWindowWidth));
        }

        #endregion Panel

        [XmlIgnore]
        public string MessageBarText
        {
            get => _messageBarText;
            set => SetValue(ref _messageBarText, value, nameof(MessageBarText));
        }
        private string _messageBarText = "Asystent ZOOM";

        [XmlIgnore]
        public bool MessageBarIsNew
        {
            get => _messageBarIsNew;
            set => SetValue(ref _messageBarIsNew, value, nameof(MessageBarIsNew));
        }
        private bool _messageBarIsNew;

        [XmlIgnore]
        public bool IsMenuOpened
        {
            get => _isMenuOpened;
            set => SetValue(ref _isMenuOpened, value, nameof(IsMenuOpened));
        }
        private bool _isMenuOpened;

        public bool ToolButtonsVisible
        {
            get => _toolButtonsVisible;
            set => SetValue(ref _toolButtonsVisible, value, nameof(ToolButtonsVisible));
        }
        private bool _toolButtonsVisible = true;

        private RelayCommand _resetApplicationCommand;
        public RelayCommand ResetApplicationCommand
            => _resetApplicationCommand ?? (_resetApplicationCommand = new RelayCommand(ResetApplicationExecute));

        private void ResetApplicationExecute()
        {
            if (string.IsNullOrEmpty(MeetingVM.LocalFileName))
                Meeting.SaveTempFile();
            else
                Meeting.SaveLocalFile(true);
            Process.Start("AsystentZOOM.GUI.exe", $@"""{MeetingVM.LocalFileName}""");
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Shutdown();
        }

        private RelayCommand _resetVisualSettingsCommand;
        public RelayCommand ResetVisualSettingsCommand
            => _resetVisualSettingsCommand ??= new RelayCommand(ResetVisualSettingsExecute);

        private void ResetVisualSettingsExecute()
        {
            //var dr = DialogHelper.ShowMessageBox(
            //    "Czy zresetować ustawienia widoków?", 
            //    "Reset ustawień widoków", 
            //    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            //if (dr == MessageBoxResult.No)
            //    return;

            foreach (var target in SingletonVMFactory.AllSingletons)
            {
                if (target == null || target != this)
                    continue;
                var source = Activator.CreateInstance(target.GetType());
                SingletonVMFactory.SetSingletonValues(source);
            }
            MainVM mainVM = this;
            MainVM s = (MainVM)Activator.CreateInstance(GetType());
            SingletonVMFactory.CopyValuesWhenDifferent(s, ref mainVM);

            SingletonVMFactory.SaveAllSingletons();
        }

        private RelayCommand _newTimePieceCommand;
        public RelayCommand NewTimePieceCommand
            => _newTimePieceCommand ??= new RelayCommand(NewTimePieceExecute);

        private void NewTimePieceExecute() 
        {
            SingletonVMFactory.TimePiece.IsEnabled = true;
            SingletonVMFactory.Background.IsEnabled = true;
        }

        private RelayCommand _quitApplicationCommand;
        public RelayCommand QuitApplicationCommand
            => _quitApplicationCommand ??= new RelayCommand(() => Application.Current.MainWindow.Close());
    }
}
