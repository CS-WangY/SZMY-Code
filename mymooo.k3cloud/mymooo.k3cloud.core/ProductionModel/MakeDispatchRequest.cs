using mymooo.product.selection.SelectionModel;
using System.Text.Json.Serialization;

namespace mymooo.k3cloud.core.ProductionModel
{
    /// <summary>
    /// 生产订单请求参数
    /// </summary>
    public class MakeDispatchRequest
    {
        /// <summary>
        /// 生产订单号
        /// </summary>
        public required string MakeNo { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 生产组织名称
        /// </summary>
        public string? PrdOrgName { get; set; }

        /// <summary>
        /// 生产组织编码
        /// </summary>
        public string? PrdOrgCode { get; set; }

        /// <summary>
        /// 生产订单明细
        /// </summary>
        public required MakeDispatchDetail[] Details { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 生产订单明细
        /// </summary>
        public class MakeDispatchDetail
        {
            /// <summary>
            /// 销售订单明细
            /// </summary>
            public string? SaleOrderNo { get; set; }

            /// <summary>
            /// 生产订单编码
            /// </summary>
            public string? MakeNo { get; set; }

            /// <summary>
            /// 生产订单序号
            /// </summary>
            public int MakeSeq { get; set; }

            /// <summary>
            /// 车间
            /// </summary>
            public string? WorksNo { get; set; }

            /// <summary>
            /// 图号
            /// </summary>
            public required string DwgNo { get; set; }

            /// <summary>
            /// 图号版本
            /// </summary>
            public required string DwgVer { get; set; }

            /// <summary>
            /// 数量
            /// </summary>
            public decimal Qty { get; set; }

            /// <summary>
            /// 急件状态（1：正常，2：急件，3：特急），String，必填
            /// </summary>
            public required string MoType { get; set; }

            /// <summary>
            /// 交期
            /// </summary>
            public DateTime DeadLine { get; set; }

            /// <summary>
            ///  是否提前下料（1：是；0：否）
            /// </summary>
            public string? EarlyMtl { get; set; }

            /// <summary>
            /// 关键字
            /// </summary>
            public string? Key { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public string? Remark { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string? PartTypeCode { get; set; }

            /// <summary>
            /// 产品Id
            /// </summary>
            public long ProductId { get; set; }

            /// <summary>
            /// 明细Id
            /// </summary>
            public long EntryId { get; set; }

            /// <summary>
            /// 用户企业微信编码
            /// </summary>
            public string? UserCode { get; set; }

            /// <summary>
            /// 单价
            /// </summary>
            public decimal Price { get; set; }

            /// <summary>
            /// 金额
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// 自动工艺
            /// </summary>
            public bool AutoCraft { get; set; }

            /// <summary>
            /// 启用mes
            /// </summary>
            public bool EnableMes { get; set; }

            /// <summary>
            /// 客户编码
            /// </summary>
            public string? CustomerCode { get; set; }

            /// <summary>
            /// 客户名称
            /// </summary>
            public string? CustomerName { get; set; }

            /// <summary>
            /// 销售订单日期
            /// </summary>
            public DateTime SalesDate { get; set; }

            /// <summary>
            /// 图号名称
            /// </summary>
            public string? DwgName { get; set; }

            /// <summary>
            /// 报价单号
            /// </summary>
            public string? QuotationOrderNo { get; set; }

            /// <summary>
            /// 报价单行号
            /// </summary>
            public string? QuotationOrderLineNo { get; set; }

            /// <summary>
            /// 图号Id
            /// </summary>
            public string? DrawingRecordId { get; set; }

            /// <summary>
            /// 是否手工单
            /// </summary>
            public bool IsManual { get; set; }

            /// <summary>
            /// 客户采购单号
            /// </summary>
            public string? CustPurchaseNo { get; set; }

            /// <summary>
            /// 销售订单明细Id
            /// </summary>
            public long OrderEntryId { get; set; }

            /// <summary>
            /// 销售订单明细序号
            /// </summary>
            public int OrderEntrySeq { get; set; }

            /// <summary>
            /// 客户物料号
            /// </summary>
            public string? CustMaterialNo { get; set; }

            /// <summary>
            /// 客户型号
            /// </summary>
            public string? CustItemNo { get; set; }

            /// <summary>
            /// 客户产品名称
            /// </summary>
            public string? CustItemName { get; set; }

            /// <summary>
            /// 3d地址
            /// </summary>
            public string? ThreeUrl { get; set; }

            /// <summary>
            /// 3d版本
            /// </summary>
            public string? ThreeVer { get; set; }

            /// <summary>
            /// pdf url
            /// </summary>
            public string? PlaneUrl { get; set; }

            /// <summary>
            /// 原材料
            /// </summary>
            public MakeDispatchMaterialDetail[]? MaterialDetails { get; set; }

            /// <summary>
            /// 参数选择值集合
            /// </summary>
            public List<ProductParameterValueResponse.ProductParameterValue>? ParameterValues { get; set; }
        }

        /// <summary>
        /// 原材料
        /// </summary>
        public class MakeDispatchMaterialDetail
        {
            /// <summary>
            /// 明细ID
            /// </summary>
            [JsonPropertyName("FENTRYID")]
            public long EntryId { get; set; }

            /// <summary>
            /// 材质编码
            /// </summary>
            public string? MtlCode { get; set; }

            /// <summary>
            /// 材质类型
            /// </summary>
            public string? MtlType { get; set; }

            /// <summary>
            /// 是否发送mes
            /// </summary>
            public string? IsSendMes { get; set; }
        }
    }
}
