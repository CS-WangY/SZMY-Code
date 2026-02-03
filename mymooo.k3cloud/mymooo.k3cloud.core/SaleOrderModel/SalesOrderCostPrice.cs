using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.core.Attributes.Redis;

namespace mymooo.k3cloud.core.SaleOrderModel
{
    /// <summary>
    /// 销售订单成本价
    /// </summary>
    [RedisKey("mymooo-product-number", 1)]
    public class SalesOrderCostPrice
    {
        private string productNumber = string.Empty;

        /// <summary>
        /// 索引key
        /// </summary>
        [RedisPrimaryKey]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// 产品型号
        /// </summary>
        public string MaterialNumber
        {
            get => productNumber;
            set
            {
                productNumber = value;
                this.Id = productNumber.Replace("-", "").Trim().ToLower();
            }
        }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;
        /// <summary>
        /// 销售单价
        /// </summary>
        public decimal SalTaxPrice { get; set; }
        /// <summary>
        /// 销售数量
        /// </summary>
        public decimal SalQTY { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; } = string.Empty;
        /// <summary>
        /// 供应商单价
        /// </summary>
        public decimal SupplierUnitPrice { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string FBILLNO { get; set; } = string.Empty;

        /// <summary>
        /// 单价类型
        /// </summary>
        [RedisMainField(groupId: "Cost")]
        public string PriceType { get; set; } = string.Empty;
        /// <summary>
        /// 订单类型
        /// </summary>
        public string OrderType { get; set; } = string.Empty;
        /// <summary>
        /// 产品ID
        /// </summary>
        public long ProductId { get; set; }
    }
}
