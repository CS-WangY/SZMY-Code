using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Stock
{



    public class SafetyStockQueryReq
    {
        public List<SafetyStockQuerySimpleDto> ProductItemList { get; set; } = [];
    }
    public class SafetyStockQuerySimpleDto
    {
        public string ProductModel { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public int StockQty { get; set; }
        public int StockDays { get; set; }
    }
}
