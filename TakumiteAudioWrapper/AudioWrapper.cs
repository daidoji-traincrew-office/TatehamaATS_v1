using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace TakumiteAudioWrapper
{
    /// <summary>
    /// 音声再生をラップするクラス
    /// </summary>
    public class AudioWrapper
    {
        private readonly string _filePath;
        private readonly float _relativeVolume;
        private IWavePlayer _wavePlayer;
        private AudioFileReader _audioFile;
        private LoopStream _loopStream;
        private bool _isLooping;

        /// <summary>
        /// 音声ラッパーを初期化
        /// </summary>
        /// <param name="filePath">音声ファイルのパス</param>
        /// <param name="relativeVolume">相対音量(0.0〜1.0)</param>
        public AudioWrapper(string filePath, float relativeVolume)
        {
            _filePath = filePath;
            _relativeVolume = relativeVolume;
            _isLooping = false;
        }

        /// <summary>
        /// 指定した音量で音声を再生する(1回のみ)
        /// </summary>
        /// <param name="volume">音量(0.0〜1.0)</param>
        public void PlayOnce(float volume)
        {
            Stop();
            _audioFile = new AudioFileReader(_filePath) { Volume = _relativeVolume * volume };
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
            if (_isLooping)
            {
                // ループ再生中は音量変更のみ
                if (_loopStream != null)
                {
                    _loopStream.Volume = _relativeVolume * volume;
                }
                return;
            }

            Stop();
            _audioFile = new AudioFileReader(_filePath);
            _loopStream = new LoopStream(_audioFile) { Volume = _relativeVolume * volume };
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
            _wavePlayer?.Stop();
            _audioFile?.Dispose();
            _loopStream?.Dispose();
            _wavePlayer?.Dispose();
            _audioFile = null;
            _loopStream = null;
            _wavePlayer = null;
            _isLooping = false;
        }

        /// <summary>
        /// 出力デバイスを変更
        /// </summary>
        /// <param name="deviceNumber">デバイス番号</param>
        public void ChangeOutputDevice(int deviceNumber)
        {
            if (_wavePlayer != null && _wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                var volume = _isLooping ? _loopStream.Volume : _audioFile.Volume;
                PlayOnce(volume); // 再生をリセットしてデバイスを切り替える
            }
        }
    }
}
