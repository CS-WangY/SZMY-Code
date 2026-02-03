using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.product.selection.SelectionModel;

namespace mymooo.k3cloud.core.SubReqModel
{
    /// <summary>
    /// 委外订单请求参数
    /// </summary>
    public class SubReqOrderRequests
    {
        /// <summary>
        /// 委外订单id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string? BillNo { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string? BillType { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public string? Date { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string? DocumentStatus { get; set; }
        /// <summary>
        /// 委外组织
        /// </summary>
        public string? SubOrgId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 用料清单展开
        /// </summary>
        public string? PPBOMType { get; set; }
        /// <summary>
        /// 委外订单明细
        /// </summary>
        public List<SubReqOrderEntity>? Details { get; set; }
    }
    /// <summary>
    /// 委外订单明细
    /// </summary>
    public class SubReqOrderEntity
    {
        /// <summary>
        /// 产品类型
        /// </summary>
        public string? FProductType { get; set; }
        /// <summary>
        /// 物料id
        /// </summary>
        public string? FMaterialId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string? FMaterialNumber { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string? FMaterialName { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string? FSpecification { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string? FUnit { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal FQty { get; set; }

        /// <summary>
        /// 计划开工时间
        /// </summary>
        public string? FPlanStartDate { get; set; }
        /// <summary>
        /// 计划完工时间
        /// </summary>
        public string? FPlanFinishDate { get; set; }
        /// <summary>
        /// BOM版本
        /// </summary>
        public string? FBom { get; set; }
        /// <summary>
        /// BOM信息
        /// </summary>
        public List<SubReqOrderBomChild>? BomChildren { get; set; }

        /// <summary>
        /// 来源单据ID
        /// </summary>
        public string? FSrcBillId { get; set; }
        /// <summary>
        /// 来源单据分录内码
        /// </summary>
        public string? FSrcBillEntryId { get; set; }
        /// <summary>
        /// 销售订单ID
        /// </summary>
        public string? FSaleOrderId { get; set; }
        /// <summary>
        /// 销售订单分录内码
        /// </summary>
        public string? FSaleOrderEntryId { get; set; }
        /// <summary>
        /// 采购订单ID
        /// </summary>
        public string? FPurOrderId { get; set; }
        /// <summary>
        /// 采购订单分录内码
        /// </summary>
        public string? FPurOrderEntryId { get; set; }

        /// <summary>
        /// 排产状态
        /// </summary>
        public string? FScheduleStatus { get; set; }
        /// <summary>
        /// 领料状态
        /// </summary>
        public string? FPickMtrlStatus { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? FDescription1 { get; set; }

        /// <summary>
        /// 源单类型
        /// </summary>
        public string? FSrcBillType { get; set; }
        /// <summary>
        /// 源单编号
        /// </summary>
        public string? FSrcBillNo { get; set; }
        /// <summary>
        /// 源单分录行号
        /// </summary>
        public string? FSrcBillEntrySeq { get; set; }
        /// <summary>
        /// 需求单据
        /// </summary>
        public string? FSALEORDERNO { get; set; }
        /// <summary>
        /// 需求单据行号
        /// </summary>
        public string? FSaleOrderEntrySeq { get; set; }

        /// <summary>
        /// 产品小类
        /// </summary>
        public string? FSMALLID { get; set; }
        /// <summary>
        /// 产品大类
        /// </summary>
        public string? FPARENTSMALLID { get; set; }
        /// <summary>
        /// 事业部
        /// </summary>
        public string? FBUSINESSDIVISIONID { get; set; }
    }
    /// <summary>
    /// BOM信息
    /// </summary>
    public class SubReqOrderBomChild
    {
        /// <summary>
        /// 物料id
        /// </summary>
        public string? FMaterialId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialNumber { get; set; } = string.Empty;
        /// <summary>
        /// 物料名称
        /// </summary>
        public string? FMaterialName { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string? FSpecification { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string? FUnit { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal FQty { get; set; }
        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 3d地址
        /// </summary>
        public string? ThreeUrl { get; set; }

        /// <summary>
        /// 3d版本
        /// </summary>
        public string? ThreeVer { get; set; }

        /// <summary>
        /// pdf url
        /// </summary>
        public string? PlaneUrl { get; set; }
        /// <summary>
        /// 参数选择值集合
        /// </summary>
        public List<ProductParameterValueResponse.ProductParameterValue>? ParameterValues { get; set; }
    }
}
