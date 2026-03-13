using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Quotation
{
    public enum OrderType
    {
        /// <summary>
        /// 标准产品的单据
        /// </summary>
        Standard = 0,

        /// <summary>
        /// 非标准产品的单据
        /// </summary>
        Nonstandard = 1,

        /// <summary>
        /// 3D产品报价
        /// </summary>
        ThreeD = 2,

        /// <summary>
        /// 样品订单
        /// </summary>
        Sample = 3


    }
}
