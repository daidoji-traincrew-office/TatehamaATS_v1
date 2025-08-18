using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TakumiteAudioWrapper;
using TatehamaATS_v1.OnboardDevice;
using TrainCrewAPI;

namespace TatehamaATS_v1.RetsubanWindow
{
    internal class LCDLogic
    {

        private List<string> LCDFontList = new List<string>();

        private int nowUnkoSetting = -1;
        private int nowStopSetting = -1;
        private int nowStopCount = 0;

        private string nowInput;

        private bool nowVerDisplay = false;

        internal StopPassManager StopPassManager;

        private PictureBox LCD { get; set; }
        private string Retsuban;
        private string Car;

        private AudioManager AudioManager;
        private AudioWrapper beep1;
        private AudioWrapper beep2;
        private AudioWrapper beep3;

        /// <summary>
        /// 設定情報変更
        /// </summary>                                 
        internal event Action<string, string> SetType;
        internal event Action<string, string> SetStaStop;

        internal LCDLogic(PictureBox lcd)
        {
            LCD = lcd;
            var lcdfontString =
                " ，．・：?？！”｜…（）［］＜＞←→＠" +
                "0123456789回試臨xyz■特停通" +
                "ABCDEFGHIJKLMNOPQRST" +
                "UVWXYZŪŌァィゥェォヵヶャュョッー" +
                "アイウエオカキクケコサシスセソタチツテト" +
                "ナニヌネノハヒフヘホマミムメモヤ　ユ　ヨ" +
                "ラリルレロワヰヱヲン゛゜";

            // LCDFontListにlcdfontStringを1文字ずつ追加
            foreach (char c in lcdfontString)
            {
                LCDFontList.Add(c.ToString());
            }

            AudioManager = new AudioManager();
            beep1 = AudioManager.AddAudio("sound/beep1.wav", 1.0f);
            beep2 = AudioManager.AddAudio("sound/beep2.wav", 1.0f);
            beep3 = AudioManager.AddAudio("sound/beep3.wav", 1.0f);

            LCDDrawing();
        }

        public void ClockTimer_Tick()
        {
            LCDDrawing();
        }

        public void SetRetsuban(string retsuban)
        {
            Retsuban = retsuban.Replace("X", "x").Replace("Y", "y").Replace("Z", "z");
            StopPassManager.TypeString(Retsuban);
            StopPassManager.TypeNameTC = StopPassManager.TypeStringTC(StopPassManager.TypeName);
            StopPassManager.TypeNameKana = StopPassManager.TypeStringKana(StopPassManager.TypeName);
            StopPassManager.TypeToStop();
            Debug.WriteLine(StopPassManager);
        }

        public void SetCar(string car)
        {
            Car = car;
        }

        public void LCDDrawing()
        {
            var displayList = GetDisplayList();

            // 画像を取得してリストに格納
            List<Bitmap> lcdImages = new List<Bitmap>();
            foreach (var str in displayList)
            {
                lcdImages.Add(GetLCDFontImageByChar(str));
            }

            // 画像をLCD領域にならべる
            var NewLCD = new Bitmap(LCD.Width, LCD.Height);
            // 起点を8,5として、xは22、yは32ごとに並べる。横は16文字制限
            using (Graphics g = Graphics.FromImage(NewLCD))
            {
                int x = 8;
                int y = 5;
                for (int i = 0; i < lcdImages.Count; i++)
                {
                    if (i % 16 == 0 && i != 0) // 15文字ごとに改行
                    {
                        x = 8; // xをリセット
                        y += 32; // yを次の行に移動
                    }
                    g.DrawImage(lcdImages[i], x, y, 20, 28); // サイズは20x28で描画
                    x += 22; // 次の文字の位置へ移動
                }
            }
            //描画する
            LCD.BackgroundImage = NewLCD;
        }

        private List<string> GetDisplayList()
        {
            var displayList = new List<string>();
            //表示文字列がないタイプ
            if (nowStopSetting >= 0)
            {
                // Ver表示
                displayList = GetStopString();
                return displayList;
            }

            //表示文字列があるタイプ
            string displayString;
            //運行設定
            if (nowUnkoSetting >= 0)
            {
                // Ver表示
                displayString = GetUnkoString();
            }
            else if (nowVerDisplay)
            {
                // Ver表示
                displayString = GetVerString();
            }
            else
            {
                // 通常状態
                displayString = GetNormalString();
            }
            var displayStringList = new List<string>();
            // 文字列を改行ごとに分割し、各行をリストに追加
            foreach (var line in displayString.Split('\n'))
            {
                displayStringList.Add(line);
            }

            for (int i = 0; i < displayStringList.Count; i++)
            {
                displayList.SetDisplayData(displayStringList[i], 0, i, false); // 横書きで配置
            }

            return displayList;
        }

        private string GetUnkoString()
        {
            if (nowUnkoSetting == 0)
            {
                if (DateTime.Now.Second % 2 == 0)
                {
                    return GetAvailableChar($"{Retsuban} ウンコウセッテイ\n{StopPassManager.TypeNameKana}\nシハツ→シュウチャク");
                }
                else
                {
                    return GetAvailableChar($"{Retsuban} ウンコウセッテイ\nシュベツ：1\nシハツ：2 イキサキ：3");
                }
            }
            if (nowUnkoSetting == 1)
            {
                if (DateTime.Now.Millisecond < 500)
                {
                    var input = StopPassManager.TypeStringKana(nowInput);
                    if (input.Contains("？"))
                    {
                        return GetAvailableChar($"ウンコウセッテイ シュベツ\nレツバン：{Retsuban}\nセッテイ：{input.Replace("？", "■")}");
                    }
                    else
                    {
                        return GetAvailableChar($"ウンコウセッテイ シュベツ\nレツバン：{Retsuban}\nセッテイ：{input}■");
                    }
                }
                else
                {
                    return GetAvailableChar($"ウンコウセッテイ シュベツ\nレツバン：{Retsuban}\nセッテイ：{StopPassManager.TypeStringKana(nowInput)}");
                }
            }
            return GetAvailableChar($"ウンコウセッテイ\nミテイギリョウイキ");
        }

        private List<string> GetStopString()
        {
            var displayList = new List<string>();
            if (nowStopSetting == 0)
            {
                displayList.SetDisplayData("テイシャセッテイ　　シュヨウエキ", 0, 0, false);
                displayList.SetDisplayData("1ミオ…タハ2リカ…ナノ3ムイ…", 0, 1, false);
                displayList.SetDisplayData("…オオ4ナタ…シワ5フエ…タイ6", 0, 2, false);
            }
            else
            {
                displayList.SetDisplayData("テイシャセッテイ", 0, 0, false);
                displayList.SetDisplayData("ミテイギリョウイキ", 0, 0, false);
            }
            return displayList;
        }

        private string GetVerString()
        {
            return GetAvailableChar($"ソフトバージョン\nV.{ServerAddress.Version.Split('-')[0].Replace("v", "")}\n{(ServerAddress.SignalAddress.Contains("dev") ? "DEV" : "PROD")}　{(ServerAddress.Version.Contains("standalone") ? "STANDALONE" : ServerAddress.Version.Contains("handbuild") ? "HAND-BUILD" : "")}");
        }

        private string GetNormalString()
        {
            if (string.IsNullOrEmpty(Retsuban)) return GetAvailableChar($"レツバン ミセッテイ\nセッテイシテクダサイ");
            if (Retsuban == "9999") return GetAvailableChar($"9999 フテイリョウスウ\nダミーレツバン\n");
            if (string.IsNullOrEmpty(Car)) return GetAvailableChar($"リョウスウ ミセッテイ\nセッテイシテクダサイ");
            return GetAvailableChar($"{Retsuban} {Car}リョウ\n{StopPassManager.TypeNameKana}\nクカンタイオウチュウ");
        }

        /// <summary>
        /// LCDに表示可能な文字列へ変換する
        /// ①全角カタカナのうち、濁点・半濁点を、濁点・半濁点なしの文字+濁点・半濁点に分離する。
        /// ②全角数値を半角数値に変換する。
        /// ③全角アルファベットを半角アルファベットに変換する。
        /// </summary>
        private string GetAvailableChar(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            // ① 全角カタカナの濁点・半濁点分離
            // 濁点・半濁点対応表
            var dakutenMap = new Dictionary<char, char>
            {
                {'ガ','カ'}, {'ギ','キ'}, {'グ','ク'}, {'ゲ','ケ'}, {'ゴ','コ'},
                {'ザ','サ'}, {'ジ','シ'}, {'ズ','ス'}, {'ゼ','セ'}, {'ゾ','ソ'},
                {'ダ','タ'}, {'ヂ','チ'}, {'ヅ','ツ'}, {'デ','テ'}, {'ド','ト'},
                {'バ','ハ'}, {'ビ','ヒ'}, {'ブ','フ'}, {'ベ','ヘ'}, {'ボ','ホ'},
                {'ヴ','ウ'}
            };
            var handakutenMap = new Dictionary<char, char>
            {
                {'パ','ハ'}, {'ピ','ヒ'}, {'プ','フ'}, {'ペ','ヘ'}, {'ポ','ホ'}
            };
            var result = new StringBuilder();

            foreach (var c in str)
            {
                // ① カタカナ濁点・半濁点分離
                if (dakutenMap.ContainsKey(c))
                {
                    result.Append(dakutenMap[c]);
                    result.Append('゛'); // U+309B
                }
                else if (handakutenMap.ContainsKey(c))
                {
                    result.Append(handakutenMap[c]);
                    result.Append('゜'); // U+309C
                }
                // ② 全角数字→半角数字
                else if (c >= '０' && c <= '９')
                {
                    result.Append((char)('0' + (c - '０')));
                }
                // ③ 全角英大文字→半角英大文字
                else if (c >= 'Ａ' && c <= 'Ｚ')
                {
                    result.Append((char)('A' + (c - 'Ａ')));
                }
                // ③ 全角英小文字→半角英小文字
                else if (c >= 'ａ' && c <= 'ｚ')
                {
                    result.Append((char)('a' + (c - 'ａ')));
                }
                else if (c == '.')
                {
                    result.Append('．');
                }
                else if (c == '-')
                {
                    result.Append('ー');
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// LCDFont画像内からその文字の画像を取得するメソッド
        /// </summary>
        /// <param name="str">対象文字</param>
        /// <returns>対象文字の画像(20*35)</returns>
        private Bitmap GetLCDFontImageByChar(string str)
        {
            if (string.IsNullOrEmpty(str) || !LCDFontList.Contains(str))
            {
                str = "?";
            }
            int index = LCDFontList.IndexOf(str);
            // 1行20文字
            int x = (index % 20) * 6 + 1;
            int y = (index / 20) * 8 + 1;
            return EnlargePixelArt(GetLCDFontImageByPos(x, y));
        }

        /// <summary>
        /// 指定した座標とサイズに基づいて画像を切り出す
        /// </summary>
        /// <param name="number">切り出す画像の番号</param>
        /// <returns>切り出された画像</returns>
        private Bitmap GetLCDFontImageByPos(int x, int y, int width = 5, int height = 7)
        {
            Bitmap croppedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(RetsubanResource.LCD_Font, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            }
            return croppedImage;
        }

        /// <summary>
        /// ピクセルアートを4倍に拡大する
        /// </summary>
        /// <param name="original">元の画像</param>
        /// <returns>4倍に拡大された画像</returns>
        private Bitmap EnlargePixelArt(Bitmap original)
        {
            int newWidth = original.Width * 4;
            int newHeight = original.Height * 4;

            Bitmap enlargedImage = new Bitmap(newWidth + 1, newHeight + 1);
            using (Graphics g = Graphics.FromImage(enlargedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                for (int y = 0; y < original.Height; y++)
                {
                    for (int x = 0; x < original.Width; x++)
                    {
                        Color pixelColor = original.GetPixel(x, y);
                        for (int dy = 0; dy < 4; dy++)
                        {
                            for (int dx = 0; dx < 4; dx++)
                            {
                                enlargedImage.SetPixel(x * 4 + dx, y * 4 + dy, pixelColor);
                            }
                        }
                    }
                }
            }
            return enlargedImage;
        }

        internal void Buttons_Func(string Name)
        {
            switch (Name)
            {
                case "Set":
                    if (nowUnkoSetting == 1)
                    {
                        if (nowInput is not "" or "特急" or "C特")
                        {
                            StopPassManager.TypeName = nowInput;
                            StopPassManager.TypeNameTC = StopPassManager.TypeStringTC(StopPassManager.TypeName);
                            StopPassManager.TypeNameKana = StopPassManager.TypeStringKana(StopPassManager.TypeName);
                            StopPassManager.TypeToStop();
                            Debug.WriteLine(StopPassManager);
                            beep2.PlayOnce(1.0f);
                            nowUnkoSetting = 0;
                        }
                    }
                    return;
                case "Del":
                    if (nowUnkoSetting == 1)
                    {
                        if (nowInput is "C特1" or "C特2" or "C特3" or "C特4")
                        {
                            nowInput = "C特";
                            beep1.PlayOnce(1.0f);
                        }
                        else if (nowInput is "A特" or "B特" or "C特" or "D特")
                        {
                            nowInput = "特急";
                            beep1.PlayOnce(1.0f);
                        }
                        else if (nowInput is "")
                        {
                            beep3.PlayOnce(1.0f);
                        }
                        else
                        {
                            nowInput = "";
                            beep1.PlayOnce(1.0f);
                        }
                    }
                    return;
                case "Clear":
                    if (nowUnkoSetting >= 1)
                    {
                        nowUnkoSetting = 0;
                    }
                    return;
                case "VerDisplay":
                    nowVerDisplay = !nowVerDisplay;
                    nowUnkoSetting = -1;
                    nowStopSetting = -1;
                    beep1.PlayOnce(1.0f);
                    return;
                case "UnkoSet":
                    nowVerDisplay = false;
                    nowUnkoSetting = 0;
                    nowStopSetting = -1;
                    nowInput = "";
                    beep1.PlayOnce(1.0f);
                    return;
                case "StopSet":
                    nowVerDisplay = false;
                    nowUnkoSetting = -1;
                    nowStopSetting = 0;
                    nowStopCount = 1;
                    beep1.PlayOnce(1.0f);
                    return;
                case "RetsuSet":
                case "CarSet":
                case "TimeSet":
                    nowVerDisplay = false;
                    nowUnkoSetting = -1;
                    nowStopSetting = -1;
                    return;
            }
        }

        internal void Buttons_Digit(string Digit)
        {
            if (nowUnkoSetting == 0)
            {
                switch (Digit)
                {
                    case "1":
                        nowUnkoSetting = 1;
                        beep1.PlayOnce(1.0f);
                        return;
                    case "2":
                        nowUnkoSetting = 2;
                        beep1.PlayOnce(1.0f);
                        return;
                    case "3":
                        nowUnkoSetting = 3;
                        beep1.PlayOnce(1.0f);
                        return;
                }
            }
            else if (nowUnkoSetting == 1)
            {
                if (nowInput == "C特")
                {
                    switch (Digit)
                    {
                        case "1":
                            nowInput = "C特1";
                            beep1.PlayOnce(1.0f);
                            return;
                        case "2":
                            nowInput = "C特2";
                            beep1.PlayOnce(1.0f);
                            return;
                        case "3":
                            nowInput = "C特3";
                            beep1.PlayOnce(1.0f);
                            return;
                        case "4":
                            nowInput = "C特4";
                            beep1.PlayOnce(1.0f);
                            return;
                    }
                }
            }
        }
        internal void Buttons_StopPass(string Digit)
        {
            if (nowUnkoSetting == 1 && Digit == "停")
            {
                nowInput = "普通";
            }
        }
        internal void Buttons_RetsuTailType(string Name)
        {
            if (nowUnkoSetting == 1)
            {
                switch (Name)
                {
                    case "A":
                        nowInput = nowInput == "特急" ? "A特" : "特急";
                        beep1.PlayOnce(1.0f);
                        break;
                    case "B":
                        nowInput = nowInput == "特急" ? "B特" : "急行";
                        beep1.PlayOnce(1.0f);
                        break;
                    case "C":
                        nowInput = nowInput == "特急" ? "C特" : "準急";
                        beep1.PlayOnce(1.0f);
                        break;
                    case "D":
                        nowInput = nowInput == "特急" ? "D特" : "区急";
                        beep1.PlayOnce(1.0f);
                        break;
                    case "K":
                        nowInput = "快速急行";
                        beep1.PlayOnce(1.0f);
                        break;
                }
            }
        }
        internal void Buttons_RetsuTailOther(string Name)
        {
            if (nowUnkoSetting == 1)
            {
                switch (Name)
                {
                    case "特":
                        nowInput = "特急";
                        break;
                }
            }
        }
    }
}
