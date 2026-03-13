using System.ComponentModel;

namespace com.mymooo.mall.core.Model.Quotation
{
	public class VendorPriceInfo
	{
		/// <summary>
		/// 物料编号
		/// </summary>
		public string? ItemCode { get; set; }

		/// <summary>
		/// 供应商编号
		/// </summary>
		public string? VendorCode { get; set; }

		/// <summary>
		/// 供应商交货天数阶梯
		/// </summary>
		public List<VendorDeliveryDaysLadder>? VendorDeliveryDaysLadders { get; set; }

		/// <summary>
		/// 供应商价格阶梯
		/// </summary>
		public List<VendorPriceLadder>? VendorPriceLadders { get; set; }
	}


	/// <summary>
	/// 供应商交货天数阶梯
	/// </summary>
	public class VendorDeliveryDaysLadder
	{

		/// <summary>
		/// 物料编号
		/// </summary>
		public string? ItemCode { get; set; }

		/// <summary>
		/// 供应商编号
		/// </summary>
		public string? VendorCode { get; set; }

		/// <summary>
		/// 数量上限
		/// </summary>
		public int Qty { get; set; }

		/// <summary>
		/// 交货天数
		/// </summary>
		public int DeliveryDays { get; set; }

	}


	/// <summary>
	/// 供应商价格阶梯
	/// </summary>
	public class VendorPriceLadder
	{
		/// <summary>
		/// 物料编号
		/// </summary>
		public string? ItemCode { get; set; }

		/// <summary>
		/// 供应商编号
		/// </summary>
		public string? VendorCode { get; set; }

		/// <summary>
		/// 数量上限
		/// </summary>
		public int Qty { get; set; }

		/// <summary>
		/// 不含税价格<seealso cref="TaxedPrice"/>
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// 含税价<seealso cref="Price"/>
		/// </summary>
		public decimal TaxedPrice { get; set; }
	}

	public enum PriceListDataType
	{
		/// <summary>
		/// 通用
		/// </summary>
		common,
		/// <summary>
		/// 等级
		/// </summary>
		level,
		/// <summary>
		/// 客户
		/// </summary>
		customer,
	}

	public enum PriceListType
	{
		/// <summary>
		/// 线性价目表
		/// </summary>
		linear,
		/// <summary>
		/// 矩阵价目表
		/// </summary>
		matrix,
	}

	public enum PriceSource
	{
		[Description("客户价目表")]
		Customer,
		[Description("客户历史价")]
		history,
        [Description("历史价")]
        fhistory,
        [Description("客户历史价")]
        minhistory,
        [Description("最低历史价")]
        chistory,
        [Description("通用价目表")]
		common,
		[Description("资料工程部导入价目表")]
		import,
		/// <summary>
		/// 产品工程师报价
		/// </summary>
		[Description("产品工程师报价")]
		productManager,
		[Description("业务员报价")]
		salesMan,

		[Description("缓存资料工程部导入价目表")]
		cacheImport,
		[Description("缓存通用价目表")]
		cacheCommon,
		[Description("缓存客户价目表")]
		cacheCustomer,
		[Description("当前客户价目表")]
		currentCustomer,
		[Description("当前通用价目表")]
		currentCommon,
		[Description("客户报价记录")]
		quotaion,
        [Description("报价记录")]
        fquotaion,
        [Description("最低报价记录")]
        minquotaion,
        [Description("--")]
		none
		//[Description("公司等级折扣")]
		//companyGrad,


	}

	public enum DeliverySource
	{
		[Description("客户价目表货期")]
		Customer,
		[Description("历史货期")]
		history,
		[Description("通用价目表货期")]
		Common,
		[Description("资料工程部导入货期")]
		import,
		[Description("可用库存货期")]
		Stock,
		[Description("供应商库存货期")]
		Suplier,
		[Description("产品工程师货期")]
		productManager,
		[Description("缓存通用价目表货期")]
		cacheCommon,
		[Description("缓存客户价目表货期")]
		cacheCustomer,
		[Description("--")]
		none
	}
	public static class EnumExtensions
	{
		public static string GetDescription(this Enum val)
		{
			var field = val.GetType().GetField(val.ToString());
			if (field == null) { return val.ToString(); }
			var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
			return customAttribute == null ? val.ToString() : ((DescriptionAttribute)customAttribute).Description;
		}
	}
}
