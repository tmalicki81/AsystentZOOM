using System;
using System.Linq;

namespace FileService
{
    /// <summary>
    /// Metadane pliku
    /// </summary>
    public class FileMetadata
    {
        /// <summary>
        /// Lokalizacja pliku (względem głównego katalogu repozytorium)
        /// </summary>
        public string[] FilePath { get; set; }

        /// <summary>
        /// Data modyfikacji pliku
        /// </summary>
        public DateTime DateMod { get; set; }

        /// <summary>
        /// Rozmiar pliku podawany w bajtach
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Lokalizacja pliku (względem głównego katalogu repozytorium) reprezentowana przez ciąg znaków
        /// </summary>
        public string PathString
            => FilePath.Aggregate((a, b) => a + "/" + b);

        public override string ToString()
            => PathString;
    }
}
