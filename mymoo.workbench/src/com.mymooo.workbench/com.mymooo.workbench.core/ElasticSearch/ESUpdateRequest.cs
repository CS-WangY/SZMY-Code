using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ESUpdateRequest
    {
        public string IndexName { get; set; }
        public Dictionary<string,Dictionary<string,string>> UpdateFields { get; set; }
        public string PriceListId { get; set; }
        /// <summary>
        /// 0 线性价格 1 矩阵价格
        /// </summary>
        public int PriceType { get; set; }
        public string ProductId { get; set; }
        public string Price { get; set; }
        public string[] CompanyCodes { get; set; }
        public int HanldType { get; set; }
        public int AuditStatus { get; set; }
        public List<ElasticSearchPriceListUpdatePriceDto> PriceList { get; set; }
    }

}
