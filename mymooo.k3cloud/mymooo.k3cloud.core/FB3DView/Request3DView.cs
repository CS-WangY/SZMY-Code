using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.FB3DView
{
	/// <summary>
	/// 
	/// </summary>
	public class Request3DView
	{
		/// <summary>
		/// 
		/// </summary>
		public string FDemandOrgId { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string FCustomerID { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string FPlanBillNo { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string FMATERIALID { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string FMATERIALName { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public decimal FCostPrice { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal FSalPrice { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal FSalQty { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal FSalAmount { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal FGrossProfit { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string FPreviewUrl2D { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string FPreviewUrl3D { get; set; } = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string[]? FFileInfos { get; set; }

	}
}
