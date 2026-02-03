using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
    public class WarehousesInventoryQueryEntity
    {
        public WarehousesInventoryQueryEntity()
        {
            goods = new List<Goods>();
        }
        public List<Goods> goods { get; set; }
        public class Goods
        {
            public Goods()
            {
                unit = new Unit();
                position = new Position();
                warehouse = new Warehouse();
            }
            public string id { get; set; }
            public string name { get; set; }
            public string modelNumber { get; set; }
            public string specification { get; set; }
            public decimal quantity { get; set; }
            public string version { get; set; }
            public Unit unit { get; set; }
            public Position position { get; set; }

            private Warehouse _warehouse;
            public Warehouse warehouse
            {
                get { return _warehouse ?? new Warehouse(); }
                set { _warehouse = value; }
            }

            public class Unit
            {
                public int id { get; set; }
                public string name { get; set; }
            }
            public class Position
            {
                public int type { get; set; }
                public string address { get; set; }
                public string coding { get; set; }
            }

            public class Warehouse
            {
                public int id { get; set; }
                public string position { get; set; }
                public string coding { get; set; } = "STORE";
            }
        }

    }

    public class WarehousesInventoryPartialEntity
    {
        /// <summary>
        /// 物料号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemDesc { get; set; }
        /// <summary>
        /// 仓位编码
        /// </summary>
        public string LocCode { get; set; }
        /// <summary>
        /// ERP库存量
        /// </summary>
        public decimal? ErpQty { get; set; }
        /// <summary>
        /// ERP备库数量
        /// </summary>
        public decimal SReserve { get; set; }
        /// <summary>
        /// ERP库存单位
        /// </summary>
        public string ErpUom { get; set; }
        /// <summary>
        /// 云仓储库存量
        /// </summary>
        public decimal? WarehousesQty { get; set; }
        /// <summary>
        /// 云仓储库存单位
        /// </summary>
        public string WarehousesUom { get; set; }
        /// <summary>
        /// ERP库存量 与 云仓储库存量 的差异数量
        /// </summary>
        public decimal DiffQty { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; set; }
        public string DisabledDesc { get; set; }
    }
    public class WarehousesInventoryDetPartialEntity
    {
        public string ItemNo { get; set; }
        public string ItemDesc { get; set; }
        public string LocCode { get; set; }
        public string PositionCode { get; set; }
        public string PositionDesc { get; set; }
        public decimal Qty { get; set; }
        public string Uom { get; set; }
    }

    public class WarehousesInventoryModel
    {
        public WarehousesInventoryModel()
        {
            WarehousesInventoryDetList = new List<WarehousesInventoryDetModel>();
        }
        /// <summary>
        /// 调整平台
        /// </summary>
        public string AdjustObject { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string CompanyId { get; set; }
        public List<WarehousesInventoryDetModel> WarehousesInventoryDetList { get; set; }
        public class WarehousesInventoryDetModel
        {
            public string ItemNo { get; set; }

            public string LocCode { get; set; }
            /// <summary>
            /// ERP库存量
            /// </summary>
            public decimal ErpQty { get; set; }
            /// <summary>
            /// 云仓储库存量
            /// </summary>
            public decimal WarehousesQty { get; set; }
            /// <summary>
            /// ERP库存单位
            /// </summary>
            public string ErpUom { get; set; }
            /// <summary>
            /// 云仓储库存单位
            /// </summary>
            public string WarehousesUom { get; set; }
        }
    }

    public class WarehousesInventoryOfStateEntity
    {
        /// <summary>
        /// 物料号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 仓位编码
        /// </summary>
        public string LocCode { get; set; }
        /// <summary>
        /// ERP当前调整中数量
        /// </summary>
        public decimal ErpAdjustQty { get; set; }
        /// <summary>
        /// Wh当前调整中数量
        /// </summary>
        public decimal WhAdjustQty { get; set; }
    }
}
