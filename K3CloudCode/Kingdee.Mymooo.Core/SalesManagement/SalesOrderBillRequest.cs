using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
	public class SalesOrderBillRequest
	{
		public long SaleBillId { get; set; }
		public long SalesOrderType { get; set; }
		public string CustPurchaseNo { get; set; }
		public string SalesOrderNo { get; set; }

		public long OrgId { get; set; }

		/// <summary>
		/// 组织编码
		/// </summary>
		[JsonProperty("CompanyCode")]
		public string OrgNumber { get; set; }

		public string OrgName { get; set; }

		public decimal SalesOrderVatTotal { get; set; }
		public decimal SalesOrderTotal { get; set; }
		public decimal KnockOff { get; set; }
		public object Discount { get; set; }
		public decimal Shipping { get; set; }
		public decimal PayHandFee { get; set; }
		public decimal VatTotal { get; set; }
		public bool IsUnionPay { get; set; }
		public decimal SalesOrderVatNet { get; set; }
		public decimal ShippingDiscountPrice { get; set; }
		public bool FreightToBeCollected { get; set; }
		public DateTime SalesOrderDate { get; set; }
		public bool IsDirectDelivery { get; set; }
		public Deliveryinfo DeliveryInfo { get; set; }
		public Salesinfo SalesInfo { get; set; }
		public Customerinfo CustomerInfo { get; set; }
		public Currencyinfo CurrencyInfo { get; set; }
		public Payterminfo PayTermInfo { get; set; }
		public int PayStatus { get; set; }
		public string PayType { get; set; }
		public decimal? PaidAmount { get; set; }
		/// <summary>
		/// 创建人
		/// </summary>
		public string FPlatCreatorId { get; set; }
		/// <summary>
		/// 平台制单人微信Code
		/// </summary>
		public string FPlatCreatorWXCode { get; set; }
		/// <summary>
		/// 创建类型(0:我方，1:客户方)
		/// </summary>
		public string FPlatCreatorType { get; set; }
		public DateTime? PaidOn { get; set; }
		public Vatinfo VatInfo { get; set; }
		public Invoiceinfo InvoiceInfo { get; set; }
		public Salesorderdetaillist[] SalesOrderDetailList { get; set; }
		public SalesOrderPlanEntry[] PlanentryList { get; set; }

		/// <summary>
		/// 是否期初单据
		/// </summary>
		public bool IsInit { get; set; } = false;
		/// <summary>
		/// 批核日期
		/// </summary>
		public DateTime? AuditTime { get; set; }

		public class Deliveryinfo
		{
			public long DeliveryAddressId { get; set; }
			public string ConsigneeName { get; set; }
			public string ConsigneePhone { get; set; }
			public string DeliveryAddress1 { get; set; }
			public string DeliveryAddress2 { get; set; }
			public string DeliveryAddress3 { get; set; }
			public string DeliveryAddress4 { get; set; }
		}

		public class Salesinfo
		{
			public long Id { get; set; }
			public string Code { get; set; }
			public string Name { get; set; }
			public string Phone { get; set; }
		}

		public class Customerinfo
		{
			public long Id { get; set; }
			public string Code { get; set; }
			public string CompanyStruID { get; set; }
			public string NameEn { get; set; }
			public string Name { get; set; }
			public string AddressEn { get; set; }
			public string Address { get; set; }
			public string Contact { get; set; }
			public string Title { get; set; }
			public string Mobile { get; set; }
			public string Email { get; set; }
			public int? StatementDay { get; set; }
			public int? CollectionDay { get; set; }
			public int? InvoiceDay { get; set; }
			public string StatementContact { get; set; }
			public string StatementPhone { get; set; }
			public int DecimalPlacesOfUnitPrice { get; set; }
		}

		public class Currencyinfo
		{
			public string Code { get; set; }
			public string NameEn { get; set; }
			public string Name { get; set; }
			public int ExchangeRate { get; set; }
		}

		public class Payterminfo
		{
			public string Code { get; set; }
			public int LeadDay { get; set; }
			public string Desc { get; set; }
			public long Id { get; set; }
		}

		public class Vatinfo
		{
			public decimal VatRate { get; set; }
		}

		public class Invoiceinfo
		{
			public string PayCustNode { get; set; }
			public string PayCustName { get; set; }
			public int InvoiceType { get; set; }
			public string InvoiceTitle { get; set; }
			public string InvoiceTel { get; set; }
			public string InvoiceAddress { get; set; }
			public string TaxCode { get; set; }
			public string BankName { get; set; }
			public string BankAccount { get; set; }
			public string InvoiceConsigneeName { get; set; }
			public string InvoiceConsigneePhone { get; set; }
			public string InvoiceConsigneeAddress1 { get; set; }
			public string InvoiceConsigneeAddress2 { get; set; }
			public string InvoiceConsigneeAddress3 { get; set; }
			public string InvoiceConsigneeAddress4 { get; set; }
		}

		public class Salesorderdetaillist
		{
			public long FbdDetId { get; set; }
			public string SupplierName { get; set; }
			public string SupplierCode { get; set; }
			public decimal SupplierUnitPrice { get; set; }
			public bool SupplierUnitPriceWhetherIncludeTax { get; set; }
			public string SupplierUnitPriceSource { get; set; }
			public DateTime CostPriceUpdateDate { get; set; } = DateTime.Now;
			public Productsmallclass ProductSmallClass { get; set; }
			public string Id { get; set; }
			public string ItemNo { get; set; }
			public long MaterialId { get; set; }
			public long MaterialMasterId { get; set; }
			public string MaterialMapId { get; set; }

			public string CustItemNo { get; set; }
			public string ShortNumber { get; set; }
			public string PriceType { get; set; }
			public string CustItemName { get; set; }
			public string ProjectNo { get; set; }
			public int ProductId { get; set; }
			public string ItemName { get; set; }
			public int Qty { get; set; }
			public decimal UnitPrice { get; set; }
			public decimal SubTotal { get; set; }
			public decimal TaxSubTotal { get; set; }
			public long ProductEngineerId { get; set; }
			public string ProductEngineerName { get; set; }
			public string ProductEngineerCode { get; set; }
			public long ProductManagerId { get; set; }
			public string ProductManagerCode { get; set; }
			public string ProductManagerName { get; set; }
			public decimal VatPrice { get; set; }
			public DateTime DeliveryDate { get; set; }
			public string Remark { get; set; }

			/// <summary>
			/// 库位、工厂
			/// </summary>
			public string LocFactory { get; set; }
			public string ItemBrand { get; set; }
			public object[] Children { get; set; }
			public decimal Price { get; set; }

			/// <summary>
			/// 采购备注
			/// </summary>
			public string InsideRemark { get; set; }

			/// <summary>
			/// 客户erp物料编码
			/// </summary>
			public string CustMaterialNo { get; set; }

			/// <summary>
			/// 库存管理特征
			/// </summary>
			public string StockFeatures { get; set; }

			/// <summary>
			/// 供应商规格型号
			/// </summary>
			public string SupplierProductCode { get; set; }
			/// <summary>
			/// 收款计划
			/// </summary>
			public List<PlanEntry> planentry { get; set; }

			/// <summary>
			/// 供货组织
			/// </summary>
			public long SupplyOrgId { get; set; }

			/// <summary>
			/// 事业部
			/// </summary>
			public string BusinessDivisionId { get; set; }

			/// <summary>
			/// 报价单单号
			/// </summary>
			public string InquiryOrder { get; set; }

			/// <summary>
			/// 行号
			/// </summary>
			public int InquiryOrderLineNo { get; set; }

			/// <summary>
			/// 图纸ID
			/// </summary>
			public int DrawingRecordId { get; set; }

			/// <summary>
			/// 是否有工序（表处/热处理）
			/// </summary>
			public bool IsProcess { get; set; } = false;

			/// <summary>
			/// 长
			/// </summary>
			public decimal Length { get; set; } = 0;

			/// <summary>
			/// 宽
			/// </summary>
			public decimal Width { get; set; } = 0;

			/// <summary>
			/// 高
			/// </summary>
			public decimal Height { get; set; } = 0;

			/// <summary>
			/// 净重
			/// </summary>
			public decimal Weight { get; set; } = 0;

			/// <summary>
			/// 材质
			/// </summary>
			public string Textures { get; set; } = "";

			/// <summary>
			/// 体积
			/// </summary>
			public decimal Volume { get; set; } = 0;
			/// <summary>
			/// 重量单位
			/// </summary>
			public string WeightUnitid { get; set; }
			/// <summary>
			/// 尺寸单位
			/// </summary>
			public string VolumeUnitid { get; set; }

			/// <summary>
			/// 非标原材料信息
			/// </summary>
			public List<FBBomInfoEntity> FBBomInfo { get; set; } = new List<FBBomInfoEntity>();
			/// <summary>
			/// 先导订单标识
			/// </summary>
			public long CustOrderDetailId { get; set; }

			public string MachineName { get; set; }
			public bool isAplAccept { get; set; }

		}

		/// <summary>
		/// 非标原材料信息
		/// </summary>
		public class FBBomInfoEntity
		{
			/// <summary>
			/// 坯料编码
			///半成品-W-1-1
			///原材料-W-2-1
			/// </summary>
			public string Specification { get; set; }

			/// <summary>
			/// 坯料名称/长x宽x高
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// 重量
			/// </summary>
			public double Weight { get; set; }

			/// <summary>
			/// 长
			/// </summary>
			public double Length { get; set; }

			/// <summary>
			/// 宽
			/// </summary>
			public double Width { get; set; }

			/// <summary>
			/// 高
			/// </summary>
			public double Height { get; set; }

			/// <summary>
			/// 上级坯料规格(编码)
			/// </summary>
			public string ParentSpecification { get; set; }
			/// <summary>
			/// 重量单位
			/// </summary>
			public string WeightUnitid { get; set; }
			/// <summary>
			/// 尺寸单位
			/// </summary>
			public string VolumeUnitid { get; set; }

		}


		public class Productsmallclass
		{
			public long Id { get; set; }
			public string Code { get; set; }
			public string Name { get; set; }
			public long ParentId { get; set; }
			public string ParentCode { get; set; }
			public string ParentName { get; set; }
			public bool IsPublish { get; set; }
			/// <summary>
			/// 事业部ID
			/// </summary>
			public string BusinessDivisionId { get; set; }
			/// <summary>
			/// 事业部编码
			/// </summary>
			public string BusinessDivisionNumber { get; set; }
			/// <summary>
			/// 事业部名称
			/// </summary>
			public string BusinessDivisionName { get; set; }
			/// <summary>
			/// 供货组织ID
			/// </summary>
			public long SupplyOrgId { get; set; } = 0;
			/// <summary>
			/// 供货组织编码
			/// </summary>
			public string SupplyOrgNumber { get; set; }
			/// <summary>
			/// 供货组织名称
			/// </summary>
			public string SupplyOrgName { get; set; }

			/// <summary>
			/// 供货组织对应事业部集合
			/// </summary>
			public List<SupplyOrgList> SupplyOrgs { get; set; }
		}

		/// <summary>
		/// 供货组织对应事业部集合
		/// </summary>
		public class SupplyOrgList
		{
			/// <summary>
			/// 事业部ID
			/// </summary>
			public string BusinessDivisionId { get; set; }
			/// <summary>
			/// 事业部编码
			/// </summary>
			public string BusinessDivisionNumber { get; set; }

			/// <summary>
			/// 事业部名称
			/// </summary>
			public string BusinessDivisionName { get; set; }
			/// <summary>
			/// 供货组织ID
			/// </summary>
			public long SupplyOrgId { get; set; }
			/// <summary>
			/// 供货组织编码
			/// </summary>
			public string SupplyOrgNumber { get; set; }
			/// <summary>
			/// 供货组织名称
			/// </summary>
			public string SupplyOrgName { get; set; }
			/// <summary>
			/// 是否默认
			/// </summary>
			public bool IsDefault { get; set; }
		}


		public class SalesOrderPlanEntry
		{
			/// <summary>
			/// 匹配核销ID
			/// </summary>
			public long FMMEID { get; set; }
			/// <summary>
			/// 源单编码
			/// </summary>
			public string AdvanceNo { get; set; }
			/// <summary>
			/// 源单ID
			/// </summary>
			public long ADVANCEID { get; set; }
			/// <summary>
			/// 源单序号
			/// </summary>
			public int AdvanceSeq { get; set; }
			/// <summary>
			/// 分录ID
			/// </summary>
			public long ADVANCEENTRYID { get; set; }
			/// <summary>
			/// 关联金额
			/// </summary>
			public decimal ActRecAmount { get; set; }
			/// <summary>
			/// 应收金额
			/// </summary>
			public decimal RemainAmount { get; set; }
			/// <summary>
			/// 未知
			/// </summary>
			public decimal PREMATCHAMOUNTFOR { get; set; }
			/// <summary>
			/// 结算组织ID
			/// </summary>
			public long SettleOrgId_Id { get; set; }
			/// <summary>
			/// 结算组织
			/// </summary>
			public string SettleOrgId { get; set; }
			/// <summary>
			/// 序号
			/// </summary>
			public int Seq { get; set; }
			/// <summary>
			/// 是否在线支付
			/// </summary>
			public bool FIsPayOnline { get; set; }
			/// <summary>
			/// 支付方式
			/// </summary>
			public string FSettleMode_PENY { get; set; }
		}
	}

	/// <summary>
	/// 关闭销售订单
	/// </summary>
	public class CloseSalesOrderRequest
	{
		/// <summary>
		/// 销售单号
		/// </summary>
		public string SalesOrderNo { get; set; }
		/// <summary>
		/// 明细ID
		/// </summary>
		public List<CloseSalesOrderDetail> Det { get; set; }

	}


	/// <summary>
	/// 关闭销售订单
	/// </summary>
	public class CloseSalesOrderDetail
	{
		public long Id { get; set; }

	}
	/// <summary>
	/// 关闭销售订单
	/// </summary>
	public class CloseSalesOrderDetailV2
	{
		public long Id { get; set; }

		public long OrderEntryId { get; set; }

		public int Seq { get; set; }

	}

	/// <summary>
	/// 取消销售订单
	/// </summary>
	public class CancelSalesOrderRequest
	{
		/// <summary>
		/// 销售单号
		/// </summary>
		public string SalesOrderNo { get; set; }
		/// <summary>
		/// 是否取消(false：只是校验，true：取消订单)
		/// </summary>
		public bool IsCance { get; set; } = false;
	}


	/// <summary>
	/// 请求是否可以变更销售订单
	/// </summary>
	public class IsCanUpdateSalesOrderRequest
	{
		/// <summary>
		/// 销售单号
		/// </summary>
		public string SalesOrderNo { get; set; }
		/// <summary>
		/// 销售订单明细
		/// </summary>
		public List<SalesOrderDetEntity> SalesOrderDetailList { get; set; }

		/// <summary>
		/// 变更列表
		/// </summary>
		public class SalesOrderChangeDetEntity
		{
			/// <summary>
			/// 销售单号
			/// </summary>
			public string SalesOrderNo { get; set; }
			/// <summary>
			/// 销售单序号
			/// </summary>
			public decimal SoSeqNo { get; set; }
			public string ItemNo { get; set; }
			public string ToItemNo { get; set; }
			public decimal FrmQty { get; set; }
			public decimal ToQty { get; set; }
			public DateTime FrmEtd { get; set; }
			public DateTime ToEtd { get; set; }
			public decimal FrmUp { get; set; }
			public decimal ToUp { get; set; }
			public decimal FrmUpVat { get; set; }
			public decimal ToUpVat { get; set; }
			/// <summary>
			/// 销售费用
			/// </summary>
			public decimal AdditionalCost { get; set; }
		}

	}

	/// <summary>
	/// 销售明细
	/// </summary>
	public class SalesOrderDetEntity
	{
		public SalesOrderDetEntity()
		{
			Children = new List<SalesOrderDetEntity>();
		}

		/// <summary>
		/// 销售单明细ID
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// 序号
		/// </summary>
		public int Seq { get; set; }

		/// <summary>
		/// 物料编号
		/// </summary>
		public string ItemNo { get; set; }

		/// <summary>
		/// 销售数量
		/// </summary>
		public decimal Qty { get; set; }

		/// <summary>
		/// 销售单价
		/// </summary>
		public decimal UnitPrice { get; set; }

		/// <summary>
		/// 含税单价
		/// </summary>
		public decimal VatPrice { get; set; }

		/// <summary>
		/// 要求送货日期
		/// </summary>
		private DateTime? _deliveryDate;
		/// <summary>
		/// 要求送货日期
		/// </summary>
		public DateTime DeliveryDate
		{
			get { return _deliveryDate ?? DateTime.Now; }
			set { _deliveryDate = value; }
		}


		public List<SalesOrderDetEntity> Children { get; set; }

		/// <summary>
		/// 销售费用
		/// </summary>
		public decimal AdditionalCost { get; set; }

	}

	/// <summary>
	/// 请求获取销售订单成本价列表
	/// </summary>
	public class SalesOrderCostPriceFilter
	{
		/// <summary>
		/// 账套
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// 订单类型(SalesOrder：销售订单/PlnReqOrder：组织间需求单)
		/// </summary>
		public string OrderType { get; set; }

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustName { get; set; }
		/// <summary>
		/// 物料编号
		/// </summary>
		public string ProductModel { get; set; }
		/// <summary>
		///物料名称
		/// </summary>
		public string ProductName { get; set; }
		/// <summary>
		/// 小类名称
		/// </summary>
		public string SmallClass { get; set; }
		/// <summary>
		/// 产品工程师名称
		/// </summary>
		public string ProductEngineer { get; set; }
		/// <summary>
		/// 更新人
		/// </summary>
		public string UpdateUser { get; set; }
		/// <summary>
		/// 业务员名称
		/// </summary>
		public string Inquirer { get; set; }
		/// <summary>
		/// 订单开始时间
		/// </summary>
		public DateTime? OrderStartDate { get; set; }
		/// <summary>
		/// 订单结束时间
		/// </summary>
		public DateTime? OrderEndDate { get; set; }
		/// <summary>
		/// 是否已填写成本价
		/// </summary>
		public bool? HasCostPrice { get; set; }

		/// <summary>
		/// 有权限的小类
		/// </summary>
		public string AuthoritySmallClass { get; set; }

		/// <summary>
		/// 是否内部客户
		/// </summary>

		public bool? IsInsideCust { get; set; }
		/// <summary>
		/// 是否无供应商
		/// </summary>
		public bool? IsNoSupplier { get; set; }
		/// <summary>
		/// 供应商名称
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// 供货组织Id
		/// </summary>
		public long SupplyOrgId { get; set; } = 0;
	}

	public class SalesOrderCostPriceEntity
	{

		/// <summary>
		/// 订单类型(SalesOrder：销售订单/PlnReqOrder：组织间需求单)
		/// </summary>
		public string OrderType { get; set; }
		public string CompanyCode { get; set; }
		public string CompanyName { get; set; }
		public string CustCode { get; set; }
		public string CustName { get; set; }
		public string SmallClassId { get; set; }
		public string SmallClass { get; set; }
		public string ProductEngineer { get; set; }
		public string CustProductModel { get; set; }
		public string CustProductName { get; set; }
		public string MymoooProductModel { get; set; }
		public string MymoooProductName { get; set; }
		/// <summary>
		/// 含税单价
		/// </summary>
		public decimal UnitPrice { get; set; }
		public int Count { get; set; }

		/// <summary>
		/// 供应商编号
		/// </summary>
		public string SupplierCode { get; set; }

		/// <summary>
		/// 供应商名称
		/// </summary>
		public string SupplierName { get; set; }
		/// <summary>
		/// 供应商ID
		/// </summary>
		public long SupplierId { get; set; }
		/// <summary>
		/// 供应商价格
		/// </summary>
		public decimal SupplierUnitPrice { get; set; }
		/// <summary>
		/// 供应商单价是否含税
		/// </summary>
		public bool SupplierUnitPriceWhetherIncludeTax { get; set; }
		/// <summary>
		/// 供应商单价来源 M-手工填写 P-价目表 S- scm填写
		/// </summary>
		public string SupplierUnitPriceSource { get; set; }
		/// <summary>
		/// 销售订单编号
		/// </summary>
		public string SalesOrderNumber { get; set; }
		/// <summary>
		/// 销售订单序号
		/// </summary>
		public string SalesOrderSeqNumber { get; set; }
		/// <summary>
		/// 业务员名称
		/// </summary>
		public string Inquirer { get; set; }
		public string OrderDate { get; set; }
		public string DeliveryDate { get; set; }
		/// <summary>
		/// 修改日期，精确到时分秒-与绩效有关
		/// </summary>
		public string UpdateDate { get; set; }
		public string UpdateUser { get; set; }
		/// <summary>
		/// 蚂蚁平台订单明细id
		/// </summary>
		public long FbdDetId { get; set; }

		/// <summary>
		/// 产品id
		/// </summary>
		public long ProductId { get; set; }

		/// <summary>
		/// 是否内部客户
		/// </summary>

		public string IsInsideCust { get; set; }
		/// <summary>
		/// 供货组织编码
		/// </summary>
		public string SupplyOrgCode { get; set; }
		/// <summary>
		/// 供货组织名称
		/// </summary>
		public string SupplyOrgName { get; set; }
	}

	public class OrderGrossProfitEntityList
	{
		public List<OrderGrossProfitEntity> OrderGrossProfitEntity { get; set; }
	}

	public class OrderGrossProfitEntity
	{
		/// <summary>
		/// 订单号
		/// </summary>
		public string SoNo { get; set; }

		/// <summary>
		/// 订单序号
		/// </summary>
		public string SeqNo { get; set; }

		/// <summary>
		/// 产品经理
		/// </summary>
		public string ProductManager { get; set; }

		/// <summary>
		/// 产品大类
		/// </summary>
		public string ParentSmallName { get; set; }

		/// <summary>
		/// 产品小类
		/// </summary>
		public string SmallClass { get; set; }

		/// <summary>
		/// 产品名称
		/// </summary>
		public string ItemDesc { get; set; }

		/// <summary>
		/// 产品型号
		/// </summary>
		public string ItemNo { get; set; }

		/// <summary>
		/// 销售单价
		/// </summary>
		public string UnitPrice { get; set; }

		/// <summary>
		/// 产品数量
		/// </summary>
		public string ProductNum { get; set; }

		/// <summary>
		/// 销售额
		/// </summary>
		public string SoVatTtl { get; set; }

		/// <summary>
		/// 成本价
		/// </summary>
		public string PoVatTtl { get; set; }

		/// <summary>
		/// 成本供应商
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// 成本
		/// </summary>
		public string TotalPoVatTtl { get; set; }

		/// <summary>
		/// 系统计算成本
		/// </summary>
		public string SystemPoVatTtl { get; set; }

		/// <summary>
		/// 最近采购价供应商
		/// </summary>
		public string Vdr_Namec { get; set; }

		/// <summary>
		/// 毛利
		/// </summary>
		public string VatProfit { get; set; }

		/// <summary>
		/// 毛利率百分比
		/// </summary>
		public string VatProfitRate { get; set; }

		/// <summary>
		/// 订单日期
		/// </summary>
		public string SoDate { get; set; }

		public string CompanyCode { get; set; }

		/// <summary>
		/// 组织名称
		/// </summary>
		public string CompanyName { get; set; }

		/// <summary>
		/// 事业部ID
		/// </summary>
		public string BusinessDivisionId { get; set; }

		/// <summary>
		/// 事业部名称
		/// </summary>
		public string BusinessDivisionName { get; set; }
	}

	/// <summary>
	/// 请求销售订单预估毛利汇总
	/// </summary>
	public class OrderGrossProfitFilter
	{

		/// <summary>
		/// 订单号
		/// </summary>
		public string SoNo { get; set; }

		/// <summary>
		/// 订单序号
		/// </summary>
		public string SeqNo { get; set; }
		/// <summary>
		/// 产品经理
		/// </summary>
		public string ProductManager { get; set; }

		/// <summary>
		/// 产品大类
		/// </summary>
		public string ParentSmallName { get; set; }

		/// <summary>
		/// 产品小类
		/// </summary>
		public string SmallClass { get; set; }

		/// <summary>
		/// 产品型号
		/// </summary>
		public string ItemNo { get; set; }

		/// <summary>
		/// 产品名称
		/// </summary>
		public string ItemDesc { get; set; }

		/// <summary>
		/// 订单日期
		/// </summary>
		public DateTime? OrderStartDate { get; set; }

		/// <summary>
		/// 订单日期
		/// </summary>
		public DateTime? OrderEndDate { get; set; }

		/// <summary>
		/// 组织
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// 组织名称
		/// </summary>
		public string CompanyName { get; set; }

		/// <summary>
		/// 事业部ID
		/// </summary>
		public string BusinessDivisionId { get; set; }

		/// <summary>
		/// 事业部名称
		/// </summary>
		public string BusinessDivisionName { get; set; }

		/// <summary>
		/// 小类权限过滤
		/// </summary>
		public string SmallClassFilter { get; set; }

		/// <summary>
		/// 汇总依据
		/// </summary>
		public string SummaryBasis { get; set; }

		/// <summary>
		/// 是否是页面跳转
		/// </summary>
		public bool IsJump { get; set; }

		/// <summary>
		/// 是否异常数据
		/// </summary>
		public bool? HasExcept { get; set; }

		/// <summary>
		/// 是否内部客户
		/// </summary>

		public bool? IsInsideCust { get; set; }

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustName { get; set; }

		/// <summary>
		/// 成本毛利润低于
		/// </summary>
		public string VatProfitRateLT { get; set; }

		/// <summary>
		/// 系统计算成本毛利润低于
		/// </summary>
		public string SystemCalculateProfitRateLT { get; set; }

		/// <summary>
		/// 查询年份
		/// </summary>
		public int Year { get; set; }

		/// <summary>
		/// 查询月份
		/// </summary>
		public int Month { get; set; }

		/// <summary>
		/// 销售单价是否为0,分开统计
		/// </summary>
		public bool IsVatPriceZero { get; set; }
	}
	/// <summary>
	/// 查询订单毛利成本分析数据
	/// </summary>
	public class OrderGrossProfitAnalysisEntity
	{
		/// <summary>
		/// 客户编码
		/// </summary>
		public string CustCode { get; set; }

		/// <summary>
		/// 订单号
		/// </summary>
		public string SoNo { get; set; }
		/// <summary>
		/// 订单序号
		/// </summary>
		public string SeqNo { get; set; }
		/// <summary>
		/// 订单日期
		/// </summary>
		public string SoDate { get; set; }

		/// <summary>
		/// 销售员
		/// </summary>
		public string SalesMan { get; set; }

		/// <summary>
		/// 小类
		/// </summary>
		public string SmallClass { get; set; }

		/// <summary>
		/// 产品工程师
		/// </summary>
		public string ProductEngineer { get; set; }

		/// <summary>
		/// 蚂蚁型号
		/// </summary>
		public string MymoooProductModel { get; set; }

		/// <summary>
		/// 蚂蚁产品名称
		/// </summary>
		public string MymoooProductName { get; set; }
		/// <summary>
		/// 销售单价
		/// </summary>
		public string VatPrice { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		public string Count { get; set; }

		/// <summary>
		/// 成本供应商
		/// </summary>
		public string SupplierName { get; set; }
		/// <summary>
		/// 成本单价
		/// </summary>
		public string PoVatTtl { get; set; }
		/// <summary>
		/// 供应商/组合单价
		/// </summary>
		public string Vdr_Namec { get; set; }
		/// <summary>
		/// 系统计算单价
		/// </summary>
		public string SystemPoVatTtl { get; set; }
		/// <summary>
		/// 系统计算单价原值（不设置小数位）
		/// </summary>
		public string OriginalSystemPoVatTtl { get; set; }
		/// <summary>
		/// 系统计算日期
		/// </summary>
		public string SystemCalculateDate { get; set; }
		/// <summary>
		/// 调整单价
		/// </summary>
		public string AdjustPrice { get; set; }
		/// <summary>
		/// 更新差额
		/// </summary>
		public string UpdateBalance { get; set; }

		/// <summary>
		/// 差异原因
		/// </summary>
		public string DifferReason { get; set; }

		/// <summary>
		/// 组织编码
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// 事业部ID
		/// </summary>
		public string BusinessDivisionId { get; set; }

		/// <summary>
		/// 销售额
		/// </summary>
		public string SoVatTtl { get; set; }

		/// <summary>
		/// 成本
		/// </summary>
		public string TotalPoVatTtl { get; set; }

		/// <summary>
		/// 成本毛利额
		/// </summary>
		public string VatProfit { get; set; }

		/// <summary>
		/// 成本毛利率
		/// </summary>
		public string VatProfitRate { get; set; }

		/// <summary>
		/// 系统计算毛利额
		/// </summary>
		public string SystemCalculateProfit { get; set; }

		/// <summary>
		/// 系统计算毛利率
		/// </summary>
		public string SystemCalculateProfitRate { get; set; }
	}

	/// <summary>
	/// 批量更新订单毛利差异的成本
	/// </summary>
	public class OrderGrossProfitExceptEntityList
	{
		public List<OrderGrossProfitExceptEntity> OrderGrossProfitExceptEntity { get; set; }
	}
	/// <summary>
	/// 批量更新订单毛利差异的成本
	/// </summary>
	public class OrderGrossProfitExceptEntity
	{
		/// <summary>
		/// 客户编码
		/// </summary>
		public string CustCode { get; set; }

		/// <summary>
		/// 订单号
		/// </summary>
		public string SoNo { get; set; }

		/// <summary>
		/// 订单序号
		/// </summary>
		public string SeqNo { get; set; }
		/// <summary>
		/// 订单日期
		/// </summary>
		public string SoDate { get; set; }

		/// <summary>
		/// 销售员
		/// </summary>
		public string SalesMan { get; set; }

		/// <summary>
		/// 小类
		/// </summary>
		public string SmallClass { get; set; }

		/// <summary>
		/// 产品工程师
		/// </summary>
		public string ProductEngineer { get; set; }

		/// <summary>
		/// 蚂蚁型号
		/// </summary>
		public string MymoooProductModel { get; set; }

		/// <summary>
		/// 蚂蚁产品名称
		/// </summary>
		public string MymoooProductName { get; set; }
		/// <summary>
		/// 销售单价
		/// </summary>
		public string VatPrice { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		public string Count { get; set; }

		/// <summary>
		/// 成本供应商
		/// </summary>
		public string SupplierName { get; set; }
		/// <summary>
		/// 成本单价
		/// </summary>
		public string PoVatTtl { get; set; }
		/// <summary>
		/// 供应商/组合单
		/// </summary>
		public string Vdr_Namec { get; set; }
		/// <summary>
		/// 系统计算单价
		/// </summary>
		public string SystemPoVatTtl { get; set; }

		/// <summary>
		/// 系统计算单价原值（不设置小数位）
		/// </summary>
		public string OriginalSystemPoVatTtl { get; set; }
		/// <summary>
		/// 系统计算日期
		/// </summary>
		public string SystemCalculateDate { get; set; }
		/// <summary>
		/// 调整单价
		/// </summary>
		public string AdjustPrice { get; set; }
		/// <summary>
		/// 更新差额
		/// </summary>
		public string UpdateBalance { get; set; }

		/// <summary>
		/// 差异原因
		/// </summary>
		public string DifferReason { get; set; }

		/// <summary>
		/// 公司Code
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// 事业部ID
		/// </summary>
		public string BusinessDivisionId { get; set; }
	}

	/// <summary>
	/// 获取产品毛利汇总数据
	/// </summary>
	public class ProductGrossEntity
	{

		/// <summary>
		/// 产品大类Id
		/// </summary>
		public string ParentSmallId { get; set; }

		/// <summary>
		/// 产品大类
		/// </summary>
		public string ParentSmallName { get; set; }

		/// <summary>
		/// 产品小类Id
		/// </summary>
		public string SmallClassId { get; set; }

		/// <summary>
		/// 产品小类
		/// </summary>
		public string SmallClass { get; set; }


		/// <summary>
		/// 销售额(含税)
		/// </summary>
		public string SoVatTtl { get; set; }

		/// <summary>
		/// 成本额(含税)
		/// </summary>
		public string TotalPoVatTtl { get; set; }

		/// <summary>
		/// 毛利
		/// </summary>
		public string VatProfit { get; set; }

		/// <summary>
		/// 毛利率
		/// </summary>
		public string VatProfitRate { get; set; }

		/// <summary>
		/// 月份
		/// </summary>
		public string Month { get; set; }

		/// <summary>
		/// 组织编码
		/// </summary>
		public string CompanyCode { get; set; }

		/// <summary>
		/// 组织名称
		/// </summary>
		public string CompanyName { get; set; }

		/// <summary>
		/// 事业部ID
		/// </summary>
		public string BusinessDivisionId { get; set; }

		/// <summary>
		/// 事业部名称
		/// </summary>
		public string BusinessDivisionName { get; set; }
	}

	/// <summary>
	/// 更新发货通知单物流信息
	/// </summary>
	public class UpDnTrackingInfo
	{
		/// <summary>
		/// 物流单号
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// 物流公司
		/// </summary>
		public string TrackingName { get; set; }

		/// <summary>
		/// 物流日期
		/// </summary>
		public DateTime TrackingDate { get; set; }

		/// <summary>
		/// 物流操作人
		/// </summary>
		public string TrackingUser { get; set; }

		/// <summary>
		/// 发货通知单单号
		/// </summary>
		public List<string> BillNo { get; set; } = new List<string>();
	}

	//销售订单物流信息
	public class SoLogisticsInfoEntity
	{
		/// <summary>
		/// 物流单号
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// 物流名称
		/// </summary>
		public string TrackingName { get; set; }
	}


	//更新物流方式变更申请审批
	public class DnLogisticsChangesEntity
	{

		/// <summary>
		/// 审批状态 申请单状态：1-审批中；2-已通过；3-已驳回；4-已撤销；6-通过后撤销；7-已删除；10-已支付
		/// </summary>
		public string SpStatus { get; set; }

		/// <summary>
		/// 完成时间
		/// </summary>
		public DateTime? CompleteTime { get; set; }

		/// <summary>
		/// 审批单号
		/// </summary>
		public string ApprovalNo { get; set; }

		/// <summary>
		/// 审核人
		/// </summary>
		public string AduitUserName { get; set; }
	}
}
