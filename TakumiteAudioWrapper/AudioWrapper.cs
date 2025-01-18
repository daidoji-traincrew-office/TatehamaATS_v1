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

        /// <summary>
        /// 音声ラッパーを初期化
        /// </summary>
        /// <param name="filePath">音声ファイルのパス</param>
        /// <param name="relativeVolume">相対音量(0.0〜1.0)</param>
        public AudioWrapper(string filePath, float relativeVolume)
        {
            _filePath = filePath;
            _relativeVolume = relativeVolume;
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
        }

        /// <summary>
        /// 指定した音量で音声をループ再生
        /// </summary>
        /// <param name="volume">音量(0.0〜1.0)</param>
        public void PlayLoop(float volume)
        {
            Stop();
            _audioFile = new AudioFileReader(_filePath) { Volume = _relativeVolume * volume };
            var loopStream = new LoopStream(_audioFile);
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(loopStream);
            _wavePlayer.Play();
        }

        /// <summary>
        /// 再生を停止
        /// </summary>
        public void Stop()
        {
            _wavePlayer?.Stop();
            _audioFile?.Dispose();
            _wavePlayer?.Dispose();
            _audioFile = null;
            _wavePlayer = null;
        }

        /// <summary>
        /// 出力デバイスを変更
        /// </summary>
        /// <param name="deviceNumber">デバイス番号</param>
        public void ChangeOutputDevice(int deviceNumber)
        {
            if (_wavePlayer != null && _wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                var volume = _audioFile.Volume;
                PlayOnce(volume); // 再生をリセットしてデバイスを切り替える
            }
        }
    }
}
