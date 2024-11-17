using System;
using System.Diagnostics;
using System.Xml.Linq;
using TatehamaATS_v1.Exceptions;

namespace TatehamaATS_v1.OnboardDevice
{
    /// <summary>
    /// 検査記録部
    /// </summary>
    internal class InspectionRecord
    {
        /// <summary>
        /// 故障情報マスター
        /// </summary>
        internal static Dictionary<string, ATSCommonException> exceptions = new Dictionary<string, ATSCommonException>();
        /// <summary>
        /// 故障時間
        /// </summary>
        internal static Dictionary<string, DateTime> exceptionsTime = new Dictionary<string, DateTime>();

        /// <summary>
        /// 故障発生から故障復帰までの最低時間
        /// </summary>
        private static TimeSpan resetTime = TimeSpan.FromSeconds(3.5);

        public bool RetsubanReset;
        public bool RelayReset;
        public bool ATSReset;
        private bool PowerReset;
        private TimeSpan RelayUpdatedTime;
        internal TrainCrewStateData TcData;

        /// <summary>
        /// 非常状態変更
        /// </summary>
        internal event Action<bool> EmBrakeStateUpdated;

        /// <summary>
        /// 故障発生
        /// </summary>
        internal event Action<ATSCommonException> AddExceptionAction;

        /// <summary>
        /// 故障コード一覧変更
        /// </summary>
        internal event Action<List<string>> ExceptionCodesChenge;

        /// <summary>
        /// 検査記録部
        /// </summary>
        internal InspectionRecord()
        {
            exceptions.Clear();
            exceptionsTime.Clear();
            RetsubanReset = false;
            RelayReset = false;
            ATSReset = false;
            PowerReset = false;
            TcData = new TrainCrewStateData();
            Task.Run(() => StartDisplayUpdateLoop());
        }

        /// <summary>
        /// 非同期で更新するループを開始する
        /// </summary>
        private async void StartDisplayUpdateLoop()
        {
            while (true)
            {
                var timer = Task.Delay(100);

                try
                {
                    //通信途絶確認
                    var NowGame = TcData.gameScreen == GameScreen.MainGame || TcData.gameScreen == GameScreen.MainGame_Pause;
                    if (DateTime.Now.TimeOfDay - RelayUpdatedTime > TimeSpan.FromSeconds(2) && NowGame)
                    {
                        var e = new RelayIOConnectionException(3, "継電部通信途絶2秒異常");
                        AddExceptionAction.Invoke(e);
                    }
                    //リセット条件確認
                    ResetException();
                    //故障コード一覧更新
                    ExceptionCodesChenge?.Invoke(exceptions.Values.Select(e => e.ToCode()).ToList());
                    //非常制動？
                    if (exceptions.Any(x => x.Value.ToBrake() == OutputBrake.EB))
                    {
                        EmBrakeStateUpdated?.Invoke(true);
                    }
                    else
                    {
                        EmBrakeStateUpdated?.Invoke(false);
                    }
                }
                catch (ATSCommonException ex)
                {
                    AddExceptionAction.Invoke(ex);
                }
                catch (Exception ex)
                {
                    var e = new LEDControlException(3, "", ex);
                    AddExceptionAction.Invoke(e);
                }
                await timer;
            }
        }


        /// <summary>
        /// 故障追加
        /// </summary>
        /// <param name="exception"></param>
        internal void AddException(ATSCommonException exception)
        {
            EmBrakeStateUpdated?.Invoke(true);
            Debug.WriteLine($"故障追加");
            Debug.WriteLine($"{exception.Message} {exception.InnerException}");
            exceptions[exception.ToCode()] = exception;
            exceptionsTime[exception.ToCode()] = DateTime.Now;
        }

        /// <summary>
        /// 故障追加
        /// </summary>
        /// <param name="exception"></param>
        internal void AddException(Exception exception)
        {
            exceptions.Add("39F", new CsharpException(3, exception.ToString()));
            exceptionsTime.Add("39F", DateTime.Now);
        }

        /// <summary>
        /// リセット条件達成確認
        /// </summary>
        internal void ResetException()
        {
            var StopDetection = TcData.myTrainData.Speed == 0;
            var MasconEB = TcData.myTrainData.Bnotch == 8;
            foreach (var ex in exceptions)
            {
                string key = ex.Key;
                TimeSpan time = DateTime.Now - exceptionsTime[key];

                switch (ex.Value.ResetCondition())
                {
                    case ResetConditions.ExceptionReset:
                        if (time < resetTime)
                        {
                            //故障復帰最低時間を下回っている場合無視
                        }
                        else
                        {
                            exceptions.Remove(key);
                        }
                        break;
                    case ResetConditions.RetsubanReset:
                        if (RetsubanReset) exceptions.Remove(key);
                        break;
                    case ResetConditions.StopDetection_ConnectionReset:
                        if (RelayReset) exceptions.Remove(key);
                        break;
                    case ResetConditions.StopDetection:
                        if (time < resetTime)
                        {
                            //故障復帰最低時間を下回っている場合無視
                        }
                        else
                        {
                            if (StopDetection) exceptions.Remove(key);
                        }
                        break;
                    case ResetConditions.StopDetection_MasconEB:
                        if (time < resetTime)
                        {
                            //故障復帰最低時間を下回っている場合無視
                        }
                        else
                        {
                            if (StopDetection && MasconEB) exceptions.Remove(key);
                        }
                        break;
                    case ResetConditions.StopDetection_MasconEB_ATSReset:
                        if (time < resetTime)
                        {
                            //故障復帰最低時間を下回っている場合無視
                        }
                        else
                        {
                            if (StopDetection && MasconEB && ATSReset) exceptions.Remove(key);
                        }
                        break;
                }
            }
            if (StopDetection)
            {
                RelayReset = false;
            }
            RetsubanReset = false;
            ATSReset = false;
        }

        internal void RelayUpdate(TrainCrewStateData tcData)
        {
            TcData = tcData;
            RelayUpdatedTime = DateTime.Now.TimeOfDay;
        }
    }
}
