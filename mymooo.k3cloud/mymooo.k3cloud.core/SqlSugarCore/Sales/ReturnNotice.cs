using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.Sales
{
    ///<summary>
    ///退货通知单
    ///</summary>
    [SugarTable("T_SAL_RETURNNOTICE")]
    public partial class ReturnNotice
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "FID")]
        public long Id { get; set; }

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
        [SugarColumn(ColumnName = "FBillNo")]
        public string BillNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDate")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Desc:退货客户
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRetcustId")]
        public long CustomerId { get; set; }

        /// <summary>
        /// Desc:库存组织
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRETORGID")]
        public long RetorgId { get; set; }

        /// <summary>
        /// Desc:库存部门
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRETDEPTID")]
        public long RetdeptId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKERGROUPID")]
        public long StockerGroupId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKERID")]
        public long StockerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRECEIVECUSID")]
        public long ReceiveCusId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSETTLECUSID")]
        public long SettleCusId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPAYCUSID")]
        public long PayCusId { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDELIVERYNO")]
        public string DeliveryNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FTAKEDELIVERYNO")]
        public string TakeDeliveryNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRETURNREASON")]
        public string ReturnReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEORGID")]
        public long SaleOrgId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEGROUPID")]
        public long SaleGroupId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEDEPTID")]
        public long SaleDeptId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALESMANID")]
        public long SalesmanId { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDESCRIPTION")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDOCUMENTSTATUS")]
        public string DocumentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCANCELSTATUS")]
        public string CancelStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCREATORID")]
        public long CreatorId { get; set; }

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
        public long ModifierId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FMODIFYDATE")]
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FAPPROVERID")]
        public long ApproverId { get; set; }

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
        [SugarColumn(ColumnName = "FCANCELLERID")]
        public long CancellerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCANCELDATE")]
        public DateTime? CancelDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FOWNERTYPEID")]
        public string OwnerTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FOWNERID")]
        public long? OwnerId { get; set; }

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
        [SugarColumn(ColumnName = "FHEADLOCID")]
        public long HeadLocId { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FHEADLOCADDRESS")]
        public string HeadLocAddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRECEIVEADDRESS")]
        public string ReceiveAddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCREDITCHECKRESULT")]
        public string CreditCheckResult { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOBJECTTYPEID")]
        public string ObjectTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCORRESPONDORGID")]
        public long? CorrespondOrgId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRECCONTACTID")]
        public long RecContactId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:A
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCLOSESTATUS")]
        public string CloseStatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCLOSERID")]
        public long CloserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMANUALCLOSE")]
        public string ManualClose { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCLOSEDDATE")]
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLINKMAN")]
        public string LinkMan { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLINKPHONE")]
        public string LinkPhone { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCLOSEREASON")]
        public string CloseReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYISPUSH")]
        public string PenyIsPush { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBUSINESSDIVISIONIDS")]
        public string BusinessDivisionIds { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYRETURNTYPE")]
        public string PenyReturnType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETBASE1")]
        public long PresetBase1 { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETBASE2")]
        public long PresetBase2 { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETASSISTANT1")]
        public string PresetAssistant1 { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETASSISTANT2")]
        public string PresetAssistant2 { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYRMTYPE")]
        public string PenyRmType { get; set; } = string.Empty;

    }
}