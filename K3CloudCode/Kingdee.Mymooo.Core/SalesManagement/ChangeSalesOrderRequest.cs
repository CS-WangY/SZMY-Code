using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    public class ChangeSalesOrderRequest
    {
        public string SalesOrderNo { get; set; }

        /// <summary>
        /// 组织编码
        /// </summary>
        [JsonProperty("CompanyCode")]
        public string OrgNumber { get; set; }

        /// <summary>
        /// 是否需要保存
        /// </summary>
        public bool IsAlreadySave { get; set; }

        /// <summary>
        /// 变更人
        /// </summary>
        public string OperatorName { get; set; }
        public string CustPurchaseNo { get; set; }
        public Deliveryinfo DeliveryInfo { get; set; }
        public Invoiceinfo InvoiceInfo { get; set; }
        public decimal SalesOrderTotal { get; set; }
        public decimal SalesOrderVatTotal { get; set; }
        public decimal KnockOff { get; set; }
        public decimal Discount { get; set; }
        public decimal Shipping { get; set; }
        public decimal ShippingDiscountPrice { get; set; }
        public bool FreightToBeCollected { get; set; }
        public DateTime SalesOrderDate { get; set; }
        public Salesorderdetaillist[] SalesOrderDetailList { get; set; }
        public long OrgId { get; set; }
        public SalesOrderBillRequest.Customerinfo CustomerInfo { get; set; }
        
        public bool IsDeliverNotice { get; set; }

        public class Deliveryinfo
        {
            public long DeliveryAddressId { get; set; }
            public string ConsigneeName { get; set; }
            public string ConsigneePhone { get; set; }
            public string DeliveryAddress1 { get; set; }
        }

        public class Invoiceinfo
        {
            public int InvoiceType { get; set; }
            public string InvoiceTitle { get; set; }
            public string InvoiceTel { get; set; }
            public string InvoiceAddress { get; set; }
            public string TaxCode { get; set; }
            public string BankName { get; set; }
            public string BankAccount { get; set; }
        }

        public class Salesorderdetaillist
        {
            public SalesOrderBillRequest.Productsmallclass ProductSmallClass { get; set; }
            public bool IsDelete { get; set; }
            public long Id { get; set; }
            public long OrderEntryId { get; set; }
            public string ShortNumber { get; set; }
            public string PriceType { get; set; }
            public string ItemNo { get; set; }
            public string CustItemNo { get; set; }
            public string CustItemName { get; set; }
            public string ProjectNo { get; set; }
            public string CustMaterialNo { get; set; }
            public string StockFeatures { get; set; }
            public string LocFactory { get; set; }
            public string ItemName { get; set; }
            public decimal Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal SubTotal { get; set; }
            public decimal TaxSubTotal { get; set; }
            public long ProductEngineerId { get; set; }
            public string ProductEngineerName { get; set; }
            public string ProductEngineerCode { get; set; }
            public string ProductManagerCode { get; set; }
            public long ProductManagerId { get; set; }
            public string ProductManagerName { get; set; }
            public decimal VatPrice { get; set; }
            public DateTime DeliveryDate { get; set; }
            public string Remark { get; set; }
            public string ItemBrand { get; set; }
            public int ProductId { get; set; }
            public long MaterialId { get; set; }
            public long MaterialMasterId { get; set; }
            public string MaterialMapId { get; set; }

            /// <summary>
            /// 事业部Id
            /// </summary>
            public string BusinessDivisionId { get; set; }

            /// <summary>
            /// 事业部名称
            /// </summary>
            public string BusinessDivisionName { get; set; }

            /// <summary>
            /// 供应组织Id
            /// </summary>
            public long SupplyOrgId { get; set; }

            /// <summary>
            /// 供货组织名称
            /// </summary>
            public string SupplyOrgName { get; set; }

            public bool IsPurchase { get; set; }
            public bool IsDeliverNotice { get; set; }

            public bool IsReceivable { get; set; }
        }
    }
}
