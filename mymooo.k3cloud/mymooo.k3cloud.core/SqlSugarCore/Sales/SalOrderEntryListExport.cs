using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.core.Attributes.Office;
using mymooo.core.Attributes.Redis;

namespace mymooo.k3cloud.core.SqlSugarCore.Sales
{
    /// <summary>
    /// 取消销售订单审批附件实体
    /// </summary>
    [ExcelExport(["attachment", "Sales", "取消销售订单附件模板.xlsx"], startRowIndex: 1)]
    public class SalOrderEntryListExport
    {
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 0)]
        public string MaterialNumber { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 1)]
        public string MaterialName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 2)]
        public string Specification { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 3)]
        public string UnitName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 4)]
        public decimal BaseCanOutQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 5)]
        public decimal Taxprice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 6)]
        public DateTime Deliverydate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 7)]
        public string OrgName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 8)]
        public string ParentSmallName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 9)]
        public string SmallName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 10)]
        public string CustItemNo { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 11)]
        public string CustItemName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumn(columnIndex: 12)]
        public string Note { get; set; } = string.Empty;
    }
}
