using Kingdee.BOS;
using Kingdee.BOS.App.Core.Report;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.SCM.Contracts;
using Kingdee.K3.SCM.Core;
using Kingdee.K3.SCM.Core.SAL;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
	public class ChangeSalesOrderBusiness : IMessageExecute
	{
		public ResponseMessage<dynamic> Execute(Context ctx, string message)
		{
			ChangeSalesOrderRequest request = JsonConvertUtils.DeserializeObject<ChangeSalesOrderRequest>(message);
			//try
			//{
			var result = ChangeSalesOrder(ctx, request);
			//    MymoooBusinessDataServiceHelper.AddRabbitMqMeaageResult(ctx, "ChangeSalesOrder", request.SalesOrderNo, message, result.IsSuccess, JsonConvertUtils.SerializeObject(result));
			return result;
			//}
			//catch (Exception ex)
			//{
			//    MymoooBusinessDataServiceHelper.AddRabbitMqMeaageResult(ctx, "ChangeSalesOrder", request.SalesOrderNo, message, false, ex.Message);
			//    throw;
			//}

		}

		private ResponseMessage<dynamic> ChangeSalesOrder(Context ctx, ChangeSalesOrderRequest request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			var org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.OrgNumber, ""));
			if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C"))
			{
				response.Code = ResponseCode.NoExistsData;
				response.Message = "对应的组织不存在或未审核";
				return response;
			}
			request.OrgId = org.Id;

			CustomerServcie customerServcie = new CustomerServcie();
			var customer = customerServcie.TryGetOrAdd(ctx, new CustomerInfo(request.CustomerInfo.Code, request.CustomerInfo.Name));
			if (customer.Id == 0 || !customer.DocumentStatus.EqualsIgnoreCase("C"))
			{
				response.Code = ResponseCode.NoExistsData;
				response.Message = "对应的客户不存在或未审核";
				return response;
			}

			Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>(StringComparer.OrdinalIgnoreCase);
			List<long> materialss = new List<long>();
			foreach (var detail in request.SalesOrderDetailList)
			{
				detail.ItemNo = detail.ItemNo.Trim();
				detail.ItemName = detail.ItemName.Trim();
				detail.CustItemNo = string.IsNullOrWhiteSpace(detail.CustItemNo) ? detail.ItemNo.Trim() : detail.CustItemNo.Trim();
				detail.CustItemName = string.IsNullOrWhiteSpace(detail.CustItemName) ? detail.ItemName.Trim() : detail.CustItemName.Trim();

				var materialInfo = new MaterialInfo(detail.ItemNo, detail.ItemName);
				if (materials.ContainsKey(detail.ItemNo))
				{
					materialInfo = materials[detail.ItemNo];
				}
				else
				{
					materialInfo.ProductId = detail.ProductId;
					materialInfo.UseOrgId = request.OrgId;
					materialInfo.ShortNumber = detail.ShortNumber.Trim();
					materialInfo.PriceType = detail.PriceType;
					materialInfo.ProductSmallClass = detail.ProductSmallClass;
					materialInfo = MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, new List<long>() { org.Id });
					materials[detail.ItemNo] = materialInfo;
				}
				detail.MaterialId = materialInfo.Id;
				detail.MaterialMasterId = materialInfo.MasterId;

				materialss.Add(materialInfo.MasterId);
			}
			MaterialServiceHelper.MaterialAllocateToAll(ctx, materialss);
			//客户物料对照表
			//var custMaterials = MaterialServiceHelper.TryGetOrAddCustMsterials(ctx, customer, request.SalesOrderDetailList.GroupBy(p => $"{p.ItemNo}_{p.CustItemNo}").Select(p =>
			//{
			//    var detail = p.FirstOrDefault();
			//    MaterialInfo materialInfo = new MaterialInfo(detail.ItemNo, detail.ItemName)
			//    {
			//        MasterId = detail.MaterialMasterId,
			//        Id = detail.MaterialId,
			//        UseOrgId = request.OrgId,
			//        CustomerMaterialNumber = detail.CustItemNo,
			//        CustomerMaterialName = detail.CustItemName
			//    };
			//    return materialInfo;
			//}).ToArray());

			//foreach (var detail in request.SalesOrderDetailList)
			//{
			//    var custMaterial = custMaterials.FirstOrDefault(p => p.Code.EqualsIgnoreCase(detail.ItemNo) && p.CustomerMaterialNumber.EqualsIgnoreCase(detail.CustItemNo));
			//    detail.MaterialMapId = custMaterial.CustomerMaterialId;
			//}
			ChangeSalesOrder(ctx, request, response);
			return response;
		}

		private void CheckChangeOrder(Context ctx, ChangeSalesOrderRequest request, long id)
		{
			//变更单价,需要判断是否已经做了应收单
			//            var receivableSql = @"/*dialect*/
			//with salesorder as
			//(
			//	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID
			//	from T_BF_INSTANCEENTRY s
			//		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
			//	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
			//	union all
			//	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID
			//	from salesorder s
			//		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
			//)
			//select 1
			//from salesorder s
			//	inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
			//where s.FTTABLENAME = @FTTABLENAME";
			var receivableSql = @"SELECT t1.FORDERDETAILID FROM dbo.T_SAL_ORDERENTRY t1
            INNER JOIN T_SAL_ORDERENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
            WHERE t2.FARAMOUNT>0 AND t2.FID=@FID";
			//request.IsReceivable = DBServiceHelper.ExecuteScalar<bool>(ctx, receivableSql, false, paramList: new SqlParam[]
			//   {
			//        new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
			//        new SqlParam("@FID", KDDbType.Int64, id),
			//        new SqlParam("@FTTABLENAME", KDDbType.String, "t_AR_receivableEntry")
			//   });
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, receivableSql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
			foreach (var data in datas)
			{
				var detail = request.SalesOrderDetailList.FirstOrDefault(p => p.Id == data.GetValue<decimal>("FORDERDETAILID"));
				if (detail != null)
				{
					detail.IsReceivable = true;
				}
			}

			//变更物料,删除物料,或者变更数量减少,需要判断是否存在组织间需求单,以及采购单
			//            var reqSql = @"SELECT e.FORDERDETAILID
			//FROM T_PLN_REQUIREMENTORDER r
			//	inner join T_SAL_ORDERENTRY e on r.FSaleOrderEntryId = e.FENTRYID
			//where e.FID=@FID and r.FDEMANDTYPE = '1' and r.FISCLOSED='A' ";
			var reqSql = @"SELECT t1.FORDERDETAILID FROM T_SAL_ORDERENTRY t1
            INNER JOIN 
            (
            SELECT t1.*,t2.FDEMANDINTERID,t2.FDEMANDENTRYID FROM T_PLN_RESERVELINKENTRY t1
            INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
            WHERE t2.FDEMANDFORMID='SAL_SaleOrder'
            ) t2 ON t1.FENTRYID=t2.FDEMANDENTRYID
            WHERE t1.FID=@FID";
			datas = DBServiceHelper.ExecuteDynamicObject(ctx, reqSql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
			foreach (var data in datas)
			{
				var detail = request.SalesOrderDetailList.FirstOrDefault(p => p.Id == data.GetValue<decimal>("FORDERDETAILID"));
				if (detail != null)
				{
					detail.IsPurchase = true;
				}
			}
			//            reqSql = @"SELECT e.FORDERDETAILID
			//FROM T_PLN_PLANORDER r
			//	inner join T_PLN_PLANORDER_B rb on r.FID = rb.FID
			//	inner join T_SAL_ORDERENTRY e on rb.FSALEORDERENTRYID = e.FENTRYID
			//where e.FID=@FID and rb.FDemandType = '1' and r.FReleaseStatus < '3'";
			//            datas = DBServiceHelper.ExecuteDynamicObject(ctx, reqSql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
			//            foreach (var data in datas)
			//            {
			//                var detail = request.SalesOrderDetailList.FirstOrDefault(p => p.Id == data.GetValue<decimal>("FORDERDETAILID"));
			//                if (detail != null)
			//                {
			//                    detail.IsPurchase = true;
			//                }
			//            }
			//            reqSql = @"select e.FORDERDETAILID
			//from T_PUR_Requisition r
			//	inner join T_PUR_ReqEntry re on r.FID = re.FID
			//	inner join T_SAL_ORDER o on re.FSONO = o.FBILLNO
			//	inner join T_SAL_ORDERENTRY e on o.FID = e.FID and re.FSOSEQ = e.FSEQ
			//where o.FID = @FID and r.FCancelStatus = 'A' and re.FMRPTERMINATESTATUS = 'A'";
			//            datas = DBServiceHelper.ExecuteDynamicObject(ctx, reqSql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
			//            foreach (var data in datas)
			//            {
			//                var detail = request.SalesOrderDetailList.FirstOrDefault(p => p.Id == data.GetValue<decimal>("FORDERDETAILID"));
			//                if (detail != null)
			//                {
			//                    detail.IsPurchase = true;
			//                }
			//            }
			//            reqSql = @"select e.FORDERDETAILID
			//from t_PUR_POOrder r
			//	inner join T_PUR_POORDERENTRY re on r.FID = re.FID
			//	inner join T_SAL_ORDER o on re.FSONO = o.FBILLNO
			//	inner join T_SAL_ORDERENTRY e on o.FID = e.FID and re.FSOSEQ = e.FSEQ
			//where o.FID = @FID and r.FCancelStatus = 'A'";
			//            datas = DBServiceHelper.ExecuteDynamicObject(ctx, reqSql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
			//            foreach (var data in datas)
			//            {
			//                var detail = request.SalesOrderDetailList.FirstOrDefault(p => p.Id == data.GetValue<decimal>("FORDERDETAILID"));
			//                if (detail != null)
			//                {
			//                    detail.IsPurchase = true;
			//                }
			//            }

			//            var deliveNotSql = @"/*dialect*/
			//with salesorder as
			//(
			//	select s.FSID,s.FSTABLENAME,s.FTTABLENAME,s.FTID,e.FORDERDETAILID
			//	from T_BF_INSTANCEENTRY s
			//		inner join T_SAL_ORDERENTRY e on s.FSID = e.FENTRYID 
			//	where s.FSTABLENAME = @FSTABLENAME and e.FID = @FID
			//	union all
			//	select s.FSID,s.FSTABLENAME,t.FTTABLENAME,t.FTID,s.FORDERDETAILID
			//	from salesorder s
			//		inner join T_BF_INSTANCEENTRY t on s.FTTABLENAME = t.FSTABLENAME and s.FTID = t.FSID
			//)
			//select s.FORDERDETAILID 
			//from salesorder s 
			//	inner join T_SAL_DELIVERYNOTICEENTRY e on s.FTID = e.FENTRYID 
			//	inner join T_SAL_DELIVERYNOTICE d on e.FID = d.FID 
			//where s.FTTABLENAME = @FTTABLENAME and d.FCANCELSTATUS = 'A'";
			var deliveNotSql = @"SELECT t1.FORDERDETAILID FROM dbo.T_SAL_ORDERENTRY t1
INNER JOIN T_SAL_ORDERENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
WHERE t2.FBASECANOUTQTY<t1.FQTY AND t2.FID=@FID";
			datas = DBServiceHelper.ExecuteDynamicObject(ctx, deliveNotSql, paramList: new SqlParam[]
				{
                    //new SqlParam("@FSTABLENAME", KDDbType.String, "T_SAL_ORDERENTRY"),
                    new SqlParam("@FID", KDDbType.Int64, id),
                    //new SqlParam("@FTTABLENAME", KDDbType.String, "T_SAL_DELIVERYNOTICEENTRY")
                });
			foreach (var data in datas)
			{
				request.IsDeliverNotice = true;
				var detail = request.SalesOrderDetailList.FirstOrDefault(p => p.Id == data.GetValue<decimal>("FORDERDETAILID"));
				if (detail != null)
				{
					detail.IsDeliverNotice = true;
				}
			}
		}

		private void ChangeSalesOrder(Context ctx, ChangeSalesOrderRequest request, ResponseMessage<dynamic> response)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, "SAL_SaleOrder");
			DynamicObjectCollection oldOrderBillDatas = null;
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = "SAL_SaleOrder";
			queryBuilderParemeter.SelectItems = SelectorItemInfo.CreateItems("FID,FBILLNO,FBillTypeID,FDOCUMENTSTATUS,FLOCKFLAG,FSaleOrderEntry_FEntryId,FOrderDetailId,FCHANGEFLAG");
			queryBuilderParemeter.FilterClauseWihtKey = $"FBILLNO='{request.SalesOrderNo}'";
			oldOrderBillDatas = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryBuilderParemeter);
			if (oldOrderBillDatas == null || oldOrderBillDatas.Count <= 0)
			{
				throw new Exception(string.Format("订单编号为【{0}】的订单不存在，请检查订单编号是否正确!", request.SalesOrderNo));
			}
			//List<long> notExists = new List<long>();
			//foreach (var entry in request.SalesOrderDetailList)
			//{
			//    if (!oldOrderBillDatas.Any(n => Convert.ToInt64(n["FOrderDetailId"]) == entry.Id))
			//    {
			//        notExists.Add(entry.Id);
			//    }
			//}
			//if (notExists.Count > 0)
			//{
			//    throw new Exception($"分录内码：{string.Join(",", notExists)},不在销售订单【{request.SalesOrderNo}】内，请检查参数！");
			//}

			FormMetadata xformMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, "SAL_XORDER");
			long num = Convert.ToInt64(oldOrderBillDatas[0]["FID"]);
			CheckChangeOrder(ctx, request, num);
			DynamicObjectType dynamicObjectType = xformMetadata.BusinessInfo.GetDynamicObjectType();
			DynamicObject xSalesData = new DynamicObject(dynamicObjectType);
			XSaleOrderCommon xSaleOrderCommon = new XSaleOrderCommon(ctx);
			xSaleOrderCommon.lstNotFields = GetTakeEffectNotWriteBackFields(xformMetadata);
			DynamicObject[] array = BusinessDataServiceHelper.Load(ctx, new object[1] { num }, formMetadata.BusinessInfo.GetDynamicObjectType());
			DynamicObject oldSalesData = ((array != null && array.Count() > 0) ? array[0] : null);
			string validateMsg = string.Empty;
			bool para_IsUseTaxCombination = SystemParameterServiceHelper.IsUseTaxCombination(ctx);
			xSaleOrderCommon.baseOrderData = oldSalesData;
			xSaleOrderCommon.para_IsUseTaxCombination = para_IsUseTaxCombination;
			xSaleOrderCommon.orderBusinessInfo = formMetadata.BusinessInfo;
			xSaleOrderCommon.SetCommonVarValue();
			xSaleOrderCommon.SetNewDataFromOriginalData(xSalesData, xSaleOrderCommon.baseOrderData, isWriteBackSO: false, xSaleOrderCommon.lstNotFields);
			SetBillTypeValue(xformMetadata.BusinessInfo, formMetadata.BusinessInfo, oldSalesData, xSalesData);
			xSalesData = FilterChangeDataByEntryIds(xSalesData, request.SalesOrderDetailList);

			if (xSalesData != null && xSalesData.DynamicObjectType.Properties.Contains("SaleOrderFinance"))
			{
				DynamicObjectCollection salesFinances = xSalesData["SaleOrderFinance"] as DynamicObjectCollection;
				salesFinances[0]["PARTOFENTRYIDS"] = string.Join(",", request.SalesOrderDetailList.Select(n => Convert.ToString(n.OrderEntryId)).Take(1).ToArray());
			}

			if (!CheckDataCanChange(ctx, oldSalesData, request.SalesOrderDetailList.Select(n => n.OrderEntryId).ToList(), ref validateMsg))
			{
				throw new Exception(validateMsg);
			}
			xSalesData["FFORMID"] = xformMetadata.BusinessInfo.GetForm().Id;
			DynamicObjectCollection salesEntrys = xSalesData["SaleOrderEntry"] as DynamicObjectCollection;
			salesEntrys.Sort((DynamicObject n) => Convert.ToInt32(n["Seq"]));
			string value3 = "B";
			for (int i = 0; i < salesEntrys.Count; i++)
			{
				salesEntrys[i]["Seq"] = i + 1;
				salesEntrys[i]["ChangeType"] = value3;
			}
			xSalesData["ChangeReason"] = ResManager.LoadKDString("WebAPI变更", "00444533030032263", SubSystemType.SCM);

			var billView = FormMetadataUtils.CreateBillView(ctx, "SAL_XORDER");
			billView.Model.DataObject = xSalesData;

			billView.Model.SetValue("FCustPurchaseNo", request.CustPurchaseNo);
			billView.Model.SetValue("FChangeDate", DateTime.Now);
			billView.Model.SetValue("FChangerId", DBServiceHelper.ExecuteScalar<long>(ctx, "select FUSERID from T_SEC_USER where FNAME = @FNAME", 0, new SqlParam("@FNAME", KDDbType.String, request.OperatorName)));
			//发票信息
			if (request.InvoiceInfo != null)
			{
				billView.Model.SetValue("FInvoiceType", request.InvoiceInfo.InvoiceType);
				billView.Model.SetValue("FInvoiceTitle", request.InvoiceInfo.InvoiceTitle);
				billView.Model.SetValue("FInvoiceTel", request.InvoiceInfo.InvoiceTel);
				billView.Model.SetValue("FInvoiceAddress", request.InvoiceInfo.InvoiceAddress);
				billView.Model.SetValue("FTaxCode", request.InvoiceInfo.TaxCode);
				billView.Model.SetValue("FBankName", request.InvoiceInfo.BankName);
			}
			//收货信息
			if (request.DeliveryInfo != null)
			{
				//变更收货信息,需要检查是否已做发货通知单
				if ((!request.DeliveryInfo.DeliveryAddress1.Replace(" ", "").EqualsIgnoreCase(billView.Model.GetValue<string>("FReceiveAddress", 0, string.Empty).Replace(" ", ""))
					|| !request.DeliveryInfo.ConsigneeName.Replace(" ", "").EqualsIgnoreCase(billView.Model.GetValue<string>("FLinkMan", 0, string.Empty).Replace(" ", ""))
					|| !request.DeliveryInfo.ConsigneePhone.EqualsIgnoreCase(billView.Model.GetValue<string>("FLinkPhone", 0, string.Empty))
					) && request.IsDeliverNotice)
				{
					//response.Code = ResponseCode.ModelError;
					//response.Message = "订单已发货,不允许修改送货地址!";
					//return;
					throw new Exception("订单已发货,不允许修改送货地址!");
				}
				billView.Model.SetValue("FReceiveAddress", request.DeliveryInfo.DeliveryAddress1);
				billView.Model.SetValue("FLinkMan", request.DeliveryInfo.ConsigneeName);
				billView.Model.SetValue("FLinkPhone", request.DeliveryInfo.ConsigneePhone);
			}

			foreach (var detail in request.SalesOrderDetailList)
			{
				int row;
				var entry = salesEntrys.FirstOrDefault(p => Convert.ToInt64(p["FOrderDetailId"]) == detail.Id);
				if (entry == null)
				{
					if (detail.Qty == 0)
					{
						continue;
					}
					billView.Model.CreateNewEntryRow("FSaleOrderEntry");
					row = billView.Model.GetEntryRowCount("FSaleOrderEntry") - 1;
					NewEntryRow(billView, detail, row);
				}
				else
				{
					//组织间需求单存在或者发货通知单存在不允许变更发货日期
					if (detail.DeliveryDate != entry.GetValue<DateTime>("DeliveryDate") && (detail.IsPurchase || detail.IsDeliverNotice))
					{
						//response.Code = ResponseCode.ModelError;
						//response.Message = "存在组织间需求单或发货通知单时,不允许变更发货日期!";
						//return;
						throw new Exception("存在组织间需求单或发货通知单时,不允许变更发货日期!");
					}
					//数量变多含税单价变多如果结算方式是现金不允许变更
					if (detail.VatPrice > entry.GetValue<decimal>("TaxPrice", 0)
						|| detail.Qty > entry.GetValue<decimal>("PriceUnitQty", 0))
					{
						var settlemodeNumber = ((DynamicObject)((DynamicObjectCollection)billView.Model.DataObject["SaleOrderFinance"])
							.FirstOrDefault()["SettleModeId"])["Number"].ToString();
						if (settlemodeNumber.Contains("Alipay") || settlemodeNumber.Contains("WeChat"))
						{
							//response.Code = ResponseCode.ModelError;
							//response.Message = "物料数量以及单价变多时,现金订单不允许变更!";
							//return;
							throw new Exception("物料数量以及单价变多时,现金订单不允许变更!");
						}
					}

					if (detail.SupplyOrgId != Convert.ToInt64(entry["FSupplyTargetOrgId_Id"]) && detail.IsPurchase)
					{
						throw new Exception("存在组织间需求单时，不允许变更供货组织!");
					}

					//变更物料,删除物料,或者变更数量,需要判断是否存在组织间需求单,以及采购单
					if ((!detail.ItemNo.EqualsIgnoreCase((entry["MaterialId"] as DynamicObject).GetValue<string>("Number"))
							|| detail.IsDelete
							|| detail.Qty != entry.GetValue<decimal>("PriceUnitQty", 0)) && detail.IsPurchase)
					{
						//response.Code = ResponseCode.ModelError;
						//response.Message = "变更物料,删除物料,或者变更数量时,需要联系采购/生产先处理订单关闭组织间需求单!";
						//return;
						throw new Exception("变更物料,删除物料,或者变更数量时,需要联系采购/生产先处理订单关闭组织间需求单!");
					}
					//变更物料,删除物料,或者变更数量,已发货不允许变更
					if ((!detail.ItemNo.EqualsIgnoreCase((entry["MaterialId"] as DynamicObject).GetValue<string>("Number"))
							|| detail.IsDelete
							|| detail.Qty != entry.GetValue<decimal>("PriceUnitQty", 0)) && detail.IsDeliverNotice)
					{
						//response.Code = ResponseCode.ModelError;
						//response.Message = "变更物料,删除物料,或者变更数量减少变更时,已发货不能变更!";
						//return;
						throw new Exception("变更物料,删除物料,或者变更数量减少变更时,已发货不能变更!");
					}
					//变更单价时,如果已经做了应收单,不能变更
					if (detail.VatPrice != entry.GetValue<decimal>("TaxPrice", 0) && detail.IsReceivable)
					{
						//response.Code = ResponseCode.ModelError;
						//response.Message = "已生成应收单,不能变更单价!";
						//return;
						throw new Exception("已生成应收单,不能变更单价!");
					}

					row = salesEntrys.IndexOf(entry);
					if (detail.IsDelete)
					{
						billView.Model.SetValue("FChangeType", "T", row);
						billView.InvokeFieldUpdateService("FChangeType", row);
						billView.Model.SetValue("FTaxPrice", 0, row);
						billView.InvokeFieldUpdateService("FTaxPrice", row);
						continue;
					}
					if (!detail.ItemNo.EqualsIgnoreCase((entry["MaterialId"] as DynamicObject).GetValue<string>("Number")))
					{
						billView.Model.SetValue("FChangeType", "D", row);
						billView.InvokeFieldUpdateService("FChangeType", row);
						billView.Model.SetValue("FTaxPrice", 0, row);
						billView.InvokeFieldUpdateService("FTaxPrice", row);
						row++;
						if (row == billView.Model.GetEntryRowCount("FSaleOrderEntry"))
						{
							billView.Model.CreateNewEntryRow("FSaleOrderEntry");
						}
						else
						{
							billView.Model.InsertEntryRow("FSaleOrderEntry", row);
						}
						NewEntryRow(billView, detail, row);
					}
					else
					{
						//billView.Model.SetValue("FMapId", detail.MaterialMapId, row);
						billView.Model.SetValue("FMaterialId", detail.MaterialId, row);
					}
				}
				//先屏蔽物料的赋值.物料变更需要删除或者关闭当前行,然后在新增行
				billView.Model.SetValue("FCustItemNo", detail.CustItemNo, row);
				billView.Model.SetValue("FCustItemName", detail.CustItemName, row);
				billView.Model.SetValue("FQty", detail.Qty, row);
				billView.InvokeFieldUpdateService("FQty", row);
				billView.Model.SetValue("FIsFree", detail.VatPrice == 0, row);

				billView.Model.SetValue("FTaxPrice", detail.VatPrice, row);
				billView.Model.SetValue("FEntryTaxRate", 13, row);
				billView.InvokeFieldUpdateService("FTaxPrice", row);

				billView.Model.SetValue("FProjectNo", detail.ProjectNo, row);
				billView.Model.SetValue("FEntryNote", detail.Remark, row);
				billView.Model.SetValue("FDeliveryDate", detail.DeliveryDate, row);
				billView.Model.SetValue("FStockFeatures", detail.StockFeatures, row);
				billView.Model.SetValue("FLocFactory", detail.LocFactory, row);
				billView.Model.SetValue("FCustMaterialNo", detail.CustMaterialNo, row);
				billView.Model.SetValue("FSupplyTargetOrgId", detail.SupplyOrgId, row);
				billView.Model.SetValue("FBusinessDivisionId", detail.BusinessDivisionId, row);
				billView.Model.SetItemValueByNumber("FProductEngineerId", detail.ProductEngineerCode, row);
				billView.Model.SetItemValueByNumber("FProductManagerId", detail.ProductManagerCode, row);
			}
			if (request.KnockOff != xSalesData.GetValue<decimal>("FAllDisCount", 0) && request.SalesOrderDetailList.Where(x => x.IsReceivable).Count() > 0)
			{
				//response.Code = ResponseCode.ModelError;
				//response.Message = "已生成应收单,不能变更整单优惠!";
				//return;
				throw new Exception("已生成应收单,不能变更整单优惠!");
			}

			if (request.IsAlreadySave)
			{
				billView.Model.SetValue("FAllDisCount", 0);
				billView.InvokeFieldUpdateService("FAllDisCount", 0);
				billView.Model.SetValue("FAllDisCount", request.KnockOff);
				billView.InvokeFieldUpdateService("FAllDisCount", 0);

				SalesOrderServiceHelper.ChangeSalesOrder(ctx, billView.BusinessInfo, billView.Model.DataObject);
			}

			response.Code = ResponseCode.Success;
		}

		private void NewEntryRow(IBillView billView, ChangeSalesOrderRequest.Salesorderdetaillist detail, int row)
		{
			//billView.Model.SetValue("FMapId", detail.MaterialMapId, row);
			//billView.InvokeFieldUpdateService("FMapId", row);
			billView.Model.SetValue("FMaterialId", detail.MaterialId, row);
			billView.InvokeFieldUpdateService("FMaterialId", row);
			billView.Model.SetValue("FSupplyTargetOrgId", detail.SupplyOrgId, row);
			billView.Model.SetValue("FBusinessDivisionId", detail.BusinessDivisionId, row);
			billView.Model.SetItemValueByNumber("FProductEngineerId", detail.ProductEngineerCode, row);
			billView.Model.SetItemValueByNumber("FProductManagerId", detail.ProductManagerCode, row);
			billView.Model.SetValue("FOrderDetailId", detail.Id, row);
		}

		public bool CheckDataCanChange(Context context, DynamicObject salorder, List<long> changeEntrysId, ref string validateMsg)
		{
			long num = Convert.ToInt64(salorder["ID"]);
			if (num <= 0)
			{
				return true;
			}
			string str = Convert.ToString(salorder["DocumentStatus"]);
			if (!str.EqualsIgnoreCase("C"))
			{
				validateMsg = ResManager.LoadKDString("订单不是已审核状态，不能变更!", "00444533030032265", SubSystemType.SCM);
				return false;
			}
			ISaleService saleService = K3.SCM.Contracts.ServiceFactory.GetSaleService(context);
			DynamicObjectCollection networkCtrlList = saleService.GetNetworkCtrlList(context, new List<long> { num }, buildNetCtlMsgFlag: true);
			if (networkCtrlList != null && networkCtrlList.Count > 0)
			{
				validateMsg = Convert.ToString(networkCtrlList[0]["FOPERATIONDESC"]);
				return false;
			}
			long orgId = Convert.ToInt64(salorder["SaleOrgId_Id"]);
			object systemProfile = CommonServiceHelper.GetSystemProfile(context, orgId, "SAL_SystemParameter", "CloseSOAlterMaterial", false);
			bool flag = systemProfile != null && Convert.ToBoolean(systemProfile);
			string str2 = Convert.ToString(salorder["CloseStatus"]);
			if (!flag && str2.EqualsIgnoreCase("B"))
			{
				validateMsg = ResManager.LoadKDString("参数【已关闭订单/合同可以变更物料】未勾选，订单已关闭不能变更。", "004072030009062", SubSystemType.SCM);
				return false;
			}
			DynamicObjectCollection dynamicObjectCollection = salorder["SaleOrderEntry"] as DynamicObjectCollection;
			List<long> list = new List<long>();
			Dictionary<long, string> dictionary = new Dictionary<long, string>();
			string text = "";
			long num3;
			foreach (DynamicObject item in dynamicObjectCollection)
			{
				if (!Convert.ToBoolean(item["FLOCKFLAG"]) || !(Convert.ToDecimal(item["LOCKQTY"]) > 0m))
				{
					continue;
				}
				string empty = ((!(item["Materialid"] is DynamicObject dynamicObject2)) ? "" : Convert.ToString(dynamicObject2["Name"]));
				num3 = Convert.ToInt64(item["Id"]);
				if (!list.Contains(num3))
				{
					if (changeEntrysId != null && changeEntrysId.Count > 0 && !changeEntrysId.Contains(num3))
					{
						continue;
					}
					list.Add(num3);
				}
				if (!dictionary.ContainsKey(num3))
				{
					dictionary.Add(num3, empty);
				}
			}
			if (list.Count > 0)
			{
				DynamicObjectCollection lockEntryIds = SaleServiceHelper.GetLockEntryIds(context, list);
				if (lockEntryIds != null)
				{
					foreach (DynamicObject item2 in lockEntryIds)
					{
						num3 = Convert.ToInt64(item2["FENTRYID"]);
						int num2 = Convert.ToInt16(item2["FSEQ"]);
						text = text + string.Format(ResManager.LoadKDString("第【{0}】行物料【{1}】存在锁库数据;", "004019000039852", SubSystemType.SCM), num2, dictionary[num3]) + "\r\n";
					}
				}
			}
			if (!text.IsNullOrEmptyOrWhiteSpace())
			{
				text = (validateMsg = text + ResManager.LoadKDString("请先解锁后再进行变更操作！", "004019000023771", SubSystemType.SCM));
				return false;
			}
			if (SaleServiceHelper.CheckSOExistsNotActivedData(context, num))
			{
				validateMsg = ResManager.LoadKDString("订单已存在未生效的变更单信息，不能再次执行变更操作！", "004019000018743", SubSystemType.SCM);
				return false;
			}
			return true;
		}

		public List<string> GetTakeEffectNotWriteBackFields(FormMetadata salxOrderMeta)
		{
			FormOperation formOperation = salxOrderMeta.BusinessInfo.GetForm().FormOperations.FirstOrDefault((FormOperation n) => n.Id.EqualsIgnoreCase("TakeEffectNotWriteBackFields"));
			if (formOperation == null)
			{
				return new List<string>();
			}
			List<string> list = (formOperation.LoadKeys.IsNullOrEmptyOrWhiteSpace() ? null : formOperation.LoadKeys.Replace("\"", "").Replace("[", "").Replace("]", "")
				.Split(',')
				.ToList());
			List<string> list2 = new List<string>();
			foreach (string item in list)
			{
				var field = salxOrderMeta.BusinessInfo.GetField(item);
				if (field != null)
				{
					list2.Add(field.PropertyName);
				}
			}
			return list2;
		}
		public void SetBillTypeValue(BusinessInfo xorderBusinessInfo, BusinessInfo orderBusineInfo, DynamicObject salOrder, DynamicObject xSalOrder)
		{
			if (xorderBusinessInfo != null)
			{
				string propertyName = xorderBusinessInfo.GetBillTypeField().PropertyName;
				if (!propertyName.IsNullOrEmptyOrWhiteSpace())
				{
					string value = Convert.ToString(salOrder[orderBusineInfo.GetBillTypeField().PropertyName + "_Id"]);
					xSalOrder[propertyName + "X"] = value;
				}
			}
		}

		public DynamicObject FilterChangeDataByEntryIds(DynamicObject objData, ChangeSalesOrderRequest.Salesorderdetaillist[] salesOrderDetailList)
		{
			DynamicObjectCollection salesOrderEntrys = objData["SaleOrderEntry"] as DynamicObjectCollection;
			List<DynamicObject> removeEntrys = new List<DynamicObject>();
			foreach (DynamicObject item in salesOrderEntrys)
			{
				var detail = salesOrderDetailList.FirstOrDefault(p => p.Id == Convert.ToInt64(item["FOrderDetailId"]));
				if (item.GetValue<string>("MrpTerminateStatus").EqualsIgnoreCase("B"))
				{
					removeEntrys.Add(item);
				}

				if (detail != null)
				{
					detail.OrderEntryId = Convert.ToInt64(item["PKIDX"]);
				}
			}
			removeEntrys.ForEach(p => salesOrderEntrys.Remove(p));
			DynamicObjectCollection salesFinances = objData["SaleOrderFinance"] as DynamicObjectCollection;
			DynamicObjectCollection saleOrderPlans = objData["SaleOrderPlan"] as DynamicObjectCollection;
			if (salesFinances[0]["RecConditionId"] is DynamicObject dynamicObject && Convert.ToInt16(dynamicObject["RECMETHOD"]) == 3)
			{
				List<DynamicObject> removePlans = new List<DynamicObject>();
				foreach (DynamicObject plan in saleOrderPlans)
				{
					if (removeEntrys.Any(p => Convert.ToInt64(p["PKIDX"]) == Convert.ToInt64(plan["OrderEntryId"])))
					{
						removePlans.Add(plan);
					}
				}
				removePlans.ForEach(p => saleOrderPlans.Remove(p));
				return objData;
			}
			return objData;
		}
	}
}
