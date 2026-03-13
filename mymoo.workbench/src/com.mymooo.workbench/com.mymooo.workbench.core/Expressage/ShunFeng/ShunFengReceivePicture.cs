using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    /// <summary>
    /// 顺丰接收回单、签单回调
    /// </summary>
    public class ShunFengReceivePicture
    {
        /// <summary>
        /// 运单号
        /// </summary>
        public string WaybillNo { get; set; }

        /// <summary>
        /// 企业标识
        /// </summary>
        public string CompanyLogo { get; set; }

        /// <summary>
        /// 图片信息（默认加密）
        /// </summary>
        public string Content { get; set; }

        public string FileUrl { get; set; }
    }
}
