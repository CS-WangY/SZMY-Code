using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using System;
using System.Collections.Generic;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Crm
{
	[ApprovalTemplate("3WLKButmnRboLtatDnsbQLPGtpijCdRSYPL1eun8")]
    public class CustomerVisitApplyDto : ApprovalRequest
    {
        public string CompanyName { get; set; }
        public string ContactsName { get; set; }
        public string ContactsMobile { get; set; }
        public string ContactsPosition { get; set; }
        public string VisitType { get; set; }
        public string ClockInAddress { get; set; }

        /// <summary>
        /// 拜访事项
        /// </summary>
        public string VisitMatter { get; set; }
        /// <summary>
        /// 沟通结论
        /// </summary>
        public string VisitResult { get; set; }
        /// <summary>
        /// 后续跟进事项
        /// </summary>
        public string FollowUpMatters { get; set; }
        /// <summary>
        /// 客户反馈
        /// </summary>
        public string CustomerFeedback { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> MediaInfos { get; set; }
    }

    public class CustomerVisitCallBackDto
    {
        /// <summary>
        /// 审批时间
        /// </summary>
        public DateTime ApplyeventDate { get; set; }

        /// <summary>
        /// 审批单号
        /// </summary>
        public string ApplyeventNo { get; set; }

        /// <summary>
        /// 审批状态
        /// </summary>
        public string SpStatus { get; set; }
        /// <summary>
        /// 审批备注
        /// </summary>
        public string ApprovalRemark { get; set; }

        public string ApprovalName { get; set; }
    }
}
