namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批详情
	/// </summary>
	public class ApprovalDetails
	{
		/// <summary>
		/// 
		/// </summary>
		public ApprovalDetails()
		{
			this.Contents = [];
		}

		/// <summary>
		/// 控件数据集合
		/// </summary>
		public List<dynamic> Contents { get; set; }
	}
}
