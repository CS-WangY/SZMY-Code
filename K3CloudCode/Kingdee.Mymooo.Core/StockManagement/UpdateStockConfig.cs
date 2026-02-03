namespace Kingdee.Mymooo.Core.StockManagement
{
	public class UpdateStockConfig
	{
		/// <summary>
		/// 单位字段
		/// </summary>
		public string UnitField { get; set; }

		/// <summary>
		/// 供应商字段
		/// </summary>
		public string SupplierField { get; set; }

		/// <summary>
		/// 客户字段
		/// </summary>
        public string CustomerField { get; set; }

        /// <summary>
        /// 订单字段
        /// </summary>
        public string OrderNoField { get; set; }

        /// <summary>
        /// 订单Id字段
        /// </summary>
        public string OrderIdField { get; set; }

        /// <summary>
        /// 订单明细字段
        /// </summary>
        public string OrderEntryField { get; set; }

        /// <summary>
        /// 订单明细序号
        /// </summary>
        public string OrderEntrySeqField { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
        public string QtyField { get; set; }

		/// <summary>
		/// 产品小类字段
		/// </summary>
		public string SmallField { get; set; }

		/// <summary>
		/// 产品大类字段
		/// </summary>
		public string ParentSmallField { get; set; }
    }
}
