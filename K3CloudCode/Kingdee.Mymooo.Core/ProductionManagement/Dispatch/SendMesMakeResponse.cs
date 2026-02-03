using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement.Dispatch
{
	public class SendMesMakeResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int Code { get; set; }
		public ResponseResult Result { get; set; }
		public long timestamp { get; set; }

		public class ResponseResult
		{
			public string Key { get; set; }
			public string MakeNo { get; set; }
			public int MakeSeq { get; set; }
		}
	}
}
