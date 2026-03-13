using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;
using System;
using System.Collections.Generic;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Scm
{
	[ApprovalTemplate("C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N")]
    public class ReserveApplyRequest : ApprovalRequest
    {

        /// <summary>
        /// 备库申请单号
        /// </summary>
        public string ReserveApplyNumber { get; set; }
        /// <summary>
        /// 备库类型
        /// </summary>
        public string ReserveType { get; set; }

        /// <summary>
        /// 备库原因
        /// </summary>
        public string ReserveReason { get; set; }

        /// <summary>
        /// 备库金额
        /// </summary>
        public decimal ReserveAmount { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 账期
        /// </summary>
        public string AccountPeriod { get; set; }


        /// <summary>
        /// 收益风险比（可能收益/可能损失）
        /// </summary>
        public string ReturnToRiskRatio { get; set; }
        //public decimal ReturnToRiskRatio { get; set; }

        /// <summary>
        /// 风险说明
        /// </summary>
        public string RiskDescription { get; set; }



        /// <summary>
        /// 附件
        /// </summary>
        public List<MediaInfo> MediaInfos { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public DateTime ApplyeventDate { get; set; }

        public string ApprovalName { get; set; }

        public string AuditRemark { get; set; }

        ///// <summary>
        ///// 备库数量
        ///// </summary>
        //public int StockNumber { get; set; }
        ///// <summary>
        ///// 备库型号数量
        ///// </summary>
        //public int StockModelNumber { get; set; }
        ///// <summary>
        ///// 品类名称
        ///// </summary>
        //public string CategoryName { get; set; }

        ///// <summary>
        ///// 备库产品系列
        ///// </summary>
        //public string ProductSeries { get; set; }


        ///// <summary>
        ///// 现有客户数
        ///// </summary>
        //public long NowCustNumbers { get; set; }

        ///// <summary>
        ///// 月出库数量（近三月平均）
        ///// </summary>
        //public long MonthlyDeliveryQuantity { get; set; }

        ///// <summary>
        ///// 月出库金额（近三月平均）
        ///// </summary>
        //public decimal MonthlyDeliveryAmount { get; set; }
        ///// <summary>
        ///// 备注
        ///// </summary>
        //public string Remark { get; set; }
    }
}
