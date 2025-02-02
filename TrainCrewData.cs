using System;
using System.Collections;
using System.Collections.Generic;

namespace TrainCrewAPI
{
    // TRAIN CREW WebSocket API

    // ▼　送信関係　▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    [Serializable]
    public class CommandToTrainCrew
    {
        public string command { get; set; }
        public string[] args { get; set; }
    }

    /* コマンド一覧
     * 
     * ●データ取得要求
     * command = DataRequest
     * args = 取得したいデータ
     *      all             :   全て
     *      tc              :   軌道回路
     *      tconlyontrain   :   踏んでいる軌道回路のみ
     *      tcall           :   全ての軌道回路
     *      signal          :   信号機の現示
     *      train           :   他列車の情報
     * 
     * ●特発の動作を設定する
     * command = SetEmergencyLight
     * args[0] = 踏切名
     * args[1] = true or false
     * 
     * ●信号の現示を設定する　※ゲーム内現示より下位の現示のみ設定可能
     * command = SetSignalPhase
     * args[0] = 信号名
     * args[1] = 現示（下記表記）
     *      解除  :  None
     *      停止  :   R
     *      警戒  :   YY
     *      注意  :   Y
     *      減速  :   YG
     *      進行  :   G
     */

    // ▼　受信関係　▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    [Serializable]
    public class Data_Base
    {
        public string type;
        public object data;
    }

    [Serializable]
    public class TrainCrewState
    {
        public string type;
        public TrainCrewStateData data;
    }


    [Serializable]
    public class TrainCrewStateData
    {
        public TimeData nowTime;
        public TrainState myTrainData = new TrainState();
        public List<TrackCircuitData> trackCircuitList = null;
        public List<RunTrainData> otherTrainDataList = null;
        public List<SignalData> signalDataList = null;

        public GameScreen gameScreen = GameScreen.Other;
        public CrewType crewType = CrewType.Driver;
        public DriveMode driveMode = DriveMode.Normal;
    }


    // 地上子イベント
    [Serializable]
    public class RecvBeaconState
    {
        public string type = "RecvBeaconStateData";
        public RecvBeaconStateData data;
    }

    [Serializable]
    public class RecvBeaconStateData
    {
        public BeaconType beaconType;
        public float Speed = 0;
        public float PtnLength = 0;
        public float Gradient = 0;
        public string DataString = "";
        public bool Processed = true;
    }

    [Serializable]
    public struct TimeData
    {
        public int hour;
        public int minute;
        public float second;

        public TimeData(int h, int m, float s)
        {
            hour = h;
            minute = m;
            second = s;
        }

        public static TimeData FromTimeSpan(TimeSpan ts)
        {
            return new TimeData(ts.Hours, ts.Minutes, ts.Seconds + ts.Milliseconds / 1000f);
        }

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(0, hour, minute, (int)second, (int)((second % 1f) * 1000));
        }

        public override string ToString()
        {
            return hour + ":" + minute + ":" + second.ToString("0.0");
        }
    }

    [Serializable]
    public class TrackCircuitData
    {
        public bool On { get; set; } = false;
        public string Last { get; set; } = null; // 軌道回路を踏んだ列車の名前
        public string Name { get; set; } = "";

        public override string ToString()
        {
            return $"{Name}";
        }
    }

    public enum DriveMode
    {
        Normal,
        Free,
        RTA,
    }

    public enum CrewType
    {
        Driver,
        Conductor,
        Passenger,
    }

    public enum GameScreen
    {
        MainGame,
        MainGame_Pause,
        MainGame_Loading,
        Menu,
        Result,
        Title,
        Other,
        NotRunningGame,
    }

    [Serializable]
    public class RunTrainData
    {
        public string Name;
        public string Class;
        public string BoundFor;
        public bool onTrack = false; // 出発
        public bool autoDriveEnable = false; // 出発
        public float Speed;
        public float speedTo = 110;
        public bool AllClose;
        public float TotalLength = 0;
        public bool isJieiR = false;
        public string debugMsg = "";
    }

    [Serializable]
    public class SignalData
    {
        public string Name { get; set; }
        public Phase phase { get; set; } = Phase.None;
        public override string ToString()
        {
            return $"SignalData:{Name}/{phase}";
        }
    }

    public enum Phase
    {
        None,
        R,
        YY,
        Y,
        YG,
        G
    }

    public enum BeaconType
    {
        Signal,
        Constant,
        SigIfStop,
        PatternConst,
        PatternSig,
        PatternFlat,
        PatternClear,
        PatternSigClear,
        PatternSwitch,
        PatternSwitchClear,
        StationStop,
        誤出発防止,
        ConstantTimer,
        PatternConstStopEnd,
        DoorCut,
    }

    [Serializable]
    public class TrainState
    {
        public float Speed;
        public bool AllClose;
        public float MR_Press;
        public List<CarState> CarStates = new List<CarState>();

        public Dictionary<PanelLamp, bool> Lamps = new Dictionary<PanelLamp, bool>();
        public string ATS_Class = "普通";
        public string ATS_Speed = "110";
        public AtsState ATS_State = AtsState.OFF;

        public string diaName = "";
        public string Class = "";
        public string BoundFor = "";

        public float nextUIDistance = 0;
        public float nextStaDistance = 0;
        public string nextStaName = "";
        public string nextStopType = "停車";
        public float speedLimit = 110;
        public float nextSpeedLimit = -1;
        public float nextSpeedLimitDistance = -1;
        public float gradient = 0;
        public float TotalLength = 0;
        public float KilometerPost = -1;

        public int Pnotch = 0;
        public int Bnotch = 0;
        public int Reverser = 1;

        public List<StationInfo> stationList = new List<StationInfo>();
        public int nowStaIndex = 0;

        public TrainState()
        {
            foreach (PanelLamp lmp in Enum.GetValues(typeof(PanelLamp)))
            {
                Lamps[lmp] = (lmp == PanelLamp.ATS_Ready);
            }
        }
    }

    [Serializable]
    public class CarState
    {
        public bool DoorClose;
        public float BC_Press;
        public float Ampare;
        public string CarModel;
        public bool HasPantograph = false;
        public bool HasDriverCab = false;
        public bool HasConductorCab = false;
        public bool HasMotor = false;
    }

    [Serializable]
    public class StationInfo
    {
        public string Name;
        public string StopPosName;
        public TimeData ArvTime;
        public TimeData DepTime;
        public string doorDir;
        public string stopType;
        public float TotalLength = 0;
    }

    public enum PanelLamp
    {
        /// <summary>
        /// ●戸閉
        /// </summary>
        DoorClose,
        /// <summary>
        /// ATS正常
        /// </summary>
        ATS_Ready,
        /// <summary>
        /// ATS動作
        /// </summary>
        ATS_BrakeApply,
        /// <summary>
        /// ATS開放
        /// </summary>
        ATS_Open,
        /// <summary>
        /// 回生
        /// </summary>
        RegenerativeBrake,
        /// <summary>
        /// EB
        /// </summary>
        EB_Timer,
        /// <summary>
        /// 非常ブレーキ
        /// </summary>
        EmagencyBrake,
        /// <summary>
        /// 過負荷
        /// </summary>
        Overload,
    }

    [Flags] // ビット演算をサポートするためFlags属性を付与
    public enum AtsState
    {
        P = 1,
        P接近 = 1 << 1,
        B動作 = 1 << 2,
        EB = 1 << 3,
        終端P = 1 << 4,
        停P = 1 << 5,
        OFF = 1 << 6
    }
}
