using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class RegisterWayBillPictureRequest
    {
        /// <summary>
        /// 顾客编码
        /// </summary>
        public string ClientCode { get; set; }
        /// <summary>
        /// 月结卡号
        /// </summary>
        public string CustomerAcctCode { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string WaybillNo { get; set; }
        /// <summary>
        /// 图片类型  2回单  22签单
        /// </summary>
        public string ImgType { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string Phone { get; set; }
        
    }
}
