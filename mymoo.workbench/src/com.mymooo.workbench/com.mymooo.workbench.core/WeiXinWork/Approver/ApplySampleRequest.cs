using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;

namespace com.mymooo.workbench.core.WeiXinWork.Approver
{
    [ApprovalTemplate("3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou")]
    public class ApplySampleRequest : ApprovalRequest
    {

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string SalesOrder { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        public string PayMethodName { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// 申请类型
        /// </summary>
        public string ApplyType { get; set; }

        /// <summary>
        /// 申请理由
        /// </summary>
        public string ApplyReason { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> Files { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string AduitUserName { get; set; }
    }
}
