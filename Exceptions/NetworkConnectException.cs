namespace TatehamaATS_v1.Exceptions
{
    /// <summary>                   
    /// ED:地上接続失敗
    /// </summary>
    internal class NetworkConnectException : ATSCommonException
    {
        /// <summary>
        /// ED:地上接続失敗
        /// </summary>
        public NetworkConnectException(int place) : base(place)
        {
        }
        /// <summary>                
        /// ED:地上接続失敗
        /// </summary>
        public NetworkConnectException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                 
        /// ED:地上接続失敗
        /// </summary>
        public NetworkConnectException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "ED";
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
