using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.Bill;
using Kingdee.K3.SCM.Core;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace Kingdee.Mymooo.Business.PlugIn.AR_ReceivebillManagement
{
	[Description("收款单表单插件"), HotUpdate]
	public class AR_ReceivebillPlugIn : AbstractBillPlugIn
	{
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (e.Field.Key.EqualsIgnoreCase("FCONTACTUNIT"))
			{
				var Contactunit = this.Model.GetValue("FCONTACTUNIT", 0) as DynamicObject;
				if (Contactunit == null) return;

				var billtype = Convert.ToString(((DynamicObject)this.Model.GetValue("FBillTypeID", 0))?["Id"]);
				if (billtype != "36cf265bd8c3452194ed9c83ec5e73d2")
				{
					return;
				}
				string sSql = $@"SELECT t2.FNAME FROM dbo.T_BD_CUSTOMER t1
                LEFT JOIN T_BD_RecCondition_L t2 ON t1.FRECCONDITIONID=t2.FID
                WHERE FCUSTID={Contactunit["Id"]}";
				var recdata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
				foreach (var item in recdata)
				{
					this.Model.SetValue("FREMARK", item["FNAME"], 0);
				}

				//var entrys = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FRECEIVEBILLENTRY"));
				var rowcount = this.Model.GetEntryRowCount("FRECEIVEBILLENTRY");
				for (int i = rowcount; i >= 0; i--)
				{
					var comment = this.Model.GetValue<string>("FCOMMENT", i, "");
					if (string.IsNullOrEmpty(comment))
					{
						this.Model.SetValue("FCOMMENT", "收" + Contactunit["Name"] + "货款", i);
					}
				}

			}
		}
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			var billtype = Convert.ToString(((DynamicObject)this.Model.GetValue("FBillTypeID", 0))?["Id"]);
			if (billtype != "36cf265bd8c3452194ed9c83ec5e73d2")
			{
				return;
			}
			var Contactunit = this.Model.GetValue("FCONTACTUNIT", 0) as DynamicObject;
			if (Contactunit != null)
			{
				var comment = this.Model.GetValue<string>("FCOMMENT", e.Row, "");
				if (string.IsNullOrEmpty(comment))
				{
					this.Model.SetValue("FCOMMENT", "收" + Contactunit["Name"] + "货款", e.Row);
				}
			}
		}
	}
}
