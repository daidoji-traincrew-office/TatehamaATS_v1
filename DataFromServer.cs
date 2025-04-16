using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TrainCrewAPI;

namespace TatehamaATS_v1
{
    public class DataToServer
    {
        public string DiaName { get; set; } = "9999";
        public List<TrackCircuitData> OnTrackList { get; set; } = new List<TrackCircuitData>();
        public bool BougoState { get; set; } = false;
        public string Kokuchi { get; set; } = "9999";
        //将来用
        public float Speed { get; set; } = 0.0f;
        public int PNotch { get; set; } = 0;
        public int BNotch { get; set; } = 8;
        public List<CarState> CarStates { get; set; } = new List<CarState>();
        public override string ToString()
        {
            return $"BougoState:{BougoState}/DiaName:{DiaName}/{string.Join(",", OnTrackList)}";
        }
    }

    public class DataFromServer
    {
        public List<SignalData> NextSignalData { get; set; } = null;
        public List<SignalData> DoubleNextSignalData { get; set; } = null;
        public bool BougoState { get; set; }
        public List<EmergencyLightData> EmergencyLightDatas { get; set; }
        public OperationNotificationData? OperationNotificationData { get; set; }
        public List<Route> RouteData { get; set; } = [];
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