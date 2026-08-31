namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// CE:継電部カウンタ異常
    /// </summary>
    internal class RelayCountaException : ATSCommonException
    {
        /// <summary>
        /// CE:継電部カウンタ異常
        /// </summary>
        public RelayCountaException(int place) : base(place)
        {
        }
        /// <summary>
        /// CE:継電部カウンタ異常
        /// </summary>
        public RelayCountaException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// CE:継電部カウンタ異常
        /// </summary>
        public RelayCountaException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "CE";
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
