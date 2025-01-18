using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace TakumiteAudioWrapper
{
    /// <summary>
    /// 音声ラッパー管理クラス
    /// </summary>
    public class AudioManager
    {
        private readonly List<AudioWrapper> _audioWrappers = new();

        /// <summary>
        /// 音声ラッパーを追加
        /// </summary>
        /// <param name="filePath">音声ファイルのパス</param>
        /// <param name="relativeVolume">相対音量</param>
        /// <returns>追加または置き換えられた音声ラッパー</returns>
        public AudioWrapper AddAudio(string filePath, float relativeVolume)
        {
            var existingWrapper = _audioWrappers.Find(wrapper => wrapper.Equals(filePath));
            if (existingWrapper != null)
            {
                _audioWrappers.Remove(existingWrapper);
            }
            var newWrapper = new AudioWrapper(filePath, relativeVolume);
            _audioWrappers.Add(newWrapper);
            return newWrapper;
        }

        /// <summary>
        /// 全ての音声の出力デバイスを変更
        /// </summary>
        /// <param name="deviceNumber">デバイス番号</param>
        public void ChangeOutputDeviceForAll(int deviceNumber)
        {
            foreach (var wrapper in _audioWrappers)
            {
                wrapper.ChangeOutputDevice(deviceNumber);
            }
        }
    }
}
