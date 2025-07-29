using System;
using System.Diagnostics;
using NAudio.Wave;

namespace TakumiteAudioWrapper
{
    /// <summary>
    /// ループ再生を実現するためのストリーム
    /// </summary>
    public class LoopStream : WaveStream
    {
        private WaveStream _sourceStream;
        private readonly object _lockObj;
        public float Volume { get; set; } = 1.0f;

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _lockObj = new object();
        }

        public override WaveFormat WaveFormat
        {
            get
            {
                lock (_lockObj)
                {
                    return _sourceStream?.WaveFormat;
                }
            }
        }
        public override long Length
        {
            get
            {
                lock (_lockObj)
                {
                    return _sourceStream?.Length ?? 0;
                }
            }
        }
        public override long Position
        {
            get
            {
                lock (_lockObj)
                {
                    return _sourceStream?.Position ?? 0;
                }
            }
            set
            {
                lock (_lockObj)
                {
                    if (_sourceStream != null)
                        _sourceStream.Position = value;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = 0;

                lock (_lockObj)
                {
                    if (_sourceStream == null)
                        break;

                    try
                    {
                        // 現在のストリームからデータを読み取る
                        bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LoopStream Read error: {ex.Message}");
                        break;
                    }
                }
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lockObj)
                {
                    _sourceStream?.Dispose();
                    _sourceStream = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
