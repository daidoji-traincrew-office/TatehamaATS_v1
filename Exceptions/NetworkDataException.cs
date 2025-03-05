namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// E4:データ異常
    /// </summary>
    internal class NetworkDataException : ATSCommonException
    {
        /// <summary>
        /// E4:データ異常
        /// </summary>
        public NetworkDataException(int place) : base(place)
        {
        }
        /// <summary>
        /// E4:データ異常
        /// </summary>
        public NetworkDataException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// E4:データ異常
        /// </summary>
        public NetworkDataException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "E4";
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
