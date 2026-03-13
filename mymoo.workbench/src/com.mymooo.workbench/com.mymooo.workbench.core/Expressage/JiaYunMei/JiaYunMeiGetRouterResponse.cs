using com.mymooo.workbench.core.Expressage.Kuayue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.JiaYunMei
{
    public class JiaYunMeiGetRouterResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<JiaYunMeiGetRouterData> Data { get; set; }
        /// <summary>
        /// 查询成功
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }
    }

    public class JiaYunMeiGetRouterData
    {
        /// <summary>
        /// 单号
        /// </summary>
        public string BillCode { get; set; }
        /// <summary>
        /// 收件、派件员
        /// </summary>
        public string DispatchOrSendMan { get; set; }
        /// <summary>
        /// 收派件员电话
        /// </summary>
        public string DispatchOrSendManPhone { get; set; }
        /// <summary>
        /// 上一站/下一站
        /// </summary>
        public string RecNextSite { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 轨迹描述
        /// </summary>
        public string ScanCount { get; set; }
        /// <summary>
        /// 扫描时间
        /// </summary>
        public string ScanDate { get; set; }
        /// <summary>
        /// 扫描人
        /// </summary>
        public string ScanMan { get; set; }
        /// <summary>
        /// 扫描网点
        /// </summary>
        public string ScanSite { get; set; }
        /// <summary>
        /// 扫描类型
        /// </summary>
        public string ScanType { get; set; }
        /// <summary>
        /// 签收人
        /// </summary>
        public string SignMan { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sitePhone { get; set; }
    }
}
