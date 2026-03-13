using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.SystemManage.Filter
{
    public class QueryMobileFilter
    {
        public int? Id { get; set; }

        public string Name { get; set; }


        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
