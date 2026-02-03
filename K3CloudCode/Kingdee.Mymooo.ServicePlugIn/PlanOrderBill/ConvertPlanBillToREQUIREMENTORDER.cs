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
using Kingdee.K3.FIN.Core;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
	[Description("计划订单下推组织间需求单修改货主"), HotUpdate]
	public class ConvertPlanBillToREQUIREMENTORDER : AbstractConvertPlugIn
	{
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
			foreach (var headEntity in headEntitys)
			{
				headEntity.DataEntity["OwnerId_Id"] = headEntity.DataEntity["SupplyOrgId_Id"];
				headEntity.DataEntity["OwnerId"] = headEntity.DataEntity["SupplyOrgId"];
				long demandOrgId = Convert.ToInt64(headEntity.DataEntity["DemandOrgId_Id"]);
				long supplyorgid = Convert.ToInt64(headEntity.DataEntity["SupplyOrgId_Id"]);
				long materialid = Convert.ToInt64(headEntity.DataEntity["MaterialId_Id"]);
				//如果是华东或华南五部
				if (supplyorgid == 7401803 || supplyorgid == 7401782)
				{
					string sSql = $@"SELECT TOP 1 FPENYWORKSHOP,FPENYLEADER FROM T_PLN_REQUIREMENTORDER
                                WHERE FSUPPLYORGID={supplyorgid} AND FMATERIALID={materialid}
                                ORDER BY FCREATEDATE DESC";
					using (IDataReader reader = DBUtils.ExecuteReader(this.Context, sSql))
					{
						while (reader.Read())
						{
							headEntity.DataEntity["FPENYWorkshop_Id"] = Convert.ToInt64(reader[0]);
							//headEntity.DataEntity["FPENYWorkshop"] = Convert.ToInt64(reader[0]);
							headEntity.DataEntity["FPENYLeader_Id"] = Convert.ToInt64(reader[1]);
							//headEntity.DataEntity["FPENYLeader"] = Convert.ToInt64(reader[0]);
							break;
						}
					}
				}
				var FDemandQty = Convert.ToDecimal(headEntity.DataEntity["FirmQty"]);
				var F_PENY_Price = Convert.ToDecimal(headEntity.DataEntity["F_PENY_Price"]);
				var F_PENY_Amount = Convert.ToDecimal(headEntity.DataEntity["F_PENY_Amount"]);
				var AgentSalReduceRate = Convert.ToDecimal(((DynamicObjectCollection)((DynamicObject)headEntity.DataEntity["SupplyMaterialId"])["MaterialSale"]).FirstOrDefault()["AgentSalReduceRate"]);


				//需求组织不属于销售组织则修改需求单据为空
				long[] SalOrgList = new long[] { 224428, 669144, 1043841, 7348029 };
				if (!SalOrgList.Contains(demandOrgId))
				{
					headEntity.DataEntity["CreateType"] = "A";
					headEntity.DataEntity["ComputerNo"] = "";
					headEntity.DataEntity["DemandType"] = 0;
					headEntity.DataEntity["SaleOrderNo"] = "";
					headEntity.DataEntity["SaleOrderEntrySeq"] = 0;
				}
				else
				{
					headEntity.DataEntity["F_PENY_Price"] = F_PENY_Price * (1 - AgentSalReduceRate / 100);
					headEntity.DataEntity["F_PENY_Amount"] = F_PENY_Amount * (1 - AgentSalReduceRate / 100);
				}
			}
		}
	}
}
