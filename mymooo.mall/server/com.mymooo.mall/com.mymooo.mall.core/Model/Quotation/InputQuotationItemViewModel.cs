using com.mymooo.mall.core.Attributes.Validation;
using com.mymooo.mall.core.Utils.JsonConverter;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace com.mymooo.mall.core.Model.Quotation
{
	public partial class InputQuotationItemViewModel
	{
		private string _customProductCode = string.Empty;
        [JsonConverter(typeof(ProductModelConverter))]
        [JsonPropertyName("CustomCode")]
        public string CustomProductCode {
			get
			{
				return  _customProductCode == null ? "" : MyRegex().Replace(_customProductCode, "");
			} 
			set
			{
				_customProductCode = value;
			}
		}

		private string _customProductName = string.Empty;
        [JsonConverter(typeof(NameConverter))]
        [JsonPropertyName("CustItemName")]
        public string CustomProductName
        {
            get
            {
                return _customProductName == null ? "" : MyRegex().Replace(_customProductName, "");
            }
            set
            {
                _customProductName = value;
            }
        }



        [Range(1, 99999, ErrorMessage = "数量超出范围")]
		public int Quantity { get; set; }

		public decimal? UnitPriceWithTax { get; set; }

		[Range(0, 1000, ErrorMessage = "销售交期不能超过1000天")]
		public int? DispatchDays { get; set; }

		private string _remark = string.Empty;
		public string Remark
        {
            get
            {
                return _remark == null ? "" : MyRegex().Replace(_remark, "");
            }
            set
            {
                _remark = value;
            }
        }

		public required ProductSeriesViewModel Product { get; set; }


		private string _insideRemark = string.Empty;
		public string InsideRemark
        {
            get
            {
                return _insideRemark == null ? "" : MyRegex().Replace(_insideRemark, "");
            }
            set
            {
                _insideRemark = value;
            }
        }

		private string  _purchaseRemark = string.Empty;
        /// <summary>
        /// 采购备注
        /// </summary>
        public string PurchaseRemark
        {
            get
            {
                return _purchaseRemark == null ? "" : MyRegex().Replace(_purchaseRemark, "");
            }
            set
            {
                _purchaseRemark = value;
            }
        }

        public string ProjectNo { get; set; } = string.Empty;

		/// <summary>
		/// 客户料号
		/// </summary>
		private string _custItemNo = string.Empty;
		public string CustItemNo
        {
            get
            {
                return _custItemNo == null ? "" : MyRegex().Replace(_custItemNo, "");
            }
            set
            {
                _custItemNo = value;
            }
        }

		/// <summary>
		/// 库存管理特征
		/// </summary>
        private string _stockFeatures = string.Empty;
		public string StockFeatures
        {
            get
            {
                return _stockFeatures == null ? "" : MyRegex().Replace(_stockFeatures, "");
            }
            set
            {
                _stockFeatures = value;
            }
        }

		/// <summary>
		/// 报价导入模板填写的小类Id
		/// </summary>
		[ProductSmall(true, true)]
		public long? ProductSmallClassId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public long? LargeClassId { get; set; }

		/// <summary>
		/// 事业部ID
		/// </summary>
		public string BusinessDivisionId { get; set; } = string.Empty;

		/// <summary>
		/// 事业部名称
		/// </summary>
        private string _businessDivisionName = string.Empty;
		public string BusinessDivisionName
        {
            get
            {
                return _businessDivisionName== null ? "" : MyRegex().Replace(_businessDivisionName, "");
            }
            set
            {
                _businessDivisionName = value;
            }
        }
    
        /// <summary>
        /// 供货组织id
        /// </summary>
        public long SupplyOrgId { get; set; }

		/// <summary>
		/// 供货组织编码
		/// </summary>
		public string SupplyorgNumber { get; set; } = string.Empty;

		/// <summary>
		/// 供货组织名称
		/// </summary>
        private string _supplyOrgName = string.Empty;
		public string SupplyOrgName
        {
            get
            {
                return _supplyOrgName == null ? "" : MyRegex().Replace(_supplyOrgName, "");
            }
            set
            {
                _supplyOrgName = value;
            }
        }

        // 报价单型号附带的附件文件名,可能多个.
        public string AttaFilesName { get; set; } = string.Empty;

		// 附件文件的相对路径
		public string AttaPath { get; set; } = string.Empty;

		/// <summary>
		/// 供应商编码
		/// </summary>
		public string SupplierCode { get; set; } = string.Empty;



        /// <summary>
        /// 客户期望交期
        /// </summary>
        public int? DesireDeliveryDays { get; set; }

        /// <summary>
        /// 客户期望价格
        /// </summary>
        public decimal? DesirePrice { get; set; }
        /// <summary>
        /// 工厂/库位
        /// </summary>
        private string _storage = string.Empty;
        public string Storage
        {
            get
            {
                return _storage == null ? "" : MyRegex().Replace(_storage, "");
            }
            set
            {
                _storage = value;
            }
        }


        [GeneratedRegex(@"\s")]
        private static partial Regex MyRegex();
    }
}
