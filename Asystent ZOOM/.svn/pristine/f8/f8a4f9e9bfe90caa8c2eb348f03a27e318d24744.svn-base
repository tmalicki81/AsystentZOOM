namespace FileService.FileRepositories
{
    public class AppIsoStorFileRepository : BaseIsoStorFileRepository
    {
        public override string RootDirectory => string.Empty;
        public override bool PullOnlyNewerFiles => true;
        public override bool CreateBackupBeforePullFile => false;
        public override bool VerifyCheckSumBeforePullFile => true;
        public override string Description => "Pamięć izolowana dla binariów aplikacji";
        public override bool PullOnlyWhereNotFound => false;
    }
}
