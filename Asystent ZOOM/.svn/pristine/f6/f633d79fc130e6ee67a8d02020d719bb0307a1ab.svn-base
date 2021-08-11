using System.Collections.Generic;
using System.Linq;

namespace FileService.Common
{
    public static class PathHelper
    {
        /// <summary>
        /// Ścieżka (względem głównego katalogu repozytorium) do katalogu na podstawie ścieżki do pliku
        /// </summary>
        /// <param name="filePath">Plik (względem głównego katalogu repozytorium)</param>
        /// <param name="fileInDirectory">Czy plik znajduje się w katalogu innym niż główny repozytorium</param>
        /// <returns>Ścieżka (względem głównego katalogu repozytorium) do katalogu na podstawie ścieżki do pliku</returns>
        public static string GetDirectoryFromFileName(string filePath, char separator, out bool fileInDirectory)
        {
            string[] filePathArray = GetFilePath(filePath, separator);
            int pathElements = filePathArray.Count();
            fileInDirectory = pathElements > 1;
            return fileInDirectory
                ? filePathArray.Take(pathElements - 1).Aggregate((a, b) => a + separator + b)
                : null;
        }

        public static string GetShortFileName(string filePath, char separator)
            => filePath?.Split(separator).LastOrDefault();

        public static string[] GetFilePath(string filePath, char separator) 
            => filePath
                .Split(separator)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

        public static string GetFilePathAsString(string[] filePath, char separator)
            => filePath?.Any() == true
                  ? filePath.Aggregate((a, b) => a + separator + b)
                  : null;

        public static string GetFileExtension(string filePath)
            => filePath != null
                  ? filePath.Split('.').Last().ToUpper()
                  : null;

        public static string GetFileExtension(string[] filePath)
            => filePath?.Any() == true
                  ? filePath.Last().Split('.').Last().ToUpper()
                  : null;

        /// <summary>
        /// Ścieżka (względem głównego katalogu repozytorium) do katalogu na podstawie ścieżki do pliku
        /// </summary>
        /// <param name="filePath">Plik (względem głównego katalogu repozytorium)</param>
        /// <returns></returns>
        public static string[] GetDirectoryPath(string[] filePath)
        {
            if (filePath.Count() <= 1)
                return new string[] { };
            List<string> tmpPath = filePath.ToList();
            tmpPath.RemoveAt(tmpPath.Count() - 1);
            return tmpPath.ToArray();
        }

        /// <summary>
        /// Normalizacja ciągu znaków aby mogła wchodzić w skład nazwy pliku
        /// </summary>
        /// <param name="text">Ciąg znaków</param>
        /// <returns></returns>
        public static string NormalizeToFileName(string text)
        {
            string result = text;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                result = result.Replace(c, '_');
            }
            return result;
        }
    }
}
