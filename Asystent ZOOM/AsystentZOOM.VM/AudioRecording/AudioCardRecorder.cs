using NAudio.Wave;
using System;

namespace AsystentZOOM.VM.Common.AudioRecording
{
    /// <summary>
    /// Obsługa nagrywania dźwięku z karty dźwiękowej
    /// </summary>
    public class AudioCardRecorder : BaseAudioRecorder
    {
        public AudioCardRecorder(Func<string> titleFunction, string recordFilesDirectory) 
            : base(titleFunction, recordFilesDirectory)
        {
        }

        public override IWaveIn CreateWaveIn()
            => new WasapiLoopbackCapture();

        public override string FilePrefix => "AUD";
    }
}