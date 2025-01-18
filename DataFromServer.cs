using System.Linq;
using TrainCrewAPI;

namespace TatehamaATS_v1
{
    public class DataToServer
    {
        public string DiaName;
        public List<TrackCircuitData> OnTrackList = new List<TrackCircuitData>();
        public bool BougoState;
        public string Kokuchi;
        //将来用
        public float Speed;
        public int PNotch;
        public int BNotch;
        public List<CarState> CarStates = new List<CarState>();
        public override string ToString()
        {
            return $"DiaName:{DiaName}/{string.Join(",", OnTrackList)}";
        }
    }

    public class DataFromServer
    {
        public SignalData NextSignalData = null;
        public SignalData DoubleNextSignalData = null;
        //進路表示の表示はTC本体実装待ち　未決定
        public bool BougoState;
        public List<EmergencyLightData> EmergencyLightDatas;
        public Dictionary<string, KokuchiData> KokuchiData;
    }

    public class EmergencyLightData
    {
        public string Name;
        public bool State;
    }
}