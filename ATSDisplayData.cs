using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaATS_v1
{
    internal class ATSDisplayData
    {
        /// <summary>
        /// 表示器上段：種別
        /// </summary>
        internal string L1 { get; private set; }
        /// <summary>
        /// 表示器中段：速度
        /// </summary>
        internal string L2 { get; private set; }
        /// <summary>
        /// 表示器下段：動作状態
        /// </summary>
        internal List<string> L3 { get; set; }

        internal bool FromTC { get; private set; }

        public ATSDisplayData(string L1, string L2, string[] L3, bool fromTC = false)
        {
            this.L1 = L1;
            this.L2 = L2;
            this.L3 = new List<string>();
            this.L3.AddRange(L3);
            this.FromTC = fromTC;
        }

        /// <summary>
        /// 表示内容を変更する
        /// </summary>
        public void SetLED(string L1, string L2)
        {
            this.L1 = L1;
            this.L2 = L2;
        }

        /// <summary>
        /// 状態を変更する。
        /// </summary>
        public void AddState(string addL3)
        {
            if (!L3.Contains(addL3))
            {
                L3.Add(addL3);
            }
            //通常表示の共存不可関係
            switch (addL3)
            {
                case "":
                case "無表示":
                    RemoveState("P");
                    RemoveState("停P");
                    RemoveState("終端P");
                    RemoveState("P接近");
                    RemoveState("B動作");
                    RemoveState("EB");
                    break;
                case "P":
                case "停P":
                case "終端P":
                    RemoveState("");
                    RemoveState("無表示");
                    RemoveState("P接近");
                    RemoveState("B動作");
                    RemoveState("EB");
                    break;
                case "P接近":
                    RemoveState("");
                    RemoveState("無表示");
                    RemoveState("P");
                    RemoveState("停P");
                    RemoveState("終端P");
                    RemoveState("B動作");
                    RemoveState("EB");
                    L3.Add("");
                    break;
                case "B動作":
                    RemoveState("");
                    RemoveState("無表示");
                    RemoveState("P");
                    RemoveState("停P");
                    RemoveState("終端P");
                    RemoveState("P接近");
                    RemoveState("EB");
                    L3.Add("");
                    break;
                case "EB":
                    RemoveState("");
                    RemoveState("無表示");
                    RemoveState("P");
                    RemoveState("停P");
                    RemoveState("終端P");
                    RemoveState("P接近");
                    RemoveState("B動作");
                    L3.Add("");
                    break;
            }

        }

        /// <summary>
        /// 状態から1要素除く
        /// </summary>
        /// <param name="removeL3"></param>
        public void RemoveState(string removeL3)
        {
            L3.Remove(removeL3);
        }

        public override string ToString()
        {
            return $"{L1}/{L2}/{string.Join(",", L3)}";
        }

        /// <summary>
        /// 渡された数値がエラーコードか判定する。
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        private bool isErrorCode(string NumberStr)
        {

            //16進数で解釈できる場合=故障表示の可能性
            if (int.TryParse(NumberStr, System.Globalization.NumberStyles.HexNumber, null, out _))
            {
                int parse = int.Parse(NumberStr, System.Globalization.NumberStyles.HexNumber);
                //数値が故障表示範囲内の場合
                if (0x180 <= parse && parse <= 0x1FF || 0x280 <= parse && parse <= 0x2FF || 0x380 <= parse && parse <= 0x3FF)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
