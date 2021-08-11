using System.Linq;

namespace FileService.EventArgs
{
    /// <summary>
    /// Argument zdarzenia zapisania pliku z jednego repozytorium plikowego do drugiego
    /// </summary>
    public class PushedFileEventArgs : System.EventArgs
    {
        public PushedFileEventArgs(string[] path, int fileNumber, int allFiles, bool isBackup, bool copied)
        {
            FileName = path.Aggregate((a, b) => a + "/" + b);
            PercentCompletted = allFiles == 0 ? 0 : (int)(100 * fileNumber / allFiles);
            IsBackup = isBackup;
            Copied = copied;
        }

        /// <summary>
        /// Procent realizacji
        /// </summary>
        public int PercentCompletted { get; }

        /// <summary>
        /// Nazwa pliku (względem katalogu głównego repozytorium)
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Czy wykonano kopię zapasową
        /// </summary>
        public bool IsBackup { get; }

        /// <summary>
        /// Czy skopiowano plik
        /// </summary>
        public bool Copied { get; }
    }
}
