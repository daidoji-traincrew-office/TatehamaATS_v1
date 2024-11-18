using TatehamaATS_v1.Exceptions;

namespace TatehamaATS_v1.OnboardDevice
{
    using System.Diagnostics;
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

        /// <summary>
        /// 継電異常変化
        /// </summary>
        internal event Action<bool> isRelayChenge;

        /// <summary>
        /// 伝送異常変化
        /// </summary>
        internal event Action<bool> isTransferChenge;

        /// <summary>
        /// 通信異常変化
        /// </summary>
        internal event Action<bool> isNetworkChenge;

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
            Network.ConnectionStatusChanged += NetworkStatesChenged;
            Network.Connect();
        }

        /// <summary>
        /// TC状態変更指令線
        /// </summary>
        /// <param name="TcData"></param>
        private void RelayUpdatad(TrainCrewStateData TcData)
        {
            InspectionRecord.RelayUpdate(TcData);
            ControlLED.TC_ATSDisplayData.SetLED(TcData.myTrainData.ATS_Class, TcData.myTrainData.ATS_Speed, TcData.myTrainData.ATS_State);
            isKyokanChenge.Invoke(TcData.driveMode == DriveMode.Normal && (TcData.gameScreen == GameScreen.MainGame || TcData.gameScreen == GameScreen.MainGame_Pause || TcData.gameScreen == GameScreen.MainGame_Loading));

            //Todo:データ整形後接続
            //SendData_to_Server
        }

        /// <summary>
        /// 故障状況変更指令線
        /// </summary>
        /// <param name="exceptionCodes"></param>
        private void ExceptionCodesChenge(List<string> exceptionCodes)
        {
            isATSReadyChenge?.Invoke(exceptionCodes.Count == 0);
            ControlLED.ExceptionCodes = exceptionCodes;
            isRelayChenge?.Invoke(ContainsPartialMatch(exceptionCodes, "3C"));
            isTransferChenge?.Invoke(ContainsPartialMatch(exceptionCodes, "3D"));
            isNetworkChenge?.Invoke(ContainsPartialMatch(exceptionCodes, "3E"));

        }

        /// <summary>
        /// List内の文字列に対し、一部一致で検索を行う
        /// </summary>
        /// <param name="list">検索対象のリスト</param>
        /// <param name="keyword">検索する文字列</param>
        /// <returns>存在する場合はtrue、存在しない場合はfalse</returns>
        static bool ContainsPartialMatch(List<string> list, string keyword)
        {
            foreach (string item in list)
            {
                if (item.Contains(keyword))
                {
                    return true; // 一部一致する文字列が見つかった場合
                }
            }
            return false; // 一部一致する文字列がなかった場合
        }


        /// <summary>
        /// TC接続状態変化指令線
        /// </summary>
        /// <param name="connectionState"></param>
        private void RelayStatesChenged(ConnectionState connectionState)
        {
            InspectionRecord.RelayState = connectionState == ConnectionState.Connected;
        }

        /// <summary>
        /// 信号接続状態変化指令線
        /// </summary>
        /// <param name="connectionState"></param>
        private void NetworkStatesChenged(bool connectionState)
        {
            InspectionRecord.RelayState = connectionState;
        }

        /// <summary>
        /// 継電部非常状態変化指令線
        /// </summary>
        /// <param name="brake"></param>
        private void RelayEmBrakeStateChenge(bool brake)
        {
            RelayEmBrakeState = brake;
            isATSBrakeApplyChenge?.Invoke(brake);
        }

        /// <summary>
        /// LEDWin表示指示指令線
        /// </summary>
        internal void LEDWinChenge()
        {
            if (ControlLED.isShow)
            {
                ControlLED.LEDHide();
                Relay.EMSet(new EmergencyLightData() { Name = "浜園2号踏切", State = true });
                Relay.SignalSet(new SignalData() { Name = "大道寺入換32L", phase = Phase.R });
                Relay.SignalSet(new SignalData() { Name = "大道寺下り出発5L", phase = Phase.R });
            }
            else
            {
                ControlLED.LEDShow();
                Relay.EMSet(new EmergencyLightData() { Name = "浜園2号踏切", State = false });
                Relay.SignalSet(new SignalData() { Name = "大道寺入換32L", phase = Phase.G });
                Relay.SignalSet(new SignalData() { Name = "大道寺下り出発5L", phase = Phase.None });
            }
        }

        /// <summary>
        /// 継電部動作開始指令線
        /// </summary>
        public async void StartRelay()
        {
            Relay.Command = "DataRequest";
            Relay.Request = new[] { "tconlyontrain" };
            await Relay.TryConnectWebSocket();
        }

        /// <summary>
        /// 故障発生送信指令線
        /// </summary>
        /// <param name="exception"></param>
        internal void AddException(ATSCommonException exception)
        {
            Console.WriteLine(exception.ToString());
            InspectionRecord.AddException(exception);
        }

        /// <summary>
        /// 信号制御情報指令線
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

        /// <summary>
        /// ATS電源切指令線
        /// </summary>
        internal void ATSOffing()
        {
            Network.Close();
        }
    }
}