using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class PrdMoStatus
    {
        public string BillNo { get; set; }
        public long MoId { get; set; }
        public long[] MoEntryId { get; set; }
        public int Type { get; set; }
    }
}
