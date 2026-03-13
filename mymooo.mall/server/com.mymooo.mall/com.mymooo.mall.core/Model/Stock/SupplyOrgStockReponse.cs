using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Stock
{



    public class SupplyOrgStockReponse
    {

        public string ItemNo { get; set; } = string.Empty;

        public OrgStockInfo StockInfo { get; set; } = new OrgStockInfo();
    }


    public class OrgStockInfo
    {
        /// <summary>
        /// 此物料总待出库数量
        /// </summary>
        public decimal UnQtyShipdSum { get; set; }

        /// <summary>
        /// 实际可用库存
        /// </summary>
        public decimal UsableQty { get; set; }

        /// <summary>
        /// 采购在途量
        /// </summary>
        public decimal OnOrderQty { get; set; }

        /// <summary>
        /// 品检库存
        /// </summary>
        public decimal QtyInsp { get; set; }

    }


}
