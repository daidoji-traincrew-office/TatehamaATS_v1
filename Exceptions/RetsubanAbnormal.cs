namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 90:列番異常
    /// </summary>
    internal class RetsubanAbnormal : ATSCommonException
    {
        /// <summary>
        /// 90:列番異常
        /// </summary>
        public RetsubanAbnormal(int place) : base(place)
        {
        }
        /// <summary>
        /// 90:列番異常
        /// </summary>
        public RetsubanAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 90:列番異常
        /// </summary>
        public RetsubanAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "90";
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
