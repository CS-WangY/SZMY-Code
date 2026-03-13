using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class PrintRequest
    {
        //public PrintRequest()
        //{
        //    this.templateSizeType = "4";
        //    this.printJJcg = 10;
        //    this.printSon = 10;
        //    this.printClientId = "2O5F-Y4MB-BM4Y-F5O2";
        //}
        public string customerCode { get; set; }
        public string platformFlag { get; set; }
        public string templateSizeType { get; set; }
        public Waybillnumberinfo[] waybillNumberInfos { get; set; }
        public Waybillnumberinfo[] waybillInfos { get; set; }
        public string printClientId { get; set; }
        //public int printJJcg { get; set; }
        //public int printSon { get; set; }

        public int generateFileType { get; set; }

        public string callbackUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PrintSettingRequest printSettingRequest { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public UserConfiguration userConfiguration { get; set; }
    }

    public class Waybillnumberinfo
    {
        public string waybillNumber { get; set; }
    }

    public class PrintSettingRequest
    {
        /// <summary>
        /// 10：一联面单 20：寄件存根
        /// </summary>
        public int templateSizeType { get; set; }
        /// <summary>
        /// 10：取货标签 20：寄件存根 30：子母单 40：普通面单
        /// </summary>
        public int printType { get; set; }
    }

    public class UserConfiguration
    {
        /// <summary>
        /// 示例：1,5,10,11-100
        /// </summary>
        public string specifyPageNumber { get; set; }
    }

}
