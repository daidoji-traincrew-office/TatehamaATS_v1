namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// FE:検査記録部カウンタ異常
    /// </summary>
    internal class InspectionRecordCountaException : ATSCommonException
    {
        /// <summary>
        /// FE:検査記録部カウンタ異常
        /// </summary>
        public InspectionRecordCountaException(int place) : base(place)
        {
        }
        /// <summary>
        /// FE:検査記録部カウンタ異常
        /// </summary>
        public InspectionRecordCountaException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// FE:検査記録部カウンタ異常
        /// </summary>
        public InspectionRecordCountaException(int place, string message, Exception inner)
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
