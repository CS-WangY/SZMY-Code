namespace com.mymooo.mall.core.SqlSugarCore
{
	/// <summary>
	/// 用户状态
	/// </summary>
	public enum UserStatus
	{
		/// <summary>
		/// 启用
		/// </summary>
		D,

		/// <summary>
		/// 禁用
		/// </summary>
		E
	}

	/// <summary>
	/// 管理用户类型
	/// </summary>
	public enum ManagementUserType
	{
		/// <summary>
		/// 平台管理员
		/// </summary>
		A,

		/// <summary>
		/// 厂商管理员
		/// </summary>
		F,

		/// <summary>
		/// 客户管理员
		/// </summary>
		C,

		/// <summary>
		/// 普通用户
		/// </summary>
		N
	}

	/// <summary>
	/// 产品价格类型
	/// </summary>
	public enum ProductPriceType
	{
		/// <summary>
		/// 矩阵
		/// </summary>
		matrix,
		/// <summary>
		/// 全型号
		/// </summary>
		number,
		/// <summary>
		/// 简易型号
		/// </summary>
		shortNumber,
		/// <summary>
		/// 简易型号+矩阵
		/// </summary>
		shortMatrix
	}

	/// <summary>
	/// 数据库分库
	/// </summary>
	public enum DataBaseSplitType
	{
		/// <summary>
		/// 子分类分库, 无联接串, 不允许再分库
		/// </summary>
		S,

		/// <summary>
		/// 本分类分库, 有联接串, 不允许再分库
		/// </summary>
		C,

		/// <summary>
		/// 父分类分库, 有联接串, 不允许再分库
		/// </summary>
		P,

		/// <summary>
		/// 无分库分类, 无联接串, 可允许再分库
		/// </summary>
		N
	}

	/// <summary>
	/// 客户状态枚举
	/// </summary>
	public enum CustomerStatus
	{
		/// <summary>
		/// 待提交
		/// </summary>
		Submitted = 0,
		/// <summary>
		/// 待审核
		/// </summary>
		Audit = 1,
		/// <summary>
		/// 已生效
		/// </summary>
		NonDisable = 2,
		/// <summary>
		/// 禁用
		/// </summary>
		Disable = 3,
		/// <summary>
		/// 驳回
		/// </summary>
		Reject = 4,
		/// <summary>
		/// 注销
		/// </summary>
		Logout = 5
	}

	/// <summary>
	/// 企业类型。
	/// </summary>
	public enum CompanyType
	{
		/// <summary>
		/// 认证企业。
		/// </summary>
		General = 0,

		/// <summary>
		/// 合伙人。
		/// </summary>
		Copartner = 1
	}

	/// <summary>
	/// 客户状态枚举
	/// </summary>
	public enum CustomerAuditStatus
	{
		/// <summary>
		/// 待提交
		/// </summary>
		Submitted = 0,

		/// <summary>
		/// 待审核
		/// </summary>
		Audit = 1,

		/// <summary>
		/// 已审核
		/// </summary>
		AuditSuccess = 2
	}

	/// <summary>
	/// 订单状态枚举
	/// </summary>
	public enum CompanyClientType
	{
		/// <summary>
		/// Web Desktop
		/// </summary>
		WebDesktop = 0,
		/// <summary>
		/// Android App
		/// </summary>
		Android = 1,
		/// <summary>
		/// iOS App
		/// </summary>
		IOS = 2,
		/// <summary>
		/// Web Mobile
		/// </summary>
		WebMobile = 3,
	}
}
