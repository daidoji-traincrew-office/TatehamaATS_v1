namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// EE:通信部カウンタ異常
    /// </summary>
    internal class NetworkCountaException : ATSCommonException
    {
        /// <summary>
        /// EE:通信部カウンタ異常
        /// </summary>
        public NetworkCountaException(int place) : base(place)
        {
        }
        /// <summary>
        /// EE:通信部カウンタ異常
        /// </summary>
        public NetworkCountaException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// EE:通信部カウンタ異常
        /// </summary>
        public NetworkCountaException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "EE";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.StopDetection_NetworkReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
