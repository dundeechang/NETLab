using System.Runtime.Serialization;

namespace JsonDemo
{
    /// <summary>
    /// 結果類別
    /// </summary>
    [DataContract]
    public class Result
    {
        /// <summary>
        /// 結果代碼
        /// </summary>
        [DataMember]
        public ResultCode Code { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// 資料
        /// </summary>
        [DataMember]
        public object Data { get; set; }
    }
}
