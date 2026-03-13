using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price
{
    public class SupplierPriceRep
    {

        public string ProductCode { get; set; } = string.Empty;

        public decimal SupplierUnitPrice { get; set; }
        public string SupplierCode { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

    }
}
