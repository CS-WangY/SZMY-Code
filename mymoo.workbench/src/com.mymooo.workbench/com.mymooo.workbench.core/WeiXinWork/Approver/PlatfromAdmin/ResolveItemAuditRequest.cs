using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using System.Collections.Generic;

namespace com.mymooo.workbench.core.WeiXinWork.Approver
{
	[ApprovalTemplate("C4UE66oVYU5xi3TTiFV2iD7Vb1d5ozqhEqpVeqisD")]
    public class ResolveItemAuditRequest : ApprovalRequest
    {
        /// <summary>
        /// 分解单号
        /// </summary>
        public string ResolveNumber { get; set; }
        /// <summary>
        /// 报价单号
        /// </summary>
        public string InquiryNumber { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 客户等级
        /// </summary>
        public string CustLevel { get; set; }
        /// <summary>
        /// 分解单总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 产品工程师
        /// </summary>
        public string ProductEngineerName { get; set; }

        /// <summary>
        /// 产品经理
        /// </summary>
        public string ProductManagerName { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public string SalesName { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public string InputName { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditBy { get; set; }

        /// <summary>
        /// 驳回原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> Files { get; set; }
    }
}
