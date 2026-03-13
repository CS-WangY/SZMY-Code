using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ElasticSearchPriceListUpdatePriceDto
    {
        public string Number { get; set; }
        public long PriceListId { get; set; }
        /// <summary>
        /// 价目表所在集合中的下标
        /// </summary>
        public int PriceIndex { get; set; }
        public decimal Price { get; set; }
    }
}
