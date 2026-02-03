namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 
	/// </summary>
	public class SelectOption
	{
		/// <summary>
		/// 选项key
		/// </summary>
		public string Key { get; set; } = string.Empty;

		/// <summary>
		/// 选项值
		/// </summary>
		public List<OptionValue> Value { get; set; } = [];
	}

	/// <summary>
	/// 选项值
	/// </summary>
	public class OptionValue
	{
		/// <summary>
		/// 选项值
		/// </summary>
		public string Text { get; set; } = string.Empty;

	}
}
