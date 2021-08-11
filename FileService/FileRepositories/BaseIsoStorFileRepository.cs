using FileService.Common;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;

namespace FileService.FileRepositories
{
    /// <summary>
    /// Repozytorium plików w pamięci izolowanej
    /// </summary>
    public abstract class BaseIsoStorFileRepository : BaseFileRepository
    {
        /// <summary>
        /// Katalog w pamięci izolowanej - katalog główny repozytorium
        /// </summary>
        public abstract string RootDirectory { get; }

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
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                PropertyInfo pi = typeof(IsolatedStorageFile).GetProperty("RootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                string phisicalRootDirectory = Path.Combine((string)pi.GetValue(file), RootDirectory);
                string[] files = Directory.GetFiles(Path.Combine(phisicalRootDirectory, Path.Combine(subDir ?? new string[] { })), "*.*", searchOption);
                return files
                    .Select(x => new FileInfo(x))
                    .Select(x => new FileMetadata 
                    { 
                        DateMod = x.LastWriteTime, 
                        Size = (int)x.Length, 
                        FilePath = PathHelper.GetFilePath(x.FullName.Replace(phisicalRootDirectory, null), '\\')
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Lokalizacja pliku na dysku lokalnym uwzględniając lokalizację pamięci izolowanej i katalogu (głównego) w tej pamięci
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku (względem katalogu głównego repozytorium)</param>
        /// <returns></returns>
        private string GetIsoStorFilePath(params string[] filePath)
        {
            List<string> isoStorFilePath = filePath.ToList();
            isoStorFilePath.Insert(0, RootDirectory);
            return Path.Combine(isoStorFilePath.ToArray());
        }

        /// <summary>
        /// Pobranie pliku z repozytorium plików
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Strumień pliku z repozytorium plików</returns>
        public override Stream LoadFile(params string[] filePath)
        {
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            using (Stream stream = file.OpenFile(GetIsoStorFilePath(filePath), FileMode.Open, FileAccess.Read))
            {
                return StreamHelper.GetMemoryStream(stream);
            }
        }

        /// <summary>
        /// Zapisanie pliku z repozytorium plików
        /// </summary>
        /// <param name="fileStream">Strumień zapisywanego pliku</param>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        public override void SaveFile(Stream fileStream, params string[] filePath)
        {
            string isoStorFilePath = GetIsoStorFilePath(filePath);
            byte[] fileBytes = StreamHelper.ToByteArray(fileStream);

            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(isoStorFilePath))
                    file.DeleteFile(isoStorFilePath);

                string dir = PathHelper.GetDirectoryFromFileName(isoStorFilePath, '\\', out _);
                if (!string.IsNullOrEmpty(dir) && !file.DirectoryExists(dir))
                    file.CreateDirectory(dir);

                using (var stream = file.OpenFile(isoStorFilePath, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(fileBytes, 0, fileBytes.Length);
                }
            }
        }

        /// <summary>
        /// Pobranie metadanych pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Metadane pliku</returns>
        public override FileMetadata GetFileMetadata(params string[] filePath)
        {
            string phisicalFilePath = GetPhisicalFilePath(filePath);

            if (!File.Exists(phisicalFilePath))
                return null;

            return new FileMetadata
            {
                FilePath = filePath,
                DateMod = File.GetLastWriteTime(phisicalFilePath),
                Size = new FileInfo(phisicalFilePath).Length
            };
        }

        /// <summary>
        /// Pobranie fizycznej ścieżki do pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        private string GetPhisicalFilePath(string[] filePath)
        {
            string phisicalRootDirectory;
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                PropertyInfo pi = typeof(IsolatedStorageFile).GetProperty("RootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                phisicalRootDirectory = Path.Combine((string)pi.GetValue(file), RootDirectory);
            }
            return Path.Combine(phisicalRootDirectory, Path.Combine(filePath));
        }

        /// <summary>
        /// Czy istnieje plik
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public override bool Exists(params string[] filePath)
        {
            string phisicalFilePath = GetPhisicalFilePath(filePath);
            return File.Exists(phisicalFilePath);
        }

        /// <summary>
        /// Usunięcie pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public override bool Delete(params string[] filePath)
        {
            string phisicalFilePath = GetPhisicalFilePath(filePath);
            bool exists = File.Exists(phisicalFilePath);
            File.Delete(phisicalFilePath);
            return exists;
        }
    }
}
