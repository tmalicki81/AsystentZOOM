using FileService.Clients;
using FileService.Common;
using FileService.EventArgs;
using FileService.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FileService.FileRepositories
{
    public abstract class BaseFtpFileRepository : BaseFileRepository
    {
        /// <summary>
        /// Klient FTP
        /// </summary>
        private readonly FtpClient _ftpClient;

        /// <summary>
        /// Podstawowe informacji o sesji połączenia z serwerem FTP
        /// </summary>
        public abstract FtpSessionInfo SessionInfo { get; }

        public BaseFtpFileRepository() 
        {
            _ftpClient = new FtpClient(SessionInfo);
            _ftpClient.OnUploadingFile += (s, e) => CallOnSavingFile(e);
            _ftpClient.OnDownloadingFile += (s, e) => CallOnLoadingFile(e);
        }

        private string GetPathAsString(string[] filePath)
            => filePath?.Any() == true 
                ? filePath.Aggregate((a, b) => a + "/" + b) 
                : string.Empty;

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
            => _ftpClient.GetListDirectoryDetails(GetPathAsString(subDir), searchOption);

        /// <summary>
        /// Pobranie pliku z repozytorium plików
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Strumień pliku z repozytorium plików</returns>
        public override Stream LoadFile(params string[] filePath)
        {
            string fullFileName = null;
            try
            {
                fullFileName = GetPathAsString(filePath);
                byte[] fileBytes = _ftpClient.DownloadFile(fullFileName);
                return new MemoryStream(fileBytes);
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse response)
            {
                var errorMessage = new StringBuilder()
                    .Append("Błąd pobierania pliku ")
                    .Append($"{SessionInfo.HostName}/")
                    .Append(!string.IsNullOrEmpty(SessionInfo.RemoteDirectory) ? $"{SessionInfo.RemoteDirectory}/" : null)
                    .Append(fullFileName)
                    .ToString();
                switch (response.StatusCode)
                {
                    case FtpStatusCode.ActionNotTakenFileUnavailable:
                        throw new FileRepositoryException(FileRepositoryExceptionCodeEnum.FileNotFound, errorMessage, ex);
                }
                throw new FileRepositoryException(FileRepositoryExceptionCodeEnum.Unknown, errorMessage, ex);
            }
        }

        /// <summary>
        /// Zapisanie pliku z repozytorium plików
        /// Jeśli nie znaleziono danego katalogu => swórz go
        /// </summary>
        /// <param name="fileStream">Strumień zapisywanego pliku</param>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        public override void SaveFile(Stream fileStream, params string[] filePath)
        {
            string fullFileName = GetPathAsString(filePath);
            int status;
            try
            {
                status = (int)_ftpClient.UploadFile(fullFileName, fileStream);
            }
            catch (WebException ex) when (((FtpWebResponse)ex.Response).StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                string directoryName = PathHelper.GetDirectoryFromFileName(fullFileName, '/', out _);
                _ftpClient.MakeDirectory(directoryName);
                status = (int)_ftpClient.UploadFile(fullFileName, fileStream);
            }
            if (status >= 400 && status < 500)
                throw new Exception($"Status: {status} - Nie udało się zapisać pliku {fullFileName}");
        }

        /// <summary>
        /// Pobranie metadanych pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Metadane pliku</returns>
        public override FileMetadata GetFileMetadata(params string[] filePath)
        {
            try
            {
                return new FileMetadata
                {
                    FilePath = filePath,
                    DateMod = _ftpClient.GetDateTimeStamp(filePath),
                    Size = _ftpClient.GetFileSize(filePath)
                };
            }
            catch (WebException ex) when (((FtpWebResponse)ex.Response).StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                return null;
            }
        }

        /// <summary>
        /// Czy istnieje plik
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public override bool Exists(params string[] filePath)
            => _ftpClient.Exists(filePath);

        /// <summary>
        /// Usunięcie pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public override bool Delete(params string[] filePath)
            => _ftpClient.Delete(filePath);

        ~BaseFtpFileRepository() 
        {
            try
            {
                _ftpClient.OnUploadingFile -= (s, e) => CallOnSavingFile(e);
                _ftpClient.OnDownloadingFile -= (s, e) => CallOnLoadingFile(e);
            }
            catch { }
        }
    }
}
