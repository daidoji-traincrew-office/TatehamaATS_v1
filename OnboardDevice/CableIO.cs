using TatehamaATS_v1.Exceptions;

namespace TatehamaATS_v1.OnboardDevice
{
    using TatehamaATS_v1.Network;
    public class CableIO
    {
        /// <summary>
        /// 検査記録部
        /// </summary>
        InspectionRecord InspectionRecord;

        /// <summary>
        /// 継電部
        /// </summary>
        Relay Relay = new Relay();

        /// <summary>
        /// LED制御部
        /// </summary>
        private ControlLED ControlLED = new ControlLED();

        /// <summary>
        /// 通信部
        /// </summary>
        Network Network;

        /// <summary>
        /// ゲーム内時間
        /// </summary>
        static TimeSpan TC_Time;

        /// <summary>
        /// 検査記録部非常線
        /// </summary>
        bool RelayEmBrakeState;

        /// <summary>
        /// 教官添乗状態変化
        /// </summary>
        internal event Action<bool> isKyokanChenge;

        /// <summary>
        /// ATS正常変化
        /// </summary>
        internal event Action<bool> isATSReadyChenge;

        /// <summary>
        /// ATS動作変化
        /// </summary>
        internal event Action<bool> isATSBrakeApplyChenge;

        internal CableIO()
        {
            InspectionRecord = new InspectionRecord();
            InspectionRecord.AddExceptionAction += AddException;
            InspectionRecord.ExceptionCodesChenge += ExceptionCodesChenge;
            InspectionRecord.EmBrakeStateUpdated += RelayEmBrakeStateChenge;
            Relay = new Relay();
            Relay.AddExceptionAction += AddException;
            Relay.TrainCrewDataUpdated += RelayUpdatad;
            Relay.ConnectionStatusChanged += RelayStatesChenged;
            Network = new Network();
            Network.AddExceptionAction += AddException;
            Network.TryConnect();
        }

        /// <summary>
        /// TC状態変更司令線
        /// </summary>
        /// <param name="TcData"></param>
        private void RelayUpdatad(TrainCrewStateData TcData)
        {
            InspectionRecord.RelayUpdate(TcData);
            ControlLED.TC_ATSDisplayData.SetLED(TcData.myTrainData.ATS_Class, TcData.myTrainData.ATS_Speed, TcData.myTrainData.ATS_State);
            isKyokanChenge.Invoke(TcData.driveMode == DriveMode.Normal && (TcData.gameScreen == GameScreen.MainGame || TcData.gameScreen == GameScreen.MainGame_Pause || TcData.gameScreen == GameScreen.MainGame_Loading));
        }

        /// <summary>
        /// 故障状況変更司令線
        /// </summary>
        /// <param name="exceptionCodes"></param>
        private void ExceptionCodesChenge(List<string> exceptionCodes)
        {
            isATSReadyChenge?.Invoke(exceptionCodes.Count == 0);
            ControlLED.ExceptionCodes = exceptionCodes;
        }

        /// <summary>
        /// TC接続状態変化
        /// </summary>
        /// <param name="connectionState"></param>
        private void RelayStatesChenged(ConnectionState connectionState)
        {
            if (connectionState == ConnectionState.Connected)
            {
                InspectionRecord.RelayReset = true;

                //特発確認                                 
                Relay.EMSet(new EmergencyLightData() { Name = "浜園2号踏切", State = true });
                Relay.EMSet(new EmergencyLightData() { Name = "浜園2号踏切", State = false });
                Relay.SignalSet(new SignalData() { Name = "津崎下り出発3L", phase = Phase.None });
            }
            else
            {

            }
        }

        private void RelayEmBrakeStateChenge(bool brake)
        {
            RelayEmBrakeState = brake;
            isATSBrakeApplyChenge?.Invoke(brake);
        }

        /// <summary>
        /// LEDWin表示指示
        /// </summary>
        internal void LEDWinChenge()
        {
            if (ControlLED.isShow)
            {
                ControlLED.LEDHide();
                Relay.EMSet(new EmergencyLightData() { Name = "浜園2号踏切", State = true });
            }
            else
            {
                ControlLED.LEDShow();
                Relay.EMSet(new EmergencyLightData() { Name = "浜園2号踏切", State = false });
            }
        }

        /// <summary>
        /// 継電部
        /// </summary>
        public async void StartRelay()
        {
            Relay.Command = "DataRequest";
            Relay.Request = new[] { "tconlyontrain" };
            await Relay.TryConnectWebSocket();
        }

        /// <summary>
        /// 故障発生時に検査記録部へ送信
        /// </summary>
        /// <param name="exception"></param>
        internal void AddException(ATSCommonException exception)
        {
            Console.WriteLine(exception.ToString());
            InspectionRecord.AddException(exception);
        }

        /// <summary>
        /// サーバーからデータきた
        /// </summary>
        /// <param name="dataFromServer"></param>
        internal void ServerDataUpdate(DataFromServer dataFromServer)
        {
            Relay.SignalSet(dataFromServer.NextSignalData);
            Relay.SignalSet(dataFromServer.DoubleNextSignalData);
            foreach (var item in dataFromServer.EmergencyLightDatas)
            {
                Relay.EMSet(item);
            }

        }
    }
}