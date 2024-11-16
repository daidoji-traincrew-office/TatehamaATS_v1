namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 94:車上DBデータ異常
    /// </summary>
    internal class RelayIOConnectionException : ATSCommonException
    {
        /// <summary>
        /// 94:車上DBデータ異常
        /// </summary>
        public RelayIOConnectionException(int place) : base(place)
        {
        }
        /// <summary>
        /// 94:車上DBデータ異常
        /// </summary>
        public RelayIOConnectionException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 94:車上DBデータ異常
        /// </summary>
        public RelayIOConnectionException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "94";
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