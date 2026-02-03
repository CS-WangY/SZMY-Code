using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    /// <summary>
    /// 采购库存信息查询
    /// </summary>
    public class PurchaseStockRequest
    {
        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 订单开始时间
        /// </summary>
        public DateTime? OrderStartDate { get; set; }
        /// <summary>
        /// 订单结束时间
        /// </summary>
        public DateTime? OrderEndDate { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 产品大类Id
        /// </summary>
        public string ParentSmallId { get; set; }

        /// <summary>
        /// 小类ID
        /// </summary>
        public string SmallClassId { get; set; }

        /// <summary>
        /// 小类权限过滤(string.Join("','", authorityData);)
        /// </summary>
        public string SmallClassFilter { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string PoNo { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SalesName { get; set; }

        /// <summary>
        /// 采购员名称
        /// </summary>
        public string BuyerName { get; set; }
    }

    /// <summary>
    /// 可用库存数明细
    /// </summary>
    public class AvailableInventoryDetailRequest
    {
        /// <summary>
        /// 产品型号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        public string WarehouseId { get; set; }

    }

    /// <summary>
    /// 物料收发明细
    /// </summary>
    public class StockDetailRptRequest
    {

        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 订单开始时间
        /// </summary>
        public DateTime? OrderStartDate { get; set; }
        /// <summary>
        /// 订单结束时间
        /// </summary>
        public DateTime? OrderEndDate { get; set; }

        /// <summary>
        /// 筛选类型(0全部，1出仓，2入仓)
        /// </summary>
        public int FilterType { get; set; }

    }

    public class PurchaseProductQuantityRequest
    {
        public string CompanyId { get; set; }
        public List<string> ProductModel { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
    public class PurchaseProductQuantity
    {

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 总采购数量
        /// </summary>
        public int TotalPurchaseQuantity { get; set; }

        /// <summary>
        /// 最后入库数量
        /// </summary>
        public int LastInventoryQuantity { get; set; }

        /// <summary>
        /// 最后入库时间
        /// </summary>
        public string LastInventoryDateTime { get; set; }
    }
}
