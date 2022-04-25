using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Xml
{
    class Program
    {
        /*
         * 先將 XML 轉成物件方案，問題：無法使用動態條件搜尋資料，雖然 where 條件式可動態組合，但是 被搜尋的集合的資料型別 無法動態產生。不使用 DataTable 存放資料。
         * 
         * 所以採用直接搜尋 XML 方案，因為 XML 的資料型別固定是字串。
         * 
         * 目前
         * //SBBINVOI/SBBEXPENS/SBBEXPEN[@exno=420]/@expr，比對欄位只能一個
         * 數值不能有千分位符號
         */

        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            List<CompareColumnDo> compareColumnDos = new()
            {
                new() { SceneId = 220400001, SeqNo = 0, Data = "//SBBINVOI/@symb" },
                new() { SceneId = 220400001, SeqNo = 1, Data = "//SBBINVOI/@ivtr" },
                new() { SceneId = 220400001, SeqNo = 2, Data = "//SBBINVOI/@ivls" },
                new() { SceneId = 220400001, SeqNo = 3, Data = "//SBBINVOI/@ivif" },
                new() { SceneId = 220400001, SeqNo = 4, Data = "//SBBINVOI/@comm" },
                new() { SceneId = 220400001, SeqNo = 5, Data = "//SBBINVOI/@ivsh" },
                new() { SceneId = 220400001, SeqNo = 6, Data = "//SBBINVOI/@ivpr" },
                new() { SceneId = 220400001, SeqNo = 7, Data = "//SBBINVOI/SBBEXPENS/SBBEXPEN[@exno=420]/@expr" },
                new()
                {
                    SceneId = 220400001,
                    SeqNo = 8,
                    Data =
@"?//SBBINVOI[@ivif=20],//SBBINVOI/SBBOFINVS/SBBOFINV/@ofse & //SBBINVOI/SBBOFINVS/SBBOFINV/@ofcm & //SBBINVOI/SBBOFINVS/SBBOFINV/SBBOFEXPS/SBBOFEXP[@oeno=411]/@oeam
?,//SBBINVOI/SBBOFINVS/SBBOFINV/@ofse & //SBBINVOI/SBBOFINVS/SBBOFINV/@ofcm & //SBBINVOI/SBBOFINVS/SBBOFINV/SBBOFEXPS/SBBOFEXP[@oeno=410]/@oeam"
                },
            };

            List<CompareDataDo> compareDataDos = new()
            {
                //new() { SceneId = 220400001, SeqNo = 0, Data = "2338,0,1,20,A2245,3000,100.5,301620" },
                //new() { SceneId = 220400001, SeqNo = 1, Data = "2338,0,1,20,A2543,2000,100,200079" },
                //new() { SceneId = 220400001, SeqNo = 2, Data = "2338,0,1,20,A2573,2000,99.9,199879" },
                new() { SceneId = 220400001, SeqNo = 3, Data = "2338,0,2,20,A2830,7000,101,-705658,1&A2245&804|2&A2573&1738|3&A2543&1538" },
                //new() { SceneId = 220400001, SeqNo = 4, Data = "2338,0,1,20,A6734,2000,101,202080" },
                //new() { SceneId = 220400001, SeqNo = 5, Data = "2338,0,2,20,A6752,2000,100,-199621" }
            };

            string xml = System.IO.File.ReadAllText("XMLFile1.xml");

            CompareTestResult(xml, compareColumnDos, compareDataDos);

            int sceneId = 0;
            StringBuilder sb = new();
            foreach (CompareDataDo compareDataDo in compareDataDos)
            {
                sceneId = compareDataDo.SceneId;
                sb.AppendFormat("\r\n{0}", compareDataDo.Result);
            }
            string message = sb.Remove(0, 2).ToString();

            // 測試腳本的所有資料都驗證通過
            if (compareDataDos.Any(x => !x.IsPass))
            {
                _log.Info($"測試腳本有資料驗證失敗, 腳本:{sceneId}");
            }
            else
            {
                _log.Info($"測試腳本的資料驗證通過, 腳本:{sceneId}");
            }

            Console.Read();
        }

        /// <summary>
        /// 比對測試結果
        /// </summary>
        /// <param name="xml">大州的結果(XML)</param>
        /// <param name="compareColumnDos">比對欄位集合</param>
        /// <param name="compareDataDos">比對資料集合</param>
        static void CompareTestResult(string xml, IEnumerable<CompareColumnDo> compareColumnDos, IEnumerable<CompareDataDo> compareDataDos)
        {
            const string _default = "0";

            // 將比對欄位轉成字典，方便查詢。
            Dictionary<int, CompareColumnDo> compareColumnDict = compareColumnDos.ToDictionary(x => x.SeqNo);

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);

            string message;

            // 組合搜尋條件
            foreach (CompareDataDo compareDataDo in compareDataDos)
            {
                string[] values = compareDataDo.Data.Split(',');

                Dictionary<string, StringBuilder> masterDict = new();
                Dictionary<string, StringBuilder> detailDict = new();
                Dictionary<string, List<string>> specialDict = new();

                // 將每一個值和對應欄位組合成 where 條件
                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i];

                    if (!compareColumnDict.TryGetValue(i, out CompareColumnDo foundCompareColumn))
                    {
                        message = $"比對失敗-找不到比對欄位, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 比對值:{value}, 對應欄位順序:{i + 1}";
                        _log.Error(message);

                        compareDataDo.IsPass = false;
                        compareDataDo.AppendResult(message);

                        continue;
                    }

                    //組合 where
                    if (foundCompareColumn.Data.Trim().StartsWith("?"))
                    {
                        //格式: ?Condiction,Action?Condiction,Action?...
                        //範例: ?//SBBINVO[@ivif=20],//SBBINVO/SBBOFINVS/SBBOFINV/@ofse & //SBBINVO/SBBOFINVS/SBBOFINV/@ofcm & //SBBINVO/SBBOFINVS/SBBOFINV/SBBOFEXPS/SBBOFEXP[@exno=411]/@oeam ?,//SBBINVO/SBBOFINVS/SBBOFINV/@ofse & //SBBINVO/SBBOFINVS/SBBOFINV/@ofcm & //SBBINVO/SBBOFINVS/SBBOFINV/SBBOFEXPS/SBBOFEXP[@exno=410]/@oeam
                        //移除第一個 "?"
                        string[] commands = foundCompareColumn.Data.Remove(0, 1).Split('?');
                        for (int j = 0; j < commands.Length; j++)
                        {
                            string[] conds = commands[j].Split(',');
                            if (conds.Length != 2)
                            {
                                message = $"比對失敗-格式不對, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 對應欄位:{foundCompareColumn.Data}";
                                _log.Error(message);

                                compareDataDo.IsPass = false;
                                compareDataDo.AppendResult(message);

                                continue;
                            }

                            string condField = conds[0].Trim();
                            string actionField = conds[1].Trim();

                            if (string.IsNullOrWhiteSpace(condField))
                            {
                                condField = _default;
                            }
                            else
                            {
                                // 移除 "//"
                                condField = condField.Remove(0, 2);
                            }

                            List<string> whereList = new();
                            specialDict[condField] = whereList;

                            // "//SBBINVO/SBBOFINVS/SBBOFINV/@ofse & //SBBINVO/SBBOFINVS/SBBOFINV/@ofcm & //SBBINVO/SBBOFINVS/SBBOFINV/SBBOFEXPS/SBBOFEXP[@exno=411]/@oeam"
                            string[] actions = actionField.Split('&');

                            // 1 & A2245 & 804 | 2 & A2573 & 1738 | 3 & A2543 & 1538
                            string[] specialValueRecordList = value.Split('|');
                            foreach (string specialValueRecord in specialValueRecordList)
                            {
                                Dictionary<string, StringBuilder> whereDict = new();

                                string[] specialValueList = specialValueRecord.Split('&');
                                for (int k = 0; k < specialValueList.Length; k++)
                                {
                                    //解析 Action
                                    (string tableName, StringBuilder sbWhere, _) = ParseCompareColumn(actions[k].Trim(), specialValueList[k].Trim());

                                    if (!whereDict.TryGetValue(tableName, out StringBuilder sb))
                                    {
                                        sb = new();
                                        whereDict[tableName] = sb;
                                    }
                                    sb.Append(sbWhere);
                                }

                                foreach (var kvp in whereDict)
                                {
                                    string xmlPath = $"/{kvp.Key}[{kvp.Value.Remove(0, 5)}]";
                                    whereList.Add(xmlPath);
                                }
                            }
                        }
                    }
                    else
                    {
                        (string tableName, StringBuilder sbWhere, bool isMaster) = ParseCompareColumn(foundCompareColumn.Data, value);
                        if (isMaster)
                        {
                            if (!masterDict.TryGetValue(tableName, out StringBuilder sb))
                            {
                                sb = new();
                                masterDict[tableName] = sb;
                            }
                            sb.Append(sbWhere);
                        }
                        else
                        {
                            if (!detailDict.TryGetValue(tableName, out StringBuilder sb))
                            {
                                sb = new();
                                detailDict[tableName] = sb;
                            }
                            sb.Append(sbWhere);
                        }
                    }
                }

                if (masterDict.Count > 0)
                {
                    //Master
                    var firstKvp = masterDict.First();
                    string where = $"/TARoot/{firstKvp.Key}S/{firstKvp.Key}[{firstKvp.Value.Remove(0, 5)}]";
                    XmlNode xmlNode = xmlDoc.SelectSingleNode(where);
                    if (xmlNode == null)
                    {
                        message = $"比對失敗-沒有符合的資料, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 條件:{where}";
                        _log.Error(message);

                        compareDataDo.IsPass = false;
                        compareDataDo.AppendResult(message);

                        continue;
                    }

                    message = $"比對通過, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 條件:{where}";
                    _log.Info(message);

                    compareDataDo.AppendResult(message);

                    // 取出這一筆的 XML，方便後續處理。
                    _log.Trace(xmlNode.OuterXml);
                    XmlDocument xmlDetail = new();
                    xmlDetail.LoadXml(xmlNode.OuterXml);

                    //Detail
                    bool isOK = true;
                    foreach (var kvp in detailDict)
                    {
                        string where2 = $"/{kvp.Key}[{kvp.Value.Remove(0, 5)}]";
                        var xmlNode2 = xmlDetail.SelectSingleNode(where2);
                        if (xmlNode2 == null)
                        {
                            isOK = false;
                            message = $"比對失敗-沒有符合的資料, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 條件:{where2}";
                            _log.Error(message);

                            compareDataDo.IsPass = false;
                            compareDataDo.AppendResult(message);

                            break;
                        }

                        message = $"比對通過, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 條件:{where2}, 大州:{xmlNode2.OuterXml}";
                        _log.Info(message);

                        compareDataDo.AppendResult(message);

                        //logger.Trace("===== Detail =====");
                        //for (int k = 0; k < xmlNode.Count; k++)
                        //{
                        //    logger.Trace(xmlNode[k].OuterXml);
                        //}
                    }

                    //特殊條件
                    if (isOK && specialDict.Count > 0)
                    {
                        //依照條件決定要使用哪一組比對條件
                        List<string> specialWhereList = null;
                        foreach (var kvp in specialDict)
                        {
                            if (kvp.Key == _default)
                            {
                                specialWhereList = kvp.Value;
                                break;
                            }
                            else
                            {
                                XmlNode xmlNode1 = xmlDetail.SelectSingleNode($"/{kvp.Key}");
                                if (xmlNode1 != null)
                                {
                                    specialWhereList = kvp.Value;
                                    break;
                                }
                            }
                        }

                        if (specialWhereList != null)
                        {
                            foreach (string specialWhere in specialWhereList)
                            {
                                XmlNode xmlNode1 = xmlDetail.SelectSingleNode(specialWhere);
                                if (xmlNode1 == null)
                                {
                                    isOK = false;
                                    message = $"比對失敗-沒有符合的資料, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 條件:{specialWhere}";
                                    _log.Error(message);

                                    compareDataDo.IsPass = false;
                                    compareDataDo.AppendResult(message);

                                    break;
                                }

                                message = $"比對通過, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, 條件:{specialWhere}, 大州:{xmlNode1.OuterXml}";
                                _log.Info(message);

                                compareDataDo.AppendResult(message);
                            }
                        }
                    }

                    if (isOK)
                    {
                        message = $"比對通過-此筆資料都符合, 腳本:{compareDataDo.SceneId}, 資料序:{compareDataDo.SeqNo}, {compareDataDo.Data}";
                        _log.Info(message);

                        compareDataDo.AppendResult(message);
                    }
                }
            }
        }

        /// <summary>
        /// 解析比對欄位
        /// </summary>
        /// <param name="compareColumnData">比對欄位</param>
        /// <param name="value">比對值</param>
        /// <returns></returns>
        private static (string tableName, StringBuilder sbWhere, bool isMaster) ParseCompareColumn(string compareColumnData, string value)
        {
            StringBuilder sbTableName = new();
            StringBuilder sbWhere = new();
            bool isMaster = true;

            // Master: "//SBBINVOI/@symb"
            // Detail: "//SBBINVOI/SBBEXPENS/SBBEXPEN[@exno=420]/@expr"
            // 移除 "//"
            string[] cols = compareColumnData.Remove(0, 2).Split('/');
            foreach (string colDef in cols)
            {
                int idx = colDef.IndexOf('[');
                if (idx > -1)
                {
                    int idxEnd = colDef.IndexOf(']');

                    sbTableName.AppendFormat("/{0}", colDef.Substring(0, idx));
                    sbWhere.AppendFormat(" and {0}", colDef.Substring(idx + 1, idxEnd - idx - 1));

                    isMaster = false;
                }
                else if (colDef.Contains('@'))
                {
                    sbWhere.AppendFormat(" and {0}='{1}'", colDef, value);
                }
                else
                {
                    sbTableName.AppendFormat("/{0}", colDef);
                }
            }

            return (sbTableName.Remove(0, 1).ToString(), sbWhere, isMaster);
        }
    }

    /// <summary>
    /// 腳本比對欄位類別
    /// </summary>
    class CompareColumnDo
    {
        /// <summary>
        /// 腳本代碼
        /// </summary>
        public int SceneId { get; set; }

        /// <summary>
        /// 流水號
        /// </summary>
        public int SeqNo { get; set; }

        /// <summary>
        /// 欄位資料
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 覆寫
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"SceneId:{SceneId}, SeqNo:{SeqNo}, Data:{Data}";
        }
    }

    /// <summary>
    /// 腳本比對資料類別
    /// </summary>
    class CompareDataDo
    {
        /// <summary>
        /// 腳本代碼
        /// </summary>
        public int SceneId { get; set; }

        /// <summary>
        /// 流水號
        /// </summary>
        public int SeqNo { get; set; }

        /// <summary>
        /// 比對資料
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 覆寫
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"SceneId:{SceneId}, SeqNo:{SeqNo}, Data:{Data}";
        }

        /// <summary>
        /// 是否通過
        /// </summary>
        public bool IsPass { get; set; } = true;

        /// <summary>
        /// 結果
        /// </summary>
        public string Result
        {
            get => _sb.ToString();
            set
            {
                _sb.Clear();
                _sb.Append(value);
            }
        }

        private readonly StringBuilder _sb = new();
        /// <summary>
        /// 附加結果
        /// </summary>
        /// <param name="result"></param>
        public void AppendResult(string result)
        {
            _sb.AppendFormat("{0}{1}", _sb.Length == 0 ? string.Empty : "\r\n", result);
        }
    }
}
