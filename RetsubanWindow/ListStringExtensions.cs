using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaATS_v1.RetsubanWindow
{
    public static class ListStringExtensions
    {
        /// <summary>
        /// 文字配置を行う拡張メソッド
        /// </summary>
        /// <param name="list">対象リスト</param>
        /// <param name="str">設定文字列</param>
        /// <param name="x">横の文字位置</param>
        /// <param name="y">縦の文字位置</param>
        /// <param name="isY">縦書きかどうか</param>
        /// <returns>変更後のリスト</returns>
        public static List<string> SetDisplayData(this List<string> list, string str, int x, int y, bool isY)
        {
            // 開始位置を求める
            if (x < 0 || y < 0 || x > 16 || y > 3)
            {
                throw new ArgumentOutOfRangeException("x or y is out of range.");
            }
            var startPosition = y * 16 + x;
            if (isY) // 縦書きの場合
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int index = startPosition + i * 16;
                    // 存在しないインデックスの場合、Listのサイズを拡張する
                    if (index >= list.Count)
                    {
                        for (int j = list.Count; j <= index; j++)
                        {
                            list.Add(" "); // 空白で埋める
                        }
                    }
                    list[index] = str[i].ToString();
                }
            }
            else // 横書きの場合
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int index = startPosition + i;
                    // 存在しないインデックスの場合、Listのサイズを拡張する
                    if (index >= list.Count)
                    {
                        for (int j = list.Count; j <= index; j++)
                        {
                            list.Add(" "); // 空白で埋める
                        }
                    }
                    list[index] = str[i].ToString();
                }
            }
            return list;
        }
    }
}
