using com.mymooo.workbench.core.WeiXinWork.Approver.Crm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mymooo.workbench.core.Enum;

namespace com.mymooo.workbench.core.WebSocket.InputData
{
    public class InputDataMessageRequest
    {
        public WebSocketImportType Type { get; set; }

        /// <summary>
        /// 导入文件路径
        /// </summary>
        public string FilePath { get; set; }


        /// <summary>
        /// 是否导入全部-转移时公海客户存在有效申领或申请是否全部导入
        /// </summary>
        public bool IsAll { get; set; }

    }
}
