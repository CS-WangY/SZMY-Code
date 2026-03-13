using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class CappQuotationOrderDto
    {
        /// <summary>
        /// 报价单号
        /// </summary>
        public string? QuotationOrderNo { get; set; }
        /// <summary>
        /// 报价单行号
        /// </summary>
        public int QuotationOrderLineNo { get; set; }
        /// <summary>
        /// 图号
        /// </summary>
        public string? DrawingNumber { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 零件分类类型
        /// </summary>
        public string? PartType { get; set; }
        /// <summary>
        /// 长
        /// </summary>
        public double Dimension1 { get; set; }
        /// <summary>
        /// 宽
        /// </summary>
        public double Dimension2 { get; set; }
        /// <summary>
        /// 高
        /// </summary>
        public double Dimension3 { get; set; }
        /// <summary>
        /// 材料ID
        /// </summary>
        public int MaterialId { get; set; }
        /// <summary>
        /// 材料名称
        /// </summary>
        public string? MaterialName { get; set; }
        /// <summary>
        /// 材料价格
        /// </summary>
        public double MaterialPrice { get; set; }
        /// <summary>
        /// 材料密度
        /// </summary>
        public double MaterialDensity { get; set; }
        /// <summary>
        /// 材料切削系数
        /// </summary>
        public double MaterialCuttingHourFactor { get; set; }
        /// <summary>
        ///零件体积
        /// </summary>
        public double BlankVolume { get; set; }
        /// <summary>
        /// 零件重量
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// 表面积
        /// </summary>
        public double SurfaceArea { get; set; }
        /// <summary>
        /// 坯料重量
        /// </summary>
        public double BlankWeight { get; set; }
        /// <summary>
        /// 坯料单价成本
        /// </summary>
        public double BlankSingleBaseCost { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string? MachineName { get; set; }
        /// <summary>
        /// 设备单价
        /// </summary>
        public double MachineUnitPrice { get; set; }
        /// <summary>
        /// 加工工时
        /// </summary>
        public double OriginCuttingHours { get; set; }
        /// <summary>
        /// 加工费
        /// </summary>
        public double ProcessCost { get; set; }
        /// <summary>
        /// 换产成本
        /// </summary>
        public double ReplaceCost { get; set; }
        /// <summary>
        /// 夹数
        /// </summary>
        public int ClampingCount { get; set; }
        /// <summary>
        /// 装夹成本
        /// </summary>
        public double FixtureCost { get; set; }
        /// <summary>
        /// 人工成本
        /// </summary>
        public double ManualCost { get; set; }
        /// <summary>
        /// 后处理单价
        /// </summary>
        public double AfterProcessSinglePrice { get; set; }
        /// <summary>
        /// 后处理单价成本
        /// </summary>
        public double AfterSingleBaseCost { get; set; }
        /// <summary>
        /// 其他工序成本
        /// </summary>
        public double OtherTechCost { get; set; }
        /// <summary>
        /// 其他损耗成本
        /// </summary>
        public double OtherLossCost { get; set; }
        /// <summary>
        /// 管理费
        /// </summary>
        public double ManagementFee { get; set; }
        /// <summary>
        /// 利润
        /// </summary>
        public double Profit { get; set; }
        /// <summary>
        /// 利润率
        /// </summary>
        public double ProfitRatio { get; set; }
        /// <summary>
        /// 税费
        /// </summary>
        public double TaxFee { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public double SingleCost { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public double TotalCost { get; set; }
        /// <summary>
        /// 表处ID
        /// </summary>
        public int SurfaceTreatmentId { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public int MachineId { get; set; }
    }

    public class CappQuotationRequest
    {
        /// <summary>
        /// 报价单号
        /// </summary>
        public string? QuotationOrderNo { get; set; }
        /// <summary>
        /// 报价单行号
        /// </summary>
        public int QuotationOrderLineNo { get; set; }

        public string? DrawingNumber { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public decimal TotalSingle { get; set; }
    }
}
