namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 83:LED制御部初期化失敗
    /// </summary>
    internal class LEDControlInitialzingFailure : ATSCommonException
    {
        /// <summary>
        /// 83:LED制御部初期化失敗
        /// </summary>
        public LEDControlInitialzingFailure(int place) : base(place)
        {
        }
        /// <summary>
        /// 83:LED制御部初期化失敗
        /// </summary>
        public LEDControlInitialzingFailure(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 83:LED制御部初期化失敗
        /// </summary>
        public LEDControlInitialzingFailure(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "83";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.PowerReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.None;
        }
    }
}