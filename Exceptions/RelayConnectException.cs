namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// CD:継電部接続異常
    /// </summary>
    internal class RelayConnectException : ATSCommonException
    {
        /// <summary>
        /// CD:継電部接続異常
        /// </summary>
        public RelayConnectException(int place) : base(place)
        {
        }
        /// <summary>
        /// CD:継電部接続異常
        /// </summary>
        public RelayConnectException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// CD:継電部接続異常
        /// </summary>
        public RelayConnectException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "CD";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.StopDetection_ConnectionReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
