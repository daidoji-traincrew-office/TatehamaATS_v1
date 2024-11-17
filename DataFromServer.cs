namespace TatehamaATS_v1
{
    public class DataToServer
    {
        public string DiaName;
        public List<TrackCircuitData> OnTrackList = null;
        public bool BougoState;
        //将来用
        public float Speed;
        public int PNotch;
        public int BNotch;
        public List<CarState> CarStates = new List<CarState>();
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