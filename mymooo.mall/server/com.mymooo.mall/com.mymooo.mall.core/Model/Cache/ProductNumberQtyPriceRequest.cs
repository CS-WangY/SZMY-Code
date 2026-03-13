namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 
	/// </summary>
	public class ProductNumberQtyPriceRequest
	{
		private string productNumber = string.Empty;

		/// <summary>
		/// 产品型号
		/// </summary>
		public string ProductNumber
		{
			get => productNumber;
			set
			{
				this.productNumber = value;
				this.Id = value.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// 数量
		/// </summary>
		public decimal Qty { get; set; }

	}
}
