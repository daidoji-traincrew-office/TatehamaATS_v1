using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaATS_v1.OnboardDevice
{
    internal class StopPassManager
    {
        internal String TypeName = "";
        internal String TypeNameTC = "";
        internal String TypeNameKana = "";

        private Dictionary<string, string> StaNameById = new Dictionary<string, string>()
        {{"TH76","館浜"},{"TH75","駒野"},{"TH74","河原崎"},{"TH73","海岸公園"},{"TH72","虹ケ浜"},{"TH71","津崎"},{"TH70","浜園"},{"TH69","羽衣橋"},{"TH68","新井川"},{"TH67","新野崎"},{"TH66","江ノ原"},{"TH66S","江ノ原信号場"},{"TH65","大道寺"},{"TH64","藤江"},{"TH63","水越"},{"TH62","高見沢"},{"TH61","日野森"},{"TH60","奥峯口"},{"TH59","西赤山"},{"TH58","赤山町"},{"TH57","三郷"},{"TH56","明神川"},{"TH55S","珠川信号場"},{"TH55","珠川温泉"},{"TH54","上吉沢"},{"TH53","下吉沢"},{"TH52","名田"},{"TH51","三石"},{"TH50","二木戸"},{"TH49","白石町"},{"TH48","東井"},{"TH47","桜坂"},{"TH46","新大路"},{"TH45","大路"},{"TH44","常盤通"},{"TH43","広小路"},{"TH42","田村"},{"TH41","二ツ山"},{"TH40","小沼"},{"TH39","六日市町"},{"TH38","朝日ヶ丘"},{"TH37","大野宮"},{"TH36","沢井"},{"TH35","箕田"},{"TH34","江西"},{"TH33","大和田町"},{"TH32","木之本"},{"TH31","新長野公園"},{"TH30","長野本町"},{"TH29","高砂町"},{"TH28","東福"},{"TH27","千里が丘"},{"TH26","矢木"},{"TH25","北美"},{"TH24","緑ヶ丘"},{"TH23","町沢"},{"TH22","大原"},{"TH21","本郷"},{"TH20","五十川"},{"TH19","出屋敷前"},{"TH18","佐川"},{"TH17","夕陽が丘"},{"TH16","五日市"},{"TH15","南五日市"},{"TH14","高井戸八幡"},{"TH13","南八幡"},{"TH12","学院前"},{"TH11","中道"},{"TH10","佐野"},{"TH09","小手川"},{"TH08","南吉岡"},{"TH07","宮の前"},{"TH06","神宮橋"},{"TH05","宮松町"},{"TH04","青木町"},{"TH03","新町"},{"TH02","三番街"},{"TH01","大手橋"},{"TH00","ダミー"},}
        ;

        private Dictionary<string, string> StaKanaById = new Dictionary<string, string>()
        {{"TH76","テハ"},{"TH75","コマ"},{"TH74","ワサ"},{"TH73","イカ"},{"TH72","ニハ"},{"TH71","ツサ"},{"TH70","ハソ"},{"TH69","ハモ"},{"TH68","ライ"},{"TH67","ノキ"},{"TH66","エラ"},{"TH66S","ラシ"},{"TH65","タイ"},{"TH64","フエ"},{"TH63","スコ"},{"TH62","タサ"},{"TH61","ヒモ"},{"TH60","ミツ"},{"TH59","ニア"},{"TH58","アマ"},{"TH57","サコ"},{"TH56","ミヨ"},{"TH55S","カシ"},{"TH55","タマ"},{"TH54","ヨシ"},{"TH53","シワ"},{"TH52","ナタ"},{"TH51","ツシ"},{"TH50","フキ"},{"TH49","シシ"},{"TH48","トイ"},{"TH47","サラ"},{"TH46","シオ"},{"TH45","オオ"},{"TH44","キリ"},{"TH43","ロシ"},{"TH42","タラ"},{"TH41","タヤ"},{"TH40","コヌ"},{"TH39","ムイ"},{"TH38","サオ"},{"TH37","オミ"},{"TH36","サイ"},{"TH35","ミタ"},{"TH34","エニ"},{"TH33","ホワ"},{"TH32","キモ"},{"TH31","シエ"},{"TH30","ナノ"},{"TH29","サチ"},{"TH28","トフ"},{"TH27","リカ"},{"TH26","ヤキ"},{"TH25","キミ"},{"TH24","リカ"},{"TH23","マワ"},{"TH22","オハ"},{"TH21","ホコ"},{"TH20","イソ"},{"TH19","テキ"},{"TH18","サカ"},{"TH17","ユイ"},{"TH16","イカ"},{"TH15","ミチ"},{"TH14","タハ"},{"TH13","ナワ"},{"TH12","カエ"},{"TH11","ナチ"},{"TH10","サノ"},{"TH09","コカ"},{"TH08","ミオ"},{"TH07","ミエ"},{"TH06","シン"},{"TH05","ヤツ"},{"TH04","アチ"},{"TH03","シチ"},{"TH02","ハイ"},{"TH01","オテ"},{"TH00",""},}
        ;

        private Dictionary<string, string> StopDataById = new Dictionary<string, string>()
        {{"TH76","停車"},{"TH75","停車"},{"TH74","停車"},{"TH73","停車"},{"TH72","停車"},{"TH71","停車"},{"TH70","停車"},{"TH69","停車"},{"TH68","停車"},{"TH67","停車"},{"TH66","停車"},{"TH66S","停車"},{"TH65","停車"},{"TH64","停車"},{"TH63","停車"},{"TH62","停車"},{"TH61","停車"},{"TH60","停車"},{"TH59","停車"},{"TH58","停車"},{"TH57","停車"},{"TH56","停車"},{"TH55S","停車"},{"TH55","停車"},{"TH54","停車"},{"TH53","停車"},{"TH52","停車"},{"TH51","停車"},{"TH50","停車"},{"TH49","停車"},{"TH48","停車"},{"TH47","停車"},{"TH46","停車"},{"TH45","停車"},{"TH44","停車"},{"TH43","停車"},{"TH42","停車"},{"TH41","停車"},{"TH40","停車"},{"TH39","停車"},{"TH38","停車"},{"TH37","停車"},{"TH36","停車"},{"TH35","停車"},{"TH34","停車"},{"TH33","停車"},{"TH32","停車"},{"TH31","停車"},{"TH30","停車"},{"TH29","停車"},{"TH28","停車"},{"TH27","停車"},{"TH26","停車"},{"TH25","停車"},{"TH24","停車"},{"TH23","停車"},{"TH22","停車"},{"TH21","停車"},{"TH20","停車"},{"TH19","停車"},{"TH18","停車"},{"TH17","停車"},{"TH16","停車"},{"TH15","停車"},{"TH14","停車"},{"TH13","停車"},{"TH12","停車"},{"TH11","停車"},{"TH10","停車"},{"TH09","停車"},{"TH08","停車"},{"TH07","停車"},{"TH06","停車"},{"TH05","停車"},{"TH04","停車"},{"TH03","停車"},{"TH02","停車"},{"TH01","停車"},{"TH00",""},};

        internal List<string> GetAllStationIds()
        {
            return StaNameById.Keys.ToList();
        }

        internal string GetStationIdByName(string name)
        {
            var entry = StaNameById.FirstOrDefault(x => x.Value == name);
            if (entry.Key != null)
            {
                return entry.Key;
            }
            return "TH00";
        }

        internal string GetStationNameById(string id)
        {
            if (StaNameById.TryGetValue(id, out string name))
            {
                return name;
            }
            return "不明駅";
        }

        internal string GetStationKanaById(string id)
        {
            if (StaKanaById.TryGetValue(id, out string name))
            {
                return name;
            }
            return "？？";
        }

        internal string GetStopDataById(string id)
        {
            if (StopDataById.TryGetValue(id, out string name))
            {
                return name;
            }
            return "？？";
        }

        internal void SetStopDataById(string id, string data)
        {
            if (StopDataById.ContainsKey(id))
            {
                StopDataById[id] = data;
            }
        }

        internal void TypeString(string Retsuban)
        {
            Retsuban = Retsuban.Replace("X", "").Replace("Y", "").Replace("Z", "");
            if (Retsuban == "9999")
            {
                TypeName = "";
                return;
            }
            if (Retsuban.Contains("溝月"))
            {
                TypeName = "溝月";
                return;
            }
            if (Retsuban.StartsWith("回"))
            {
                TypeName = "回送";
                return;
            }
            if (Retsuban.StartsWith("試"))
            {
                TypeName = "試運転";
                return;
            }
            if (Retsuban.Contains("A"))
            {
                if (Retsuban.StartsWith("臨"))
                {
                    TypeName = "臨時特急";
                    return;
                }
                TypeName = "特急";
                return;
            }
            if (Retsuban.Contains("K"))
            {
                if (Retsuban.StartsWith("臨"))
                {
                    TypeName = "臨時快速急行";
                    return;
                }
                TypeName = "快速急行";
                return;
            }
            if (Retsuban.Contains("B"))
            {
                if (Retsuban.StartsWith("臨"))
                {
                    TypeName = "臨時急行";
                    return;
                }
                TypeName = "急行";
                return;
            }
            if (Retsuban.Contains("C"))
            {
                if (Retsuban.StartsWith("臨"))
                {
                    TypeName = "臨時準急";
                    return;
                }
                TypeName = "準急";
                return;
            }
            if (Retsuban.StartsWith("臨"))
            {
                TypeName = "臨時";
                return;
            }
            if (int.TryParse(Retsuban, null, out _))
            {
                TypeName = "普通";
                return;
            }
            TypeName = "不明";
            return;

        }

        internal string TypeStringTC(string TypeName)
        {
            string TypeNameTC;
            switch (TypeName)
            {
                case "回送":
                case "A特":
                case "B特":
                case "C特1":
                case "C特2":
                case "C特3":
                case "C特4":
                case "D特":
                case "快速急行":
                case "急行":
                case "区急":
                case "準急":
                case "普通":
                    TypeNameTC = TypeName;
                    break;
                case "試運転":
                case "臨時":
                    TypeNameTC = "回送";
                    break;
                case "C特":
                    TypeNameTC = "C特4";
                    break;
                case "特急":
                    TypeNameTC = "D特";
                    break;
                default:
                    TypeNameTC = ""; // その他のケースはそのまま
                    break;
            }
            return TypeNameTC;
        }

        internal string TypeStringKana(string TypeName)
        {
            string TypeNameKana;
            switch (TypeName)
            {
                case "回送":
                    TypeNameKana = "カイソウ";
                    break;
                case "試運転":
                    TypeNameKana = "シウンテン";
                    break;
                case "臨時":
                    TypeNameKana = "リンジ";
                    break;
                case "A特":
                case "B特":
                case "C特1":
                case "C特2":
                case "C特3":
                case "C特4":
                case "D特":
                    TypeNameKana = TypeName;
                    break;
                case "C特":
                    TypeNameKana = "C特？";
                    break;
                case "臨時特急":
                    TypeNameKana = "リンジ？特";
                    break;
                case "特急":
                    TypeNameKana = "？特";
                    break;
                case "臨時快速急行":
                    TypeNameKana = "リンジカイソクキュウコウ";
                    break;
                case "快速急行":
                    TypeNameKana = "カイソクキュウコウ";
                    break;
                case "臨時急行":
                    TypeNameKana = "リンジキュウコウ";
                    break;
                case "急行":
                    TypeNameKana = "キュウコウ";
                    break;
                case "臨時区急":
                    TypeNameKana = "リンジクカンキュウコウ";
                    break;
                case "区急":
                    TypeNameKana = "クカンキュウコウ";
                    break;
                case "臨時準急":
                    TypeNameKana = "リンジジュンキュウ";
                    break;
                case "準急":
                    TypeNameKana = "ジュンキュウ";
                    break;
                case "普通":
                    TypeNameKana = "フツウ";
                    break;
                default:
                    TypeNameKana = "？"; // その他のケースはそのまま
                    break;
            }
            return TypeNameKana;
        }

        internal void TypeToStop()
        {
            // 一旦全部停車にする
            ClearAllStopDataById();
            if (TypeName == "普通")
            {
                // 普通は全て停車
                return;
            }
            //準急通過駅
            //館浜
            //駒野
            SetStopDataById("TH74", "通過");  //河原崎
            SetStopDataById("TH73", "通過");
            SetStopDataById("TH72", "通過");
            SetStopDataById("TH71", "通過");  //津崎      
            //浜園
            SetStopDataById("TH69", "通過");  //羽衣橋
            SetStopDataById("TH68", "通過");  //あらいす川     
            //新野崎
            //　各駅に停車
            //長野本町
            SetStopDataById("TH29", "通過");  //高砂町  
            SetStopDataById("TH28", "通過");
            SetStopDataById("TH27", "通過");
            SetStopDataById("TH26", "通過");
            SetStopDataById("TH25", "通過");  //北美
            //緑ヶ丘      
            SetStopDataById("TH23", "通過");  //町沢  
            SetStopDataById("TH22", "通過");
            SetStopDataById("TH21", "通過");
            SetStopDataById("TH20", "通過");  //五十川
            //出屋敷前 
            SetStopDataById("TH18", "通過");  //佐川
            SetStopDataById("TH17", "通過");  //夕陽が丘
            //五日市                  
            SetStopDataById("TH15", "通過");  //南五日市
            //高井戸八幡
            SetStopDataById("TH13", "通過");  //南八幡
            SetStopDataById("TH12", "通過");
            SetStopDataById("TH11", "通過");
            SetStopDataById("TH10", "通過");
            SetStopDataById("TH09", "通過");  //小手川
            //南吉岡
            SetStopDataById("TH07", "通過");  //宮の前
            //神宮橋
            SetStopDataById("TH05", "通過");  //宮松町
            SetStopDataById("TH04", "通過");  //青木町
            SetStopDataById("TH03", "通過");  //新町
            //三番街
            //大手橋
            if (TypeName == "準急")
            {
                // 準急停車駅までで抜ける
                return;
            }
            // 区急通過駅
            //東井  
            //　各駅に停車
            //六日市町       
            SetStopDataById("TH38", "通過");  //朝日ヶ丘
            SetStopDataById("TH37", "通過");
            SetStopDataById("TH36", "通過");
            SetStopDataById("TH35", "通過");
            SetStopDataById("TH34", "通過");  //江西
            //大和田町
            SetStopDataById("TH32", "通過");  //木之本
            //新長野公園
            //　以降準急と同じ
            if (TypeName == "区急")
            {
                // 区急停車駅までで抜ける
                return;
            }
            // 急行通過駅
            //館浜
            SetStopDataById("TH75", "通過");  //駒野     
            //河原崎
            //　通過済み
            //津崎
            SetStopDataById("TH70", "通過");  //浜園
            //羽衣橋
            //　準急と同じ
            //大路
            SetStopDataById("TH44", "通過");  //常盤通
            SetStopDataById("TH43", "通過");
            SetStopDataById("TH42", "通過");
            SetStopDataById("TH41", "通過");
            SetStopDataById("TH40", "通過");  //小沼
            //六日市町
            //　区急と同じ
            //江西
            SetStopDataById("TH33", "通過");  //大和田町
            //木之本
            //　以降区急と同じ   
            if (TypeName == "急行")
            {
                // 急行停車駅までで抜ける
                return;
            }
            // 快急通過駅
            //館浜
            //　急行と同じ
            //新野崎
            SetStopDataById("TH66", "通過");  //江ノ原
            //大道寺
            //藤江
            //水越
            SetStopDataById("TH62", "通過");  //高見沢
            //日野森
            SetStopDataById("TH60", "通過");  //奥峯口
            SetStopDataById("TH59", "通過");  //西赤山
            //赤山町
            SetStopDataById("TH57", "通過");  //三郷
            SetStopDataById("TH56", "通過");  //明神川
            SetStopDataById("TH55", "通過");  //珠川温泉
            SetStopDataById("TH54", "通過");  //上吉沢
            SetStopDataById("TH53", "通過");  //下吉沢
            //名田
            //　各駅に停車
            //東井
            SetStopDataById("TH48", "通過");  //桜坂
            //新大路
            //大路
            //　急行と同じ
            //新長野公園
            SetStopDataById("TH30", "通過");  //長野本町
            //高砂町
            //　急行と同じ
            //夕陽が丘
            SetStopDataById("TH16", "通過");  //五日市
            //南五日市
            //　急行と同じ
            //小手川
            SetStopDataById("TH09", "通過");  //南吉岡
            //宮の前
            //　急行と同じ
            //大手橋   
            if (TypeName == "快速急行")
            {
                // 快急停車駅までで抜ける
                return;
            }
            // D特以上通過駅
            //館浜
            //　快急と同じ
            //新井川
            SetStopDataById("TH67", "通過");  //新野崎
            //江ノ原　通過済み
            //大道寺
            SetStopDataById("TH65", "通過");  //藤江
            SetStopDataById("TH63", "通過");  //水越
            //高見沢　通過済み
            SetStopDataById("TH61", "通過");  //日野森
            //奥峯口
            //　快急と同じ
            //明神川
            //珠川温泉　C特2・C特3・D特停車
            //上吉沢
            //　快急と同じ
            //名田
            SetStopDataById("TH52", "通過");  //三石
            SetStopDataById("TH51", "通過");  //二木戸
            SetStopDataById("TH50", "通過");  //白石町
            //東井
            //　快急と同じ
            //小沼
            SetStopDataById("TH40", "通過");  //六日市町
            //朝日ヶ丘
            //　快急と同じ
            //北美
            SetStopDataById("TH24", "通過");  //緑ヶ丘
            //町沢
            //　快急と同じ
            //五十川
            SetStopDataById("TH20", "通過");  //出屋敷前
            //佐川
            //　快急と同じ
            //大手橋

            //ここから各パターンで割り振り    
            if (TypeName is "A特" or "C特4" or "回送")
            {
                SetStopDataById("TH65", "通過");  //大道寺
            }
            if (TypeName is "A特" or "C特2" or "C特3" or "回送")
            {
                SetStopDataById("TH58", "通過");  //赤山町
            }
            if (TypeName is "C特2" or "C特3" or "D特")
            {
                SetStopDataById("TH55", "停車");  //珠川温泉
            }
            if (TypeName is "A特" or "C特2" or "C特3" or "回送")
            {
                SetStopDataById("TH53", "通過");  //名田
            }
            if (TypeName is "A特" or "C特1" or "回送")
            {
                SetStopDataById("TH49", "通過");  //東井
            }
            if (TypeName is "A特" or "C特1" or "C特2" or "回送")
            {
                SetStopDataById("TH45", "通過");  //大路
            }
            if (TypeName is "A特" or "C特3" or "C特4" or "回送")
            {
                SetStopDataById("TH31", "通過");  //新長野公園
            }
            if (TypeName is "A特" or "C特1" or "回送")
            {
                SetStopDataById("TH14", "通過");  //高井戸八幡
            }
            if (TypeName is "A特" or "C特1" or "C特3" or "C特4" or "回送")
            {
                SetStopDataById("TH06", "通過");  //神宮橋
            }

        }

        internal void ClearAllStopDataById()
        {
            foreach (var key in StopDataById.Keys.ToList())
            {
                StopDataById[key] = "停車"; // 全ての駅の停車データを「停車」にリセット
            }
        }

        public override string ToString()
        {
            return TypeName + " " + string.Join(" ", StopDataById.Select(kv => $"{kv.Key}:{kv.Value}"));
        }
    }
}
