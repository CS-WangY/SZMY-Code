using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymooo.k3cloud.core.Sales
{
	/// <summary>
	/// 销售出库请求参数
	/// </summary>
	public class SalOutStockRequest
	{
		/// <summary>
		/// 销售单号
		/// </summary>
		public string SalOrderBillNo { get; set; } = string.Empty;
		/// <summary>
		/// 销售行号
		/// </summary>
		public int SalOrderSeq { get; set; }
	}
}
