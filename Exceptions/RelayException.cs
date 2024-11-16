using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// CF:継電部未定義故障
    /// </summary>
    internal class RelayException : ATSCommonException
    {
        /// <summary>
        /// CF:継電部未定義故障
        /// </summary>
        public RelayException(int place) : base(place)
        {
        }
        /// <summary>
        /// CF:継電部未定義故障
        /// </summary>
        public RelayException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// CF:継電部未定義故障
        /// </summary>
        public RelayException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "CF";
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