
namespace mymooo.k3cloud.core.Sales
{
    /// <summary>
    /// 销售出库单明细
    /// </summary>
    public class OutStockDetail
    {
        /// <summary>
        /// 销售出库单号
        /// </summary>
        public string DeliveryNo { get; set; } = string.Empty;

        /// <summary>
        /// 销售出库日期
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 物流公司
        /// </summary>
        public string TrackingName { get; set; } = string.Empty;

        /// <summary>
        /// 物流单号
        /// </summary>
        public string TrackingNumber { get; set; } = string.Empty;

        /// <summary>
        /// 仓库
        /// </summary>
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// 物料编码
        /// </summary>
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 物料名称
        /// </summary>
        public string SkuDesc { get; set; } = string.Empty;

        /// <summary>
        /// 销售出库单明细ID
        /// </summary>
        public long DetailId { get; set; }

        /// <summary>
        /// 发货数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;
    }
}
