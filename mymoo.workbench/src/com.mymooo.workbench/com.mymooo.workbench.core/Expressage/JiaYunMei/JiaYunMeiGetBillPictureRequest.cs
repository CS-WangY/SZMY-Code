using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.JiaYunMei
{
    public class JiaYunMeiGetBillPictureRequest
    {
        public string BillCode { get; set; }

        /// <summary>
        /// 图片类型：1签收单 2回单
        /// </summary>
        public string BlType { get; set; }
    }
}
