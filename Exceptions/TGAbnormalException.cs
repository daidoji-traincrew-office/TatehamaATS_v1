using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// C0:TG異常
    /// </summary>
    internal class TGAbnormalException : ATSCommonException
    {
        /// <summary>
        /// C0:TG異常
        /// </summary>
        public TGAbnormalException(int place) : base(place)
        {
        }
        /// <summary>
        /// C0:TG異常
        /// </summary>
        public TGAbnormalException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// C0:TG異常
        /// </summary>
        public TGAbnormalException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "C0";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.PowerReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}