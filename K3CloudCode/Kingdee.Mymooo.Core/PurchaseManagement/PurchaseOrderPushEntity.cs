using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    public class PurchaseOrderPushEntity
    {
        public long FID { get; set; }
        public long FEntryID { get; set; }
    }
    public class PrStatus
    {
        public long PrId { get; set; }
        public long[] EntryId { get; set; }
    }
}
