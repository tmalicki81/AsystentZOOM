using System;

namespace FileService.Exceptions
{
    /// <summary>
    /// Wyjątek błędu, który pojawił się przy wykonywaniu metod z danego repozytorium plikowego
    /// </summary>
    public class FileRepositoryException : Exception
    {
        public FileRepositoryException(FileRepositoryExceptionCodeEnum exceptionCode, string message, Exception innerException) 
            : base(message, innerException)
            => ExceptionCode = exceptionCode;

        /// <summary>
        /// Kod błędu
        /// </summary>
        public FileRepositoryExceptionCodeEnum ExceptionCode { get; }
    }
}
