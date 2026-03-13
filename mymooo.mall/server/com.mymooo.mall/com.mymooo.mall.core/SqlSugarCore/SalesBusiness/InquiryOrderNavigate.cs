using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	public partial class InquiryOrder
	{
		[Navigate(NavigateType.OneToMany, nameof(InquiryId))]
		public List<InquiryOrderDetail> Details { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(InquiryId), nameof(InquiryId))]
		public InquiryQuotationOrder QuotationOrder { get; set; }
	}
}
