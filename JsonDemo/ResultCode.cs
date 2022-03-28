using System.Runtime.Serialization;

namespace JsonDemo
{
    /// <summary>
    /// 結果代碼列舉
    /// </summary>
    [DataContract]
    public enum ResultCode
    {
        /// <summary>
        /// 錯誤
        /// </summary>
        [EnumMember]
        Error = -1,

        /// <summary>
        /// 未定義
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// 正常
        /// </summary>
        [EnumMember]
        OK = 1,
    }
}
