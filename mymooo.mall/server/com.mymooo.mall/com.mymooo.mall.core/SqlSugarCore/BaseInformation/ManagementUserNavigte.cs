using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation
{
	public partial class ManagementUser
	{

		[Navigate(NavigateType.OneToOne, nameof(UserId))]
		public ManagementUserInfo UserInfo { get; set; }
	}
}
