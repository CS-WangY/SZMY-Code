using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{
    public class PriceListCalcRequest
    {
        public List<SalesPriceListCalcItem>? ProductItems { get; set; }

        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyGroupCode { get; set; } = string.Empty;  //集团公司,也就是父公司代码
      //  public string? CompanyId { get; set; }
    }

    public class SalesPriceListCalcItem
    {
        public string? ProductModel { get; set; }
        public long ProductId { get; set; }
        public decimal Qty { get; set; }
    }
}
