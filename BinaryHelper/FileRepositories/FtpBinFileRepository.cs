using FileService.Clients;
using FileService.Common;
using FileService.FileRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace BinaryHelper.FileRepositories
{
    /// <summary>
    /// Repozytorium plikowe na serwerze FTP
    /// Binaria aplikacji
    /// </summary>
    public class FtpBinFileRepository : BaseFtpFileRepository
    {
        public override string Description => "Binaria aplikacji na serwerze FTP";

        public override bool CreateBackupBeforePullFile => false;
        public override bool PullOnlyNewerFiles => true;
        public override bool VerifyCheckSumBeforePullFile => false;
        public override bool PullOnlyWhereNotFound => false;

        private FtpSessionInfo _sessionInfo;
        private FtpSessionInfo GetFtpSessionInfo()
        {
            var xDoc = XDocument.Load(Assembly.GetExecutingAssembly().Location + ".config");
            Dictionary<string, string> elements = xDoc
                .Element(XName.Get("configuration"))
                .Element(XName.Get("appSettings"))
                .Elements(XName.Get("add"))
                .ToDictionary(k => k.Attribute(XName.Get("key")).Value, v => v.Attribute(XName.Get("value")).Value);
            return new FtpSessionInfo
            {
                HostName = elements["FtpHostName"],
                Password = CryptoHelper.DecryptString("992545eca56942ad", elements["FtpPassword"]),
                RemoteDirectory = elements["FtpRemoteDirectory"],
                UserName = $"tmalicki81-{elements["FtpUserName"]}"
            };
        }

        public override FtpSessionInfo SessionInfo
            => _sessionInfo ??= GetFtpSessionInfo();
    }
}
