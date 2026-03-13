using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class CancelOrderResponse
    {
        public bool success { get; set; }
        public string errorCode { get; set; }
        public object errorMsg { get; set; }
        public Msgdata msgData { get; set; }

        public class Msgdata
        {
            public string orderId { get; set; }
            public Waybillnoinfolist[] waybillNoInfoList { get; set; }

            /// <summary>
            /// 备注 1:客户订单号与顺丰运单不匹配 2 :操作成功
            /// </summary>
            public int resStatus { get; set; }
            public object extraInfoList { get; set; }
        }

        public class Waybillnoinfolist
        {
            public int waybillType { get; set; }
            public string waybillNo { get; set; }
        }
    }
}
