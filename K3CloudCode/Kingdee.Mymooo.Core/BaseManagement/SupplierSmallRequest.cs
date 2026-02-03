using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class SupplierSmallRequest
    {
        public string SupplierCode { get; set; }
        public long SmallId { get; set; }
        public string SmallCode { get; set; }
    }
    public class PurchaseSmallRequest
    {
        public long userId { get; set; }
        public string userCode { get; set; }
        public string userName { get; set; }
        public long productSmallClassId { get; set; }
        public string productSmallClassName { get; set; }
    }
}
