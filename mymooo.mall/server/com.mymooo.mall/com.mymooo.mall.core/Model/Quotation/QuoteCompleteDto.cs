using mymooo.core.Attributes.Redis;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.core.Model.Quotation
{

    [RedisKey("mymooo-resolve-part", databaseId: 3)]
    public class FbQuotationCacheDto
    {
        /// <summary>
        /// 报价成功/失败
        /// </summary>
      
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        [RedisPrimaryKey]
        public string? SalesOrderNo { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailFileUrl { get; set; }

        /// <summary>
        /// 图纸ID
        /// </summary>
        public int DrawingRecordId { get; set; }

        /// <summary>
        /// 图号
        /// </summary>
        [RedisMainField]
        public string DrawingNumber { get; set; } = string.Empty;

        /// <summary>
        /// 图纸地址
        /// </summary>
        public string? DrawingFileUrl { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        //[JsonIgnore]
        public decimal Qty { get; set; } = 1;
        /// <summary>
        /// 零件类型
        /// </summary>
        public string PartType { get; set; } = string.Empty;

        /// <summary>
        /// 零件类型名称
        /// </summary>
        public string PartTypeName { get; set; } = string.Empty;
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
        /// 
        /// </summary>
        public double Long { get { return Dimension1; } set { Dimension1 = value; } }

        /// <summary>
        /// 
        /// </summary>
        public double Width { get { return Dimension2; } set { Dimension2 = value; } }

        /// <summary>
        ///
        /// </summary>
        public double Height { get { return Dimension3; } set { Dimension3 = value; }  }

        public int MaterialId { get; set; }
        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;

        /// <summary>
        /// 材料价格
        /// </summary>
        public decimal MaterialPrice { get; set; }

        /// <summary>
        /// 材料密度
        /// </summary>
        public decimal MaterialDensity { get; set; }

        /// <summary>
        /// 材料切削系数
        /// </summary>
        public decimal MaterialCuttingHourFactor { get; set; }

        /// <summary>
        /// 零件体积
        /// </summary>
        public double Volume { get; set; }

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
        /// 坯料单件成本
        /// </summary>
        public decimal BlankSingleBaseCost { get; set; }

        /// <summary>
        /// 坯料费用占比
        /// </summary>
        public decimal BlankCostRatio { get; set; }

        /// <summary>
        /// 坯料--长
        /// </summary>
        public double BlankLong { get; set; }
        /// <summary>
        /// 坯料--宽
        /// </summary>
        public double BlankWidth { get; set; }
        /// <summary>
        /// 坯料--高
        /// </summary>
        public double BlankHeight { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public int MachineId { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// 设备单价
        /// </summary>
        public decimal MachinePrice { get; set; }

        /// <summary>
        /// 原始加工工时(未乘以材质系数)
        /// </summary>
        public double OriginCuttingHours { get; set; }


        /// <summary>
        /// 加工工时
        /// </summary>
        public double CuttingHours { get; set; }

        /// <summary>
        /// 加工费
        /// </summary>
        public decimal ProcessCost { get; set; }


        /// <summary>
        /// 加工费用占比
        /// </summary>
        public decimal ProcessCostRatio { get; set; }

        /// <summary>
        /// 换产成本
        /// </summary>
        public decimal ReplaceCost { get; set; }

        /// <summary>
        /// 夹数
        /// </summary>
        public int ClampingCount { get; set; }

        /// <summary>
        /// 装夹成本
        /// </summary>
        public decimal FixtureCost { get; set; }

        /// <summary>
        /// 人工成本
        /// </summary>
        public decimal ManualCost { get; set; }

        /// <summary>
        /// 人工费用占比
        /// </summary>
        public decimal ManualCostRatio { get; set; }

        /// <summary>
        /// 后处理单价
        /// </summary>
        public decimal AfterProcessSinglePrice { get; set; }

        /// <summary>
        /// 后处理单件成本
        /// </summary>
        public decimal AfterSingleBaseCost { get; set; }

        /// <summary>
        /// 其它工序成本
        /// </summary>
        public decimal OtherTechCost { get; set; }

        /// <summary>
        /// 其它损耗成本
        /// </summary>
        public decimal OtherLossCost { get; set; }

        /// <summary>
        /// 后处理费用占比
        /// </summary>
        public decimal AfterProcessCostRatio { get; set; }

        /// <summary>
        /// 管理费
        /// </summary>
        public decimal ManagementFee { get; set; }

        /// <summary>
        /// 管理费率
        /// </summary>
        public decimal ManagementFeeRatio { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// 利润率
        /// </summary>
        public decimal ProfitRatio { get; set; } = 30m;

        /// <summary>
        /// 税费
        /// </summary>
        public decimal TaxFee { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal SingleCost { get; set; }

        /// <summary>
        /// 表处类型Id
        /// </summary>
        public int? SurfaceTreatmentId { get; set; }
        /// <summary>
        /// 表处类型
        /// </summary>
        public string? SurfaceTreatment { get; set; }

        /// <summary>
        /// 表处单价
        /// </summary>
        public decimal SurfacePrice { get; set; }

        /// <summary>
        /// 表处单价成本
        /// </summary>
        public decimal SurfaceCost { get; set; }

        /// <summary>
        /// 表处费用占比(%)
        /// </summary>
        public decimal SurfaceRatio { get; set; }

        /// <summary>
        /// 总价
        /// </summary>
        public decimal TotalCost { get; set; }

    }
}
