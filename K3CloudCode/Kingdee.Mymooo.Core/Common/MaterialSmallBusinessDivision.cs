using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    public class MaterialSmallBusinessDivision
    {
        public long MaterialId { get; set; }

        /// <summary>
        /// 产品小类
        /// </summary>
        public long SmallId { get; set; }

        /// <summary>
        /// 产品大类
        /// </summary>
        public long ParentSmallId { get; set; }

        /// <summary>
        /// 税收分类编码
        /// </summary>
        public long TaxCodeId { get; set; }

        /// <summary>
        /// 事业部
        /// </summary>
        public string BusinessDivision { get; set;}

        /// <summary>
        /// 供应组织
        /// </summary>
        public long SupplyOrgId { get; set; }
    }
}
