using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using System.Security.Cryptography;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Util;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	[Description("采购订单保存更新是否重点跟踪")]
	[Kingdee.BOS.Util.HotUpdate]
	public class SaveEditIsFocusTracking : AbstractOperationServicePlugIn
	{
		/// <summary>
		/// 事务中 操作结束
		/// </summary>
		/// <param name="e"></param>
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			List<FocusTrackingPoDet> list = new List<FocusTrackingPoDet>();
			List<ReturnFocusTrackingPoDet> returnList = new List<ReturnFocusTrackingPoDet>();
			foreach (var item in e.DataEntitys)
			{
				var fid = Convert.ToInt64(item["Id"]);
				var sql = $@"/*dialect*/select t1.FENTRYID EntryID,FPARENTSMALLID GrpID,isnull(gl.FNUMBER,'') GrpCode,FSMALLID TypeID,isnull(g.FNUMBER,'') TypeCode,t2.FALLAMOUNT Amount from t_PUR_POOrderEntry t1
                            inner join T_PUR_POORDERENTRY_F t2 on t1.FENTRYID=t2.FENTRYID
                            left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                            left join T_BD_MATERIALGROUP gl on t1.FPARENTSMALLID = gl.FID
                            where t1.FID={fid} ";
				var datas = DBUtils.ExecuteDynamicObject(this.Context, sql);
				foreach (var data in datas)
				{
					list.Add(new FocusTrackingPoDet
					{
						EntryID = Convert.ToInt64(data["EntryID"]),
						GrpCode = Convert.ToString(data["GrpCode"]),
						TypeCode = Convert.ToString(data["TypeCode"]),
						Amount = Convert.ToDecimal(data["Amount"])
					});
				}
			}
			var requestData = list.Where(w => w.GrpCode != "" && w.TypeCode != "").GroupBy(g => new { g.GrpCode, g.TypeCode }).Select(x => new { x.Key.GrpCode, x.Key.TypeCode }).ToList();
			if (requestData.Count == 0)
			{
				return;
			}
			try
			{
				var requestJsonData = JsonConvertUtils.SerializeObject(requestData);
				string url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/ProductSmallClass/GetSmallClassAmounts";
				var resultJson = ApigatewayUtils.InvokePostWebService(url, requestJsonData);
				var result = JsonConvertUtils.DeserializeObject<ResponseMessage<List<ReturnFocusTrackingPoDet>>>(resultJson);
				if (result != null && result.Data != null && result.Data.Count > 0)
				{
					returnList = result.Data;
				}
				List<SqlObject> sqlList = new List<SqlObject>();
				foreach (var item in list)
				{
					//平台返回的金额
					decimal returnAmount = returnList.Where(x => (x.GrpCode.EqualsIgnoreCase(item.GrpCode) && x.TypeCode.EqualsIgnoreCase(item.TypeCode))).Count() == 0 ? 0 : returnList.Where(x => (x.GrpCode.EqualsIgnoreCase(item.GrpCode) && x.TypeCode.EqualsIgnoreCase(item.TypeCode))).FirstOrDefault().Amount;
					if (returnAmount > 0 && item.Amount > 0 && item.Amount >= (returnAmount * Convert.ToDecimal("0.8")))
					{
						//更新重点跟踪的明细
						sqlList.Add(new SqlObject("/*dialect*/update t_PUR_POOrderEntry set FIsFocusTracking='1' where FENTRYID =@FENTRYID ",
							 new List<SqlParam>() { new SqlParam("@FENTRYID", KDDbType.Int64, item.EntryID) }));
					}
					else
					{
						//更新非重点跟踪的明细
						sqlList.Add(new SqlObject("/*dialect*/update t_PUR_POOrderEntry set FIsFocusTracking='0' where FENTRYID =@FENTRYID ",
							 new List<SqlParam>() { new SqlParam("@FENTRYID", KDDbType.Int64, item.EntryID) }));

					}
				}
				if (sqlList.Count > 0)
				{
					DBUtils.ExecuteBatch(this.Context, sqlList);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("采购订单保存更新是否重点跟踪：" + ex.Message);
			}
		}

		public class FocusTrackingPoDet
		{
			/// <summary>
			/// 采购明细ID
			/// </summary>
			public long EntryID { get; set; }

			/// <summary>
			/// 大类编码
			/// </summary>
			public string GrpCode { get; set; }

			/// <summary>
			/// 小类编码
			/// </summary>
			public string TypeCode { get; set; }

			/// <summary>
			/// 价税合计
			/// </summary>
			public decimal Amount { get; set; }
		}

		public class ReturnFocusTrackingPoDet
		{
			/// <summary>
			/// 大类编码
			/// </summary>
			public string GrpCode { get; set; }

			/// <summary>
			/// 小类编码
			/// </summary>
			public string TypeCode { get; set; }

			/// <summary>
			/// 金额
			/// </summary>
			public decimal Amount { get; set; } = 0;
		}

	}
}
