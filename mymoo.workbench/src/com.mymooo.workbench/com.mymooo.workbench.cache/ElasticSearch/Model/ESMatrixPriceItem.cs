using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{
    /// <summary>
    /// 价目明细
    /// </summary>
    public class ESMatrixPriceItem
    {
        public int PriceType { get; set; }

        public long PriceListId { get; set; }
        /// <summary>
        /// 价目表编码
        /// </summary>
        public string PriceNumber { get; set; }

        /// <summary>
        /// 价目表名称
        /// </summary>
        public string PriceName { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 数量上限
        /// </summary>
        public int MaxQuantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 最小起订量
        /// </summary>
        public int MinPieces { get; set; }

        /// <summary>
        /// 简易型号
        /// </summary>
        public string ShortNumber { get; set; }

        /// <summary>
        /// 企业编码
        /// </summary>
        public string[] CompanyCodes { get; set; }
    }
}
