namespace TatehamaATS_v1.Exceptions
{
    /// <summary>              
    /// 95:継電部・検査記録部伝送異常
    /// </summary>
    internal class RelayIOConnectionException : ATSCommonException
    {
        /// <summary>               
        /// 95:継電部・検査記録部伝送異常
        /// </summary>
        public RelayIOConnectionException(int place) : base(place)
        {
        }
        /// <summary>            
        /// 95:継電部・検査記録部伝送異常
        /// </summary>
        public RelayIOConnectionException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>              
        /// 95:継電部・検査記録部伝送異常
        /// </summary>
        public RelayIOConnectionException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "95";
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