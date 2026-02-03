using Kingdee.BOS.Core;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.K3.FIN.Core.RPA;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core;
using static Kingdee.Mymooo.ServicePlugIn.PurchaseBill.SaveEditIsFocusTracking;
using Kingdee.Mymooo.Core.StockManagement;
using System.Web.UI.WebControls.WebParts;
using System.Net;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.ReceiveBill
{
	/// <summary>
	/// 采购收料通知单转检验单插件
	/// </summary>
	[Description("采购收料通知单转检验单插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class ConvertPurReceiveToInspect : AbstractConvertPlugIn
	{
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
			foreach (var headEntity in headEntitys)
			{
				List<string> strList = new List<string>();
				List<ReturnComplain> returnList = new List<ReturnComplain>();
				long orgId = Convert.ToInt64(headEntity["STOCKORGID_Id"]);
				foreach (var item in headEntity.DataEntity["Entity"] as DynamicObjectCollection)
				{
					string itemNo = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]);
					//历史客诉
					strList.Add(itemNo);
				}
				try
				{
					strList = strList.Distinct().ToList();
					var sql = $@"/*dialect*/select FAntComplaintModel ItemNo,FComplaintDetailContent Complain from (
						select ROW_NUMBER() OVER (PARTITION BY t1.FAntComplaintModel ORDER BY t1.FComplaintDate DESC) AS rn,FAntComplaintModel,FComplaintDetailContent 
						from PENY_T_CompanyComplaint t1 
						where FAntComplaintModel in ('{string.Join("、", strList).Replace("、", "','")}')) datas where rn=1 ";
					returnList = JsonConvertUtils.DeserializeObject<List<ReturnComplain>>
							(JsonConvertUtils.SerializeObject(DBUtils.ExecuteDynamicObject(this.Context, sql)));
					//循环赋值
					foreach (var item in headEntity.DataEntity["Entity"] as DynamicObjectCollection)
					{
						if (returnList.Where(x => x.ItemNo.EqualsIgnoreCase(Convert.ToString(((DynamicObject)item["MaterialID"])["Number"].ToString()))).Count() > 0)
						{
							item["FHistoricalComplain"] = returnList.Where(x => x.ItemNo.EqualsIgnoreCase(Convert.ToString(((DynamicObject)item["MaterialID"])["Number"].ToString()))).FirstOrDefault().Complain;
						}

					}
				}
				catch (Exception ex)
				{
					throw new Exception("获取平台历史客诉失败：" + ex.Message);
				}
			}
		}

		public class ReturnComplain
		{
			/// <summary>
			/// 物料编码
			/// </summary>
			public string ItemNo { get; set; }

			/// <summary>
			/// 客诉
			/// </summary>
			public string Complain { get; set; } = "";
		}
	}
}
