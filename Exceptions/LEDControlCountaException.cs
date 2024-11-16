namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// BE:LED制御部カウンタ異常
    /// </summary>
    internal class LEDControlCountaException : ATSCommonException
    {
        /// <summary>
        /// BE:LED制御部カウンタ異常
        /// </summary>
        public LEDControlCountaException(int place) : base(place)
        {
        }
        /// <summary>
        /// BE:LED制御部カウンタ異常
        /// </summary>
        public LEDControlCountaException(int place, string message)
            : base(place, message)
        {
        }
        /// <summary>
        /// BE:LED制御部カウンタ異常
        /// </summary>
        public LEDControlCountaException(int place, string message, Exception inner)
            : base(place, message, inner)
        {
        }
        public override string ToCode()
        {
            return Place.ToString() + "BE";
        }
        public override ResetConditions ResetCondition()
        {
            return ResetConditions.PowerReset;
        }
        public override OutputBrake ToBrake()
        {
            return OutputBrake.None;
        }
    }
}


