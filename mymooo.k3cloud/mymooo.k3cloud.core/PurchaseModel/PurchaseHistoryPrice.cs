using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.core.Attributes.Redis;

namespace mymooo.k3cloud.core.RedisCacheModel
{
    /// <summary>
    /// 采购历史价
    /// </summary>
    [RedisKey("mymooo-product-number", 1)]
    public class PurchaseHistoryPrice
    {
        private string productNumber = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [RedisMainField(groupId: "history")]
        public string type { get; set; } = string.Empty;
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>

        public decimal Price { get; set; }

        /// <summary>
        /// 索引key
        /// </summary>
        [RedisPrimaryKey]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ProductNumber
        {
            get => productNumber;
            set
            {
                productNumber = value;
                this.Id = productNumber.Replace("-", "").Trim().ToLower();
            }
        }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime AuditTime { get; set; }

        /// <summary>
        /// 采购订单号
        /// </summary>
        public string POOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 含税单价
        /// </summary>
        public decimal TaxPrice { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;
        /// <summary>
        /// 产品ID
        /// </summary>
        public long FProductId { get; set; }
        /// <summary>
        /// 简易型号
        /// </summary>
        public string ShortProductNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// 产品ID全型号
    /// </summary>
    [RedisKey("mymooo-product-fullnumber-price", 1, false)]
    public class ProductIdFullNumber
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        [RedisPrimaryKey]
        [RedisValue]
        public long ProductId { get; set; }
        /// <summary>
        /// 产品型号编码
        /// </summary>
        [RedisMainField(groupId: "full")]
        public string ProductNumber { get; set; } = string.Empty;
        /// <summary>
        /// 历史价格日期
        /// </summary>
        [RedisValue(mainKey: "full")]
        public DateTime HistoryDate { get; set; }
    }

    /// <summary>
    /// 产品ID简易型号
    /// </summary>
    [RedisKey("mymooo-product-shortnumber-price", 1, false)]
    public class ProductIdShortNumber
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        [RedisPrimaryKey]
        [RedisValue]
        public long ProductId { get; set; }
        /// <summary>
        /// 产品型号编码
        /// </summary>
        [RedisMainField(groupId: "short")]
        
        public string ProductNumber { get; set; } = string.Empty;
        /// <summary>
        /// 历史价格日期
        /// </summary>
        [RedisValue(mainKey: "short")]
        public DateTime HistoryDate { get; set; }
    }

}
