using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaATS_v1.Exceptions
{   /// <summary>                   
    /// 87:地上認証拒否
    /// </summary>
    internal class NetworkAccessDenied : ATSCommonException
    {
        /// <summary>
        /// 87:地上認証拒否
        /// </summary>
        public NetworkAccessDenied(int place) : base(place)
        {
        }
        /// <summary>                
        /// 87:地上認証拒否
        /// </summary>
        public NetworkAccessDenied(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>                 
        /// 87:地上認証拒否
        /// </summary>
        public NetworkAccessDenied(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "87";
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
