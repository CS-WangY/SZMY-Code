using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using System;
using System.Collections.Generic;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Crm
{
	[ApprovalTemplate("3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg")]
    public class StockApplyRequest : ApprovalRequest
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 客户等级
        /// </summary>
        public string CompanyGrade { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// 备货金额
        /// </summary>
        public decimal StockAmount { get; set; }

        /// <summary>
        /// 申请理由
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> MediaInfos { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public DateTime ApplyeventDate { get; set; }

        /// <summary>
        /// 备货申请单号
        /// </summary>
        public string StockApplyNumber { get; set; }

        public string ApprovalName { get; set; }
    }
}
