namespace mymooo.k3cloud.core.Sales
{
    /// <summary>
    /// 主对象：表示交付信息实体
    /// </summary>
    public class OutStockResponse
    {
        /// <summary>
        /// 交付单号 (DEL开头+日期+序列号)
        /// </summary>
        public required string DeliveryNo { get; set; }

        /// <summary>
        /// 交付日期时间 (格式: yyyy-MM-dd HH:mm:ss)
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public required string WarehouseName { get; set; }

        /// <summary>
        /// SKU（库存量单位）明细列表
        /// </summary>
        public required List<SkuInfo> Skus { get; set; }

        /// <summary>
        /// 对应物流单列表
        /// </summary>
        public List<LogisticsInfo> LogisticsList { get; set; } = [];

        /// <summary>
        /// SKU（库存量单位）信息实体
        /// </summary>
        public class SkuInfo
        {
            /// <summary>
            /// 明细ID (注意JSON字段包含空格)
            /// </summary>
            public long DetailId { get; set; }

            /// <summary>
            /// SKU唯一标识
            /// </summary>
            public required string SkuId { get; set; }

            /// <summary>
            /// SKU描述信息
            /// </summary>
            public string? SkuDesc { get; set; }

            /// <summary>
            /// 计量单位
            /// </summary>
            public required string Unit { get; set; }

            /// <summary>
            /// 商品数量
            /// </summary>
            public int Quantity { get; set; }
        }

        /// <summary>
        /// 对应物流单列表
        /// </summary>
        public class LogisticsInfo
        {
            /// <summary>
            /// 销售订单号
            /// </summary>
            public required string ThirdOrderId { get; set; }

            /// <summary>
            /// 物流单号
            /// </summary>
            public string? LogisticsCompany { get; set; }

            /// <summary>
            /// 物流单号
            /// </summary>
            public string? DeliveryId { get; set; }

            /// <summary>
            /// 物流详情
            /// </summary>
            public List<Tracking>? OrderTrack { get; set; }
        }

        /// <summary>
        /// 订单追踪记录实体
        /// </summary>
        public class Tracking
        {
            /// <summary>
            /// 操作时间戳 (Unix时间戳，毫秒级)
            /// </summary>
            public long OperateTime { get; set; }

            /// <summary>
            /// 多语言描述内容
            /// </summary>
            public Dictionary<string, string> Content { get; set; } = [];

            /// <summary>
            /// 操作执行方
            /// </summary>
            public string? Operator { get; set; }
        }
    }
}
