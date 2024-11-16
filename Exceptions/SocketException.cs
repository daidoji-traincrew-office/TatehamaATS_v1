namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// EF:通信部未定義故障
    /// </summary>
    internal class SocketException : ATSCommonException
    {
        /// <summary>
        /// EF:通信部未定義故障
        /// </summary>
        public SocketException(int place) : base(place)
        {
        }
        /// <summary>
        /// EF:通信部未定義故障
        /// </summary>
        public SocketException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// EF:通信部未定義故障
        /// </summary>
        public SocketException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "EF";
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
