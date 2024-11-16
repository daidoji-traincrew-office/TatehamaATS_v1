namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// CA:ほか情報異常
    /// </summary>
    internal class RelayOtherInfoAbnormal : ATSCommonException
    {
        /// <summary>            
        /// CA:ほか情報異常
        /// </summary>
        public RelayOtherInfoAbnormal(int place) : base(place)
        {
        }
        /// <summary>          
        /// CA:ほか情報異常
        /// </summary>
        public RelayOtherInfoAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>         
        /// CA:ほか情報異常
        /// </summary>
        public RelayOtherInfoAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "CA";
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
