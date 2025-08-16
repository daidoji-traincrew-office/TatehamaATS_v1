using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TakumiteAudioWrapper;
using TatehamaATS_v1.Exceptions;
using TrainCrewAPI;

namespace TatehamaATS_v1.RetsubanWindow
{
    internal class TimeLogic
    {
        private TimeData BeforeTimeData { get; set; }
        private TimeSpan ShiftTime { get; set; } = TimeSpan.FromHours(-10);
        private Dictionary<string, Image> Images_7seg { get; set; }
        private string NewHour { get; set; }

        public bool nowSetting;

        private AudioManager AudioManager;
        private AudioWrapper beep1;
        private AudioWrapper beep2;

        private PictureBox Time_h2 { get; set; }
        private PictureBox Time_h1 { get; set; }
        private PictureBox Time_m2 { get; set; }
        private PictureBox Time_m1 { get; set; }
        private PictureBox Time_s2 { get; set; }
        private PictureBox Time_s1 { get; set; }

        internal TimeLogic(PictureBox time_h2, PictureBox time_h1, PictureBox time_m2, PictureBox time_m1, PictureBox time_s2, PictureBox time_s1)
        {
            // 表示領域渡し
            Time_h2 = time_h2;
            Time_h1 = time_h1;
            Time_m2 = time_m2;
            Time_m1 = time_m1;
            Time_s2 = time_s2;
            Time_s1 = time_s1;

            // 初期化
            var tst_time = DateTime.Now + ShiftTime;
            BeforeTimeData = new TimeData()
            {
                hour = tst_time.Hour,
                minute = tst_time.Minute,
                second = tst_time.Second
            };
            Images_7seg = new Dictionary<string, Image> {
                {"0",  RetsubanResource._7seg_0} ,
                {"1",  RetsubanResource._7seg_1} ,
                {"2",  RetsubanResource._7seg_2} ,
                {"3",  RetsubanResource._7seg_3} ,
                {"4",  RetsubanResource._7seg_4} ,
                {"5",  RetsubanResource._7seg_5} ,
                {"6",  RetsubanResource._7seg_6} ,
                {"7",  RetsubanResource._7seg_7} ,
                {"8",  RetsubanResource._7seg_8} ,
                {"9",  RetsubanResource._7seg_9} ,
                {" ",  RetsubanResource._7seg_N} ,
                {"",  RetsubanResource._7seg_N}
            };
            NewHour = BeforeTimeData.hour.ToString();
            Time_h2 = time_h2;

            AudioManager = new AudioManager();
            beep1 = AudioManager.AddAudio("sound/beep1.wav", 1.0f);
            beep2 = AudioManager.AddAudio("sound/beep2.wav", 1.0f);
        }

        public void ClockTimer_Tick()
        {
            var tst_time = DateTime.Now + ShiftTime;
            TimeData timeData = new TimeData()
            {
                hour = tst_time.Hour < 4 ? tst_time.Hour + 24 : tst_time.Hour,
                minute = tst_time.Minute,
                second = tst_time.Second
            };
            if (nowSetting)
            {
                TimeDrawing(timeData, DateTime.Now.Millisecond < 500);
                BeforeTimeData = timeData;
            }
            else
            {
                if (BeforeTimeData.second != timeData.second)
                {
                    NewHour = timeData.hour.ToString();
                    TimeDrawing(timeData);
                    BeforeTimeData = timeData;
                }
            }
        }

        /// <summary>
        /// 時間部描画
        /// </summary>
        /// <param name="timeData"></param>
        private void TimeDrawing(TimeData timeData, bool hourDot = false)
        {
            Time_h1.Image = hourDot ? RetsubanResource._7seg_dot : null;
            string hour = timeData.hour.ToString().PadLeft(2, ' ');
            if (nowSetting)
            {
                hour = NewHour.PadLeft(2, ' ');
            }
            Time_h2.BackgroundImage = Images_7seg[$"{hour[0]}"];
            Time_h1.BackgroundImage = Images_7seg[$"{hour[1]}"];
            string minute = timeData.minute.ToString().PadLeft(2, '0');
            Time_m2.BackgroundImage = Images_7seg[$"{minute[0]}"];
            Time_m1.BackgroundImage = Images_7seg[$"{minute[1]}"];
            string second = timeData.second.ToString().PadLeft(2, '0');
            Time_s2.BackgroundImage = Images_7seg[$"{second[0]}"];
            Time_s1.BackgroundImage = Images_7seg[$"{second[1]}"];
        }

        internal void Buttons_Digit(string Digit)
        {
            // 自身が設定中でない場合入力スルーする
            if (!nowSetting)
            {
                return;
            }
            switch (NewHour)
            {
                case "":
                    NewHour += Digit;
                    beep1.PlayOnce(1.0f);
                    break;
                case "0":
                case "1":
                    NewHour += Digit;
                    beep1.PlayOnce(1.0f);
                    break;
                case "2":
                    if (!(Digit == "8" || Digit == "9"))
                    {
                        NewHour += Digit;
                        beep1.PlayOnce(1.0f);
                    }
                    break;
                default:
                    break;
            }
        }

        internal void Buttons_Func(string Name)
        {
            switch (Name)
            {
                case "Set":
                    if (nowSetting)
                    {
                        var newHour = Int32.Parse(NewHour);
                        newHour = newHour + 24;
                        ShiftTime = TimeSpan.FromHours(newHour - DateTime.Now.Hour);
                        nowSetting = false;
                        beep2.PlayOnce(1.0f);
                    }
                    return;
                case "Del":
                    if (nowSetting)
                    {
                        if (string.IsNullOrEmpty(NewHour))
                        {
                            return;
                        }
                        NewHour = NewHour.Remove(NewHour.Length - 1);
                        beep1.PlayOnce(1.0f);
                    }
                    return;
                case "Clear":
                    if (nowSetting)
                    {
                        NewHour = "";
                        beep2.PlayOnce(1.0f);
                    }
                    return;
                case "TimeSet":
                    nowSetting = true;
                    NewHour = "";
                    beep1.PlayOnce(1.0f);
                    TimeDrawing(BeforeTimeData, DateTime.Now.Millisecond < 500);
                    return;
                case "RetsuSet":
                case "CarSet":
                case "UnkoSet":
                case "StopSet":
                case "VerDisplay":
                    nowSetting = false;
                    TimeDrawing(BeforeTimeData);
                    return;
            }
        }
    }
}
