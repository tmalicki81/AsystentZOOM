using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections.Generic;
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

        private double _top = 0;
        public double Top
        {
            get => _top;
            set => SetValue(ref _top, value, nameof(Top));
        }

        private double _left = 0;
        public double Left
        {
            get => _left;
            set => SetValue(ref _left, value, nameof(Left));
        }

        private double _height = 600;
        public double Height
        {
            get => _height;
            set
            {
                if (SetValue(ref _height, value, nameof(Height)))
                    EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(Height)}", _height);
            }
        }

        private double _width = 824;
        public double Width
        {
            get => _width;
            set
            {
                if (SetValue(ref _width, value, nameof(Width)))
                    EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(Width)}", _width);
            }
        }

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
    }
}
