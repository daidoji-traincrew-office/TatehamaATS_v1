using System;
using System.Drawing;
using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using OpenIddict.Client;
using TatehamaATS_v1.OnboardDevice;


namespace TatehamaATS_v1.MainWindow
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    public partial class MainWindow : Form, IWinFormsShell
    {
        private const int HOTKEY_ID_F4 = 2; // F4キー用ホットキーID
        private bool bougoState = false; // トグル用の状態管理変数

        private Timer longPressTimer; // 長押し判定用
        private Timer resetImageTimer; // 画像差し替え期間管理用
        private bool isLongPressed = false;

        private int atsState = 0; // 0: 蓋アリ正常, 1: 蓋アリ開放, 2: 蓋ナシ正常, 3: 蓋ナシ開放
        private Timer lidTimer; // 0.5秒無操作で蓋が戻る                               
        private Timer cutLongPressTimer; // 0.3秒長押し判定用
        private bool isLongPressHandled = false; // 長押し処理を1回だけ実行するためのフラグ

        private CableIO CableIO;
        private RetsubanWindow.RetsubanWindow retsubanWindow = new RetsubanWindow.RetsubanWindow();

        //TopMost切替用
        private const int HOTKEY_ID = 1; // ホットキーID
        private bool isNPressed = false; // Nキーが押されたかを判定するフラグ

        // WinAPIの宣言
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        // 修飾キーの定義
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;

        private IntPtr previousWindowHandle = IntPtr.Zero;

        public MainWindow(OpenIddictClientService service)
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            this.Load += (s, e) => SavePreviousWindowHandle();
            this.Activated += (s, e) => SavePreviousWindowHandle();
            this.FormClosing += MainForm_FormClosing;

            // グローバルホットキーを登録 (Ctrl + 0)
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL, Keys.D0.GetHashCode());

            // グローバルホットキーの登録 (F4)
            RegisterHotKey(this.Handle, HOTKEY_ID_F4, 0, Keys.F4.GetHashCode());

            // イベントの設定
            Image_Reset.MouseDown += Image_Reset_MouseDown;
            Image_Reset.MouseUp += Image_Reset_MouseUp;
            Image_Reset.Click += Image_Reset_Click;

            // 長押し判定用タイマー（0.3秒）
            longPressTimer = new Timer { Interval = 300 };
            longPressTimer.Tick += LongPressTimer_Tick;

            // 差し替え画像のタイマー（0.8秒）
            resetImageTimer = new Timer { Interval = 800 };
            resetImageTimer.Tick += ResetImageTimer_Tick;

            // 0.5 秒後に蓋が戻るためのタイマー       
            lidTimer = new Timer();
            lidTimer.Interval = 500; // 0.5 秒
            lidTimer.Tick += LidTimer_Tick;
            // 0.3 秒長押し判定用のタイマー                   
            cutLongPressTimer = new Timer();
            cutLongPressTimer.Interval = 300; // 0.3 秒
            cutLongPressTimer.Tick += CutLongPressTimer_Tick;

            CableIO = new CableIO(service);
            CableIO.isKyokanChenge += Kyokan;
            CableIO.isATSReadyChenge += ATSReadyLamp;
            CableIO.isATSBrakeApplyChenge += ATSBrakeApplyLamp;
            CableIO.isRelayChenge += RelayLamp;
            CableIO.isTransferChenge += TransferLamp;
            CableIO.isNetworkChenge += NetworkLamp;
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            retsubanWindow.AddExceptionAction += CableIO.AddException;
            retsubanWindow.SetDiaNameAction += CableIO.RetsubanSet;
        }

        /// <summary>
        /// MainForm_Loadイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            CableIO.StartRelay();
            CableIO.NetworkAuthorize();
        }

        private void ATSReadyLamp(bool state)
        {
            // コントロールがまだ作成されていない場合は処理しない
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                // InvokeでUIスレッドに処理を委譲
                this.Invoke((Action)(() => ATSReadyLamp(state)));
            }
            else
            {
                Image_ATSReady.Visible = state;
            }
        }

        private void ATSBrakeApplyLamp(bool state)
        {
            // コントロールがまだ作成されていない場合は処理しない
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                // InvokeでUIスレッドに処理を委譲
                this.Invoke((Action)(() => ATSBrakeApplyLamp(state)));
            }
            else
            {
                Image_ATSBrakeApply.Visible = state;
            }
        }

        private void RelayLamp(bool state)
        {
            // コントロールがまだ作成されていない場合は処理しない
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                // InvokeでUIスレッドに処理を委譲
                this.Invoke((Action)(() => RelayLamp(state)));
            }
            else
            {
                Image_Relay.Visible = state;
            }
        }

        private void TransferLamp(bool state)
        {
            // コントロールがまだ作成されていない場合は処理しない
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                // InvokeでUIスレッドに処理を委譲
                this.Invoke((Action)(() => TransferLamp(state)));
            }
            else
            {
                Image_Transfer.Visible = state;
            }
        }

        private void NetworkLamp(bool state)
        {
            // コントロールがまだ作成されていない場合は処理しない
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                // InvokeでUIスレッドに処理を委譲
                this.Invoke((Action)(() => NetworkLamp(state)));
            }
            else
            {
                Image_Network.Visible = state;
            }
        }

        private void Kyokan(bool state)
        {
            Image_Kyokan?.Invoke((MethodInvoker)(() =>
            {
                Image_Kyokan.Visible = state;
            }));
        }

        private void Image_LED_Click(object sender, EventArgs e)
        {
            CableIO.LEDWinChenge();
        }

        private void Image_Retsuban_Click(object sender, EventArgs e)
        {
            if (retsubanWindow.Visible)
            {
                retsubanWindow.Hide();
            }
            else
            {
                retsubanWindow.Show();
            }
        }

        private void Image_Kokuchi_Click(object sender, EventArgs e)
        {
            CableIO.KokuchiWinChenge();
        }
        public void Application_ApplicationExit(object sender, EventArgs e)
        {
            //切断処理
            CableIO.ATSPower_Off();
            //ApplicationExitイベントハンドラを削除
            Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);
        }

        private void Image_Reset_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLongPressed = false; // 初期化
                longPressTimer.Start();
            }
        }

        private void Image_Reset_MouseUp(object sender, MouseEventArgs e)
        {
            longPressTimer.Stop();

            if (isLongPressed)
            {
                // 長押し成功後に、差し替え画像状態のまま放された場合の処理を開始
                resetImageTimer.Start();
            }
        }

        private void LongPressTimer_Tick(object sender, EventArgs e)
        {
            longPressTimer.Stop(); // 長押し時間を超えたらタイマーを止める
            isLongPressed = true; // 長押し成功を設定
            Image_Reset.BackgroundImage = MainResource.ATS_Reset1; // 差し替え画像に変更
        }

        private void ResetImageTimer_Tick(object sender, EventArgs e)
        {
            resetImageTimer.Stop();
            Image_Reset.BackgroundImage = MainResource.ATS_Reset0; // 画像を元に戻す
        }

        private void Image_Reset_Click(object sender, EventArgs e)
        {
            if (isLongPressed)
            {
                //Todo: ATS復帰操作を送る
                Debug.WriteLine("ATS復帰");
                CableIO.ATSResetPush();
            }
        }

        private void Image_ATSCut_MouseDown(object sender, MouseEventArgs e)
        {
            isLongPressHandled = false; // フラグリセット
            cutLongPressTimer.Start(); // 長押し判定タイマー開始
            lidTimer.Stop(); // クリック時に蓋戻りタイマーをリセット
        }

        private void Image_ATSCut_MouseUp(object sender, MouseEventArgs e)
        {
            cutLongPressTimer.Stop(); // 長押しタイマーを停止

            lidTimer.Start(); // クリック後 0.5 秒後に蓋を戻す
            if (isLongPressHandled)
            {
                return; // すでに長押し処理が実行されていたら、クリック処理を無効化
            }

            // クリック（短押し）による正常位置と開放位置の切り替え
            if (atsState == 2 || atsState == 3)
            {
                atsState = (atsState == 2) ? 3 : 2;
                if (atsState == 2)
                {
                    Debug.WriteLine("ATS: 正常位置へ変更");
                    CableIO.ATSPower_On();
                }
                else if (atsState == 3)
                {
                    Debug.WriteLine("ATS: 開放位置へ変更");
                    CableIO.ATSPower_Off();
                }
            }

            UpdateATSCutImage();
        }

        private void CutLongPressTimer_Tick(object sender, EventArgs e)
        {
            cutLongPressTimer.Stop(); // 長押し検知完了
            isLongPressHandled = true; // フラグをセット

            // 0.3秒以上の長押しで蓋を外す
            if (atsState == 0 || atsState == 1) // 通常 or 開放状態で蓋あり → 蓋なし
            {
                atsState += 2;
            }
            else if (atsState == 3) // 開放蓋なし → 開放蓋ありに変更
            {
                atsState = 1;
            }

            UpdateATSCutImage();
        }



        private void LidTimer_Tick(object sender, EventArgs e)
        {
            // 0.5 秒後に蓋を戻す
            if (atsState == 2 || atsState == 3)
            {
                atsState -= 2;
                Debug.WriteLine("ATS: 蓋が落ちて元の状態に戻る");
                UpdateATSCutImage();
            }
            lidTimer.Stop();
        }

        private void UpdateATSCutImage()
        {
            switch (atsState)
            {
                case 0:
                    Image_ATSCut.Image = MainResource.ATS_Cut0; // 蓋アリ/正常位置
                    break;
                case 1:
                    Image_ATSCut.Image = MainResource.ATS_Cut1; // 蓋アリ/開放位置
                    break;
                case 2:
                    Image_ATSCut.Image = MainResource.ATS_Cut2; // 蓋ナシ/正常位置
                    break;
                case 3:
                    Image_ATSCut.Image = MainResource.ATS_Cut3; // 蓋ナシ/開放位置
                    break;
            }
        }

        // メッセージ処理
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == HOTKEY_ID_F4)
                {
                    ToggleBougoState(); // F4キーの処理を呼び出し
                }
                else if (m.WParam.ToInt32() == HOTKEY_ID)
                {
                    OnTopMost(); // Ctrl+0の処理
                }
            }
            base.WndProc(ref m);
        }

        private void OnTopMost()
        {
            this.TopMost = true;
        }

        /// <summary>
        /// F4キー押下時にBougoStateをトグルする処理
        /// </summary>
        private void ToggleBougoState()
        {
            bougoState = !bougoState; // トグル処理
            CableIO.BougoStateChenge(bougoState); // 状態をCableIOに送信
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームが閉じる際にホットキーを解除          
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            UnregisterHotKey(this.Handle, HOTKEY_ID_F4);
        }
        private void SavePreviousWindowHandle()
        {
            IntPtr currentWindow = GetForegroundWindow();
            if (currentWindow != this.Handle)
            {
                previousWindowHandle = currentWindow;
            }
        }

        private void ReturnFocusToTrainCrew()
        {
            Process[] processes = Process.GetProcessesByName("TrainCrew");

            if (processes.Length > 0)
            {
                // TrainCrew.exeが存在する場合、最初のプロセスのメインウィンドウハンドルを取得
                IntPtr trainCrewHandle = processes[0].MainWindowHandle;

                if (trainCrewHandle != IntPtr.Zero)
                {
                    uint currentThreadId = GetCurrentThreadId();
                    uint trainCrewThreadId = GetWindowThreadProcessId(trainCrewHandle, out _);

                    // 必要ならスレッド間で入力を接続
                    if (currentThreadId != trainCrewThreadId)
                    {
                        AttachThreadInput(currentThreadId, trainCrewThreadId, true);
                    }

                    // TrainCrew.exeのウィンドウをアクティブにする
                    SetForegroundWindow(trainCrewHandle);

                    // スレッド間の接続を解除
                    if (currentThreadId != trainCrewThreadId)
                    {
                        AttachThreadInput(currentThreadId, trainCrewThreadId, false);
                    }
                    return;
                }
            }

            // TrainCrew.exeが見つからなければ何もしない
        }

        private void Image_TopMostOFF_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            ReturnFocusToTrainCrew();
        }

        private void Image_ATSCut_Click(object sender, MouseEventArgs e)
        {

        }

        private void Image_ATSReady_Click(object sender, EventArgs e)
        {
            CableIO.TestWinChenge();
        }
    }
}