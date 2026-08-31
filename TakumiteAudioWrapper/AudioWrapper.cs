using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;
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
        private readonly object _lockObj;
        private IWavePlayer _wavePlayer;
        private AudioFileReader _audioFile;
        private LoopStream _loopStream;
        private bool _isLooping;
        private bool _disposed;
        private int _deviceNumber = -1; // -1はデフォルトデバイス

        /// <summary>
        /// 音声ラッパーを初期化
        /// </summary>
        /// <param name="filePath">音声ファイルのパス</param>
        /// <param name="relativeVolume">相対音量(0.0〜1.0)</param>
        public AudioWrapper(string filePath, float relativeVolume)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _relativeVolume = Math.Clamp(relativeVolume, 0.0f, 1.0f);
            _lockObj = new object();
            _isLooping = false;
            _disposed = false;
        }

        /// <summary>
        /// WavePlayerを初期化(WaveOutEventで失敗した場合はWasapiOutにフォールバック)
        /// </summary>
        /// <param name="waveProvider">音声ストリーム</param>
        private void InitializeWavePlayer(IWaveProvider waveProvider)
        {
            try
            {
                _wavePlayer = new WaveOutEvent
                {
                    DeviceNumber = _deviceNumber
                };
                _wavePlayer.Init(waveProvider);
            }
            catch (NAudio.MmException ex)
            {
                Debug.WriteLine("WaveoutEvent Error, Fallback to WasapiEvent");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                // WasapiOutでデバイスを指定する場合
                if (_deviceNumber >= 0 && _deviceNumber < WaveOut.DeviceCount)
                {
                    var deviceInfo = WaveOut.GetCapabilities(_deviceNumber);
                    // WasapiOutでデバイス名からデバイスを取得
                    using var enumerator = new MMDeviceEnumerator();
                    var wasapiDevices = enumerator.EnumerateAudioEndPoints(
                        DataFlow.Render,
                        DeviceState.Active);

                    MMDevice? targetDevice = null;
                    foreach (var device in wasapiDevices)
                    {
                        if (!device.FriendlyName.Contains(deviceInfo.ProductName))
                        {
                            continue;
                        }

                        targetDevice = device;
                        break;
                    }

                    _wavePlayer = targetDevice != null
                        ? new WasapiOut(targetDevice, AudioClientShareMode.Shared, false, 100)
                        : new(AudioClientShareMode.Shared, false, 100);
                }
                else
                {
                    _wavePlayer = new WasapiOut(AudioClientShareMode.Shared, false, 100);
                }

                _wavePlayer.Init(waveProvider);
            }
        }

        /// <summary>
        /// 指定した音量で音声を再生する(1回のみ)
        /// </summary>
        /// <param name="volume">音量(0.0〜1.0)</param>
        public void PlayOnce(float volume)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioWrapper));

            lock (_lockObj)
            {
                Stop();

                _audioFile = new AudioFileReader(_filePath)
                {
                    Volume = _relativeVolume * Math.Clamp(volume, 0.0f, 1.0f)
                };

                InitializeWavePlayer(_audioFile);
                _wavePlayer.Play();

                _isLooping = false;
            }
        }

        /// <summary>
        /// 指定した音量で音声をループ再生
        /// </summary>
        /// <param name="volume">音量(0.0〜1.0)</param>
        public void PlayLoop(float volume)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioWrapper));
            if (_isLooping) return; // 既にループ再生中の場合は何もしない

            lock (_lockObj)
            {
                Stop(); // 既存リソースを必ず破棄

                try
                {
                    _audioFile = new AudioFileReader(_filePath);
                    _loopStream = new LoopStream(_audioFile)
                    {
                        Volume = _relativeVolume * Math.Clamp(volume, 0.0f, 1.0f)
                    };

                    InitializeWavePlayer(_loopStream);
                    _wavePlayer.Play();

                    _isLooping = true;
                }
                catch (Exception ex)
                {
                    DiagnoseAudioDevices();
                    Debug.WriteLine($"PlayLoop error: {ex.Message}");
                    Dispose();
                    throw;
                }
            }
        }

        public static void DiagnoseAudioDevices()
        {
            Debug.WriteLine($"=== オーディオ診断 ===");
            Debug.WriteLine($"WaveOut デバイス数: {WaveOut.DeviceCount}");

            for (var i = 0; i < WaveOut.DeviceCount; i++)
            {
                var cap = WaveOut.GetCapabilities(i);
                Debug.WriteLine($"  [{i}] {cap.ProductName}");
            }

            if (WaveOut.DeviceCount == 0)
            {
                Debug.WriteLine("⚠ 有効なオーディオ出力デバイスがありません");
            }
        }

        /// <summary>
        /// 再生を停止
        /// </summary>
        public void Stop()
        {
            if (_disposed) return;

            lock (_lockObj)
            {
                try
                {
                    _wavePlayer?.Stop();
                }
                catch
                {
                }

                try
                {
                    _wavePlayer?.Dispose();
                }
                catch
                {
                }

                _wavePlayer = null;

                try
                {
                    _loopStream?.Dispose();
                }
                catch
                {
                }

                _loopStream = null;

                try
                {
                    _audioFile?.Dispose();
                }
                catch
                {
                }

                _audioFile = null;

                _isLooping = false;
            }
        }

        /// <summary>
        /// 出力デバイスを変更
        /// </summary>
        /// <param name="deviceNumber">デバイス番号</param>
        public void ChangeOutputDevice(int deviceNumber)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioWrapper));

            lock (_lockObj)
            {
                _deviceNumber = deviceNumber;

                // デバイスを変更するために再生を停止して再度初期化
                if (_wavePlayer != null && _wavePlayer.PlaybackState == PlaybackState.Playing)
                {
                    var volume = _isLooping ? _loopStream?.Volume ?? 0.0f : _audioFile?.Volume ?? 0.0f;
                    Stop();

                    if (_isLooping)
                    {
                        _audioFile = new AudioFileReader(_filePath);
                        _loopStream = new LoopStream(_audioFile)
                        {
                            Volume = volume
                        };
                        InitializeWavePlayer(_loopStream);
                    }
                    else
                    {
                        _audioFile = new AudioFileReader(_filePath)
                        {
                            Volume = volume
                        };
                        InitializeWavePlayer(_audioFile);
                    }

                    _wavePlayer.Play();
                }
            }
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            lock (_lockObj)
            {
                Stop();
                _disposed = true;
            }
        }
    }
}