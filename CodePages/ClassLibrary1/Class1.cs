namespace ClassLibrary1
{
    /// <summary>
    /// 
    /// </summary>
    public class Class1
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        public byte[] GetBytes(string data, int codePage)
        {
#if NET5_0_OR_GREATER
            //dotnet add package System.Text.Encoding.CodePages
            return System.Text.CodePagesEncodingProvider.Instance.GetEncoding(codePage).GetBytes(data);
#else
            return System.Text.Encoding.GetEncoding(codePage).GetBytes(data);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        public string GetString(byte[] rawData, int codePage)
        {
#if NET5_0_OR_GREATER
            //dotnet add package System.Text.Encoding.CodePages
            return System.Text.CodePagesEncodingProvider.Instance.GetEncoding(codePage).GetString(rawData);
#else
            return System.Text.Encoding.GetEncoding(codePage).GetString(rawData);
#endif
        }
    }
}
