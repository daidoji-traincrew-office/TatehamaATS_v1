namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// C2:地上子情報異常
    /// </summary>
    internal class TransponderInfoAbnormal : ATSCommonException
    {
        /// <summary>                
        /// C2:地上子情報異常
        /// </summary>
        public TransponderInfoAbnormal(int place) : base(place)
        {
        }
        /// <summary>                   
        /// C2:地上子情報異常
        /// </summary>
        public TransponderInfoAbnormal(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                   
        /// C2:地上子情報異常
        /// </summary>
        public TransponderInfoAbnormal(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "C2";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.StopDetection_MasconEB_ATSReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
