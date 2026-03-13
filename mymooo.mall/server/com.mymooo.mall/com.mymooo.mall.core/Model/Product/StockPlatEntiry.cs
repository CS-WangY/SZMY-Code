using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Product
{

    public class StockPlatEntity
    {
        /// <summary>
        /// 物料ID
        /// </summary>
        public int FMaterialId { get; set; }
        /// <summary>
        /// 组织ID
        /// </summary>
        public int FOrgId { get; set; }
        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgNum { get; set; } = string.Empty; 
        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrGName { get; set; } = string.Empty;
        /// <summary>
        /// 仓库ID
        /// </summary>
        public int StoId { get; set; }
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string StoNum { get; set; } = string.Empty;
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string StoName { get; set; } = string.Empty;
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialNum { get; set; } = string.Empty;
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;
        /// <summary>
        /// 库存数量
        /// </summary>
        public int FBaseQty { get; set; }

        /// <summary>
        /// 库存锁库量
        /// </summary>
        public int FLockQty { get; set; }
        /// <summary>
        /// 单位编码
        /// </summary>
        public string UnitNum { get; set; } = string.Empty;
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UNitName { get; set; } = string.Empty;
        /// <summary>
        /// 是否为外放仓库
        /// </summary>
        public string FIsOutSourceStock { get; set; } = string.Empty;

        /// <summary>
        /// 此物料总待出库数量
        /// </summary>
        public decimal UnQtyShipdSum { get; set; }

        /// <summary>
        /// 实际可用库存
        /// </summary>
        public decimal UsableQty { get; set; }

        /// <summary>
        /// 采购在途量
        /// </summary>
        public decimal OnOrderQTY { get; set; }

        /// <summary>
        /// 品检库存
        /// </summary>
        public decimal QtyInsp { get; set; }

        /// <summary>
        /// 发货地
        /// </summary>
        public string FOutSourceStockLoc { get; set; } = string.Empty;
    }

    //public class KeyValue<T1, T2>
    //{
    //    public required T1 Key { get; set; }
    //    public  T2? Value { get; set; }
    //}
}
