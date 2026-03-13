using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class ConfirmQuotationItemViewModel
    {
        public ConfirmQuotationItemViewModel()
        {
            this.QtyDiscount = 100;
            this.LevelDiscount = 100;
            this.ShortNumber = "";
            this.PriceType = "1";
        }

        public string PriceSourceName { get; set; } = string.Empty;
        public PriceSource? PriceSource { get; set; }

        public string DeliverySourceName { get; set; } = string.Empty;
        public DeliverySource? DeliverySource { get; set; }

        public string CustomCode { get; set; } = string.Empty;

        public string CustItemName { get; set; } = string.Empty;

        public ProductSeriesViewModel Product { get; set; } = new ProductSeriesViewModel();

        public int Quantity { get; set; }

        public string? Remark { get; set; } 

        public int? InputDispatchDays { get; set; }

        public int? SystemDispatchDays { get; set; }

        public bool SelectedSystemDispatchDays { get; set; }

        public decimal? InputUnitPriceWithTax { get; set; }

        public decimal? SystemUnitPriceWithTax { get; set; }

        public bool SelectedSystemUnitPriceWithTax { get; set; }

        public long ProductEngineerId { get; set; }
        public string ProductEngineerName { get; set; } = string.Empty;
        public string ProductManagerName { get; set; } = string.Empty;
        public long ProductManagerId { get; set; }

        public decimal? SupplyUnitPriceWithTax { get; set; }

        public decimal? SubtotalWithTax
        {
            get
            {
                if (InputUnitPriceWithTax != null && InputUnitPriceWithTax > 0)
                {

                    return decimal.Round((decimal)InputUnitPriceWithTax * Quantity, 2, System.MidpointRounding.AwayFromZero);

                }

                if (SystemUnitPriceWithTax != null && SystemUnitPriceWithTax > 0)
                {

                    return decimal.Round((decimal)SystemUnitPriceWithTax * Quantity, 2, System.MidpointRounding.AwayFromZero);
                }

                return null;
            }
        }

        public byte CategoryType { get; set; }

        /// <summary>
        /// 数量折扣
        /// </summary>
        public decimal QtyDiscount { get; set; }

        /// <summary>
        /// 客户等级折扣
        /// </summary>
        public decimal LevelDiscount { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public decimal OrgPrice { get; set; }
        public string InsideRemark { get; set; } = string.Empty;

        /// <summary>
        /// 采购备注
        /// </summary>
        public string PurchaseRemark { get; set; } = string.Empty;

        /// <summary>
        /// 产品小类id
        /// </summary>
        public ProductSmallClass? ProductSmallClass { get; set; }
        

        /// <summary>
        /// 是否历史价格
        /// </summary>
        public bool IsHistory { get; set; }

        /// <summary>
        /// 简易型号
        /// </summary>
        public string ShortNumber { get; set; } = string.Empty;

        /// <summary>
        /// 采购价格形式 1 矩阵 2 全型号 3 简易型号 4 简易型号+矩阵
        /// </summary>
        public string PriceType { get; set; } = string.Empty;

        public string ProjectNo { get; set; } = string.Empty;

        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustItemNo { get; set; } = string.Empty;

        /// <summary>
        /// 库存管理特征
        /// </summary>
        public string StockFeatures { get; set; } = string.Empty;

        /// <summary>
        /// 供货组织Id
        /// </summary>
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 供货组织名称
        /// </summary>
        public string SupplyOrgName { get; set; } = string.Empty;

        /// <summary>
        /// 是否是模板导入指定的小类的Id
        /// </summary>
        public bool IsTempSmallClass { get; set; } = false;

        /// <summary>
        /// 历史最低报价(最近一年)
        /// </summary>
        public decimal? QuoLPrice { get; set; }
        /// <summary>
        /// 历史最低成交价(最近一年)
        /// </summary>
        public decimal? DealLPrice { get; set; }

        /// <summary>
        /// 附件文件名,可能多个
        /// </summary>
        public string AttaFilesName { get; set; } = string.Empty;

        public decimal? DesirePrice { get; set; }

        public int? DesireDeliveryDays { get; set; }

        public string Storage {  get; set; } = string.Empty;

        /// <summary>
        /// 图纸附件文件名(多个用|分开)
        /// </summary>
        public string PdmFilesName { get; set; } = "";
    }

}
