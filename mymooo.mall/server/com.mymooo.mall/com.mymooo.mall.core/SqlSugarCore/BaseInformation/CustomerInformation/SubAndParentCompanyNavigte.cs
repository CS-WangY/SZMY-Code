using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
	public partial class SubAndParentCompany
	{
		/// <summary>
		/// 父级公司
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(ParentCompanyId), nameof(Company.Id))]
		public Company? ParentCompany { get; set; }

		/// <summary>
		/// 子公司
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(CompanyId), nameof(Company.Id))]
		public Company? SubCompany { get; set; }
	}
}
