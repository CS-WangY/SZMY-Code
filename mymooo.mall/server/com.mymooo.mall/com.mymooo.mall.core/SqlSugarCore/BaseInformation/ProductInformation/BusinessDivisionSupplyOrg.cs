using SqlSugar;
using System.Text.Json.Serialization;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
    ///<summary>
    ///
    ///</summary>
    public partial class BusinessDivisionSupplyOrg
    {
        public BusinessDivisionSupplyOrg()
		{

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string BusinessDivisionId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string BusinessDivisionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string BusinessDivisionName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string SupplyOrgNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string SupplyOrgName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool IsDefault { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [JsonIgnore]
        public long CreateUserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [JsonIgnore]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [JsonIgnore]
        public long ModifyUserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [JsonIgnore]
        public DateTime? ModifyDate { get; set; }

    }
}