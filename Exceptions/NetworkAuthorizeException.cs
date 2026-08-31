
namespace TatehamaATS_v1.Exceptions
{
    /// <summary>                   
    /// 88:地上認証失敗
    /// </summary>
    internal class NetworkAuthorizeException : ATSCommonException
    {
        /// <summary>
        /// 88:地上認証失敗
        /// </summary>
        public NetworkAuthorizeException(int place) : base(place)
        {
        }
        /// <summary>                
        /// 88:地上認証失敗
        /// </summary>
        public NetworkAuthorizeException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                 
        /// 88:地上認証失敗
        /// </summary>
        public NetworkAuthorizeException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "88";
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
