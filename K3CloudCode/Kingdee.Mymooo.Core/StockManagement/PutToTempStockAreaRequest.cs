using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kingdee.Mymooo.Core.StockManagement
{
    /// <summary>
    /// 云仓储入库单参数实体
    /// </summary>
    public class PutToTempStockAreaRequest
    {
        /// <summary>
        /// 入库单号和序号(DN123456-1)
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        public string EntryWarehouseOrderNumber { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ModelNumber { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 待入库数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public NewUnitModel Unit { get; set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        public StockGoodsSupplierModel Supplier { get; set; }

        /// <summary>
        /// 剩余待入库数量
        /// </summary>
        public decimal SurplusQuantity => Quantity;

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime EntryOnUtc { get; set; }

        /// <summary>
        /// 入库类型
        /// </summary>
        public ExternalTypeModel Type { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string LocCode { get; set; }

        /// <summary>
        /// 是否直发仓
        /// </summary>
        public bool IsDirectDeliveryStock { get; set; } = false;

        /// <summary>
        /// 组织编码
        /// </summary>
        public string DeliveryDetOrgCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 仓库对应的仓库发货区域
        /// </summary>
        public string DeliveryplaceCode { get; set; }

        /// <summary>
        /// 大类
        /// </summary>
        public string GroupDesc { get; set; }

        /// <summary>
        /// 小类
        /// </summary>
        public string TypeDesc { get; set; }

        /// <summary>
        /// 净重
        /// </summary>
        public decimal NetWeight { get; set; }

        /// <summary>
        /// 是否自动处理
        /// </summary>
        public bool IsAutoHandle { get; set; }

        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string ExWarehouseOrderNumber { get; set; }

    }

    /// <summary>
    /// 供应商信息
    /// </summary>
    public class StockGoodsSupplierModel
    {
        public string Name { get; set; }

        public string Coding { get; set; }
    }
    /// <summary>
    /// 改调拨入库审核状态
    /// </summary>
    public class ExamineInboundBillModel
    {
        public string ItemId { get; set; }

        public decimal Qty { get; set; }
    }
}
