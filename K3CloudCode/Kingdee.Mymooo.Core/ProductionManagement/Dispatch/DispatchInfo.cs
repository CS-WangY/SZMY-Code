using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    /// <summary>
    /// 派产准备Entity
    /// </summary>
    public class DispatchInfo
    {
        /// <summary>
        /// 派场订单零件信息
        /// </summary>
        public DispatchPart part{ get; set; }

        /// <summary>
        /// 派产订单零件加工生产信息
        /// </summary>
        public DispatchProcess process { get; set; }
        
        /// <summary>
        /// 派产订单BOM信息，仅ERP使用，MES系统不处理
        /// </summary>
        public DispatchBom bom { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 公司编码，比如顺德蚂蚁即MYZZ
        /// </summary>
        public string CompanyCode { get; set; }

    }

    /// <summary>
    /// 工序汇报Entity
    /// </summary>
    public class ProcessReportEntity
    {
        /// <summary>
        /// 工单编号
        /// </summary>
        public string WoNo { get; set; }

        /// <summary>
        /// 工单序号
        /// </summary>
        public string WoSeqNo { get; set; }

        /// <summary>
        /// 工序编号
        /// </summary>
        public string ProcessNo { get; set; }

        /// <summary>
        /// 加工顺序
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// 开工车间
        /// </summary>
        public string WorkWorkshop { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 使用设备
        /// </summary>
        public string UsageDevice { get; set; }

        /// <summary>
        /// 开工时间
        /// </summary>
        public DateTime WorkTime { get; set; }

        /// <summary>
        /// 完工时间
        /// </summary>
        public DateTime CompleteTime { get; set; }

        /// <summary>
        /// 运行时长（分钟）
        /// </summary>
        public int RunMinute { get; set; }

        /// <summary>
        /// 完工数量
        /// </summary>
        public int CompleteAmount { get; set; }

        /// <summary>
        /// Mes工序回传的主键唯一值
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 汇报类型：1正常汇报; 2返工; 3报废;
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// 公司编码，比如顺德蚂蚁即MYZZ
        /// </summary>
        public string CompanyCode { get; set; }
    }

    /// <summary>
    /// 工序汇报Entity（ERP传递给云平台）
    /// </summary>
    public class ProcessReportForCloudEntity
    {
        /// <summary>
        /// 销售订单编号
        /// </summary>
        public string SalesNumber { get; set; }

        /// <summary>
        /// 零件编号
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// 完工数量
        /// </summary>
        public int CompleteAmount { get; set; }     
    }

    /// <summary>
    /// 完工入库Entity
    /// </summary>
    public class CompleteInWarehouseEntity
    {
        /// <summary>
        /// 工单编号
        /// </summary>
        public string WoNo { get; set; }

        /// <summary>
        /// 完工入库时间
        /// </summary>
        public DateTime CompleteTime { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// 完工入库数量
        /// </summary>
        public int CompleteAmount { get; set; }

        /// <summary>
        /// 完工重量（KG）
        /// </summary>
        public decimal CompleteWeight { get; set; }

        /// <summary>
        /// 流程工作时长（分钟）
        /// </summary>
        public int WorkflowMinute { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 公司编码，比如顺德蚂蚁即MYZZ
        /// </summary>
        public string CompanyCode { get; set; }
    }

    public class ErpToMesForOrder
    {
        /// <summary>
        /// 工单编号
        /// </summary>
        public string woNo { get; set; }

        /// <summary>
        /// 工单序号
        /// </summary>
        public string woSeqNo { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        public string drawNum { get; set; }

        /// <summary>
        /// 图号版本
        /// </summary>
        public string drawNumVersion { get; set; }

        /// <summary>
        /// 工单数量
        /// </summary>
        public int orderNum { get; set; }

        /// <summary>
        /// 交期
        /// </summary>
        public string deadLine { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }
    }

    public class WorkshopEntity
    {
        public List<Workshop> workshop { get; set; } = new List<Workshop>();

        /// <summary>
        /// 公司编码
        /// </summary>
        public string CompanyCode { get; set; }

    }

    public class Workshop
    {
        /// <summary>
        /// 车间编码
        /// </summary>
        public string WorkshopCode { get; set; }

        /// <summary>
        /// 车间名称
        /// </summary>
        public string WorkshopName { get; set; }

        /// <summary>
        /// 公司编码，比如顺德蚂蚁即MYZZ
        /// </summary>
        public string CompanyCode { get; set; } = "MYZZ";

    }

    public class ProcessCancelEntity
    {
        /// <summary>
        /// 工序主键唯一值
        /// </summary>
        public string[] Id { get; set; }

        /// <summary>
        /// 公司编码，比如顺德蚂蚁即MYZZ
        /// </summary>
        public string CompanyCode { get; set; } = "MYZZ";

    }

}
