using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class PrintCallbackRequest
    {
        public string LogisticID { get; set; }
        public string RequestID { get; set; }
        public string ServiceCode { get; set; }
        public long Timestamp { get; set; }
        public string MsgDigest { get; set; }
        public string MsgData { get; set; }
        public string Nonce { get; set; }
    }

    public class PrintFileCallbackRequest
    {
        public string content { get; set; }
        public string fileName { get; set; }
        public string waybillNo { get; set; }
        public string fileType { get; set; }
        public string seqNo { get; set; }
        public string areaNo { get; set; }
        public string pageNo { get; set; }
        public string templateCode { get; set; }
    }

}
