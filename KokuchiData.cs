using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaATS_v1
{
    public class KokuchiData
    {
        public KokuchiType Type;
        public string DisplayData;
        public DateTime OriginTime;

        public KokuchiData(KokuchiType Type, string DisplayData, DateTime OriginTime)
        {
            this.Type = Type;
            this.DisplayData = DisplayData;
            this.OriginTime = OriginTime;
        }
    }

    public enum KokuchiType
    {
        None,
        Yokushi,
        Tsuuchi,
        TsuuchiKaijo,
        Kaijo,
        Shuppatsu,
        ShuppatsuJikoku,
        Torikeshi,
        Tenmatsusho
    }
}
