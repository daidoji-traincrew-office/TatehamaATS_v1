using System;
using NAudio.Wave;

namespace TakumiteAudioWrapper
{
    /// <summary>
    /// ループ再生を実現するためのストリーム
    /// </summary>
    public class LoopStream : WaveStream
    {
        private readonly WaveStream _sourceStream;
        public float Volume { get; set; } = 1.0f; // 音量調整用プロパティを追加

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
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
            int bytesRead = _sourceStream.Read(buffer, offset, count);
            if (bytesRead == 0)
            {
                _sourceStream.Position = 0; // ループ
                bytesRead = _sourceStream.Read(buffer, offset, count);
            }

            // 音量調整
            for (int i = 0; i < bytesRead; i += 2)
            {
                short sample = (short)(buffer[offset + i] | (buffer[offset + i + 1] << 8));
                sample = (short)(sample * Volume);
                buffer[offset + i] = (byte)(sample & 0xFF);
                buffer[offset + i + 1] = (byte)((sample >> 8) & 0xFF);
            }

            return bytesRead;
        }
    }
}
