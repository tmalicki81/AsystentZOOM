using FileService.FileRepositories;
using System.IO;

namespace BinaryHelper.FileRepositories
{
    /// <summary>
    /// Repozytorium plikowe na dysku lokalnym
    /// Binaria aplikacji
    /// </summary>
    public class LocalBinFileRepository : BaseLocalFileRepository
    {
        public override string Description => "Binaria aplikacji na dysku lokalnym w katalogu bin";
        public override bool PullOnlyNewerFiles => true;
        public override bool CreateBackupBeforePullFile => false;
        public override bool VerifyCheckSumBeforePullFile => true;
        public override bool PullOnlyWhereNotFound => false;

        public override string RootDirectory
            => Path.Combine(Directory.GetCurrentDirectory(),
                        "bin",
#if(DEBUG)
                        "Debug",
#else
                        "Release",
#endif
                        "net5.0-windows");
                        //"netcoreapp3.1");
    }
}
