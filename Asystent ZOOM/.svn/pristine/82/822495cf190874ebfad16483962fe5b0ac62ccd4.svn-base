namespace FileService.EventArgs
{
    /// <summary>
    /// Argument zdarzenia zapisu pojedyńczego pliku w repozytorium plikowym
    /// </summary>
    public class SavingFileEventArgs : System.EventArgs
    {
        public SavingFileEventArgs(string fileName, int fileSize, int bytesUploaded)
        {
            FileName = fileName;
            FileSize = fileSize;
            BytesUploaded = bytesUploaded;
            PercentCompleted = fileSize > 0 ? 100 * bytesUploaded / fileSize : 0;
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
        /// Ilość już wysłanych bajtów
        /// </summary>
        public long BytesUploaded { get; }

        /// <summary>
        /// Procent realizacji pobierania pojedyńczego pliku
        /// </summary>
        public int PercentCompleted { get; }
    }
}
