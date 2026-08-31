namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 9C:車上DB閉塞データ編集異常
    /// </summary>
    internal class DBTrackDataChengeAbnormal : ATSCommonException
    {
        /// <summary>
        /// 9C:車上DB閉塞データ編集異常
        /// </summary>
        public DBTrackDataChengeAbnormal(int place) : base(place)
        {
        }
        /// <summary>
        /// 9C:車上DB閉塞データ編集異常
        /// </summary>
        public DBTrackDataChengeAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 9C:車上DB閉塞データ編集異常
        /// </summary>
        public DBTrackDataChengeAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "9C";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.StopDetection_MasconEB;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
