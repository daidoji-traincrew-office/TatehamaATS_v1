namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 9D:車上DB閉塞データ異常
    /// </summary>
    internal class OnCarDBTrackDataAbnormal : ATSCommonException
    {
        /// <summary>
        /// 9D:車上DB閉塞データ異常
        /// </summary>
        public OnCarDBTrackDataAbnormal(int place) : base(place)
        {
        }
        /// <summary>
        /// 9D:車上DB閉塞データ異常
        /// </summary>
        public OnCarDBTrackDataAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 9D:車上DB閉塞データ異常
        /// </summary>
        public OnCarDBTrackDataAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "9D";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.RetsubanReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}