using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.BaseInfo
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("T_BD_CUSTOMER")]
    public partial class Customer
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "FCUSTID")]
        public int FCUSTID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FMASTERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FNUMBER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FDOCUMENTSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCOUNTRY { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FPROVINCIAL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FZIP { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FWEBSITE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FTEL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FFAX { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FTAXREGISTERCODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FGROUPCUSTID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FSUPPLIERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FTRADINGCURRID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FSALDEPTID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FSALGROUPID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FSELLER { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FTRANSLEADTIME { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FPRICELISTID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FDISCOUNTLISTID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FSETTLETYPEID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FRECEIVECURRID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FRECCONDITIONID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FISCREDITCHECK { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FTAXRATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FFORBIDSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FCREATEORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FUSEORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FCREATORID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public DateTime? FCREATEDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FMODIFIERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public DateTime? FMODIFYDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FAPPROVERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public DateTime? FAPPROVEDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FFORBIDDERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public DateTime? FFORBIDDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FTAXTYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCUSTTYPEID { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int? FPRIMARYGROUP { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCOMPANYSCALE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCOMPANYNATURE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCOMPANYCLASSIFY { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int FCORRESPONDORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public int? FPRIORITY { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FINVOICETYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FISDEFPAYER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCPADMINCODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FISGROUP { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FISTRADE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FINVOICETITLE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FINVOICEADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FSENDADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FUNCHECKEXPECTQTY { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FLEGALPERSON { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FREGISTERFUND { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FFOUNDDATE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FSOCIALCRECODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FREGISTERADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FISGROUPTAG { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FDOMAINS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:  
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTID")]
        public string FCUSTINVOICETYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        public string FINVOICERECIPIENT { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:4
        /// Nullable:False
        /// </summary>           
        public int FDecimalPlacesOfUnitPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FPENYCUSTOMERLEVEL { get; set; } = string.Empty;

    }
}