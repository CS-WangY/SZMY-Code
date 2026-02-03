using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    /// <summary>
    /// 变更费用采购订单
    /// </summary>
    public class ChangeCostPurchaseOrderRequest
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }

        ///// <summary>
        ///// 变更原因
        ///// </summary>
        //public string Note { get; set; }

        /// <summary>
        /// 费用采购订单明细
        /// </summary>
        public List<ChangeCostPurchaseOrderDet> Details { get; set; }
    }

    /// <summary>
    /// 费用采购订单明细
    /// </summary>
    public class ChangeCostPurchaseOrderDet
    {
        /// <summary>
        /// MES明细ID
        /// </summary>
        public string MesEntryId { get; set; }

        /// <summary>
        /// 含税单价
        /// </summary>
        public decimal TaxPrice { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

    }


    /// <summary>
    /// 费用采购删除
    /// </summary>
    public class CostPurchaseOrderDeleteRequest
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }

    }
}
