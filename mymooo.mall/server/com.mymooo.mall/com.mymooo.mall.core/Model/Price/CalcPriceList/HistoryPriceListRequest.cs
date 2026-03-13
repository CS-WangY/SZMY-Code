using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Quotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{

    /// <summary>
    /// 得到历史价的请求.
    /// </summary>
    public class HistoryPriceListRequest
    {
        public List<string> CompanyIdList { get; set; } = new List<string>();

        /// <summary>
        /// 产品型号
        /// </summary>
        public List<string> ProductCodeList { get; set; } = new List<string>();
    }


    /// <summary>
    /// 公司历史 最低成交和报价请求
    /// </summary>
    public class HistoryLowPriceListRequest
    {
        public List<string> CompanyIdList { get; set; } = new List<string>();

        /// <summary>
        /// 产品型号
        /// </summary>
        public List<string> ProductCodeList { get; set; } = new List<string>();

    }
}
