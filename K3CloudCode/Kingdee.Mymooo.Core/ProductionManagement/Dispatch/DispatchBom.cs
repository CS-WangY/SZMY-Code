using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchBom
    {
        /// <summary>
        /// 坯料材质（新需求不需要了，暂时不注释）
        /// </summary>
        public string material { get; set; }


        /// <summary>
        /// 坯料名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 坯料规格，和ERP统一一下命名规则（新需求为物料号）
        /// </summary>
        public string specification { get; set; }
        ///// <summary>
        ///// 坯料用量：切割长度
        ///// </summary>
        //public float cuttingLength { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public float weight { get; set; }
        /// <summary>
        /// Url（图纸地址）
        /// </summary>
        public string url { get; set; }
    }
}
