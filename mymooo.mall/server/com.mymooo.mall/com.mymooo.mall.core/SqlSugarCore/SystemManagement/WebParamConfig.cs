using System;
using System.Linq;
using System.Text;
using mymooo.core.Attributes.Redis;
using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SystemManagement
{
    ///<summary>
    ///
    ///</summary>
    [RedisKey("mymooo-management-webparamconfig", isSaveMain: false)]
    public partial class WebParamConfig
    {
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WebParamConfig()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string WContent { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [RedisMainField(groupId: "system")]
        public string WKey { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public byte WKeyType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [RedisValue(mainKey: "system")]
        public string WValue { get; set; } = string.Empty;

    }
}