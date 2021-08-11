using FileService.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileService.FileRepositories
{
    /// <summary>
    /// Repozytorium plików na dysku lokalnym (ew. sieciowym)
    /// </summary>
    public abstract class BaseLocalFileRepository : BaseFileRepository
    {
        /// <summary>
        /// Lokalizacja katalogu, który dla repozytorium plikowego traktowany jest jako główny
        /// Np. w katalogu C:\Pliki\Repozytorium_1\Dokumenty znajduje się repozytorium "Dokumenty"
        /// Katalog "Dokumenty" jest głównym katalogiem dla repozytorium
        /// </summary>
        public abstract string RootDirectory { get; }

        /// <summary>
        /// Pełna nazwa pliku na podstawie lokalizacji względem głównego katalogu repozytorium
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        private string GetFullFileName(string[] filePath)
            => Path.Combine(RootDirectory, Path.Combine(filePath));

        /// <summary>
        /// Pobranie pliku z repozytorium plików
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Strumień pliku z repozytorium plików</returns>
        public override Stream LoadFile(params string[] filePath)
        { 
            string fullFileName = GetFullFileName(filePath);
            return File.OpenRead(fullFileName);
        }

        /// <summary>
        /// Lista plików wraz z ich metadanymi
        /// Jeśli w danym katalogu występują inne katalogi, lista taka otrzyma postać
        ///     plikPierwszy
        ///     katalogPierwszy/plikDrugi
        /// </summary>
        /// <param name="subDir">Katalog, wględem którego tworzyć listę</param>
        /// <param name="searchOption">Czy ignorować informacje o plikach znajdujących się w podrzednych katalogach</param>
        /// <returns>Lista plików wraz z ich metadanymi</returns>
        public override List<FileMetadata> GetFileList(string[] subDir, SearchOption searchOption)
        {
            return Directory
                .GetFiles(Path.Combine(RootDirectory, Path.Combine(subDir ?? new string[] { })), "*.*", searchOption)
                .Select(filePath => new FileMetadata
                {
                    FilePath = PathHelper.GetFilePath(filePath.Replace(RootDirectory + "\\", null), '\\'),
                    DateMod = File.GetLastWriteTime(filePath)
                })
                .ToList();
        }

        /// <summary>
        /// Pobranie metadanych pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Metadane pliku</returns>
        public override FileMetadata GetFileMetadata(params string[] filePath)
        {
            string filePathAsString = Path.Combine(RootDirectory, Path.Combine(filePath));
            if (!File.Exists(filePathAsString))
                return null;

            return new FileMetadata 
            {
                FilePath = filePath, 
                DateMod = File.GetLastWriteTime(filePathAsString), 
                Size = new FileInfo(filePathAsString).Length
            };
        }

        /// <summary>
        /// Czy istnieje plik
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public override bool Exists(params string[] filePath)
        {
            string filePathAsString = Path.Combine(RootDirectory, Path.Combine(filePath));
            return File.Exists(filePathAsString);
        }

        /// <summary>
        /// usunięcie pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public override bool Delete(params string[] filePath)
        {
            string filePathAsString = Path.Combine(RootDirectory, Path.Combine(filePath));
            bool exists = File.Exists(filePathAsString);
            if(exists)
                File.Delete(filePathAsString);
            return exists;
        }

        /// <summary>
        /// Zapisanie pliku z repozytorium plików
        /// </summary>
        /// <param name="fileStream">Strumień zapisywanego pliku</param>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        public override void SaveFile(Stream fileStream, params string[] fileName)
        {
            string fullFileName = GetFullFileName(fileName);
            var fi = new FileInfo(fullFileName);
            if (!fi.Directory.Exists)
                Directory.CreateDirectory(fi.Directory.FullName);
            
            byte[] fileBytes = StreamHelper.ToByteArray(fileStream);
            File.WriteAllBytes(fullFileName, fileBytes);
        }
    }
}
