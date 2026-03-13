using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.SDK.Model
{
    /// <summary>
    /// 产品小类简单对象
    /// </summary>
    public class ProductClassSimpleModel
    {
        /// <summary>
        /// 
        /// </summary>
        public long ClassId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClassName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public long SmallClassId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SmallClassName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public long ProductEngineerId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProductEngineerName { get; set; } = string.Empty;
    }
}
