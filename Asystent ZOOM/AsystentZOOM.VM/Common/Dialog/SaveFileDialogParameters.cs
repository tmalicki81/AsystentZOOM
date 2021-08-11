namespace AsystentZOOM.VM.Common.Dialog
{
    /// <summary>
    /// Parametry okna do zapisywania pliku
    /// </summary>
    public class SaveFileDialogParameters
    {
        /// <summary>
        /// Tytuł okna
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Filtr pliku
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Wybrane pliki (jeśli wiele)
        /// </summary>
        public string[] FileNames { get; set; }

        /// <summary>
        /// Wybrany plik do zapisu
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Czy dodawać rozszerzenie pliku
        /// </summary>
        public bool AddExtension { get; set; }

        /// <summary>
        /// Domyślne rozszerzeni pliku
        /// </summary>
        public string DefaultExt { get; set; }

        /// <summary>
        /// Katalog, w którym zapisywane są pliki
        /// </summary>
        public string InitialDirectory { get; set; }

        /// <summary>
        /// Czy użytkownik wskazał plik do zapisu
        /// </summary>
        public bool? Result { get; set; }
    }
}
