namespace AsystentZOOM.VM.FileRepositories
{
    public static class MediaFtpFileRepositoryFactory
    {
        public static BaseMediaFtpFileRepository Images = new ImagesMediaFtpFileService();
        public static BaseMediaFtpFileRepository Videos = new VideosMediaFtpFileService();
        public static BaseMediaFtpFileRepository Music = new MusicMediaFtpFileService();
        public static BaseMediaFtpFileRepository AudioRecording = new AudioRecordingFtpFileService();
        public static BaseMediaFtpFileRepository Meetings = new MeetingsMediaFtpFileService();
        public static BaseMediaFtpFileRepository TimePiece = new TimePieceMediaFtpFileService();
        public static BaseMediaFtpFileRepository Background = new BackgroundMediaFtpFileService();
    }

    public class ImagesMediaFtpFileService : BaseMediaFtpFileRepository
    {
        public override string RemoteDirectory => "Pictures";
        public override string Description => "Obrazy na serwerze FTP";
    }

    public class VideosMediaFtpFileService : BaseMediaFtpFileRepository
    {
        public override string RemoteDirectory => "Videos";
        public override string Description => "Video na serwerze FTP";
    }

    public class MusicMediaFtpFileService : BaseMediaFtpFileRepository
    {
        public override string RemoteDirectory => "Music";
        public override string Description => "Muzyka na serwerze FTP";
    }

    public class AudioRecordingFtpFileService : BaseMediaFtpFileRepository
    {
        public override string RemoteDirectory => "AudioRecording";
        public override string Description => "Nagrania dźwiękowe na serwerze FTP";
        public override string[] FileExtensions => new string[] { "MP3" };
        public override bool PullOnlyWhereNotFound => true;
    }

    public class MeetingsMediaFtpFileService : BaseMediaFtpFileRepository
    {
        public override bool VerifyCheckSumBeforePullFile => true;
        public override bool PullOnlyNewerFiles => false;
        public override bool CreateBackupBeforePullFile => true;
        public override string RemoteDirectory => "Meetings";
        public override string[] FileExtensions => new string[] { "MEETING" };
        public override string Description => "Dokumenty spotkań na serwerze FTP";
    }

    public class TimePieceMediaFtpFileService : BaseMediaFtpFileRepository
    {
        public override string RemoteDirectory => "TimePieces";
        public override string Description => "Zegary spotkań na serwerze FTP";
    }

    public class BackgroundMediaFtpFileService : BaseMediaFtpFileRepository
    {
        public override string RemoteDirectory => "Backgrounds";
        public override string Description => "Tła na serwerze FTP";
    }
}