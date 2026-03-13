using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using System;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Credit
{
	/// <summary>
	/// 发货申请
	/// </summary>
	[ApprovalTemplate("ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd")]
    public class DeliveryRequest : ApprovalRequest
    {
        /// <summary>
        /// 发货日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string SalesOrderNo { get; set; }

        /// <summary>
        /// 发货单号
        /// </summary>
        public string ConsignNum { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustNumber { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal SalesOrderAmount { get; set; }

        /// <summary>
        /// 发货金额
        /// </summary>
        public decimal ConsignNumAmount { get; set; }

        /// <summary>
        /// 逾期金额
        /// </summary>
        public decimal ExpiryAmount { get; set; }

        /// <summary>
        /// 逾期天数
        /// </summary>
        public int ExpiryDay { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
    }
}
