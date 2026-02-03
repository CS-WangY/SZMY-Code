using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.BaseInfo
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("T_BD_UNIT")]
    public partial class Unit
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public int FUNITID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FMASTERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FCREATEORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FUSEORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FCREATORID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? FCREATEDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FMODIFIERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? FMODIFYDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FDOCUMENTSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FFORBIDSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public int? FAPPROVERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? FAPPROVEDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public int? FFORBIDDERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? FFORBIDDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FNUMBER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FUNITGROUPID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISBASEUNIT { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISSYSTEMSET { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FPRECISION { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FROUNDTYPE { get; set; } = string.Empty;

    }
}