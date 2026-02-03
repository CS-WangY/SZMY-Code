using mymooo.core.Attributes.Redis;

namespace mymooo.weixinWork.SDK.AddressBook.Model
{
	/// <summary>
	/// 父级用户信息
	/// </summary>
	[RedisKey("mymooo-weixin-AddressBook", 14)]
	public class ParentUserInfo
	{
		/// <summary>
		/// 用户Id
		/// </summary>
		public long UserId { get; set; }

		/// <summary>
		/// 用户编码
		/// </summary>
		[RedisPrimaryKey]
		public string UserCode { get; set; } = string.Empty;

		/// <summary>
		/// 用户名称
		/// </summary>
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// 父级用户Id
		/// </summary>
		public long ParentUserId { get; set; }

		/// <summary>
		/// 父级用户编码
		/// </summary>
		public string ParentUserCode { get; set; } = string.Empty;

		/// <summary>
		/// 第几级部门领导
		/// </summary>
		[RedisMainField(1, "parent-user")]
		public int RowIndex { get; set; }

		/// <summary>
		/// 父级用户名称
		/// </summary>
		public string ParentUserName { get; set; } = string.Empty;

		/// <summary>
		/// 部门Id
		/// </summary>
		public long DepartmentId { get; set; }

		/// <summary>
		/// 部门名称
		/// </summary>
		public string DepartmentName { get; set; } = string.Empty;

		/// <summary>
		/// 父级部门Id
		/// </summary>
		public long ParentDepartmentId { get; set; }
	}
}
