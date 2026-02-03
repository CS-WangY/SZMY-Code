using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Data;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.BaseManagement;
using System.Net;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.Core.Metadata;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
	[Description("计划订单合并订单"), HotUpdate]
	public class Merge : AbstractConvertPlugIn
	{
		bool firstElementChecked = false;
		bool firstElement = false;
		string FMachineName = "";
		long FSupplierId = 0;
		long salorderentryid = 0;
		string FSALEORDERNO = "";
		long[] orglongs = new long[] { 7401803, 7401821, 14053641 };
		public override void OnQueryBuilderParemeter(QueryBuilderParemeterEventArgs e)
		{
			base.OnQueryBuilderParemeter(e);
			e.SelectItems.Add(new SelectorItemInfo("FPlanTenderType"));
			e.SelectItems.Add(new SelectorItemInfo("FMachineName"));
			e.SelectItems.Add(new SelectorItemInfo("FSupplierId"));
			e.SelectItems.Add(new SelectorItemInfo("FSALEORDERNO"));
		}

		public override void OnGetSourceData(GetSourceDataEventArgs e)
		{
			base.OnGetSourceData(e);
			foreach (var item in e.SourceData)
			{
				if (!firstElementChecked)
				{
					firstElement = Convert.ToBoolean(item["FIsSendCNCMES"]);
					firstElementChecked = true;
					continue;
				}
				if (Convert.ToBoolean(item["FIsSendCNCMES"]) != firstElement)
				{
					throw new Exception("计划发送mes选项不相同无法合并！");
				}
			}
			foreach (var item in e.SourceData)
			{
				if (string.IsNullOrEmpty(FMachineName))
				{
					FMachineName = Convert.ToString(item["FMachineName"]);
				}
			}
			foreach (var item in e.SourceData)
			{
				if (salorderentryid == 0)
				{
					FSALEORDERNO = Convert.ToString(item["FSALEORDERNO"]);
					salorderentryid = Convert.ToInt64(item["FSALEORDERENTRYID"]);
				}
			}


			foreach (var item in e.SourceData)
			{
				if (!orglongs.Contains(Convert.ToInt64(item["FSupplyOrgId"])))
				{
					continue;
				}
				if (FSupplierId == 0)
				{
					FSupplierId = Convert.ToInt64(item["FPENYCustomerID"]);
					continue;
				}
				if (Convert.ToInt64(item["FPENYCustomerID"]) != FSupplierId)
				{
					throw new Exception("客户不一致，请修改一致后再合并！");
				}
			}
		}
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
			foreach (var headEntity in headEntitys)
			{
				var entity = headEntity.DataEntity as DynamicObject;
				List<long> listFid = new List<long>();
				if (entity != null)
				{
					if (((DynamicObjectCollection)entity["FBillHead_Link"]) != null)
					{
						foreach (var item in headEntity.DataEntity["FBillHead_Link"] as DynamicObjectCollection)
						{
							//需求单FID(计划订单FId)
							listFid.Add(Convert.ToInt64(item["SId"]));
						}
						entity["FPENYCUSTOMERID_Id"] = GetCustId(listFid);
					}

					if (orglongs.Contains(Convert.ToInt64(entity["SupplyOrgId_Id"])))
					{
						entity["FMachineName"] = FMachineName;
						entity["SaleOrderEntryId"] = salorderentryid;
						entity["SaleOrderNo"] = FSALEORDERNO;
					}
				}
			}
		}

		public long GetCustId(List<long> list)
		{
			var sql = $@"/*dialect*/select distinct FPENYCUSTOMERID from T_PLN_PLANORDER where FID in ({string.Join(",", list)})";
			var datas = DBUtils.ExecuteDynamicObject(this.Context, sql);
			if (datas.Count() == 1)
			{
				return Convert.ToInt64(datas[0]["FPENYCUSTOMERID"]);
			}
			return 0;

		}


	}
}
