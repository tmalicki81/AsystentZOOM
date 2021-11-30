using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using FileService.Common;
using FileService.EventArgs;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Common.AudioRecording
{
    public interface IAudioRecordingProvider : IBaseVM, IDisposable
    {
        bool IsEnabledInThisMachine { get; set; }
        bool IsReady { get; }
        bool IsRecording { get; set; }
        IRelayCommand OpenRecordingFolderCommand { get; }
        TimeSpan RecordingTime { get; set; }
        IRelayCommand StartRecordingCommand { get; }
        IRelayCommand StopRecordingCommand { get; }
        string Title { get; }
        bool UseMicrophoneInThisMachine { get; set; }
        event EventHandler<EventArgs<IRelayCommand>> OnCommandExecuted;
        void ExecuteRequests();
    }

    /// <summary>
    /// Rejestrowanie dźwięku przez inny komponent (np. spotkanie albo punkt)
    /// </summary>
    [Serializable]
    public abstract class AudioRecordingProvider : BaseVM, IAudioRecordingProvider
    {
        /// <summary>
        /// Tytuł spotkania lub punktu, które będzie nagrywane
        /// </summary>
        [XmlIgnore]
        public abstract string Title { get; }

        /// <summary>
        /// Czy rejestrator dźwięku jest gotowy do pracy
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Czy wykonano żądanie rozpoczęcia nagrywania (poprzez zmianę wartości IsRecording)
        /// </summary>
        private bool _requestStartRecording;

        /// <summary>
        /// Czy wykonano żądanie zakonczenia nagrywania (poprzez zmianę wartości IsRecording)
        /// </summary>
        private bool _requestStopRecording;

        /// <summary>
        /// Czy nagrywanie jest w toku
        /// </summary>
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording == value)
                    return;

                _requestStartRecording = !_isRecording && value;
                _requestStopRecording = _isRecording && !value;

                ExecuteRequests();

                SetValue(ref _isRecording, value, nameof(IsRecording));
            }
        }
        private bool _isRecording;

        public void ExecuteRequests()
        {
            if (IsReady)
            {
                if (_requestStartRecording)
                    // Jeśli zarządano rozpoczęcia nagrywania
                    MainVM.Dispatcher.Invoke(StartRecording);
                else if (_requestStopRecording)
                    // Jeśli zarządano zakończenia nagrywania
                    MainVM.Dispatcher.Invoke(StopRecording);

                _requestStartRecording = false;
                _requestStopRecording = false;
            }
            else
            { 
            }
        }

        /// <summary>
        /// Czy nagrywać dźwięk
        /// </summary>
        [XmlIgnore]
        public bool IsEnabledInThisMachine
        {
            get => _isEnabledInThisMachine;
            set => SetValue(ref _isEnabledInThisMachine, value, nameof(IsEnabledInThisMachine));
        }
        private static bool _isEnabledInThisMachine;

        /// <summary>
        /// Czy nagrywać dźwięk z mikrofonu na bieżącej maszynie
        /// </summary>
        public bool UseMicrophoneInThisMachine
        {
            get => _useMicrophoneInThisMachine;
            set => SetValue(ref _useMicrophoneInThisMachine, value, nameof(UseMicrophoneInThisMachine));
        }
        private static bool _useMicrophoneInThisMachine;

        /// <summary>
        /// Czy nagrywać dźwięk
        /// </summary>
        [XmlIgnore]
        public bool SaveMp3FileAfterExitApp
        {
            get => _saveMp3FileAfterExitApp;
            set => SetValue(ref _saveMp3FileAfterExitApp, value, nameof(SaveMp3FileAfterExitApp));
        }
        private static bool _saveMp3FileAfterExitApp = true;

        /// <summary>
        /// Czas nagrania
        /// </summary>
        public TimeSpan RecordingTime
        {
            get => _recordingTime;
            set => SetValue(ref _recordingTime, value, nameof(RecordingTime), false);
        }
        private TimeSpan _recordingTime;

        /// <summary>
        /// Obsługa zapisu dźwięku z karty dźwiękowej
        /// </summary>
        private AudioCardRecorder _audioCardRecorder;

        /// <summary>
        /// Obsługa zapisu dźwięku z mikrofonu
        /// </summary>
        private MicrophoneRecorder _microphoneRecorder;

        /// <summary>
        /// Zdarzenie wykonania polecenia z interfejsu użytkownika
        /// </summary>
        public event EventHandler<EventArgs<IRelayCommand>> OnCommandExecuted;

        /// <summary>
        /// Krótka nazwa pliku
        /// </summary>
        private string ShortFileName
            => PathHelper.GetShortFileName(_audioCardRecorder.FileName, '\\');

        #region StartRecordingCommand

        public RelayCommand StartRecordingCommand
            => _startRecordingCommand ??= new RelayCommand(StartRecordingExecute, () => !IsRecording);
        private RelayCommand _startRecordingCommand;

        private void StartRecordingExecute()
        {
            if (IsRecording)
                return;
            IsRecording = true;
            OnCommandExecuted?.Invoke(this, new EventArgs<IRelayCommand>(StartRecordingCommand));
        }

        private void StartRecording()
        {
            if (!IsEnabledInThisMachine)
            {
                DialogHelper.ShowMessageBar($"Na tym komputerze wyłączono nagrywanie dźwięku.");
                return;
            }

            RecordingTime = TimeSpan.Zero;
            string getFileName() => $"{DateTime.Now:yyyy_MM_dd__HH_mm_ss}__{PathHelper.NormalizeToFileName(Title)}";

            _audioCardRecorder = new AudioCardRecorder(getFileName, GetRecordingFolder());
            _audioCardRecorder.OnRecordingTimeChanged += _recorderFromAudioCard_OnRecordingTimeChanged;
            _audioCardRecorder.OnStartRecording += _recorderFromAudioCard_OnStartRecording;
            _audioCardRecorder.StartRecording();

            if (UseMicrophoneInThisMachine)
            {
                _microphoneRecorder = new MicrophoneRecorder(getFileName, GetRecordingFolder());
                _microphoneRecorder.StartRecording();
                SystemSounds.Beep.Play();
            }
            else
            {
                _microphoneRecorder = null;
                DialogHelper.ShowMessageBar($"Oczekiwanie na sygnał z karty dźwiękowej (zapis do pliku: {ShortFileName})");
            }
        }

        private void _recorderFromAudioCard_OnStartRecording(object sender, EventArgs<DateTime> e)
            => DialogHelper.ShowMessageBar($"Rozpoczęto nagrywanie dźwięku do pliku: {ShortFileName}");

        private void _recorderFromAudioCard_OnRecordingTimeChanged(object sender, EventArgs<TimeSpan> e)
            => RecordingTime = e.Value;

        #endregion StartRecordingCommand

        #region StopRecordingCommand

        private RelayCommand _stopRecordingCommand;
        public RelayCommand StopRecordingCommand
            => _stopRecordingCommand ??= new RelayCommand(StopRecordingExecute, () => IsRecording);

        private void StopRecordingExecute()
        {
            if (!IsRecording)
                return;
            IsRecording = false;
            OnCommandExecuted?.Invoke(this, new EventArgs<IRelayCommand>(StopRecordingCommand));
        }

        private async void StopRecording()
        {
            RecordingTime = TimeSpan.Zero;

            if (_audioCardRecorder == null)
                return;

            _audioCardRecorder.StopRecording();
            _audioCardRecorder.OnRecordingTimeChanged -= _recorderFromAudioCard_OnRecordingTimeChanged;
            _audioCardRecorder.OnStartRecording -= _recorderFromAudioCard_OnStartRecording;

            _microphoneRecorder?.StopRecording();

            if (!SaveMp3FileAfterExitApp)
            {
                var mp3Files = new string[]
                    {
                        _audioCardRecorder?.FileName,
                        _microphoneRecorder?.FileName
                    }
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToArray();

                try
                {
                    using (var progress = new ShowProgressInfo("Kończenie nagrywania", true, null))
                    {
                        string mp3FileName = await Task.Run(() => MixMp3Files(progress, mp3Files));
                        string mp3FileShortName = PathHelper.GetShortFileName(mp3FileName, '\\');
                        DialogHelper.ShowMessageBar($"Przekonwertowano plik {ShortFileName} do formatu mp3");
                        _audioCardRecorder = null;
                        _microphoneRecorder = null;

                        var localRepo = MediaLocalFileRepositoryFactory.AudioRecording;
                        var ftpRepo = MediaFtpFileRepositoryFactory.AudioRecording;

                        progress.TaskName = "Wysyłanie nagrania do chmury";
                        progress.IsIndeterminate = false;
                        var handler = new EventHandler<SavingFileEventArgs>((s, e) => progress.PercentCompletted = e.PercentCompleted);
                        ftpRepo.OnSavingFile += handler;
                        try
                        {
                            await Task.Run(() => localRepo.CopyTo(ftpRepo, mp3FileShortName));
                        }
                        finally
                        {
                            ftpRepo.OnSavingFile -= handler;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await HandleException(ex);
                }
            }
        }

        /// <summary>
        /// Miksowanie kilku plików mp3
        /// </summary>
        /// <param name="mp3Files">Lista plików mp3</param>
        /// <returns>Nazwa powstałego pliku mp3 (będącego miksem kilku)</returns>
        private string MixMp3Files(IProgressInfoVM progress, params string[] mp3Files)
        {
            string directory = GetRecordingFolder();
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fileNameForMix = Path.Combine(directory, BaseAudioRecorder.GetFileName(() => Title, null, "mp3"));
            var audioFileReaders = mp3Files.Select(fileName => new AudioFileReader(fileName)).ToList();

            try
            {
                var mixer = new MixingSampleProvider(audioFileReaders);
                var waveProvider = mixer.ToWaveProvider();

                using (var mp3Writer = new LameMP3FileWriter(fileNameForMix, mixer.WaveFormat, 128))
                using (var waveStream = new WaveProviderToWaveStream(waveProvider))
                {
                    const int bufferSize = 4096;
                    byte[] buffer = new byte[bufferSize];
                    int readedBytes;
                    decimal allWritedKiloBytes = 0;
                    while (true)
                    {
                        readedBytes = waveStream.Read(buffer, 0, bufferSize);
                        if (readedBytes <= 0) break;
                        allWritedKiloBytes += Math.Round((decimal)readedBytes / 1024, 2);
                        mp3Writer.Write(buffer, 0, readedBytes);
                        progress.TaskName = $"Konwersja pliku do formatu mp3 ({allWritedKiloBytes} kB)";
                    }
                }
                mixer.RemoveAllMixerInputs();
            }
            finally
            {
                audioFileReaders.ForEach(r => r.Dispose());
            }
            mp3Files.ToList().ForEach(f => File.Delete(f));
            return fileNameForMix;
        }

        #endregion StopRecordingCommand

        #region OpenRecordingFolderCommand

        public RelayCommand OpenRecordingFolderCommand
            => _openRecordingFolderCommand ??= new RelayCommand(OpenRecordingFolder);

        private RelayCommand _openRecordingFolderCommand;

        /// <summary>
        /// Otworzenie pliku z nagraniami
        /// </summary>
        private void OpenRecordingFolder()
        {
            string folderPath = GetRecordingFolder();
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            Process.Start("explorer.exe", folderPath);
        }

        public const string AudioRecording = nameof(AudioRecording);

        /// <summary>
        /// Pobranie katalogu, do którego zapisywane sa nagrania
        /// </summary>
        /// <returns></returns>
        private string GetRecordingFolder()
            => MediaLocalFileRepositoryFactory.AudioRecording.RootDirectory;

        #endregion OpenRecordingFolderCommand

        /// <summary>
        /// Zwolnienie zasobów, zakończenie nagrywania(np. przed zamknięciem aplikacji)
        /// </summary>
        public void Dispose()
        {
            if (IsRecording)
                IsRecording = false;
        }

        #region IAudioRecordingProvider

        IRelayCommand IAudioRecordingProvider.OpenRecordingFolderCommand => OpenRecordingFolderCommand;
        IRelayCommand IAudioRecordingProvider.StartRecordingCommand => StartRecordingCommand;
        IRelayCommand IAudioRecordingProvider.StopRecordingCommand => StopRecordingCommand;

        #endregion IAudioRecordingProvider
    }
}
