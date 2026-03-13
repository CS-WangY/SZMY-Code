using System;
using System.Linq;
using System.Text;
using mymooo.core.Attributes.Redis;
using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
    ///<summary>
    ///
    ///</summary>
    [RedisKey("mymooo-companysupplyorg")]
    public partial class CompanyMapSupplyOrg
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public CompanyMapSupplyOrg()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [RedisPrimaryKey]
        public string CompanyCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>         
        [RedisValue]
        public string SupplyOrgCode { get; set; } = string.Empty;


        public string SupplyOrgName { get; set; } = string.Empty;

        public long SupplyOrgId { get; set; }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>  
        [RedisPrimaryKey]
        public string BusinessDivisionNumber { get; set; } = string.Empty;


        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool IsDeleted { get; set; }

    }
}