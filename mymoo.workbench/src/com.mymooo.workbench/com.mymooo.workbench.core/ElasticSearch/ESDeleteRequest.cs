using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ESDeleteRequest
    {
        public string IndexName { get; set; }
        public string[] DocumentIds { get; set; }
        public Dictionary<string,string> DeleteFilter { get; set; }
    }
}
