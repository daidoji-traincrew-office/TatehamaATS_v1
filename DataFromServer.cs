using System.Linq;
using TrainCrewAPI;

namespace TatehamaATS_v1
{
    public class DataToServer
    {
        public string DiaName { get; set; }
        public List<TrackCircuitData> OnTrackList { get; set; } = new List<TrackCircuitData>();
        public bool BougoState { get; set; }
        public string Kokuchi { get; set; }
        //将来用
        public float Speed { get; set; }
        public int PNotch { get; set; }
        public int BNotch { get; set; }
        public List<CarState> CarStates { get; set; } = new List<CarState>();
        public override string ToString()
        {
            return $"DiaName:{DiaName}/{string.Join(",", OnTrackList)}";
        }
    }

    public class DataFromServer
    {
        public List<SignalData> NextSignalData { get; set; } = null;
        public List<SignalData> DoubleNextSignalData { get; set; } = null;
        //進路表示の表示はTC本体実装待ち　未決定
        public bool BougoState { get; set; }
        public List<EmergencyLightData> EmergencyLightDatas { get; set; }
        public Dictionary<string, KokuchiData> KokuchiData { get; set; }
        public override string ToString()
        {
            return $"BougoState:{BougoState}/{string.Join(",", NextSignalData)}/{string.Join(",", DoubleNextSignalData)}";
        }
    }

    public class EmergencyLightData
    {
        public string Name { get; set; }
        public bool State { get; set; }
    }
}