using FileService.FileRepositories;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace BinaryHelper
{
    /// <summary>
    /// Klasa opisująca konkretny build na maszynie lokalnej
    /// </summary>
    [XmlRoot("BuildInfo")]
    public class BuildVersion
    {
        /// <summary>
        /// Wersja aplikacji
        /// </summary>
        [XmlAttribute("app-version")]
        public string AppVersion { get; set; }

        /// <summary>
        /// Czas wykonania buildu
        /// </summary>
        [XmlAttribute("build-date")]
        public DateTime BuildDate { get; set; }

        /// <summary>
        /// Maszyna, z której wykonano build
        /// </summary>
        [XmlAttribute("machine-name")]
        public string MachineName { get; set; }

        /// <summary>
        /// Nazwa użytkownika wykonującego build
        /// </summary>
        [XmlAttribute("user-name")]
        public string UserName { get; set; }

        /// <summary>
        /// Numer kolejny buildu
        /// </summary>
        [XmlAttribute("build-version")]
        public int Version { get; set; }

        /// <summary>
        /// Pobierz plik z informacją o buildzie z danego repozytorium plikowego
        /// </summary>
        /// <param name="fileRepository"></param>
        /// <param name="filePath"></param>
        /// <returns>Reprezentacja pliku z informacją o buildzie z danego repozytorium plikowego</returns>
        public static BuildVersion Load(BaseFileRepository fileRepository, params string[] filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(BuildVersion));
            using (var stream = fileRepository.LoadFile(filePath))
            {
                return (BuildVersion)xmlSerializer.Deserialize(stream);
            }
        }
    }
}
