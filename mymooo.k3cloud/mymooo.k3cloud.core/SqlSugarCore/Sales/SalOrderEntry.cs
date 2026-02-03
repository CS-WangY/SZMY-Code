using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace mymooo.k3cloud.core.SqlSugarCore.Sales
{
    ///<summary>
    ///销售订单子表
    ///</summary>
    [SugarTable("T_SAL_ORDERENTRY")]
    public partial class SalOrderEntry
    {
        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "FENTRYID")]
        public long EntryId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FID")]
        public long Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSEQ")]
        public long Seq { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FMAPID")]
        public string Mapid { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FMAPNAME")]
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMATERIALID")]
        public long Materialid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FAUXPROPID")]
        public long Auxpropid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBOMID")]
        public long Bomid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUNITID")]
        public long Unitid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0.0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQTY")]
        public decimal Qty { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBASEUNITID")]
        public long BaseUnitid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBASEUNITQTY")]
        public decimal BaseUnitqty { get; set; }

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
        [SugarColumn(ColumnName = "FMRPFREEZESTATUS")]
        public string MrpFreezestatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FFREEZEDATE")]
        public DateTime? Freezedate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FFREEZERID")]
        public long Freezerid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMRPTERMINATESTATUS")]
        public string Mrpterminatestatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FTERMINATERID")]
        public long Terminaterid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FTERMINATESTATUS")]
        public string Terminatestatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FTERMINATEDATE")]
        public DateTime? Terminatedate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMRPCLOSESTATUS")]
        public string Mrpclosestatus { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLOT")]
        public long Lot { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCHANGEFLAG")]
        public string Changeflag { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKORGID")]
        public long Stockorgid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKID")]
        public long Stockid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLOCKQTY")]
        public decimal Lockqty { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLOCKFLAG")]
        public string Lockflag { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOWNERTYPEID")]
        public string Ownertypeid { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOWNERID")]
        public long Ownerid { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLOT_TEXT")]
        public string Lot_text { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPRODUCEDATE")]
        public DateTime? Producedate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FEXPIRYDATE")]
        public DateTime? Expirydate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FEXPUNIT")]
        public string Expunit { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FEXPPERIOD")]
        public decimal Expperiod { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FRETURNTYPE")]
        public string Returntype { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBFLOWID")]
        public string Bflowid { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPRIORITY")]
        public long? Priority { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FMTONO")]
        public string Mtono { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FRESERVETYPE")]
        public string Reservetype { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPLANDELIVERYDATE")]
        public DateTime? FPLANDELIVERYDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDELIVERYSTATUS")]
        public string FDELIVERYSTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOLDQTY")]
        public decimal FOLDQTY { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPROMOTIONMATCHTYPE")]
        public string FPROMOTIONMATCHTYPE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSUPPLYORGID")]
        public long Supplyorgid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FNETORDERENTRYID")]
        public long NetOrderEntryid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKUNITID")]
        public long Stockunitid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0.0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKQTY")]
        public decimal Stockqty { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKBASEQTY")]
        public decimal Stockbaseqty { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLEFTQTY")]
        public decimal FLEFTQTY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINSTOCKPRICE")]
        public decimal FINSTOCKPRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSOSTOCKID")]
        public long Sostockid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSOSTOCKLOCALID")]
        public long FSOSTOCKLOCALID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBarcode")]
        public string FBarcode { get; set; } = string.Empty;

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
        [SugarColumn(ColumnName = "FRetailSaleProm")]
        public string FRetailSaleProm { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FALLAMOUNTEXCEPTDISCOUNT")]
        public decimal FALLAMOUNTEXCEPTDISCOUNT { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMATERIALGROUP")]
        public long FMATERIALGROUP { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMANUALROWCLOSE")]
        public string FMANUALROWCLOSE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMATERIALGROUPBYMAT")]
        public long FMATERIALGROUPBYMAT { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "F_UN_JOINPREPICKBASEQTY")]
        public decimal F_UN_JOINPREPICKBASEQTY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FISDISPATCH")]
        public string FISDISPATCH { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FORDERDETAILID")]
        public long FORDERDETAILID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSUPPLIERID")]
        public long FSUPPLIERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSUPPLIERUNITPRICE")]
        public decimal FSUPPLIERUNITPRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRODUCTENGINEERID")]
        public long FPRODUCTENGINEERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPRODUCTMANAGERID")]
        public long FPRODUCTMANAGERID { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTMATERIALNO")]
        public string FCUSTMATERIALNO { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPROJECTNO")]
        public string FPROJECTNO { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSTOCKFEATURES")]
        public string FSTOCKFEATURES { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FLOCFACTORY")]
        public string FLOCFACTORY { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINSIDEREMARK")]
        public string FINSIDEREMARK { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSmallId")]
        public long FSmallId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FParentSmallId")]
        public long FParentSmallId { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBusinessDivisionId")]
        public string FBusinessDivisionId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FSUPPLIERDESCRIPTIONS")]
        public string FSUPPLIERDESCRIPTIONS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FSUPPLIERREPLYDATE")]
        public DateTime? FSUPPLIERREPLYDATE { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSupplierUnitPriceSource")]
        public string FSupplierUnitPriceSource { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FCostPriceUpdateDate")]
        public DateTime? FCostPriceUpdateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCostPriceUpdateUser")]
        public string FCostPriceUpdateUser { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSupplierProductCode")]
        public string FSupplierProductCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FAdjustPrice")]
        public decimal FAdjustPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default: 
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDifferReason")]
        public string FDifferReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FUpdateBalance")]
        public decimal FUpdateBalance { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FOUTSOURCESTOCKLOC")]
        public string FOUTSOURCESTOCKLOC { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYMOQTY")]
        public decimal FPENYMOQTY { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYCLOSESTATUS")]
        public string FPENYCLOSESTATUS { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FSUPPLYTARGETORGID")]
        public long FSUPPLYTARGETORGID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPENYCLOSEDATETIME")]
        public DateTime? FPENYCLOSEDATETIME { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINQUIRYORDER")]
        public string FINQUIRYORDER { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FINQUIRYORDERLINENO")]
        public long FINQUIRYORDERLINENO { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDRAWINGRECORDID")]
        public long FDRAWINGRECORDID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTITEMNO")]
        public string FCUSTITEMNO { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCUSTITEMNAME")]
        public string FCUSTITEMNAME { get; set; } = string.Empty;

    }
}