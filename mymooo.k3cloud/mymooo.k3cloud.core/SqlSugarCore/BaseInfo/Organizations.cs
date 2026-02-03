using System;
using System.Linq;
using System.Text;
using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.BaseInfo
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("T_ORG_ORGANIZATIONS")]
    public partial class Organizations
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public int FORGID { get; set; }

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
        public int FORGFORMID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FCONTACT { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FTEL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FORGFUNCTIONS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISACCOUNTORG { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISBUSINESSORG { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FACCTORGTYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FPARENTID { get; set; }

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
        public string FFORBIDSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FFORBIDORID { get; set; }

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
        public string FDOCUMENTSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISSYSPRESET { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FAUDITORID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? FAUDITDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? FACCTGRANGEID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FVALUATIONMETHOD { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? FACCTGORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? FACCTGSYSTEMID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FDIVIDEDBASIS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? FACCTPOLICYID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FPOSTCODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public string FISNETSTORE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISSYNCERPOWNER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int FTIMEZONE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FErpCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal FPOINTSRATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FOUTSOURCESTOCKLOC { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FTRANSISSYNCLOUDSTOCK { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public string FISCLOUDSTOCKCALLBACK { get; set; } = string.Empty;

    }
}