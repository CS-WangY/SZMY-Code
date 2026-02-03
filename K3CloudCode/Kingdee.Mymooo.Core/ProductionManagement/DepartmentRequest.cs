using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DepartmentRequest
    {
        /// <summary>
        /// 创建组织
        /// </summary>
        public string FCreateOrgId { get; set; } = "GDMYZZ"; //广东蚂蚁工场制造有限公司
        /// <summary>
        /// 使用组织
        /// </summary>
        public string FUseOrgId { get; set; } = "GDMYZZ";
        /// <summary>
        /// 名称
        /// </summary>
        public string FName { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string FNumber { get; set; }
        /// <summary>
        /// 部门全称
        /// </summary>
        public string FFullName { get; set; }
        /// <summary>
        /// 部门属性
        /// </summary>
        public string FDeptProperty { get; set; } = "DP01_SYS"; //基本生产部门

        /// <summary>
        /// 车间编码
        /// </summary>
        public string workshopCode { get; set; }

        /// <summary>
        /// 车间名称
        /// </summary>
        public string workshopName { get; set; }

    }
}
