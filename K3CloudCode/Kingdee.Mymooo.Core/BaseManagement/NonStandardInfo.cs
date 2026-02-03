using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class NonStandardRequest
    {
        public Boolean success { get; set; }
        public Int32 code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }
    }
}
