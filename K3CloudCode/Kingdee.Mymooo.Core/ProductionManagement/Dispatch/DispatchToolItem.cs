using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchToolItem
    {
        /// <summary>
        /// ***必要参数***）刀位号
        /// </summary>
        public int toolLocation { get; set; }
        /// <summary>
        /// （***必要参数***）刀名
        /// </summary>
        public string toolName { get; set; }
        /// <summary>
        /// （***必要参数***）刀具总长
        /// </summary>
        public float toolLength { get; set; }
    }
}
