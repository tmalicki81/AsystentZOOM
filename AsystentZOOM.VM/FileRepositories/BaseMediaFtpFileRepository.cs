using FileService.Clients;
using FileService.FileRepositories;
using System;

namespace AsystentZOOM.VM.FileRepositories
{
    public abstract class BaseMediaFtpFileRepository : BaseFtpFileRepository
    {
        private bool IsTmalicki
            => Environment.MachineName.Contains("MALICKI");

        public override FtpSessionInfo SessionInfo => new FtpSessionInfo
        {
            HostName = "av-labedy.pl",
            UserName = IsTmalicki 
                ? "tmalicki81-asystent-zoom-documents"
                : "tmalicki81-asystent-zoom-documents-2",
            Password = FileService.Common.CryptoHelper.DecryptString("992545eca56942ad", "BqdMmA9ox8m2XKGJNBXx0w=="),
            RemoteDirectory = RemoteDirectory
        };

        public override bool PullOnlyNewerFiles => true;
        public override bool CreateBackupBeforePullFile => false;
        public override bool VerifyCheckSumBeforePullFile => false;
        public override bool PullOnlyWhereNotFound => false;

        public abstract string RemoteDirectory { get; }
    }
}
