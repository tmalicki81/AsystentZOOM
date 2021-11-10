using FileService.Common;
using FileService.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using static FileService.Common.SyncData;
using static FileService.Common.SyncData.FileSync;

namespace FileService.FileRepositories
{
    public abstract class BaseFileRepository
    {
        /// <summary>
        /// Zdarzenie wypchnięcia pliku do docelowego repozytorium
        /// </summary>
        public event EventHandler<PushedFileEventArgs> OnPushedFile;

        /// <summary>
        /// Zdarzenie zapisywania pliku (poszczególnych bajtów)
        /// </summary>
        public event EventHandler<SavingFileEventArgs> OnSavingFile;

        /// <summary>
        /// Zdarzenie pobierania pliku (poszczególnych bajtów)
        /// </summary>
        public event EventHandler<LoadingFileEventArgs> OnLoadingFile;

        /// <summary>
        /// Zdarzenie błędu dla pojedynczego pliku.
        /// Jeśli zdarzenie jest niepodpięte => wywołanie błędu
        /// W przeciwnym wypadku => powiadomienie subskrybenta
        /// </summary>
        public event EventHandler<FileExceptionEventArgs> OnFileException;

        /// <summary>
        /// Wywołanie zdarzenia zapisywania pliku (metoda służy do przekazywania jednego zdarzenia do drugiego)
        /// </summary>
        /// <param name="e">Szczegóły zapisywanego pliku</param>
        internal void CallOnSavingFile(SavingFileEventArgs e)
            => OnSavingFile?.Invoke(this, e);

        /// <summary>
        /// Wywołanie zdarzenia pobierania pliku (metoda służy do przekazywania jednego zdarzenia do drugiego)
        /// </summary>
        /// <param name="e">Szczegóły pobieranego pliku</param>
        internal void CallOnLoadingFile(LoadingFileEventArgs e)
            => OnLoadingFile?.Invoke(this, e);

        /// <summary>
        /// Czy pobierać z repozytorium tylko wtedy gdy nie ma takiego pliku
        /// </summary>
        public abstract bool PullOnlyWhereNotFound { get; }

        /// <summary>
        /// Czy pobierać z repozytorium tylko nowsze pliki
        /// </summary>
        public abstract bool PullOnlyNewerFiles { get; }

        /// <summary>
        /// Czy tworzyć kopię zapasową przed zapisem pliku z innego repozytorium
        /// </summary>
        public abstract bool CreateBackupBeforePullFile { get; }

        /// <summary>
        /// Czy sprawdzać sumę kontrolną (skrót strumienia) pliku przed jego pobraniem z innego repozytorium
        /// </summary>
        public abstract bool VerifyCheckSumBeforePullFile { get; }
        
        /// <summary>
        /// Opis repozytorium
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Dopuszczalne rozszerzenia plików.
        /// Brak wartości = wszystkie rozszerzenia
        /// </summary>
        public virtual string[] FileExtensions { get; }

        public override string ToString()
            => Description;

        /// <summary>
        /// Pobranie pliku z repozytorium plików
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Strumień pliku z repozytorium plików</returns>
        public abstract Stream LoadFile(params string[] filePath);

        /// <summary>
        /// Zapisanie pliku z repozytorium plików
        /// </summary>
        /// <param name="fileStream">Strumień zapisywanego pliku</param>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        public abstract void SaveFile(Stream fileStream, params string[] filePath);

        /// <summary>
        /// Lista plików wraz z ich metadanymi
        /// Jeśli w danym katalogu występują inne katalogi, lista taka otrzyma postać
        ///     plikPierwszy
        ///     katalogPierwszy/plikDrugi
        /// </summary>
        /// <param name="subDir">Katalog, wględem którego tworzyć listę</param>
        /// <param name="searchOption">Czy ignorować informacje o plikach znajdujących się w podrzednych katalogach</param>
        /// <returns>Lista plików wraz z ich metadanymi</returns>
        public abstract List<FileMetadata> GetFileList(string[] subDir, SearchOption searchOption);

        /// <summary>
        /// Pobranie metadanych pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns>Metadane pliku</returns>
        public abstract FileMetadata GetFileMetadata(params string[] filePath);

        /// <summary>
        /// Czy istnieje plik
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public abstract bool Exists(params string[] filePath);

        /// <summary>
        /// Usunięcie pliku
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku względem głównego katalogu repozytorium</param>
        /// <returns></returns>
        public abstract bool Delete(params string[] filePath);

        /// <summary>
        /// Pobranie skrótu strumienia pliku
        /// </summary>
        /// <param name="hashingAlgoritmType">Algorytm</param>
        /// <param name="filePath">Lokalizacja pliku (względem głównego katalogu repozytorium)</param>
        /// <returns>Skrót strumienia pliku</returns>
        private string GetChecksum(HashingAlgoritmTypes hashingAlgoritmType, params string[] filePath)
        {
            using (var hasher = HashAlgorithm.Create(Enum.GetName(typeof(HashingAlgoritmTypes), hashingAlgoritmType)))
            using (Stream fileStream = LoadFile(filePath))
            using (MemoryStream memoryStream = StreamHelper.GetMemoryStream(fileStream))
            {
                fileStream.Position = 0;
                var hash = hasher.ComputeHash(memoryStream);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        /// <summary>
        /// Synchronizacja całego repozytorium z innym
        ///     - Wrzucenie własnych zmian
        ///     - Pobranie zmian z innego repozytorium
        /// </summary>
        /// <param name="targetFileRepository">Inne repozytorium (docelowe)</param>
        /// <param name="filePath">Konkretny plik do synchronizacji</param>
        public void Synchronize(BaseFileRepository targetFileRepository, string[] filePath)
        {
            var pushedFiles = new List<string[]>();
            this.PushToTarget(targetFileRepository, filePath, ref pushedFiles);
            targetFileRepository.PushToTarget(this, filePath, ref pushedFiles);
        }

        /// <summary>
        /// Wrzucenie zmian do innego repozytorium
        /// </summary>
        /// <param name="targetFileRepository">Inne repozytorium (docelowe)</param>
        /// /// <param name="filePath">Konkretny plik do synchronizacji</param>
        public void PushToTarget(BaseFileRepository targetFileRepository, string[] filePath)
        {
            var pushedFiles = new List<string[]>();
            PushToTarget(targetFileRepository, filePath, ref pushedFiles);
        }
      
        /// <summary>
        /// Pobranie lokalizacji (względem głównego katalogu repozytorium) jako ścieżkę
        /// </summary>
        /// <param name="filePath">Lokalizacja pliku</param>
        /// <returns></returns>
        private string GetPathString(string[] filePath)
            => filePath.Aggregate((a, b) => a + "/" + b);

        /// <summary>
        /// Czy istnieje różnica w skrótach dwóch plików
        /// </summary>
        /// <param name="sourceFileDetails">Metadane pliku w repozytorium źródłowym</param>
        /// <param name="targetFileDetails">Metadane pliku w repozytorium docelowym</param>
        /// <param name="targetFileRepository">Repozytorium docelowe</param>
        /// <returns></returns>
        private bool DifferentFileChecksums(FileMetadata sourceFileDetails, FileMetadata targetFileDetails, BaseFileRepository targetFileRepository)
        {
            string sourceFileChecksum = GetChecksum(HashingAlgoritmTypes.MD5, sourceFileDetails.FilePath);
            string targetFileChecksum = targetFileRepository.GetChecksum(HashingAlgoritmTypes.MD5, targetFileDetails.FilePath);
            return sourceFileChecksum != targetFileChecksum;
        }

        /// <summary>
        /// Sprawdzenie czy należy umieścić plik na innym repozytorium (docelowym)
        /// </summary>
        /// <param name="sourceFileDetails">Metadane pliku w repozytorium źródłowym</param>
        /// <param name="targetFileDetails">Metadane pliku w repozytorium docelowym</param>
        /// <param name="targetFileRepository">Repozytorium docelowe</param>
        /// <returns></returns>
        private bool MustPushFile(FileMetadata sourceFileDetails, FileMetadata targetFileDetails, BaseFileRepository targetFileRepository)
        {
            // Jeśli nie ma pliku w miejscu docelowym
            if (targetFileDetails == null)
            {
                return true;
            }
            // Jeśli pobierać tylko gdy brakuje pliku docelowego
            if (targetFileRepository.PullOnlyWhereNotFound)
            {
                return false;
            }
            // 
            if (targetFileRepository.PullOnlyNewerFiles)
            {
                if (sourceFileDetails.DateMod > targetFileDetails.DateMod)
                {
                    var syncData = GetSyncData();

                    // Pobierz ostatnią odwrotną synchronizację.
                    // Czyli jeśli teraz zapisujesz plik z lokalnego dysku na serwer FTP, to pobierz
                    // log ostatniego odwrotnego zapisu tego pliku (z FTP na dysk lokalny).
                    var oldFileSync = syncData.FileSyncList
                        .FirstOrDefault(f => f.FilePathAsString == sourceFileDetails.PathString &&
                                             f.Source.Repository == targetFileRepository.Description && // Odwrotne repozytoria
                                             f.Target.Repository == Description);                       // Odwrotne repozytoria
                    // Jeśli nie znaleziono logu => zapisuj plik z FTP na dysk lokalny
                    // Jeśli znaleziono log => sprawdź, czy plik na FTP jest nowszy niż data
                    // ostatniego pobrania pliku z FTP na dysk lokalny. Jeśli tak => zapisz taki plik
                    return
                        oldFileSync == null ||
                        oldFileSync != null && sourceFileDetails.DateMod > oldFileSync.Target.FileDateMod;
                }
                return false;
            }
            // Jeśli trzeba porównać sumy kontrolne plików => skopiuj tylo wtedy gdy jest różnica
            if (targetFileRepository.VerifyCheckSumBeforePullFile)
            {
                return DifferentFileChecksums(sourceFileDetails, targetFileDetails, targetFileRepository);
            }
            return true;
        }

        /// <summary>
        /// Pobranie nazwy pliku kopii zapasowej
        /// </summary>
        /// <returns></returns>
        private string GetBackupFileName() 
            => $"{DateTime.Now:yyyy-MM-dd__HH_mm_ss}__{PathHelper.NormalizeToFileName(Environment.MachineName)}__{PathHelper.NormalizeToFileName(Environment.UserName)}.backup";
        private const string BackupDirectory = ".backup";

        /// <summary>
        /// Skopiowanie pojedyńczego pliku do docelowego repozytorium
        /// </summary>
        /// <param name="targetFileRepository">Docelowe repozytorium plikowe</param>
        /// <param name="filePath">Lokalizacja pliku (względem katalogu głównego repozytorium)</param>
        /// <returns>Lokalizacja zapisanego pliku</returns>
        public string[] CopyTo(BaseFileRepository targetFileRepository, params string[] filePath)
        {
            // Lokalizacja docelowa pliku
            string[] destFilePath = filePath.ToArray();

            // Jeśli plik znajduje się już w docelowym repozytorium
            if (targetFileRepository.Exists(filePath))
            {
                // Jeśli repozytorium docelowe wymaga wykonania kopii zapasowej przed podmianą pliku => wykonaj kopię
                if (targetFileRepository.CreateBackupBeforePullFile)
                {
                    string backupFileName = GetBackupFileName();
                    using (Stream fileStream = targetFileRepository.LoadFile(filePath))
                    {
                        List<string> path = filePath.ToList();
                        path.Insert(0, BackupDirectory);
                        path.Add(backupFileName);
                        targetFileRepository.SaveFile(fileStream, path.ToArray());
                    }
                }
                // Jeśli repozytorium docelowe nie wymaga wykonania kopii zapasowej przed podmianą pliku => zapisz plik pod inną nazwą
                else
                {
                    destFilePath[destFilePath.Count() - 1] = $"{Guid.NewGuid()}__{destFilePath.Last()}";
                }
            }
            // Zapisz plik w repozytorium docelowym i zwróć (nową - jeśli ją zmieniono) lokalizację zapisanego pliku
            using (Stream fileStream = LoadFile(filePath))
            {
                targetFileRepository.SaveFile(fileStream, destFilePath);
                // Zapisz log o synchronizacji
                var fileMetadata = GetFileMetadata(filePath);
                SetFileSync(fileMetadata, targetFileRepository);
            }
            return destFilePath;
        }

        /// <summary>
        /// Zapisanie plików w docelowym repozytorium (różne mechanizmy w zależności od konfiguracji repozytorium)
        /// </summary>
        /// <param name="targetFileRepository">Docelowe repozytorium plikowe</param>
        /// <param name="filePath">Konkretny plik do synchronizacji</param>
        /// <param name="pushedFiles">Już zapisane pliku. Nie będą one uwzględniane podczas synchronizacji</param>
        private void PushToTarget(BaseFileRepository targetFileRepository, string[] filePath, ref List<string[]> pushedFiles)
        {
            // Pobierz listę plików repozytorium źródłowego
            List<FileMetadata> sourceFileList = filePath?.Any() == true
                ? // Jeśli podano konkretny plik
                     new FileMetadata[] { GetFileMetadata(filePath) }.ToList()
                : // Jeśli synchronizować całe repozytorium
                    GetFileList(subDir: new string[] { }, searchOption: SearchOption.AllDirectories)
                        .Where(f => f.FilePath.First() != BackupDirectory)
                        .ToList();
           
            // Wyklucz pliki, które już zsynchronizowano
            // oraz te, które mają nieobsługiwane rozszerzenie
            var tmpPushedFiles = pushedFiles;
            List<FileMetadata> filesToPush = sourceFileList
                .Where(sourceFile => sourceFile != null)
                .Where(sourceFile => // pomiń backup
                                     sourceFile.FilePath.FirstOrDefault() != BackupDirectory &&
                                     // pomiń pliki, które już zsynchronizowano
                                     !tmpPushedFiles.Any(pushedFile => sourceFile.PathString == GetPathString(pushedFile)))
                // Pobierz dodatkowo rozszerzenie pliku
                .Select(sourceFile => new
                {
                    File = sourceFile,
                    Extension = PathHelper.GetFileExtension(sourceFile.FilePath)
                })
                // Pomiń pliki, które mają nieobsługiwane rozszerzenie
                .Where(additionalFileInfo => 
                    FileExtensions?.Any() != true || 
                    FileExtensions.Any(extension => extension.ToUpper() == additionalFileInfo.Extension))
                // Zwróć tyko pliki (bez dodatkowej informacji o rozszerzeniu)
                .Select(additionalFileInfo => additionalFileInfo.File)
                .ToList();

            // Pobierz nazwę pliku kopii zapasowej
            string backupFileName = GetBackupFileName();

            // Zapisuj wszystkie pliki z repozytorium źródłowego
            int fileNumber = 0;
            foreach (var sourceFileDetails in filesToPush)
            {
                try
                {
                    // Pobierz metadane pliku docelowego
                    FileMetadata targetFileDetails = targetFileRepository.GetFileMetadata(sourceFileDetails.FilePath);

                    // Sprawdź czy na podstawie konfiguracji repozytorium docelowego należy zapisac w nim plik
                    bool mustPushFile = MustPushFile(sourceFileDetails, targetFileDetails, targetFileRepository);
                    bool isBackup = false;

                    // Jeśli należy zapisać plik w repozytorium docelowym => zapisz go, jeśli nie => pomiń
                    if (mustPushFile)
                    {
                        // Jeśli repozytorium docelowe wymaga wykonania kopii zapasowej przez podmianą pliku
                        // oraz plik istnieje w lokalizacji repozytorium docelowego
                        // => wykonaj kopie zapasową
                        if (targetFileRepository.CreateBackupBeforePullFile && targetFileDetails != null)
                        {
                            // Pobierz plik z repozytorium docelowego
                            // i skopiuj go do lokalizacji z backupem
                            using (Stream fileStream = targetFileRepository.LoadFile(targetFileDetails.FilePath))
                            {
                                List<string> path = sourceFileDetails.FilePath.ToList();
                                path.Insert(0, BackupDirectory);
                                path.Add(backupFileName);
                                targetFileRepository.SaveFile(fileStream, path.ToArray());
                            }
                            // Zapamiętaj, ze wykonano backup
                            isBackup = true;
                        }
                        // Zapisz plik z repozytorium źródłowego do repozytorium docelowego
                        using (Stream fileStream = LoadFile(sourceFileDetails.FilePath))
                        {
                            targetFileRepository.SaveFile(fileStream, sourceFileDetails.FilePath);

                            // Zapisz log o synchronizacji
                            SetFileSync(sourceFileDetails, targetFileRepository);
                        }
                        // Zapamiętaj, żeby w nie synchronizowac już tego pliku
                        pushedFiles.Add(sourceFileDetails.FilePath);
                    }
                    // Poinformuj subskrybentów o wykonanej operacji
                    OnPushedFile?.Invoke(this, new PushedFileEventArgs(
                        path: sourceFileDetails.FilePath,
                        fileNumber: ++fileNumber,
                        allFiles: filesToPush.Count(),
                        isBackup: isBackup,
                        copied: mustPushFile));
                }
                catch (Exception ex)
                {
                    if (OnFileException != null)
                        OnFileException(this, new FileExceptionEventArgs
                        {
                            FileName = sourceFileDetails.PathString,
                            Exception = ex
                        });
                    else
                        throw;
                }
            }
        }

        /// <summary>
        /// Zapisz log o synchronizacji plików
        /// </summary>
        /// <param name="sourceFileDetails">Metadane pliku w repozytorium źródłowym</param>
        /// <param name="targetFileRepository">Repozytorium docelowe</param>
        private void SetFileSync(FileMetadata sourceFileDetails, BaseFileRepository targetFileRepository)
        {
            // Nowy log
            //FileMetadata targetFileDetails = targetFileRepository.GetFileMetadata(sourceFileDetails.FilePath);

            var fileSync = new FileSync
            {
                FilePathAsString = sourceFileDetails.PathString,
                Source = new FileInRepository
                {
                    Repository = this.Description,
                    FileDateMod = sourceFileDetails.DateMod
                },
                Target = new FileInRepository
                {
                    Repository = targetFileRepository.Description,
                    FileDateMod = DateTime.Now  //targetFileDetails.DateMod
                }
            };
            // Pobierz ostatni log
            var syncData = GetSyncData();

            // Zmień czas logu
            syncData.LastSync = DateTime.Now;

            // Znajdź tożsamy log w celu zastąpienia go nowym
            var oldFileSync = syncData.FileSyncList
                .FirstOrDefault(f => f.FilePathAsString == fileSync.FilePathAsString &&
                                     f.Source.Repository == fileSync.Source.Repository &&
                                     f.Target.Repository == fileSync.Target.Repository);
            // Jeśl trzeba => usuń stary log
            if (oldFileSync != null)
                syncData.FileSyncList.Remove(oldFileSync);

            // Dodaj nowy log
            syncData.FileSyncList.Add(fileSync);

            // Zapisz zmiany
            SetSyncData(syncData);
        }

        /// <summary>
        /// Nazwa pliku z logiem synchronizacji
        /// </summary>
        private readonly static string SyncDataFilePath = $"{nameof(SyncData)}.xml";

        /// <summary>
        /// Zbuforowane dane o synchronizacji plików
        /// </summary>
        private static SyncData _syncData;

        /// <summary>
        /// Blokada dla zapisu/odczytu pliku synchronizacji plików
        /// </summary>
        private static readonly object _syncDataLocker = new object();

        /// <summary>
        /// Pobranie logu synchronizacji plików
        /// </summary>
        /// <returns>Log synchronizacji plików</returns>
        private static SyncData GetSyncData() 
        {
            lock (_syncDataLocker)
            {
                // Jeśli zbuforowano log => zwróć log z bufora
                if (_syncData != null)
                    return _syncData;

                // Jeśli nie zbuforowano logu => zwróć go z dysku
                var iso = new AppIsoStorFileRepository();
                try
                {
                    using (Stream isoStream = iso.LoadFile(SyncDataFilePath))
                    {
                        var serializer = new XmlSerializer(typeof(SyncData));
                        _syncData = (SyncData)serializer.Deserialize(isoStream);
                    }
                }
                catch
                {
                    // Jeśli nie udało się pobrac logu z dysku => wyczyść go
                    _syncData = new SyncData();
                }
                // Zwróć log z dysku lub w przypadku awarii - pusty
                return _syncData;
            }
        }

        /// <summary>
        /// Zapisz log synchronizacji plików
        /// </summary>
        /// <param name="value"></param>
        private static void SetSyncData(SyncData value)
        {
            lock (_syncDataLocker)
            {
                _syncData = value;
                var iso = new AppIsoStorFileRepository();
                var serializer = new XmlSerializer(typeof(SyncData));
                using (var m = new MemoryStream())
                {
                    serializer.Serialize(m, value);
                    iso.SaveFile(m, SyncDataFilePath);
                }
            }
        }
    }
}