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
        public List<string> VisibleSignalNames { get; set; } = [];

        public override string ToString()
        {
            return $"BougoState:{BougoState}/DiaName:{DiaName}/{string.Join(",", OnTrackList)}";
        }
    }

    [Flags]
    public enum ServerStatusFlags
    {
        None = 0,

        /// <summary>
        /// 踏みつぶし状態
        /// </summary>
        IsOnPreviousTrain = 1 << 0,

        /// <summary>
        /// 同一運番状態
        /// </summary>
        IsTherePreviousTrain = 1 << 1,

        /// <summary>
        /// ワープの可能性あり状態
        /// </summary>
        IsMaybeWarp = 1 << 2,

        /// <summary>
        /// 接続拒否状態
        /// </summary>
        IsDisconnected = 1 << 3,

        /// <summary>
        /// 鎖錠状態
        /// </summary>
        IsLocked = 1 << 4,

        /// <summary>
        /// 地上装置停止中
        /// </summary>
        IsServerStopped = 1 << 5
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
        /// 編成構成不一致
        /// </summary>
        public bool IsCarMismatch;

        /// <summary>
        /// ステータスフラグ(ビットフラグ)
        /// </summary>
        public ServerStatusFlags StatusFlags { get; set; } = ServerStatusFlags.None;

        /// <summary>
        /// 次の信号機名リスト
        /// </summary>
        public List<string> NextSignalNames { get; set; } = [];

        public override string ToString()
        {
            return $"BougoState:{BougoState}/{OperationNotificationData}/{RouteData}/{string.Join(",", NextSignalData)}/{string.Join(",", DoubleNextSignalData)}";
        }
    }

    public class DataFromServerBySchedule
    {
        /// <summary>
        /// TST時差
        /// </summary>
        public int TimeOffset { get; set; }

        /// <summary>
        /// 進路情報
        /// </summary>
        public List<Route> RouteData { get; set; } = [];
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