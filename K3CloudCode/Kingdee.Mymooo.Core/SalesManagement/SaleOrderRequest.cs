using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.BaseManagement;

namespace Kingdee.Mymooo.Core.SalesManagement
{
	public class SaleOrderRequest
	{
		public string SalesOrderNo { get; set; }
		public string CustomerCode { get; set; }
		public string CustomerName { get; set; }
		public DateTime Date { get; set; }

		public decimal TatolAmount { get; set; }
		public string Contact { get; set; }
		public string Mobile { get; set; }
		public string Address { get; set; }
		public DeliveryInfo Delivery { get; set; }
		public InvoiceInfo Invoice { get; set; }
		public List<SalesOrderDetail> SalesOrderDetails { get; set; }

		public class DeliveryInfo
		{
			public string ConsigneeName { get; set; }
			public string ConsigneePhone { get; set; }
			public string DeliveryAddress { get; set; }
		}

		public class InvoiceInfo
		{
			public int InvoiceType { get; set; }
			public string InvoiceTitle { get; set; }
			public string InvoiceTel { get; set; }
			public string InvoiceAddress { get; set; }
			public string TaxCode { get; set; }
			public string BankName { get; set; }
			public string BankAccount { get; set; }
			public string InvoiceConsigneeName { get; set; }
			public string InvoiceConsigneePhone { get; set; }
		}

		public class SalesOrderDetail
		{
			public long SalesDetailId { get; set; }
			public Productsmallclass ProductSmallClass { get; set; }
			public string ItemNo { get; set; }
			public long ProductId { get; set; }
			public string ItemName { get; set; }
			public decimal Qty { get; set; }
			public decimal Price { get; set; }
			public decimal TaxPrice { get; set; }
			public decimal SubTotal { get; set; }
			public decimal TaxSubTotal { get; set; }
			public DateTime DeliveryDate { get; set; }
			public string Remark { get; set; }
		}

		public class Productsmallclass
		{
			public long Id { get; set; }
			public string Number { get; set; }
			public string Name { get; set; }
			public long ParentId { get; set; }
			public string ParentNumber { get; set; }
			public string ParentName { get; set; }
		}
	}
	public enum SalOrderClosedOperationType
	{
		Bill,
		Entity
	}
	public class K3CloudClosedSalOrderRequest
	{
		public string SpStatus { get; set; }
		public string ApplyeventNo { get; set; }
		/// <summary>
		/// 操作类型
		/// </summary>
		public SalOrderClosedOperationType OperationType { get; set; }
		/// <summary>
		/// 销售订单ID
		/// </summary>
		public long SaleOrderID { get; set; }
		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustName { get; set; } = string.Empty;
		/// <summary>
		/// 取消金额
		/// </summary>
		public decimal ClosedAmount { get; set; }
		/// <summary>
		/// 下单日期
		/// </summary>
		public DateTime PlaceDate { get; set; } = DateTime.Now;
		/// <summary>
		/// 销售订单号
		/// </summary>
		public string BillNo { get; set; } = string.Empty;
		/// <summary>
		/// 产品型号/名称
		/// </summary>
		public string Material { get; set; } = string.Empty;

		/// <summary>
		/// 订单金额
		/// </summary>
		public decimal OrderQty { get; set; }

		/// <summary>
		/// 客户类型
		/// </summary>
		public string CustType { get; set; } = string.Empty;
		/// <summary>
		/// 是否退款客户
		/// </summary>
		public string IsRefund { get; set; } = string.Empty;

		/// <summary>
		/// 产品所属事业部
		/// </summary>
		public string OrgID { get; set; } = string.Empty;
		/// <summary>
		/// 供货组织名称
		/// </summary>
		public string OrgName { get; set; } = string.Empty;
		/// <summary>
		/// 取消订单原因
		/// </summary>
		public string Remarks { get; set; } = string.Empty;
		/// <summary>
		/// 明细ID
		/// </summary>
		public List<long> SaleOrderEntrys { get; set; }
		/// <summary>
		/// 发起人
		/// </summary>
		public string Creator_userid { get; set; }

		/// <summary>
		/// 备注信息，请提供3行。
		/// </summary>
		public string[] SummaryInfo { get; set; }
		public string SendRabbitCode { get; set; }
		public string EnvCode { get; set; }

		/// <summary>
		/// 抄送人
		/// </summary>
		public List<string> Notifyer { get; set; }
	}

	public class SaleOrder2FoPushEntity
	{
		public long SaleOrgId { get; set; }
		public long SaleBillId { get; set; }
		public long SaleEntryId { get; set; }
		public long MaterialId { get; set; }
		public long CustId { get; set; }
		public decimal Qty { get; set; }
	}
	public class resEntryInfo
	{
		/// <summary>
		/// 供给类型
		/// </summary>
		public string SupplyFormID { get; set; }
		/// <summary>
		/// 供给单据内码
		/// </summary>
		public string SupplyInterID { get; set; }
		/// <summary>
		/// 供给单分录内码
		/// </summary>
		public string SupplyEntryID { get; set; }
		/// <summary>
		/// 供给单据号
		/// </summary>
		public string SupplyBillNO { get; set; }
		/// <summary>
		/// 物料编码
		/// </summary>
		public string SupplyMaterialID { get; set; }
		/// <summary>
		/// 供应组织
		/// </summary>
		public string SupplyOrgID { get; set; }
		/// <summary>
		/// 仓库
		/// </summary>
		public string SupplyStockID { get; set; }
		/// <summary>
		/// 基本计量单位
		/// </summary>
		public string BaseSupplyUnitID { get; set; }
		/// <summary>
		/// 基本单位申请数量
		/// </summary>
		public decimal BaseSupplyQty { get; set; }
		/// <summary>
		/// 供给单内码(整形)
		/// </summary>
		public long IntsupplyID { get; set; }
		/// <summary>
		/// 供给单分录内码(整形)
		/// </summary>
		public long IntsupplyEntryID { get; set; }
		/// <summary>
		/// 供给日期
		/// </summary>
		public DateTime SupplyDate { get; set; }
		/// <summary>
		/// 供应优先级
		/// </summary>
		public int Supplypriority { get; set; }
		/// <summary>
		/// 是否MTO
		/// </summary>
		public int Ismto { get; set; }
		/// <summary>
		/// 成品率
		/// </summary>
		public decimal YieldRate { get; set; }
		/// <summary>
		/// 预留产生类型
		/// </summary>
		public int Linktype { get; set; }
		/// <summary>
		/// 内码
		/// </summary>
		public string EntryPkId { get; set; }
		/// <summary>
		/// 消耗优先级
		/// </summary>
		public int Consumpriority { get; set; }
		/// <summary>
		/// 标识号
		/// </summary>
		public string GenerateId { get; set; }
	}
	public class SupplyViewItem
	{
		/// <summary>
		/// 
		/// </summary>
		public string SupplyFormID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyInterID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyEntryId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyBillNO { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyMaterialID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyOrgID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime SupplyDate { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyStockID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyStockLocID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyBomID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyLot_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyLot_Text { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyMtoNO { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyAuxproID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long BaseSupplyUnitID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal BaseActSupplyQty { get; set; }
		public long IntsupplyID { get; set; }
		public long IntsupplyEntryId { get; set; }
		/// <summary>
		/// 供应优先级
		/// </summary>
		public int Supplypriority { get; set; }
		/// <summary>
		/// 是否MTO
		/// </summary>
		public int Ismto { get; set; }
		/// <summary>
		/// 成品率
		/// </summary>
		public decimal YieldRate { get; set; }
		/// <summary>
		/// 预留产生类型
		/// </summary>
		public int Linktype { get; set; }
		/// <summary>
		/// 内码
		/// </summary>
		public string EntryPkId { get; set; }
		/// <summary>
		/// 消耗优先级
		/// </summary>
		public int Consumpriority { get; set; }
		/// <summary>
		/// 标识号
		/// </summary>
		public string GenerateId { get; set; }
	}

	public class DemandView
	{
		/// <summary>
		/// 
		/// </summary>
		public string DemandFormID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string DemandInterID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string DemandEntryID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string DemandBillNO { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandFormId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandInterId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandEntryId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandBillNo { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long DemandOrgID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime DemandDate { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long MaterialID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long BaseUnitID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal BaseQty { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public List<SupplyViewItem> supplyView { get; set; }
	}
}
