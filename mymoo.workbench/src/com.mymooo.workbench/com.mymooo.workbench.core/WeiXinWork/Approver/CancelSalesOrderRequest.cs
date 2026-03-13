using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;

namespace com.mymooo.workbench.core.WeiXinWork.Approver
{
    /// <summary>
    /// 
    /// </summary>
    [ApprovalTemplate("C4ZW87D68U6Pye3KtNszkNYV8JW6TybcKRaYuDdwk")]
    public class CancelSalesOrderRequest : ApprovalRequest
    {
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { get; set; } = string.Empty;


        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// 业务员姓名
        /// </summary>
        public string SalesName { get; set; } = string.Empty;

        /// <summary>
        /// 取消订单的理由
        /// </summary>
        public string CancelReason { get; set; } = string.Empty;


        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> Files { get; set; }

    }
}
