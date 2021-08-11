using BinaryHelper.FileRepositories;
using FileService.Exceptions;
using FileService.FileRepositories;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BinaryHelper
{
    /// <summary>
    /// Rozszerzenie Klasy opisującej konkretny build na maszynie lokalnej
    /// </summary>
    public static class BuildVersionHelper
    {
        /// <summary>
        /// Inkrementacja buildu, czyli modyfikacja zaczytanego pliku
        /// </summary>
        /// <param name="buildVersion">Obiekt, na którym przeprowadzana jest operacja</param>
        private static void Increment(this BuildVersion buildVersion)
        {
            buildVersion.BuildDate = DateTime.Now;
            buildVersion.MachineName = Environment.MachineName;
            buildVersion.UserName = Environment.UserName;
            buildVersion.Version++;
        }

        /// <summary>
        /// Zapisanie pliku z ostatnio wykonanym buildem w repozytorium plikowym
        /// </summary>
        /// <param name="buildVersion">Obiekt, na którym przeprowadzana jest operacja</param>
        /// <param name="fileRepository">Repozytorium plikowe</param>
        /// <param name="filePath">Lokalizacja pliku w repozytorium</param>
        public static void Save(this BuildVersion buildVersion, BaseFileRepository fileRepository, params string[] filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(BuildVersion));
            using (var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            }))
            {
                xmlSerializer.Serialize(writer, buildVersion);
                fileRepository.SaveFile(stream, filePath);
            }
        }

        public const string LocalBuildInfoDirectory = "BuildInfo";
        public const string BuildVersionFile = "BuildVersion.xml";

        /// <summary>
        /// Inkrementacja buildu
        /// </summary>
        /// <param name="mustSendToServer">Czy nalezy wysłać binaria na serwer</param>
        public static void Increment(out bool mustSendToServer)
        {
            // Pobierz plik buildu z repozytorium kodu źródłowego (tutaj: katalog z projektem VisualStudio)
            var buildVersion = BuildVersion.Load(new LocalCodeFileRepository(), LocalBuildInfoDirectory, BuildVersionFile);

            try
            {
                // Pobierz plik buildu z serwera FTP
                var buildVersionFromFtp = BuildVersion.Load(new FtpBinFileRepository(), BuildVersionFile);

                // Jeśli nie zgadzają się wersje => wyślesz binaria na serwer
                mustSendToServer = buildVersion.AppVersion != buildVersionFromFtp.AppVersion;
            }
            catch (FileRepositoryException)
            {
                // Jeśli nie udało się pobrać pliku buildu z FTP => wyślesz binaria na serwer
                mustSendToServer = true;
            }

            // Inkrementuj build
            buildVersion.Increment();

            // Zapisz zmieniony plik buildu w kodzie
            buildVersion.Save(new LocalCodeFileRepository(), LocalBuildInfoDirectory, BuildVersionFile);

            // Zapisz zmieniony plik buildu w binariach (katalog bin\Debug|Release)
            buildVersion.Save(new LocalBinFileRepository(), BuildVersionFile);

            // Jeśli jest nowa wersja aplikacji => zapisz zmieniony plik buildu na FTP (a potem wyślij binaria)
            if (mustSendToServer)
                buildVersion.Save(new FtpBinFileRepository(), BuildVersionFile);
        }
    }
}
