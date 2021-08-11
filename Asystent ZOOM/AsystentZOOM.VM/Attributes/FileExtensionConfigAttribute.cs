using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.Model;
using System;

namespace AsystentZOOM.VM.Attributes
{
    /// <summary>
    /// Atrybut wiążący rozszerzenie pliku z
    ///     - klasą reprezentującą plik (np. plik graficzny, plik wideo)
    ///     - repozytorium plikowym na dysku lokalnym (ew. sieciowym)
    ///     - repozytorium plikowym na serwerze FTP
    /// </summary>
    public class FileExtensionConfigAttribute : Attribute
    {
        public FileExtensionConfigAttribute(
            string extension,
            Type baseMediaFileInfoType,
            Type mediaLocalFileRepository,
            Type mediaFtpFileRepository)
        {
            Extension = extension;
            FileExtension = (FileExtensionEnum)Enum.Parse(typeof(FileExtensionEnum), extension);
            MediaLocalFileRepository = (BaseMediaLocalFileRepository)Activator.CreateInstance(mediaLocalFileRepository);
            MediaFtpFileRepository = (BaseMediaFtpFileRepository)Activator.CreateInstance(mediaFtpFileRepository);
            BaseMediaFileInfoType = baseMediaFileInfoType;
        }

        /// <summary>
        /// Rozszerzenie pliku (wielkimi literami)
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Enum dla rozszerzenia pliku
        /// </summary>
        public FileExtensionEnum FileExtension { get; }

        /// <summary>
        /// Klasa reprezentująca plik multimedialny
        /// </summary>
        public Type BaseMediaFileInfoType { get; }

        /// <summary>
        /// Repozytorium plikowe na dysku lokalnym, w którym przechowywane są pliki określonego typu
        /// </summary>
        public BaseMediaLocalFileRepository MediaLocalFileRepository { get; }

        /// <summary>
        /// Repozytorium plikowe na serwerze FTP, w którym przechowywane są pliki określonego typu
        /// </summary>
        public BaseMediaFtpFileRepository MediaFtpFileRepository { get; }        
    }
}
