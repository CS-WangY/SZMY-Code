using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.BaseInfo
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("T_ORG_ORGANIZATIONS_L")]
    public partial class OrganizationsLocale
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public int FPKID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FLOCALEID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FNAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FDESCRIPTION { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FADDRESS { get; set; } = string.Empty;

    }
}