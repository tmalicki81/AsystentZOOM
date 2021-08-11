namespace FileService.Clients
{
    /// <summary>
    /// Podstawowe informacji o sesji połączenia z serwerem FTP
    /// </summary>
    public class FtpSessionInfo
    {
        /// <summary>
        /// Nazwa maszyny lub jej IP (w tym numer portu, jeśli konieczny)
        ///     - Maszyna
        ///     - Maszyna:NrPortu
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Nazwa użytkownika
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Hasło
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Katalog zdalny
        /// </summary>
        public string RemoteDirectory { get; set; }
    }
}
