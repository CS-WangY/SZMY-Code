using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.ReportModel
{
    /// <summary>
    /// crm获取报表请求model
    /// </summary>
    public class CrmReportRequestModel
    {
        /// <summary>
        /// 权限过滤
        /// </summary>
        public int IsAll { get; set; }

        /// <summary>
        /// 微信code
        /// </summary>
        public List<string> AuthWechatCodes { get; set; } = [];
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime DateS { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime DateE { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortFiled { get; set; } = string.Empty;
        /// <summary>
        /// 排序规则
        /// </summary>
        public string Asc { get; set; } = string.Empty;
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CompanyCode { get; set; } = string.Empty;
    }
    
}
