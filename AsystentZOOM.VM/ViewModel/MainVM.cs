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
using System.IO;
using AsystentZOOM.VM.Common.Dialog;

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

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get => _windowState;
            set => SetValue(ref _windowState, value, nameof(WindowState));
        }

        private double _outputWindowTop = 50;
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

        [XmlIgnore]
        public bool IsMenuOpened
        {
            get => _IsMenuOpened;
            set => SetValue(ref _IsMenuOpened, value, nameof(IsMenuOpened));
        }
        private bool _IsMenuOpened;

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

            EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(OutputWindowWidth)}", OutputWindowWidth);
            EventAggregator.Publish($"{nameof(MainVM)}_Change_{nameof(OutputWindowHeight)}", OutputWindowHeight);
        }
    }
}
