namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// C9:車両情報異常
    /// </summary>
    internal class RelayCarInfoAbnormal : ATSCommonException
    {
        /// <summary>         
        /// C9:車両情報異常
        /// </summary>
        public RelayCarInfoAbnormal(int place) : base(place)
        {
        }
        /// <summary>         
        /// C9:車両情報異常
        /// </summary>
        public RelayCarInfoAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>            
        /// C9:車両情報異常
        /// </summary>
        public RelayCarInfoAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "C9";
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
