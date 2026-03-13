using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class PushRequest
    {
        public string CompanyCode { get; set; }
        public string Timestamp { get; set; }
        public string Digest { get; set; }
        public string Params { get; set; }
    }
}
