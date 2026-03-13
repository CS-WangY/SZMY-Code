using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{
    public class ESMatrixPrice
    {
        public ESMatrixPrice()
        {
            PriceItems = new List<ESMatrixPriceItem>();
        }
        /// <summary>
        /// 型号 也用作文档Id
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 产品Id 
        /// </summary>
        public long? ProductId { get; set; }
        /// <summary>
        /// 简易型号
        /// </summary>
        public string ShortNumber { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 小类Id
        /// </summary>
        public int SecondCategoryId { get; set; }
        /// <summary>
        /// 小类名称
        /// </summary>
        public string SecondCategoryName { get; set; }
        /// <summary>
        /// 价目明细集合
        /// </summary>
        public List<ESMatrixPriceItem> PriceItems { get; set; }
    }
}
