using System;
using System.Collections.Generic;

namespace Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled
{
	public class ChangeOrderTaskRequest
	{
		/// <summary>
		/// 订单编号
		/// </summary>
		public string SalesOrderNo { get; set; }

		/// <summary>
		/// 销售订单日期
		/// </summary>
		public DateTime SalesOrderDate { get; set; }

		/// <summary>
		/// 客户编码
		/// </summary>
		public string CustomerNumber { get; set; }

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustomerName { get; set; }

		/// <summary>
		/// 销售组织编码
		/// </summary>
		public string OrganizationNumber { get; set; }

		/// <summary>
		/// 销售组织名称
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// 客户采购单号
		/// </summary>
		public string CustPurchaseNo { get; set; }

		/// <summary>
		/// 整单折扣
		/// </summary>
		public decimal Discount { get; set; }

		/// <summary>
		/// 部门编码
		/// </summary>
		public string DepartmentNumber { get; set; }

		/// <summary>
		/// 部门名称
		/// </summary>
		public string DepartmentName { get; set; }

		/// <summary>
		/// 销售员编码
		/// </summary>
		public string SalesNumber { get; set; }

		/// <summary>
		/// 销售员名称
		/// </summary>
		public string SalesName { get; set; }

		/// <summary>
		/// 关闭状态
		/// </summary>
		public string CloseStatus { get; set; }

		/// <summary>
		/// 取消状态
		/// </summary>
		public string CancelStatus { get; set; }

		/// <summary>
		/// 是否平台下单
		/// </summary>
		public string PlatCreatorType { get; set; }

		/// <summary>
		/// 制单人
		/// </summary>
		public string PlatCreatorName { get; set; }

		/// <summary>
		/// 单据类型 FA  标准件订单   FB  非标订单
		/// </summary>
		public string BillType { get; set; }

		/// <summary>
		/// 审核日期
		/// </summary>
		public DateTime? AuditTime { get; set; }

		/// <summary>
		/// 付款金额
		/// </summary>
		public decimal PaidAmount { get; set; }
		/// <summary>
		/// 原订单价格
		/// </summary>
		public decimal OriginalPaidAmount { get; set; }

		/// <summary>
		/// 明细
		/// </summary>
		public List<ChangeOrderTaskRequestDetails> Details { get; set; }

		/// <summary>
		/// 操作
		/// </summary>
		public string Operation { get; set; }

		/// <summary>
		/// 结算方式编码
		/// </summary>
		public string SettleTypeNumber { get; set; }

		/// <summary>
		/// 结算方式名称
		/// </summary>
		public string SettleTypeName { get; set; }

		public class ChangeOrderTaskRequestDetails
		{
			public long EntryId { get; set; }
			/// <summary>
			/// 序号
			/// </summary>
			public int Seq { get; set; }

			/// <summary>
			/// 产品型号
			/// </summary>
			public string MaterialNumber { get; set; }

			/// <summary>
			/// 产品名称
			/// </summary>
			public string MaterialName { get; set; }

			/// <summary>
			/// 客户产品型号
			/// </summary>
			public string CustomerMaterialNumber { get; set; }

			/// <summary>
			/// 客户产品名称
			/// </summary>
			public string CustomerMaterialName { get; set; }

			/// <summary>
			/// 客户物料号
			/// </summary>
			public string CustomerMaterialNo { get; set; }

			/// <summary>
			/// 库存特征码
			/// </summary>
			public string StockFeatures { get; set; }

			/// <summary>
			/// 库位
			/// </summary>
			public string LocFactory { get; set; }

			/// <summary>
			/// 事业部编码
			/// </summary>
			public string BusinessDivisionNumber { get; set; }

			/// <summary>
			/// 事业部名称
			/// </summary>
			public string BusinessDivisionName { get; set; }
			/// <summary>
			/// 产品小类ID
			/// </summary>
			public long SmallId { get; set; }
			/// <summary>
			/// 产品小类编码
			/// </summary>
			public string SmallNumber { get; set; }

			/// <summary>
			/// 产品小类名称
			/// </summary>
			public string SmallName { get; set; }

			/// <summary>
			/// 产品大类编码
			/// </summary>
			public string ParentSmallNumber { get; set; }

			/// <summary>
			/// 产品大类名称
			/// </summary>
			public string ParentSmallName { get; set; }

			/// <summary>
			/// 终止状态
			/// </summary>
			public string MrpTerminateStatus { get; set; }

			/// <summary>
			/// 行关闭状态
			/// </summary>
			public string MrpCloseStatus { get; set; }

			/// <summary>
			/// 数量
			/// </summary>
			public decimal Qty { get; set; }

			/// <summary>
			/// 出库数量
			/// </summary>
			public decimal NoOutQty { get; set; }

			/// <summary>
			/// 单价
			/// </summary>
			public decimal Price { get; set; }

			/// <summary>
			/// 金额
			/// </summary>
			public decimal Amount { get; set; }

			/// <summary>
			/// 交期
			/// </summary>
			public DateTime DeliveryDate { get; set; }

		}
		public SyncOrderPaymentRequest OrderPayment { get; set; }

	}
	/// <summary>
	/// 同步订单支付状态
	/// </summary>
	public class SyncOrderPaymentRequest
	{
		/// <summary>
		/// 0:微信支付,1:支付宝支付，2：银行转账
		/// </summary>
		public int PayType { get; set; }

		/// <summary>
		/// 支付时间
		/// </summary>
		public DateTime? PaidDateTime { get; set; }

		/// <summary>
		/// 支付总金额
		/// </summary>
		public decimal PaidAmount { get; set; }

		/// <summary>
		/// 销售单号
		/// </summary> 
		public string OrderNumber { get; set; }
	}
	/// <summary>
	/// 同步发货状态
	/// </summary>
	public class SyncOrderDeliveryRequest
	{
		public string BillNo { get; set; }
		public int isCancel { get; set; }
		/// <summary>
		/// 实发数量
		/// </summary>
		public decimal ActualQuantity { get; set; }

		/// <summary>
		/// 发货时间
		/// </summary>
		public DateTime? DeliveryDate { get; set; }


		/// <summary>
		/// 销售订单明细Id
		/// </summary>
		public long DetailId { get; set; }
	}
	/// <summary>
	/// 取消订单
	/// </summary>
	public class SalesOrderCancelRequest
	{
		/// <summary>
		/// 订单Id
		/// </summary>
		public string OrderNumber { get; set; }

		/// <summary>
		/// 取消类型:0--按行取消，1--按单取消
		/// </summary>
		public int CancelType { get; set; }

		/// <summary>
		/// 取消订单明细ID
		/// </summary>
		public List<long> orderDetails { get; set; }
	}
}
