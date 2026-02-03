using Kingdee.BOS;
using Kingdee.Mymooo.Core.BaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    public class PUR_Requisition
    {
        public string BillNo { get; set; }
        public string BillTypeID { get; set; }
        /// <summary>
        /// 组织编码
        /// </summary>
        public string FApplicationOrgId { get; set; }
        public string FApplicantId { get; set; }
        public List<PURReqEntry> Entry { get; set; }
    }
    public class PURReqEntry
    {
        public string FMaterialId { get; set; }
        public string FMaterialName { get; set; }
        public string ProductSmallClassId { get; set; }
        public long ProductId { get; set; }
        public decimal FReqQty { get; set; }
        public decimal FSUPPLIERUNITPRICE { get; set; }
        public string FEntryNote { get; set; }
        /// <summary>
        /// 源单类型
        /// </summary>
        public string SrcBillTypeId { get; set; }
        /// <summary>
        /// 源单编号
        /// </summary>
        public string SrcBillNo { get; set; }
        /// <summary>
        /// 需求来源
        /// </summary>
        public string DEMANDTYPE { get; set; }
        /// <summary>
        /// 需求单据编号
        /// </summary>
        public string DEMANDBILLNO { get; set; }
        /// <summary>
        /// 需求单据行号
        /// </summary>
        public string DEMANDBILLENTRYSEQ { get; set; }
        /// <summary>
        /// 需求单据分录内码
        /// </summary>
        public long DEMANDBILLENTRYID { get; set; }
        /// <summary>
        /// 事业部
        /// </summary>
        public string BUSINESSDIVISIONID { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }
        /// <summary>
        /// 销售单序号
        /// </summary>
        public int SoSeq { get; set; }
        /// <summary>
        /// 销售要货日期
        /// </summary>
        public DateTime PENYDELIVERYDATE { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string salCreatorName { get; set; }
        /// <summary>
        /// 客户ERP物料编码
        /// </summary>
        public string CUSTMATERIALNO { get; set; }
        /// <summary>
        /// 客户物料编号
        /// </summary>
        public string PENYMAPCODE { get; set; }
        /// <summary>
        /// 客户物料名称
        /// </summary>
        public string PENYMAPNAME { get; set; }
        /// <summary>
        /// 净重
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// 体积
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// 材质
        /// </summary>
        public string Textures { get; set; }
        /// <summary>
        /// 重量单位
        /// </summary>
        public string WeightUnitid { get; set; }
        /// <summary>
        /// 尺寸单位
        /// </summary>
        public string VolumeUnitid { get; set; }
        /// <summary>
        /// 图纸ID
        /// </summary>
        public long DrawingRecordId { get; set; }
    }
}
