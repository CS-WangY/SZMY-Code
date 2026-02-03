using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using static Kingdee.Mymooo.Core.SalesManagement.SalesOrderBillRequest;

namespace Kingdee.Mymooo.Core.PlanOrderManagement
{
    public class PrOrMoBill
    {
        /// <summary>
        /// 单据ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string DocumentStatus { get; set; }
        /// <summary>
        /// 生产组织/采购组织
        /// </summary>
        public string PrdOrgId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 生产发料
        /// </summary>
        public string IssueMtrl { get; set; }
        public List<PrOrMoBillEntity> Entity { get; set; }
    }
    public class PrOrMoBillEntity
    {
        /// <summary>
        /// 分录ID
        /// </summary>
        public long EntryId { get; set; }
        public int Seq { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialId { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }
        /// <summary>
        /// 生产车间
        /// </summary>
        public string WorkShopID { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string UnitId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 业务状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 计划开工时间
        /// </summary>
        public DateTime PlanStartDate { get; set; }
        /// <summary>
        /// 计划完工时间
        /// </summary>
        public DateTime PlanFinishDate { get; set; }
        /// <summary>
        /// 需求组织
        /// </summary>
        public string RequestOrgId { get; set; }
        /// <summary>
        /// BOM版本
        /// </summary>
        public string BomId { get; set; }
        /// <summary>
        /// 入库组织
        /// </summary>
        public string StockInOrgId { get; set; }
        /// <summary>
        /// 源单类型
        /// </summary>
        public string SrcBillType { get; set; }
        /// <summary>
        /// 来源单据分录内码
        /// </summary>
        public long SrcBillEntryId { get; set; }
        /// <summary>
        /// 销售订单分录内码
        /// </summary>
        public long SaleOrderEntryId { get; set; }
        /// <summary>
        /// 销售订单ID
        /// </summary>
        public long SaleOrderId { get; set; }
        /// <summary>
        /// 来源单据ID
        /// </summary>
        public long SrcBillId { get; set; }
        /// <summary>
        /// 源单编号
        /// </summary>
        public string SrcBillNo { get; set; }
        /// <summary>
        /// 源单分录行号
        /// </summary>
        public long SrcBillEntrySeq { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string MemoItem { get; set; }
        /// <summary>
        /// 需求单据
        /// </summary>
        public string SaleOrderNo { get; set; }
        /// <summary>
        /// 基本单位
        /// </summary>
        public string BaseUnitId { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string StockId { get; set; }
        /// <summary>
        /// 计划确认日期
        /// </summary>
        public DateTime PlanConfirmDate { get; set; }
        /// <summary>
        /// 需求来源
        /// </summary>
        public string ReqSrc { get; set; }
        /// <summary>
        /// 产品小类
        /// </summary>
        public string SMALLID { get; set; }
        /// <summary>
        /// 产品大类
        /// </summary>
        public string PARENTSMALLID { get; set; }
        /// <summary>
        /// 事业部
        /// </summary>
        public string BUSINESSDIVISIONID { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string PENYCustomerID { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string PENYCustomerName { get; set; }
        /// <summary>
        /// 报价单单号
        /// </summary>
        public string InquiryOrder { get; set; }
        /// <summary>
        /// 报价单行号
        /// </summary>
        public string InquiryOrderLineNo { get; set; }
        /// <summary>
        /// 图纸ID
        /// </summary>
        public string DrawingRecordId { get; set; }
        /// <summary>
        /// 含税单价
        /// </summary>
        public decimal PENYPrice { get; set; }
    }
    public class PlanorderBillEntity
    {
        /// <summary>
        /// 单据ID
        /// </summary>
        public long BillId { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillTypeID { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 采购/生产组织
        /// </summary>
        public string SupplyOrgNumber { get; set; }
        /// <summary>
        /// 采购/生产组织
        /// </summary>
        public string SupplyOrgName { get; set; }
        /// <summary>
        /// 需求组织
        /// </summary>
        public string DemandOrgNumber { get; set; }
        /// <summary>
        /// 需求组织
        /// </summary>
        public string DemandOrgName { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialNumber { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; }
        /// <summary>
        /// 辅助属性
        /// </summary>
        public string AuxPropId { get; set; }
        /// <summary>
        /// 投放类型
        /// </summary>
        public string ReleaseType { get; set; }
        /// <summary>
        /// 投放单据类型
        /// </summary>
        public string ReleaseBillType { get; set; }
        /// <summary>
        /// 成品率
        /// </summary>
        public decimal YieldRate { get; set; }
        /// <summary>
        /// 供应组织物料
        /// </summary>
        public string SupplyMaterialId { get; set; }
        /// <summary>
        /// BOM版本
        /// </summary>
        public string BomId { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string UnitId { get; set; }
        /// <summary>
        /// 建议订单量
        /// </summary>
        public string SugQty { get; set; }
        /// <summary>
        /// 建议采购/生产日期
        /// </summary>
        public DateTime PlanStartDate { get; set; }
        /// <summary>
        /// 建议到货/完工日期
        /// </summary>
        public DateTime PlanFinishDate { get; set; }
        /// <summary>
        /// 计划标签
        /// </summary>
        public string MrpNote { get; set; }
        /// <summary>
        /// 生产车间
        /// </summary>
        public string PrdDeptId { get; set; }
        /// <summary>
        /// 计划员
        /// </summary>
        public string PlanerId { get; set; }
        /// <summary>
        /// 入库组织
        /// </summary>
        public string InStockOrgId { get; set; }
        /// <summary>
        /// 净需求
        /// </summary>
        public string DemandQty { get; set; }
        /// <summary>
        /// 计划订单量
        /// </summary>
        public string OrderQty { get; set; }
        /// <summary>
        /// 需求日期
        /// </summary>
        public DateTime DemandDate { get; set; }
        /// <summary>
        /// 数据来源
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string DocumentStatus { get; set; }
        /// <summary>
        /// 业务状态
        /// </summary>
        public string ReleaseStatus { get; set; }
        /// <summary>
        /// 需求单据编号
        /// </summary>
        public string SaleOrderNo { get; set; }
        /// <summary>
        /// 需求单据行号
        /// </summary>
        public string SaleOrderEntrySeq { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public DateTime ApproveDate { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string ApproverId { get; set; }
        /// <summary>
        /// 需求来源
        /// </summary>
        public string DemandType { get; set; }
        /// <summary>
        /// 需求单据内码
        /// </summary>
        public string SaleOrderId { get; set; }
        /// <summary>
        /// 需求单据分录内码
        /// </summary>
        public string SaleOrderEntryId { get; set; }
        /// <summary>
        /// 委托组织
        /// </summary>
        public string EntrustOrgId { get; set; }
        /// <summary>
        /// 产品小类
        /// </summary>
        public string SMALLID { get; set; }
        /// <summary>
        /// 产品大类
        /// </summary>
        public string PARENTSMALLID { get; set; }
        /// <summary>
        /// 事业部
        /// </summary>
        public string BUSINESSDIVISIONID { get; set; }
        /// <summary>
        /// 销售日期
        /// </summary>
        public string PENYSalDatetime { get; set; }
        /// <summary>
        /// 发送MES
        /// </summary>
        public string IsSendMes { get; set; }
        /// <summary>
        /// 图纸ID
        /// </summary>
        public long FDrawingRecordId { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string FPENYCustomerNumber { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string FPENYCustomerName { get; set; }
        /// <summary>
        /// 客户ERP物料号
        /// </summary>
        public string FCustMaterialNo { get; set; }
        /// <summary>
        /// 客户物料编号
        /// </summary>
        public string FCustItemNo { get; set; }
        /// <summary>
        /// 客户物料名称
        /// </summary>
        public string FCustItemName { get; set; }
        /// <summary>
        /// 含税单价
        /// </summary>
        public decimal FPENYPRICE { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal FPENYAMOUNT { get; set; }
        /// <summary>
        /// 客户采购订单号
        /// </summary>
        public string CustPurchaseNo { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SalCreatorName { get; set; }
        /// <summary>
        /// 先导订单标识
        /// </summary>
        public string CustOrderDetailId { get; set; }
        /// <summary>
        /// 项目号
        /// </summary>
        public string ProjectNo { get; set; }

        /// <summary>
        /// 长
        /// </summary>
        public decimal LENGTH { get; set; }
        /// <summary>
        /// 宽
        /// </summary>
        public decimal WIDTH { get; set; }
        /// <summary>
        /// 高
        /// </summary>
        public decimal HEIGHT { get; set; }
        /// <summary>
        /// 表面积
        /// </summary>
        public decimal PENYSurfaceArea { get; set; }
        /// <summary>
        /// 零件重量
        /// </summary>
        public decimal PENYWeight { get; set; }
        /// <summary>
        /// 体积
        /// </summary>
        public decimal VOLUME { get; set; }
        /// <summary>
        /// 尺寸单位
        /// </summary>
        public string VOLUMEUNITNAME { get; set; }
        /// <summary>
        /// 重量单位
        /// </summary>
        public string WEIGHTUNITNAME { get; set; }
        /// <summary>
        /// 材质
        /// </summary>
        public string Textures { get; set; }

        /// <summary>
        /// 图纸文件URL
        /// </summary>
        public string DrawingFileUrl { get; set; } = string.Empty;
        /// <summary>
        /// 加工设备名称，例如四轴T5
        /// </summary>
        public string MachineName { get; set; } = string.Empty;
        /// <summary>
        /// 招标机型
        /// </summary>
        public string PlanTenderType { get; set; } = string.Empty;

        /// <summary>
        /// 表面处理，例如磨砂
        /// </summary>
        public string SurfaceTreatment { get; set; } = string.Empty;
        /// <summary>
        /// 类型，值为棒料或柱料
        /// </summary>
        public string BlankType { get; set; } = string.Empty;
        /// <summary>
        /// 热处理，例如淬火
        /// </summary>
        public string HeatTreatment { get; set; } = string.Empty;
        /// <summary>
        /// 最终成本
        /// </summary>
        public decimal FinalCost { get; set; }
        /// <summary>
        /// STL文件URL
        /// </summary>
        public string StlFileUrl { get; set; } = string.Empty;
        /// <summary>
        /// 缩略图文件URL
        /// </summary>
        public string ThumbnailFileUrl { get; set; } = string.Empty;
        /// <summary>
        /// 胚料类型
        /// </summary>
        public string PartTypeName { get; set; }
    }
    public class PlanOrderSplitEntity
    {
        /// <summary>
        /// 投放类型 1生产2采购
        /// </summary>
        public int ReleaseType { get; set; }
        /// <summary>
        /// 计划订单ID
        /// </summary>
        public long PlanPkId { get; set; }
        /// <summary>
        /// 计划订单单据编号
        /// </summary>
        public string PlanBillNo { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }
        public bool IsMulti { get; set; }
        /// <summary>
        /// 生产组织
        /// </summary>
        public string ApplicationOrgId { get; set; }
        /// <summary>
        /// 生产车间
        /// </summary>
        public string WorkShopID { get; set; }
        /// <summary>
        /// 计划组
        /// </summary>
        public string WorkGroupId { get; set; }
        /// <summary>
        /// 计划员
        /// </summary>
        public string PlannerID { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public DateTime Date { get; set; }
        public List<DetailEntity> DetailEntity { get; set; }
    }
    public class DetailEntity
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialId { get; set; }
        /// <summary>
        /// 生产车间
        /// </summary>
        public string WorkShopID { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 计划开工时间
        /// </summary>
        public DateTime PlanStartDate { get; set; }
        /// <summary>
        /// 计划完工时间
        /// </summary>
        public DateTime PlanFinishDate { get; set; }
        /// <summary>
        /// BOM版本
        /// </summary>
        public string BomNumber { get; set; }
        public long BomId { get; set; }
        public string ProcessID { get; set; }
        /// <summary>
        /// 销售订单分录内码
        /// </summary>
        public string SaleOrderEntryId { get; set; }
        /// <summary>
        /// 销售订单ID
        /// </summary>
        public string SaleOrderId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string MemoItem { get; set; }
        /// <summary>
        /// 需求单据
        /// </summary>
        public string SaleOrderNo { get; set; }
        /// <summary>
        /// 需求单据行号
        /// </summary>
        public int SaleOrderEntrySeq { get; set; }
        /// <summary>
        /// 销售单价
        /// </summary>
        public decimal PenyPrice { get; set; }
        /// <summary>
        /// 销售要货日期
        /// </summary>
        public DateTime PenySalDatetime { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SalCreatorName { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string StockId { get; set; }

        /// <summary>
        /// 事业部
        /// </summary>
        public string BUSINESSDIVISIONID { get; set; }
        /// <summary>
        /// 计划需求数量
        /// </summary>
        public decimal PlanDemandQty { get; set; } = 0;

        public List<BomInfoEntity> BomInfoEntity { get; set; }
    }
    public class BomInfoEntity
    {
        /// <summary>
        /// 上级坯料规格(编码)
        /// </summary>
        public string ParentSpecification { get; set; }
        /// <summary>
        /// 坯料编码
        /// </summary>
        public string BomMaterialID { get; set; }
        public long ChildMaterialID { get; set; }
        /// <summary>
        /// 坯料名称/长x宽x高
        /// </summary>
        public string FBBomName { get; set; }
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
        public decimal VOLUME { get; set; }
        /// <summary>
        /// 材质
        /// </summary>
        public string Textures { get; set; }
        /// <summary>
        /// 材型
        /// </summary>
        public string MaterialType { get; set; }
        /// <summary>
        /// 重量单位
        /// </summary>
        public string WeightUnitid { get; set; }
        /// <summary>
        /// 尺寸单位
        /// </summary>
        public string VolumeUnitid { get; set; }
        /// <summary>
        /// 用量
        /// </summary>
        public decimal mtlQty { get; set; }
        /// <summary>
        /// 用量分母
        /// </summary>
        public decimal denominatorQty { get; set; }
        /// <summary>
        /// 产品小类
        /// </summary>
        public string SMALLID { get; set; }
        /// <summary>
        /// 产品大类
        /// </summary>
        public string PARENTSMALLID { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string EntryNote { get; set; }
    }
    public class QuoteRequestDtocs
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SalesOrderNo { get; set; }
        /// <summary>
        /// 销售订单行号
        /// </summary>
        public string SalesOrderLineNo { get; set; }
        /// <summary>
        /// 图纸ID
        /// </summary>
        public int DrawingRecordId { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        public string DrawingNumber { get; set; } = string.Empty;
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; } = 1m;
        /// <summary>
        /// 长
        /// </summary>
        public decimal Long { get; set; } = 1m;
        /// <summary>
        /// 宽
        /// </summary>
        public decimal Width { get; set; } = 1m;
        /// <summary>
        /// 高
        /// </summary>
        public decimal Height { get; set; } = 1m;
        /// <summary>
        /// 表面积
        /// </summary>
        public double SurfaceArea { get; set; }
        /// <summary>
        /// 零件体积
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 零件重量
        /// </summary>
        public double Weight { get; set; }


        /// <summary>
        /// 胚料类型
        /// </summary>
        public string BlankType { get; set; }
        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;
        /// <summary>
        /// 表处类型
        /// </summary>
        public string SurfaceTreatment { get; set; }
        public string HeatTreatment { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string MachineName { get; set; } = string.Empty;
        /// <summary>
        /// 设备Id
        /// </summary>
        public int MachineId { get; set; }
        /// <summary>
        /// 总成本
        /// </summary>
        public decimal FinalCost { get; set; }
        /// <summary>
        /// 图纸文件pdf
        /// </summary>
        public string DrawingFileUrl { get; set; }
        /// <summary>
        /// stl文件
        /// </summary>
        public string StlFileUrl { get; set; }
        /// <summary>
        /// 缩略图文件
        /// </summary>
        public string ThumbnailFileUrl { get; set; }


        /// <summary>
        /// 胚料成本单价
        /// </summary>
        public decimal MaterialPrice { get; set; }
        /// <summary>
        /// CNC加工工时
        /// </summary>
        public double CuttingHours { get; set; }
        /// <summary>
        /// CNC加工单价
        /// </summary>
        public decimal MachinePrice { get; set; }
        /// <summary>
        /// 夹数
        /// </summary>
        public int ClampingCount { get; set; }
        /// <summary>
        /// 人工成本
        /// </summary>
        public decimal ManualCost { get; set; }
        /// <summary>
        /// 表处单价成本
        /// </summary>
        public decimal SurfaceCost { get; set; }
        /// <summary>
        /// 胚料类型
        /// </summary>
        public string PartTypeName { get; set; }

		//
		// 摘要:
		//     长
		public double Dimension1 { get; set; }
		//
		// 摘要:
		//     宽
		public double Dimension2 { get; set; }
		//
		// 摘要:
		//     高
		public double Dimension3 { get; set; }
	}
}
