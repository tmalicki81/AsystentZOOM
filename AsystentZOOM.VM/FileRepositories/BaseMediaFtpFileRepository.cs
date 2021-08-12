using FileService.Clients;
using FileService.FileRepositories;

namespace AsystentZOOM.VM.FileRepositories
{
    public abstract class BaseMediaFtpFileRepository : BaseFtpFileRepository
    {
        public override FtpSessionInfo SessionInfo => new FtpSessionInfo
        {
            HostName = "av-labedy.pl",
            UserName = "tmalicki81-asystent-zoom-documents",
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
