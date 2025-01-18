using System;
using System.Drawing;
using TatehamaATS_v1.OnboardDevice;


namespace TatehamaATS_v1.MainWindow
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    public partial class MainWindow : Form
    {
        private const int HOTKEY_ID_F4 = 2; // F4キー用ホットキーID
        private bool bougoState = false; // トグル用の状態管理変数

        private Timer longPressTimer; // 長押し判定用
        private Timer resetImageTimer; // 画像差し替え期間管理用
        private bool isLongPressed = false;

        private CableIO CableIO;
        private RetsubanWindow.RetsubanWindow retsubanWindow = new RetsubanWindow.RetsubanWindow();
        private KokuchiWindow.KokuchiWindow kokuchiWindow = new KokuchiWindow.KokuchiWindow();

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

        public MainWindow()
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

            // 長押し判定用タイマー（0.2秒）
            longPressTimer = new Timer { Interval = 300 };
            longPressTimer.Tick += LongPressTimer_Tick;

            // 差し替え画像のタイマー（0.5秒）
            resetImageTimer = new Timer { Interval = 800 };
            resetImageTimer.Tick += ResetImageTimer_Tick;

            CableIO = new CableIO();
            CableIO.isKyokanChenge += Kyokan;
            CableIO.isATSReadyChenge += ATSReadyLamp;
            CableIO.isATSBrakeApplyChenge += ATSBrakeApplyLamp;
            CableIO.isRelayChenge += RelayLamp;
            CableIO.isTransferChenge += TransferLamp;
            CableIO.isNetworkChenge += NetworkLamp;
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        /// <summary>
        /// MainForm_Loadイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            CableIO.StartRelay();
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
            if (kokuchiWindow.Visible)
            {
                kokuchiWindow.Hide();
            }
            else
            {
                kokuchiWindow.Show();
            }
        }
        public void Application_ApplicationExit(object sender, EventArgs e)
        {
            //切断処理
            CableIO.ATSOffing();
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
    }
}