namespace TatehamaATS_v1.Exceptions
{
    /// <summary>                  
    /// 85:伝送部初期化失敗
    /// </summary>
    internal class TransferInitialzingFailure : ATSCommonException
    {
        /// <summary>
        /// 85:伝送部初期化失敗
        /// </summary>
        public TransferInitialzingFailure(int place) : base(place)
        {
        }
        /// <summary>
        /// 85:伝送部初期化失敗
        /// </summary>
        public TransferInitialzingFailure(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// 85:伝送部初期化失敗
        /// </summary>
        public TransferInitialzingFailure(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "85";
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
