using FileService.FileRepositories;
using System.IO;

namespace BinaryHelper.FileRepositories
{
    /// <summary>
    /// Repozytorium plikowe na dysku lokalnym
    /// Kod źródłowy
    /// </summary>
    public class LocalCodeFileRepository : BaseLocalFileRepository
    {
        public override string RootDirectory => Directory.GetCurrentDirectory();
        public override string Description => "Kod solucji";
        public override bool PullOnlyNewerFiles => false;
        public override bool CreateBackupBeforePullFile => false;
        public override bool VerifyCheckSumBeforePullFile => false;
        public override bool PullOnlyWhereNotFound => false;
    }
}
