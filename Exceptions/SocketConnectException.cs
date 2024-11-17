namespace TatehamaATS_v1.Exceptions
{
    /// <summary>                   
    /// ED:Socket接続失敗
    /// </summary>
    internal class SocketConnectException : ATSCommonException
    {
        /// <summary>
        /// ED:Socket接続失敗
        /// </summary>
        public SocketConnectException(int place) : base(place)
        {
        }
        /// <summary>                
        /// ED:Socket接続失敗
        /// </summary>
        public SocketConnectException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                 
        /// ED:Socket接続失敗
        /// </summary>
        public SocketConnectException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "ED";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.StopDetection;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
