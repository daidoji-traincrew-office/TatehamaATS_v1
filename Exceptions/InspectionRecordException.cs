namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// FF:検査記録部未定義故障
    /// </summary>
    internal class InspectionRecordException : ATSCommonException
    {
        /// <summary>
        /// FF:検査記録部未定義故障
        /// </summary>
        public InspectionRecordException(int place) : base(place)
        {
        }
        /// <summary>
        /// FF:検査記録部未定義故障
        /// </summary>
        public InspectionRecordException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// FF:検査記録部未定義故障
        /// </summary>
        public InspectionRecordException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "FE";
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
