using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.Sales
{
    ///<summary>
    ///销售订单主表
    ///</summary>
    [SugarTable("T_SAL_ORDER")]
    public partial class SalOrder
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
        public string BillTypeid { get; set; } = string.Empty;

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
        /// Default:0
        /// Nullable:False
        /// </summary>        
        [SugarColumn(ColumnName = "FCUSTID")]
        public long CustId { get; set; }

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
        public long SaleDeptid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALERID")]
        public long SalerId { get; set; }

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
        public DateTime Createdate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMODIFIERID")]
        public long Modifierid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FMODIFYDATE")]
        public DateTime? Modifydate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDOCUMENTSTATUS")]
        public string Documentstatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FAPPROVERID")]
        public long Approverid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FAPPROVEDATE")]
        public DateTime? Approvedate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
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
        public long Closerid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCLOSEDATE")]
        public DateTime? CloseDate { get; set; }

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
        [SugarColumn(ColumnName = "FCANCELLERID")]
        public long Cancellerid { get; set; }

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
        [SugarColumn(ColumnName = "FRECEIVEID")]
        public long Receiveid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSETTLEID")]
        public long Settleid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCHARGEID")]
        public long Chargeid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FVERSIONNO")]
        public string Versionno { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCHANGEREASON")]
        public string Changereason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCHANGEDATE")]
        public DateTime? Changedate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCHANGERID")]
        public long Changerid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FNOTE")]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBUSINESSTYPE")]
        public string Businesstype { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FHEADLOCID")]
        public long Headlocid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FHEADLOCADDRESS")]
        public string Headlocaddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FHEADDELIVERYWAY")]
        public string Headdeliveryway { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCOUNTRY")]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRECEIVEADDRESS")]
        public string Receiveaddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCREDITCHECKRESULT")]
        public string Creditcheckresult { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOBJECTTYPEID")]
        public string Objecttypeid { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FFINALVERSION")]
        public string Finalversion { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FORIGINALFID")]
        public long Originalfid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCORRESPONDORGID")]
        public long? Correspondorgid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRECCONTACTID")]
        public long Reccontactid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FNETORDERBILLNO")]
        public string Netorderbillno { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FNETORDERBILLID")]
        public long Netorderbillid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISINIT")]
        public string Isinit { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOPPID")]
        public long Oppid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSALEPHASEID")]
        public long FSALEPHASEID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISMOBILE")]
        public string FISMOBILE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMANUALCLOSE")]
        public string FMANUALCLOSE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSIGNSTATUS")]
        public string FSIGNSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSOFROM")]
        public string FSOFROM { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLinkMan")]
        public string FLinkMan { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLinkPhone")]
        public string FLinkPhone { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCONTRACTTYPE")]
        public string FCONTRACTTYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCONTRACTID")]
        public long FCONTRACTID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISUSEOEMBOMPUSH")]
        public string FISUSEOEMBOMPUSH { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FXPKID")]
        public long FXPKID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCLOSEREASON")]
        public string FCLOSEREASON { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISUSEDRPSALEPOPUSH")]
        public string FISUSEDRPSALEPOPUSH { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISCREATESTRAIGHTOUTIN")]
        public string FISCREATESTRAIGHTOUTIN { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTPURCHASENO")]
        public string FCUSTPURCHASENO { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICETYPE")]
        public string FINVOICETYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICETITLE")]
        public string FINVOICETITLE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICETEL")]
        public string FINVOICETEL { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICEADDRESS")]
        public string FINVOICEADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FTAXCODE")]
        public string FTAXCODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBANKNAME")]
        public string FBANKNAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBANKACCOUNT")]
        public string FBANKACCOUNT { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICECONSIGNEENAME")]
        public string FINVOICECONSIGNEENAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICECONSIGNEEPHONE")]
        public string FINVOICECONSIGNEEPHONE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FINVOICECONSIGNEEADDRESS")]
        public string FINVOICECONSIGNEEADDRESS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMEETFILTER")]
        public string FMEETFILTER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENY_MOBILLSTATUS")]
        public string FPENY_MOBILLSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPLATCREATORID")]
        public string FPLATCREATORID { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPLATCREATORTYPE")]
        public string FPLATCREATORTYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FSALESORDERDATE")]
        public DateTime? FSALESORDERDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FAUDITTIME")]
        public DateTime? FAUDITTIME { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPLATCREATORWXCODE")]
        public string FPLATCREATORWXCODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETBASE1")]
        public long FPRESETBASE1 { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETBASE2")]
        public long FPRESETBASE2 { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETASSISTANT1")]
        public string FPRESETASSISTANT1 { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRESETASSISTANT2")]
        public string FPRESETASSISTANT2 { get; set; } = string.Empty;

        /// <summary>
        /// 销售订单明细
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(SalOrderEntry.Id))]
        public List<SalOrderEntry> SaleOrderEntry { get; set; } = [];
    }
}