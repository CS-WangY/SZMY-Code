using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchEntity
    {
        /// <summary>
        /// 销售订单编号
        /// </summary>
        public string SalesOrderNumber { get; set; }

        /// <summary>
        /// 派产订单中的零件编号
        /// </summary>
        public List<string> Parts { get; set; }

        /// <summary>
        /// 派产日期
        /// </summary>
        public DateTime? DispatchDateTime { get; set; }

        /// <summary>
        /// 派产人
        /// </summary>
        public string DispatchUser { get; set; }

        /// <summary>
        /// 派产订单中的零件ID
        /// </summary>
        public List<int> PartsID { get; set; }
    }
}
