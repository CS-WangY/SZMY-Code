using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    public class SalerCustList
    {
        /// <summary>
        /// 员工编码
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// 员工名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 岗位id
        /// </summary>
        public long Staffid { get; set; }
        /// <summary>
        /// 组织id
        /// </summary>
        public long Bizorgid { get; set; }
        /// <summary>
        /// 组织编码
        /// </summary>
        public string BizOrgNumber { get; set; }
        /// <summary>
        /// 业务员id
        /// </summary>
        public long Operatorid { get; set; }
        /// <summary>
        /// 业务组id
        /// </summary>
        public long Operatorgroupid { get; set; }
    }
}
