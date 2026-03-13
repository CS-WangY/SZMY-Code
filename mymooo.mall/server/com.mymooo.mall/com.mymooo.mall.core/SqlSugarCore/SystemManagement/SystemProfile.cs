using mymooo.core.Attributes.Redis;
using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SystemManagement
{
	/// <summary>
	/// 系统参数表
	/// </summary>
	[SugarTable("F_SYSPROFILE")]
	[RedisKey("mymooo-management-systemprofile", isSaveMain: false)]
	public class SystemProfile
	{
		/// <summary>
		/// 系统参数标识
		/// </summary>
		[SugarColumn(IsPrimaryKey = true, ColumnName = "FSP_KEY")]
		[RedisMainField(groupId: "system")]
		public string Key { get; set; } = string.Empty;

		/// <summary>
		/// 系统参数值
		/// </summary>
		[RedisValue(mainKey: "system")]
		[SugarColumn(ColumnName = "FSP_VALUE")]
		public string Value { get; set; } = string.Empty;
	}
}
