using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation
{
	public partial class ManagementUserInfo
	{

		[Navigate(NavigateType.OneToOne, nameof(UserId))]
		public ManagementUserInfo UserInfo { get; set; }
	}
}
