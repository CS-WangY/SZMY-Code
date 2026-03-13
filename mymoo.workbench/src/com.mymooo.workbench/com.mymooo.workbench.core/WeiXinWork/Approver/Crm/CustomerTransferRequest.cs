using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Crm
{
    [ApprovalTemplate("3TkZjkT19QVEvMxzwD565BjE7oeXfDEKpkgaAXq2")]
    public class CustomerTransferRequest : ApprovalRequest
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 现归属人
        /// </summary>
        public string CurrentBelonger { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// 转移原因
        /// </summary>
        public string TransferCause { get; set; }

        /// <summary>
        /// 转移时间
        /// </summary>
        public DateTime TransferTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 审批单号
        /// </summary>
        public string ApprovalNo { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string AduitUserName { get; set; }
    }

    /// <summary>
    /// 转移客户
    /// </summary>
    public class TransferCompanySalesMan
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string SalesName { get; set; }
        //[DefaultValue("weixinwork")]
        public string MymoooCompany { get; set; } = "weixinwork";
        public string SalesCode { get; set; }
        public long SalesId { get; set; }
        public string UserCode { get; set; }
        public string CompanyCodeList { get; set; }

        /// <summary>
        /// excel批量导入状态
        /// </summary>
        public string Status { get; set; }

        public string FailureReason { get; set; }

        /// <summary>
        /// 审批单号
        /// </summary>
        public string ApprovalNo { get; set; }
        /// <summary>
        /// 审批状态
        /// </summary>
        public string SpStatus { get; set; }
    }
}
