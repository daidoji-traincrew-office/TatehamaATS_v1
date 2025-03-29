namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 97:通信部・検査記録部伝送異常
    /// </summary>
    internal class NetworkIOConnectionException : ATSCommonException
    {
        /// <summary>
        /// 97:通信部・検査記録部伝送異常
        /// </summary>
        public NetworkIOConnectionException(int place) : base(place)
        {
        }
        /// <summary>
        /// 97:通信部・検査記録部伝送異常
        /// </summary>
        public NetworkIOConnectionException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 97:通信部・検査記録部伝送異常
        /// </summary>
        public NetworkIOConnectionException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "97";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.ExceptionReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}