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
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                // 現在のストリームからデータを読み取る
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    // ストリームの終端に到達した場合、始端に戻る
                    _sourceStream.Position = 0;
                }
                else
                {
                    totalBytesRead += bytesRead;
                }
            }

            return totalBytesRead;
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
