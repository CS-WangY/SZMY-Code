using System.ComponentModel.DataAnnotations;

namespace com.mymooo.mall.core.Model.Cache
{
	/// <summary>
	/// 产品型号缓存
	/// </summary>
	public class ProductNumberIndex
	{
		private string number = string.Empty;
		private string mymoooNumber = string.Empty;
		private string mymoooProductName = string.Empty;

		/// <summary>
		/// 主键
		/// </summary>
		public string Id { get; private set; } = string.Empty;

		/// <summary>
		/// 产品型号
		/// </summary>
		[Required(ErrorMessage = "产品型号不能为空!")]
		[StringLength(500)]
		public string ProductNumber
		{
			get => number; set
			{
				number = value;
				this.Id = number.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 产品名称
		/// </summary>
		[Required(ErrorMessage = "产品名称不能为空!")]
		public string ProductName { get; set; } = string.Empty;

		/// <summary>
		/// 产品小类Id
		/// </summary>
		public long SmallId { get; set; }

		/// <summary>
		/// 产品小类编码
		/// </summary>
		public string SmallCode { get; set; } = string.Empty;

		/// <summary>
		/// 产品小类名称
		/// </summary>
		public string SmallName { get; set; } = string.Empty;

		/// <summary>
		/// 产品Id
		/// </summary>
		public long ProductId { get; set; }

		/// <summary>
		/// 蚂蚁型号
		/// </summary>
		[Required(ErrorMessage = "产品型号不能为空!")]
		[StringLength(500)]
		public string MymoooProductNumber
		{
			get => mymoooNumber; set
			{
				mymoooNumber = value;
				this.MymoooNumberId = mymoooNumber.Replace("-", "").Trim().ToLower();
			}
		}

		/// <summary>
		/// 简易型号
		/// </summary>
		public string ShortNumber { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string MymoooNumberId { get; private set; } = string.Empty;

		/// <summary>
		/// 蚂蚁产品名称
		/// </summary>
		[Required(ErrorMessage = "产品名称不能为空!")]
		public string MymoooProductName { get => mymoooProductName;
			set
			{
				mymoooProductName = value;
				if (string.IsNullOrWhiteSpace(this.ProductName))
				{
					this.ProductName = value;
				}
			}
		}

		/// <summary>
		/// 产品类型Id
		/// </summary>
		public long TypeId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte CategoryType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int DataSource { get; set; }
	}
}
