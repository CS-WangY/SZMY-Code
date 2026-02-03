using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement.Report
{
    /// <summary>
    /// 加急收货日志
    /// </summary>
    public class T_PUR_UrgentDeliveryLog
    {
        /// <summary>
        /// 销售单Id
        /// </summary>
        public long FSoId { get; set; }
        /// <summary>
        /// 销售单明细Id
        /// </summary>
        public long FSoEntryId { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public string FSoNo { get; set; }
        /// <summary>
        /// 销售单序号
        /// </summary>
        public long FSoSeq { get; set; }
        /// <summary>
        /// 供货组织
        /// </summary>
        public long FSupplyTargetOrgId { get; set; }
        /// <summary>
        /// 物料编号
        /// </summary>
        public string FMaterialCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string FMaterialName { get; set; }
        /// <summary>
        /// 销售单行状态
        /// </summary>
        public string FSoStatus { get; set; }
        /// <summary>
        /// 销售单数量
        /// </summary>
        public decimal FQty { get; set; }
        /// <summary>
        /// 采购单Id
        /// </summary>
        public long FPoId { get; set; }
        /// <summary>
        /// 采购单明细Id
        /// </summary>
        public long FPoEntryId { get; set; }
        /// <summary>
        /// 采购单号
        /// </summary>
        public string FPoNo { get; set; }
        /// <summary>
        /// 采购单序号
        /// </summary>
        public long FPoSeq { get; set; }
        /// <summary>
        /// 预留数量
        /// </summary>
        public decimal FBaseQty { get; set; }
        /// <summary>
        /// 采购员微信Code
        /// </summary>
        public string FBuyWechatCode { get; set; }
        /// <summary>
        /// 采购员
        /// </summary>
        public string FBuyName { get; set; }
        /// <summary>
        /// 物流公司编码
        /// </summary>
        public string FTrackingCode { get; set; }
        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string FTrackingName { get; set; }
        /// <summary>
        /// 物流公司单号
        /// </summary>
        public string FTrackingNumber { get; set; }
        /// <summary>
        /// 供应商回复交期
        /// </summary>
        public string FSupplierDescriptions { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime FCreateDate { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string FCreateUser { get; set; }
        /// <summary>
        /// 通知人
        /// </summary>
        public string FNotifyer { get; set; }
        /// <summary>
        /// 通知备注
        /// </summary>
        public string FRemarks { get; set; }
    }
}
