using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;

namespace AsystentZOOM.VM.ViewModel
{
    public class MainVM : SingletonBaseVM
    {
        public MainVM()
        {
            ProgressInfo = new ProgressInfoVM();
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

        private WindowModeEnum _windowModePrev;
        private WindowModeEnum _windowMode = WindowModeEnum.Normal;
        public WindowModeEnum WindowMode
        {
            get => _windowMode;
            set
            {
                _windowModePrev = _windowMode;
                SetValue(ref _windowMode, value, nameof(WindowMode));

                if (_windowMode == WindowModeEnum.FullScreen && WindowState == WindowState.Maximized)
                {
                    WindowStyle = WindowStyle.None;
                    AllowsTransparency = true;
                    return;
                }
                EventAggregator.Publish(nameof(MainVM) + "_Close", WarnBeforeStoppingSharing);
                switch (_windowMode)
                {
                    case WindowModeEnum.FullScreen:
                        WindowState = WindowState.Maximized;
                        WindowStyle = WindowStyle.None;
                        AllowsTransparency = true;
                        Topmost = false;
                        break;
                    case WindowModeEnum.Normal:
                        WindowState = WindowState.Normal; 
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        AllowsTransparency = false;
                        Topmost = false;
                        break;
                    case WindowModeEnum.NoBorder:
                        WindowState = WindowState.Normal; 
                        WindowStyle = WindowStyle.None;         
                        AllowsTransparency = true;
                        Topmost = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                EventAggregator.Publish(nameof(MainVM) + "_Open");
                EventAggregator.Publish(nameof(MainVM) + "_ActivatePanel");
            }
        }

        private WindowStyle _windowStyle = WindowStyle.SingleBorderWindow;
        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set => SetValue(ref _windowStyle, value, nameof(WindowStyle));
        }

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                SetValue(ref _windowState, value, nameof(WindowState));
                if (_windowState == WindowState.Maximized && WindowMode != WindowModeEnum.FullScreen)
                    WindowMode = WindowModeEnum.FullScreen;
            }
        }

        private bool _allowsTransparency = false;
        public bool AllowsTransparency
        {
            get => _allowsTransparency;
            set => SetValue(ref _allowsTransparency, value, nameof(AllowsTransparency));
        }

        private bool _topmost = false;
        public bool Topmost
        {
            get => _topmost;
            set => SetValue(ref _topmost, value, nameof(Topmost));
        }

        private double _outputWindowTop = 0;
        public double OutputWindowTop
        {
            get => _outputWindowTop;
            set => SetValue(ref _outputWindowTop, value, nameof(OutputWindowTop));
        }

        private double _outputWindowLeft = 0;
        public double OutputWindowLeft
        {
            get => _outputWindowLeft;
            set => SetValue(ref _outputWindowLeft, value, nameof(OutputWindowLeft));
        }

        private double _outputWindowHeight = 600;
        public double OutputWindowHeight
        {
            get => _outputWindowHeight;
            set
            {
                if (SetValue(ref _outputWindowHeight, value, nameof(OutputWindowHeight)))
                    EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(OutputWindowHeight)}", _outputWindowHeight);
            }
        }

        private double _outputWindowWidth = 824;
        public double OutputWindowWidth
        {
            get => _outputWindowWidth;
            set
            {
                if (SetValue(ref _outputWindowWidth, value, nameof(OutputWindowWidth)))
                    EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(OutputWindowWidth)}", _outputWindowWidth);
            }
        }

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
            set
            {
                if (SetValue(ref _panelWindowHeight, value, nameof(PanelWindowHeight)))
                    EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(PanelWindowHeight)}", _panelWindowHeight);
            }
        }

        private double _panelWindowWidth = 824;
        public double PanelWindowWidth
        {
            get => _panelWindowWidth;
            set
            {
                if (SetValue(ref _panelWindowWidth, value, nameof(PanelWindowWidth)))
                    EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(PanelWindowWidth)}", _panelWindowWidth);
            }
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

        private RelayCommand _exitFullScreenCommand;
        public RelayCommand ExitFullScreenCommand
            => _exitFullScreenCommand ?? (_exitFullScreenCommand = new RelayCommand(ExitFullScreenExecute));

        private void ExitFullScreenExecute()
        {
            if (WindowState == WindowState.Maximized)
                WindowMode = _windowModePrev;
        }

        private RelayCommand _resetApplicationCommand;
        public RelayCommand ResetApplicationCommand
            => _resetApplicationCommand ?? (_resetApplicationCommand = new RelayCommand(ResetApplicationExecute));

        private void ResetApplicationExecute()
        {
            Meeting.SaveLocalFile();
            string meetingFile = MeetingVM.LocalFileName;
            Process.Start("AsystentZOOM.GUI.exe", meetingFile);
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Shutdown();
        }
    }
}
