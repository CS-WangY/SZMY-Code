using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class SearchOrderRequest
    {
        public SearchOrderRequest()
        {
            this.SearchType = "1";
            this.Language = "zh-cn";
        }

        /// <summary>
        /// 查询类型：1正向单 2退货单
        /// </summary>
        public string SearchType { get; set; }

        public string OrderId { get; set; }

        /// <summary>
        /// 响应报文的语言， 缺省值为zh-CN，目前支持以下值zh-CN 表示中文简体， zh-TW或zh-HK或 zh-MO表示中文繁体， en表示英文
        /// </summary>
        public string Language { get; set; }
    }

}
