using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class OrderRoutePushRequest
    {
        public Track_List[] track_list { get; set; }
        public class Track_List
        {
            public Trace_List[] trace_list { get; set; }
            public string tracking_number { get; set; }
        }

        public class Trace_List
        {
            public string city { get; set; }
            public string description { get; set; }
            public string site { get; set; }
            public string status { get; set; }
            public string time { get; set; }
        }
    }
}
