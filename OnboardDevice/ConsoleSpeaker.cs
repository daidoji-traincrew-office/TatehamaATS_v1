using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakumiteAudioWrapper;
using TatehamaATS_v1.Exceptions;

namespace TatehamaATS_v1.OnboardDevice
{
    internal class ConsoleSpeaker
    {
        AudioManager AudioManager;
        AudioWrapper Bougo;
        int Mizotsuki;
        AudioWrapper Mizotsuki1;
        AudioWrapper Mizotsuki2;

        Random rnd = new Random();

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        public ConsoleSpeaker()
        {
            try
            {
                AudioManager = new AudioManager();
                Bougo = AudioManager.AddAudio("sound/bougo.wav", 1.0f);
                Mizotsuki1 = AudioManager.AddAudio("sound/Mizotsuki1.wav", 1.0f);
                Mizotsuki2 = AudioManager.AddAudio("sound/Mizotsuki2.wav", 1.0f);
            }
            catch (ATSCommonException ex)
            {
                AddExceptionAction.Invoke(ex);
            }
            catch (Exception ex)
            {
                var e = new CsharpException(3, "sound死亡", ex);
                AddExceptionAction.Invoke(e);
            }
        }

        public void ChengeBougoState(bool State)
        {
            if (State)
            {
                Bougo?.PlayLoop(1.0f);
            }
            else
            {
                Bougo?.Stop();
            }
        }

        public void ChengeKyokan(bool State)
        {
            if (State)
            {
                Mizotsuki = rnd.Next(1, 3);
                Debug.WriteLine(Mizotsuki);
                switch (Mizotsuki)
                {
                    case 1:
                        Mizotsuki1?.PlayLoop(1.0f);
                        break;
                    case 2:
                        Mizotsuki2?.PlayLoop(1.0f);
                        break;
                    default:
                        Mizotsuki1?.PlayLoop(1.0f);
                        break;
                }
            }
            else
            {
                switch (Mizotsuki)
                {
                    case 1:
                        Mizotsuki1?.Stop();
                        break;
                    case 2:
                        Mizotsuki2?.Stop();
                        break;
                    default:
                        Mizotsuki1?.Stop();
                        break;
                }
            }
        }
    }
}
