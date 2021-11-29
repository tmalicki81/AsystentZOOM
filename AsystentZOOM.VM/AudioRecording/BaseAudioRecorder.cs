using FileService.Common;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AsystentZOOM.VM.Common.AudioRecording
{
    /// <summary>
    /// Obsługa nagrywania dźwięku do pliku
    /// </summary>
    public abstract class BaseAudioRecorder
    {
        /// <summary>
        /// Zdarzenie zmiany czasu nagrania
        /// </summary>
        public event EventHandler<EventArgs<TimeSpan>> OnRecordingTimeChanged;

        /// <summary>
        /// Zdarzenie rozpoczęcia nagrywania
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> OnStartRecording;

        /// <summary>
        /// Nazwa pliku, do którego następuje zapis nagrania dźwiękowego
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// Wejście audio
        /// </summary>
        private IWaveIn _waveIn;

        /// <summary>
        /// Zapisywanie dźwięku
        /// </summary>
        private WaveFileWriter _waveWriter;

        /// <summary>
        /// Czy pojawiły się dane do zapisu
        /// </summary>
        private bool _waitingForDataAvailable;

        /// <summary>
        /// Czas rozpoczecia nagrywania
        /// </summary>
        private DateTime _startRecording;

        /// <summary>
        /// Czas ostatniej aktualizacji czasu nagrania
        /// </summary>
        private DateTime _recordingTimeLastUpdated;

        /// <summary>
        /// Metoda tworząca obiekt wejścia audio
        /// </summary>
        /// <returns></returns>
        public abstract IWaveIn CreateWaveIn();

        /// <summary>
        /// Prefiks nazwy pliku
        /// </summary>
        public abstract string FilePrefix { get; }

        /// <summary>
        /// Funkcja zwracająca tytuł nagrania
        /// </summary>
        private readonly Func<string> _titleFunction;

        /// <summary>
        /// Obsługa nagrywania dźwięku do pliku
        /// </summary>
        /// <param name="titleFunction">Funkcja zwracająca tytuł nagrania</param>
        public BaseAudioRecorder(Func<string> titleFunction, string recordFilesDirectory)
        {
            _titleFunction = titleFunction;

            if (!Directory.Exists(recordFilesDirectory))
                Directory.CreateDirectory(recordFilesDirectory);

            FileName = Path.Combine(recordFilesDirectory, GetFileName(FilePrefix, "wav"));
        }

        /// <summary>
        /// Nazwa pliku dźwiękowego
        /// </summary>
        /// <param name="prefix">Prefiks pliku</param>
        /// <param name="extension">Rozszerzenie pliku</param>
        /// <returns></returns>
        public string GetFileName(string prefix, string extension)
            => GetFileName(_titleFunction, prefix, extension);

        /// <summary>
        /// Nazwa pliku dźwiękowego
        /// </summary>
        /// <param name="titleFunction">Funkcja zwracająca tytuł nagrania</param>
        /// <param name="prefix">Prefiks pliku</param>
        /// <param name="extension">Rozszerzenie pliku</param>
        /// <returns></returns>
        public static string GetFileName(Func<string> titleFunction, string prefix, string extension)
            => $"{titleFunction.Invoke()}_{prefix}.{extension}";

        /// <summary>
        /// Obsługa sytuacji, w której dostępne sa dane do zapisu w pliku dźwiękowym
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waitingForDataAvailable && e.BytesRecorded > 0)
            {
                // Rozpoczecie nagrywania
                _waitingForDataAvailable = false;
                _startRecording = DateTime.Now;
                OnStartRecording?.Invoke(null, new EventArgs<DateTime>(_startRecording));
            }
            if (!_waitingForDataAvailable && (DateTime.Now - _recordingTimeLastUpdated).TotalSeconds >= 1)
            {
                // Aktualizacja czasu nagrania
                var recordingTime = DateTime.Now - _startRecording;
                OnRecordingTimeChanged?.Invoke(null, new EventArgs<TimeSpan>(recordingTime));
                _recordingTimeLastUpdated = DateTime.Now;
            }
            // Zapis danych do pliku dxwiękowego
            _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        /// <summary>
        /// Rozpoczęcie nagrywania
        /// </summary>
        public void StartRecording()
        {
            _waveIn = CreateWaveIn();
            _waveIn.DataAvailable += OnDataAvailable;
            _waveWriter = new WaveFileWriter(FileName, _waveIn.WaveFormat);
            _waitingForDataAvailable = true;

            _waveIn.StartRecording();
            OnRecordingTimeChanged?.Invoke(null, new EventArgs<TimeSpan>(TimeSpan.Zero));
        }

        /// <summary>
        /// Zatrzymanie nagrywania (i zamknięcie zapisywania pliku dźwiękowego)
        /// </summary>
        public async void StopRecording()
        {
            _waveIn.DataAvailable -= OnDataAvailable;

            _waveWriter.Flush();
            _waveWriter.Dispose();
            _waveWriter = null;

            _waveIn.StopRecording();
            await Task.Run(_waveIn.Dispose);
            _waveIn = null;

            OnRecordingTimeChanged?.Invoke(null, new EventArgs<TimeSpan>(TimeSpan.Zero));
        }
    }
}
