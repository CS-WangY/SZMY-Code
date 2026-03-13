using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class SearchRoutesResponse
    {
        public bool success { get; set; }
        public string errorCode { get; set; }
        public object errorMsg { get; set; }
        public Msgdata msgData { get; set; }

        public class Msgdata
        {
            public Routeresp[] routeResps { get; set; }
        }

        public class Routeresp
        {
            public string mailNo { get; set; }
            public Route[] routes { get; set; }
        }

        public class Route
        {
            public string acceptAddress { get; set; }
            public string acceptTime { get; set; }
            public string remark { get; set; }
            public string opCode { get; set; }
        }
    }
}
