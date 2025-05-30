using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TakumiteAudioWrapper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TatehamaATS_v1.RetsubanWindow
{
    internal class RetsubanLogic
    {
        public string Retsuban { get; set; }
        private string NewRetsuban { get; set; }
        public bool nowRetsuSetting;
        public bool nowCarSetting;
        private int Car { get; set; }
        private string NewCar { get; set; }
        private PictureBox Retsuban_Head { get; set; }
        private PictureBox[] Retsuban_7seg { get; set; }
        private PictureBox Retsuban_Tail { get; set; }
        private PictureBox Car_2 { get; set; }
        private PictureBox Car_1 { get; set; }
        private Dictionary<string, Image> Images_7seg { get; set; }

        /// <summary>
        /// 設定情報変更
        /// </summary>
        internal event Action<string> SetDiaNameAction;

        private AudioManager AudioManager;
        private AudioWrapper beep1;
        private AudioWrapper beep2;
        private AudioWrapper beep3;
        private AudioWrapper set_trainsetlen;
        private AudioWrapper set_trainnum;
        private AudioWrapper set_complete;

        internal RetsubanLogic(PictureBox retsuban_head, PictureBox[] retsuban_7seg, PictureBox retsuban_tail, PictureBox car_2, PictureBox car_1)
        {
            // 表示領域渡し
            Retsuban_Head = retsuban_head;
            Retsuban_7seg = retsuban_7seg;
            Retsuban_Tail = retsuban_tail;
            Car_2 = car_2;
            Car_1 = car_1;

            // 初期化
            Retsuban = "9999";
            Car = 0;
            NewRetsuban = "9999";
            NewCar = "";
            Images_7seg = new Dictionary<string, Image> {
                {" ",  RetsubanResource._7seg_N} ,
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
            };

            AudioManager = new AudioManager();
            beep1 = AudioManager.AddAudio("sound/beep1.wav", 1.0f);
            beep2 = AudioManager.AddAudio("sound/beep2.wav", 1.0f);
            beep3 = AudioManager.AddAudio("sound/beep3.wav", 1.0f);
            set_trainnum = AudioManager.AddAudio("sound/set_trainnum.wav", 1.0f);
            set_trainsetlen = AudioManager.AddAudio("sound/set_trainsetlen.wav", 1.0f);
            set_complete = AudioManager.AddAudio("sound/set_complete.wav", 1.0f);

            set_trainnum?.PlayLoop(1.0f);
            RetsubanDrawing();
        }

        /// <summary>
        /// 列番部描画
        /// </summary>
        public void RetsubanDrawing()
        {
            // 正規表現パターンの定義                                                       
            var pattern = @"^(回|試|臨)?([0-9]{0,4})(A|B|C|K|X|Y|Z|AX|BX|CX|KX|AY|BY|CY|KY|AZ|BZ|CZ|KZ)?$";
            var match = Regex.Match(NewRetsuban, pattern);

            if (match.Success)
            {
                // Head領域 - 回・試・臨のいずれか
                string head = match.Groups[1].Value;
                // Headに画像を描画
                // 描画処理: head画像をHead領域に配置
                //先頭文字                    
                // Headに画像を描画
                Retsuban_Head.Image = head switch
                {
                    "回" => RetsubanResource._16dot_Kai,
                    "試" => RetsubanResource._16dot_Shi,
                    "臨" => RetsubanResource._16dot_Rin,
                    _ => RetsubanResource._16dot_Null,
                };

                // 4~1領域 - 数字部分を右寄せで各桁に描画
                string digits = match.Groups[2].Value.PadLeft(4, ' '); // 数字を4桁に右寄せ、空白で埋める
                for (int i = 0; i < 4; i++)
                {
                    Retsuban_7seg[i].Image = Images_7seg[$"{digits[i]}"];
                }


                // Tail領域 - A,B,C,K,X,AX,BX,CX,KXのいずれか
                string tail = match.Groups[3].Value;
                // Tailに画像を描画
                // 描画処理: tail画像をTail領域に配置
                //接尾文字
                // 圧縮表記でTail領域描画
                Retsuban_Tail.Image = tail switch
                {
                    "A" => RetsubanResource._16dot_A,
                    "B" => RetsubanResource._16dot_B,
                    "C" => RetsubanResource._16dot_C,
                    "K" => RetsubanResource._16dot_K,
                    "X" => RetsubanResource._16dot_X,
                    "Y" => RetsubanResource._16dot_Y,
                    "Z" => RetsubanResource._16dot_Z,
                    "AX" => RetsubanResource._16dot_AX,
                    "BX" => RetsubanResource._16dot_BX,
                    "CX" => RetsubanResource._16dot_CX,
                    "KX" => RetsubanResource._16dot_KX,
                    "AY" => RetsubanResource._16dot_AY,
                    "BY" => RetsubanResource._16dot_BY,
                    "CY" => RetsubanResource._16dot_CY,
                    "KY" => RetsubanResource._16dot_KY,
                    "AZ" => RetsubanResource._16dot_AZ,
                    "BZ" => RetsubanResource._16dot_BZ,
                    "CZ" => RetsubanResource._16dot_CZ,
                    "KZ" => RetsubanResource._16dot_KZ,
                    _ => RetsubanResource._16dot_Null,
                };
            }
        }

        private void CarDrawing(int car)
        {
            CarDrawing(car.ToString());
        }

        private void CarDrawing(string car)
        {
            Debug.WriteLine($"carDraw{car}");
            car = car.PadLeft(2, ' ');
            if (car == " 0")
            {
                Car_2.BackgroundImage = Images_7seg[$" "];
                Car_1.BackgroundImage = Images_7seg[$" "];
            }
            else
            {
                Car_2.BackgroundImage = Images_7seg[$"{car[0]}"];
                Car_1.BackgroundImage = Images_7seg[$"{car[1]}"];
            }
        }

        internal void Buttons_Digit(string Digit)
        {
            // 自身が設定中でない場合入力スルーする
            if (!nowRetsuSetting && !nowCarSetting)
            {
                return;
            }
            if (nowRetsuSetting)
            {
                // 正規表現パターンの定義
                var pattern = @"^([回試臨]?)([0-9]{1,4})$";
                if (Regex.IsMatch(NewRetsuban + Digit, pattern))
                {
                    NewRetsuban += Digit;
                    beep1.PlayOnce(1.0f);
                    RetsubanDrawing();
                }
                return;
            }
            if (nowCarSetting)
            {
                if (NewCar == "" && (Digit != "0"))
                {
                    NewCar += Digit;
                    beep1.PlayOnce(1.0f);
                }
                else if (NewCar == "1" && Digit == "0")
                {
                    NewCar += Digit;
                    beep1.PlayOnce(1.0f);
                }
                CarDrawing(NewCar);
                return;
            }
        }

        internal void Buttons_RetsuHead(string Name)
        {
            // 自身が設定中でない場合入力スルーする
            if (!nowRetsuSetting)
            {
                return;
            }
            if (NewRetsuban == "")
            {
                NewRetsuban += Name;
                RetsubanDrawing();
                beep1.PlayOnce(1.0f);
            }
        }

        internal void Buttons_RetsuTailType(string Name)
        {
            // 自身が設定中でない場合入力スルーする
            if (!nowRetsuSetting)
            {
                return;
            }
            var pattern = @"^(回|試|臨)?([0-9]{3,4})$";
            // 正規表現パターンの定義
            if (Regex.IsMatch(NewRetsuban, pattern))
            {
                NewRetsuban += Name;
                beep1.PlayOnce(1.0f);
                RetsubanDrawing();
            }
        }

        internal void Buttons_RetsuTailCompany(string Name)
        {
            // 自身が設定中でない場合入力スルーする
            if (!nowRetsuSetting)
            {
                return;
            }
            var pattern = @"^(回|試|臨)?([0-9]{3,4})$";
            // 正規表現パターンの定義
            if (Regex.IsMatch(NewRetsuban, pattern))
            {
                NewRetsuban += Name;
                beep1.PlayOnce(1.0f);
                RetsubanDrawing();
            }
        }

        internal void Buttons_RetsuTailOther(string Name)
        {
            // 自身が設定中でない場合入力スルーする
            if (!nowRetsuSetting)
            {
                return;
            }
            var pattern = @"^(回|試|臨)?([0-9]{3,4})(A|B|C|K)?$";
            // 正規表現パターンの定義
            if (Regex.IsMatch(NewRetsuban, pattern))
            {
                NewRetsuban += Name;
                beep1.PlayOnce(1.0f);
                RetsubanDrawing();
            }
        }

        internal void Buttons_Func(string Name)
        {
            switch (Name)
            {
                case "Set":
                    if (nowRetsuSetting)
                    {
                        var pattern = @"^(回|試|臨)?([0-9]{3,4})(A|B|C|K|X|Y|Z|AX|BX|CX|KX|AY|BY|CY|KY|AZ|BZ|CZ|KZ)?$";
                        // 正規表現パターンの定義
                        if (Regex.IsMatch(NewRetsuban, pattern))
                        {
                            Retsuban = NewRetsuban;
                            SetDiaNameAction?.Invoke(Retsuban);
                            RetsubanDrawing();
                            set_trainnum?.Stop();
                            if (Car != 0 || Retsuban == "9999")
                            {
                                set_trainsetlen?.Stop();
                                set_complete.PlayOnce(1.0f);
                            }
                            else
                            {
                                set_trainsetlen.PlayLoop(1.0f);
                            }
                            nowRetsuSetting = false;
                            beep2.PlayOnce(1.0f);
                        }
                    }
                    else if (nowCarSetting)
                    {
                        if (Int32.TryParse(NewCar, out var newCar))
                        {
                            if (2 <= newCar && newCar <= 10)
                            {
                                Car = newCar;
                            }
                            CarDrawing(NewCar);
                            set_trainsetlen?.Stop();
                            if (Retsuban != null || Retsuban == "9999")
                            {
                                set_trainnum?.Stop();
                                set_complete.PlayOnce(1.0f);
                            }
                            else
                            {
                                set_trainnum.PlayLoop(1.0f);
                            }
                            nowCarSetting = false;
                            beep2.PlayOnce(1.0f);
                            return;
                        }
                    }
                    return;
                case "Del":
                    if (nowRetsuSetting)
                    {
                        if (string.IsNullOrEmpty(NewRetsuban))
                        {
                            RetsubanDrawing();
                            return;
                        }
                        NewRetsuban = NewRetsuban.Remove(NewRetsuban.Length - 1);
                        beep1.PlayOnce(1.0f);
                        RetsubanDrawing();
                        return;
                    }
                    else if (nowCarSetting)
                    {
                        if (string.IsNullOrEmpty(NewCar))
                        {
                            return;
                        }
                        NewCar = NewCar.Remove(NewCar.Length - 1);
                        beep1.PlayOnce(1.0f);
                        CarDrawing(NewCar);
                        return;
                    }
                    return;
                case "Clear":
                    if (nowRetsuSetting)
                    {
                        NewRetsuban = "";
                        beep2.PlayOnce(1.0f);
                        RetsubanDrawing();
                    }
                    else if (nowCarSetting)
                    {
                        NewCar = "";
                        beep2.PlayOnce(1.0f);
                        RetsubanDrawing();
                    }
                    return;
                case "RetsuSet":
                    nowRetsuSetting = true;
                    nowCarSetting = false;
                    NewRetsuban = "";
                    beep1.PlayOnce(1.0f);
                    RetsubanDrawing();
                    return;
                case "CarSet":
                    nowRetsuSetting = false;
                    nowCarSetting = true;
                    NewRetsuban = Retsuban;
                    NewCar = "";
                    beep1.PlayOnce(1.0f);
                    RetsubanDrawing();
                    return;
                case "TimeSet":
                case "UnkoSet":
                case "VerDisplay":
                    nowRetsuSetting = false;
                    nowCarSetting = false;
                    NewRetsuban = Retsuban;
                    RetsubanDrawing();
                    CarDrawing(Car);
                    return;
            }
        }
    }
}
