using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.CN_BillReceivable
{
	[Description("应收票据-收款单转换插件"), HotUpdate]
	public class CN_BillRecToAR_ReceiveBillConvert : AbstractConvertPlugIn
	{
		public override void OnAfterCreateLink(CreateLinkEventArgs e)
		{
		}
		public override void OnGetSourceData(GetSourceDataEventArgs e)
		{
			base.OnGetSourceData(e);
			string sSql = $@"SELECT t3.FNAME,t2.FCOMMENT FROM T_CN_BILLRECEIVABLE t2
			LEFT JOIN dbo.T_BD_CUSTOMER_L t3 ON t2.FCONTACTUNIT=t3.FCUSTID
			WHERE t2.FID={e.SourceData.FirstOrDefault()["FID"]}";
			var outsource = DBUtils.ExecuteDynamicObject(this.Context, sSql);
			if (Convert.ToString(outsource.FirstOrDefault()["FNAME"]) != "")
			{
				noets = "收" + outsource.FirstOrDefault()["FNAME"] + "货款";
			}
			if (!string.IsNullOrEmpty(Convert.ToString(outsource.FirstOrDefault()["FCOMMENT"]).Trim()))
			{
				noets += "(" + outsource.FirstOrDefault()["FCOMMENT"] + ")";
			}
			if (!string.IsNullOrEmpty(Convert.ToString(outsource.FirstOrDefault()["FCOMMENT"])))
			{
				oldnoets = Convert.ToString(outsource.FirstOrDefault()["FCOMMENT"]);
			}
		}
		public static string noets;
		public static string oldnoets;
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			// 目标单单据体元数据
			Entity entity = e.TargetBusinessInfo.GetEntity("FRECEIVEBILLENTRY");
			// 读取已经生成的
			ExtendedDataEntity[] bills = e.Result.FindByEntityKey("FBillHead");
			// 对目标单据进行循环
			foreach (var bill in bills)
			{
				var billtype = Convert.ToString(((DynamicObject)bill.DataEntity["BillTypeID"])?["Id"]);
				if (billtype != "36cf265bd8c3452194ed9c83ec5e73d2")
				{
					// 取单据体集合
					DynamicObjectCollection rowObjs = entity.DynamicProperty.GetValue(bill.DataEntity)
					as DynamicObjectCollection;
					foreach (var item in rowObjs)
					{
						item["COMMENT"] = oldnoets;
					}
				}
				else
				{
					// 取单据体集合
					DynamicObjectCollection rowObjs = entity.DynamicProperty.GetValue(bill.DataEntity)
					as DynamicObjectCollection;
					foreach (var item in rowObjs)
					{
						item["COMMENT"] = noets;
					}
				}
			}
		}
	}
}
