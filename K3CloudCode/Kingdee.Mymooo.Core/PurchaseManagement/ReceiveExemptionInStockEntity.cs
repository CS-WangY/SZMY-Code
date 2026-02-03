using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    /// <summary>
    /// 收料免检下推采购入库
    /// </summary>
    public class ReceiveExemptionInStockEntity
    {
        public string BillNo { get; set; }
        public string PrimaryKeyValue { get; set; }

        public string EntryPrimaryKeyValue { get; set; }

        public string FormID { get; set; }
    }
}
