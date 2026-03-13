using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
	public partial class Company
	{
		/// <summary>
		/// 父级公司
		/// </summary>
		[Navigate(NavigateType.OneToMany, nameof(SubAndParentCompany.ParentCompanyId), nameof(Id))]
		public List<SubAndParentCompany> ParentCompanys { get; set; } 

		/// <summary>
		/// 子公司
		/// </summary>
		[Navigate(NavigateType.OneToMany, nameof(SubAndParentCompany.CompanyId), nameof(Id))]
		public List<SubAndParentCompany> SubCompanys { get; set; }
	}
}
