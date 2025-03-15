using System;
using NAudio.Wave;

namespace TakumiteAudioWrapper
{
    /// <summary>
    /// ループ再生を実現するためのストリーム
    /// </summary>
    public class LoopStream : WaveStream
    {
        private WaveStream _sourceStream;
        public float Volume { get; set; } = 1.0f;

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => _sourceStream.Length;
        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_sourceStream == null)
                return 0;

            try
            {
                int bytesRead = _sourceStream.Read(buffer, offset, count);
                if (bytesRead == 0)
                {
                    _sourceStream.Position = 0;
                    bytesRead = _sourceStream.Read(buffer, offset, count);
                }

                // 音量調整
                for (int i = 0; i < bytesRead; i += 2)
                {
                    short sample = (short)(buffer[offset + i] | (buffer[offset + i + 1] << 8));
                    sample = (short)Math.Clamp(sample * Volume, short.MinValue, short.MaxValue);
                    buffer[offset + i] = (byte)(sample & 0xFF);
                    buffer[offset + i + 1] = (byte)((sample >> 8) & 0xFF);
                }

                return bytesRead;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during reading: {ex.Message}");
                return 0;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sourceStream?.Dispose();
                _sourceStream = null;
            }
            base.Dispose(disposing);
        }
    }
}
