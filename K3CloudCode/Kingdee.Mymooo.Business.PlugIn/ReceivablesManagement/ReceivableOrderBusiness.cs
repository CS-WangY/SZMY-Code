using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.DirectSaleManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System.IO;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json.Serialization;
using Kingdee.Mymooo.ServiceHelper.PurRequisitionManagement;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Orm;

namespace Kingdee.Mymooo.Business.PlugIn.ReceivablesManagement
{
	public class ReceivableOrderBusiness
	{
		public ResponseMessage<DynamicObject> SyncReceivableOrser(Context ctx)
		{
			ResponseMessage<DynamicObject> response = new ResponseMessage<DynamicObject>() { Data = null };
			try
			{
				string sSql = @"SELECT * FROM (
SELECT t1.FCUSTID,t1.FNUMBER,t1l.FNAME,t2.arid,t3.said,t4.void FROM dbo.T_BD_CUSTOMER t1
INNER JOIN T_BD_CUSTOMER_L t1l ON t1.FCUSTID=t1l.FCUSTID
LEFT JOIN (
    SELECT 
        MAX(FID) as arid,
		FCUSTOMERID
    FROM t_AR_receivable
    GROUP BY FCUSTOMERID
) t2 ON t1.FCUSTID = t2.FCUSTOMERID
LEFT JOIN (
    SELECT 
        MAX(FID) as said,
		FCUSTID
    FROM dbo.T_SAL_ORDER
    GROUP BY FCUSTID
) t3 ON t1.FCUSTID = t3.FCUSTID
LEFT JOIN (
SELECT
MAX(FID) AS void,
FFLEX6
FROM T_BD_FLEXITEMDETAILV
GROUP BY FFLEX6
) t4 ON t1.FCUSTID=t4.FFLEX6
) _t WHERE _t.arid IS NULL AND _t.said IS NULL AND _t.void IS NULL
AND NOT EXISTS(
SELECT 1 FROM temp_DeleteCust t2 WHERE _t.FCUSTID=t2.FCUSTID
)";
				var recedata = DBUtils.ExecuteDynamicObject(ctx, sSql);

				FormMetadata materialMetadata = MetaDataServiceHelper.Load(ctx, "BD_Customer") as FormMetadata;

				foreach (var item in recedata)
				{
					var cuid = Convert.ToInt64(item["FCUSTID"]);
					var cunumber = Convert.ToString(item["FNUMBER"]);
					var cuname = Convert.ToString(item["FNAME"]);
					var arid = Convert.ToInt64(item["arid"]);
					var said = Convert.ToInt64(item["said"]);
					var void1 = Convert.ToInt64(item["void"]);
					//			sSql = $@"SELECT 
					//          FENTRYID,
					//          ROW_NUMBER() OVER (PARTITION BY FID ORDER BY FENDORSEDATE DESC) as rn
					//      FROM T_CN_BILLRECEIVABLEENDORSE
					//WHERE FID={receivebleid}";
					//			var orsedata = DBUtils.ExecuteDynamicObject(ctx, sSql);
					//			foreach (var orseitem in orsedata)
					//			{
					//				if (Convert.ToInt32(orseitem["rn"]) > 1)
					//				{
					//					sSql = $@"DELETE T_CN_BILLRECEIVABLEENDORSE WHERE FENTRYID={orseitem["FENTRYID"]}";
					//					DBUtils.Execute(ctx, sSql);
					//				}

					//			}



					// 模拟删除服务端操作完整过程
					OperateOption option = OperateOption.Create();
					option.SetIgnoreWarning(false);
					IOperationResult unAuditResult = BusinessDataServiceHelper.UnAudit(ctx, materialMetadata.BusinessInfo, new object[] { cuid }, option);
					var oper = BusinessDataServiceHelper.Delete(ctx, materialMetadata.BusinessInfo, new object[] { cuid });
					string insertSql = @"INSERT INTO temp_DeleteCust (FCUSTID,FNUMBER,FNAME,FNOTE) VALUES 
							(
							@FCUSTID,
							@FNUMBER,
							@FNAME,
							@FNOTE
							)";


					//var oper = service.UnAudit(ctx, billView.BusinessInfo, new object[] { cuid });
					//oper = service.DeleteBill(ctx, billView.BusinessInfo, new object[] { cuid });
					string opernote = "删除成功";
					if (!oper.IsSuccess)
					{
						if (oper.ValidationErrors.Count > 0)
						{
							opernote = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
						}
						else
						{
							opernote = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
						}
					}
					var insertParams = new List<SqlParam>()
							{
								new SqlParam("@FCUSTID", KDDbType.Int64, cuid),
								new SqlParam("@FNUMBER", KDDbType.String, cunumber),
								new SqlParam("@FNAME", KDDbType.String, cuname),
								new SqlParam("@FNOTE", KDDbType.String, opernote),
							};
					DBUtils.Execute(ctx, insertSql, insertParams);

					//清除释放网控
					//billView.CommitNetworkCtrl();
					//billView.InvokeFormOperation(FormOperationEnum.Close);
					//billView.Close();




					//var entitycount = billView.Model.GetEntryRowCount("FEndorseEntity") - 1;
					//sSql = $@"SELECT MAX(FENTRYID) FENTRYID,t2.FDATE
					//FROM T_CN_PAYBILLREC t1 LEFT JOIN T_AP_PAYBILL t2 ON t2.FID = t1.FID
					//WHERE t1.FRECEIVEBLEBILLID={receivebleid}
					//GROUP BY t1.FRECEIVEBLEBILLID,t2.FDATE";
					//var date = DBUtils.ExecuteDataSet(ctx, sSql);
					//if (date.Tables[0].Rows.Count <= 0)
					//{
					//	continue;
					//}
					//var orsedate = date.Tables[0].Rows[0]["FDATE"];
					//var drawer = billView.Model.GetValue("FDRAWER");
					//billView.Model.SetValue("FENDORSEDATE", orsedate, entitycount);
					//billView.Model.SetValue("FENDORSER", drawer, entitycount);
					//List<DynamicObject> dynamicObjects = new List<DynamicObject>();
					//dynamicObjects.Add(billView.Model.DataObject);
					//var oper = service.SaveBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());

				}

				response.Code = ResponseCode.Success;
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Warning;
				response.Message = ex.ToString();
			}

			return response;
		}
	}
}
