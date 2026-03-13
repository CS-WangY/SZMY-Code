using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class QueryMobileHistory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string QueryByName { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
