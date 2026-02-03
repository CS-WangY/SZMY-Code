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
	[Description("接收银行明细-收款单转换"), HotUpdate]
	public class WB_RecBkTradeDetailToAR_RECEIVEBILLConvert : AbstractConvertPlugIn
	{
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
				var contact = bill.DataEntity["CONTACTUNIT"] as DynamicObject;
				if (contact == null) return;
				// 取单据体集合
				DynamicObjectCollection rowObjs = entity.DynamicProperty.GetValue(bill.DataEntity)
				as DynamicObjectCollection;
				foreach (var item in rowObjs)
				{
					item["COMMENT"] = "收" + contact["Name"] + "货款";
				}
			}
		}
	}
}
