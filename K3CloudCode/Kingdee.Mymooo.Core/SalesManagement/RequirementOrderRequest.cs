using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    /// <summary>
    /// 组织间需求单
    /// </summary>
    public class RequirementOrderRequest
    {
        public long RequirementOrgId { get; set; }

        /// <summary>
        /// 需求组织
        /// </summary>
        public string RequirementOrgNumber { get; set; }
        public string RequirementOrgName { get; set; }

        /// <summary>
        /// 需求日期
        /// </summary>
        public DateTime? RequirementDate { get; set; }
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 供货组织
        /// </summary>
        public string SupplyOrgNumber { get; set; }
        public string SupplyOrgName { get; set; }

        public List<RequirementOrderDetailRequest> Details { get; set; }

        /// <summary>
        /// 需求单号（报价单号）
        /// </summary>
        public string RequirementOrderNo { get; set; }

        public string Remarks { get; set; }

        /// <summary>
        /// 核算组织
        /// </summary>
        public long AccountOrgId { get; set; }
    }
}
