using OpenIddict.Client;
using TatehamaATS_v1.Exceptions;
using TrainCrewAPI;

namespace TatehamaATS_v1.OnboardDevice
{
    using System;
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
        /// スピーカー
        /// </summary>
        ConsoleSpeaker Speaker;

        /// <summary>
        /// 運転告知器
        /// </summary>
        KokuchiWindow.KokuchiWindow KokuchiWindow = new KokuchiWindow.KokuchiWindow();

        /// <summary>
        /// ゲーム内時間
        /// </summary>
        static TimeSpan TC_Time;

        /// <summary>
        /// ATS電源状態
        /// </summary>
        bool ATSPowerState;

        /// <summary>
        /// 検査記録部非常線
        /// </summary>
        bool InspectionRecordEmBrakeState;

        /// <summary>
        /// 自車防護無線
        /// </summary>
        bool MyBougoState;

        /// <summary>
        /// 他車防護無線
        /// </summary>
        bool OtherBougoState;


        /// <summary>
        /// 教官添乗状態
        /// </summary>
        bool isKyokan;

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

        internal CableIO(OpenIddictClientService service)
        {
            Speaker = new ConsoleSpeaker();
            ATSPower_On();

            Network = new Network(service);
            Network.AddExceptionAction += AddException;
            Network.ServerDataUpdate += ServerDataUpdate;
            Network.ConnectionStatusChanged += NetworkStatesChenged;
            Network.RetsubanInOutStatusChanged += ForceStopSignal;

            Relay = new Relay();
            Relay.AddExceptionAction += AddException;
            Relay.TrainCrewDataUpdated += RelayUpdatad;
            Relay.ConnectionStatusChanged += RelayStatesChenged;

            InspectionRecord = new InspectionRecord();
            InspectionRecord.AddExceptionAction += AddException;
            InspectionRecord.ExceptionCodesChenge += ExceptionCodesChenge;
            InspectionRecord.EmBrakeStateUpdated += InspectionRecordEmBrakeStateChenge;

            KokuchiWindow = new KokuchiWindow.KokuchiWindow();
            KokuchiWindow.Hide();

            ControlLED.AddExceptionAction += AddException;
        }

        /// <summary>
        /// ATS電源入指令線
        /// </summary>
        public void ATSPower_On()
        {
            ATSPowerState = true;
            EmBrakeStateChenge();
            ExceptionCodesChenge(InspectionRecord.exceptions.Values.Select(e => e.ToCode()).ToList());
        }

        /// <summary>
        /// ATS電源切指令線
        /// </summary>
        public void ATSPower_Off()
        {
            ATSPowerState = false;
            ExceptionCodesChenge(new List<string> { });
            EmBrakeStateChenge();
            ControlLED.TC_ATSDisplayData.SetLED("", "", AtsState.OFF);
            Relay?.SetEB(false);
            InspectionRecord.PowerReset = true;
        }

        /// <summary>
        /// TC状態変更指令線
        /// </summary>
        /// <param name="TcData"></param>
        private void RelayUpdatad(TrainCrewStateData TcData)
        {
            InspectionRecord.RelayUpdate(TcData);
            if (ATSPowerState)
            {
                ControlLED.TC_ATSDisplayData.SetLED(TcData.myTrainData.ATS_Class, TcData.myTrainData.ATS_Speed, TcData.myTrainData.ATS_State);
            }
            else
            {
                ControlLED.TC_ATSDisplayData.SetLED("", "", AtsState.OFF);
            }
            var nowKyokan = TcData.driveMode == DriveMode.Normal && (TcData.gameScreen == GameScreen.MainGame || TcData.gameScreen == GameScreen.MainGame_Pause || TcData.gameScreen == GameScreen.MainGame_Loading);
            if (nowKyokan != isKyokan)
            {
                isKyokanChenge.Invoke(nowKyokan);
                Speaker.ChengeKyokan(nowKyokan);
                isKyokan = nowKyokan;
            }
            Network.TcDataUpdate(TcData);
        }

        /// <summary>
        /// 故障状況変更指令線
        /// </summary>
        /// <param name="exceptionCodes"></param>
        private void ExceptionCodesChenge(List<string> exceptionCodes)
        {
            if (ATSPowerState)
            {
                isATSReadyChenge?.Invoke(exceptionCodes.Count == 0);
                ControlLED.ExceptionCodes = exceptionCodes;
                isRelayChenge?.Invoke(ContainsPartialMatch(exceptionCodes, "5C") || ContainsPartialMatch(exceptionCodes, "58") || ContainsPartialMatch(exceptionCodes, "95"));
                isTransferChenge?.Invoke(ContainsPartialMatch(exceptionCodes, "6D") || ContainsPartialMatch(exceptionCodes, "68") || ContainsPartialMatch(exceptionCodes, "96"));
                isNetworkChenge?.Invoke(ContainsPartialMatch(exceptionCodes, "7E") || ContainsPartialMatch(exceptionCodes, "78") || ContainsPartialMatch(exceptionCodes, "97"));
            }
            else
            {
                isATSReadyChenge?.Invoke(false);
                ControlLED.ExceptionCodes = new List<string> { };
                isRelayChenge?.Invoke(false);
                isTransferChenge?.Invoke(false);
                isNetworkChenge?.Invoke(false);
            }
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
            InspectionRecord.NetworkState = connectionState;
        }

        /// <summary>
        /// 検査記録部非常状態変化指令線
        /// </summary>
        /// <param name="brake"></param>
        private void InspectionRecordEmBrakeStateChenge(bool brake)
        {
            InspectionRecordEmBrakeState = brake;
            EmBrakeStateChenge();
        }

        /// <summary>
        /// 非常状態変化指令線
        /// </summary>
        private void EmBrakeStateChenge()
        {
            bool emBrakeState;
            if (ATSPowerState)
            {
                emBrakeState = InspectionRecordEmBrakeState;
            }
            else
            {
                emBrakeState = false;
            }
            isATSBrakeApplyChenge?.Invoke(emBrakeState);
            Relay?.SetEB(emBrakeState);
        }

        /// <summary>
        /// LEDWin表示指示指令線
        /// </summary>
        internal void LEDWinChenge()
        {
            if (ControlLED.isShow)
            {
                ControlLED.LEDHide();
            }
            else
            {
                ControlLED.LEDShow();
            }
        }
        /// <summary>
        /// 告知Win表示指示指令線
        /// </summary>
        internal void KokuchiWinChenge()
        {
            if (KokuchiWindow.Visible)
            {
                KokuchiWindow.Hide();
            }
            else
            {
                KokuchiWindow.Show();
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
            InspectionRecord?.AddException(exception);
        }

        /// <summary>
        /// 信号制御情報指令線
        /// </summary>
        /// <param name="dataFromServer"></param>
        internal void ServerDataUpdate(DataFromServer dataFromServer, bool ForceStop)
        {
            InspectionRecord.NetworkUpdate();
            foreach (var item in dataFromServer.EmergencyLightDatas)
            {
                Relay.EMSet(item);
            }
            OtherBougoState = dataFromServer.BougoState;
            Speaker.ChengeBougoState(MyBougoState, OtherBougoState);
            KokuchiWindow.SetData(dataFromServer.OperationNotificationData);
            if (!ForceStop)
            {
                var signalDataList = new List<SignalData>(dataFromServer.NextSignalData);
                signalDataList.AddRange(dataFromServer.DoubleNextSignalData);
                Relay.SignalSet(signalDataList);
            }
        }

        /// <summary>
        /// ATS復帰指令線
        /// </summary>
        internal void ATSResetPush()
        {
            InspectionRecord.ATSReset = true;
        }

        /// <summary>
        /// 防護無線指令線
        /// </summary>
        /// <param name="State"></param>
        internal void BougoStateChenge(bool State)
        {
            MyBougoState = State;
            Network.IsBougo = State;
            Speaker.ChengeBougoState(MyBougoState, OtherBougoState);
        }

        /// <summary>
        /// 認証指令線
        /// </summary>
        internal void NetworkAuthorize()
        {
            Network.Authorize();
        }

        /// <summary>
        /// 列番情報変更線
        /// </summary>
        internal void RetsubanSet(string Retsuban)
        {
            Network.OverrideDiaName = Retsuban;
        }

        /// <summary>
        /// 強制前方停止司令線
        /// </summary>
        internal void ForceStopSignal(bool IsStop)
        {
            Relay?.ForceStopSignal(IsStop);
        }

        /// <summary>
        /// サーバー強制再接続司令線
        /// </summary>
        internal void StartUpdateLoop()
        {
            Network.StartUpdateLoop();
        }
    }
}