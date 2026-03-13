using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using System;
using System.Collections.Generic;

namespace com.mymooo.workbench.core.WeiXinWork.Approver
{
	/// <summary>
	/// 产品直发
	/// </summary>
	[ApprovalTemplate("Bs2fyWvkPyTau8ouxMqkUyDSrimoP48oK3YMUtPUT")]
    public class ApplyDirectdeliveryRequest:ApprovalRequest
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditTime { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditBy { get; set; }
        /// <summary>
        /// 客户类型
        /// </summary>
        public string CustType { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string PayName { get; set; }

        /// <summary>
        /// 信用额度
        /// </summary>
        public decimal CreditLine { get; set; }

        /// <summary>
        /// 可用额度
        /// </summary>
        public decimal AvailableCredit { get; set; }
        /// <summary>
        /// 逾期金额
        /// </summary>
        public decimal ExpiryAmount { get; set; }
        /// <summary>
        /// 逾期天数
        /// </summary>
        public int ExpiryDay { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }
        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 收货信息
        /// </summary>
        public string ReceivingMsg { get; set; }
        /// <summary>
        /// 现金支付状态
        /// </summary>
        public string IsPay { get; set; }
        /// <summary>
        /// 是否担保
        /// </summary>
        public string IsWarrant { get; set; }
        /// <summary>
        /// 申请直发原因
        /// </summary>
        public string Reason { get; set; }
        /// <summary>
        /// 直发产品所属事业部
        /// </summary>
        public string Division { get;set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> File { get; set; }

        /// <summary>
        /// 物流单号
        /// </summary>
        public string LogisticsNo { get; set; }

    }
}
