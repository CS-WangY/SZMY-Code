using mymooo.core.Attributes.Redis;
using SqlSugar;
using SqlSugar.DbConvert;
using System.Text.Json.Serialization;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation
{
	///<summary>
	///用户表
	///</summary>
	[SugarTable("F_USER_MSTR")]
    [RedisKey("mymooo-management-user")]
    public partial class ManagementUser
	{
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public ManagementUser()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

        }

        /// <summary>
        /// Desc:用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FUM_USER_ID")]
        [RedisPrimaryKey]
        public long UserId { get; set; }

        /// <summary>
        /// Desc:用户账号
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_USER_NO")]
        [RedisValue]   
        public string UserAccount { get; set; } = string.Empty;

        /// <summary>
        /// Desc:厂商KEY
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_FACTORY_ID")]
        public long? FactoryId { get; set; }

        /// <summary>
        /// Desc:客户KEY
        /// Default:-1
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_CUST_ID")]
        [JsonIgnore]
        public long? FUM_CUST_ID { get; set; }

        /// <summary>
        /// Desc:用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_USER_NAME")]
        [RedisValue]   
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户密码
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_USER_PASSWORD")]
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户状态	   E:启用	   D:禁用
        /// Default:D
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_USER_STATUS", SqlParameterDbType = typeof(EnumToStringConvert))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [RedisValue]   
		public UserStatus UserStatus { get; set; } 

        /// <summary>
        /// Desc:用户创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_CREATE_DATE")]
        [JsonIgnore]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Desc:用户创建用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_CREATE_USER")]
        [JsonIgnore]
        public string CreateUserName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户创建用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_CREATE_USER_ID")]
        [JsonIgnore]
        public long CreateUserId { get; set; }

        /// <summary>
        /// Desc:用户修改时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_UPDATE_DATE")]
        [JsonIgnore]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Desc:用户修改用户名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_UPDATE_USER")]
        [JsonIgnore]
        public string UpdateUserName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户修改用户KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_UPDATE_USER_ID")]
        [JsonIgnore]
        public long UpdateUserId { get; set; }

        /// <summary>
        /// Desc:用户类型	   A: 平台管理员	   F: 厂商管理员	  C: 客户管理员  N: 普通用户
        /// Default:N
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_USER_TYPE", SqlParameterDbType = typeof(EnumToStringConvert))]
		[JsonConverter(typeof(JsonStringEnumConverter))]
        [RedisValue]   
		public ManagementUserType UserType { get; set; } 

        /// <summary>
        /// Desc:用户注册邮箱
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUM_USER_EMAIL")]
        [JsonIgnore]
        public string EMail { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FInternalAccount")]
        [JsonIgnore]
        public bool InternalAccount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [JsonIgnore]
        public string PasswordSalt { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [RedisValue]   
        public string WeChatCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string MymoooCompany { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long WeChatUserId { get; set; }
	}
}