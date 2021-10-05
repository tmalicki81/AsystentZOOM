using AsystentZOOM.VM.Common;
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
            EventAggregator.Subscribe<Size>($"{typeof(ILayerVM)}_ChangeOutputSize", ChangeOutputSize, (s) => !ProgressInfo.ProgressBarVisibility);
        }

        private void ChangeOutputSize(Size size)
        {
            double proportions = size.Height / size.Width;
            BorderWindowHeight = BorderWindowWidth * proportions;
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

        private double _BorderWindowTop = 50;
        public double BorderWindowTop
        {
            get => _BorderWindowTop;
            set => SetValue(ref _BorderWindowTop, value, nameof(BorderWindowTop));
        }

        private double _BorderWindowLeft = 0;
        public double BorderWindowLeft
        {
            get => _BorderWindowLeft;
            set => SetValue(ref _BorderWindowLeft, value, nameof(BorderWindowLeft));
        }

        private double _BorderWindowHeight = 600;
        public double BorderWindowHeight
        {
            get => _BorderWindowHeight;
            set => SetValue(ref _BorderWindowHeight, value, nameof(BorderWindowHeight));
        }

        private double _BorderWindowWidth = 824;
        public double BorderWindowWidth
        {
            get => _BorderWindowWidth;
            set => SetValue(ref _BorderWindowWidth, value, nameof(BorderWindowWidth));
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
