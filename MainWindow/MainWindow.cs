using TatehamaATS_v1.OnboardDevice;

namespace TatehamaATS_v1.MainWindow
{
    public partial class MainWindow : Form
    {
        private CableIO CableIO;
        private RetsubanWindow.RetsubanWindow retsubanWindow = new RetsubanWindow.RetsubanWindow();
        private KokuchiWindow.KokuchiWindow kokuchiWindow = new KokuchiWindow.KokuchiWindow();

        public MainWindow()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
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
    }
}