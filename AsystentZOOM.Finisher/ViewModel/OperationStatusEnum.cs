namespace AsystentZOOM.Finisher.ViewModel
{
    /// <summary>
    /// Status zadania
    /// </summary>
    public enum OperationStatusEnum
    {
        /// <summary>
        /// Zakolejkowano
        /// </summary>
        Queued,

        /// <summary>
        /// Zadanie w trakcie
        /// </summary>
        InProgress,

        /// <summary>
        /// Zadanie zakończone
        /// </summary>
        Finished,

        /// <summary>
        /// Zadanie błędne
        /// </summary>
        Error
    }
}
