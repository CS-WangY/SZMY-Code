using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Core.Report.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement.Report
{
	[Description("供应商小类检验评分报表插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class SuppClassInspScoreBusiness : AbstractSysReportPlugIn
	{
		/// <summary>
		/// 初始化事件
		/// </summary>
		/// <param name="e"></param>
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		//双击
		//单元格,双击事件
		public override void CellDbClick(CellEventArgs Args)
		{
			base.CellDbClick(Args);

			if (Args.Header.FieldName == "FSupplierCode")//供应商编码，查询检验评分更新变化明细记录
			{
				string fId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FID"));
				if (!string.IsNullOrWhiteSpace(fId))
				{
					SysReportShowParameter para = new SysReportShowParameter();
					para.OpenStyle.ShowType = ShowType.Modal;
					//唯一标识
					para.FormId = "PENY_InspectScoreDetailsRpt";
					para.IsShowFilter = false;
					para.CustomParams["FId"] = fId;
					this.View.ShowForm(para);
				}
			}
		
		}
	}
}
