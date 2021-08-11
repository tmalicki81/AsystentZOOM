using BinaryHelper;
using BinaryHelper.FileRepositories;
using FileService.EventArgs;
using FileService.FileRepositories;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;

namespace AsystentZOOM
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OnQuit;

        private string _logText;
        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogText)));
            }
        }

        private int _percentCompletted;
        public int PercentCompletted
        {
            get => _percentCompletted;
            set
            {
                _percentCompletted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PercentCompletted)));
            }
        }

        private void CheckForUpdate()
        {
            var ftpService = new FtpBinFileRepository();
            var isoService = new AppIsoStorFileRepository();

            bool mustDownloadBinaryFiles = MustDownloadBinaryFiles(ftpService, isoService, out BuildVersion ftpBuildVersion);
            if (mustDownloadBinaryFiles)
            {
                ftpService.OnPushedFile += FtpService_OnPushed;
                ftpService.PushToTarget(targetFileRepository: isoService, null);
                ftpBuildVersion.Save(isoService, BuildVersionHelper.BuildVersionFile);
            }
        }

        private void FtpService_OnPushed(object sender, PushedFileEventArgs e)
        {
            PercentCompletted = e.PercentCompletted;
        }

        private bool MustDownloadBinaryFiles(BaseFileRepository ftpService, BaseFileRepository isoService, out BuildVersion ftpBuildVersion)
        {
            ftpBuildVersion = BuildVersion.Load(ftpService, BuildVersionHelper.BuildVersionFile);
            BuildVersion isoBuildVersion;
            try
            {
                isoBuildVersion = BuildVersion.Load(isoService, BuildVersionHelper.BuildVersionFile);
            }
            catch (IsolatedStorageException ex) when (ex.InnerException is FileNotFoundException)
            {
                isoBuildVersion = null;
            }
            return isoBuildVersion == null || isoBuildVersion.AppVersion != ftpBuildVersion.AppVersion;
        }

        private void RunApplication()
        {
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                PropertyInfo pi = typeof(IsolatedStorageFile).GetProperty("RootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                string rootDirectory = (string)pi.GetValue(file);
                // C:\Users\tmalicki.KAMSOFT\AppData\Local\IsolatedStorage\5rzfwjzq.kvy\mlxk5hri.hh4\Url.jgnhmyjdhkcv11tu5f1hm4lbwrztxzqa\AppFiles\
                Directory.SetCurrentDirectory(rootDirectory);
                Process.Start(Path.Combine(rootDirectory, "AsystentZOOM.GUI.exe"), App.Args);
            }
        }

        private AsyncCommand _runUpdatedAppCommand;
        public AsyncCommand RunUpdatedAppCommand
            => _runUpdatedAppCommand ??= new AsyncCommand(RunUpdatedApp);

        private void RunUpdatedApp() 
        {
            CheckForUpdate();
            RunApplication();
            OnQuit?.Invoke(this, EventArgs.Empty);
        }
    }
}
