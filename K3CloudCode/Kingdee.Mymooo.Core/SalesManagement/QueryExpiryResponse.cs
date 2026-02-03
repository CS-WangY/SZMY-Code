using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
	public class QueryExpiryResponse
	{
		/// <summary>
		/// 逾期天数，取最大天数
		/// </summary>
		public int ExpiryDay { get; set; }

		/// <summary>
		/// 逾期金额，逾期金额合计
		/// </summary>
		public decimal ExpiryAmount { get; set; }
	}
	public class QueryExpiryResponseList
	{
		public string ExpiryBill { get; set; }
		/// <summary>
		/// 逾期天数，取最大天数
		/// </summary>
		public int ExpiryDay { get; set; }

		/// <summary>
		/// 逾期金额，逾期金额合计
		/// </summary>
		public decimal ExpiryAmount { get; set; }
	}
	public class RequestExpiry
	{
		public string CustomerCode { get; set; }
	}
}
