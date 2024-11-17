using System.Diagnostics;
using TatehamaATS_v1.ATSDisplay;
using TatehamaATS_v1.Exceptions;

namespace TatehamaATS_v1
{

    internal class ControlLED
    {
        internal bool isShow;
        internal bool isTest;
        internal List<string> ExceptionCodes;
        private LEDWindow ledWindow;
        private int l3Index = 0; // display.L3のインデックスを追跡するための変数         
        private TimeSpan L3Start = new TimeSpan();
        private DateTime TestStart = DateTime.MinValue;
        internal string? overrideText = null;

        internal bool ATSLEDTest;
        internal ATSDisplayData TC_ATSDisplayData;

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;


        internal ControlLED()
        {
            ExceptionCodes = new List<string>();
            TC_ATSDisplayData = new ATSDisplayData("", "", [""], true);
            try
            {
                ledWindow = new LEDWindow();
                ledWindow.LEDTestModePush += TestModePush;
                LEDHide();
            }
            catch (Exception ex)
            {
                throw new LEDControlInitialzingFailure(3, "ControlLED.cs@ControlLED()", ex);
            }
            Task.Run(() => StartDisplayUpdateLoop());
        }

        private void TestModePush()
        {
            ATSLEDTest = true;
        }

        public void LEDHide()
        {
            if (ledWindow.InvokeRequired)
            {
                ledWindow.Invoke(new Action(() => LEDHide()));
                return;
            }
            ledWindow.Hide();
            isShow = false;
        }

        public void LEDShow()
        {
            if (ledWindow.IsDisposed)
            {
                ledWindow = new LEDWindow();
                ledWindow.Show();
                ledWindow.BringToFront();
                isShow = true;
            }
            else
            {
                ledWindow.Show();
                ledWindow.BringToFront();
                isShow = true;
            }
        }

        /// <summary>
        /// 非同期で表示器を更新するループを開始する
        /// </summary>
        private async void StartDisplayUpdateLoop()
        {
            while (true)
            {
                var timer = Task.Delay(20);
                try
                {
                    UpdateDisplay();
                }
                catch (ATSCommonException ex)
                {
                    AddExceptionAction.Invoke(ex);
                }
                catch (Exception ex)
                {
                    var e = new LEDControlException(3, "LEDカウンタ異常", ex);
                    AddExceptionAction.Invoke(e);
                }
                await timer;
            }
        }

        /// <summary>
        /// 表示を更新する
        /// </summary>
        private void UpdateDisplay()
        {
            //Debug.WriteLine(TrainState.ATSDisplay);
            if (ATSLEDTest)
            {
                if (TestStart == DateTime.MinValue)
                {
                    TestStart = DateTime.Now;
                }
                var deltaT = DateTime.Now - TestStart;

                var LED = deltaT.Seconds % 3 + 27;
                var Place = deltaT.Seconds / 3 % 3 + 1;
                if (deltaT < TimeSpan.FromSeconds(6))
                {
                    ledWindow.DisplayImage(2, 66);
                    ledWindow.DisplayImage(3, 67);
                    if (deltaT < TimeSpan.FromSeconds(3))
                    {
                        ledWindow.DisplayImage(1, 64);
                    }
                    else if (deltaT < TimeSpan.FromSeconds(6))
                    {
                        ledWindow.DisplayImage(1, 65);
                    }
                    return;
                }
                else
                {
                    TestStart = DateTime.MinValue;
                    ATSLEDTest = false;
                }
            }
            if (TC_ATSDisplayData != null)
            {

                var display = TC_ATSDisplayData;
                if (display != null)
                {
                    List<string> L3List;
                    if (ExceptionCodes.Count != 0)
                    {
                        L3List = ExceptionCodes;
                    }
                    else
                    {
                        L3List = display.L3;
                    }
                    if (L3List.Count == 0)
                    {
                        L3Start = DateTime.Now.TimeOfDay;
                        ledWindow.DisplayImage(1, ConvertToLEDNumber(overrideText != null ? overrideText : display.L1));
                        ledWindow.DisplayImage(2, ConvertToLEDNumber(display.L2));
                        ledWindow.DisplayImage(3, 0);
                    }
                    else
                    {
                        var NowTime = DateTime.Now.TimeOfDay;

                        if (L3List.Count == 1)
                        {
                            L3Start = NowTime;
                            l3Index = 0;
                        }
                        else
                        {
                            l3Index = (int)((NowTime - L3Start).TotalSeconds * 2 + 1) % L3List.Count;
                        }

                        ledWindow.DisplayImage(3, ConvertToLEDNumber(L3List[l3Index]));

                        //L3に数値赤要素があるとき
                        if (L3List.Contains("B動作") || L3List.Contains("EB"))
                        {
                            //赤い方
                            ledWindow.DisplayImage(2, ConvertToLEDNumber(display.L2) + 100);
                        }
                        else if (L3List.Contains("P接近"))
                        {
                            //橙の方
                            ledWindow.DisplayImage(2, ConvertToLEDNumber(display.L2) + 50);
                        }
                        else
                        {
                            ledWindow.DisplayImage(2, ConvertToLEDNumber(display.L2));
                        }
                        ledWindow.DisplayImage(1, ConvertToLEDNumber(overrideText != null ? overrideText : display.L1));
                    }
                }
                else
                {
                    ledWindow.DisplayImage(1, 0);
                    ledWindow.DisplayImage(2, 0);
                    ledWindow.DisplayImage(3, 0);
                }
            }
            else
            {
                ledWindow.DisplayImage(1, 0);
                ledWindow.DisplayImage(2, 0);
                if (ExceptionCodes.Count != 0)
                {
                    List<string> L3List;
                    L3List = ExceptionCodes;
                    var NowTime = DateTime.Now.TimeOfDay;
                    if (L3List.Count == 1)
                    {
                        L3Start = NowTime;
                        l3Index = 0;
                    }
                    else
                    {
                        l3Index = (int)((NowTime - L3Start).TotalSeconds * 2 + 1) % L3List.Count;
                    }
                    ledWindow.DisplayImage(3, ConvertToLEDNumber(L3List[l3Index]));
                }
                else
                {
                    L3Start = DateTime.Now.TimeOfDay;
                    ledWindow.DisplayImage(3, 0);
                }
            }
        }

        private int ConvertToLEDNumber(string str)
        {
            //16進数で解釈できる場合=故障表示の可能性
            if (int.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out _))
            {
                int parse = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                //数値が故障表示範囲内の場合
                if (0x180 <= parse && parse <= 0x1FF || 0x280 <= parse && parse <= 0x2FF || 0x380 <= parse && parse <= 0x3FF)
                {
                    return parse;
                }
            }
            if (int.TryParse(str, System.Globalization.NumberStyles.Number, null, out _))
            {
                int parse = int.Parse(str, System.Globalization.NumberStyles.Number);
                if (parse == 0)
                {
                    return 100;
                }
                if (parse == 5)
                {
                    return 101;
                }
                if (parse == 110)
                {
                    return 122;
                }
                if (parse == 112)
                {
                    return 122;
                }
                if (parse == 120)
                {
                    return 123;
                }
                if (parse == 130)
                {
                    return 124;
                }
                if (parse == 160)
                {
                    return 125;
                }
                if (parse == 300)
                {
                    return 122;
                }
                return (parse / 5) + 101;
            }
            switch (str)
            {
                case "":
                case "無表示":
                case "OFF":
                case null:
                    return 0;
                case "普通":
                    return 1;
                case "準急":
                    return 2;
                case "急行":
                    return 3;
                case "快急":
                    return 4;
                case "快速急行":
                    return 4;
                case "区急":
                    return 6;
                case "A特":
                    return 7;
                case "B特":
                    return 8;
                case "C特1":
                    return 9;
                case "C特2":
                    return 10;
                case "C特3":
                    return 11;
                case "C特4":
                    return 12;
                case "D特":
                    return 13;
                case "回送":
                    return 15;
                case "だんじり準急":
                    return 21;
                case "だんじり急行":
                    return 22;
                case "だんじり快急":
                    return 23;
                case "回送-2":
                    return 24;
                case "回送-3":
                    return 25;
                case "C特2-2":
                    return 26;
                case "F":
                    return 126;
                case "P":
                    return 50;
                case "P接近":
                    return 51;
                case "B動作":
                    return 52;
                case "EB":
                    return 53;
                case "終端P":
                    return 54;
                case "停P":
                    return 55;
                case "非常":
                    return 57;
                case "運転":
                    return 58;
                case "開放":
                    return 59;
                case "早着":
                    return 61;
                case "撤去":
                    return 62;
                case "御水 澪":
                    return 63;
                default:
                    throw new LEDDisplayStringAbnormal(3, $"未定義:{str}　ControlLED.cs@ConvertToLEDNumber()");
            }
        }
    }
}
