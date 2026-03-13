using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Stock
{



    public class SupplyOrgStockQueryReq
    {

        public long OrgId { get; set; } 
        public string ProductCode { get; set; } = string.Empty;
    }
}
