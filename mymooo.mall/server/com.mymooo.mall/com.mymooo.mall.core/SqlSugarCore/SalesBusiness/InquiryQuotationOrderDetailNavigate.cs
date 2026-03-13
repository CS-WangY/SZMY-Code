using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	public partial class InquiryQuotationOrderDetail
	{
		/// <summary>
		/// 询价单明细
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(InquiryDetailId))]
		public InquiryOrderDetail InquiryDetail { get; set; }

		/// <summary>
		/// 询价单主表 拆分表
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(QuotationId))]
		public InquiryQuotationOrder QuotationOrder { get; set; }
	}
}
