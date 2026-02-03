using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;

namespace mymooo.k3cloud.core.Approver
{
    /// <summary>
    /// 加急发货申请
    /// </summary>
    [ApprovalTemplate("3WMWXJjitfB1oNCBxUjxpz5TEWz5XAyPCb3H214Y")]
	public class K3cloudDnUrgentShipmentRequest : ApprovalRequest
	{
		/// <summary>
		/// DN单号
		/// </summary>
		public string DnNo { get; set; } = string.Empty;

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustName { get; set; } = string.Empty;

		/// <summary>
		/// 物流发货仓库
		/// </summary>
		public string Warehouse { get; set; } = string.Empty;

		/// <summary>
		/// 订单金额
		/// </summary>
		public decimal OrderAmount { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string Remarks { get; set; } = string.Empty;

		/// <summary>
		/// 申请日期
		/// </summary>
		public DateTime ApplyDate { get; set; } = DateTime.Now;

		/// <summary>
		/// 出库项数
		/// </summary>
		public int OutCount { get; set; } = 0;

        /// <summary>
        ///加急出库日期
        /// </summary>
        public DateTime UrgentDeliveryDate { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

		/// <summary>
		/// 审批单号
		/// </summary>
		public string ApprovalNo { get; set; } = string.Empty;

		/// <summary>
		/// 审核人
		/// </summary>
		public string AduitUserName { get; set; } = string.Empty;

	}
}
