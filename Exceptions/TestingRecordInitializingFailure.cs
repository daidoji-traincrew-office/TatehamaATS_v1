namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 80:検査記録部初期化失敗
    /// </summary>
    internal class TestingRecordInitializingFailure : ATSCommonException
    {
        /// <summary>
        /// 80:検査記録部初期化失敗
        /// </summary>
        public TestingRecordInitializingFailure(int place) : base(place)
        {
        }
        /// <summary>
        /// 80:検査記録部初期化失敗
        /// </summary>
        public TestingRecordInitializingFailure(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 80:検査記録部初期化失敗
        /// </summary>
        public TestingRecordInitializingFailure(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "80";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.PowerReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}