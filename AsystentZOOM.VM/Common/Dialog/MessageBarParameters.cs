namespace AsystentZOOM.VM.Common.Dialog
{
    /// <summary>
    /// Parametry paska powiadomień
    /// </summary>
    public class MessageBarParameters
    {
        /// <summary>
        /// Treść powiadomienia
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Ważność
        /// </summary>
        public MessageBarLevelEnum MessageBarLevel { get; set; } = MessageBarLevelEnum.Information;
    }
}
