using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    public class RequirementOrderDetailRequest
    {
        public int Seq { get; set; }
        public long MaterialId { get; set; }

        public long ProductId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialNumber { get; set; }
        public string MaterialName { get; set; }
        public string ShortNumber { get; set; }
        public string PriceType { get; set; }

        public decimal Qty { get; set; }

        /// <summary>
        /// 成本价价格来源
        /// </summary>
        public string SupplierUnitPriceSource { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime? CostPriceUpdateDate { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string CostPriceUpdateUser { get; set; }

        /// <summary>
        /// 供应商产品型号
        /// </summary>
        public string SupplierProductCode { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierNumber { get; set; }

        /// <summary>
        /// 产品工程师
        /// </summary>
        public string ProductEngineerNumber { get; set; }

        /// <summary>
        /// 产品经理
        /// </summary>
        public string ProductManagerNumber { get; set; }

        /// <summary>
        /// 供应商单价
        /// </summary>
        public decimal SupplierUnitPrice { get; set; }

        /// <summary>
        /// 交期
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        public decimal Price { get; set; }
        public decimal Amount { get; set; }

        /// <summary>
        /// 事业部
        /// </summary>
        public string BusinessDivisionId { get; set; }
        public string BusinessDivisionNumber { get; set; }
        public string BusinessDivisionName { get; set; }
        public SalesOrderBillRequest.Productsmallclass ProductSmallClass { get; set; }

        /// <summary>
        /// 采购备注
        /// </summary>
        public string InsideRemark { get; set; }

        /// <summary>
        /// 销售备注
        /// </summary>
        public string Remark { get; set; }
    }
}
