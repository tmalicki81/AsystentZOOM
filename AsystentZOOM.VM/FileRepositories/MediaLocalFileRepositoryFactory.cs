using AsystentZOOM.VM.Common.AudioRecording;
using System;
using System.IO;

namespace AsystentZOOM.VM.FileRepositories
{
    public static class MediaLocalFileRepositoryFactory
    {
        public static BaseMediaLocalFileRepository Images 
            => new ImagesMediaLocalFileRepository();

        public static BaseMediaLocalFileRepository Videos 
            => new VideosMediaLocalFileRepository();
        
        public static BaseMediaLocalFileRepository Music 
            => new MusicMediaLocalFileRepository();
        public static BaseMediaLocalFileRepository AudioRecording 
            => new AudioRecordingLocalFileRepository();

        public static BaseMediaLocalFileRepository Meetings 
            => new MeetingsMediaLocalFileRepository();

        public static BaseMediaLocalFileRepository TimePiece 
            => new TimePieceMediaLocalFileRepository();

        public static BaseMediaLocalFileRepository Background 
            => new BackgroundMediaLocalFileRepository();
    }

    public class ImagesMediaLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyPictures;
        public override string Description => "Obrazy na dysku lokalnym";
    }

    public class VideosMediaLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyVideos;
        public override string Description => "Video na dysku lokalnym";
    }

    public class MusicMediaLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyMusic;
        public override string[] FileExtensions => new string[] { "MP3" };
        public override string Description => "Muzyka na dysku lokalnym";
    }

    public class AudioRecordingLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyMusic;
        public override string RootDirectory => Path.Combine(base.RootDirectory, AudioRecordingProvider.AudioRecording);
        public override bool PullOnlyWhereNotFound => true;
        public override string[] FileExtensions => new string[] { "MP3" };
        public override string Description => "Nagrania dźwiękowe na dysku lokalnym";
    }

    public class MeetingsMediaLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override bool VerifyCheckSumBeforePullFile => false;
        public override bool PullOnlyNewerFiles => true;
        public override bool CreateBackupBeforePullFile => true;
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyDocuments;
        public override string[] FileExtensions => new string[] { "MEETING" };
        public override string Description => "Dokumenty spotkań na dysku lokalnym";
    }

    public class TimePieceMediaLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyDocuments;
        public override string Description => "Zegary spotkań na dysku lokalnym";
        public override string[] FileExtensions => new[] { "TIM" };
    }

    public class BackgroundMediaLocalFileRepository : BaseMediaLocalFileRepository
    {
        public override Environment.SpecialFolder DestinationInLocal => Environment.SpecialFolder.MyDocuments;
        public override string Description => "Tła na dysku lokalnym";
        public override string[] FileExtensions => new[] { "BCG" };
    }
}