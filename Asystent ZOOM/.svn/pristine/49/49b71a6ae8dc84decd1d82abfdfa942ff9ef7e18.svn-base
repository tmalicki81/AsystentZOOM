using FileService.Common;
using FileService.EventArgs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace FileService.Clients
{
    /// <summary>
    /// Klient FTP
    /// </summary>
    public class FtpClient
    {
        /// <summary>
        /// Zapisywanie pliku na serwer (zapisywanie bajtów)
        /// </summary>
        public event EventHandler<SavingFileEventArgs> OnUploadingFile;

        /// <summary>
        /// Pobieranie pliku z serwera (pobieranie bajtów)
        /// </summary>
        public event EventHandler<LoadingFileEventArgs> OnDownloadingFile;

        // Informacje do logowania na serwer FTP
        private readonly FtpSessionInfo _ftpSessionInfo;

        public FtpClient(FtpSessionInfo ftpSessionInfo) 
            => _ftpSessionInfo = ftpSessionInfo;

        /// <summary>
        /// Pobieranie adresu URI na podstawie nazwy pliku
        /// </summary>
        /// <param name="filePath">Nazwa pliku (względem głównego katalogu repozytorium)</param>
        /// <returns></returns>
        internal string GetUriString(string filePath)
            => $"ftp://{_ftpSessionInfo.HostName}/{_ftpSessionInfo.RemoteDirectory}/{filePath}";

        /// <summary>
        /// Pobranie poświadczeń
        /// </summary>
        /// <returns>Poświadczenia</returns>
        private NetworkCredential GetNetworkCredential()
            => new NetworkCredential(_ftpSessionInfo.UserName, _ftpSessionInfo.Password);

        /// <summary>
        /// Żądanie FTP na podstawie nazwy metody i pliku (względem głównego katalogu repozytorium)
        /// </summary>
        /// <param name="method">Metoda żądania FTP</param>
        /// <param name="filePath">Plik lub katalog (względem głównego katalogu repozytorium)</param>
        /// <returns></returns>
        private FtpWebRequest GetFtpWebRequest(string method, string filePath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetUriString(filePath));
            request.Method = method;
            request.Credentials = GetNetworkCredential();
            return request;
        }

        /// <summary>
        /// Zapisanie pliku na serwer FTP
        /// </summary>
        /// <param name="filePath">Plik (względem głównego katalogu repozytorium)</param>
        /// <param name="stream">Strumień zapisywany na serwer FTP</param>
        /// <returns></returns>
        public FtpStatusCode UploadFile(string filePath, Stream stream)
            => UploadFile(filePath, StreamHelper.ToByteArray(stream));

        /// <summary>
        /// Utworzenie katalogu na serwerze FTP
        /// (względem głównego katalogu repozytorium)
        /// </summary>
        /// <param name="directoryPath">Katalog, który należy utworzyć</param>
        /// <returns></returns>
        public FtpStatusCode MakeDirectory(string directoryPath)
        {
            FtpWebRequest request = GetFtpWebRequest(WebRequestMethods.Ftp.MakeDirectory, directoryPath);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusCode;
            }
        }

        /// <summary>
        /// Zapisanie pliku na serwer FTP
        /// </summary>
        /// <param name="filePath">Ścieżka do zapisywanego pliku (względem głównego katalogu repozytorium)</param>
        /// <param name="fileBytes">Bajty pliku</param>
        /// <returns>Status odpowiedzi z serwera FTP</returns>
        public FtpStatusCode UploadFile(string filePath, byte[] fileBytes)
        {
            FtpWebRequest clsRequest = GetFtpWebRequest(WebRequestMethods.Ftp.UploadFile, filePath);
            clsRequest.ContentLength = fileBytes.Length;
            using (Stream clsStream = clsRequest.GetRequestStream())
            {
                int bytesUploaded = 0;
                int prevPercent = 0;
                int fileBytesLength = fileBytes.Length;

                // Zapisuj plik oraz powiadamiaj o postępie (co 1%)
                foreach (byte b in fileBytes)
                {
                    clsStream.WriteByte(b);
                    if (OnUploadingFile != null)
                    {
                        var e = new SavingFileEventArgs(filePath, fileBytesLength, bytesUploaded++);
                        if (e.PercentCompleted > prevPercent + 1 || 
                            bytesUploaded == 1 || 
                            bytesUploaded == fileBytesLength )
                        {
                            OnUploadingFile(this, e);
                            prevPercent = e.PercentCompleted;
                        }
                    }
                }
            }
            // Sprawdź czy operacja się powiodła i zwróć kod odpowiedzi
            using (var response = (FtpWebResponse)clsRequest.GetResponse())
            {
                return response.StatusCode;
            }
        }

        /// <summary>
        /// Czy istnieje plik lub katalog o podanej ścieżce
        /// </summary>
        /// <param name="filePath">Ścieżka do zapisywanego pliku (względem głównego katalogu repozytorium)</param>
        /// <returns></returns>
        public bool Exists(string[] filePath)
        {
            string filePathAsString = PathHelper.GetFilePathAsString(filePath, '/');
            FtpWebRequest ftpWebRequest = GetFtpWebRequest(WebRequestMethods.Ftp.GetFileSize, filePathAsString);
            try
            {
                using (var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse())
                {
                    return true;
                }
            }
            catch (WebException ex) when (((FtpWebResponse)ex.Response).StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                return false;
            }
        }

        /// <summary>
        /// Czy istnieje plik lub katalog o podanej ścieżce
        /// </summary>
        /// <param name="filePath">Ścieżka do zapisywanego pliku (względem głównego katalogu repozytorium)</param>
        /// <returns></returns>
        public bool Delete(string[] filePath)
        {
            string filePathAsString = PathHelper.GetFilePathAsString(filePath, '/');
            FtpWebRequest ftpWebRequest = GetFtpWebRequest(WebRequestMethods.Ftp.DeleteFile, filePathAsString);
            try
            {
                using (var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse())
                {
                    return true;
                }
            }
            catch (WebException ex) when (((FtpWebResponse)ex.Response).StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                return false;
            }
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
        public List<FileMetadata> GetListDirectoryDetails(string subDir = "", SearchOption searchOption = SearchOption.AllDirectories)
        {
            var list = new List<FileMetadata>();
            FillListDirectoryDetails(list, subDir, searchOption);
            return list;
        }

        /// <summary>
        /// Pobranie czasu modyfikacji pliku
        /// </summary>
        /// <param name="filePath">Ścieżka do zapisywanego pliku (względem głównego katalogu repozytorium)</param>
        /// <returns>Czas modyfikacji pliku</returns>
        public DateTime GetDateTimeStamp(string [] filePath)
        {
            string filePathAsString = PathHelper.GetFilePathAsString(filePath, '/');
            FtpWebRequest ftpWebRequest = GetFtpWebRequest(WebRequestMethods.Ftp.GetDateTimestamp, filePathAsString);
            using (var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse())
            {
                return ftpWebResponse.LastModified;
            }
        }

        /// <summary>
        /// Pobranie rozmiaru pliku
        /// </summary>
        /// <param name="filePath">Ścieżka do zapisywanego pliku (względem głównego katalogu repozytorium)</param>
        /// <returns>Rozmiar pliku</returns>
        public long GetFileSize(string[] filePath)
        {
            string filePathAsString = PathHelper.GetFilePathAsString(filePath, '/');
            FtpWebRequest ftpWebRequest = GetFtpWebRequest(WebRequestMethods.Ftp.GetFileSize, filePathAsString);
            using (var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse())
            {
                return ftpWebResponse.ContentLength;
            }
        }

        /// <summary>
        /// Uzupełnienie listy z metadanymi pliku.
        /// </summary>
        /// <param name="fileList">Lista plików</param>
        /// <param name="subDir">Katalog, wględem którego tworzyć listę</param>
        /// <param name="searchOption">Czy ignorować informacje o plikach znajdujących się w podrzednych katalogach</param>
        private void FillListDirectoryDetails(List<FileMetadata> fileList, string subDir, SearchOption searchOption)
        {
            // Stwórz listę na podstawie zwróconego z serwera strumienia pobranego metodą LIST
            FtpWebRequest clsRequest = GetFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, subDir);
            using (var response = (FtpWebResponse)clsRequest.GetResponse())
            using (var resStream = response.GetResponseStream())
            using (var r = new StreamReader(resStream))
            {
                string dirResponse = r.ReadToEnd();
                foreach (string line in dirResponse.Replace("\n", "").Split(Convert.ToChar('\r')))
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // Iteruj po każdej linii i pobieraj metadane na podstawie dopasowana z wyrażenia regularnego
                    Match m = GetMatchingRegex(line);
                    if (m == null)
                        throw new ApplicationException("Błąd parsowania linii: " + line);

                    string dir = m.Groups["dir"].Value;
                    string size = m.Groups["size"].Value;
                    string name = m.Groups["name"].Value;

                    if (name.EndsWith(".backup"))
                        continue;

                    //string timestamp = m.Groups["timestamp"].Value;
                    string filePath = $"{subDir}/{name}";

                    FtpWebRequest ccc = GetFtpWebRequest(WebRequestMethods.Ftp.GetDateTimestamp, filePath);
                    DateTime dateMod;
                    using (var ddd = (FtpWebResponse)ccc.GetResponse())
                    {
                        dateMod = ddd.LastModified;
                    }

                    // Jeśli nie podano roku, przyjmij rok bieżący
                    //if (!(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }.Any(x => x == timestamp[0])))
                    //    timestamp = $"{DateTime.Now.Year} {timestamp}";

                    if (dir[0] == 'd' && searchOption == SearchOption.AllDirectories)
                    {
                        // Jeśli katalog oraz oczekuje sie pobierania plików wgłąb => pobierz je rekurencyjnie
                        FillListDirectoryDetails(fileList, filePath, searchOption);
                    }
                    else
                    {
                        // Jeśli to plik => dodaj go do listy
                        fileList.Add(new FileMetadata
                        {
                            FilePath = PathHelper.GetFilePath(filePath, '/'),
                            DateMod = dateMod,//DateTime.Parse(timestamp, CultureInfo.InvariantCulture),
                            Size = int.Parse(size)
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Lista wyrażeń regularnych do pobierania metadanych pliku z serwera FTP
        /// </summary>
        private static readonly string[] _parseFormats = new string[] 
        {
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})(\\s+)(?<size>(\\d+))(\\s+)(?<ctbit>(\\w+\\s\\w+))(\\s+)(?<size2>(\\d+))\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{2}:\\d{2})\\s+(?<name>.+)",
            "(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)" 
        };

        /// <summary>
        /// Pobranie dopasowania z wyrażenia regularnego
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Match GetMatchingRegex(string line)
        {
            Regex rx;
            Match m;
            for (int i = 0; i <= _parseFormats.Length - 1; i++)
            {
                rx = new Regex(_parseFormats[i]);
                m = rx.Match(line);
                if (m.Success)
                    return m;
            }
            return null;
        }

        /// <summary>
        /// Rozmiar pliku w bajtach
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku (względem głównego katalogu repozytorium)</param>
        /// <returns>Rozmiar pliku w bajtach</returns>
        public long GetFileSize(string filePath)
        {
            FtpWebRequest ccc = GetFtpWebRequest(WebRequestMethods.Ftp.GetFileSize, filePath);
            using (var ddd = (FtpWebResponse)ccc.GetResponse())
            {
                return ddd.ContentLength;
            }
        }

        /// <summary>
        /// Pobranie pliku z serwera FTP
        /// </summary>
        /// <param name="filePath">Ścieżka do pobieranego pliku (względem głównego katalogu repozytorium)</param>
        /// <returns>Tablica bajtów reprezentująca plik</returns>
        public byte[] DownloadFile(string filePath)
        {
            long fileBytesLength = OnDownloadingFile != null ? GetFileSize(filePath) : 0;
            var downloadedFileBytes = new List<byte>();
            FtpWebRequest clsRequest = GetFtpWebRequest(WebRequestMethods.Ftp.DownloadFile, filePath);
            using (var response = (FtpWebResponse)clsRequest.GetResponse())
            using (var resStream = response.GetResponseStream())
            {
                int bytesDownloaded = 0;
                int prevPercent = 0;
                int b;
                // Pobierz plik i informuj na bieżąco o postępie (co 1%)
                while((b = resStream.ReadByte()) != -1)
                {
                    downloadedFileBytes.Add((byte)b);
                    if (OnDownloadingFile != null)
                    {
                        var e = new LoadingFileEventArgs(filePath, fileBytesLength, bytesDownloaded++);
                        if (e.PercentCompleted > prevPercent + 1 ||
                            bytesDownloaded == 1 ||
                            bytesDownloaded == fileBytesLength)
                        {
                            OnDownloadingFile(this, e);
                            prevPercent = e.PercentCompleted;
                        }
                    }
                }
                return downloadedFileBytes.ToArray();
            }
        }
    }
}
