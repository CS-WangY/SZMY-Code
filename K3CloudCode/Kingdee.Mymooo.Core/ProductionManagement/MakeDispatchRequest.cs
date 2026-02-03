using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class MakeRequest
    {
        public string MakeNo { get; set; }
        public DateTime Date { get; set; }
        public string PrdOrgName { get; set; }
        public string PrdOrgCode { get; set; }
        public string PlannerName { get; set; }
        public string PlannerCode { get; set; }

        public List<MakeDispatchRequest> Details { get; set; }
        public long Id { get; set; }
    }
    public class MakeDispatchRequest
    {
        public MakeDispatchRequest()
        {
            this.MoType = "1";
            this.EarlyMtl = "1";
            this.Key = Guid.NewGuid().ToString("N");
        }
        public string SaleOrderNo { get; set; }
        public string MakeNo { get; set; }
        public int MakeSeq { get; set; }
        public string WorksNo { get; set; }
        public string DwgNo { get; set; }
        public string DwgVer { get; set; }
        public int Qty { get; set; }

        /// <summary>
        /// 急件状态（1：正常，2：急件，3：特急），String，必填
        /// </summary>
        public string MoType { get; set; }

        /// <summary>
        /// 交期
        /// </summary>
        public DateTime DeadLine { get; set; }
        public string MtlCode { get; set; }
        public string MtlType { get; set; }

        /// <summary>
        /// 是否提前下料（1：是；0：否）
        /// </summary>
        public string EarlyMtl { get; set; }
        public string Key { get; set; }
        public string PlaneUrl { get; set; }
        public string Remark { get; set; }
        public string PartTypeCode { get; set; }
        public string ThreeUrl { get; set; }
        public string ThreeVer { get; set; }
        public long ProductId { get; set; }
        public long EntryId { get; set; }

        /// <summary>
        /// 用户企业微信编码
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 销售额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 自动工艺
        /// </summary>
        public bool AutoCraft { get; set; }

        /// <summary>
        /// 车间启用MES
        /// </summary>
        public bool EnableMes { get; set; }

        public List<DynamicObject> MaterialDetails { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime? SalesDate { get; set; }

        /// <summary>
        /// 图 名称
        /// </summary>
        public string DwgName { get; set; }
        /// <summary>
        /// 报价单单号
        /// </summary>
        public string quotationOrderNo { get; set; }
        /// <summary>
        /// 报价单行号
        /// </summary>
        public string quotationOrderLineNo { get; set; }
        /// <summary>
        /// 图号编码
        /// </summary>
        public string DrawingNumber { get; set; }
        /// <summary>
        /// 图号ID
        /// </summary>
        public string DrawingRecordId { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CutNumber { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CutName { get; set; }
        /// <summary>
        /// 是否手工单
        /// </summary>
        public bool isManual { get; set; }
        /// <summary>
        /// 客户采购单号
        /// </summary>
        public string CustPurchaseNo { get; set; }

        /// <summary>
        /// 销售订单明细ID
        /// </summary>
        public long OrderEntryId { get; set; }

        /// <summary>
        /// 销售订单明细序号
        /// </summary>
        public int OrderEntrySeq { get; set; }

        /// <summary>
        /// 客户erp物料编码
        /// </summary>
        public string CustMaterialNo { get; set; }

        /// <summary>
        /// 客户物料编码
        /// </summary>
        public string CustItemNo { get; set; }

        /// <summary>
        /// 客户物料名称
        /// </summary>

        public string CustItemName { get; set; }

    }
}
