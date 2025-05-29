using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using TakumiteAudioWrapper;
using TatehamaATS_v1.Exceptions;
using TrainCrewAPI;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Image = System.Drawing.Image;


namespace TatehamaATS_v1.RetsubanWindow
{
    enum RetsubanMode
    {
        None,
        RetsubanHead,
        RetsubanDigit,
        RetsubanTail,
        Car,
        Time,
        Unko
    }

    public partial class RetsubanWindow : Form
    {
        private RetsubanLogic retsubanLogic;
        private TimeLogic timeLogic;


        private List<string> LCDFontList = new List<string>();

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        /// <summary>
        /// 設定情報変更
        /// </summary>
        internal event Action<string> SetDiaNameAction;

        public RetsubanWindow()
        {
            InitializeComponent();
            this.Load += Loaded;
            TopMost = true;

            var lcdfontString =
                " ，．・：；？！”｜…（）［］＜＞←→＠" +
                "0123456789回試臨xyz■特停通" +
                "ABCDEFGHIJKLMNOPQRST" +
                "UVWXYZŪŌァィゥェォヵヶャュョッー" +
                "アイウエオカキクケコサシスセソタチツテト" +
                "ナニヌネノハヒフヘホマミムメモヤ　ユ　ヨ" +
                "ラリルレロワヰヱヲン゛゜　　　　　　　?";

            // LCDFontListにlcdfontStringを1文字ずつ追加
            foreach (char c in lcdfontString)
            {
                LCDFontList.Add(c.ToString());
            }
            retsubanLogic = new RetsubanLogic(Retsuban_Head, new PictureBox[] { Retsuban_4, Retsuban_3, Retsuban_2, Retsuban_1 }, Retsuban_Tail, Car_2, Car_1);
            timeLogic = new TimeLogic(Time_h2, Time_h1, Time_m2, Time_m1, Time_s2, Time_s1);
            //set_trainnum?.PlayLoop(1.0f);
            LCDDrawing();
        }

        private void Loaded(object sender, EventArgs e)
        {
            RetsubanDrawing();
        }

        private void RetsubanWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //閉じずに消す
            Hide();
            e.Cancel = true;
        }

        private void LampDrawing()
        {
            if (timeLogic == null)
            {
                return;
            }
            Lamp_Retsuban.Visible = retsubanLogic.nowRetsuSetting;
            Lamp_Car.Visible = retsubanLogic.nowCarSetting;
            Lamp_Time.Visible = timeLogic.nowSetting;
        }


        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            if (timeLogic == null)
            {
                return;
            }
            LCDDrawing();
            timeLogic.ClockTimer_Tick();
            LampDrawing();
        }

        /// <summary>
        /// 列番部描画
        /// </summary>
        private void RetsubanDrawing()
        {
            retsubanLogic.RetsubanDrawing();
        }

        private void LCDDrawing()
        {
            var displayList = GetLCDData();

            // 画像を取得してリストに格納
            List<Bitmap> lcdImages = new List<Bitmap>();
            foreach (var str in displayList)
            {
                lcdImages.Add(GetLCDFontImageByChar(str));
            }

            // 画像をLCD領域にならべる
            var NewLCD = new Bitmap(LCD.Width, LCD.Height);
            // 起点を5,5として、xは24、yは32ごとに並べる。横は15文字制限
            using (Graphics g = Graphics.FromImage(NewLCD))
            {
                int x = 5;
                int y = 5;
                for (int i = 0; i < lcdImages.Count; i++)
                {
                    if (i % 15 == 0 && i != 0) // 15文字ごとに改行
                    {
                        x = 5; // xをリセット
                        y += 32; // yを次の行に移動
                    }
                    g.DrawImage(lcdImages[i], x, y, 20, 28); // サイズは24x32で描画
                    x += 24; // 次の文字の位置へ移動
                }
            }
            //描画する
            LCD.BackgroundImage = NewLCD;
        }

        private List<string> GetLCDData()
        {
            var displayString = GetAvailableChar("ヒョウジテスト　フォント試\n臨回0123456789xyz\nカナ英数字ト特殊文字ノミ表示可");

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

        private void Buttons_Click(string Name, ButtonType buttonType)
        {
            Debug.WriteLine($"押下：{Name}");
            switch (buttonType)
            {
                case ButtonType.Function:
                    retsubanLogic.Buttons_Func(Name);
                    timeLogic.Buttons_Func(Name);
                    break;
                case ButtonType.Digit:
                    retsubanLogic.Buttons_Digit(Name);
                    timeLogic.Buttons_Digit(Name);
                    break;
                case ButtonType.StopPass:
                    break;
                case ButtonType.RetsuHead:
                    retsubanLogic.Buttons_RetsuHead(Name);
                    break;
                case ButtonType.RetsuTailType:
                    retsubanLogic.Buttons_RetsuTailType(Name);
                    break;
                case ButtonType.RetsuTailCompany:
                    retsubanLogic.Buttons_RetsuTailCompany(Name);
                    break;
                case ButtonType.RetsuTailOther:
                    retsubanLogic.Buttons_RetsuTailOther(Name);
                    break;
                case ButtonType.OtherInput:
                    break;
                default:
                    break;
            }
        }

        private void Button_RetsuSet_Click(object sender, EventArgs e)
        {
            Buttons_Click("RetsuSet", ButtonType.Function);
        }

        private void Button_CarSet_Click(object sender, EventArgs e)
        {
            Buttons_Click("CarSet", ButtonType.Function);
        }

        private void Button_TimeSet_Click(object sender, EventArgs e)
        {
            Buttons_Click("TimeSet", ButtonType.Function);
        }

        private void Button_UnkoSet_Click(object sender, EventArgs e)
        {
            Buttons_Click("UnkoSet", ButtonType.Function);
        }

        private void Button_StopSet_Click(object sender, EventArgs e)
        {
            Buttons_Click("StopSet", ButtonType.Function);
        }

        private void Button_VerDisplay_Click(object sender, EventArgs e)
        {
            Buttons_Click("VerDisplay", ButtonType.Function);
        }

        private void Button_A_Click(object sender, EventArgs e)
        {
            Buttons_Click("A", ButtonType.RetsuTailType);
        }

        private void Button_B_Click(object sender, EventArgs e)
        {
            Buttons_Click("B", ButtonType.RetsuTailType);
        }

        private void Button_C_Click(object sender, EventArgs e)
        {
            Buttons_Click("C", ButtonType.RetsuTailType);
        }

        private void Button_D_Click(object sender, EventArgs e)
        {
            Buttons_Click("D", ButtonType.RetsuTailType);
        }

        private void Button_K_Click(object sender, EventArgs e)
        {
            Buttons_Click("K", ButtonType.RetsuTailType);
        }

        private void Button_S_Click(object sender, EventArgs e)
        {
            Buttons_Click("S", ButtonType.RetsuTailOther);
        }

        private void Button_T_Click(object sender, EventArgs e)
        {
            Buttons_Click("T", ButtonType.RetsuTailOther);
        }

        private void Button_X_Click(object sender, EventArgs e)
        {
            Buttons_Click("X", ButtonType.RetsuTailOther);
        }

        private void Button_Y_Click(object sender, EventArgs e)
        {
            Buttons_Click("Y", ButtonType.RetsuTailOther);
        }

        private void Button_Z_Click(object sender, EventArgs e)
        {
            Buttons_Click("Z", ButtonType.RetsuTailOther);
        }
        private void Button_Danjiri_Click(object sender, EventArgs e)
        {
            Buttons_Click("だんじり", ButtonType.RetsuTailOther);
        }

        private void Button_Toku_Click(object sender, EventArgs e)
        {
            Buttons_Click("特", ButtonType.RetsuTailOther);
        }

        private void Button_Kai_Click(object sender, EventArgs e)
        {
            Buttons_Click("回", ButtonType.RetsuHead);
        }

        private void Button_Shi_Click(object sender, EventArgs e)
        {
            Buttons_Click("試", ButtonType.RetsuHead);
        }

        private void Button_Rin_Click(object sender, EventArgs e)
        {
            Buttons_Click("臨", ButtonType.RetsuHead);
        }

        private void Button_0_Click(object sender, EventArgs e)
        {
            Buttons_Click("0", ButtonType.Digit);
        }

        private void Button_1_Click(object sender, EventArgs e)
        {
            Buttons_Click("1", ButtonType.Digit);
        }

        private void Button_2_Click(object sender, EventArgs e)
        {
            Buttons_Click("2", ButtonType.Digit);
        }

        private void Button_3_Click(object sender, EventArgs e)
        {
            Buttons_Click("3", ButtonType.Digit);
        }

        private void Button_4_Click(object sender, EventArgs e)
        {
            Buttons_Click("4", ButtonType.Digit);
        }

        private void Button_5_Click(object sender, EventArgs e)
        {
            Buttons_Click("5", ButtonType.Digit);
        }

        private void Button_6_Click(object sender, EventArgs e)
        {
            Buttons_Click("6", ButtonType.Digit);
        }

        private void Button_7_Click(object sender, EventArgs e)
        {
            Buttons_Click("7", ButtonType.Digit);
        }

        private void Button_8_Click(object sender, EventArgs e)
        {
            Buttons_Click("8", ButtonType.Digit);
        }

        private void Button_9_Click(object sender, EventArgs e)
        {
            Buttons_Click("9", ButtonType.Digit);
        }

        private void Button_Tei_Click(object sender, EventArgs e)
        {
            Buttons_Click("停", ButtonType.Digit);
        }

        private void Button_Tsu_Click(object sender, EventArgs e)
        {
            Buttons_Click("通", ButtonType.Digit);
        }

        private void Button_Set_Click(object sender, EventArgs e)
        {
            Buttons_Click("Set", ButtonType.Function);
        }

        private void Button_Del_Click(object sender, EventArgs e)
        {
            Buttons_Click("Del", ButtonType.Function);
        }

        private void Button_Clear_Click(object sender, EventArgs e)
        {
            Buttons_Click("Clear", ButtonType.Function);
        }
    }

    public static class ListStringExtensions
    {
        /// <summary>
        /// 文字配置を行う拡張メソッド
        /// </summary>
        /// <param name="list">対象リスト</param>
        /// <param name="str">設定文字列</param>
        /// <param name="x">横の文字位置</param>
        /// <param name="y">縦の文字位置</param>
        /// <param name="isY">縦書きかどうか</param>
        /// <returns>変更後のリスト</returns>
        public static List<string> SetDisplayData(this List<string> list, string str, int x, int y, bool isY)
        {
            // 開始位置を求める
            if (x < 0 || y < 0 || x > 15 || y > 3)
            {
                throw new ArgumentOutOfRangeException("x or y is out of range.");
            }
            var startPosition = y * 15 + x;
            if (isY) // 縦書きの場合
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int index = startPosition + i * 15;
                    // 存在しないインデックスの場合、Listのサイズを拡張する
                    if (index >= list.Count)
                    {
                        for (int j = list.Count; j <= index; j++)
                        {
                            list.Add(" "); // 空白で埋める
                        }
                    }
                    list[index] = str[i].ToString();
                }
            }
            else // 横書きの場合
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int index = startPosition + i;
                    // 存在しないインデックスの場合、Listのサイズを拡張する
                    if (index >= list.Count)
                    {
                        for (int j = list.Count; j <= index; j++)
                        {
                            list.Add(" "); // 空白で埋める
                        }
                    }
                    list[index] = str[i].ToString();
                }
            }
            return list;
        }
    }
}
