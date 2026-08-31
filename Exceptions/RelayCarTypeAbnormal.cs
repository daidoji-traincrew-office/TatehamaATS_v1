namespace TatehamaATS_v1.Exceptions
{
    /// <summary>              
    /// C8:車種異常
    /// </summary>
    internal class RelayCarTypeAbnormal : ATSCommonException
    {
        /// <summary>              
        /// C8:車種異常
        /// </summary>
        public RelayCarTypeAbnormal(int place) : base(place)
        {
        }
        /// <summary>           
        /// C8:車種異常
        /// </summary>
        public RelayCarTypeAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// C8:車種異常
        /// </summary>
        public RelayCarTypeAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "C8";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.StopDetection;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
