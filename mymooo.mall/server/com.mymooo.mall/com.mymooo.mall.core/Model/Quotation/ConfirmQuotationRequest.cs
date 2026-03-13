using System.ComponentModel.DataAnnotations;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class ConfirmQuotationRequest
    {
        public long AddressId { get; set; }

        public Guid? CompanyId { get; set; }

        public string CompanyCode { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string InquiryNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "客户信息信息必填")]
        public InputQuotationBuyerViewModel? Buyer { get; set; }

        [StringLength(50, ErrorMessage = "客户采购单号长度不能超过50个字符")]
        public string CustomerPurchaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// 销售组织编码
        /// </summary>
        [Required(ErrorMessage = "销售组织编码信息必填")]
        [StringLength(10, ErrorMessage = "销售组织编码名称不能超过10个字符")]
        public string SalesOrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 销售组织名称
        /// </summary>
        [StringLength(100, ErrorMessage = "销售组织名称不能超过100个字符")]
        public string SalesOrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// 内部订单
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// 特价单
        /// </summary>
        public bool SpecialPrice { get; set; }

        /// <summary>
        /// 是否非标
        /// </summary>
        public bool IsNonStandard { get; set; }

        /// <summary>
        /// 上传附件的路径
        /// </summary>
        public string UploadPath { get; set; }

        [Required(ErrorMessage = "必须要有明细数据")]
        public required IEnumerable<InputQuotationItemViewModel> QuotationItems { get; set; }


    }

}
