using System.Windows.Forms;

namespace TatehamaATS_v1.Exceptions
{
    /// <summary>
    /// 復旧条件の種類を表す列挙型。
    /// </summary>
    public enum ResetConditions
    {
        /// <summary>故障復旧のみで復旧</summary>
        ExceptionReset,
        /// <summary>列番等再設定</summary>
        RetsubanReset,
        /// <summary>停車検知・TC接続リセット</summary>
        StopDetection_ConnectionReset,
        /// <summary>停車検知</summary>
        StopDetection,
        /// <summary>停車検知・マスコン非常</summary>
        StopDetection_MasconEB,
        /// <summary>停車検知・マスコン非常・ATS復帰</summary>
        StopDetection_MasconEB_ATSReset,
        /// <summary>電源再投入</summary>
        PowerReset,
    }
    public enum OutputBrake
    {
        None,
        EB
    }

    /// <summary>       
    /// F0:未定義故障
    /// </summary>
    internal class ATSCommonException : Exception
    {
        /// <summary>
        /// 系コード
        /// </summary>
        public int Place;

        /// <summary>      
        /// F0:未定義故障
        /// </summary>
        public ATSCommonException(int place)
        {
            Place = place;
        }
        /// <summary>        
        /// F0:未定義故障
        /// </summary>
        public ATSCommonException(int place, string message)
            : base(message)
        {
            Place = place;
        }
        /// <summary>
        /// F0:未定義故障
        /// </summary>
        public ATSCommonException(int place, string message, Exception inner)
            : base(message, inner)
        {
            Place = place;
        }
        public virtual string ToCode()
        {
            return Place.ToString() + "F0";
        }
        public virtual ResetConditions ResetCondition()
        {
            return ResetConditions.PowerReset;
        }
        public virtual OutputBrake ToBrake()
        {
            return OutputBrake.EB;
        }
    }
}
