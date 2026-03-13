using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class PrintRequest
    {
        public PrintRequest()
        {
            this.customTemplateCode = "fm_76165_standard1_custom_10040740265_33"; // 自定义模板  ”顺丰开放平台->开发者对接->基础通用API->云打印面单接口->查看“下配置
            this.templateCode = "fm_76165_standard1_MYGCKs6BQmV0";
            this.version = "2.0";
            this.fileType = "pdf";
           
        }

        /// <summary>
        /// 丰密二联面单100mm*150mm ( fm_150_standard_MYGCKs6BQmV0 )
        /// 丰密三联面单100mm*210mm(fm_210_standard_MYGCKs6BQmV0 )
        /// 丰密二联国际面单100mm*150mm(fm_150_international_MYGCKs6BQmV0 )
        /// 丰密唯品会专属面单100mm*100mm(fm_100_vips_MYGCKs6BQmV0 )
        /// 丰密三联国际面单100mm*210mm(fm_210_international_MYGCKs6BQmV0 )
        /// 丰密一联面单76mm*130mm(fm_76130_standard_MYGCKs6BQmV0 )
        /// 丰密二联面单100mm*180mm(fm_180_standard_MYGCKs6BQmV0 )
        /// </summary>
        public string templateCode { get; set; }
        public string version { get; set; }
        public string fileType { get; set; }
        // 同步异步返回面单pdf文件，默认异步
        public bool sync { get; set; }
        public Document[] documents { get; set; }
        public string customTemplateCode { get; set; }
        

        public class Document
        {
            public string masterWaybillNo { get; set; }
            public string backWaybillNo { get; set; }
            public string waybillNoCheckType { get; set; }
            public string waybillNoCheckValue { get; set; }
            public string remark { get; set; }
            public CustomData customData { get; set; }
        }

        public class CustomData
        {
            public string remark { get; set; }
            public string waybillNo { get; set; }
            public string waybillNoLastFourNumber { get; set; }
            public string expressTypeName { get; set; }
        }
    }
}
