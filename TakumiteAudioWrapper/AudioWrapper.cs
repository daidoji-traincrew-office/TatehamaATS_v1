using System;
using NAudio.Wave;

namespace TakumiteAudioWrapper
{
    /// <summary>
    /// 音声再生をラップするクラス
    /// </summary>
    public class AudioWrapper : IDisposable
    {
        private readonly string _filePath;
        private readonly float _relativeVolume;
        private IWavePlayer _wavePlayer;
        private AudioFileReader _audioFile;
        private LoopStream _loopStream;
        private bool _isLooping;
        private bool _disposed;

        /// <summary>
        /// 音声ラッパーを初期化
        /// </summary>
        /// <param name="filePath">音声ファイルのパス</param>
        /// <param name="relativeVolume">相対音量(0.0〜1.0)</param>
        public AudioWrapper(string filePath, float relativeVolume)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _relativeVolume = Math.Clamp(relativeVolume, 0.0f, 1.0f);
            _isLooping = false;
            _disposed = false;
        }

        /// <summary>
        /// 指定した音量で音声を再生する(1回のみ)
        /// </summary>
        /// <param name="volume">音量(0.0〜1.0)</param>
        public void PlayOnce(float volume)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioWrapper));

            Stop();

            _audioFile = new AudioFileReader(_filePath)
            {
                Volume = _relativeVolume * Math.Clamp(volume, 0.0f, 1.0f)
            };
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(_audioFile);
            _wavePlayer.Play();

            _isLooping = false;
        }

        /// <summary>
        /// 指定した音量で音声をループ再生
        /// </summary>
        /// <param name="volume">音量(0.0〜1.0)</param>
        public void PlayLoop(float volume)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioWrapper));

            if (_isLooping)
            {
                if (_loopStream != null)
                {
                    _loopStream.Volume = _relativeVolume * Math.Clamp(volume, 0.0f, 1.0f);
                }
                return;
            }

            Stop();

            _audioFile = new AudioFileReader(_filePath);
            _loopStream = new LoopStream(_audioFile)
            {
                Volume = _relativeVolume * Math.Clamp(volume, 0.0f, 1.0f)
            };

            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(_loopStream);
            _wavePlayer.Play();

            _isLooping = true;
        }

        /// <summary>
        /// 再生を停止
        /// </summary>
        public void Stop()
        {
            if (_disposed) return;

            _wavePlayer?.Stop();

            _wavePlayer?.Dispose();
            _wavePlayer = null;

            _audioFile?.Dispose();
            _audioFile = null;

            _loopStream?.Dispose();
            _loopStream = null;

            _isLooping = false;
        }

        /// <summary>
        /// 出力デバイスを変更
        /// </summary>
        /// <param name="deviceNumber">デバイス番号</param>
        public void ChangeOutputDevice(int deviceNumber)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioWrapper));

            // デバイスを変更するために再生を停止して再度初期化
            if (_wavePlayer != null && _wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                var volume = _isLooping ? _loopStream?.Volume ?? 0.0f : _audioFile?.Volume ?? 0.0f;
                Stop();

                _wavePlayer = new WaveOutEvent
                {
                    DeviceNumber = deviceNumber
                };

                if (_isLooping)
                {
                    _loopStream = new LoopStream(new AudioFileReader(_filePath))
                    {
                        Volume = volume
                    };
                    _wavePlayer.Init(_loopStream);
                }
                else
                {
                    _audioFile = new AudioFileReader(_filePath)
                    {
                        Volume = volume
                    };
                    _wavePlayer.Init(_audioFile);
                }

                _wavePlayer.Play();
            }
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Stop();

            _disposed = true;
        }
    }
}
