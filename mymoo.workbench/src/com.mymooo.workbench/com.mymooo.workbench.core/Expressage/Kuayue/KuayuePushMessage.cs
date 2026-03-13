using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class KuayuePushMessage
    {
        public string desc { get; set; }
        public string ecWaybillNumber { get; set; }
        public string mailno { get; set; }
        public int node { get; set; }
        public string pickupTime { get; set; }
        public string receiveTime { get; set; }
        public string receiver { get; set; }
        public string returnFlag { get; set; }
        public string step { get; set; }
        public string time { get; set; }
        public string packager { get; set; }
        public string packagerTel { get; set; }

        public string currCity { get; set; }
    }
}
