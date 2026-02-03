using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchPart
    {
        /// <summary>
        /// ***必要参数***）零件图号
        /// 且一个订单内不应重复
        /// </summary>
        public string drawNum { get; set; }

        /// <summary>
        /// 图号版本 ，如果不输入，默认为A
        /// </summary>
        public string drawNumVersion { get; set; } = "A";

        /// <summary>
        /// （***必要参数***）订单号
        /// </summary>
        public string moNum { get; set; }

        /// <summary>
        /// （***必要参数***）订单数量
        /// </summary>
        public int orderNum { get; set; } = 1;

        /// <summary>
        /// 订单类型，允许输入1、2、3，其中：1-正常；2-急件；3-特急
        /// </summary>
        public int orderType { get; set; } = 1;

        /// <summary>
        /// （***必要参数***）要求交期
        /// </summary>
        public DateTime deadLine { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }
    }
}
