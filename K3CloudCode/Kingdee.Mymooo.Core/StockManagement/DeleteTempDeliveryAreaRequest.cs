using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
    public class DeleteTempDeliveryAreaRequest
    {
        public string ExWarehouseOrderNumber { get; set; }

        public string ItemId { get; set; }
    }

    public class CloudStockRevocationStockOutRequest
    {
        public string ExWarehouseOrderNumber { get; set; }

        public string ItemId { get; set; }
    }

    public class CloudStockRevocationStockInRequest
    {
        public string EntryWarehouseOrderNumber { get; set; }

        public string ItemId { get; set; }

        public decimal Quantity { get; set; }
    }
}
