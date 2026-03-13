using SqlSugar;
using System.Text.Json.Serialization;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation
{
	///<summary>
	///用户信息表
	///</summary>
	[SugarTable("F_USER_INFO")]
	public partial class ManagementUserInfo
	{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		public ManagementUserInfo()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

		/// <summary>
		/// Desc:用户KEY
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(IsPrimaryKey = true, ColumnName = "FUI_USER_ID")]
		public long UserId { get; set; }

		/// <summary>
		/// Desc:厂商KEY
		/// Default:-1
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_FACTORY_ID")]
        [JsonIgnore]
		public long? FactoryId { get; set; }

		/// <summary>
		/// Desc:用户名称
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_USER_NAME")]
        [JsonIgnore]
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_GENDER")]
		public string Gender { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_MOBILE")]
		public string Mobile { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_TEL")]
		public string Tel { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_EMAIL")]
		public string EMail { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_DESC")]
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_DEPARTMENT")]
		public string Department { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_ENTRYDATE")]
        [JsonIgnore]
		public DateTime? EntryDate { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FUI_UID")]
        [JsonIgnore]
		public string FUI_UID { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FERP_STAFFID")]
        [JsonIgnore]
		public long FERP_STAFFID { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FJobNumber")]
		public string JobNumber { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FJobAddress")]
		public string JobAddress { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FRemarks")]
		public string Remarks { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FHeadPortrait")]
		public string HeadPortrait { get; set; } = string.Empty;

	}
}