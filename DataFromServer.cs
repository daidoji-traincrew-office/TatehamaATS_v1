using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TrainCrewAPI;

namespace TatehamaATS_v1
{
    public class DataToServer
    {
        public bool BougoState { get; set; } = false;
        public List<CarState> CarStates { get; set; } = new List<CarState>();
        public string DiaName { get; set; } = "9999";
        public List<TrackCircuitData> OnTrackList { get; set; } = new List<TrackCircuitData>();

        //早着撤去無視フラグ      
        public bool IsTherePreviousTrainIgnore { get; set; } = false;
        //ワープ許容フラグ
        public bool IsMaybeWarpIgnore { get; set; } = false;

        public float Speed { get; set; } = 0.0f;
        public float Acceleration { get; set; } = 0.0f;
        //将来用
        public int PNotch { get; set; } = 0;
        public int BNotch { get; set; } = 8;
        public override string ToString()
        {
            return $"BougoState:{BougoState}/DiaName:{DiaName}/{string.Join(",", OnTrackList)}";
        }
    }

    public class DataFromServer
    {
        /// <summary>
        /// 次信号の状態
        /// </summary>
        public List<SignalData> NextSignalData { get; set; } = [];
        /// <summary>
        /// 次々信号の状態
        /// </summary>
        public List<SignalData> DoubleNextSignalData { get; set; } = [];
        /// <summary>
        /// 防護無線の状態
        /// </summary>
        public bool BougoState { get; set; } = false;
        /// <summary>
        /// 特発状態
        /// </summary>
        public List<EmergencyLightData> EmergencyLightDatas { get; set; } = [];
        /// <summary>
        /// 運転告知器の状態
        /// </summary>
        public OperationNotificationData? OperationNotificationData { get; set; } = null;
        /// <summary>
        /// 進路情報
        /// </summary>
        public List<Route> RouteData { get; set; } = new();
        /// <summary>
        /// 踏みつぶし状態
        /// </summary>
        public bool IsOnPreviousTrain { get; set; } = false;
        /// <summary>
        /// 同一運番状態
        /// </summary>
        public bool IsTherePreviousTrain { get; set; } = false;
        /// <summary>
        /// ワープの可能性あり状態
        /// </summary>
        public bool IsMaybeWarp { get; set; } = false;
        /// <summary>
        /// 編成構成不一致
        /// </summary>
        public bool IsCarMismatch;
        public override string ToString()
        {
            return $"BougoState:{BougoState}/{OperationNotificationData}/{RouteData}/{string.Join(",", NextSignalData)}/{string.Join(",", DoubleNextSignalData)}";
        }
    }

    public class EmergencyLightData
    {
        public string Name { get; set; }
        public bool State { get; set; }
    }

    public class Route
    {
        public string TcName { get; set; }
        public RouteType RouteType { get; set; }
        public ulong? RootId { get; set; }
        public Route? Root { get; set; }
        public string? Indicator { get; set; }
        public int? ApproachLockTime { get; set; }
        public RouteState? RouteState { get; set; }
    }

    public enum RouteType
    {
        Arriving,       // 場内
        Departure,      // 出発
        Guide,          // 誘導
        SwitchSignal,   // 入換信号
        SwitchRoute     // 入換標識
    }
    public class RouteState
    {
        public ulong Id { get; init; }
        /// <summary>
        /// てこ反応リレー
        /// </summary>
        public RaiseDrop IsLeverRelayRaised { get; set; }
        /// <summary>
        /// 進路照査リレー
        /// </summary>
        public RaiseDrop IsRouteRelayRaised { get; set; }
        /// <summary>
        /// 信号制御リレー
        /// </summary>
        public RaiseDrop IsSignalControlRaised { get; set; }
        /// <summary>
        /// 接近鎖錠リレー(MR)
        /// </summary>
        public RaiseDrop IsApproachLockMRRaised { get; set; }
        /// <summary>
        /// 接近鎖錠リレー(MS)
        /// </summary>
        public RaiseDrop IsApproachLockMSRaised { get; set; }
        /// <summary>
        /// 進路鎖錠リレー(実在しない)
        /// </summary>
        public RaiseDrop IsRouteLockRaised { get; set; }
        /// <summary>
        /// 総括反応リレー
        /// </summary>
        public RaiseDrop IsThrowOutXRRelayRaised { get; set; }
        /// <summary>
        /// 総括反応中継リレー
        /// </summary>
        public RaiseDrop IsThrowOutYSRelayRaised { get; set; }
    }
    public enum RaiseDrop
    {
        Drop,
        Raise
    }
}