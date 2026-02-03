using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Kingdee.BOS.Util;
using Kingdee.BOS.App.Data;
using System.Data;

namespace Kingdee.Mymooo.ServicePlugIn.SalOutstock
{
	[Description("出库单下推应收单修改到期日"), HotUpdate]
	public class ConvertOutStock_Receivable : AbstractConvertPlugIn
	{
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
			foreach (var headEntity in headEntitys)
			{
				var paytype = (DynamicObject)headEntity.DataEntity["PayConditon"];
				var entryRow = ((DynamicObjectCollection)headEntity.DataEntity["AP_PAYABLEENTRY"]).FirstOrDefault();
				var OrderBillNo = Convert.ToString(entryRow["SourceBillNo"]);
				if ((bool)entryRow["FIsGuarantee"])
				{
					string sSql = $@"SELECT FEXPECTEDPAYMENTDATE FROM dbo.T_SAL_OUTSTOCK WHERE FBILLNO='{OrderBillNo}'";
					var expdate = DBUtils.ExecuteScalar<string>(this.Context, sSql, "");
					headEntity.DataEntity["FENDDATE_H"] = expdate;
				}
				else
				{
					switch (Convert.ToString(paytype["Number"]))
					{
						case "501":
						case "502":
						case "DAP":
							headEntity.DataEntity["FENDDATE_H"] = headEntity.DataEntity["DATE"];
							break;
						default:
							string sSql = $@"SELECT FAPPROVEDATE FROM dbo.T_SAL_OUTSTOCK WHERE FBILLNO='{OrderBillNo}'";
							var appdate = DBUtils.ExecuteScalar<string>(this.Context, sSql, "");
							if (!string.IsNullOrEmpty(appdate))
							{
								var odday = Convert.ToInt32(((DynamicObjectCollection)paytype["PayRecCondtionEntry"]).FirstOrDefault()["ODDay"]);
								headEntity.DataEntity["FENDDATE_H"] = Convert.ToDateTime(appdate).AddDays(odday).AddDays(7);
							}
							break;
					}
				}
			}
		}
	}
}
