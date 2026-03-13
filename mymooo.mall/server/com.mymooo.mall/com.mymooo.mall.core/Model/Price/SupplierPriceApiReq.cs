using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price
{
    public class SupplierPriceApiReq
    {

        public long ProductId { get; set; }

       
        public string Number { get; set; } = string.Empty;

        public decimal Qty { get; set; }
        public string ShortNumber { get; set; } = string.Empty;

        public string SupplierCode { get; set; } = string.Empty;  

    }
}
