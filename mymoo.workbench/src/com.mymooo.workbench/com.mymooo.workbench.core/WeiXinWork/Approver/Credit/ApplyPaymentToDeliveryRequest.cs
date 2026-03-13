using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using System;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Credit
{
	/// <summary>
	/// 款到发货申请
	/// </summary>
	[ApprovalTemplate("C4NvrtgioqzwRdFFrggQpLmn1cGkDJ92dj2GYYJiU")]
    public class ApplyPaymentToDeliveryRequest : ApprovalRequest
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string SalesOrderNo { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal SalesOrderAmount { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime SalesOrderDate { get; set; }

        /// <summary>
        /// 申请理由
        /// </summary>
        public string ApprovalReason { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public DateTime ApplyeventDate { get; set; }
    }
}
