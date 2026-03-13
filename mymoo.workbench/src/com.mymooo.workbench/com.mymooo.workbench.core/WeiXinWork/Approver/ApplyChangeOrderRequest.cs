using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;

namespace com.mymooo.workbench.core.WeiXinWork.Approver
{
    [ApprovalTemplate("C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4")]
    public class ApplyChangeOrderRequest : ApprovalRequest
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public string SaleName { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 逾期天数
        /// </summary>
        public long ExpiryDay { get; set; }
        /// <summary>
        /// 逾期金额
        /// </summary>
        public decimal OverdueAmount { get; set; }
        /// <summary>
        /// 信用额度
        /// </summary>
        public decimal CreditLine { get; set; }

        /// <summary>
        /// 可用额度
        /// </summary>
        public decimal AvailableCredit { get; set; }

        /// <summary>
        /// 占用额度
        /// </summary>
        public decimal OccupyCredit { get; set; }

        /// <summary>
        /// 原订单总金额
        /// </summary>
        public decimal OrderTotalAmount { get; set; }
        /// <summary>
        /// 变更后订单总金额
        /// </summary>
        public decimal ChangeOrderTotalAmount { get; set; }
        /// <summary>
        /// 变更原因
        /// </summary>
        public string ChangeReason { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string PayMethodName { get; set; }

        /// <summary>
        /// 申请日期
        /// </summary>
        public DateTime ApplyDate { get; set; }

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

        /// <summary>
        /// 单据类型
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SaleOrder { get; set; }

        /// <summary>
        /// 订单业务员
        /// </summary>
        public List<string> SalesMans { get; set; }

    }
}
