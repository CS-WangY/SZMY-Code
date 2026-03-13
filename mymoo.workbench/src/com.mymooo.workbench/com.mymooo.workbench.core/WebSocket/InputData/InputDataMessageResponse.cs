using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.WebSocket.InputData
{
    public class InputDataMessageResponse<T>
    {

        /// <summary>
        /// 进度
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 发送的数据包
        /// </summary>
        public T Data { get; set; }
        public bool IsEnd { get; set; }
        public string Code { get; set; }
    }
}
