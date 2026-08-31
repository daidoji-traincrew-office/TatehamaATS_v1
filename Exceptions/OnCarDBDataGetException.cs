namespace TatehamaATS_v1.Exceptions
{
    /// <summary>                    
    /// 9B:車上DB閉塞データ取得失敗
    /// </summary>
    internal class OnCarDBDataGetException : ATSCommonException
    {
        /// <summary>
        /// 9B:車上DB閉塞データ取得失敗
        /// </summary>
        public OnCarDBDataGetException(int place) : base(place)
        {
        }
        /// <summary>                   
        /// 9B:車上DB閉塞データ取得失敗
        /// </summary>
        public OnCarDBDataGetException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                  
        /// 9B:車上DB閉塞データ取得失敗
        /// </summary>
        public OnCarDBDataGetException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "9B";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.RetsubanReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
