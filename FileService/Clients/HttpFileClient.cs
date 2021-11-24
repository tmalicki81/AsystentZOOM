using FileService.Common;
using FileService.EventArgs;
using FileService.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FileService.Clients
{
    /// <summary>
    /// Klient HTTP do pobierania plików
    /// </summary>
    public class HttpFileClient
    {
        /// <summary>
        /// Pobieranie pliku z serwera (pobieranie bajtów)
        /// </summary>
        public event EventHandler<LoadingFileEventArgs> OnDownloadingFile;

        /// <summary>
        /// Pobranie rozmiaru pliku (wyrażonego w bajtach)
        /// </summary>
        /// <param name="webAddress">Adres pliku</param>
        /// <returns>Rozmiar pliku</returns>
        public long GetFileSize(string webAddress)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(webAddress);
            try
            {
                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    return webResponse.ContentLength;
                }
            }
            catch (WebException ex) when (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
            {
                throw new FileRepositoryException(FileRepositoryExceptionCodeEnum.FileNotFound, $"Nie znaleziono pliku", ex);
            }
        }

        /// <summary>
        /// Pobranie pliku jako tablicy bajtów
        /// </summary>
        /// <param name="webAddress">Adres pliku</param>
        /// <returns>Tablica bajtów</returns>
        public byte[] DownloadFile(string webAddress)
        {
            // Lista bajtów (później przekonwertowana do tablicy)
            var downloadedFileBytes = new List<byte>();
            // Krótka nazwa pliku (taka jaka będzie zapisana w repozytorium plikowym)
            string shortFileName = PathHelper.GetShortFileName(webAddress, '/');

            // Wykona żądanie
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(webAddress);            
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                // Rozmiar pliku
                long fileBytesLength = webResponse.ContentLength;

                // Pobierz strumień z serwera WWW
                using (var stream = webResponse.GetResponseStream())
                {
                    // Pobranych bajtów
                    int bytesDownloaded = 0;

                    // Ostatnio zarejestrowany postęp pobierania pliku
                    int prevPercent = 0;

                    // Wartość pobieranego bajtu
                    int byteValue;
                    while ((byteValue = stream.ReadByte()) != -1)
                    {
                        // Dołącz do listy zaczytanych bajtów
                        downloadedFileBytes.Add((byte)byteValue);

                        // Jeśli istnieje subskrybent zdarzenia
                        if (OnDownloadingFile != null)
                        {
                            // Pobierz informację o postępie
                            var e = new LoadingFileEventArgs(shortFileName, fileBytesLength, bytesDownloaded++);

                            // Jeśli początek lub koniec pobierania lub skok o 1%
                            if (e.PercentCompleted > prevPercent + 1 ||
                                bytesDownloaded == 1 ||
                                bytesDownloaded == fileBytesLength)
                            {
                                OnDownloadingFile(this, e);
                                prevPercent = e.PercentCompleted;
                            }
                        }
                    }
                    // Zwróć tablice gotową bajtów pobranego pliku
                    return downloadedFileBytes.ToArray();
                }
            }
        }
    }
}
