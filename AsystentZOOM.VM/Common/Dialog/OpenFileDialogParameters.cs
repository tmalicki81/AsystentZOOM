namespace AsystentZOOM.VM.Common.Dialog
{
    /// <summary>
    /// Parametry okna otwierania pliku
    /// </summary>
    public class OpenFileDialogParameters
    {
        /// <summary>
        /// Multiwybór
        /// </summary>
        public bool Multiselect { get; set; }

        /// <summary>
        /// Tytuł okna
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Filtr
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Nazwy otwieranych plików (jesli wiele)
        /// </summary>
        public string[] FileNames { get; set; }

        /// <summary>
        /// Nazwa otwieranego pliku
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Czy dodawać rozszerzenie pliku
        /// </summary>
        public bool AddExtension { get; set; }

        /// <summary>
        /// Domyślne rozszerzenie pliku
        /// </summary>
        public string DefaultExt { get; set; }

        /// <summary>
        /// Katalog, od którego szukac pliku do otworzenia
        /// </summary>
        public string InitialDirectory { get; set; }

        /// <summary>
        /// Czy użytkownik wybrał plik do otworzenia
        /// </summary>
        public bool? Result { get; set; }
    }
}
