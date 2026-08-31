namespace TatehamaATS_v1.Exceptions
{
    /// <summary>                   
    /// EC:地上接続失敗
    /// </summary>
    internal class NetworkNonConnectException : ATSCommonException
    {
        /// <summary>
        /// EC:地上接続失敗
        /// </summary>
        public NetworkNonConnectException(int place) : base(place)
        {
        }
        /// <summary>                
        /// EC:地上接続失敗
        /// </summary>
        public NetworkNonConnectException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                 
        /// EC:地上接続失敗
        /// </summary>
        public NetworkNonConnectException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "EC";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.NetworkReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
