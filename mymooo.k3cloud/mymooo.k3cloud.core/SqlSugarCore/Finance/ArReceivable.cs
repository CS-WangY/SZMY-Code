using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.Finance
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("T_AR_RECEIVABLE")]
    public partial class ArReceivable
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "FID")]
        public int Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBILLTYPEID")]
        public string BillTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBILLNO")]
        public string BillNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDATE")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FENDDATE")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTOMERID")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCURRENCYID")]
        public int CurrencyId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSETTLEORGID")]
        public int SettleOrgId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEORGID")]
        public int SalesOrgId { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOWNERTYPE")]
        public string OwnerType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOWNERID")]
        public int OwnerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FALLAMOUNTFOR")]
        public decimal TatolAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FWRITTENOFFSTATUS")]
        public string WritenOffStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOPENSTATUS")]
        public string OpenStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FWRITTENOFFAMOUNTFOR")]
        public decimal WritenOffAmountFor { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FNOTWRITTENOFFAMOUNTFOR")]
        public decimal NotWritenOffAmountFor { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRELATEHADPAYAMOUNT")]
        public decimal RelateHadPayAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FWRITTENOFFAMOUNT")]
        public decimal WritenOffAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FNOTWRITTENOFFAMOUNT")]
        public decimal NotWritenOffAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDOCUMENTSTATUS")]
        public string DocumentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCREATORID")]
        public int CreatorId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCREATEDATE")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMODIFIERID")]
        public int ModifyId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMODIFYDATE")]
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FACCOUNTSYSTEM")]
        public int AccountSystem { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISTAX")]
        public string IsTax { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBYVERIFY")]
        public string ByVerify { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:A
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCANCELSTATUS")]
        public string CancelStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCANCELLERID")]
        public int? CancelId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCANCELDATE")]
        public DateTime? CancelDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FAPPROVERID")]
        public int ApproveId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FAPPROVEDATE")]
        public DateTime? ApproveDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPAYCONDITON")]
        public int PayCondition { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEDEPTID")]
        public int salesDepartmentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEGROUPID")]
        public int SalesGroupId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEERID")]
        public int SalesId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCREDITCHECKRESULT")]
        public string CreditCheckResult { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISINIT")]
        public string IsInit { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALESBUSTYPE")]
        public string SalesBusType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBUSINESSTYPE")]
        public string BusinessType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPAYORGID")]
        public int PayOrgId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISRETAIL")]
        public string IsRetail { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCASHSALE")]
        public string CashSale { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMATCHMETHODID")]
        public int MatchmethonId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISPRICEEXCLUDETAX")]
        public string FISPRICEEXCLUDETAX { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FREMARK")]
        public string FREMARK { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSETACCOUNTTYPE")]
        public string FSETACCOUNTTYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISHOOKMATCH")]
        public string FISHOOKMATCH { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISWRITEOFF")]
        public string FISWRITEOFF { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOUTSTOCKBILLNO")]
        public string FOUTSTOCKBILLNO { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRECEIVABLETYPE")]
        public string FRECEIVABLETYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISINVOICEARLIER")]
        public string FISINVOICEARLIER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FREDBLUE")]
        public string FREDBLUE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISCASHCUSTROMER")]
        public string FISCASHCUSTROMER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "F_PENY_ISINVALID")]
        public string F_PENY_ISINVALID { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBILLMATCHLOGID")]
        public int FBILLMATCHLOGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FORDERDISCOUNTAMOUNTFOR")]
        public decimal FORDERDISCOUNTAMOUNTFOR { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBranchId")]
        public long FBranchId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISGENERATEPLANBYCOSTITEM")]
        public string FISGENERATEPLANBYCOSTITEM { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISZY")]
        public string FISZY { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "F_PENY_ISCUSTOMERNUMBERINVOICE")]
        public string F_PENY_ISCUSTOMERNUMBERINVOICE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYMONTH")]
        public string FPENYMONTH { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "F_PENY_ISAPPROVAL")]
        public string F_PENY_ISAPPROVAL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISOPENIV")]
        public string FISOPENIV { get; set; } = string.Empty;

    }
}