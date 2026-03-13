using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.JiaYunMei
{
    public class JiaYunMeiGetBillPictureResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public bool Success { get; set; }
        public List<JiaYunMeiGetBillPicture> Data { get; set; }
    }

    public class JiaYunMeiGetBillPicture
    {
        public string Path { get; set; }

        public string BillCode { get; set; }
    }
}
