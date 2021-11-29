using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.Model;

namespace AsystentZOOM.VM.Enums
{
    public enum FileExtensionEnum
    {
        [FileExtensionConfig(
            "MEETING",
            null,
            typeof(MeetingsMediaLocalFileRepository),
            typeof(MeetingsMediaFtpFileService))]
        MEETING,

        [FileExtensionConfig(
            "TMP_MEETING",
            null,
            typeof(MeetingsMediaLocalFileRepository),
            typeof(MeetingsMediaFtpFileService))]
        TMP_MEETING,

        [FileExtensionConfig(
            "MP4",
            typeof(VideoFileInfo),
            typeof(VideosMediaLocalFileRepository),
            typeof(VideosMediaFtpFileService))]
        MP4,

        [FileExtensionConfig(
            "TIM",
            typeof(TimePieceFileInfo),
            typeof(TimePieceMediaLocalFileRepository),
            typeof(TimePieceMediaFtpFileService))]
        TIM,

        [FileExtensionConfig(
            "TMP_TIM",
            typeof(TimePieceFileInfo),
            typeof(TimePieceMediaLocalFileRepository),
            typeof(TimePieceMediaFtpFileService))]
        TMP_TIM,

        [FileExtensionConfig(
            "MP3",
            typeof(AudioFileInfo),
            typeof(MusicMediaLocalFileRepository),
            typeof(MusicMediaFtpFileService))]
        MP3,

        [FileExtensionConfig(
            "WAV",
            typeof(AudioFileInfo),
            typeof(MusicMediaLocalFileRepository),
            typeof(MusicMediaFtpFileService))]
        WAV,

        [FileExtensionConfig(
            "JPG",
            typeof(ImageFileInfo),
            typeof(ImagesMediaLocalFileRepository),
            typeof(ImagesMediaFtpFileService))]
        JPG,

        [FileExtensionConfig(
            "JPEG",
            typeof(ImageFileInfo),
            typeof(ImagesMediaLocalFileRepository),
            typeof(ImagesMediaFtpFileService))]
        JPEG,

        [FileExtensionConfig(
            "PNG",
            typeof(ImageFileInfo),
            typeof(ImagesMediaLocalFileRepository),
            typeof(ImagesMediaFtpFileService))]
        PNG,

        [FileExtensionConfig(
            "BCG",
            typeof(BackgroundFileInfo),
            typeof(BackgroundMediaLocalFileRepository),
            typeof(BackgroundMediaFtpFileService))]
        BCG
    }
}