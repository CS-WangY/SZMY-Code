using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price
{
    public class SupplierPriceReq
    {

        public long ProductId { get; set; }

        public long resolvedItemId { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public decimal Qty { get; set; }
        public string ShortNumber { get; set; } = string.Empty;
        
    }
}
