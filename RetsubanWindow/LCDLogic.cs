using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TakumiteAudioWrapper;
using TrainCrewAPI;

namespace TatehamaATS_v1.RetsubanWindow
{
    internal class LCDLogic
    {

        private List<string> LCDFontList = new List<string>();

        private bool nowUnkoSetting = false;
        private bool nowStopSetting = false;
        private int nowStopCount = 0;

        private bool nowVerDisplay = false;

        private PictureBox LCD { get; set; }
        private string Retsuban;
        private string Car;

        private AudioManager AudioManager;
        private AudioWrapper beep1;
        private AudioWrapper beep2;
        private AudioWrapper beep3;

        private string しあしあ欲しいものリスト公開待機;

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

            //表示文字列があるタイプ
            string displayString;
            if (nowVerDisplay)
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

            var displayList = new List<string>();
            for (int i = 0; i < displayStringList.Count; i++)
            {
                displayList.SetDisplayData(displayStringList[i], 0, i, false); // 横書きで配置
            }

            return displayList;
        }

        private string GetUnkoString()
        {
            return GetAvailableChar("シュベツ:" + ServerAddress.Version.Split('-')[0].Replace("v", "") + "\n" + (ServerAddress.Version.Contains("de") ? "DEV" : "PROD") + (ServerAddress.Version.Contains("standalone") ? "　STANDALONE" : ServerAddress.Version.Contains("handbuild") ? "　HAND-BUILD" : ""));
        }

        private string GetVerString()
        {
            return GetAvailableChar("ソフトバージョン\nV." + ServerAddress.Version.Split('-')[0].Replace("v", "") + "\n" + (ServerAddress.Version.Contains("de") ? "DEV" : "PROD") + (ServerAddress.Version.Contains("standalone") ? "　STANDALONE" : ServerAddress.Version.Contains("handbuild") ? "　HAND-BUILD" : ""));
        }

        private string GetNormalString()
        {
            if (string.IsNullOrEmpty(Retsuban)) return GetAvailableChar($"レツバン ミセッテイ\nセッテイシテクダサイ");
            if (Retsuban == "9999") return GetAvailableChar($"9999 フテイリョウスウ\nダミーレツバン\n");
            if (string.IsNullOrEmpty(Car)) return GetAvailableChar($"リョウスウ ミセッテイ\nセッテイシテクダサイ");
            return GetAvailableChar($"{Retsuban} {Car}リョウ\nレッシャジョウホウヒョウジ\nタイオウサギョウチュウ");
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
                    return;
                case "Del":
                    return;
                case "Clear":
                    return;
                case "VerDisplay":
                    nowVerDisplay = !nowVerDisplay;
                    beep1.PlayOnce(1.0f);
                    return;
                case "UnkoSet":
                    nowVerDisplay = false;
                    nowUnkoSetting = true;
                    nowStopSetting = false;
                    beep1.PlayOnce(1.0f);
                    return;
                case "StopSet":
                    nowVerDisplay = false;
                    nowUnkoSetting = false;
                    nowStopSetting = true;
                    beep1.PlayOnce(1.0f);
                    return;
                case "RetsuSet":
                case "CarSet":
                case "TimeSet":
                    nowVerDisplay = false;
                    nowUnkoSetting = false;
                    nowStopSetting = false;
                    return;
            }
        }
    }
}
