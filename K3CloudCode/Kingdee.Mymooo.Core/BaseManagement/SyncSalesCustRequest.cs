using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class SyncSalesCustRequest
    {
        public int SalesUserId { get; set; }

        public int DeptId { get; set; }

        public int SaleGroupId { get; set; }

        public string OrgNumber { get; set; }
    }
}
