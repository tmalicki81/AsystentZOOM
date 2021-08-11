namespace FileService.EventArgs
{
    /// <summary>
    /// Argument zdarzenia pobierania pojedyńczego pliku z repozytorium plikowego
    /// </summary>
    public class LoadingFileEventArgs : System.EventArgs
    {
        public LoadingFileEventArgs(string fileName, long fileSize, long bytesUploaded)
        {
            FileName = fileName;
            FileSize = fileSize;
            BytesDownloaded = bytesUploaded;
            PercentCompleted = fileSize > 0 ? (int)(100 * bytesUploaded / fileSize) : 0;
        }

        /// <summary>
        /// Nazwa pobieranego pliku (względem katalogu głównego danego repozytorium)
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Rozmiar pobieranego pliku
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// Ilość już pobranych bajtów
        /// </summary>
        public long BytesDownloaded { get; }

        /// <summary>
        /// Procent realizacji pobierania pojedyńczego pliku
        /// </summary>
        public int PercentCompleted { get; }
    }
}
