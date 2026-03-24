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
using TatehamaATS_v1.OnboardDevice;
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

    public partial class RetsubanWindow : Form {
        private RetsubanLogic retsubanLogic;
        private TimeLogic timeLogic;
        private LCDLogic LCDLogic;

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        /// <summary>
        /// 設定情報変更
        /// </summary>
        internal event Action<string> SetDiaNameAction;
        internal event Action<TimeSpan> SetShiftTime;

        internal RetsubanWindow(StopPassManager stopPassManager) {
            InitializeComponent();
            this.Load += Loaded;
            TopMost = true;

            retsubanLogic = new RetsubanLogic(Retsuban_Head, new PictureBox[] { Retsuban_4, Retsuban_3, Retsuban_2, Retsuban_1 }, Retsuban_Tail, Car_2, Car_1);
            timeLogic = new TimeLogic(Time_h2, Time_h1, Time_m2, Time_m1, Time_s2, Time_s1);
            LCDLogic = new LCDLogic(LCD) { StopPassManager = stopPassManager };
            retsubanLogic.SetCarAction += LCDLogic.SetCar;

            retsubanLogic.SetDiaNameAction += OnSetDiaName;
            timeLogic.SetShiftTime += OnSetShiftTime;
        }

        private void OnSetDiaName(string retsuban) {
            SetDiaNameAction?.Invoke(retsuban); // 外部への伝播
            LCDLogic?.SetRetsuban(retsuban);     // LCDLogicへの伝播
        }

        private void OnSetShiftTime(TimeSpan shiftTime) {
            SetShiftTime?.Invoke(shiftTime);
        }

        private void Loaded(object sender, EventArgs e) {
            RetsubanDrawing();
        }

        private void RetsubanWindow_FormClosing(object sender, FormClosingEventArgs e) {
            //閉じずに消す
            Hide();
            e.Cancel = true;
        }

        private void LampDrawing() {
            if (timeLogic == null) {
                return;
            }
            Lamp_Retsuban.Visible = retsubanLogic.nowRetsuSetting;
            Lamp_Car.Visible = retsubanLogic.nowCarSetting;
            Lamp_Time.Visible = timeLogic.nowSetting;
        }


        private void ClockTimer_Tick(object sender, EventArgs e) {
            if (timeLogic == null) {
                return;
            }
            timeLogic.ClockTimer_Tick();
            LCDLogic.ClockTimer_Tick();
            LampDrawing();
        }

        /// <summary>
        /// 列番部描画
        /// </summary>
        private void RetsubanDrawing() {
            retsubanLogic.RetsubanDrawing();
        }


        private void Buttons_Click(string Name, ButtonType buttonType) {
            Debug.WriteLine($"押下：{Name}/{buttonType}");
            switch (buttonType) {
                case ButtonType.Function:
                    retsubanLogic.Buttons_Func(Name);
                    timeLogic.Buttons_Func(Name);
                    LCDLogic.Buttons_Func(Name);
                    break;
                case ButtonType.Digit:
                    retsubanLogic.Buttons_Digit(Name);
                    timeLogic.Buttons_Digit(Name);
                    LCDLogic.Buttons_Digit(Name);
                    break;
                case ButtonType.StopPass:
                    LCDLogic.Buttons_StopPass(Name);
                    break;
                case ButtonType.RetsuHead:
                    retsubanLogic.Buttons_RetsuHead(Name);
                    LCDLogic.Buttons_RetsuHead(Name);
                    break;
                case ButtonType.RetsuTailType:
                    retsubanLogic.Buttons_RetsuTailType(Name);
                    LCDLogic.Buttons_RetsuTailType(Name);
                    break;
                case ButtonType.RetsuTailCompany:
                    retsubanLogic.Buttons_RetsuTailCompany(Name);
                    break;
                case ButtonType.RetsuTailOther:
                    retsubanLogic.Buttons_RetsuTailOther(Name);
                    LCDLogic.Buttons_RetsuTailOther(Name);
                    break;
                case ButtonType.OtherInput:
                    break;
                default:
                    break;
            }
        }

        private void Button_RetsuSet_Click(object sender, EventArgs e) {
            Buttons_Click("RetsuSet", ButtonType.Function);
        }

        private void Button_CarSet_Click(object sender, EventArgs e) {
            Buttons_Click("CarSet", ButtonType.Function);
        }

        private void Button_TimeSet_Click(object sender, EventArgs e) {
            Buttons_Click("TimeSet", ButtonType.Function);
        }

        private void Button_UnkoSet_Click(object sender, EventArgs e) {
            Buttons_Click("UnkoSet", ButtonType.Function);
        }

        private void Button_StopSet_Click(object sender, EventArgs e) {
            Buttons_Click("StopSet", ButtonType.Function);
        }

        private void Button_VerDisplay_Click(object sender, EventArgs e) {
            Buttons_Click("VerDisplay", ButtonType.Function);
        }

        private void Button_A_Click(object sender, EventArgs e) {
            Buttons_Click("A", ButtonType.RetsuTailType);
        }

        private void Button_B_Click(object sender, EventArgs e) {
            Buttons_Click("B", ButtonType.RetsuTailType);
        }

        private void Button_C_Click(object sender, EventArgs e) {
            Buttons_Click("C", ButtonType.RetsuTailType);
        }

        private void Button_D_Click(object sender, EventArgs e) {
            Buttons_Click("D", ButtonType.RetsuTailType);
        }

        private void Button_K_Click(object sender, EventArgs e) {
            Buttons_Click("K", ButtonType.RetsuTailType);
        }

        private void Button_S_Click(object sender, EventArgs e) {
            Buttons_Click("S", ButtonType.RetsuTailOther);
        }

        private void Button_T_Click(object sender, EventArgs e) {
            Buttons_Click("T", ButtonType.RetsuTailOther);
        }

        private void Button_X_Click(object sender, EventArgs e) {
            Buttons_Click("X", ButtonType.RetsuTailOther);
        }

        private void Button_Y_Click(object sender, EventArgs e) {
            Buttons_Click("Y", ButtonType.RetsuTailOther);
        }

        private void Button_Z_Click(object sender, EventArgs e) {
            Buttons_Click("Z", ButtonType.RetsuTailOther);
        }
        private void Button_Danjiri_Click(object sender, EventArgs e) {
            Buttons_Click("だんじり", ButtonType.RetsuTailOther);
        }

        private void Button_Toku_Click(object sender, EventArgs e) {
            Buttons_Click("特", ButtonType.RetsuTailOther);
        }

        private void Button_Kai_Click(object sender, EventArgs e) {
            Buttons_Click("回", ButtonType.RetsuHead);
        }

        private void Button_Shi_Click(object sender, EventArgs e) {
            Buttons_Click("試", ButtonType.RetsuHead);
        }

        private void Button_Rin_Click(object sender, EventArgs e) {
            Buttons_Click("臨", ButtonType.RetsuHead);
        }

        private void Button_0_Click(object sender, EventArgs e) {
            Buttons_Click("0", ButtonType.Digit);
        }

        private void Button_1_Click(object sender, EventArgs e) {
            Buttons_Click("1", ButtonType.Digit);
        }

        private void Button_2_Click(object sender, EventArgs e) {
            Buttons_Click("2", ButtonType.Digit);
        }

        private void Button_3_Click(object sender, EventArgs e) {
            Buttons_Click("3", ButtonType.Digit);
        }

        private void Button_4_Click(object sender, EventArgs e) {
            Buttons_Click("4", ButtonType.Digit);
        }

        private void Button_5_Click(object sender, EventArgs e) {
            Buttons_Click("5", ButtonType.Digit);
        }

        private void Button_6_Click(object sender, EventArgs e) {
            Buttons_Click("6", ButtonType.Digit);
        }

        private void Button_7_Click(object sender, EventArgs e) {
            Buttons_Click("7", ButtonType.Digit);
        }

        private void Button_8_Click(object sender, EventArgs e) {
            Buttons_Click("8", ButtonType.Digit);
        }

        private void Button_9_Click(object sender, EventArgs e) {
            Buttons_Click("9", ButtonType.Digit);
        }

        private void Button_Tei_Click(object sender, EventArgs e) {
            Buttons_Click("停", ButtonType.StopPass);
        }

        private void Button_Tsu_Click(object sender, EventArgs e) {
            Buttons_Click("通", ButtonType.StopPass);
        }

        private void Button_Set_Click(object sender, EventArgs e) {
            Buttons_Click("Set", ButtonType.Function);
        }

        private void Button_Del_Click(object sender, EventArgs e) {
            Buttons_Click("Del", ButtonType.Function);
        }

        private void Button_Clear_Click(object sender, EventArgs e) {
            Buttons_Click("Clear", ButtonType.Function);
        }

        private void RetsubanWindow_KeyDown(object sender, KeyEventArgs e) {
            Debug.WriteLine($"keycode: {e.KeyValue}");
            switch (e.KeyCode) {
                case Keys.Enter:
                    Buttons_Click("Set", ButtonType.Function);
                    break;
                case Keys.Back:
                case Keys.Delete:
                    Buttons_Click("Del", ButtonType.Function);
                    break;
                case Keys.Escape:
                    Buttons_Click("Clear", ButtonType.Function);
                    break;
                case Keys.D0:
                case Keys.NumPad0:
                    Buttons_Click("0", ButtonType.Digit);
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    Buttons_Click("1", ButtonType.Digit);
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    Buttons_Click("2", ButtonType.Digit);
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    Buttons_Click("3", ButtonType.Digit);
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    Buttons_Click("4", ButtonType.Digit);
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    Buttons_Click("5", ButtonType.Digit);
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    Buttons_Click("6", ButtonType.Digit);
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    Buttons_Click("7", ButtonType.Digit);
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    Buttons_Click("8", ButtonType.Digit);
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    Buttons_Click("9", ButtonType.Digit);
                    break;
                case Keys.A:
                    Buttons_Click("A", ButtonType.RetsuTailType);
                    break;
                case Keys.B:
                    Buttons_Click("B", ButtonType.RetsuTailType);
                    break;
                case Keys.C:
                    Buttons_Click("C", ButtonType.RetsuTailType);
                    break;
                case Keys.D:
                    Buttons_Click("D", ButtonType.RetsuTailType);
                    break;
                case Keys.K:
                    Buttons_Click("K", ButtonType.RetsuTailType);
                    break;
                case Keys.S:
                    Buttons_Click("S", ButtonType.RetsuTailOther);
                    break;
                case Keys.T:
                    Buttons_Click("T", ButtonType.RetsuTailOther);
                    break;
                case Keys.X:
                    Buttons_Click("X", ButtonType.RetsuTailOther);
                    break;
                case Keys.Y:
                    Buttons_Click("Y", ButtonType.RetsuTailOther);
                    break;
                case Keys.Z:
                    Buttons_Click("Z", ButtonType.RetsuTailOther);
                    break;
                case Keys.J:
                    Buttons_Click("だんじり", ButtonType.RetsuTailOther);
                    break;
                case Keys.L:
                    Buttons_Click("特", ButtonType.RetsuTailOther);
                    break;
                case Keys.OemMinus:
                    Buttons_Click("回", ButtonType.RetsuHead);
                    break;
                case Keys.OemQuotes:
                    Buttons_Click("試", ButtonType.RetsuHead);
                    break;
                case Keys.OemPipe:
                    Buttons_Click("臨", ButtonType.RetsuHead);
                    break;
                case Keys.Oemtilde:
                    Buttons_Click("RetsuSet", ButtonType.Function);
                    break;
                case Keys.OemOpenBrackets:
                    Buttons_Click("CarSet", ButtonType.Function);
                    break;
                case Keys.Oemplus:
                    Buttons_Click("TimeSet", ButtonType.Function);
                    break;
                case Keys.OemSemicolon:
                    Buttons_Click("UnkoSet", ButtonType.Function);
                    break;
                case Keys.OemCloseBrackets:
                    Buttons_Click("StopSet", ButtonType.Function);
                    break;
                case Keys.V:
                    Buttons_Click("VerDisplay", ButtonType.Function);
                    break;
                case Keys.O:
                    Buttons_Click("停", ButtonType.StopPass);
                    break;
                case Keys.P:
                    Buttons_Click("通", ButtonType.StopPass);
                    break;
            }
        }
    }
}
