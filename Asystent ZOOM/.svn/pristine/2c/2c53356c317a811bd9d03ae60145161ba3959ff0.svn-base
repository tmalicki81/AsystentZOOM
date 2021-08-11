using FileService.FileRepositories;
using System;
using System.IO;

namespace AsystentZOOM.VM.FileRepositories
{
    public abstract class BaseMediaLocalFileRepository : BaseLocalFileRepository
    {
        public abstract Environment.SpecialFolder DestinationInLocal { get; }

        public override string RootDirectory 
            => Path.Combine(Environment.GetFolderPath(DestinationInLocal), "Asystent ZOOM");

        public override bool PullOnlyNewerFiles => true;
        public override bool CreateBackupBeforePullFile => false;
        public override bool VerifyCheckSumBeforePullFile => false;
        public override bool PullOnlyWhereNotFound => false;
    }
}
