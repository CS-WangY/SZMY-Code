using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mymooo.mall.core.Model.InquiryOrder;
using com.mymooo.mall.core.Model.ResolveOrder;

namespace com.mymooo.mall.core.Model.QuotationOrder
{
    /// <summary>
    /// 报价项。
    /// </summary>
    [Serializable]
    public class QuotedItem 
    {
        public long Id { get; set; }

        public QuotedOrderBAL QuotedOrder { get; set; } = new QuotedOrderBAL();

        /// <summary>
        /// 询价项Id。
        /// </summary>
        public InquiryItem InquiryItem { get; set; } = new InquiryItem();

        /// <summary>
        /// 带税单价。
        /// </summary>
        public decimal UnitPriceWithTax { get; set; }

        /// <summary>
        /// 未税单价。
        /// </summary>
        public decimal UnitPriceWithoutTax { get; set; }

        /// <summary>
        /// 原销售价。
        /// </summary>
        public decimal OriginalUnitPriceWithTax { get; set; }

        /// <summary>
        /// 产品工程师
        /// </summary>
        public long ProductEngineerId { get; set; }

        public string ProductEngineerName { get; set; } = string.Empty;

        public string ProductManagerName { get; set; } = string.Empty;

        public long ProductManagerId { get; set; }
        //分解单报价,提交人, 就是分解单
        public string ResolvedSubmitUserName { get; set; } = string.Empty;

        /// <summary>
        /// 量。
        /// </summary>
        public decimal Quantity { get; set; }

        public decimal SubtotalWithTax { get; set; }

        public decimal SubtotalWithoutTax { get; set; }

        /// <summary>
        /// 供应链的发货天数。
        /// </summary>
        public int? SupplierDispatchDays { get; set; }

        /// <summary>
        /// 发货天数。
        /// </summary>SaveData
        public int DispatchDays { get; set; }

        /// <summary>
        /// 需求发货天数
        /// </summary>
        public decimal? RequirementDays { get; set; }

        /// <summary>
        /// 产品小类
        /// </summary>
        public virtual ProductSmallClass? ProductSmallClass { get; set; }

        /// <summary>
        /// 报价模板导入的小类Id
        /// </summary>
        public long? SmallClassId { get; set; }

        /// <summary>
        /// 报价模板导入的大类Id
        /// </summary>
        public long? LargeClassId { get; set; }

        /// <summary>
        /// 价格来源
        /// </summary>
        public PriceSource? PriceSource { get; set; }

        /// <summary>
        /// 货期来源
        /// </summary>
        public DeliverySource? DeliverySource { get; set; }
        /// <summary>
        /// 货期提交人
        /// </summary>
        public string DeliverySubmitBy { get; set; } = string.Empty;

        /// <summary>
        /// 历史最低报价(最近一年)
        /// </summary>
        public decimal? QuoLPrice { get; set; }
        /// <summary>
        /// 历史最低成交价(最近一年)
        /// </summary>
        public decimal? DealLPrice { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 客户项目号
        /// </summary>
        public string ProjectNo { get; set; } = string.Empty;

        /// <summary>
        /// 客户物料号
        /// </summary>
        public string CustItemNo { get; set; } = string.Empty;

        /// <summary>
        /// 库存管理特征
        /// </summary>
        public string StockFeatures { get; set; } = string.Empty;

        /// <summary>
        /// 是否为礼品。
        /// </summary>
        public bool IsGift { get; set; }

        /// <summary>
        /// 修改时间。
        /// </summary>
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// 分解状态。
        /// 只在未分解情况下可分解。
        /// </summary>
        public ResolveOrderStatus ResolveStatus { get; set; }

        /// <summary>
        /// 驳回类型的字段 0.未驳回 1.驳回到产品  2.驳回到业务 3 驳回（特价） 在驳回时 根据前端的用户操作，写入， 主要是在保存/提交时进行判断是否允许操作
        /// </summary>
        public int RejectType { get; set; }

        /// <summary>
        /// 向供应连询价时的附件。  --  是报价单项目的附件
        /// </summary>
        public string AttachmentUrl { get; set; } = string.Empty;

        /// <summary>
        /// 做过多少次版本变更。
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 供应链给的结算价。
        /// </summary>
        public decimal FirstCost { get; set; }

        /// <summary>
        /// 审核状态。
        /// </summary>
        public QuoteOrderAuditStatus AuditStatus { get; set; }

        /// <summary>
        /// 审核人。
        /// </summary>
        public long AuditedBy { get; set; }

        /// <summary>
        /// 审核时间。
        /// </summary>
        public DateTime AuditedOn { get; set; }

        /// <summary>
        /// 提交人。
        /// </summary>
        public long SubmittedBy { get; set; }

        /// <summary>
        /// 提交时间。
        /// </summary>
        public DateTime SubmittedOn { get; set; }

        /// <summary>
        /// 是否需要供应链报价。
        /// </summary>
        public bool AskPrice { get; set; }

        public QuotedItem? Parent { get; set; }

        /// <summary>
        /// 仅供内部查看的备注。
        /// </summary>
        public string InsideRemark { get; set; } = string.Empty;

        /// <summary>
        /// 采购备注
        /// </summary>
        public string PurchaseRemark { get; set; } = string.Empty;

        /// <summary>
        /// 用于区别是否为隐藏的第三方类别产品。
        /// </summary>
        public CategoryType CategoryType { get; set; }

        //public virtual Department? ResolveDepartment { get; set; }
        public Guid? ResolveDepartmentId { get; set; }

        public Guid? SupplierId { get; set; }

        /// <summary>
        /// 整单折扣前的单价
        /// </summary>
        public decimal? BeforeWholeDiscountUnitPriceWithTax { get; set; }

        /// <summary>
        /// 数量折扣
        /// </summary>
        public decimal QtyDiscount { get; set; }

        /// <summary>
        /// 等级折扣
        /// </summary>
        public decimal LevelDiscount { get; set; }

        public PriceListDataType PriceListDataType { get; set; }

        public DateTime? RequirementDate { get; set; }

        public decimal? DesirePrice { get; set; }

        public int? DesireDeliveryDays { get; set; }

        public string PdmFilesName { get; set; } = string.Empty;

        public string AuditStatusName()
        {
            switch (AuditStatus)
            {
                case QuoteOrderAuditStatus.IsDraft:
                    return "待提交";
                case QuoteOrderAuditStatus.Submitted:
                    return "待审核";
                case QuoteOrderAuditStatus.Passed:
                    return "已通过审核";
                default:
                    return "未知状态";
            }
        }

        public string ResolveStatusName()
        {
            switch (ResolveStatus)
            {
                case ResolveOrderStatus.Nothing:
                    return "未分解";
                case ResolveOrderStatus.Did:
                    return "已分解";
                case ResolveOrderStatus.Cancel:
                    return "取消分解";
                case ResolveOrderStatus.Completed:
                    return "完成分解";
                case ResolveOrderStatus.NotAllowed:
                    return "不可分解";
                default:
                    return "未知分解单状态"; ;
            }
        }

        /// <summary>
        /// 能否向供应链询价或货期。
        /// </summary>
        /// <returns></returns>
        public bool CanResolve()
        {
            return ResolveStatus == ResolveOrderStatus.Nothing && AuditStatus == QuoteOrderAuditStatus.IsDraft;
        }

        /// <summary>
        /// 能否审核。
        /// </summary>
        /// <returns></returns>
        public bool CanAudit()
        {
            return AuditStatus == QuoteOrderAuditStatus.Submitted;
        }

        /// <summary>
        /// 在单据可编辑状态下，单项是否能修改取决于是否为草稿。
        /// </summary>
        /// <returns></returns>
        public bool IsDraft()
        {
            return AuditStatus == QuoteOrderAuditStatus.IsDraft;
        }

        public bool Audited()
        {
            return AuditStatus == QuoteOrderAuditStatus.Passed;
        }
    }
}
