using NAudio.Wave;
using System;

namespace AsystentZOOM.VM.Common.AudioRecording
{
    public class WaveProviderToWaveStream : WaveStream
    {
        private readonly IWaveProvider _source;
        private long _position;

        public WaveProviderToWaveStream(IWaveProvider source)
            => _source = source;

        public override WaveFormat WaveFormat
            => _source.WaveFormat;

        public override long Length
            => int.MaxValue;

        public override long Position
        {
            get => _position;
            set => throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _source.Read(buffer, offset, count);
            _position += read;
            return read;
        }
    }
}
