using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	public partial class InquiryOrderDetail
	{
		[Navigate(NavigateType.OneToOne, nameof(InquiryId))]
		public InquiryOrder InquiryOrder { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(InquiryDetailId), nameof(InquiryDetailId))]
		public InquiryQuotationOrderDetail QuotationOrderDetail { get; set; }
	}
}
