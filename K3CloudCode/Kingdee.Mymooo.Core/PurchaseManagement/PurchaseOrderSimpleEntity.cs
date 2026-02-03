using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    /// <summary>
    /// 获取产品采购记录
    /// </summary>
    public class PurchaseOrderSimpleEntity
    {
        public string ProductModel { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string ApplyTime { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDay { get; set; }
    }

    /// <summary>
    /// 获取产品采购记录
    /// </summary>
    public class PurchaseOrderDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SmallClassId { get; set; }
        public string SmallClassName { get; set; }

        public string PurchaseOrderNumber { get; set; }
        public string PurchaseDate { get; set; }
        public long ProductId { get; set; }
        public string ProductModel { get; set; }
        public string ProductName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public string CustomerModel { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        public int DeliveryDay { get; set; }
    }

    public class PurchaseProductFilter
    {
        public long? ClassId { get; set; }
        public long? SmallClassId { get; set; }
        public string ProductName { get; set; }
        public string ProductModel { get; set; }
        public DateTime PurchaseDateBegin { get; set; }
        public DateTime PurchaseDateEnd { get; set; }
        public string PurchaseSupplierName { get; set; }
    }
}
