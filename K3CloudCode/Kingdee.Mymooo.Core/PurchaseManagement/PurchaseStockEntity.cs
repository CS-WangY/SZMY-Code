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
    public class PurchaseStockEntity
    {
        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 采购单编号
        /// </summary>
        public string PoNo { get; set; }

        /// <summary>
        /// 采购单序号
        /// </summary>
        public int PoSeq { get; set; }

        /// <summary>
        /// 采购单日期
        /// </summary>
        public string PoDate { get; set; }

        /// <summary>
        /// 蚂蚁型号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// 客户型号
        /// </summary>
        public string CustItemNo { get; set; }

        /// <summary>
        /// 大类名称
        /// </summary>
        public string ParentSmallDesc { get; set; }

        /// <summary>
        /// 小类名称
        /// </summary>
        public string SmallClassDesc { get; set; }

        /// <summary>
        /// 含税单价(采购单价)
        /// </summary>
        public decimal VatPrice { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public decimal SupplierUnitPrice { get; set; }

        /// <summary>
        /// 采购数量
        /// </summary>
        public decimal PoQty { get; set; }
        /// <summary>
        /// 入库数量
        /// </summary>
        public decimal QtyRecd { get; set; }
        /// <summary>
        /// 未入库数量
        /// </summary>
        public decimal UnQtyRecd { get; set; }
        /// <summary>
        /// 可用库存
        /// </summary>
        public decimal UsableQty { get; set; }

        /// <summary>
        /// 供应商回复发货日期
        /// </summary>
        public string VdrDnDate { get; set; }

        /// <summary>
        /// 回复说明(延误说明)
        /// </summary>
        public string DelayText { get; set; }

        /// <summary>
        /// 采购订单状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 采购单交期
        /// </summary>
        public string PoArrivalDate { get; set; }

        /// <summary>
        /// 销售单编号
        /// </summary>
        public string SoNo { get; set; }

        /// <summary>
        /// 销售单序号
        /// </summary>
        public int SoSeq { get; set; }

        /// <summary>
        /// 销售单日期
        /// </summary>
        public string SoDate { get; set; }

        /// <summary>
        /// 销售交期
        /// </summary>
        public string SoArrivalDate { get; set; }

        /// <summary>
        /// 采购员名称
        /// </summary>
        public string BuyerName { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        public string SalesName { get; set; }
    }

    /// <summary>
    /// 可用库存数明细
    /// </summary>
    public class AvailableInventoryDetailEntity
    {
        /// <summary>
        /// 蚂蚁型号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WarehouseName { get; set; }

        /// <summary>
        /// 可用库存
        /// </summary>
        public decimal UsableQty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

    }

    /// <summary>
    /// 物料收发明细
    /// </summary>
    public class StockDetailRptEntity
    {
        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 单号
        /// </summary>
        public string DocNo { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public string TranDate { get; set; }

        /// <summary>
        /// 交易类型
        /// </summary>
        public string TranType { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string StockName { get; set; }

        /// <summary>
        /// 仓存数量
        /// </summary>
        public decimal PQoh { get; set; }

        /// <summary>
        /// 结余数量
        /// </summary>
        public decimal LQoh { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 参考单号1
        /// </summary>
        public string RefNo { get; set; }

        /// <summary>
        /// 进出状态(不需要的数据,out出仓,in入仓)
        /// </summary>
        public string InOutStatus { get; set; } = "";

        /// <summary>
        /// 序号(只用于记录金蝶接口返回的顺序，用于数据排序)
        /// </summary>
        public int Rn { get; set; }
    }

    /// <summary>
    /// 组织ID和物料ID
    /// </summary>
    public class OrgIdAndMaterialIdEntity
    {
        /// <summary>
        /// 组织ID
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public string MaterialId { get; set; }
    }
}
