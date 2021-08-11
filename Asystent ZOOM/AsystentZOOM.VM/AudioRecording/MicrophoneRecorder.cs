using NAudio.Wave;
using System;

namespace AsystentZOOM.VM.Common.AudioRecording
{
    /// <summary>
    /// Obsługa nagrywania dźwięku z mikrofonu
    /// </summary>
    public class MicrophoneRecorder : BaseAudioRecorder
    {
        public MicrophoneRecorder(Func<string> titleFunction, string recordFilesDirectory) 
            : base(titleFunction, recordFilesDirectory)
        {
        }

        public override IWaveIn CreateWaveIn()
            => new WaveIn 
            { 
                WaveFormat = new WasapiLoopbackCapture().WaveFormat 
            };

        public override string FilePrefix => "MIC";
    }
}
