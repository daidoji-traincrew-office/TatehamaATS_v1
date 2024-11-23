namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// E3:データ有効期限切れ
    /// </summary>
    internal class SocketDataExpired : ATSCommonException
    {
        /// <summary>
        /// E3:データ有効期限切れ
        /// </summary>
        public SocketDataExpired(int place) : base(place)
        {
        }
        /// <summary>
        /// E3:データ有効期限切れ
        /// </summary>
        public SocketDataExpired(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// E3:データ有効期限切れ
        /// </summary>
        public SocketDataExpired(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "E3";
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
