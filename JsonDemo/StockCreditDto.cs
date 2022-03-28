namespace JsonDemo
{
    /// <summary>
    /// 個股額度資料物件
    /// </summary>
    public class StockCreditDto
    {
        /// <summary>
        /// 分公司
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 股票代號
        /// </summary>
        public string StockId { get; set; }

        /// <summary>
        /// 額度上限
        /// </summary>
        public int CreditLimit { get; set; }

        /// <summary>
        /// 不限用途使用額度
        /// </summary>
        public int NonrestrictedUsage { get; set; }

        /// <summary>
        /// 融資使用額度
        /// </summary>
        public int StockLoanUsage { get; set; }

        /// <summary>
        /// 可用額度
        /// </summary>
        public int RestCredit { get; set; }
    }
}