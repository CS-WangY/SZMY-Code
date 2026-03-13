using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.WeChat
{
    public class WeUserResponse
    {
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }
    public class WeUserAssistant
    {
        public string assistantCode { get; set; } = string.Empty;
        public string assistantName { get; set; } = string.Empty;
        public bool isDelete { get; set; }
    }

}
