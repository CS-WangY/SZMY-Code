using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.BusinessFlow.ServiceArgs;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.Core.MFG.PLN.Reserved.ReserveArgs;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN.Reserve;
using Kingdee.K3.SCM.Core;
using Kingdee.K3.SCM.Core.SAL;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Kingdee.Mymooo.Core.SalesManagement.SalesOrderBillRequest;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
	public class SalesOrderBusiness : IMessageExecute
	{
		/// <summary>
		/// 销售订单构建预测单关系转移预留
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="bill"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public ResponseMessage<dynamic> SalesOrder2Forecast(Context ctx, string message)
		{
			SalesOrderBillRequest request = JsonConvertUtils.DeserializeObject<SalesOrderBillRequest>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			response.Code = ResponseCode.Success;
			try
			{
				string sSql = @"SELECT t1.FID,t3.FSALEORGID,t3.FCUSTID,t1.FENTRYID,FSEQ,FQTY,FMATERIALID,t2.FCANOUTQTY FROM dbo.T_SAL_ORDERENTRY t1
                INNER JOIN dbo.T_SAL_ORDERENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
                INNER JOIN T_SAL_ORDER t3 ON t1.FID=t3.FID
                WHERE t3.FID=@FBILLID AND t2.FCANOUTQTY>0";
				var sqlParams = new SqlParam[]
				{
				new SqlParam("@FBILLID",KDDbType.Int64,request.SaleBillId)
				};
				DynamicObjectCollection saleEntitylist = DBServiceHelper.ExecuteDynamicObject(ctx, sSql, paramList: sqlParams);
				List<SaleOrder2FoPushEntity> so2fo = new List<SaleOrder2FoPushEntity>();
				foreach (var salEntity in saleEntitylist)
				{
					var salOrgId = Convert.ToInt64(salEntity["FSALEORGID"]);
					var entryid = Convert.ToInt64(salEntity["FENTRYID"]);
					var materialid = Convert.ToInt64(salEntity["FMATERIALID"]);
					var custid = Convert.ToInt64(salEntity["FCUSTID"]);
					var salqty = Convert.ToDecimal(salEntity["FCANOUTQTY"]);

					so2fo.Add(new SaleOrder2FoPushEntity
					{
						SaleOrgId = salOrgId,
						SaleBillId = request.SaleBillId,
						SaleEntryId = entryid,
						MaterialId = materialid,
						CustId = custid,
						Qty = salqty
					});
				}
				TransferSO2FOReserved(ctx, so2fo);
				List<long> materiallist = new List<long>();
				foreach (var item in request.SalesOrderDetailList)
				{
					materiallist.Add(item.MaterialMasterId);
				}
				response.Data = materiallist;
			}
			catch (Exception err)
			{
				response.Code = ResponseCode.Exception;
				response.ErrorMessage = err.Message;
			}
			return response;
		}
		private void TransferSO2FOReserved(Context ctx, List<SaleOrder2FoPushEntity> tranList)
		{
			foreach (var item in tranList)
			{
				//查询有效的预测单信息
				string sql = @"SELECT top 1 t1.FID,t1.FENTRYID,t2.FBILLNO,t1.FCUSTID,t1.FMATERIALID,t1.FQTY-t1.FWRITEOFFQTY AS FEOFQTY,t1.FSTARTDATE,t1.FENDDATE
                        FROM dbo.T_PLN_FORECASTENTRY t1 INNER JOIN T_PLN_FORECAST t2 ON t1.FID=t2.FID
                        WHERE t1.FMATERIALID=@FMATERIALID
                        AND t1.FCUSTID=@FCUSTID
						AND t2.FFOREORGID=@FSALEORGID
                        AND t1.FQTY-t1.FWRITEOFFQTY>0
                        AND t1.FENDDATE>=GETDATE()
                        AND t1.FCLOSESTATUS='A'
                        AND t2.FDOCUMENTSTATUS='C'
                        ORDER BY t1.FSTARTDATE";
				var sqlParams = new SqlParam[]
				{
						new SqlParam("@FMATERIALID",KDDbType.Int64,item.MaterialId),
						new SqlParam("@FCUSTID",KDDbType.Int64,item.CustId),
						new SqlParam("@FSALEORGID",KDDbType.Int64,item.SaleOrgId)
				};
				DynamicObjectCollection plnfolist = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams);
				foreach (var foitem in plnfolist)
				{
					if (item.Qty - Convert.ToInt64(foitem["FEOFQTY"]) > 0)
					{
						item.Qty = Convert.ToInt64(foitem["FEOFQTY"]);
					}

					using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
					{
						var billView = FormMetadataUtils.CreateBillView(ctx, "SAL_SaleOrder", item.SaleBillId);
						var salerow = ((DynamicObjectCollection)billView.Model.DataObject["SaleOrderEntry"]).Where(x => Convert.ToInt64(x["Id"]) == item.SaleEntryId).First();
						//foreach (var salerow in (DynamicObjectCollection)billView.Model.DataObject["SaleOrderEntry"])
						//{
						//	if (Convert.ToInt64(salerow["Id"]) == item.SaleEntryId)
						//	{

						//	}
						//}
						salerow["SRCTYPE"] = "PLN_FORECAST";
						salerow["SRCBILLNO"] = foitem["FBILLNO"];
						salerow["FPENY_FO2SOQTY"] = item.Qty;

						var ss = (DynamicObjectCollection)salerow["FSaleOrderEntry_Link"];
						ss.Clear();
						// 切换到单据体的某一行
						// 注意：这句非常重要，子单据体的数据，是依附在单据体的某一行上，而不是一个独立存在的集合。
						//billView.Model.SetEntryCurrentRowIndex("SaleOrderEntry", Convert.ToInt32(salerow["Seq"]) - 1);
						//删除源关联关系
						//billView.Model.DeleteEntryData("FSaleOrderEntry_Link");

						//billView.Model.CreateNewEntryRow("FSaleOrderEntry_Link");
						var entity = billView.Model.BillBusinessInfo.GetEntryEntity("FSaleOrderEntry_Link") as SubEntryEntity;
						billView.Model.CreateNewEntryRow(salerow, entity, 0);
						((DynamicObjectCollection)salerow["FSaleOrderEntry_Link"])[0]["SBILLID"] = foitem["FID"];
						((DynamicObjectCollection)salerow["FSaleOrderEntry_Link"])[0]["SID"] = foitem["FENTRYID"];
						((DynamicObjectCollection)salerow["FSaleOrderEntry_Link"])[0]["RULEID"] = "FO2SO";
						((DynamicObjectCollection)salerow["FSaleOrderEntry_Link"])[0]["STABLENAME"] = "T_PLN_FORECASTENTRY";

						//((DynamicObjectCollection)salerow["FSaleOrderEntry_Link"])[0]["FPENY_FO2SOQTYOLD"] = item.Qty;
						//billView.Model.DeleteEntryData("FSaleOrderEntry_Link");
						var saveResult = BusinessDataServiceHelper.Save(ctx, billView.BusinessInfo, billView.Model.DataObject);
						//清除释放网控
						billView.CommitNetworkCtrl();
						billView.InvokeFormOperation(FormOperationEnum.Close);
						billView.Close();
						if (!saveResult.IsSuccess)
						{
							if (saveResult.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", saveResult.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", saveResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
						var sSqlu = $@"/*dialect*/UPDATE dbo.T_PLN_FORECASTENTRY SET FWRITEOFFQTY=FWRITEOFFQTY+{item.Qty},FBASEWRITEOFFQTY=FBASEWRITEOFFQTY+{item.Qty}
						WHERE FENTRYID={foitem["FENTRYID"]}";
						DBServiceHelper.Execute(ctx, sSqlu);
						sSqlu = $@"/*dialect*/UPDATE dbo.T_PLN_FORECASTENTRY SET FCLOSESTATUS='C' WHERE FWRITEOFFQTY>=FQTY AND FENTRYID={foitem["FENTRYID"]}";
						DBServiceHelper.Execute(ctx, sSqlu);

						cope.Complete();
					}
					//获取销售订单下需求来源为预测单的组织间需求单
					var sSql = $@"SELECT t1.FENTRYID,t1.FSUPPLYINTERID FROM T_PLN_RESERVELINKENTRY t1
					           INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
					           INNER JOIN T_PLN_REQUIREMENTORDER t3 ON t1.FSUPPLYINTERID=t3.FID
					           WHERE t2.FDEMANDENTRYID='{item.SaleEntryId}' AND t3.FDEMANDTYPE=2";
					DynamicObjectCollection saldatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
					//删除冲销预留记录，创建销售订单新组织间需求单
					List<string> sqllist = new List<string>();
					foreach (var soroitem in saldatas)
					{
						sqllist.Add($"DELETE T_PLN_RESERVELINKENTRY WHERE FENTRYID={Convert.ToString(soroitem["FENTRYID"])}");
					}
					DBServiceHelper.ExecuteBatch(ctx, sqllist);

					List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
					StockPushEntity push = new StockPushEntity();
					var row = new ListSelectedRow(Convert.ToString(item.SaleBillId), Convert.ToString(item.SaleEntryId), 0, "SAL_SaleOrder");
					row.EntryEntityKey = "FSaleOrderEntry"; //这里最容易忘记加，是重点的重点
					selectedRows.Add(row);
					push.listSelectedRow = selectedRows;
					push.ConvertRule = "PENY_SalOrder_REQUIREMENTORDER";
					var result = this.SaleBillPushRo(ctx, push, item.Qty);
					long salroid = 0;
					if (!result.IsSuccess)
					{
						if (result.ValidationErrors.Count > 0)
						{
							throw new Exception(string.Join(";", result.ValidationErrors.Select(p => p.Message)));
						}
						else
						{
							throw new Exception(string.Join(";", result.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
						}
					}
					else
					{
						salroid = Convert.ToInt64(result.OperateResult.Select(x => x.PKValue).FirstOrDefault());
					}

					var baseqty = item.Qty;
					foreach (var roid in saldatas)
					{
						sSql = $@"SELECT FID FROM T_PLN_RESERVELINK WHERE FDEMANDINTERID='{roid["FSUPPLYINTERID"]}'";
						long resid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
						if (resid > 0)
						{
							//加载预测单预留信息
							var fobillView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", resid);
							var foentryView = fobillView.Model.DataObject["Entity"] as DynamicObjectCollection;
							List<resEntryInfo> reslist = new List<resEntryInfo>();
							foreach (var resitem in foentryView.OrderBy(b => b["SupplyDate"]))
							{
								if (baseqty > 0)
								{
									resEntryInfo entryInfo = new resEntryInfo();
									decimal qty = Convert.ToDecimal(resitem["BaseSupplyQty"]);
									entryInfo.SupplyFormID = Convert.ToString(resitem["SupplyFormID_Id"]);
									entryInfo.SupplyInterID = Convert.ToString(resitem["SupplyInterID"]);
									entryInfo.SupplyEntryID = Convert.ToString(resitem["SupplyEntryID"]);
									entryInfo.SupplyBillNO = Convert.ToString(resitem["SupplyBillNo"]);
									entryInfo.SupplyMaterialID = Convert.ToString(resitem["SupplyMaterialID_Id"]);
									entryInfo.SupplyOrgID = Convert.ToString(resitem["SupplyOrgId_Id"]);
									entryInfo.SupplyStockID = Convert.ToString(resitem["SupplyStockID_Id"]);
									entryInfo.BaseSupplyUnitID = Convert.ToString(resitem["BaseSupplyUnitID_Id"]);
									entryInfo.SupplyDate = Convert.ToDateTime(resitem["SupplyDate"]);
									entryInfo.Supplypriority = Convert.ToInt32(resitem["SupplyPriority"]);
									entryInfo.Ismto = Convert.ToInt32(resitem["IsMto"]);
									entryInfo.YieldRate = Convert.ToDecimal(resitem["YieldRate"]);
									entryInfo.Linktype = Convert.ToInt32(resitem["LinkType"]);
									entryInfo.EntryPkId = Convert.ToString(resitem["EntryPkId"]);
									entryInfo.Consumpriority = Convert.ToInt32(resitem["ConsumPriority"]);
									entryInfo.GenerateId = Convert.ToString(resitem["GenerateId"]);
									entryInfo.IntsupplyID = Convert.ToInt64(resitem["IntSupplyId"]);
									entryInfo.IntsupplyEntryID = Convert.ToInt64(resitem["IntSuppyEntryId"]);

									if (qty - baseqty >= 0)
									{
										resitem["BaseSupplyQty"] = qty - baseqty;
										entryInfo.BaseSupplyQty = baseqty;
										baseqty = 0;
									}
									else
									{
										resitem["BaseSupplyQty"] = 0;
										entryInfo.BaseSupplyQty = qty;
										baseqty = baseqty - qty;
									}

									reslist.Add(entryInfo);
								}
							}
							var rowcount = fobillView.Model.GetEntryRowCount("FEntity");
							for (int i = rowcount; i >= 0; i--)
							{
								var eqty = fobillView.Model.GetValue<decimal>("FBaseSupplyQty", i, 0);
								if (eqty <= 0)
								{
									fobillView.Model.DeleteEntryRow("FEntity", i);
								}
							}
							List<DynamicObject> list = new List<DynamicObject>();
							if (fobillView.Model.GetEntryRowCount("FEntity") <= 0)
							{
								OperateOption option = OperateOption.Create();
								BusinessDataServiceHelper.Delete(ctx, fobillView.BusinessInfo, new object[] { resid }, option);
							}
							else
							{
								list.Add(fobillView.Model.DataObject);
								var saveFoResult = BusinessDataServiceHelper.Save(ctx, fobillView.BusinessInfo, list.ToArray());
								if (!saveFoResult.IsSuccess)
								{
									if (saveFoResult.ValidationErrors.Count > 0)
									{
										throw new Exception(string.Join(";", saveFoResult.ValidationErrors.Select(p => p.Message)));
									}
									else
									{
										throw new Exception(string.Join(";", saveFoResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
									}
								}
							}
							//清除释放网控
							fobillView.CommitNetworkCtrl();
							fobillView.InvokeFormOperation(FormOperationEnum.Close);
							fobillView.Close();

							//创建销售订单需求预留
							CreateSaleBillReserveLink(ctx, salroid, reslist);
						}
						//获取预测单组织间需求单对象
						var view = FormMetadataUtils.CreateBillView(ctx, "PLN_REQUIREMENTORDER", Convert.ToInt64(roid["FSUPPLYINTERID"]));
						using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
						{
							var OldRoQty = Convert.ToDecimal(view.Model.DataObject["ReMainQty"]);
							view.Model.DataObject["ReMainQty"] = OldRoQty - item.Qty;
							view.Model.DataObject["ReMainBaseQty"] = OldRoQty - item.Qty;
							var oper = BusinessDataServiceHelper.Save(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
							//如果需求单扣减数量为零则关闭需求单
							if (OldRoQty - item.Qty <= 0)
							{
								SetStatusService setStatusService = new SetStatusService();
								var operateOption = OperateOption.Create();
								operateOption.SetIgnoreWarning(true);
								List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
								keyValuePairs.Add(new KeyValuePair<object, object>(roid["FSUPPLYINTERID"], ""));
								setStatusService.SetBillStatus(ctx, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
							}
							cope.Complete();
						}
						//清除释放网控
						view.CommitNetworkCtrl();
						view.InvokeFormOperation(FormOperationEnum.Close);
						view.Close();
					}
				}
			}
		}
		private IOperationResult SaleBillPushRo(Context ctx, StockPushEntity pushEntity, decimal salqty)
		{
			//得到转换规则
			var convertRule = this.GetConvertRule(ctx, pushEntity.ConvertRule);
			OperateOption pushOption = OperateOption.Create();//操作选项
															  //构建下推参数
															  //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
															  //单据下推参数
			PushArgs pushArgs = new PushArgs(convertRule, pushEntity.listSelectedRow.ToArray());
			//目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
			pushArgs.TargetOrgId = pushEntity.TargetOrgId;
			//目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
			pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
			// 自动下推，无需验证用户功能权限
			pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
			// 设置是否整单下推
			//pushOption.SetVariableValue(ConvertConst., false);

			var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
			var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
			foreach (DynamicObject targeEntry in targetObjs)
			{
				var mid = Convert.ToInt64(targeEntry["MaterialId_Id"]);
				var unitid = Convert.ToInt64(targeEntry["UnitID_Id"]);
				var baseunitid = Convert.ToInt64(targeEntry["BaseUnitId_Id"]);
				var baseqty = salqty;
				IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
				if (unitid != baseunitid)
				{
					baseqty = convertService.GetUnitTransQty(ctx, mid, unitid, baseunitid, salqty);
				}
				targeEntry["DemandQty"] = salqty;
				targeEntry["BaseDemandQty"] = baseqty;
				targeEntry["FirmQty"] = salqty;
				targeEntry["BaseFirmQty"] = baseqty;
				targeEntry["ReMainQty"] = salqty;
				targeEntry["ReMainBaseQty"] = baseqty;
				targeEntry["F_PENY_Amount"] = salqty * Convert.ToDecimal(targeEntry["F_PENY_Price"]);
			}
			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//对转换结果进行处理
			//1. 直接调用保存接口，对数据进行保存
			return MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, targetBInfo, targetObjs);
		}
		//得到转换规则
		private ConvertRuleElement GetConvertRule(Context ctx, string convertRuleId)
		{
			var convertRuleMeta = ConvertServiceHelper.GetConvertRule(ctx, convertRuleId);
			return convertRuleMeta.Rule;
		}
		// 得到表单元数据
		private BusinessInfo GetBusinessInfo(Context ctx, string formId, FormMetadata metaData = null)
		{
			if (metaData != null) return metaData.BusinessInfo;
			metaData = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
			return metaData.BusinessInfo;
		}
		public ResponseMessage<SalesOrderBillRequest> SyncCreateSalesOrderReceivebill(Context ctx, string message)
		{
			SalesOrderBillRequest request = JsonConvertUtils.DeserializeObject<SalesOrderBillRequest>(message);
			ResponseMessage<SalesOrderBillRequest> response = new ResponseMessage<SalesOrderBillRequest>() { Data = request };

			if (request.PaidAmount > 0)
			{
				if (request.PayType.EqualsIgnoreCase("WeChat") || request.PayType.EqualsIgnoreCase("Alipay"))
				{
					string sSql = $"SELECT FPARENTID FROM dbo.T_ORG_ORGANIZATIONS WHERE FORGID={request.OrgId}";
					var payorgid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
					FormMetadata meta = MetaDataServiceHelper.Load(ctx, "AR_RECEIVEBILL") as FormMetadata;
					var billView = FormMetadataUtils.CreateBillView(ctx, "AR_RECEIVEBILL");
					//收款组织
					billView.Model.SetItemValueByID("FPAYORGID", payorgid, 0);
					billView.InvokeFieldUpdateService("FPAYORGID", 0);
					//结算组织
					billView.Model.SetItemValueByID("FSETTLEORGID", request.OrgId, 0);
					billView.InvokeFieldUpdateService("FPAYORGID", 0);

					billView.Model.SetItemValueByID("FCONTACTUNIT", request.CustomerInfo.Id, 0);
					billView.InvokeFieldUpdateService("FCONTACTUNIT", 0);

					billView.Model.DeleteEntryData("FRECEIVEBILLENTRY");
					var rowcount = 0;
					billView.Model.CreateNewEntryRow("FRECEIVEBILLENTRY");
					//结算方式
					billView.Model.SetItemValueByNumber("FSETTLETYPEID", request.PayType, rowcount);
					billView.InvokeFieldUpdateService("FSETTLETYPEID", rowcount);
					//收款用途预收款
					billView.Model.SetItemValueByNumber("FPURPOSEID", "SFKYT02_SYS", rowcount);
					billView.InvokeFieldUpdateService("FPURPOSEID", rowcount);

					billView.Model.SetValue("FRECTOTALAMOUNTFOR", request.PaidAmount, rowcount);
					billView.InvokeFieldUpdateService("FRECTOTALAMOUNTFOR", rowcount);
					billView.Model.SetValue("FHANDLINGCHARGEFOR", request.PayHandFee, rowcount);
					billView.InvokeFieldUpdateService("FHANDLINGCHARGEFOR", rowcount);
					//获取结算组织内部账号
					sSql = $"SELECT TOP 1 FID FROM T_CN_INNERACCOUNT WHERE FMAPPINGORGID={request.OrgId}";
					var inneraccountid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
					billView.Model.SetValue("FINNERACCOUNTID", inneraccountid, rowcount);
					//增加备注
					billView.Model.SetValue("FCOMMENT", "预收-" + request.CustomerInfo.Name + "-货款", rowcount);
					//核销金额
					//billView.Model.SetValue("FWRITTENOFFAMOUNTFOR_D", request.PaidAmount, rowcount);
					//核销状态
					//billView.Model.SetValue("FWRITTENOFFSTATUS_D", "C", rowcount);

					IOperationResult oper = new OperationResult();
					using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
					{
						var operateOption = OperateOption.Create();
						operateOption.SetIgnoreWarning(true);

						SaveService saveService = new SaveService();
						oper = saveService.SaveAndAudit(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
						if (!oper.IsSuccess)
						{
							if (oper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
						cope.Complete();
					}
					//response.Data.PlanentryList
					response.Data.PlanentryList = oper.SuccessDataEnity.Select(x => new SalesOrderPlanEntry
					{
						AdvanceNo = Convert.ToString(x["BillNo"]),
						ADVANCEID = Convert.ToInt64(x["Id"]),
						AdvanceSeq = Convert.ToInt32(((DynamicObjectCollection)x["RECEIVEBILLENTRY"]).Select(e => e["Seq"]).FirstOrDefault()),
						ADVANCEENTRYID = Convert.ToInt64(((DynamicObjectCollection)x["RECEIVEBILLENTRY"]).Select(e => e["Id"]).FirstOrDefault()),
						ActRecAmount = Convert.ToDecimal(((DynamicObjectCollection)x["RECEIVEBILLENTRY"]).Select(e => e["RECTOTALAMOUNTFOR"]).FirstOrDefault()),
						RemainAmount = request.SalesOrderVatNet,
						//PREMATCHAMOUNTFOR = Convert.ToInt64(((DynamicObjectCollection)x["RECEIVEBILLENTRY"]).Select(e => e["Seq"]).FirstOrDefault()),
						SettleOrgId_Id = Convert.ToInt64(x["SETTLEORGID_Id"]),
						//SettleOrgId = Convert.ToString(x.DataEntity["aa"]),
					}).ToArray();
				}
			}

			response.Code = ResponseCode.Success;
			return response;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public ResponseMessage<SalesOrderBillRequest> SyncSalesOrderMaterial(Context ctx, string message)
		{
			SalesOrderBillRequest request = JsonConvertUtils.DeserializeObject<SalesOrderBillRequest>(message);
			ResponseMessage<SalesOrderBillRequest> response = new ResponseMessage<SalesOrderBillRequest>() { Data = request };
			if (string.IsNullOrWhiteSpace(request.SalesOrderNo))
			{
				response.Code = ResponseCode.Abort;
				response.Message = "销售订单号不能为空！";
				return response;
			}
			if (DBServiceHelper.ExecuteScalar<bool>(ctx, "select 1 from T_SAL_ORDER where FBILLNO = @FBILLNO", false, new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo)))
			{
				response.Code = ResponseCode.Abort;
				response.Message = "销售订单号已经存在！";
				return response;
			}
			if (request.PayTermInfo == null)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "结算方式不能为空！";
				return response;
			}
			var org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.OrgNumber, ""));
			if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C"))
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "对应的组织不存在或未审核";
				return response;
			}
			request.OrgId = org.Id;
			request.OrgName = GetOrgName(ctx, org.Id);
			CustomerServcie customerServcie = new CustomerServcie();
			var customer = customerServcie.TryGetOrAdd(ctx, new CustomerInfo(request.CustomerInfo.Code, request.CustomerInfo.Name));
			if (customer.Id == 0 || !customer.DocumentStatus.EqualsIgnoreCase("C"))
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "对应的客户不存在或未审核";
				return response;
			}
			request.CustomerInfo.Id = customer.Id;
			//平台结算方式对应金蝶付款条件
			var payTerm = FormMetadataUtils.GetIdForNumber(ctx, new PayTermInfo(request.PayTermInfo.Code, request.PayTermInfo.Desc));
			if (payTerm.Id == 0)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "对应的结算方式不存在或未审核";
				return response;
			}

			request.SalesInfo.Id = BasicDataSyncServiceHelper.GetSaleId(ctx, request.OrgId, request.SalesInfo.Code);
			if (request.SalesInfo.Id == 0)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "对应的业务员不存在或未审核";
				return response;
			}
			request.PayTermInfo.Id = payTerm.Id;
			foreach (var details in request.SalesOrderDetailList)
			{
				if (!string.IsNullOrWhiteSpace(details.WeightUnitid))
				{
					details.WeightUnitid = Convert.ToString(GetUnitId(ctx, details.WeightUnitid));
				}
				else
				{
					details.WeightUnitid = "0";
				}
				if (!string.IsNullOrWhiteSpace(details.VolumeUnitid))
				{
					details.VolumeUnitid = Convert.ToString(GetUnitId(ctx, details.VolumeUnitid));
				}
				else
				{
					details.VolumeUnitid = "0";
				}
			}
			//if (request.OrgNumber == "SZMYGC" || request.OrgNumber=="HDFI")
			//{
			//	string sql = @"/*dialect*/select 1 from T_SAL_SCSALERCUST a
			//                         left join V_BD_SALESMANENTRY b on a.FSALERGROUPID = b.FOPERATORGROUPID
			//                         left join V_BD_SALESMAN c on b.fid = c.Fid
			//                         left join T_BD_CUSTOMER d on a.FCUSTOMERID = d.FCUSTID
			//                         left join T_ORG_ORGANIZATIONS e on c.FBIZORGID = e.FORGID
			//                         where (e.FNUMBER = 'SZMYGC' or e.FNUMBER = 'HDFI') and c.fempnumber=@SalesCode and d.FNUMBER=@Code ";
			//	List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SalesCode", KDDbType.String, request.SalesInfo.Code), new SqlParam("@Code", KDDbType.String, request.CustomerInfo.Code) };
			//	var isExitSalesGroup = DBServiceHelper.ExecuteScalar(ctx, sql, false, paramList: pars.ToArray());
			//	if (!isExitSalesGroup)
			//	{
			//		string data = $"{{\"UserCode\":\"{request.SalesInfo.Code}\",\"CustCode\":\"{request.CustomerInfo.Code}\",\"IsFirstSync\":false}}";
			//		SalesCustBusiness salesCust = new SalesCustBusiness();
			//		var result = salesCust.Execute(ctx, data);
			//		if (!result.IsSuccess)
			//		{
			//			response.Code = result.Code;
			//			response.Message = result.Message;
			//			return response;
			//		}
			//	}
			//}

			Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>(StringComparer.OrdinalIgnoreCase);
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
					if (request.OrgId == 7401803)
					{
						materialInfo.ErpClsID = 2;
					}
					materialInfo.Length = detail.Length;
					materialInfo.Width = detail.Width;
					materialInfo.Height = detail.Height;
					materialInfo.Weight = detail.Weight;
					materialInfo.Volume = detail.Volume;
					materialInfo.Textures = detail.Textures;
					materialInfo.ProductId = detail.ProductId;
					materialInfo.UseOrgId = request.OrgId;
					materialInfo.ShortNumber = detail.ShortNumber.Trim();
					materialInfo.PriceType = detail.PriceType;
					materialInfo.ProductSmallClass = detail.ProductSmallClass;
					materialInfo.WeightUnitid = detail.WeightUnitid;
					materialInfo.VolumeUnitid = detail.VolumeUnitid;
					if (org.Id == detail.SupplyOrgId)
					{
						MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, new List<long>() { org.Id });
					}
					else
					{
						MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, new List<long>() { org.Id, detail.SupplyOrgId });
					}

					materials[detail.ItemNo] = materialInfo;
				}
				detail.MaterialId = materialInfo.Id;
				detail.MaterialMasterId = materialInfo.MasterId;
			}
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
			response.Code = ResponseCode.Success;
			return response;
		}

		public ResponseMessage<dynamic> CreateSalesOrder(Context ctx, string message)
		{
			SalesOrderBillRequest request = JsonConvertUtils.DeserializeObject<SalesOrderBillRequest>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>() { Data = request };
			if (DBServiceHelper.ExecuteScalar<bool>(ctx, "select 1 from T_SAL_ORDER where FBILLNO = @FBILLNO", false, new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo)))
			{
				response.Code = ResponseCode.Warning;
				response.Message = "销售订单号已经存在！";
				return response;
			}
			CetateBill(ctx, request);
			response.Code = ResponseCode.Success;
			return response;
		}

		public ResponseMessage<dynamic> Execute(Context ctx, string message)
		{
			var result = this.SyncSalesOrderMaterial(ctx, message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (result.Code == ResponseCode.Success)
			{
				CetateBill(ctx, result.Data);
				//非标订单同步分配其他组织
				MaterialServiceHelper.MaterialAllocateToAll(ctx, result.Data.SalesOrderDetailList.Select(x => x.MaterialMasterId).ToList());
				response.Code = ResponseCode.Success;
			}
			else
			{
				response.Code = result.Code;
				response.Message = result.Message;
			}
			return response;
		}

		private void CetateBill(Context ctx, SalesOrderBillRequest request)
		{
			var billView = FormMetadataUtils.CreateBillView(ctx, "SAL_SaleOrder");
			billView.Model.SetValue("FSaleOrgId", request.OrgId);
			if (request.SalesOrderType == 1)
			{
				billView.Model.SetValue("FBillTypeID", "63719cf361775e");
			}
			//billView.Model.SetValue("FDATE", DateTime.Now);
			billView.Model.SetValue("FSalesOrderDate", request.SalesOrderDate);
			if (request.AuditTime == null)
			{
				billView.Model.SetValue("FAuditTime", request.SalesOrderDate);
			}
			else
			{
				billView.Model.SetValue("FAuditTime", request.AuditTime);
			}
			billView.Model.SetValue("FISINIT", request.IsInit);
			billView.Model.SetValue("FSalerId", request.SalesInfo.Id);
			billView.InvokeFieldUpdateService("FSalerId", 0);

			//改为直接赋值，Set过程会校验可销权限
			billView.Model.DataObject["CustId_Id"] = request.CustomerInfo.Id;
			//billView.Model.SetItemValueByID("FCustId", request.CustomerInfo.Id, 0);
			//billView.Model.SetValue("FCustId", request.CustomerInfo.Id);

			billView.Model.SetValue("FReceiveId", request.CustomerInfo.Id);
			billView.Model.SetValue("FSettleId", request.CustomerInfo.Id);
			billView.Model.SetValue("FChargeId", request.CustomerInfo.Id);
			billView.Model.SetValue("FBillNo", request.SalesOrderNo);
			billView.Model.SetValue("FCustPurchaseNo", request.CustPurchaseNo);

			//平台结算方式对应金蝶付款条件
			billView.Model.SetValue("FRecConditionId", request.PayTermInfo.Id);
			billView.InvokeFieldUpdateService("FRecConditionId", 0);
			//平台支付类型对应金蝶的结算方式
			billView.Model.SetItemValueByNumber("FSettleModeId", request.PayType, 0);
			//制单人
			billView.Model.SetValue("FPlatCreatorId", request.FPlatCreatorId);
			billView.Model.SetValue("FPlatCreatorType", request.FPlatCreatorType);
			billView.Model.SetValue("FPlatCreatorWXCode", request.FPlatCreatorWXCode);
			//发票信息
			if (request.InvoiceInfo != null)
			{
				billView.Model.SetValue("FInvoiceType", request.InvoiceInfo.InvoiceType);
				billView.Model.SetValue("FInvoiceTitle", request.InvoiceInfo.InvoiceTitle);
				billView.Model.SetValue("FInvoiceTel", request.InvoiceInfo.InvoiceTel);
				billView.Model.SetValue("FInvoiceAddress", request.InvoiceInfo.InvoiceAddress);
				billView.Model.SetValue("FTaxCode", request.InvoiceInfo.TaxCode);
				billView.Model.SetValue("FBankName", request.InvoiceInfo.BankName);
				billView.Model.SetValue("FBankAccount", request.InvoiceInfo.BankAccount);
				billView.Model.SetValue("FInvoiceConsigneeName", request.InvoiceInfo.InvoiceConsigneeName);
				billView.Model.SetValue("FInvoiceConsigneePhone", request.InvoiceInfo.InvoiceConsigneePhone);
				billView.Model.SetValue("FInvoiceConsigneeAddress", request.InvoiceInfo.InvoiceConsigneeAddress1);
			}

			if (request.DeliveryInfo != null)
			{
				billView.Model.SetValue("FReceiveAddress", request.DeliveryInfo.DeliveryAddress1);
				billView.Model.SetValue("FLinkMan", request.DeliveryInfo.ConsigneeName);
				billView.Model.SetValue("FLinkPhone", request.DeliveryInfo.ConsigneePhone);
			}
			List<SmallClassModelEntity> smallClassModel = new List<SmallClassModelEntity>();
			int row = 0;
			foreach (var detail in request.SalesOrderDetailList)
			{
				if (row > 0)
				{
					billView.Model.CreateNewEntryRow("FSaleOrderEntry");
				}
				//billView.Model.SetValue("FMapId", detail.MaterialMapId, row);
				//billView.InvokeFieldUpdateService("FMapId", row);
				billView.Model.SetValue("FMaterialId", detail.MaterialId, row);
				billView.InvokeFieldUpdateService("FMaterialId", row);
				billView.Model.SetValue("FQty", detail.Qty, row);
				billView.InvokeFieldUpdateService("FQty", row);
				//计算含税单价
				//样品
				if (request.SalesOrderType == 3)
				{
					billView.Model.SetValue("FIsFree", true, row);
				}
				detail.Price = Math.Round(detail.TaxSubTotal / detail.Qty, 6);
				billView.Model.SetValue("FTaxPrice", detail.Price, row);
				billView.Model.SetValue("FEntryTaxRate", 13, row);
				billView.InvokeFieldUpdateService("FTaxPrice", row);
				billView.Model.SetValue("FProjectNo", detail.ProjectNo, row);
				billView.Model.SetValue("FCustOrderDetailId", detail.CustOrderDetailId, row);

				billView.Model.SetValue("FSupplierProductCode", detail.SupplierProductCode, row);
				billView.Model.SetValue("FOrderDetailId", detail.FbdDetId, row);

				billView.Model.SetValue("FSupplierId", GetSupplierId(ctx, 1, detail.SupplierCode), row);
				//billView.Model.SetItemValueByNumber("FSupplierId", detail.SupplierCode, row);
				billView.Model.SetValue("FSupplierUnitPrice", detail.SupplierUnitPrice, row);
				if (!string.IsNullOrWhiteSpace(detail.SupplierCode) && detail.SupplierUnitPrice > 0)
				{
					billView.Model.SetValue("FCostPriceUpdateDate", detail.CostPriceUpdateDate, row);
					billView.Model.SetValue("FCostPriceUpdateUser", detail.ProductEngineerName, row);
					billView.Model.SetValue("FSupplierUnitPriceSource", detail.SupplierUnitPriceSource, row);
				}
				billView.Model.SetValue("FEntryNote", detail.Remark, row);
				billView.Model.SetValue("FDeliveryDate", detail.DeliveryDate < DateTime.Now.Date ? DateTime.Now.Date : detail.DeliveryDate, row);
				billView.Model.SetValue("FStockFeatures", detail.StockFeatures, row);
				billView.Model.SetValue("FLocFactory", detail.LocFactory, row);
				billView.Model.SetValue("FCustMaterialNo", detail.CustMaterialNo, row);
				billView.Model.SetValue("FInsideRemark", detail.InsideRemark, row);
				billView.Model.SetItemValueByNumber("FProductEngineerId", detail.ProductEngineerCode, row);
				billView.Model.SetItemValueByNumber("FProductManagerId", detail.ProductManagerCode, row);
				billView.Model.SetValue("FInquiryOrder", detail.InquiryOrder, row);
				billView.Model.SetValue("FInquiryOrderLineNo", detail.InquiryOrderLineNo, row);
				billView.Model.SetValue("FDrawingRecordId", detail.DrawingRecordId, row);
				//新客户物料编号和名称
				billView.Model.SetValue("FCustItemNo", detail.CustItemNo, row);
				billView.Model.SetValue("FCustItemName", detail.CustItemName, row);
				billView.Model.SetValue("FMachineName", detail.MachineName, row);

				billView.Model.SetValue("FisAplAccept", detail.isAplAccept, row);

				//非标云平台销售订单，同步到深圳蚂蚁ERP，默认库存组织、供货组织为华东五部(HDFI)、第五事业部
				if (request.SalesOrderType == 1)
				{
					if (detail.SupplyOrgId == 0)
					{
						var fbOrgId = GetOrgId(ctx, "HDFI");
						//默认华东五部(HNFI)
						billView.Model.SetValue("FSupplyTargetOrgId", fbOrgId, row);
						//非标默认第五事业部
						billView.Model.SetValue("FBusinessDivisionId", "638fef4619b689", row);
					}
					else
					{
						//供货组织
						billView.Model.SetValue("FSupplyTargetOrgId", detail.SupplyOrgId, row);
						billView.Model.SetValue("FBusinessDivisionId", detail.BusinessDivisionId, row);
					}
					//组装非标原材料信息
					//半成品-W-1-1
					//原材料-W-2-1
					int bomRow = 0;
					int bomCount = 1;
					foreach (var bomItem in detail.FBBomInfo)
					{
						if (bomRow > 0)
						{
							billView.Model.CreateNewEntryRow("FPENYSubFBBomInfoEntity");
						}
						//有工序，需要加一层半成品
						if (detail.IsProcess)
						{
							//半成品
							var semiProducts = detail.ItemNo + "-W-1-" + bomCount;
							billView.Model.SetValue("FParentSpecification", detail.ItemNo, bomRow);
							billView.Model.SetValue("FSpecification", semiProducts, bomRow);
							billView.Model.SetValue("FFBBomName", bomItem.Name, bomRow);
							billView.Model.SetValue("FWeight", 1, bomRow);
							billView.Model.SetValue("FLength", bomItem.Length, bomRow);
							billView.Model.SetValue("FWidth", bomItem.Width, bomRow);
							billView.Model.SetValue("FHeight", bomItem.Height, bomRow);
							billView.Model.SetValue("FVolume", bomItem.Length * bomItem.Width * bomItem.Height, bomRow);//长*宽*高
							billView.Model.SetValue("FTextures", detail.Textures, bomRow);
							billView.Model.SetValue("FWeightUnitid", bomItem.WeightUnitid, bomRow);
							billView.Model.SetValue("FVolumeUnitid", bomItem.VolumeUnitid, bomRow);
							//原材料
							bomRow += 1;
							billView.Model.CreateNewEntryRow("FPENYSubFBBomInfoEntity");
							billView.Model.SetValue("FParentSpecification", semiProducts, bomRow);
							billView.Model.SetValue("FSpecification", detail.ItemNo + "-W-2-" + bomCount, bomRow);
							billView.Model.SetValue("FFBBomName", bomItem.Name, bomRow);
							billView.Model.SetValue("FWeight", bomItem.Weight, bomRow);
							billView.Model.SetValue("FLength", bomItem.Length, bomRow);
							billView.Model.SetValue("FWidth", bomItem.Width, bomRow);
							billView.Model.SetValue("FHeight", bomItem.Height, bomRow);
							billView.Model.SetValue("FVolume", bomItem.Length * bomItem.Width * bomItem.Height, bomRow);//长*宽*高
							billView.Model.SetValue("FTextures", detail.Textures, bomRow);
							billView.Model.SetValue("FWeightUnitid", bomItem.WeightUnitid, bomRow);
							billView.Model.SetValue("FVolumeUnitid", bomItem.VolumeUnitid, bomRow);

						}
						else
						{
							//无工序，直接是原材料
							billView.Model.SetValue("FParentSpecification", detail.ItemNo, bomRow);
							billView.Model.SetValue("FSpecification", detail.ItemNo + "-W-2-" + bomCount, bomRow);
							billView.Model.SetValue("FFBBomName", bomItem.Name, bomRow);
							billView.Model.SetValue("FWeight", bomItem.Weight, bomRow);
							billView.Model.SetValue("FLength", bomItem.Length, bomRow);
							billView.Model.SetValue("FWidth", bomItem.Width, bomRow);
							billView.Model.SetValue("FHeight", bomItem.Height, bomRow);
							billView.Model.SetValue("FVolume", bomItem.Length * bomItem.Width * bomItem.Height, bomRow);//长*宽*高
							billView.Model.SetValue("FTextures", detail.Textures, bomRow);//取原材料
							billView.Model.SetValue("FWeightUnitid", bomItem.WeightUnitid, bomRow);
							billView.Model.SetValue("FVolumeUnitid", bomItem.VolumeUnitid, bomRow);
						}
						bomRow += 1;
						bomCount += 1;
					}

				}
				else
				{
					//供货组织
					billView.Model.SetValue("FSupplyTargetOrgId", detail.SupplyOrgId, row);
					billView.Model.SetValue("FBusinessDivisionId", detail.BusinessDivisionId, row);
					//billView.Model.SetValue("FOUTSOURCESTOCKLOC", GetOutSourceStockLoc(ctx, detail.SupplyOrgId), row);

					//获取供应商成本价

					if (string.IsNullOrEmpty(detail.SupplierCode) || detail.SupplierUnitPrice == 0)
					{
						// 没有手动填写成本价并且价目表没有匹配到价格
						smallClassModel.Add(new SmallClassModelEntity
						{
							SmallClassId = detail.ProductSmallClass.Id.ToString(),
							SmallClassName = detail.ProductSmallClass.Name,
							ItemNo = detail.ItemNo,
							ItemName = detail.ItemName
						});
					}
				}
				row++;
			}

			if (smallClassModel.Count > 0)
			{
				try
				{
					// 提示产品经理去填写成本价
					var smallClass = smallClassModel.Select(c => c.SmallClassId).Distinct().ToList();
					var requestData = JsonConvertUtils.SerializeObject<object>(smallClass);
					string scmSpUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetSmallClassRobot";
					var response = ApigatewayUtils.InvokePostWebService(scmSpUrl, requestData);

					var result = JsonConvertUtils.DeserializeObject<ResponseMessage<List<SmallClassRobotMapping>>>(response);

					if (result != null && result.Code == "success")
					{
						var resultGroup = result.Data.GroupBy(c => c.RobotUrl);
						foreach (IGrouping<string, SmallClassRobotMapping> g in resultGroup)
						{
							var scIdArr = g.Select(c => c.ProductSmallClassId);
							var filterItem = smallClassModel.Where(c => scIdArr.Contains(c.SmallClassId)).Take(10);
							var requests = new
							{
								msgtype = "markdown",
								markdown = new
								{
									content = $"> 组织:<font color=\"comment\">{request.OrgName}</font>\n" +
								  $"订单号:<font color=\"warning\">{request.SalesOrderNo}</font>\t" +
								  $"订单日期:<font color=\"warning\">{request.SalesOrderDate.ToString("yyyy-MM-dd")}</font>，" +
								  $"询价人:<font color=\"warning\">{request.SalesInfo.Name}</font>\t" +
								  $"一共有<font color=\"warning\">{filterItem.Count()}行</font>没有成本价。消息最多展示前10条 \n> " +
								  string.Join("", filterItem.Select(c => new { Str = $">产品型号:<font color=\"comment\">{c.ItemNo}</font>\t产品名称:<font color=\"comment\">{c.ItemName}</font>\n" }).Select(c => c.Str).ToList())
								}
							};
							string inputData = JsonConvertUtils.SerializeObject(requests);
							var res = WebApiServiceUtils.SendRobot(g.Key, inputData);
						}
					}
				}
				catch
				{

				}
			}

			billView.Model.SetValue("FAllDisCount", request.KnockOff);
			billView.InvokeFieldUpdateService("FAllDisCount", 0);

			//if (request.PayTermInfo.Code.EqualsIgnoreCase("YFDJ") || request.PayTermInfo.Code.EqualsIgnoreCase("501"))
			var planEntrys = billView.Model.DataObject["SaleOrderPlan"] as DynamicObjectCollection;
			if (request.PlanentryList != null && request.PlanentryList.Length > 0)
			{
				row = 0;
				var relbillno = string.Join(",", request.PlanentryList.Select(x => x.AdvanceNo));
				var advanceRecAmount = request.PlanentryList.Sum(x => x.ActRecAmount);
				foreach (var item in request.PlanentryList)
				{
					billView.Model.CreateNewEntryRow("FSaleOrderPlanEntry");
					billView.Model.SetValue("FSEQ", item.Seq, row);
					billView.Model.SetValue("FADVANCENO", item.AdvanceNo, row);
					billView.Model.SetValue("FADVANCEID", item.ADVANCEID, row);
					billView.Model.SetValue("FADVANCESEQ", item.AdvanceSeq, row);
					billView.Model.SetValue("FADVANCEENTRYID", item.ADVANCEENTRYID, row);
					billView.Model.SetValue("FActRecAmount", item.ActRecAmount, row);
					billView.Model.SetValue("FRemainAmount", item.RemainAmount - item.ActRecAmount, row);
					//billView.Model.SetValue("FPreMatchAmountFor", item.ActRecAmount, row);
					billView.Model.SetItemValueByID("FPESettleOrgId", item.SettleOrgId_Id, row);
					billView.Model.SetValue("FISPAYONLINE", item.FIsPayOnline, row);
					billView.Model.SetItemValueByNumber("FSettleMode_PENY", item.FSettleMode_PENY, row);
					row++;
				}
				var soplan = planEntrys.FirstOrDefault();
				soplan["RelBillNo"] = relbillno;
				soplan["RecAmount"] = advanceRecAmount;
				//soplan["RecAdvanceAmount"] = advanceRecAmount;
				//soplan["RecAdvanceRate"] = Math.Round(advanceRecAmount / request.SalesOrderVatNet * 100, 2);

				if (request.PayTermInfo.Code.EqualsIgnoreCase("YFDJ"))
				{
					soplan = planEntrys.LastOrDefault();
					soplan["RecAdvanceAmount"] = request.SalesOrderVatNet - advanceRecAmount;
					soplan["RecAdvanceRate"] = Math.Round((request.SalesOrderVatNet - advanceRecAmount) / request.SalesOrderVatNet * 100, 2);
				}
			}
			//收款计划自动算出不需要再赋值
			//else
			//{
			//    var soplan = planEntrys.FirstOrDefault();
			//    soplan["RecAdvanceAmount"] = request.SalesOrderVatTotal;
			//    soplan["RecAdvanceRate"] = 100;

			//    for (int rowIndex = billView.Model.GetEntryRowCount("FSaleOrderPlan") - 1; rowIndex > 0; rowIndex--)
			//    {
			//        billView.Model.DeleteEntryRow("FSaleOrderPlan", rowIndex);
			//    }
			//}

			var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
			if (!oper.IsSuccess)
			{
				if (oper.ValidationErrors.Count > 0)
				{
					if (request.SalesOrderType == 1)
					{
						//更新非标的订单状态
						UpFbOrderStatus(false, request.SalesOrderNo, string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
					}
					throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
				}
				else
				{
					if (request.SalesOrderType == 1)
					{
						//更新非标的订单状态
						UpFbOrderStatus(false, request.SalesOrderNo, string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					}
					throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
				}
			}
			else
			{
				//List<long> materials = new List<long>();
				//foreach (var item in request.SalesOrderDetailList)
				//{
				//    materials.Add(item.MaterialMasterId);
				//}
				//KafkaProducerService kafkaProducer = new KafkaProducerService();
				//List<RabbitMQMessage> messages = new List<RabbitMQMessage>
				//{
				//    new RabbitMQMessage()
				//    {
				//        Exchange = "materialManagement",
				//        Routingkey = "MaterialAllocate",
				//        Keyword = "",
				//        Message = JsonConvertUtils.SerializeObject(materials)
				//    }
				//};
				//kafkaProducer.AddMessage(ctx, messages.ToArray());

				request.SaleBillId = oper.OperateResult.Select(x => Convert.ToInt64(x.PKValue)).FirstOrDefault();
				//更新核销记录
				var sSql = $"UPDATE PENY_t_MatchMoneyEntry SET FISCHECK=1 where FSALBILLNO='{billView.Model.DataObject["BillNo"]}'";
				DBServiceHelper.Execute(ctx, sSql);
				if (request.SalesOrderType == 1)
				{
					//更新非标的订单状态
					UpFbOrderStatus(true, request.SalesOrderNo, "创建成功");
				}
			}
		}
		#region 获取供应商成本价

		//根据组织ID获取组织名称
		private string GetOrgName(Context ctx, long orgid)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@Orgid", KDDbType.Int64, orgid) };
			var sql = $@"/*dialect*/select top 1 FNAME from  t_org_organizations org 
             inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=2052
			 where org.FORGID=@Orgid";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}
		//根据单位获取id
		private long GetUnitId(Context ctx, string number)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UnitId", KDDbType.String, number) };
			var sql = $@"/*dialect*/SELECT FUNITID FROM dbo.T_BD_UNIT WHERE FNUMBER=@UnitId";
			return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
		}

		//根据组织ID获取组织编码
		private string GetOrgCode(Context ctx, long orgid)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@Orgid", KDDbType.Int64, orgid) };
			var sql = $@"select top 1 FNUMBER from  t_org_organizations org where org.FORGID=@Orgid";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}

		/// <summary>
		/// 根据供应商编号获取供应商ID
		/// </summary>
		/// <param name="SupplierCode"></param>
		/// <returns></returns>
		private long GetSupplierId(Context ctx, long useOrgId, string supplierCode)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UseOrgId", KDDbType.Int64, useOrgId) ,
				new SqlParam("@SupplierCode", KDDbType.String, supplierCode) };
			var sql = $@"select top 1 FSUPPLIERID from t_BD_Supplier where FUSEORGID=@UseOrgId and  FNUMBER=@SupplierCode  and FDOCUMENTSTATUS='C' and FFORBIDSTATUS='A'  ";
			return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
		}

		/// <summary>
		/// 根据供货组织获取老ERP对应的账套
		/// </summary>
		/// <param name="supplyOrgId"></param>
		/// <returns></returns>
		private string GetOldCompanyCode(Context ctx, long supplyOrgId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SupplyOrgId", KDDbType.Int64, supplyOrgId) };
			var sql = $@"/*dialect*/select top 1 t1.ERPCODE from T_BD_JDCODECONVERTERPCODE t1
                         inner join t_org_organizations t2 on t1.JDCODE=t2.FNUMBER
                         where t2.FORGID=@SupplyOrgId ";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, "", paramList: pars.ToArray());
		}

		//根据供货组织获取发货地
		private string GetOutSourceStockLoc(Context ctx, long orgid)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@Orgid", KDDbType.Int64, orgid) };
			var sql = $@"select top 1 FOUTSOURCESTOCKLOC from  t_org_organizations org where org.FORGID=@Orgid";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}
		/// <summary>
		/// 根据组织编号获取组织ID
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="orgCode"></param>
		/// <returns></returns>
		private long GetOrgId(Context ctx, string orgCode)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@OrgCode", KDDbType.String, orgCode) };
			var sql = $@"select top 1 FORGID from  t_org_organizations  where FNUMBER=@OrgCode";
			return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
		}

		#endregion

		/// <summary>
		/// 更新非标的订单状态
		/// </summary>
		private void UpFbOrderStatus(bool isSuccess, string orderNumber, string errorMessage)
		{
			try
			{
				//string disurl = WebApiServiceUtils.DispatchToCloudUrl;
				//string url = disurl + "api/cnc/orders/orderCreatedNotify";
				//var pairs = new
				//{
				//	Code = "",
				//	Message = "操作成功",
				//	IsSuccess = isSuccess,
				//	ErrorMessage = errorMessage,
				//	Data = new { OrderNumber = orderNumber },
				//};
				//WebApiServiceUtils.HttpPost(url, pairs);
			}
			catch (Exception)
			{

			}
		}

		/// <summary>
		/// 整单销售订单作废(非标使用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> CloseSalesOrder(Context ctx, string salesOrderNo)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrEmpty(salesOrderNo))
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "销售订单号不能为空"; ;
				return response;
			}

			//是否存在
			var salesOrderId = 0;
			//作废(A:未作废，B:已作废)
			var fcancelStatus = "";
			//单据状态(Z:暂存，A创建，B:审核中，C:已审核，D:重新审核)
			var fdocumentStatus = "";
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FBILLNO", KDDbType.String, salesOrderNo) };
			var sql = $@"select top 1 FID,FDOCUMENTSTATUS,FCANCELSTATUS from T_SAL_ORDER  where FBILLNO=@FBILLNO ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				salesOrderId = Convert.ToInt32(data["FID"].ToString());
				fcancelStatus = data["FCANCELSTATUS"].ToString();
				fdocumentStatus = data["FDOCUMENTSTATUS"].ToString();
			}
			if (salesOrderId == 0)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = $"该订单[{salesOrderNo}]不存在";
				response.Data = "ERROR";
				return response;
			}
			//如果已经作废，返回正确
			if (fcancelStatus.Equals("B"))
			{
				response.Code = ResponseCode.Success;
				response.Message = $"该订单[{salesOrderNo}]金蝶已作废";
				return response;
			}
			var view = FormMetadataUtils.CreateBillView(ctx, "SAL_SaleOrder", salesOrderId);
			var entryObjs = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FSaleOrderEntry"));
			foreach (DynamicObject entry in entryObjs)
			{
				if (BusinessFlowDataServiceHelper.IsPush(ctx,
					new IsPushArgs(view.BillBusinessInfo, "FSaleOrderEntry", entry)))
				{
					response.Code = ResponseCode.ModelError;
					response.Message = $"销售订单[{salesOrderNo}]存在已下推的分录,不能作废"; ;
					return response;
				}
			}
			return SalesOrderServiceHelper.CloseSalesOrderAction(ctx, salesOrderId, fdocumentStatus);
		}

		/// <summary>
		/// 销售订单明细项作废(非标使用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> CloseSalesOrderDetail(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			CloseSalesOrderRequest request = JsonConvertUtils.DeserializeObject<CloseSalesOrderRequest>(message);
			if (string.IsNullOrEmpty(request.SalesOrderNo))
			{
				response.Code = ResponseCode.Exception;
				response.Message = "销售订单号不能为空"; ;
				return response;
			}
			if (request.Det.Count == 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "销售订单明细不能为空"; ;
				return response;
			}
			//是否存在
			var salesOrderId = 0;
			//作废(A:未作废，B:已作废)
			var fcancelStatus = "";
			//单据状态(Z:暂存，A创建，B:审核中，C:已审核，D:重新审核)
			var fdocumentStatus = "";
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo) };
			var sql = $@"select top 1 FID,FDOCUMENTSTATUS,FCANCELSTATUS from T_SAL_ORDER  where FBILLNO=@FBILLNO ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				salesOrderId = Convert.ToInt32(data["FID"].ToString());
				fcancelStatus = data["FCANCELSTATUS"].ToString();
				fdocumentStatus = data["FDOCUMENTSTATUS"].ToString();
			}
			if (salesOrderId == 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = $"该订单[{request.SalesOrderNo}]不存在";
				response.Data = "ERROR";
				return response;
			}
			//如果已经作废，返回正确
			if (fcancelStatus.Equals("B"))
			{
				response.Code = ResponseCode.Success;
				response.Message = $"该订单[{request.SalesOrderNo}]金蝶已作废";
				return response;
			}
			//获取销售订单明细的FENTRYID
			ChangeSalesOrderRequest.Salesorderdetaillist[] SalesOrderDetailList;
			string param = string.Empty;
			if (request.Det.Count > 0)
			{
				int i = 1;
				foreach (var item in request.Det)
				{
					if (i == 1)
						param = "@FbdDetId" + i;
					else
						param += ",@FbdDetId" + i;
					pars.Add(new SqlParam("@FbdDetId" + i++, KDDbType.Int64, item.Id));
				}
			}
			sql = $@"/*dialect*/select t2.FENTRYID,t2.FOrderDetailId,t2.FSEQ,t2.FQTY,t3.FTaxPrice from T_SAL_ORDER t1
            inner join T_SAL_ORDERENTRY t2 on t1.FID=t2.FID
			inner join T_SAL_ORDERENTRY_F  t3 on t2.FENTRYID=t3.FENTRYID
            where t1.FBILLNO=@FBILLNO and t2.FOrderDetailId in({param}) ";
			datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			SalesOrderDetailList = new ChangeSalesOrderRequest.Salesorderdetaillist[datas.Count];
			int k = 0;
			foreach (var data in datas)
			{
				SalesOrderDetailList[k] = new ChangeSalesOrderRequest.Salesorderdetaillist();
				SalesOrderDetailList[k].Id = Convert.ToInt64(data["FOrderDetailId"].ToString());
				SalesOrderDetailList[k].OrderEntryId = Convert.ToInt64(data["FENTRYID"].ToString());
				SalesOrderDetailList[k].Qty = Convert.ToDecimal(data["FQTY"].ToString());
				SalesOrderDetailList[k].VatPrice = Convert.ToDecimal(data["FTaxPrice"].ToString());
				SalesOrderDetailList[k].IsDelete = true;
				k++;
			}
			var view = FormMetadataUtils.CreateBillView(ctx, "SAL_SaleOrder", salesOrderId);
			var entryObjs = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FSaleOrderEntry"));
			foreach (var item in entryObjs)
			{
				var entry = SalesOrderDetailList.FirstOrDefault(x => x.OrderEntryId == Convert.ToInt64(item["Id"]));
				if (entry != null)
				{
					if (BusinessFlowDataServiceHelper.IsPush(ctx, new IsPushArgs(view.BillBusinessInfo, "FSaleOrderEntry", item)))
					{
						response.Code = ResponseCode.Exception;
						response.Message = $"销售订单明细[{item["FOrderDetailId"]}]存在已下推的分录,不能删除"; ;
						return response;
					}
				}
			}
			//如果全部都作废，要调用整单作废
			if (request.Det.Count == entryObjs.Count)
			{
				return SalesOrderServiceHelper.CloseSalesOrderAction(ctx, salesOrderId, fdocumentStatus);
			}

			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, "SAL_SaleOrder");
			FormMetadata xformMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, "SAL_XORDER");
			List<long> notExists = new List<long>();
			foreach (var entry in request.Det)
			{
				if (!SalesOrderDetailList.Any(n => Convert.ToInt64(n.Id) == entry.Id))
				{
					notExists.Add(entry.Id);
				}
			}
			if (notExists.Count > 0)
			{
				throw new Exception($"分录内码：{string.Join(",", notExists)},不在销售订单【{request.SalesOrderNo}】内，请检查参数！");
			}
			DynamicObjectType dynamicObjectType = xformMetadata.BusinessInfo.GetDynamicObjectType();
			DynamicObject xSalesData = new DynamicObject(dynamicObjectType);
			XSaleOrderCommon xSaleOrderCommon = new XSaleOrderCommon(ctx);
			ChangeSalesOrderBusiness changeSalesOrderBusiness = new ChangeSalesOrderBusiness();
			xSaleOrderCommon.lstNotFields = changeSalesOrderBusiness.GetTakeEffectNotWriteBackFields(xformMetadata);
			DynamicObject[] array = BusinessDataServiceHelper.Load(ctx, new object[1] { salesOrderId }, formMetadata.BusinessInfo.GetDynamicObjectType());
			DynamicObject oldSalesData = ((array != null && array.Count() > 0) ? array[0] : null);
			string validateMsg = string.Empty;
			bool para_IsUseTaxCombination = SystemParameterServiceHelper.IsUseTaxCombination(ctx);
			xSaleOrderCommon.baseOrderData = oldSalesData;
			xSaleOrderCommon.para_IsUseTaxCombination = para_IsUseTaxCombination;
			xSaleOrderCommon.orderBusinessInfo = formMetadata.BusinessInfo;
			xSaleOrderCommon.SetCommonVarValue();
			xSaleOrderCommon.SetNewDataFromOriginalData(xSalesData, xSaleOrderCommon.baseOrderData, isWriteBackSO: false, xSaleOrderCommon.lstNotFields);
			changeSalesOrderBusiness.SetBillTypeValue(xformMetadata.BusinessInfo, formMetadata.BusinessInfo, oldSalesData, xSalesData);
			xSalesData = changeSalesOrderBusiness.FilterChangeDataByEntryIds(xSalesData, SalesOrderDetailList);

			if (xSalesData != null && xSalesData.DynamicObjectType.Properties.Contains("SaleOrderFinance"))
			{
				DynamicObjectCollection salesFinances = xSalesData["SaleOrderFinance"] as DynamicObjectCollection;
				salesFinances[0]["PARTOFENTRYIDS"] = string.Join(",", SalesOrderDetailList.Select(n => Convert.ToString(n.OrderEntryId)).Take(1).ToArray());
			}

			if (!changeSalesOrderBusiness.CheckDataCanChange(ctx, oldSalesData, SalesOrderDetailList.Select(n => n.OrderEntryId).ToList(), ref validateMsg))
			{
				throw new Exception(validateMsg);
			}
			xSalesData["FFORMID"] = xformMetadata.BusinessInfo.GetForm().Id;
			DynamicObjectCollection salesEntrys = xSalesData["SaleOrderEntry"] as DynamicObjectCollection;
			salesEntrys.Sort((DynamicObject n) => Convert.ToInt32(n["Seq"]));
			for (int i = 0; i < salesEntrys.Count; i++)
			{
				salesEntrys[i]["Seq"] = i + 1;
				salesEntrys[i]["ChangeType"] = "D";
			}
			xSalesData["ChangeReason"] = ResManager.LoadKDString("WebAPI变更", "00444533030032263", SubSystemType.SCM);

			var billView = FormMetadataUtils.CreateBillView(ctx, "SAL_XORDER");
			billView.Model.DataObject = xSalesData;
			//获取财务金额
			decimal allAmount = Convert.ToDecimal(billView.Model.GetValue("FBillAllAmount", 0));
			decimal taxAmount = Convert.ToDecimal(billView.Model.GetValue("FBillTaxAmount", 0));
			decimal amount = Convert.ToDecimal(billView.Model.GetValue("FBillAmount", 0));
			decimal allAmount_LC = Convert.ToDecimal(billView.Model.GetValue("FBillAllAmount_LC", 0));
			decimal taxAmount_LC = Convert.ToDecimal(billView.Model.GetValue("FBillTaxAmount_LC", 0));
			decimal amount_LC = Convert.ToDecimal(billView.Model.GetValue("FBillAmount_LC", 0));
			foreach (var detail in SalesOrderDetailList)
			{
				int row;
				var entry = salesEntrys.FirstOrDefault(p => Convert.ToInt64(p["FOrderDetailId"]) == detail.Id);
				row = salesEntrys.IndexOf(entry);
				if (detail.IsDelete)
				{
					billView.Model.SetValue("FChangeType", "D", row);
					billView.InvokeFieldUpdateService("FChangeType", row);

					allAmount = allAmount - Convert.ToDecimal(billView.Model.GetValue("FAllAmount", row));
					taxAmount = taxAmount - Convert.ToDecimal(billView.Model.GetValue("FTaxAmount", row));
					amount = amount - Convert.ToDecimal(billView.Model.GetValue("FAmount", row));
					allAmount_LC = allAmount_LC - Convert.ToDecimal(billView.Model.GetValue("FAllAmount_LC", row));
					taxAmount_LC = taxAmount_LC - Convert.ToDecimal(billView.Model.GetValue("FTaxAmount_LC", row));
					amount_LC = amount_LC - Convert.ToDecimal(billView.Model.GetValue("FAmount_LC", row));
					continue;
				}
				billView.Model.SetValue("FQty", detail.Qty, row);
				billView.InvokeFieldUpdateService("FQty", row);
				billView.Model.SetValue("FTaxPrice", detail.VatPrice, row);
				billView.Model.SetValue("FEntryTaxRate", 13, row);
				billView.InvokeFieldUpdateService("FTaxPrice", row);

			}

			billView.Model.SetValue("FBillAllAmount_LC", allAmount_LC);
			billView.Model.SetValue("FBillAllAmount", allAmount);
			billView.Model.SetValue("FBillTaxAmount_LC", taxAmount_LC);
			billView.Model.SetValue("FBillTaxAmount", taxAmount);
			billView.Model.SetValue("FBillAmount_LC", amount_LC);
			billView.Model.SetValue("FBillAmount", amount);
			billView.InvokeFieldUpdateService("FBillAllAmount", 0);
			var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
			if (!oper.IsSuccess)
			{
				if (oper.ValidationErrors.Count > 0)
				{
					throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
				}
				else
				{
					throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
				}
			}
			response.Code = ResponseCode.Success;
			response.ErrorMessage = "操作成功";
			return response;

		}

		/// <summary>
		/// 取消销售订单(调用作废)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> CancelSalesOrder(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			CancelSalesOrderRequest request = JsonConvert.DeserializeObject<CancelSalesOrderRequest>(message);
			if (string.IsNullOrWhiteSpace(request.SalesOrderNo))
			{
				response.Code = ResponseCode.Exception;
				response.Message = "销售订单号不能为空";
				response.Data = "ERROR";
				return response;
			}
			//是否存在
			var salesOrderId = 0;
			//作废(A:未作废，B:已作废)
			var fcancelStatus = "";
			//单据状态(Z:暂存，A创建，B:审核中，C:已审核，D:重新审核)
			var fdocumentStatus = "";
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo) };
			var sql = $@"select top 1 FID,FDOCUMENTSTATUS,FCANCELSTATUS from T_SAL_ORDER  where FBILLNO=@FBILLNO ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				salesOrderId = Convert.ToInt32(data["FID"].ToString());
				fcancelStatus = data["FCANCELSTATUS"].ToString();
				fdocumentStatus = data["FDOCUMENTSTATUS"].ToString();
			}
			if (salesOrderId == 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = $"该订单[{request.SalesOrderNo}]不存在";
				response.Data = "ERROR";
				return response;
			}
			//如果已经作废，返回正确
			if (fcancelStatus.Equals("B"))
			{
				response.Code = ResponseCode.Success;
				response.Message = $"该订单[{request.SalesOrderNo}]金蝶已作废";
				return response;
			}

			//是否取消(false：只是校验，true：取消订单)
			if (request.IsCance)
			{
				//调用取消销售订单的事务
				return SalesOrderServiceHelper.CloseSalesOrderAction(ctx, salesOrderId, fdocumentStatus);
			}
			else
			{
				//是否送货
				sql = $@"SELECT COUNT(1) FROM T_SAL_OUTSTOCK  SOUT
                 INNER JOIN T_SAL_OUTSTOCKENTRY_R SOUTR ON SOUT.FID=SOUTR.FID
                 WHERE SOUT.FCANCELSTATUS='A' AND FSOORDERNO=@FBILLNO ";
				var retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
				if (retCount > 0)
				{
					response.Code = ResponseCode.Exception;
					response.Message = $"该订单[{request.SalesOrderNo}]已存在销售出库单";
					response.Data = "DN";
					return response;
				}

				//是否存在采购单
				sql = @"select SUM(num) from (	 
						select count(1) num
                        from T_PUR_POORDERENTRY t1
                        left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
						inner join t_PUR_POOrder t3 on  t1.FID=t3.FID
                        where isnull(t2.FDEMANDBILLNO,'')<>'' and t3.FCANCELSTATUS='A' and t2.FDEMANDBILLNO=@FBILLNO
                        union
                        select count(1)num
                         from T_PUR_POORDERENTRY t1
                        left join T_PUR_POORDERENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                        left join T_PUR_REQENTRY_LK t3 on t3.FENTRYID=t2.FSID
                        left join T_PLN_PLANORDER_LK t4 on t3.FSID=t4.FID
                        left JOIN T_PLN_PLANORDER_B t5 ON t4.FSBILLID=t5.FID
						inner join t_PUR_POOrder t6 on  t1.FID=t6.FID
                        where isnull(t5.FSALEORDERNO,'')<>'' and t6.FCANCELSTATUS='A' and t5.FSALEORDERNO=@FBILLNO
						) datas";
				retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
				if (retCount > 0)
				{
					response.Code = ResponseCode.Exception;
					response.Message = $"该订单[{request.SalesOrderNo}]已存在采购单";
					response.Data = "DN";
					return response;
				}

				//是否存在组织间需求单
				sql = @"SELECT COUNT(1) FROM T_PLN_REQUIREMENTORDER where FSALEORDERNO=@FBILLNO and FISCLOSED='A' ";
				retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
				if (retCount > 0)
				{
					response.Code = ResponseCode.Exception;
					response.Message = $"该订单[{request.SalesOrderNo}]已存在组织间需求单";
					response.Data = "ERROR";
					return response;
				}

				response.Code = ResponseCode.Success;
				response.Message = "可取消";
				return response;
			}
		}

		/// <summary>
		/// 查询销售单是否可以变更接口
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> IsCanUpdateSalesOrder(Context ctx, string message)
		{
			IsCanUpdateSalesOrderRequest request = JsonConvertUtils.DeserializeObject<IsCanUpdateSalesOrderRequest>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrWhiteSpace(request.SalesOrderNo))
			{
				response.Code = ResponseCode.Exception;
				response.Message = "订单号不能为空";
				return response;
			}
			if (request.SalesOrderDetailList == null || request.SalesOrderDetailList.Count <= 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "无变更项";
				return response;
			}

			//把明细两层BOM拆成一层（不是拆成一层，而 是只留下第一层，ERP不支持BOM）
			var detailList = request.SalesOrderDetailList.Where(t => t.Children == null || t.Children.Count == 0).ToList();
			//获取销售单明细
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo) };
			var sql = $@"SELECT SALDES.FSEQ,MATE.FNUMBER,SALDES.FQTY,SALPLAN.FDELIVERYDATE,SALDES.FOrderDetailId,SALDESF.FTAXPRICE,SALDESF.FALLAMOUNT FROM  T_SAL_ORDER SAL
                        INNER JOIN T_SAL_ORDERENTRY SALDES ON SAL.FID=SALDES.FID
                        INNER JOIN T_BD_MATERIAL MATE ON MATE.FMATERIALID=SALDES.FMATERIALID
                        INNER JOIN T_SAL_ORDERENTRY_F SALDESF ON SALDESF.FENTRYID=SALDES.FENTRYID
                        LEFT JOIN T_SAL_ORDERENTRYDELIPLAN SALPLAN ON SALPLAN.FENTRYID=SALDES.FENTRYID
                        WHERE SAL.FBILLNO=@FBILLNO AND SAL.FCANCELSTATUS='A' ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			List<SalesOrderDetEntity> currentSalesOrderDetList = new List<SalesOrderDetEntity>();
			foreach (var data in datas)
			{

				currentSalesOrderDetList.Add(new SalesOrderDetEntity()
				{
					Id = Convert.ToInt64(data["FOrderDetailId"]),
					Seq = Convert.ToInt32(data["FSEQ"]),
					ItemNo = data["FNUMBER"].ToString(),
					Qty = Convert.ToDecimal(data["FQTY"]),
					UnitPrice = Convert.ToDecimal(data["FTAXPRICE"]),
					VatPrice = Convert.ToDecimal(data["FTAXPRICE"]),
					DeliveryDate = Convert.ToDateTime(data["FDELIVERYDATE"])
				});
			}

			if (currentSalesOrderDetList.Count == 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶没找到该订单";
				return response;
			}

			//变更列表
			var salesOrderChangeDetList = new List<IsCanUpdateSalesOrderRequest.SalesOrderChangeDetEntity>();
			foreach (var salesOrderDetail in detailList)
			{
				var cEntity = currentSalesOrderDetList.FirstOrDefault(t => t.Id == salesOrderDetail.Id);
				//添加到变更列表
				var chageEntity = new IsCanUpdateSalesOrderRequest.SalesOrderChangeDetEntity();
				if (cEntity != null)
				{
					chageEntity.SoSeqNo = cEntity.Seq;
					chageEntity.ItemNo = cEntity.ItemNo?.Trim();
					chageEntity.FrmQty = cEntity.Qty;
					chageEntity.FrmEtd = Convert.ToDateTime(cEntity.DeliveryDate);
					chageEntity.FrmUp = cEntity.UnitPrice;
					chageEntity.FrmUpVat = cEntity.VatPrice;

					chageEntity.AdditionalCost = cEntity.AdditionalCost;
					if (salesOrderDetail.Qty == 0)
					{
						chageEntity.ToItemNo = cEntity.ItemNo?.Trim();
						chageEntity.ToQty = salesOrderDetail.Qty;
						chageEntity.ToEtd = Convert.ToDateTime(cEntity.DeliveryDate);
						chageEntity.ToUp = cEntity.UnitPrice;
						chageEntity.ToUpVat = cEntity.VatPrice;
					}
					else
					{
						chageEntity.ToItemNo = salesOrderDetail.ItemNo?.Trim();
						chageEntity.ToQty = salesOrderDetail.Qty;
						chageEntity.ToEtd = Convert.ToDateTime(salesOrderDetail.DeliveryDate);
						chageEntity.ToUp = salesOrderDetail.UnitPrice;
						chageEntity.ToUpVat = salesOrderDetail.VatPrice;
					}
				}

				if (!chageEntity.ItemNo.EqualsIgnoreCase(chageEntity.ToItemNo)
					|| chageEntity.FrmQty != chageEntity.ToQty
					|| chageEntity.FrmEtd != chageEntity.ToEtd
					|| chageEntity.FrmUpVat != chageEntity.ToUpVat)
				{
					salesOrderChangeDetList.Add(chageEntity);
				}
			}

			//变更物料号的变更项
			var p1 = salesOrderChangeDetList
				.Where(t => t.SoSeqNo > 0 && (!t.ItemNo.EqualsIgnoreCase(t.ToItemNo))
				).ToList();

			//交期的变更项
			var p2 = salesOrderChangeDetList
				.Where(t => t.SoSeqNo > 0 && (t.FrmEtd > t.ToEtd)
				).Select(t => t.SoSeqNo).ToList();

			//变更数量
			var p3 = salesOrderChangeDetList
				.Where(t => t.SoSeqNo > 0 && (t.FrmQty != t.ToQty)
				).ToList();

			//判断是否已采购
			//根据销售单号、序号和物料号获取采购
			pars = new List<SqlParam>();

			string param = string.Empty;
			string sqlAdd = "";
			if (p1.Count > 0)
			{
				int i = 1;
				foreach (var item in p1)
				{
					pars.Add(new SqlParam("@FSEQ" + i, KDDbType.Int64, item.SoSeqNo));
					pars.Add(new SqlParam("@FNUMBER" + i, KDDbType.String, item.ItemNo));
					sqlAdd += $" OR (B.FSEQ=@FSEQ{i} AND D.FNUMBER=@FNUMBER{i}) ";
					i++;
				}
			}

			pars.Add(new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo));
			sql = $@"SELECT COUNT(1) FROM T_SAL_ORDER A
            INNER JOIN T_SAL_ORDERENTRY B ON A.FID=B.FID
            INNER JOIN T_PUR_POORDERENTRY_R C ON C.FDEMANDBILLENTRYID=B.FENTRYID
            INNER JOIN T_BD_MATERIAL D ON D.FMATERIALID=B.FMATERIALID
            INNER JOIN t_PUR_POOrder E ON C.FID=E.FID
            WHERE E.FCLOSESTATUS='A' AND A.FBILLNO=@FBILLNO AND (1!=1 {sqlAdd})";
			var retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
			if (retCount > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶已采购，不能变更[ERR001]";
				return response;
			}

			//根据销售单号和序号获取采购
			pars = new List<SqlParam>();
			param = string.Empty;
			sqlAdd = "";
			if (p2.Count > 0)
			{
				int i = 1;
				foreach (var item in p2)
				{
					if (i == 1)
						param = "@FSEQ" + i;
					else
						param += ",@FSEQ" + i;
					pars.Add(new SqlParam("@FSEQ" + i++, KDDbType.String, item));
				}
			}
			else
			{
				param += "@FSEQ";
				pars.Add(new SqlParam("@FSEQ", KDDbType.String, "0"));
			}

			pars.Add(new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo));
			sql = $@"SELECT COUNT(1) FROM T_SAL_ORDER A
            INNER JOIN T_SAL_ORDERENTRY B ON A.FID=B.FID
            INNER JOIN T_PUR_POORDERENTRY_R C ON C.FDEMANDBILLENTRYID=B.FENTRYID
            INNER JOIN t_PUR_POOrder D ON C.FID=D.FID
            WHERE D.FCLOSESTATUS='A' AND A.FBILLNO=@FBILLNO AND B.FSEQ IN ({param})";
			retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
			if (retCount > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶已采购，不能变更[ERR002]";
				return response;
			}

			//根据销售单号、序号和采购数量获取采购
			pars = new List<SqlParam>();
			param = string.Empty;
			sqlAdd = "";
			if (p3.Count > 0)
			{
				int i = 1;
				foreach (var item in p3)
				{
					pars.Add(new SqlParam("@FSEQ" + i, KDDbType.Int64, item.SoSeqNo));
					pars.Add(new SqlParam("@FQTY" + i, KDDbType.Int64, item.ToQty));
					sqlAdd += $" OR (B.FSEQ=@FSEQ{i} AND E.FQTY>@FQTY{i}) ";
					i++;
				}
			}
			pars.Add(new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo));
			sql = $@"SELECT COUNT(1) FROM T_SAL_ORDER A
            INNER JOIN T_SAL_ORDERENTRY B ON A.FID=B.FID
            INNER JOIN T_PUR_POORDERENTRY_R C ON C.FDEMANDBILLENTRYID=B.FENTRYID
            INNER JOIN t_PUR_POOrder D ON C.FID=D.FID
            INNER JOIN T_PUR_POORDERENTRY E ON E.FID=D.FID
            WHERE D.FCLOSESTATUS='A' AND A.FBILLNO=@FBILLNO AND (1!=1 {sqlAdd})";
			retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
			if (retCount > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶已采购，不能变更[ERR003]";
				return response;
			}

			//获得更改物料号或者数量或者发货日期的Seq
			var soSeqList = salesOrderChangeDetList
				.Where(t => t.SoSeqNo > 0 && (!t.ItemNo.EqualsIgnoreCase(t.ToItemNo)
											  || t.FrmQty != t.ToQty
											  || t.FrmEtd > t.ToEtd)
				).Select(t => t.SoSeqNo).ToList();

			//判断是否已送货
			pars = new List<SqlParam>();

			param = string.Empty;

			if (soSeqList.Count > 0)
			{
				int i = 1;
				foreach (var item in soSeqList)
				{
					if (i == 1)
						param = "@FSEQ" + i;
					else
						param += ",@FSEQ" + i;
					pars.Add(new SqlParam("@FSEQ" + i++, KDDbType.String, item));
				}
			}
			else
			{
				param += "@FSEQ";
				pars.Add(new SqlParam("@FSEQ", KDDbType.String, "0"));
			}
			pars.Add(new SqlParam("@FSOORDERNO", KDDbType.String, request.SalesOrderNo));
			sql = $@"SELECT SORD.FSEQ FROM T_SAL_OUTSTOCK  SOUT
                INNER JOIN T_SAL_OUTSTOCKENTRY_R SOUTR ON SOUT.FID=SOUTR.FID
                INNER JOIN T_SAL_ORDERENTRY SORD ON SOUTR.FSOENTRYID=SORD.FENTRYID
                WHERE SOUT.FCANCELSTATUS='A' AND FSOORDERNO=@FSOORDERNO AND SORD.FSEQ IN ({param})";
			var existDelivery = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			if (existDelivery.Count > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶已送货";
				return response;
			}

			//是否存在组织间需求单
			pars = new List<SqlParam>();
			pars.Add(new SqlParam("@FBILLNO", KDDbType.String, request.SalesOrderNo));
			sql = @"SELECT COUNT(1) FROM T_PLN_REQUIREMENTORDER where FSALEORDERNO=@FBILLNO and FISCLOSED='A' ";
			retCount = DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray());
			if (retCount > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "该订单已存在组织间需求单";
				return response;
			}

			//获得所有变更SeqNo
			var seqListAll = salesOrderChangeDetList.Select(t => t.SoSeqNo).ToList();

			//判断是否对账

			pars = new List<SqlParam>();
			param = string.Empty;
			if (seqListAll.Count > 0)
			{
				int i = 1;
				foreach (var item in seqListAll)
				{
					if (i == 1)
						param = "@FSEQ" + i;
					else
						param += ",@FSEQ" + i;
					pars.Add(new SqlParam("@FSEQ" + i++, KDDbType.String, item));
				}
			}
			else
			{
				param += "@FSEQ";
				pars.Add(new SqlParam("@FSEQ", KDDbType.String, "0"));
			}
			pars.Add(new SqlParam("@FORDERNUMBER", KDDbType.String, request.SalesOrderNo));
			sql = $@"SELECT B.FORDERENTRYSEQ FROM t_AR_receivable A
            INNER JOIN t_AR_receivableEntry B ON A.FID=B.FID
            WHERE A.FCANCELSTATUS='A' AND B.FORDERNUMBER=@FORDERNUMBER AND B.FORDERENTRYSEQ IN ({param})";
			var existSalesStatement = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			if (existSalesStatement.Count > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶已对帐";
				return response;
			}

			//判断是否已开票

			pars = new List<SqlParam>();
			param = string.Empty;
			if (seqListAll.Count > 0)
			{
				int i = 1;
				foreach (var item in seqListAll)
				{
					if (i == 1)
						param = "@FORDERENTRYSEQ" + i;
					else
						param += ",@FORDERENTRYSEQ" + i;
					pars.Add(new SqlParam("@FORDERENTRYSEQ" + i++, KDDbType.String, item));
				}
			}
			else
			{
				param += "@FORDERENTRYSEQ";
				pars.Add(new SqlParam("@FORDERENTRYSEQ", KDDbType.String, "0"));
			}
			pars.Add(new SqlParam("@FORDERNUMBER", KDDbType.String, request.SalesOrderNo));
			sql = $@"SELECT B.FORDERENTRYSEQ FROM t_AR_receivable A
            INNER JOIN t_AR_receivableEntry B ON A.FID=B.FID
            INNER JOIN T_IV_SALESICENTRY C ON C.FSALESORDERNO=B.FORDERNUMBER AND C.FSRCROWID=B.FENTRYID
            INNER JOIN T_IV_SALESIC D ON D.FID=C.FID
            WHERE A.FCANCELSTATUS='A' AND D.FCANCELSTATUS='A' AND B.FORDERNUMBER=@FORDERNUMBER AND B.FORDERENTRYSEQ IN ({param})";
			var existSalesInvoice = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			if (existSalesInvoice.Count > 0)
			{
				response.Code = ResponseCode.Exception;
				response.Message = "金蝶已开票";
				return response;
			}

			//销售费用已存在
			//var existSalesCost = salesOrderChangeDetList.Where(x => x.AdditionalCost > 0).Any();
			//if (existSalesCost)
			//{
			//    response.IsSuccess = false;
			//    response.Message = "销售费用已存在";
			//    return response;
			//}

			response.Code = ResponseCode.Success;
			response.Message = "可变更";
			return response;
		}

		/// <summary>
		///  计算信用额度
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<decimal> CalcAmountOccupied(Context ctx, string message)
		{
			ResponseMessage<decimal> response = new ResponseMessage<decimal>();
			if (string.IsNullOrWhiteSpace(message))
			{
				response.Code = ResponseCode.NoExistsData;
				response.Message = "参数不能为空";
				return response;
			}
			var pars = new List<SqlParam>();
			string param = "''";
			List<string> request = JsonConvert.DeserializeObject<List<string>>(message);
			if (request.Count > 0)
			{
				int i = 1;
				foreach (var item in request)
				{
					if (i == 1)
						param = "@custcode" + i;
					else
						param += ",@custcode" + i;
					pars.Add(new SqlParam("@custcode" + i++, KDDbType.String, item));
				}
			}
			else
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "参数不能为空";
				return response;
			}
			string sql = string.Format(@"select Convert(decimal(18,2),isnull((select sum(Amount) from OrderAmount_Old where CustCode in ({0})),0)+ isnull(sum(case when FCLOSESTATUS='A' THEN FALLAMOUNT ELSE (FSTOCKOUTQTY- FRETURNQTY)* FTAXPRICE END),0)-
            (select isnull(sum(FREALRECAMOUNTFOR),0) from T_AR_RECEIVEBILL b inner join T_BD_CUSTOMER c on b.FCONTACTUNIT = c.FCUSTID and b.FCONTACTUNITTYPE = 'BD_Customer' where c.FNUMBER in ({0}))+
            (select isnull(sum(FREALREFUNDAMOUNTFOR),0) from T_AR_REFUNDBILL b inner join T_BD_CUSTOMER c on b.FCONTACTUNIT = c.FCUSTID and b.FCONTACTUNITTYPE = 'BD_Customer' where c.FNUMBER in ({0})))
            ToTALAMOUNT
            from T_SAL_ORDER a
            left join T_SAL_ORDERENTRY b on
            a.FID=b.FID 
            inner join T_SAL_ORDERENTRY_F c on
            b.FENTRYID=c.FENTRYID
            inner join T_SAL_ORDERENTRY_R d on
            d.FENTRYID = b.FENTRYID
            where  FDOCUMENTSTATUS='C'and exists (select 1 from T_BD_CUSTOMER b where a.FCUSTID=b.FCUSTID and b.FNUMBER in ({0}))", param);
			var amouont = DBServiceHelper.ExecuteScalar<decimal>(ctx, sql, 0, pars.ToArray());

			response.Code = ResponseCode.Success;
			response.Data = amouont;
			return response;
		}

		/// <summary>
		/// 查询逾期数据
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<QueryExpiryResponse> QueryCustomerExpiry(Context ctx, string message)
		{
			ResponseMessage<QueryExpiryResponse> response = new ResponseMessage<QueryExpiryResponse>();
			List<string> request = JsonConvert.DeserializeObject<List<string>>(message);
			string param = string.Empty;
			List<SqlParam> pars = new List<SqlParam>();
			if (request != null && request.Count > 0)
			{
				int i = 1;
				foreach (var item in request)
				{
					if (i == 1)
						param = "@custcode" + i;
					else
						param += ",@custcode" + i;
					pars.Add(new SqlParam("@custcode" + i++, KDDbType.String, item));
				}
			}
			else
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "参数不能为空";
				return response;
			}
			response.Data = new QueryExpiryResponse();
			//查询核销金额
			var sql = string.Format(@"select t.FBILLFORMID,m.FSRCDATE as FDATE,m.FSRCBILLID as FID,m.FMATCHAMOUNTFOR as FALLAMOUNTFOR
from T_AR_MatckEntry m
	inner join T_BAS_BILLTYPE t on m.FSOURCETYPE = t.FBILLTYPEID
	inner join T_BD_CUSTOMER c on m.FCONTACTUNIT = c.FCUSTID and m.FCONTACTUNITTYPE='BD_Customer'
where  c.FNUMBER in({0}) and m.FSOURCETYPE in ('180ecd4afd5d44b5be78a6efe4a7e041','36cf265bd8c3452194ed9c83ec5e73d2','ef06f87d394a462d9f96cb2397803372')", param);
			var matchDatas = CreateCustomerExpiry(DBServiceHelper.ExecuteReader(ctx, sql, pars));

			//查询应收单
			sql = string.Format(@"select r.FID,r.FENDDATE as FDATE,r.FALLAMOUNTFOR,'AR_receivable' as FBILLFORMID 
from T_AR_RECEIVABLE r
    inner join T_BD_CUSTOMER c on r.FCUSTOMERID = c.FCUSTID 
where c.FNUMBER in({0})
order by r.FENDDATE", param);
			var receivableDatas = CreateCustomerExpiry(DBServiceHelper.ExecuteReader(ctx, sql, pars));
			//if (reqest.OccupyCredit != 0)
			//{
			//    receivableDatas.Insert(0, new CustomerExpiry() { Date = new DateTime(2017, 1, 1), Amount = reqest.OccupyCredit });
			//}
			//减去特殊核销的金额
			MatchDeductions(receivableDatas, matchDatas, "AR_receivable");
			//正负先核销
			receivableDatas = ReceivableDeductions(receivableDatas);
			//收款单
			sql = string.Format(@"select r.FID,r.FDATE,r.FRECEIVEAMOUNTFOR as FALLAMOUNTFOR,'AR_RECEIVEBILL' as FBILLFORMID
from T_AR_RECEIVEBILL r
	inner join T_BD_CUSTOMER c on r.FCONTACTUNIT = c.FCUSTID and r.FCONTACTUNITTYPE='BD_Customer'
where c.FNUMBER in({0}) and r.FBILLTYPEID = '36cf265bd8c3452194ed9c83ec5e73d2'
order by r.FDATE", param);
			var receiveDatas = CreateCustomerExpiry(DBServiceHelper.ExecuteReader(ctx, sql, pars));
			//减去特殊核销的金额
			MatchDeductions(receiveDatas, matchDatas, "AR_RECEIVEBILL");

			sql = string.Format(@"select r.FID,r.FDATE,r.FREFUNDTOTALAMOUNTFOR as FALLAMOUNTFOR,'AR_REFUNDBILL' as FBILLFORMID
from T_AR_REFUNDBILL r
	inner join T_BD_CUSTOMER c on r.FCONTACTUNIT = c.FCUSTID and r.FCONTACTUNITTYPE='BD_Customer'
where c.FNUMBER in({0}) and r.FBILLTYPEID = 'ef06f87d394a462d9f96cb2397803372'
order by r.FDATE", param);
			var refundDatas = CreateCustomerExpiry(DBServiceHelper.ExecuteReader(ctx, sql, pars));
			//减去特殊核销的金额
			MatchDeductions(refundDatas, matchDatas, "AR_REFUNDBILL");

			receiveDatas = Deductions(receiveDatas, refundDatas.Sum(p => p.Amount));
			//计算账龄
			receivableDatas = Deductions(receivableDatas, receiveDatas.Sum(p => p.Amount));
			//移除没有到期的
			receivableDatas = receivableDatas.Where(r => r.Date < DateTime.Now.Date).ToList();
			if (receivableDatas.Count > 0)
			{
				response.Data.ExpiryDay = DateTime.Now.Date.Subtract(receivableDatas[0].Date).Days;
				response.Data.ExpiryAmount = receivableDatas.Sum(p => p.Amount);
			}
			response.Code = ResponseCode.Success;
			return response;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="receivableDatas"></param>
		/// <param name="matchDatas"></param>
		/// <param name="formId"></param>
		private void MatchDeductions(List<CustomerExpiry> receivableDatas, List<CustomerExpiry> matchDatas, string formId)
		{
			var datas = matchDatas.Where(r => r.FormId.Equals(formId, StringComparison.OrdinalIgnoreCase)).ToList();
			foreach (var data in datas)
			{
				var receivable = receivableDatas.FirstOrDefault(r => r.Id == data.Id);
				if (receivable != null)
				{
					receivable.Amount -= data.Amount;
					if (receivable.Amount == 0)
					{
						receivableDatas.Remove(receivable);
					}
				}
			}
		}
		/// <summary>
		/// 正负核销
		/// </summary>
		/// <param name="receivableDatas"></param>
		/// <returns></returns>
		private List<CustomerExpiry> ReceivableDeductions(List<CustomerExpiry> receivableDatas)
		{
			List<CustomerExpiry> justs = new List<CustomerExpiry>();
			List<CustomerExpiry> negatives = new List<CustomerExpiry>();
			foreach (var receivableData in receivableDatas)
			{
				if (receivableData.Amount > 0)
				{
					justs.Add(receivableData);
				}
				else
				{
					negatives.Add(receivableData);
				}
			}

			return Deductions(justs, negatives.Sum(p => 0 - p.Amount));
		}

		/// <summary>
		/// 正负抵扣
		/// </summary>
		/// <param name="justs"></param>
		/// <param name="negatives"></param>
		/// <returns></returns>
		private List<CustomerExpiry> Deductions(List<CustomerExpiry> justs, decimal negativeAmount)
		{
			var removes = new List<CustomerExpiry>();
			foreach (var just in justs)
			{
				if (just.Amount > negativeAmount)
				{
					just.Amount -= negativeAmount;
					break;
				}
				else
				{
					negativeAmount -= just.Amount;
					removes.Add(just);
				}
			}
			removes.ForEach(r => justs.Remove(r));
			return justs;
		}

		/// <summary>
		/// 将数据库对象转成实体
		/// </summary>
		/// <param name="dataReader"></param>
		/// <returns></returns>
		private List<CustomerExpiry> CreateCustomerExpiry(IDataReader dataReader)
		{
			List<CustomerExpiry> data = new List<CustomerExpiry>();
			using (dataReader)
			{
				while (dataReader.Read())
				{
					data.Add(new CustomerExpiry()
					{
						Id = Convert.ToInt64(dataReader["FID"]),
						FormId = Convert.ToString(dataReader["FBILLFORMID"]),
						Date = Convert.ToDateTime(dataReader["FDATE"]),
						Amount = Convert.ToDecimal(dataReader["FALLAMOUNTFOR"])
					});
				}
			}

			return data;
		}
		private List<CustomerExpiryBill> CreateCustomerExpiryBill(IDataReader dataReader)
		{
			List<CustomerExpiryBill> data = new List<CustomerExpiryBill>();
			using (dataReader)
			{
				while (dataReader.Read())
				{
					data.Add(new CustomerExpiryBill()
					{
						BillNo = Convert.ToString(dataReader["FBILLNO"]),
						Id = Convert.ToInt64(dataReader["FID"]),
						FormId = Convert.ToString(dataReader["FBILLFORMID"]),
						Date = Convert.ToDateTime(dataReader["FDATE"]),
						Amount = Convert.ToDecimal(dataReader["FALLAMOUNTFOR"])
					});
				}
			}

			return data;
		}

		/// <summary>
		/// 获取销售订单成本价列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetSalesOrderCostPriceDet(Context ctx, string message)
		{
			PageResqust<SalesOrderCostPriceFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<SalesOrderCostPriceFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			PageReponse<SalesOrderCostPriceEntity> costPrice = new PageReponse<SalesOrderCostPriceEntity>();
			costPrice.Data = new List<SalesOrderCostPriceEntity>();

			var pars = new List<SqlParam>
			{
				new SqlParam("@PageIndex", KDDbType.Int32,filter.PageIndex),
				new SqlParam("@PageSize", KDDbType.Int32,filter.PageSize),
                //new SqlParam("@CompanyCode", KDDbType.String, filter.Filter.CompanyCode),
                new SqlParam("@ProductModel", KDDbType.String,  filter.Filter.ProductModel),
				new SqlParam("@CUST_NAMEE", KDDbType.String,  filter.Filter.CustName??""),
				new SqlParam("@ProductName", KDDbType.String, filter.Filter.ProductName),
				new SqlParam("@SmallClass", KDDbType.String,  filter.Filter.SmallClass),
				new SqlParam("@ProductEngineer", KDDbType.String,  filter.Filter.ProductEngineer??""),
				new SqlParam("@UpdateUser", KDDbType.String, filter.Filter.UpdateUser),
				new SqlParam("@Inquirer", KDDbType.String, filter.Filter.Inquirer),
				new SqlParam("@OrderStartDate", KDDbType.DateTime, filter.Filter.OrderStartDate == null ? Convert.ToDateTime("1900-01-01 00:00:00") :filter.Filter.OrderStartDate.Value),
				new SqlParam("@OrderEndDate", KDDbType.DateTime, filter.Filter.OrderEndDate == null ? Convert.ToDateTime("1900-01-01 00:00:00") :filter.Filter.OrderEndDate.Value.AddDays(1)),
				new SqlParam("@SupplierName", KDDbType.String,  filter.Filter.SupplierName??""),
				new SqlParam("@OrderType", KDDbType.String,  filter.Filter.OrderType??""),
			};

			var localeId = ctx.UserLocale.LCID;
			pars.Add(new SqlParam("@LocaleId", KDDbType.Int64, localeId));
			//金蝶销售订单
			var supplyOrgSql = "";

			//金蝶组织间需求单
			var plnSupplyOrgSql = "";
			if (filter.Filter.SupplyOrgId > 0)
			{
				supplyOrgSql = $" and salord.FSupplyTargetOrgId={filter.Filter.SupplyOrgId} ";
				plnSupplyOrgSql = $" and pln.FSupplyOrganId={filter.Filter.SupplyOrgId} ";
			}
			string countSql = $@"/*dialect*/select count(1) from (
                        select org.FNUMBER COMPANY_ID,orgl.FNAME CCOMP_NAME,cust.FNUMBER CUST_CODE,custl.FNAME CUST_NAMEC,
                        convert(nvarchar(10),salord.FSMALLID) ITEM_TYPE,gl.FNAME TYPE_DESC,mhel.FNAME PRODUCT_ENGINEER_NAME,salord.FCustItemNo SIDE_MARKB,salord.FCustItemName CustItemName,
                        mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC,salf.FTAXPRICE VAT_PRICE,salord.FQTY QTY,supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
                        salord.FSupplierUnitPrice SupplierUnitPrice,salord.FSupplierUnitPriceSource SupplierUnitPriceSource,
                        sal.FBILLNO SO_NO,salord.FSEQ SEQ_NO,salesl.FNAME SALES_NAME,sal.FSalesOrderDate SO_DATE,sald.FDELIVERYDATE RTD,salord.FCostPriceUpdateUser CostPriceUpdateUser,
                        salord.FCostPriceUpdateDate CostPriceUpdateDate,
                        mat.FProductId ProductId,cust.FCorrespondOrgId,salord.FOrderDetailId FbdDetId,org2.FNUMBER SupplyOrgCode,orgl2.FNAME SupplyOrgName,'SalesOrder' OrderType
                        from T_SAL_ORDERENTRY salord
                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                        inner join t_org_organizations org on org.FORGID=sal.FSALEORGID
                        inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=@LocaleId
                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=@LocaleId
                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
                        left join T_HR_EMPINFO mhe on salord.FPRODUCTENGINEERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
                        left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId
                        left join V_BD_SALESMAN sales  on sales.FID=sal.FSalerId
                        left join V_BD_SALESMAN_L salesl on sales.fid=salesl.fid
                        inner join t_org_organizations org2 on org2.FORGID=salord.FSupplyTargetOrgId
                        inner join T_ORG_ORGANIZATIONS_L orgl2 on org2.FORGID=orgl2.FORGID and orgl2.FLOCALEID=@LocaleId
                        where sal.FDOCUMENTSTATUS='C'  and sal.FCANCELSTATUS='A' and (sal.FCancelStatus<>'B' 
                        or exists(select top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO)) 
                        {supplyOrgSql}
                        union all
						select org.FNUMBER COMPANY_ID,orgl.FNAME CCOMP_NAME,
						cust.FNUMBER CUST_CODE,custl.FNAME CUST_NAMEC,
						convert(nvarchar(10),pln.FSMALLID) ITEM_TYPE,gl.FNAME TYPE_DESC,mhel.FNAME PRODUCT_ENGINEER_NAME,'' SIDE_MARKB,'' CustItemName,
						mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC,pln.F_PENY_Price VAT_PRICE,pln.FDemandQty QTY,supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
						pln.FSupplierUnitPrice SupplierUnitPrice,pln.FSupplierUnitPriceSource SupplierUnitPriceSource,
						pln.FBILLNO SO_NO,pln.FSaleOrderEntrySeq SEQ_NO,'' SALES_NAME,pln.FDemandDate SO_DATE,pln.FFirmFinishDate RTD,pln.FCostPriceUpdateUser CostPriceUpdateUser,
						pln.FCostPriceUpdateDate CostPriceUpdateDate,
						mat.FProductId ProductId,1 FCorrespondOrgId,pln.FID FbdDetId,org.FNUMBER SupplyOrgCode,orgl.FNAME SupplyOrgName,'PlnReqOrder' OrderType
						from T_PLN_REQUIREMENTORDER pln
						inner join t_org_organizations org on org.FORGID=pln.FSupplyOrganId
						inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=@LocaleId
						inner join t_org_organizations cust on cust.FORGID=pln.FDemandOrgId
						inner join T_ORG_ORGANIZATIONS_L custl on cust.FORGID=custl.FORGID and custl.FLOCALEID=@LocaleId
						inner join T_BD_MATERIAL mat on pln.FMATERIALID=mat.FMATERIALID
						inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
						left join T_BD_MATERIALGROUP_L gl on pln.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
						left join T_HR_EMPINFO mhe on pln.FPRODUCTENGINEERID=mhe.FID
						left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
						left join t_BD_Supplier sup on sup.FSUPPLIERID=pln.FSupplierId
						left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId
						where pln.FDemandType in (0,8) and pln.FDocumentStatus='C' --and pln.FIsClosed='A' 
                        {plnSupplyOrgSql}
                        ) datas  where 1=1 ";

			string sql = $@"/*dialect*/select * from (
                        select org.FNUMBER COMPANY_ID,orgl.FNAME CCOMP_NAME,cust.FNUMBER CUST_CODE,custl.FNAME CUST_NAMEC,
                        convert(nvarchar(10),salord.FSMALLID) ITEM_TYPE,gl.FNAME TYPE_DESC,mhel.FNAME PRODUCT_ENGINEER_NAME,salord.FCustItemNo SIDE_MARKB,salord.FCustItemName CustItemName,
                        mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC,salf.FTAXPRICE VAT_PRICE,salord.FQTY QTY,supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
                        salord.FSupplierUnitPrice SupplierUnitPrice,salord.FSupplierUnitPriceSource SupplierUnitPriceSource,
                        sal.FBILLNO SO_NO,salord.FSEQ SEQ_NO,salesl.FNAME SALES_NAME,sal.FSalesOrderDate SO_DATE,sald.FDELIVERYDATE RTD,salord.FCostPriceUpdateUser CostPriceUpdateUser,
                        salord.FCostPriceUpdateDate CostPriceUpdateDate,
                        mat.FProductId ProductId,cust.FCorrespondOrgId,salord.FOrderDetailId FbdDetId,org2.FNUMBER SupplyOrgCode,orgl2.FNAME SupplyOrgName,'SalesOrder' OrderType
                        from T_SAL_ORDERENTRY salord
                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                        inner join t_org_organizations org on org.FORGID=sal.FSALEORGID
                        inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=@LocaleId
                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=@LocaleId
                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
                        left join T_HR_EMPINFO mhe on salord.FPRODUCTENGINEERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
                        left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId
                        left join V_BD_SALESMAN sales  on sales.FID=sal.FSalerId
                        left join V_BD_SALESMAN_L salesl on sales.fid=salesl.fid
                        inner join t_org_organizations org2 on org2.FORGID=salord.FSupplyTargetOrgId
                        inner join T_ORG_ORGANIZATIONS_L orgl2 on org2.FORGID=orgl2.FORGID and orgl2.FLOCALEID=@LocaleId
                        where sal.FDOCUMENTSTATUS='C'  and sal.FCANCELSTATUS='A' and (sal.FCancelStatus<>'B' 
                        or exists(select top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO))
                        {supplyOrgSql}
                        union all
						select org.FNUMBER COMPANY_ID,orgl.FNAME CCOMP_NAME,
						cust.FNUMBER CUST_CODE,custl.FNAME CUST_NAMEC,
						convert(nvarchar(10),pln.FSMALLID) ITEM_TYPE,gl.FNAME TYPE_DESC,mhel.FNAME PRODUCT_ENGINEER_NAME,'' SIDE_MARKB,'' CustItemName,
						mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC,pln.F_PENY_Price VAT_PRICE,pln.FDemandQty QTY,supl.FNAME SupplierName,sup.FNUMBER SupplierCode,
						pln.FSupplierUnitPrice SupplierUnitPrice,pln.FSupplierUnitPriceSource SupplierUnitPriceSource,
						pln.FBILLNO SO_NO,pln.FSaleOrderEntrySeq SEQ_NO,'' SALES_NAME,pln.FDemandDate SO_DATE,pln.FFirmFinishDate RTD,pln.FCostPriceUpdateUser CostPriceUpdateUser,
						pln.FCostPriceUpdateDate CostPriceUpdateDate,
						mat.FProductId ProductId,1 FCorrespondOrgId,pln.FID FbdDetId,org.FNUMBER SupplyOrgCode,orgl.FNAME SupplyOrgName,'PlnReqOrder' OrderType
						from T_PLN_REQUIREMENTORDER pln
						inner join t_org_organizations org on org.FORGID=pln.FSupplyOrganId
						inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=@LocaleId
						inner join t_org_organizations cust on cust.FORGID=pln.FDemandOrgId
						inner join T_ORG_ORGANIZATIONS_L custl on cust.FORGID=custl.FORGID and custl.FLOCALEID=@LocaleId
						inner join T_BD_MATERIAL mat on pln.FMATERIALID=mat.FMATERIALID
						inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
						left join T_BD_MATERIALGROUP_L gl on pln.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
						left join T_HR_EMPINFO mhe on pln.FPRODUCTENGINEERID=mhe.FID
						left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
						left join t_BD_Supplier sup on sup.FSUPPLIERID=pln.FSupplierId
						left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId
						where pln.FDemandType in (0,8) and pln.FDocumentStatus='C' --and pln.FIsClosed='A' 
                        {plnSupplyOrgSql}
                        ) datas where 1=1  ";

			if (filter.Filter.IsInsideCust != null)
			{
				if (filter.Filter.IsInsideCust.Value)
				{
					countSql += @" and FCorrespondOrgId>0 ";
					sql += @" and FCorrespondOrgId>0 ";
				}
				else
				{
					countSql += @" and FCorrespondOrgId=0  ";
					sql += @" and FCorrespondOrgId=0 ";
				}
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.CustName))
			{
				countSql += @" and isnull(CUST_NAMEC, '') like '%' + @CUST_NAMEE + '%'";
				sql += @" and isnull(CUST_NAMEC, '') like '%' + @CUST_NAMEE + '%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductModel))
			{
				countSql += @" and (ITEM_NO like '%' + @ProductModel + '%' or SIDE_MARKB like '%' + @ProductModel + '%')";
				sql += @" and (ITEM_NO like '%' + @ProductModel + '%' or SIDE_MARKB like '%' + @ProductModel + '%')";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductName))
			{
				countSql += @" and (ITEM_DESC like '%' + @ProductName + '%' or CustItemName like '%' + @ProductName + '%')";
				sql += @" and (ITEM_DESC like '%' + @ProductName + '%' or CustItemName like '%' + @ProductName + '%')";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				countSql += @" and isnull(TYPE_DESC, '') like '%' + @SmallClass + '%'";
				sql += @" and isnull(TYPE_DESC, '') like '%' + @SmallClass + '%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductEngineer))
			{
				countSql += @" and PRODUCT_ENGINEER_NAME = @ProductEngineer";
				sql += @" and PRODUCT_ENGINEER_NAME = @ProductEngineer";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.UpdateUser))
			{
				countSql += @" and CostPriceUpdateUser like '%' + @UpdateUser + '%'";
				sql += @" and CostPriceUpdateUser like '%' + @UpdateUser + '%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.Inquirer))
			{
				countSql += @" and SALES_NAME like '%' + @Inquirer+ '%'";
				sql += @" and SALES_NAME like '%' + @Inquirer+ '%'";
			}
			if (filter.Filter.OrderStartDate != null)
			{
				countSql += @" and SO_DATE >= @OrderStartDate";
				sql += @" and SO_DATE >= @OrderStartDate";
			}
			if (filter.Filter.OrderEndDate != null)
			{
				countSql += @" and SO_DATE < @OrderEndDate";
				sql += @" and SO_DATE < @OrderEndDate";
			}
			if (filter.Filter.HasCostPrice != null)
			{
				if (filter.Filter.HasCostPrice.Value)
				{
					countSql += @" and (SupplierUnitPrice != 0 and isnull(SupplierName,'') != '') ";
					sql += @" and (SupplierUnitPrice != 0 and isnull(SupplierName,'') != '') ";
				}
				else
				{
					countSql += @" and (SupplierUnitPrice = 0 or isnull(SupplierName,'') = '') ";
					sql += @" and (SupplierUnitPrice = 0 or isnull(SupplierName,'') = '') ";
				}
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.AuthoritySmallClass))
			{
				countSql += $@" and ITEM_TYPE in ('{filter.Filter.AuthoritySmallClass}')";
				sql += $@" and ITEM_TYPE in ('{filter.Filter.AuthoritySmallClass}')";
			}

			//是否无供应商
			if (filter.Filter.IsNoSupplier != null)
			{
				if (filter.Filter.IsNoSupplier.Value)
				{
					countSql += @" and isnull(SupplierCode,'') = '' ";
					sql += @" and isnull(SupplierCode,'') = '' ";
				}
				else
				{
					countSql += @" and isnull(SupplierCode,'') != '' ";
					sql += @" and isnull(SupplierCode,'') != '' ";
				}
			}
			//订单类型
			if (!string.IsNullOrEmpty(filter.Filter.OrderType))
			{
				countSql += @" and OrderType = @OrderType ";
				sql += @" and OrderType = @OrderType ";
			}

			//供应商名称
			if (!string.IsNullOrWhiteSpace(filter.Filter.SupplierName))
			{
				countSql += @" and isnull(SupplierName, '') like '%' + @SupplierName + '%'";
				sql += @" and isnull(SupplierName, '') like '%' + @SupplierName + '%'";
			}

			sql += @" order by convert(varchar(10),SO_DATE,20) desc,TYPE_DESC asc 
                        offset ((@PageIndex-1)*@PageSize) rows
                        fetch next @PageSize rows only";

			var retCount = DBServiceHelper.ExecuteScalar<int>(ctx, countSql, 0, paramList: pars.ToArray());
			costPrice.Count = retCount;
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				var entity = new SalesOrderCostPriceEntity
				{
					CompanyCode = Convert.ToString(data["COMPANY_ID"]),
					CompanyName = Convert.ToString(data["CCOMP_NAME"]),
					CustCode = Convert.ToString(data["CUST_CODE"]),
					CustName = Convert.ToString(data["CUST_NAMEC"]),
					SmallClass = Convert.ToString(data["TYPE_DESC"]),
					SmallClassId = string.IsNullOrEmpty(Convert.ToString(data["ITEM_TYPE"])) ? "0" : Convert.ToString(data["ITEM_TYPE"]),
					ProductEngineer = Convert.ToString(data["PRODUCT_ENGINEER_NAME"]),
					CustProductModel = Convert.ToString(data["SIDE_MARKB"]),
					CustProductName = Convert.ToString(data["CustItemName"]),
					MymoooProductModel = Convert.ToString(data["ITEM_NO"]),
					MymoooProductName = Convert.ToString(data["ITEM_DESC"]),
					UnitPrice = Convert.ToDecimal(data["VAT_PRICE"]),
					Count = Convert.ToInt32(data["QTY"]),
					SupplierName = Convert.ToString(data["SupplierName"]),
					SupplierCode = Convert.ToString(data["SupplierCode"]),
					SupplierUnitPrice = Convert.ToDecimal(data["SupplierUnitPrice"]),
					SupplierUnitPriceWhetherIncludeTax = true,
					SupplierUnitPriceSource = Convert.ToString(data["SupplierUnitPriceSource"]),
					SalesOrderNumber = Convert.ToString(data["SO_NO"]),
					SalesOrderSeqNumber = Convert.ToString(data["SEQ_NO"]),
					Inquirer = Convert.ToString(data["SALES_NAME"]),
					OrderDate = Convert.ToDateTime(data["SO_DATE"]).ToString("yyyy-MM-dd"),
					//DeliveryDate = Convert.ToDateTime(data["RTD"]).AddYears(-1).ToString("yyyy-MM-dd"),
					//UpdateDate = Convert.ToDateTime(data["CostPriceUpdateDate"]).ToString("yyyy-MM-dd HH:mm:ss"),
					UpdateUser = Convert.ToString(data["CostPriceUpdateUser"]),
					ProductId = Convert.ToInt64(data["ProductId"]),
					FbdDetId = Convert.ToInt64(data["FbdDetId"]),
					SupplyOrgCode = Convert.ToString(data["SupplyOrgCode"]),
					SupplyOrgName = Convert.ToString(data["SupplyOrgName"]),
					OrderType = Convert.ToString(data["OrderType"]),
				};
				if (Convert.ToInt32(data["FCorrespondOrgId"].ToString()) == 0)
				{
					entity.IsInsideCust = "否";
				}
				else
				{
					entity.IsInsideCust = "是";
				}
				if (!string.IsNullOrEmpty(Convert.ToString(data["RTD"])))
				{
					if (Convert.ToDateTime(data["RTD"]).ToString("yyyy-MM-dd").Contains("0001-01-01") || Convert.ToDateTime(data["RTD"]).ToString("yyyy-MM-dd").Contains("1900-01-01"))
					{
						entity.DeliveryDate = "";
					}
					else
					{
						entity.DeliveryDate = Convert.ToDateTime(data["RTD"]).ToString("yyyy-MM-dd");
					}
				}
				else
				{
					entity.DeliveryDate = "";
				}

				if (!string.IsNullOrEmpty(Convert.ToString(data["CostPriceUpdateDate"])))
				{
					if (Convert.ToDateTime(data["CostPriceUpdateDate"]).ToString("yyyy-MM-dd").Contains("0001-01-01") || Convert.ToDateTime(data["CostPriceUpdateDate"]).ToString("yyyy-MM-dd").Contains("1900-01-01"))
					{
						entity.UpdateDate = "";
					}
					else
					{
						entity.UpdateDate = Convert.ToDateTime(data["CostPriceUpdateDate"]).ToString("yyyy-MM-dd HH:mm:ss");
					}
				}
				else
				{
					entity.UpdateDate = "";
				}

				costPrice.Data.Add(entity);

			}
			response.Message = "成功";
			response.Data = costPrice;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 更新销售订单成本价
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpdateSalesOrderCostPriceDet(Context ctx, string message)
		{
			List<SalesOrderCostPriceEntity> filter = JsonConvertUtils.DeserializeObject<List<SalesOrderCostPriceEntity>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			foreach (var item in filter)
			{
				//var companyCode = GetErpSoCompanyCode(ctx, item.CompanyCode, item.SalesOrderNumber, Convert.ToInt32(item.SalesOrderSeqNumber), item.FbdDetId);
				////老ERP
				//if (!string.IsNullOrEmpty(companyCode))
				//{
				//    var pars = new List<SqlParam>
				//    {
				//        new SqlParam("@CompanyCode", KDDbType.String,item.CompanyCode),
				//        new SqlParam("@SalesOrderNumber", KDDbType.String,item.SalesOrderNumber),
				//        new SqlParam("@SupplierCode", KDDbType.String, item.SupplierCode),
				//        new SqlParam("@SupplierName", KDDbType.String, item.SupplierName),
				//        new SqlParam("@SupplierUnitPriceSource", KDDbType.String,item.SupplierUnitPriceSource),
				//        new SqlParam("@SupplierUnitPrice", KDDbType.Decimal,item.SupplierUnitPrice),
				//        new SqlParam("@CostPriceUpdateUser", KDDbType.String, item.UpdateUser)
				//    };
				//    var sql = $@"/*dialect*/UPDATE M_SOD_DET set SupplierName=@SupplierName,SupplierCode=@SupplierCode,
				//                SupplierUnitPriceSource=@SupplierUnitPriceSource,SupplierUnitPrice=@SupplierUnitPrice,
				//                CostPriceUpdateDate=GETDATE(),CostPriceUpdateUser=@CostPriceUpdateUser
				//                WHERE SO_NO=@SalesOrderNumber and  COMP_CODE=@CompanyCode ";
				//    var sql2 = $@" ;UPDATE {companyCode}DATA..M_SOD_DET set SupplierName=@SupplierName,SupplierCode=@SupplierCode,
				//                SupplierUnitPriceSource=@SupplierUnitPriceSource,SupplierUnitPrice=@SupplierUnitPrice,
				//                CostPriceUpdateDate=GETDATE(),CostPriceUpdateUser=@CostPriceUpdateUser
				//                WHERE SO_NO=@SalesOrderNumber ";
				//    if (!string.IsNullOrEmpty(item.SalesOrderSeqNumber))
				//    {
				//        pars.Add(new SqlParam("@SalesOrderSeqNumber", KDDbType.Int32, item.SalesOrderSeqNumber));
				//        sql += " and SEQ_NO =@SalesOrderSeqNumber ";
				//        sql2 += " and SEQ_NO =@SalesOrderSeqNumber ";
				//    }
				//    if (item.FbdDetId != 0)
				//    {
				//        pars.Add(new SqlParam("@FbdDetId", KDDbType.Int64, item.FbdDetId));
				//        sql += " and FbdDetId =@FbdDetId ";
				//        sql2 += " and FbdDetId =@FbdDetId ";
				//    }
				//    DBServiceHelper.Execute(ctx, sql + sql2, pars);
				//}
				//else
				//{
				long orgId = 0;
				if (!string.IsNullOrWhiteSpace(item.CompanyCode))
				{
					var org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(item.CompanyCode, ""));
					if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C"))
					{
						response.Code = ResponseCode.ModelError;
						response.Message = "对应的组织不存在或未审核";
						return response;
					}
					orgId = org.Id;
				}

				var SupplierId = DBServiceHelper.ExecuteScalar<long>(ctx, $"select top 1 FSUPPLIERID from t_BD_Supplier  where FNUMBER='{item.SupplierCode}' and FUSEORGID=1 ", 0);
				if (SupplierId == 0)
				{
					response.Code = ResponseCode.ModelError;
					response.Message = $"范思德不存在供应商编码[{item.SupplierCode}]";
					return response;
				}

				var pars = new List<SqlParam>
					{
						new SqlParam("@SalesOrderNumber", KDDbType.String,item.SalesOrderNumber),
						new SqlParam("@SupplierId", KDDbType.Int64, SupplierId),
						new SqlParam("@SupplierUnitPriceSource", KDDbType.String,item.SupplierUnitPriceSource),
						new SqlParam("@SupplierUnitPrice", KDDbType.Decimal,item.SupplierUnitPrice),
						new SqlParam("@CostPriceUpdateUser", KDDbType.String, item.UpdateUser)
					};
				if (item.OrderType.Equals("PlnReqOrder"))
				{
					var sql = $@"/*dialect*/update pln set FSUPPLIERID=@SupplierId,FSUPPLIERUNITPRICE=@SupplierUnitPrice,FCOSTPRICEUPDATEDATE=GETDATE(),
                                  FCostPriceUpdateUser=@CostPriceUpdateUser,FSupplierUnitPriceSource=@SupplierUnitPriceSource
                                 from T_PLN_REQUIREMENTORDER pln
                                 where pln.FBILLNO=@SalesOrderNumber ";
					if (!string.IsNullOrEmpty(item.SalesOrderSeqNumber))
					{
						pars.Add(new SqlParam("@SalesOrderSeqNumber", KDDbType.Int32, item.SalesOrderSeqNumber));
						sql += " and pln.FSaleOrderEntrySeq = @SalesOrderSeqNumber ";
					}
					if (item.FbdDetId != 0)
					{
						pars.Add(new SqlParam("@FbdDetId", KDDbType.Int64, item.FbdDetId));
						sql += " and pln.FID = @FbdDetId ";
					}
					DBServiceHelper.Execute(ctx, sql, pars);
				}
				else
				{
					var sql = $@"/*dialect*/update salord set FSUPPLIERID=@SupplierId,FSUPPLIERUNITPRICE=@SupplierUnitPrice,FCOSTPRICEUPDATEDATE=GETDATE(),
                                 FCostPriceUpdateUser=@CostPriceUpdateUser,FSupplierUnitPriceSource=@SupplierUnitPriceSource
                                 from T_SAL_ORDER sal,T_SAL_ORDERENTRY salord
                                 where sal.FID=salord.FID and sal.FBILLNO=@SalesOrderNumber ";
					if (!string.IsNullOrEmpty(item.SalesOrderSeqNumber))
					{
						pars.Add(new SqlParam("@SalesOrderSeqNumber", KDDbType.Int32, item.SalesOrderSeqNumber));
						sql += " and salord.FSEQ = @SalesOrderSeqNumber ";
					}
					if (item.FbdDetId != 0)
					{
						pars.Add(new SqlParam("@FbdDetId", KDDbType.Int64, item.FbdDetId));
						sql += " and salord.FORDERDETAILID = @FbdDetId ";
					}
					DBServiceHelper.Execute(ctx, sql, pars);
				}
				//}
			}

			response.Message = "更新成功";
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 填充成本
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> WritePoVatTtl(Context ctx, string message)
		{
			OrderGrossProfitEntityList list = JsonConvertUtils.DeserializeObject<OrderGrossProfitEntityList>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			foreach (var item in list.OrderGrossProfitEntity)
			{
				//var companyCode = GetErpSoCompanyCode(ctx, item.CompanyCode, item.SoNo, Convert.ToInt32(item.SeqNo), 0);

				var pars = new List<SqlParam>
					{
						new SqlParam("@CompanyCode", KDDbType.String,item.CompanyCode),
						new SqlParam("@SoNo", KDDbType.String,item.SoNo),
						new SqlParam("@SeqNo", KDDbType.Int32, item.SeqNo),
						new SqlParam("@SupplierUnitPrice", KDDbType.Decimal,item.SystemPoVatTtl),
					};

				//金蝶更新
				var sql = $@"/*dialect*/update salord set FSUPPLIERUNITPRICE=@SupplierUnitPrice from T_SAL_ORDER sal,T_SAL_ORDERENTRY salord
                where sal.FID=salord.FID and sal.FBILLNO=@SoNo and salord.FSEQ=@SeqNo  ";
				//老ERP更新
				//if (!string.IsNullOrEmpty(companyCode))
				//{
				//    sql = $@"/*dialect*/UPDATE M_SOD_DET SET SupplierUnitPrice= @SupplierUnitPrice where SO_NO=@SoNo AND SEQ_NO=@SeqNo and COMP_CODE=@CompanyCode;
				//             UPDATE {companyCode}DATA..M_SOD_DET SET SupplierUnitPrice= @SupplierUnitPrice where SO_NO=@SoNo AND SEQ_NO=@SeqNo ";
				//}
				DBServiceHelper.Execute(ctx, sql, pars);
			}
			response.Message = "更新成功";
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 批量更新订单毛利差异的成本
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpdateBalance(Context ctx, string message)
		{
			OrderGrossProfitExceptEntityList list = JsonConvertUtils.DeserializeObject<OrderGrossProfitExceptEntityList>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			foreach (var item in list.OrderGrossProfitExceptEntity)
			{
				if (string.IsNullOrWhiteSpace(item.OriginalSystemPoVatTtl))
				{
					item.OriginalSystemPoVatTtl = "0";
				}
				if (string.IsNullOrWhiteSpace(item.AdjustPrice))
				{
					item.AdjustPrice = "0";
				}
				if (string.IsNullOrWhiteSpace(item.SystemPoVatTtl))
				{
					item.SystemPoVatTtl = "0";
				}

				var pars = new List<SqlParam>
					{
						new SqlParam("@SoNo", KDDbType.String,item.SoNo),
						new SqlParam("@SeqNo", KDDbType.Int32, item.SeqNo),
						new SqlParam("@AdjustPrice", KDDbType.Decimal,item.OriginalSystemPoVatTtl),
						new SqlParam("@DifferReason", KDDbType.String,item.DifferReason??""),
						new SqlParam("@UpdateBalance", KDDbType.Decimal,Convert.ToDecimal(item.AdjustPrice) - Convert.ToDecimal(item.SystemPoVatTtl)),
					};
				var sql = $@"/*dialect*/update salord set FAdjustPrice=@AdjustPrice,FDifferReason=@DifferReason,FUpdateBalance=@UpdateBalance 
                from T_SAL_ORDER sal,T_SAL_ORDERENTRY salord
                where sal.FID=salord.FID and sal.FBILLNO=@SoNo and salord.FSEQ=@SeqNo ";
				DBServiceHelper.Execute(ctx, sql, pars);
			}
			response.Message = "更新成功";
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 零成本更新
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpdateZeroCost(Context ctx, string message)
		{
			PageResqust<OrderGrossProfitFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<OrderGrossProfitFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var localeId = ctx.UserLocale.LCID;
			var pars = new List<SqlParam> { new SqlParam("@LocaleId", KDDbType.Int64, localeId) };
			var sql = $@"/*dialect*/select 
                            salord.FENTRYID,
                            case when
                            salord.FAdjustPrice is null
                            then salord.FSupplierUnitPrice
                            else salord.FAdjustPrice
                            end AdjustPrice,salord.FSupplierUnitPrice PoVatTtl,
                            convert(decimal(18,6),0) SystemPoVatTtl,
							mat.FNUMBER ITEM_NO,sal.FSalesOrderDate SO_DATE
							into #t1
                            from T_SAL_ORDERENTRY salord
                            inner join T_SAL_ORDER sal on sal.FID=salord.FID
                            inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                            inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                            inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                            inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=@LocaleId
                            inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                            inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
                            left join T_HR_EMPINFO mhe on salord.FPRODUCTENGINEERID=mhe.FID
                            left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
                            where sal.FDOCUMENTSTATUS='C' and (sal.FCancelStatus<>'B' 
							or exists(select top 1 * from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO))  ";

			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductManager))
			{
				sql += $@" and mhel.FNAME = '{filter.Filter.ProductManager}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemNo))
			{
				sql += $@" and mat.FNUMBER like '%{filter.Filter.ItemNo}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
			{
				sql += $@" and salord.FSMALLID in ('{filter.Filter.SmallClassFilter}')";
			}
			if (filter.Filter.OrderStartDate != null)
			{
				sql += $@" and sal.FSalesOrderDate >= '{filter.Filter.OrderStartDate}'";
			}
			if (filter.Filter.OrderEndDate != null)
			{
				sql += $@" and sal.FSalesOrderDate <= '{filter.Filter.OrderEndDate.Value.ToString("yyyy-MM-dd 23:59:59")}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				sql += $@" and salord.FPARENTSMALLID = '{filter.Filter.ParentSmallName}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				sql += $@" and salord.FSMALLID='{filter.Filter.SmallClass}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
			{
				sql += $@" and matL.FNAME like '%{filter.Filter.ItemDesc}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.CustName))
			{
				sql += $@" and custl.FNAME like '%{filter.Filter.CustName}%'";
			}
			//改成查询事业部
			if (!string.IsNullOrWhiteSpace(filter.Filter.BusinessDivisionId))
			{
				sql += $@" and salord.FBUSINESSDIVISIONID='{filter.Filter.BusinessDivisionId}' ";
			}

			//处理数据
			sql += @" --更新最近采购价供应商(采购日期大于等于销售日期)
							update t1 set SystemPoVatTtl=t2.SystemPoVatTtl  from #t1 t1,(
							select ROW_NUMBER() OVER(PARTITION BY dmat.FNUMBER ORDER BY m.FCREATEDATE asc) i,
													dmat.FNUMBER ITEM_NO,isnull(f.FTAXPRICE,0) SystemPoVatTtl,isnull(convert(nvarchar(30),m.FDATE,121),'') SystemCalculateDate,convert(nvarchar(200),isnull(supl.FNAME,'')) VDR_NAMEC
													from t_PUR_POOrderEntry d
													inner join t_PUR_POOrder m on m.FID=d.FID
													inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
													left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
													left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
													left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052 
													inner join #t1 datas ON dmat.FNUMBER=datas.ITEM_NO and m.FDATE>= convert(varchar(10),datas.SO_DATE,20)
													where  m.FCANCELSTATUS='A' and f.FTAXPRICE>0) t2
							where t1.ITEM_NO=t2.ITEM_NO and t2.i=1 	


							--更新最近采购价供应商(采购日期小于销售日期)需要已批核的
							update t1 set SystemPoVatTtl=t2.SystemPoVatTtl from #t1 t1,(
							select ROW_NUMBER() OVER(PARTITION BY dmat.FNUMBER ORDER BY m.FCREATEDATE desc) i,
													dmat.FNUMBER ITEM_NO,isnull(f.FTAXPRICE,0) SystemPoVatTtl,isnull(convert(nvarchar(30),m.FDATE,121),'') SystemCalculateDate,convert(nvarchar(200),isnull(supl.FNAME,'')) VDR_NAMEC
													from t_PUR_POOrderEntry d
													inner join t_PUR_POOrder m on m.FID=d.FID
													inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
													left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
													left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
													left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052 
													inner join #t1 datas ON dmat.FNUMBER=datas.ITEM_NO and m.FDATE< convert(varchar(10),datas.SO_DATE,20)
													where  m.FDOCUMENTSTATUS='C' and m.FCANCELSTATUS='A' and f.FTAXPRICE>0) t2
							where t1.ITEM_NO=t2.ITEM_NO and t2.i=1 and t1.SystemPoVatTtl=0

							--更新组装拆卸单价格
							update t1 set SystemPoVatTtl=t2.SystemPoVatTtl  from #t1 t1,(
							select t1.FBILLNO,t1.FDATE SystemCalculateDate,t1.GDItemNo,((lysPrice+FEE)/FQTY) SystemPoVatTtl from (
							select ROW_NUMBER() OVER (PARTITION BY dmat.FNUMBER ORDER BY ly.FAPPROVEDATE desc) AS rid,ly.FID,lyp.FENTRYID,FBILLNO,dmat.FNUMBER GDItemNo,lyp.FEE,lyp.FQTY,ly.FDATE from T_STK_ASSEMBLY ly
													inner join T_STK_ASSEMBLYPRODUCT lyp on lyp.FID=ly.FID
													inner join T_BD_MATERIAL dmat on lyp.FMATERIALID=dmat.FMATERIALID
													where ly.FAffairType='Assembly' and ly.FDOCUMENTSTATUS='C'
							) t1 
							inner join (select lys.FENTRYID,SUM(lys.FQTY*lysPo.VatPrice) lysPrice  from T_STK_ASSEMBLYSUBITEM lys
							inner join T_BD_MATERIAL lysMat on lys.FMATERIALID=lysMat.FMATERIALID
							inner join (select ROW_NUMBER() OVER (PARTITION BY dmat.FNUMBER ORDER BY m.FDATE desc) AS rid,dmat.FNUMBER PoItemNo,f.FTAXPRICE VatPrice from t_PUR_POOrderEntry d
													inner join t_PUR_POOrder m on m.FID=d.FID
													inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
													left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
							where m.FDOCUMENTSTATUS='C' and  m.FCANCELSTATUS='A' and f.FTAXPRICE!= 0) lysPo on lysPo.rid=1 and lysPo.PoItemNo=lysMat.FNUMBER
							group by lys.FENTRYID) lys on lys.FENTRYID=t1.FENTRYID
							where t1.rid=1
							) t2
							where t1.ITEM_NO=t2.GDItemNo  and t1.SystemPoVatTtl=0 ";



			sql += $@" /*dialect*/update salord set FAdjustPrice=b.PoVatTtl  from T_SAL_ORDERENTRY salord,#t1 b where b.FENTRYID=salord.FENTRYID and (b.AdjustPrice = 0 or b.AdjustPrice < isnull(b.SystemPoVatTtl,0))  drop table #t1 ";
			DBServiceHelper.Execute(ctx, sql, pars);
			response.Message = "更新成功";
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取旧ERP的销售单账套
		/// </summary>
		/// <param name="soNo"></param>
		/// <param name="seqNo"></param>
		/// <param name="fbdDetId"></param>
		/// <returns></returns>
		private string GetErpSoCompanyCode(Context ctx, string companyCode, string soNo, int seqNo = 0, long fbdDetId = 0)
		{
			var sql = $@"select top 1 COMP_CODE from M_SOD_DET where COMP_CODE='{companyCode}' ";
			if (!string.IsNullOrWhiteSpace(soNo))
			{
				sql += $" and SO_NO='{soNo}' ";
			}
			if (seqNo > 0)
			{
				sql += $" and SEQ_NO='{seqNo}' ";
			}
			if (fbdDetId > 0)
			{
				sql += $" and FbdDetId='{fbdDetId}' ";
			}

			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, "");
		}

		/// <summary>
		/// 查询订单毛利异常数据，调整单价<0或调整单价<系统计算单价
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetOrderGrossProfitExceptList(Context ctx, string message)
		{
			string sqlFilter = "";
			List<OrderGrossProfitExceptEntity> costPrice = new List<OrderGrossProfitExceptEntity>();

			PageResqust<OrderGrossProfitFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<OrderGrossProfitFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var localeId = ctx.UserLocale.LCID;

			var pars = new List<SqlParam> { new SqlParam("@LocaleId", KDDbType.Int64, localeId) };
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductManager))
			{
				sqlFilter += $@" and mhel.FNAME = '{filter.Filter.ProductManager}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemNo))
			{
				sqlFilter += $@" and mat.FNUMBER like '%{filter.Filter.ItemNo}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
			{
				sqlFilter += $@" and salord.FSMALLID in ('{filter.Filter.SmallClassFilter}')";
			}
			if (filter.Filter.OrderStartDate != null)
			{
				sqlFilter += $@" and sal.FSalesOrderDate >= '{filter.Filter.OrderStartDate}'";
			}
			if (filter.Filter.OrderEndDate != null)
			{
				sqlFilter += $@" and sal.FSalesOrderDate <= '{filter.Filter.OrderEndDate.Value.ToString("yyyy-MM-dd 23:59:59")}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				sqlFilter += $@" and salord.FPARENTSMALLID = '{filter.Filter.ParentSmallName}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				sqlFilter += $@" and salord.FSMALLID='{filter.Filter.SmallClass}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
			{
				sqlFilter += $@" and matL.FNAME like '%{filter.Filter.ItemDesc}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.CustName))
			{
				sqlFilter += $@" and custl.FNAME like '%{filter.Filter.CustName}%'";
			}
			//改成查询事业部
			if (!string.IsNullOrWhiteSpace(filter.Filter.BusinessDivisionId))
			{
				sqlFilter += $@" and salord.FBUSINESSDIVISIONID='{filter.Filter.BusinessDivisionId}' ";
			}

			var sql = $@"/*dialect*/If Object_id('Tempdb..#SupplierPrice') Is Not Null
                                        Begin
                                            Drop Table #SupplierPrice
                                        End
                                        create table #SupplierPrice
                                        (
	                                        UpdateBalance	decimal(23,6),
	                                        AdjustPrice	decimal(23,6),
	                                        Differreason	nvarchar(1000),
	                                        DecimalPlacesOfUnitPrice	int,
	                                        CUST_CODE	nvarchar(1000),
	                                        SALES_NAME	nvarchar(1000),
	                                        SupplierName	nvarchar(1000),
	                                        SO_NO	nvarchar(1000),
	                                        SEQ_NO	int,
	                                        EntryId	int,
	                                        PRODUCT_ENGINEER_NAME	nvarchar(1000),
	                                        GROUP_DESC	nvarchar(1000),
	                                        TYPE_DESC	nvarchar(1000),
	                                        ITEM_NO	nvarchar(1000),
	                                        ITEM_DESCC	nvarchar(1000),
	                                        QTY	decimal(23,6),
	                                        VAT_PRICE	decimal(23,6),
	                                        SoVatTtl	decimal(23,6),
	                                        SO_DATE	datetime,
	                                        PoVatTtl	decimal(23,6),
	                                        TotalPoVatTtl	decimal(23,6),
	                                        VatProfit	decimal(23,6),
	                                        VatProfitRate	decimal(23,6),
	                                        VDR_NAMEC	nvarchar(1000),
	                                        SystemPoVatTtl	decimal(23,4) not null default 0,
	                                        SystemCalculateDate	datetime
                                        )

                                        insert into #SupplierPrice(UpdateBalance,AdjustPrice,Differreason,DecimalPlacesOfUnitPrice,CUST_CODE,SALES_NAME,SupplierName,SO_NO,SEQ_NO,EntryId
                                        ,PRODUCT_ENGINEER_NAME,GROUP_DESC,TYPE_DESC,ITEM_NO,ITEM_DESCC
                                        ,QTY,VAT_PRICE,SoVatTtl,SO_DATE,PoVatTtl,TotalPoVatTtl,VatProfit,VatProfitRate)
                                        select  salord.FUpdateBalance,
                                        case when
                                        salord.FAdjustPrice is null
                                        then salord.FSupplierUnitPrice
                                        else salord.FAdjustPrice
                                        end AdjustPrice,
                                        salord.FDifferReason,
                                        cust.FDecimalPlacesOfUnitPrice,cust.FNUMBER CUST_CODE,
                                        salesl.FNAME SALES_NAME,supl.FNAME SupplierName,
                                        sal.FBILLNO SO_NO,salord.FSEQ SEQ_NO,salord.FENTRYID,
                                        mhel.FNAME PRODUCT_ENGINEER_NAME,gll.FNAME GROUP_DESC,gl.FNAME TYPE_DESC,mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC,
                                        salord.FQTY QTY,salf.FTAXPRICE VAT_PRICE,salf.FALLAMOUNT SoVatTtl,sal.FSalesOrderDate SO_DATE,
                                        salord.FSupplierUnitPrice PoVatTtl,(salord.FQTY*salord.FSupplierUnitPrice) TotalPoVatTtl,
                                        salf.FALLAMOUNT-(salord.FQTY*salord.FSupplierUnitPrice) VatProfit,
                                        (salf.FALLAMOUNT-(salord.FQTY*salord.FSupplierUnitPrice)) /nullif(salf.FALLAMOUNT,0) VatProfitRate
                                        from T_SAL_ORDERENTRY salord
                                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=@LocaleId
                                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
                                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
                                        left join T_BD_MATERIALGROUP_L gll on salord.FPARENTSMALLID = gll.FID and gll.FLOCALEID = @LocaleId
                                        left join T_HR_EMPINFO mhe on salord.FPRODUCTENGINEERID=mhe.FID
                                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
                                        left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
                                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId
                                        left join V_BD_SALESMAN sales  on sales.FID=sal.FSalerId
                                        left join V_BD_SALESMAN_L salesl on sales.fid=salesl.fid 
	                                        where sal.FDOCUMENTSTATUS='C' and (sal.FCancelStatus<>'B' or exists(select top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO)) 
                                            {sqlFilter}

	                                    create index #Idx_SupplierPrice_ItemNo on #SupplierPrice(ITEM_NO)

	                                    update tmp
	                                    set VDR_NAMEC = t.VDR_NAMEC,SystemPoVatTtl=t.VAT_PRICE,SystemCalculateDate=t.PO_DATE
	                                    from #SupplierPrice tmp
	                                    inner join (select  
	                                    f.FTAXPRICE VAT_PRICE ,supl.FNAME VDR_NAMEC,m.FDATE PO_DATE,r.FDEMANDBILLENTRYID EntryId,ROW_NUMBER() OVER(PARTITION BY sp.SO_NO,sp.SEQ_NO ORDER BY m.FCREATEDATE asc,d.FENTRYID asc ) AS rowNumber
	                                    from t_PUR_POOrderEntry d
	                                    inner join t_PUR_POOrder m on m.FID=d.FID
	                                    inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
	                                    inner join T_PUR_POORDERENTRY_R r on r.FENTRYID=d.FENTRYID
	                                    inner join  T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
	                                    left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
	                                    left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId 
	                                    inner join #SupplierPrice sp on sp.ITEM_NO = mat.FNUMBER and convert(varchar(10),m.FDATE,20) >= convert(varchar(10),sp.SO_DATE,20) 
	                                    and convert(varchar(10),m.FDATE,20) < convert(varchar(10),dateadd(m,3,sp.SO_DATE),20)  
	                                    where m.FCANCELSTATUS='A' and r.FStockInQty>0) t on tmp.EntryId = t.EntryId and t.rowNumber = 1

	                                    update tmp
	                                    set VDR_NAMEC = t.VDR_NAMEC,SystemPoVatTtl=t.VAT_PRICE,SystemCalculateDate=t.PO_DATE
	                                    from #SupplierPrice tmp
	                                    inner join (select  
	                                    f.FTAXPRICE VAT_PRICE ,supl.FNAME VDR_NAMEC,m.FDATE PO_DATE,r.FDEMANDBILLENTRYID EntryId,ROW_NUMBER() OVER(PARTITION BY sp.SO_NO,sp.SEQ_NO ORDER BY m.FCREATEDATE asc,d.FENTRYID asc ) AS rowNumber
	                                    from t_PUR_POOrderEntry d
	                                    inner join t_PUR_POOrder m on m.FID=d.FID
	                                    inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
	                                    inner join T_PUR_POORDERENTRY_R r on r.FENTRYID=d.FENTRYID
	                                    inner join  T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
	                                    left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
	                                    left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId 
	                                    inner join #SupplierPrice sp on sp.ITEM_NO = mat.FNUMBER and convert(varchar(10),m.FDATE,20) < convert(varchar(10),sp.SO_DATE,20) and sp.SystemPoVatTtl = 0 
	                                    where m.FDOCUMENTSTATUS='C' and  m.FCANCELSTATUS='A' and r.FStockInQty>0 ) t on tmp.EntryId = t.EntryId and t.rowNumber = 1

	                                    If Object_id('Tempdb..#TmpItemNoPrice') Is Not Null
	                                    Begin
		                                    Drop Table #TmpItemNoPrice
	                                    End
	                                    select	t.ITEM_NO,t.VAT_PRICE
	                                    into #TmpItemNoPrice
	                                    from (
		                                    select  
		                                    f.FTAXPRICE VAT_PRICE,mat.FNUMBER ITEM_NO,ROW_NUMBER() OVER(PARTITION BY mat.FNUMBER ORDER BY m.FCREATEDATE desc ) AS rowNumber
		                                    from t_PUR_POOrderEntry d
		                                    inner join t_PUR_POOrder m on m.FID=d.FID
		                                    inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
		                                    inner join T_PUR_POORDERENTRY_R r on r.FENTRYID=d.FENTRYID
		                                    inner join T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
		                                    where m.FDOCUMENTSTATUS='C' and  m.FCANCELSTATUS='A' and r.FStockInQty>0  and f.FTAXPRICE>0
		                                    and exists (select 1 from T_STK_ASSEMBLY ly
					                                    inner join T_STK_ASSEMBLYPRODUCT lyp on ly.FID=lyp.FID
					                                    inner join T_STK_ASSEMBLYSUBITEM lys on lyp.FENTRYID=lys.FENTRYID
					                                    inner join T_BD_MATERIAL pmat on lyp.FMATERIALID=pmat.FMATERIALID
					                                    inner join T_BD_MATERIAL smat on lys.FMATERIALID=smat.FMATERIALID
					                                    where ly.FAffairType='Assembly' and exists (select 1 from #SupplierPrice sp where pmat.FNUMBER = sp.ITEM_NO and sp.SystemPoVatTtl = 0) and smat.FNUMBER = mat.FNUMBER)
		                                    ) t where t.rowNumber = 1

	                                    create index #Idx_TmpItemNoPrice on #TmpItemNoPrice(ITEM_NO)

	                                    update tmp
		                                    set VDR_NAMEC = t.RECFG_NO,SystemPoVatTtl=t.Prcie,SystemCalculateDate=t.Appdate
		                                    from #SupplierPrice tmp
			                                    inner join (select son.RECFG_NO,son.ITEM_NO,son.Prcie,son.Appdate,ROW_NUMBER() OVER(PARTITION BY son.ITEM_NO ORDER BY son.Appdate desc) AS rowNumber
						                                    from (
						                                    select ly.FBILLNO RECFG_NO,ly.FAPPROVEDATE Appdate,pmat.FNUMBER ITEM_NO,
						                                    (SUM(lys.FQTY*t.VAT_PRICE)+(SUM(ly.FEE)/SUM(1)))/(SUM(lyp.FQTY)/SUM(1)) Prcie
						                                    from T_STK_ASSEMBLY ly
						                                    inner join T_STK_ASSEMBLYPRODUCT lyp on ly.FID=lyp.FID
						                                    inner join T_STK_ASSEMBLYSUBITEM lys on lyp.FENTRYID=lys.FENTRYID
						                                    inner join T_BD_MATERIAL pmat on lyp.FMATERIALID=pmat.FMATERIALID
						                                    inner join T_BD_MATERIAL smat on lys.FMATERIALID=smat.FMATERIALID
						                                    inner join #TmpItemNoPrice t on pmat.FNUMBER = t.ITEM_NO
						                                    where exists (select 1 from #SupplierPrice sp where pmat.FNUMBER = sp.ITEM_NO and sp.SystemPoVatTtl = 0)
						                                    group by ly.FBILLNO,ly.FAPPROVEDATE,pmat.FNUMBER
						                                    having (SUM(lys.FQTY*t.VAT_PRICE)+(SUM(ly.FEE)/SUM(1)) > 0)) son
						                                    ) t on tmp.ITEM_NO = t.ITEM_NO and t.rowNumber = 1 

                                                select AdjustPrice,SystemPoVatTtl,* from #SupplierPrice a 
                                                where a.AdjustPrice = 0 or a.AdjustPrice < a.SystemPoVatTtl order by a.PRODUCT_ENGINEER_NAME,a.GROUP_DESC,a.TYPE_DESC,ITEM_DESCC ";

			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				OrderGrossProfitExceptEntity orderGrossProfitEntity = new OrderGrossProfitExceptEntity();
				orderGrossProfitEntity.SoNo = Convert.ToString(data["SO_NO"]);
				orderGrossProfitEntity.SeqNo = Convert.ToString(data["SEQ_NO"]);
				orderGrossProfitEntity.CustCode = Convert.ToString(data["CUST_CODE"]);
				orderGrossProfitEntity.SoDate = data["SO_DATE"] is DBNull ? "" : Convert.ToDateTime(data["SO_DATE"]).ToString("yyyy-MM-dd");
				orderGrossProfitEntity.SalesMan = Convert.ToString(data["SALES_NAME"]);
				orderGrossProfitEntity.SmallClass = Convert.ToString(data["TYPE_DESC"]);
				orderGrossProfitEntity.ProductEngineer = Convert.ToString(data["PRODUCT_ENGINEER_NAME"]);
				orderGrossProfitEntity.MymoooProductModel = Convert.ToString(data["ITEM_NO"]);
				orderGrossProfitEntity.MymoooProductName = Convert.ToString(data["ITEM_DESCC"]);
				orderGrossProfitEntity.VatPrice = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["VAT_PRICE"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.Count = Convert.ToDecimal(data["QTY"]).ToString("#0.00");
				orderGrossProfitEntity.SupplierName = Convert.ToString(data["SupplierName"]);
				orderGrossProfitEntity.CompanyCode = filter.Filter.CompanyCode;
				orderGrossProfitEntity.BusinessDivisionId = filter.Filter.BusinessDivisionId;
				orderGrossProfitEntity.AdjustPrice = Convert.ToString(Decimal.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["AdjustPrice"])) ? 0 : data["AdjustPrice"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.PoVatTtl = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["PoVatTtl"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.SystemCalculateDate = data["SystemCalculateDate"] is DBNull ? "" : Convert.ToDateTime(data["SystemCalculateDate"]).ToString("yyyy-MM-dd");
				orderGrossProfitEntity.SystemPoVatTtl = Convert.ToString(Decimal.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["SystemPoVatTtl"])) ? 0 : data["SystemPoVatTtl"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.OriginalSystemPoVatTtl = Convert.ToString(Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["SystemPoVatTtl"])) ? 0 : data["SystemPoVatTtl"]));
				orderGrossProfitEntity.Vdr_Namec = Convert.ToString(data["VDR_NAMEC"]);
				orderGrossProfitEntity.UpdateBalance = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["UpdateBalance"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.DifferReason = Convert.ToString(data["Differreason"]);
				if (Convert.ToDecimal(orderGrossProfitEntity.PoVatTtl) == 0 && Convert.ToDecimal(orderGrossProfitEntity.SystemPoVatTtl) == 0
					&& Convert.ToDecimal(orderGrossProfitEntity.AdjustPrice) == 0 && !string.IsNullOrWhiteSpace(orderGrossProfitEntity.DifferReason))
				{
					continue;
				}
				costPrice.Add(orderGrossProfitEntity);

			}
			response.Message = "成功";
			response.Data = costPrice;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取销售订单预估毛利汇总
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetOrderGrossProfitList(Context ctx, string message)
		{
			PageResqust<OrderGrossProfitFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<OrderGrossProfitFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var localeId = ctx.UserLocale.LCID;

			List<OrderGrossProfitEntity> costPrice = new List<OrderGrossProfitEntity>();

			var sql = $@"/*dialect*/select
                             sum(t.SoVatTtl) SoVatTtl
                            ,sum(t.TotalPoVatTtl) TotalPoVatTtl
                            ,sum(t.SoVatTtl)-sum(t.TotalPoVatTtl) VatProfit
                            ,(sum(t.SoVatTtl)-sum(t.TotalPoVatTtl))/nullif(sum(t.SoVatTtl),0) VatProfitRate ";
			var groupSql = $@" group by ";
			var orderbySql = $@" order by ";
			var whereSql = $" where 1=1 ";
			//汇总依据不为空则按选择的依据汇总，否则按订单汇总
			if (!string.IsNullOrWhiteSpace(filter.Filter.SummaryBasis))
			{
				if (filter.Filter.SummaryBasis.Contains("ProductManager"))
				{
					sql += $@",t.PRODUCT_ENGINEER_NAME ";
					groupSql += " t.PRODUCT_ENGINEER_NAME,";
					orderbySql += " t.PRODUCT_ENGINEER_NAME,";
				}
				if (filter.Filter.SummaryBasis.Contains("ParentSmallName"))
				{
					sql += $@",t.GROUP_DESC ";
					groupSql += " t.GROUP_DESC,";
					orderbySql += " t.GROUP_DESC,";
				}
				if (filter.Filter.SummaryBasis.Contains("SmallClass"))
				{
					sql += $@",t.TYPE_DESC ";
					groupSql += " t.TYPE_DESC,";
					orderbySql += " t.TYPE_DESC,";
				}
				if (filter.Filter.SummaryBasis.Contains("ItemDesc"))
				{
					sql += $@",t.ITEM_DESC ";
					groupSql += " t.ITEM_DESC ";
					orderbySql += " t.ITEM_DESC,";
				}
			}
			else
			{
				sql += $@",t.SO_NO ";
				groupSql += " t.SO_NO ";
			}
			sql += $@" from (
                        select mhel.FNAME PRODUCT_ENGINEER_NAME,convert(nvarchar(10),salord.FPARENTSMALLID) ITEM_GRP,gll.FNAME GROUP_DESC,convert(nvarchar(10),salord.FSMALLID) ITEM_TYPE,gl.FNAME TYPE_DESC,mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC,
                        sal.FBILLNO SO_NO,salord.FSEQ SEQ_NO,sal.FSalesOrderDate SO_DATE,
                        cust.FCorrespondOrgId CUST_INTERIOR,org.FNUMBER COMP_CODE,(salf.FTAXPRICE*salord.FQTY) SoVatTtl,
                        (salord.FSupplierUnitPrice*salord.FQTY) TotalPoVatTtl,(salf.FTAXPRICE*salord.FQTY-salord.FSupplierUnitPrice*salord.FQTY) VatProfit,salord.FBusinessDivisionId BusinessDivisionId
                        from T_SAL_ORDERENTRY salord
                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                        inner join t_org_organizations org on org.FORGID=sal.FSALEORGID
                        inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID={localeId}
                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID={localeId}
                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = {localeId}
                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = {localeId}
						left join T_BD_MATERIALGROUP_L gll on salord.FPARENTSMALLID = gll.FID and gll.FLOCALEID = {localeId}
                        left join T_HR_EMPINFO mhe on salord.FPRODUCTMANAGERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = {localeId}
                        left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = {localeId}
                        left join V_BD_SALESMAN sales  on sales.FID=sal.FSalerId
                        left join V_BD_SALESMAN_L salesl on sales.fid=salesl.fid 
                        where sal.FDOCUMENTSTATUS='C' and (sal.FCancelStatus<>'B' 
                        or exists(select top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO)) 
                        ) t ";
			if (filter.Filter.IsInsideCust != null)
			{
				if (filter.Filter.IsInsideCust.Value)
				{
					whereSql += " and CUST_INTERIOR = 1 ";
				}
				else
				{
					whereSql += " and CUST_INTERIOR = 0 ";
				}
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductManager))
			{
				whereSql += $@" and PRODUCT_ENGINEER_NAME = '{filter.Filter.ProductManager}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				whereSql += $@" and ITEM_GRP = '{filter.Filter.ParentSmallName}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				whereSql += $@" and ITEM_TYPE='{filter.Filter.SmallClass}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
			{
				whereSql += $@" and ITEM_DESC like '%{filter.Filter.ItemDesc}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
			{
				whereSql += $@" and ITEM_TYPE in ('{filter.Filter.SmallClassFilter}')";
			}
			if (filter.Filter.OrderStartDate != null)
			{
				whereSql += $@" and SO_DATE >= '{filter.Filter.OrderStartDate}'";
			}
			if (filter.Filter.OrderEndDate != null)
			{
				whereSql += $@" and SO_DATE <= '{filter.Filter.OrderEndDate.Value.ToString("yyyy-MM-dd 23:59:59")}'";
			}
			//改成查询事业部
			if (!string.IsNullOrWhiteSpace(filter.Filter.BusinessDivisionId))
			{
				whereSql += $@" and BusinessDivisionId='{filter.Filter.BusinessDivisionId}' ";
			}
			sql += whereSql;
			if (!string.IsNullOrWhiteSpace(filter.Filter.SummaryBasis))
			{
				sql += groupSql.TrimEnd(',');
				sql += orderbySql.Trim(',');
			}
			else
			{
				sql += groupSql.TrimEnd(',');
			}

			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				OrderGrossProfitEntity orderGrossProfitEntity = new OrderGrossProfitEntity();
				orderGrossProfitEntity.SoVatTtl = Convert.ToDecimal(data["SoVatTtl"]).ToString("#0.00");
				orderGrossProfitEntity.TotalPoVatTtl = Convert.ToDecimal(data["TotalPoVatTtl"]).ToString("#0.00");
				orderGrossProfitEntity.VatProfit = Convert.ToDecimal(data["VatProfit"]).ToString("#0.00");//销售额-采购额
				orderGrossProfitEntity.VatProfitRate = Convert.ToDouble(data["VatProfitRate"] is DBNull ? 0 : data["VatProfitRate"]).ToString("P");
				if (!string.IsNullOrWhiteSpace(filter.Filter.SummaryBasis))
				{
					if (filter.Filter.SummaryBasis.Contains("ProductManager"))
					{
						orderGrossProfitEntity.ProductManager = Convert.ToString(data["PRODUCT_ENGINEER_NAME"]);
					}
					if (filter.Filter.SummaryBasis.Contains("ParentSmallName"))
					{
						orderGrossProfitEntity.ParentSmallName = Convert.ToString(data["GROUP_DESC"]);
					}
					if (filter.Filter.SummaryBasis.Contains("SmallClass"))
					{
						orderGrossProfitEntity.SmallClass = Convert.ToString(data["TYPE_DESC"]);
					}
					if (filter.Filter.SummaryBasis.Contains("ItemDesc"))
					{
						orderGrossProfitEntity.ItemDesc = Convert.ToString(data["ITEM_DESC"]);
					}
				}
				else
				{
					orderGrossProfitEntity.SoNo = Convert.ToString(data["SO_NO"]);
				}
				costPrice.Add(orderGrossProfitEntity);

			}
			response.Message = "成功";
			response.Data = costPrice;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 查询订单毛利成本分析数据
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetOrderGrossProfitAnalysisList(Context ctx, string message)
		{
			string sqlFilter = "";
			List<OrderGrossProfitAnalysisEntity> costPrice = new List<OrderGrossProfitAnalysisEntity>();

			PageResqust<OrderGrossProfitFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<OrderGrossProfitFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var localeId = ctx.UserLocale.LCID;
			var pars = new List<SqlParam> { new SqlParam("@LocaleId", KDDbType.Int64, localeId) };
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductManager))
			{
				sqlFilter += $@" and mhel.FNAME = '{filter.Filter.ProductManager}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemNo))
			{
				sqlFilter += $@" and mat.FNUMBER like '%{filter.Filter.ItemNo}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
			{
				sqlFilter += $@" and salord.FSMALLID in ('{filter.Filter.SmallClassFilter}')";
			}
			if (filter.Filter.OrderStartDate != null)
			{
				sqlFilter += $@" and sal.FSalesOrderDate >= '{filter.Filter.OrderStartDate}'";
			}
			if (filter.Filter.OrderEndDate != null)
			{
				sqlFilter += $@" and sal.FSalesOrderDate <= '{filter.Filter.OrderEndDate.Value.ToString("yyyy-MM-dd 23:59:59")}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				sqlFilter += $@" and salord.FPARENTSMALLID = '{filter.Filter.ParentSmallName}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				sqlFilter += $@" and salord.FSMALLID='{filter.Filter.SmallClass}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
			{
				sqlFilter += $@" and matL.FNAME like '%{filter.Filter.ItemDesc}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.CustName))
			{
				sqlFilter += $@" and custl.FNAME like '%{filter.Filter.CustName}%'";
			}
			//改成查询事业部
			if (!string.IsNullOrWhiteSpace(filter.Filter.BusinessDivisionId))
			{
				sqlFilter += $@" and salord.FBUSINESSDIVISIONID='{filter.Filter.BusinessDivisionId}' ";
			}

			var sql = $@"/*dialect*/select cust.FDecimalPlacesOfUnitPrice DecimalPlacesOfUnitPrice,cust.FNUMBER CUST_CODE,
                        salesl.FNAME SALES_NAME,supl.FNAME SupplierName,
                        sal.FBILLNO SO_NO,salord.FSEQ SEQ_NO,salord.FENTRYID,
                        mhel.FNAME PRODUCT_ENGINEER_NAME,gll.FNAME GROUP_DESC,gl.FNAME TYPE_DESC,mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESCC,
                        salord.FQTY QTY,salf.FTAXPRICE VAT_PRICE,salf.FALLAMOUNT SoVatTtl,sal.FSalesOrderDate SO_DATE,
                        salord.FSupplierUnitPrice PoVatTtl,
                        salord.FAdjustPrice AdjustPrice,
                        (salord.FQTY*salord.FSupplierUnitPrice) TotalPoVatTtl,
                        salf.FALLAMOUNT-(salord.FQTY*salord.FSupplierUnitPrice) VatProfit,
                        (salf.FALLAMOUNT-(salord.FQTY*salord.FSupplierUnitPrice))/nullif(salf.FALLAMOUNT,0) VatProfitRate,
						convert(decimal(18,6),0) SystemPoVatTtl,convert(nvarchar(30),'') SystemCalculateDate,convert(nvarchar(200),'') VDR_NAMEC
						into #t1
                        from T_SAL_ORDERENTRY salord
                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=2052
                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = 2052
                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                        left join T_BD_MATERIALGROUP_L gll on salord.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
                        left join T_HR_EMPINFO mhe on salord.FPRODUCTENGINEERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = 2052
                        left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052
                        left join V_BD_SALESMAN sales  on sales.FID=sal.FSalerId
                        left join V_BD_SALESMAN_L salesl on sales.fid=salesl.fid 
                        where sal.FDOCUMENTSTATUS='C' and (sal.FCancelStatus<>'B' 
						or exists(select top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO)) " + sqlFilter;

			sql += @" --更新最近采购价供应商(采购日期大于等于销售日期)
                        update t1 set SystemPoVatTtl=t2.SystemPoVatTtl,SystemCalculateDate=t2.SystemCalculateDate,VDR_NAMEC=t2.VDR_NAMEC  from #t1 t1,(
                        select ROW_NUMBER() OVER(PARTITION BY dmat.FNUMBER ORDER BY m.FCREATEDATE asc) i,
						                        dmat.FNUMBER ITEM_NO,isnull(f.FTAXPRICE,0) SystemPoVatTtl,isnull(convert(nvarchar(30),m.FDATE,121),'') SystemCalculateDate,convert(nvarchar(200),isnull(supl.FNAME,'')) VDR_NAMEC
                                                from t_PUR_POOrderEntry d
                                                inner join t_PUR_POOrder m on m.FID=d.FID
                                                inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                                                left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
                                                left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
						                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052 
						                        inner join #t1 datas ON dmat.FNUMBER=datas.ITEM_NO and m.FDATE>= convert(varchar(10),datas.SO_DATE,20)
                                                where  m.FCANCELSTATUS='A' and f.FTAXPRICE>0) t2
                        where t1.ITEM_NO=t2.ITEM_NO and t2.i=1 	

                        --更新最近采购价供应商(采购日期小于销售日期)需要已批核的
                        update t1 set SystemPoVatTtl=t2.SystemPoVatTtl,SystemCalculateDate=t2.SystemCalculateDate,VDR_NAMEC=t2.VDR_NAMEC  from #t1 t1,(
                        select ROW_NUMBER() OVER(PARTITION BY dmat.FNUMBER ORDER BY m.FCREATEDATE desc) i,
						                        dmat.FNUMBER ITEM_NO,isnull(f.FTAXPRICE,0) SystemPoVatTtl,isnull(convert(nvarchar(30),m.FDATE,121),'') SystemCalculateDate,convert(nvarchar(200),isnull(supl.FNAME,'')) VDR_NAMEC
                                                from t_PUR_POOrderEntry d
                                                inner join t_PUR_POOrder m on m.FID=d.FID
                                                inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                                                left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
                                                left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
                                                left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = 2052 
						                        inner join #t1 datas ON dmat.FNUMBER=datas.ITEM_NO and m.FDATE< convert(varchar(10),datas.SO_DATE,20)
                                                where  m.FDOCUMENTSTATUS='C' and m.FCANCELSTATUS='A' and f.FTAXPRICE>0) t2
                        where t1.ITEM_NO=t2.ITEM_NO and t2.i=1 and t1.SystemPoVatTtl=0

                        --更新组装拆卸单价格
                        update t1 set SystemPoVatTtl=t2.SystemPoVatTtl,SystemCalculateDate=t2.SystemCalculateDate,VDR_NAMEC=t2.FBILLNO  from #t1 t1,(
                        select t1.FBILLNO,t1.FDATE SystemCalculateDate,t1.GDItemNo,((lysPrice+FEE)/FQTY) SystemPoVatTtl from (
                        select ROW_NUMBER() OVER (PARTITION BY dmat.FNUMBER ORDER BY ly.FAPPROVEDATE desc) AS rid,ly.FID,lyp.FENTRYID,FBILLNO,dmat.FNUMBER GDItemNo,lyp.FEE,lyp.FQTY,ly.FDATE from T_STK_ASSEMBLY ly
                                                inner join T_STK_ASSEMBLYPRODUCT lyp on lyp.FID=ly.FID
                                                inner join T_BD_MATERIAL dmat on lyp.FMATERIALID=dmat.FMATERIALID
                                                where ly.FAffairType='Assembly' and ly.FDOCUMENTSTATUS='C'
                        ) t1 
                        inner join (select lys.FENTRYID,SUM(lys.FQTY*lysPo.VatPrice) lysPrice  from T_STK_ASSEMBLYSUBITEM lys
                        inner join T_BD_MATERIAL lysMat on lys.FMATERIALID=lysMat.FMATERIALID
                        inner join (select ROW_NUMBER() OVER (PARTITION BY dmat.FNUMBER ORDER BY m.FDATE desc) AS rid,dmat.FNUMBER PoItemNo,f.FTAXPRICE VatPrice from t_PUR_POOrderEntry d
                                                inner join t_PUR_POOrder m on m.FID=d.FID
                                                inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                                                left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
                        where m.FDOCUMENTSTATUS='C' and  m.FCANCELSTATUS='A' and f.FTAXPRICE!= 0) lysPo on lysPo.rid=1 and lysPo.PoItemNo=lysMat.FNUMBER
                        group by lys.FENTRYID) lys on lys.FENTRYID=t1.FENTRYID
                        where t1.rid=1
                        ) t2
                        where t1.ITEM_NO=t2.GDItemNo  and t1.SystemPoVatTtl=0 ";

			sql += @"/*dialect*/select * from #t1 where AdjustPrice >= SystemPoVatTtl or (PoVatTtl<>0 and PoVatTtl>=SystemPoVatTtl) order by convert(varchar(10),SO_DATE,20) desc,TYPE_DESC,GROUP_DESC asc 
                      drop table #t1";


			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				OrderGrossProfitAnalysisEntity orderGrossProfitEntity = new OrderGrossProfitAnalysisEntity();
				orderGrossProfitEntity.SoNo = Convert.ToString(data["SO_NO"]);
				orderGrossProfitEntity.SeqNo = Convert.ToString(data["SEQ_NO"]);
				orderGrossProfitEntity.CustCode = Convert.ToString(data["CUST_CODE"]);
				orderGrossProfitEntity.SoDate = data["SO_DATE"] is DBNull ? "" : Convert.ToDateTime(data["SO_DATE"]).ToString("yyyy-MM-dd");
				orderGrossProfitEntity.SalesMan = Convert.ToString(data["SALES_NAME"]);
				orderGrossProfitEntity.SmallClass = Convert.ToString(data["TYPE_DESC"]);
				orderGrossProfitEntity.ProductEngineer = Convert.ToString(data["PRODUCT_ENGINEER_NAME"]);
				orderGrossProfitEntity.MymoooProductModel = Convert.ToString(data["ITEM_NO"]);
				orderGrossProfitEntity.MymoooProductName = Convert.ToString(data["ITEM_DESCC"]);
				orderGrossProfitEntity.VatPrice = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["VAT_PRICE"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));//Convert.ToDecimal(reader["VAT_PRICE"]).ToString("#0.0000");
				orderGrossProfitEntity.Count = Convert.ToDecimal(data["QTY"]).ToString("#0.00");
				orderGrossProfitEntity.SupplierName = Convert.ToString(data["SupplierName"]);
				orderGrossProfitEntity.CompanyCode = filter.Filter.CompanyCode;
				orderGrossProfitEntity.BusinessDivisionId = filter.Filter.BusinessDivisionId;
				orderGrossProfitEntity.PoVatTtl = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["PoVatTtl"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.SystemCalculateDate = string.IsNullOrEmpty(Convert.ToString(data["SystemCalculateDate"])) ? "" : Convert.ToDateTime(data["SystemCalculateDate"]).ToString("yyyy-MM-dd");
				orderGrossProfitEntity.SystemPoVatTtl = Convert.ToString(Decimal.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["SystemPoVatTtl"])) ? 0 : data["SystemPoVatTtl"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.OriginalSystemPoVatTtl = Convert.ToString(Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["SystemPoVatTtl"])) ? 0 : data["SystemPoVatTtl"]));
				orderGrossProfitEntity.Vdr_Namec = Convert.ToString(data["VDR_NAMEC"]);
				orderGrossProfitEntity.SoVatTtl = Convert.ToDecimal(data["SoVatTtl"]).ToString("#0.00");
				orderGrossProfitEntity.TotalPoVatTtl = Convert.ToDecimal(data["TotalPoVatTtl"]).ToString("#0.00");
				orderGrossProfitEntity.VatProfit = Convert.ToDecimal(data["VatProfit"]).ToString("#0.00");
				orderGrossProfitEntity.VatProfitRate = Convert.ToDouble(data["VatProfitRate"] is DBNull ? 0 : data["VatProfitRate"]).ToString("P");
				orderGrossProfitEntity.SystemCalculateProfit = ((Convert.ToDecimal(orderGrossProfitEntity.VatPrice) - Convert.ToDecimal(orderGrossProfitEntity.SystemPoVatTtl)) * Convert.ToDecimal(orderGrossProfitEntity.Count)).ToString("#0.00");
				orderGrossProfitEntity.SystemCalculateProfitRate = Convert.ToDecimal(orderGrossProfitEntity.SoVatTtl) == 0 ? "0" : (Convert.ToDecimal(orderGrossProfitEntity.SystemCalculateProfit) / Convert.ToDecimal(orderGrossProfitEntity.SoVatTtl)).ToString("P");
				costPrice.Add(orderGrossProfitEntity);
			}

			if (!string.IsNullOrWhiteSpace(filter.Filter.VatProfitRateLT) || !string.IsNullOrWhiteSpace(filter.Filter.SystemCalculateProfitRateLT))
			{
				List<OrderGrossProfitAnalysisEntity> result = new List<OrderGrossProfitAnalysisEntity>();
				foreach (var item in costPrice)
				{
					if (!string.IsNullOrWhiteSpace(filter.Filter.VatProfitRateLT) && !string.IsNullOrWhiteSpace(filter.Filter.SystemCalculateProfitRateLT))
					{
						double vatProfitRateLT = Convert.ToDouble(filter.Filter.VatProfitRateLT) / 100;
						double vatProfitRate = double.Parse(item.VatProfitRate.TrimEnd('%')) / 100;
						double systemCalculateProfitRateLT = Convert.ToDouble(filter.Filter.SystemCalculateProfitRateLT) / 100;
						double systemCalculateProfitRate = double.Parse(item.SystemCalculateProfitRate.TrimEnd('%')) / 100;
						if (vatProfitRateLT > vatProfitRate && systemCalculateProfitRateLT > systemCalculateProfitRate)
						{
							result.Add(item);
						}
						continue;
					}
					if (!string.IsNullOrWhiteSpace(filter.Filter.VatProfitRateLT))
					{
						double vatProfitRateLT = Convert.ToDouble(filter.Filter.VatProfitRateLT) / 100;
						double vatProfitRate = double.Parse(item.VatProfitRate.TrimEnd('%')) / 100;
						if (vatProfitRateLT > vatProfitRate)
						{
							result.Add(item);
						}
						continue;
					}
					if (!string.IsNullOrWhiteSpace(filter.Filter.SystemCalculateProfitRateLT))
					{
						double systemCalculateProfitRateLT = Convert.ToDouble(filter.Filter.SystemCalculateProfitRateLT) / 100;
						double systemCalculateProfitRate = double.Parse(item.SystemCalculateProfitRate.TrimEnd('%')) / 100;
						if (systemCalculateProfitRateLT > systemCalculateProfitRate)
						{
							result.Add(item);
						}
					}

				}
				response.Message = "成功";
				response.Data = result;
				response.Code = ResponseCode.Success;
				return response;
			}
			response.Message = "成功";
			response.Data = costPrice;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取销售订单毛利预估明细
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetOrderGrossProfitDetailList(Context ctx, string message)
		{
			List<OrderGrossProfitEntity> costPrice = new List<OrderGrossProfitEntity>();

			PageResqust<OrderGrossProfitFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<OrderGrossProfitFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var localeId = ctx.UserLocale.LCID;

			var pars = new List<SqlParam> { new SqlParam("@LocaleId", KDDbType.Int64, localeId) };
			var sql = $@"/*dialect*/select cust.FDecimalPlacesOfUnitPrice DecimalPlacesOfUnitPrice,supl.FNAME SupplierName,sal.FBILLNO SO_NO,salord.FSEQ SEQ_NO,
                        mhel.FNAME PRODUCT_ENGINEER_NAME,gll.FNAME GROUP_DESC,gl.FNAME TYPE_DESC,mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESCC,
                        salord.FQTY QTY,salf.FTAXPRICE VAT_PRICE,salf.FALLAMOUNT SoVatTtl,sal.FSalesOrderDate SO_DATE,
                        salord.FSupplierUnitPrice PoVatTtl,
                        (salord.FQTY*salord.FSupplierUnitPrice) TotalPoVatTtl,
                        salf.FALLAMOUNT-salord.FQTY*salord.FSupplierUnitPrice VatProfit,
                        (salf.FALLAMOUNT-(salord.FQTY*salord.FSupplierUnitPrice))/nullif(salf.FALLAMOUNT,0) VatProfitRate,el.FDATAVALUE BusinessDivisionName,
						convert(decimal(18,6),0) SystemPoVatTtl,convert(nvarchar(200),'') VDR_NAMEC
						into #t1
                        from T_SAL_ORDERENTRY salord
                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=@LocaleId
                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
                        left join T_BD_MATERIALGROUP_L gll on salord.FPARENTSMALLID = gll.FID and gll.FLOCALEID = @LocaleId
                        left join T_HR_EMPINFO mhe on salord.FPRODUCTMANAGERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
                        left join t_BD_Supplier sup on sup.FSUPPLIERID=salord.FSUPPLIERID
                        left join T_BD_SUPPLIER_L supl on sup.FSUPPLIERID=supl.FSUPPLIERID and supl.FLOCALEID = @LocaleId
                        left join V_BD_SALESMAN sales  on sales.FID=sal.FSalerId
                        left join V_BD_SALESMAN_L salesl on sales.fid=salesl.fid
                        left join T_BAS_ASSISTANTDATAENTRY_L el on salord.FBUSINESSDIVISIONID = el.FENTRYID and el.FLOCALEID = @LocaleId
                        where sal.FDOCUMENTSTATUS='C' and (sal.FCancelStatus<>'B' or
						exists(select top 1 d.FID from T_SAL_DELIVERYNOTICEENTRY d where d.FSRCTYPE='SAL_SaleOrder' and d.FSRCBILLNO=sal.FBILLNO))  ";//只取有效订单

			if (filter.Filter.IsInsideCust != null)
			{
				if (filter.Filter.IsInsideCust.Value)
				{
					sql += " and cust.FCorrespondOrgId>0 ";
				}
				else
				{
					sql += " and cust.FCorrespondOrgId=0 ";
				}
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SoNo))
			{
				sql += $@" and sal.FBILLNO = '{filter.Filter.SoNo}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductManager))
			{
				sql += $@" and mhel.FNAME = '{filter.Filter.ProductManager}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ItemNo))
			{
				sql += $@" and mat.FNUMBER like '%{filter.Filter.ItemNo}%'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
			{
				sql += $@" and salord.FSMALLID in ('{filter.Filter.SmallClassFilter}')";
			}
			if (filter.Filter.OrderStartDate != null)
			{
				sql += $@" and sal.FSalesOrderDate >= '{filter.Filter.OrderStartDate}'";
			}
			if (filter.Filter.OrderEndDate != null)
			{
				sql += $@" and sal.FSalesOrderDate <= '{filter.Filter.OrderEndDate.Value.ToString("yyyy-MM-dd 23:59:59")}'";
			}
			//如果汇总依据选择了大类且大类为空
			if (filter.Filter.IsJump && !string.IsNullOrWhiteSpace(filter.Filter.SummaryBasis)
				&& filter.Filter.SummaryBasis.Contains("ParentSmallName") && string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				sql += $@" and salord.FPARENTSMALLID=0";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				sql += $@" and salord.FPARENTSMALLID = '{filter.Filter.ParentSmallName}'";
			}
			//如果汇总依据选择了小类且小类为空
			if (filter.Filter.IsJump && !string.IsNullOrWhiteSpace(filter.Filter.SummaryBasis)
				&& filter.Filter.SummaryBasis.Contains("SmallClass") && string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				sql += $@" and salord.FSMALLID=0";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				sql += $@" and salord.FSMALLID='{filter.Filter.SmallClass}'";
			}
			if (filter.Filter.IsJump && !string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
			{
				sql += $@" and matL.FNAME = '{filter.Filter.ItemDesc}'";
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
				{
					sql += $@" and matL.FNAME like '%{filter.Filter.ItemDesc}%'";
				}
			}
			//改成查询事业部
			if (!string.IsNullOrWhiteSpace(filter.Filter.BusinessDivisionId))
			{
				sql += $@" and salord.FBUSINESSDIVISIONID='{filter.Filter.BusinessDivisionId}' ";
			}

			//更新数据
			sql += @"  --更新组装拆卸单
						update t1 set SystemPoVatTtl=t2.SystemPoVatTtl,VDR_NAMEC=t2.FBILLNO  from #t1 t1,(
						select t1.FBILLNO,t1.GDItemNo,((lysPrice+FEE)/FQTY) SystemPoVatTtl from (
						select ROW_NUMBER() OVER (PARTITION BY dmat.FNUMBER ORDER BY ly.FAPPROVEDATE desc) AS rid,ly.FID,lyp.FENTRYID,FBILLNO,dmat.FNUMBER GDItemNo,lyp.FEE,lyp.FQTY,ly.FDATE from T_STK_ASSEMBLY ly
												inner join T_STK_ASSEMBLYPRODUCT lyp on lyp.FID=ly.FID
												inner join T_BD_MATERIAL dmat on lyp.FMATERIALID=dmat.FMATERIALID
												where ly.FAffairType='Assembly' and ly.FDOCUMENTSTATUS='C'
						) t1 
						inner join (select lys.FENTRYID,SUM(lys.FQTY*lysPo.VatPrice) lysPrice  from T_STK_ASSEMBLYSUBITEM lys
						inner join T_BD_MATERIAL lysMat on lys.FMATERIALID=lysMat.FMATERIALID
						inner join (select ROW_NUMBER() OVER (PARTITION BY dmat.FNUMBER ORDER BY m.FDATE desc) AS rid,dmat.FNUMBER PoItemNo,f.FTAXPRICE VatPrice from t_PUR_POOrderEntry d
												inner join t_PUR_POOrder m on m.FID=d.FID
												inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
												left join  T_BD_MATERIAL dmat on d.FMATERIALID=dmat.FMATERIALID
												left join t_BD_Supplier sup on sup.FSUPPLIERID=m.FSUPPLIERID and  sup.FDocumentStatus='C' and sup.FForbidStatus='A'
						where m.FDOCUMENTSTATUS='C' and  m.FCANCELSTATUS='A' and f.FTAXPRICE!= 0 and m.FBillTypeID='83d822ca3e374b4ab01e5dd46a0062bd' and sup.FCorrespondOrgId=0) lysPo on lysPo.rid=1 and lysPo.PoItemNo=lysMat.FNUMBER
						group by lys.FENTRYID) lys on lys.FENTRYID=t1.FENTRYID
						where t1.rid=1
						) t2
						where t1.ITEM_NO=t2.GDItemNo  and t1.SystemPoVatTtl=0 ";


			//是否异常数据
			if (filter.Filter.HasExcept != null)
			{
				if (filter.Filter.HasExcept.Value)
				{
					sql += "/*dialect*/select * from #t1 a where a.PoVatTtl = 0 or a.PoVatTtl < a.SystemPoVatTtl ";
				}
				else
				{
					sql += "/*dialect*/select * from #t1 a where a.PoVatTtl<>0 and a.PoVatTtl >= a.SystemPoVatTtl ";
				}
			}
			else
			{
				sql += "/*dialect*/select * from #t1";
			}
			sql += $@" order by PRODUCT_ENGINEER_NAME,GROUP_DESC,TYPE_DESC,ITEM_DESCC  drop table #t1  ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			foreach (var data in datas)
			{
				OrderGrossProfitEntity orderGrossProfitEntity = new OrderGrossProfitEntity();
				orderGrossProfitEntity.SoNo = Convert.ToString(data["SO_NO"]);
				orderGrossProfitEntity.SeqNo = Convert.ToString(data["SEQ_NO"]);
				orderGrossProfitEntity.SupplierName = Convert.ToString(data["SupplierName"]);
				orderGrossProfitEntity.SoVatTtl = Convert.ToDecimal(data["SoVatTtl"]).ToString("#0.00");
				orderGrossProfitEntity.PoVatTtl = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["PoVatTtl"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.VatProfit = Convert.ToDecimal(data["VatProfit"]).ToString("#0.00");
				orderGrossProfitEntity.VatProfitRate = Convert.ToDouble(data["VatProfitRate"] is DBNull ? 0 : data["VatProfitRate"]).ToString("P");
				orderGrossProfitEntity.SoDate = data["SO_DATE"] is DBNull ? "" : Convert.ToDateTime(data["SO_DATE"]).ToString("yyyy-MM-dd HH:mm:ss");
				orderGrossProfitEntity.ProductManager = Convert.ToString(data["PRODUCT_ENGINEER_NAME"]);
				orderGrossProfitEntity.ParentSmallName = Convert.ToString(data["GROUP_DESC"]);
				orderGrossProfitEntity.ItemNo = Convert.ToString(data["ITEM_NO"]);
				orderGrossProfitEntity.ItemDesc = Convert.ToString(data["ITEM_DESCC"]);
				orderGrossProfitEntity.SmallClass = Convert.ToString(data["TYPE_DESC"]);
				orderGrossProfitEntity.UnitPrice = Convert.ToString(Decimal.Round(Convert.ToDecimal(data["VAT_PRICE"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.ProductNum = Convert.ToDecimal(data["QTY"]).ToString("#0.00");
				orderGrossProfitEntity.TotalPoVatTtl = Convert.ToDecimal(data["TotalPoVatTtl"]).ToString("#0.00");
				orderGrossProfitEntity.CompanyCode = filter.Filter.CompanyCode;
				orderGrossProfitEntity.CompanyName = filter.Filter.CompanyName;
				orderGrossProfitEntity.BusinessDivisionId = filter.Filter.BusinessDivisionId;
				orderGrossProfitEntity.BusinessDivisionName = Convert.ToString(data["BusinessDivisionName"]);
				orderGrossProfitEntity.SystemPoVatTtl = Convert.ToString(Decimal.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(data["SystemPoVatTtl"].ToString()) ? 0 : data["SystemPoVatTtl"]), Convert.ToInt32(data["DecimalPlacesOfUnitPrice"])));
				orderGrossProfitEntity.Vdr_Namec = Convert.ToString(data["VDR_NAMEC"]);
				costPrice.Add(orderGrossProfitEntity);
			}
			response.Message = "成功";
			response.Data = costPrice;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取产品毛利汇总数据
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetProductGrossList(Context ctx, string message)
		{
			List<ProductGrossEntity> productGrossEntities = new List<ProductGrossEntity>();

			PageResqust<OrderGrossProfitFilter> filter = JsonConvertUtils.DeserializeObject<PageResqust<OrderGrossProfitFilter>>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var localeId = ctx.UserLocale.LCID;

			var pars = new List<SqlParam> { new SqlParam("@LocaleId", KDDbType.Int64, localeId) };
			var sql = $@"/*dialect*/select
                        sum(t.SoVatTtl) SoVatTtl
                        ,sum(t.TotalPoVatTtl) TotalPoVatTtl
                        ,sum(t.SoVatTtl)-sum(t.TotalPoVatTtl) VatProfit
                        , (sum(t.SoVatTtl)-sum(t.TotalPoVatTtl))/nullif(sum(t.SoVatTtl),0) VatProfitRate ,t.ITEM_GRP,t.GROUP_DESC,t.ITEM_TYPE,t.TYPE_DESC,t.month from 
                        (
                        select convert(varchar(7),sal.FSalesOrderDate,120) month,gll.FNAME GROUP_DESC,salord.FPARENTSMALLID ITEM_GRP,gl.FNAME TYPE_DESC,salord.FSMALLID ITEM_TYPE,
                        salf.FTAXPRICE*(case when sal.FCLOSESTATUS ='B' then salr.FStockOutQty else salord.FQTY end) as SoVatTtl,
                        salord.FAdjustPrice*(case when sal.FCLOSESTATUS ='B' then salr.FStockOutQty else salord.FQTY end) as TotalPoVatTtl,
                        (salf.FTAXPRICE*(case when sal.FCLOSESTATUS ='B' then salr.FStockOutQty else salord.FQTY end))-(salord.FAdjustPrice*(case when sal.FCLOSESTATUS ='B' then salr.FStockOutQty else salord.FQTY end)) VatProfit,
                        el.FDATAVALUE BusinessDivisionName
                        from T_SAL_ORDERENTRY salord
                        inner join T_SAL_ORDER sal on sal.FID=salord.FID
                        inner join T_SAL_ORDERENTRY_F salf on salord.FENTRYID = salf.FENTRYID
                        inner join T_SAL_ORDERENTRY_D sald on salord.FENTRYID=sald.FENTRYID
                        inner join T_SAL_ORDERENTRY_R salr on salord.FENTRYID=salr.FENTRYID
                        inner join T_BD_CUSTOMER  cust on cust.FCUSTID=sal.FCUSTID
                        inner join T_BD_CUSTOMER_L  custl on cust.FCUSTID=custl.FCUSTID and custl.FLOCALEID=@LocaleId
                        inner join T_BD_MATERIAL mat on salord.FMATERIALID=mat.FMATERIALID
                        inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @LocaleId
                        left join T_BD_MATERIALGROUP_L gl on salord.FSMALLID = gl.FID and gl.FLOCALEID = @LocaleId
                        left join T_BD_MATERIALGROUP_L gll on salord.FPARENTSMALLID = gll.FID and gll.FLOCALEID = @LocaleId
                        left join T_HR_EMPINFO mhe on salord.FPRODUCTENGINEERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID and mhel.FLOCALEID = @LocaleId
                        left join T_BAS_ASSISTANTDATAENTRY_L el on salord.FBUSINESSDIVISIONID = el.FENTRYID and el.FLOCALEID = @LocaleId
                        where sal.FDOCUMENTSTATUS='C' and salord.FAdjustPrice>0 ";
			if (!string.IsNullOrWhiteSpace(filter.Filter.ProductManager))
			{
				sql += $@" and mhel.FNAME = '{filter.Filter.ProductManager}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallName))
			{
				sql += $@" and salord.FPARENTSMALLID = '{filter.Filter.ParentSmallName}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClass))
			{
				sql += $@" and salord.FSMALLID='{filter.Filter.SmallClass}'";
			}
			if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
			{
				sql += $@" and salord.FSMALLID in ('{filter.Filter.SmallClassFilter}')";
			}
			if (filter.Filter.Year > 0)
			{
				sql += $@" and YEAR(sal.FSalesOrderDate)={filter.Filter.Year} ";
			}
			if (filter.Filter.Month > 0 && filter.Filter.Month < 13)
			{
				sql += $@" and Month(sal.FSalesOrderDate)={filter.Filter.Month} ";
			}
			////改成查询事业部
			//if (!string.IsNullOrWhiteSpace(filter.Filter.BusinessDivisionId))
			//{
			//    sql += $@" and salord.FBUSINESSDIVISIONID='{filter.Filter.BusinessDivisionId}' ";
			//}
			if (filter.Filter.IsVatPriceZero)
			{
				var sql1 = sql;
				var sql2 = sql;
				sql1 += $@" and salf.FTAXPRICE = 0 and sal.FCLOSESTATUS ='A'
                            ) as t  
                            group by t.ITEM_GRP,t.GROUP_DESC, t.TYPE_DESC,t.ITEM_TYPE,t.month  
                            union all ";
				sql2 += $@" and salf.FTAXPRICE <> 0
                            ) as t  
                            group by t.ITEM_GRP,t.GROUP_DESC, t.TYPE_DESC,t.ITEM_TYPE,t.month  ";
				sql = sql1 + sql2;
			}
			else
			{
				sql += $@" ) as t  
                        group by  t.ITEM_GRP,t.GROUP_DESC, t.TYPE_DESC,t.ITEM_TYPE,t.month   ";
			}
			sql += $@" order by t.month desc,t.TYPE_DESC,t.GROUP_DESC asc ";



			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			foreach (var data in datas)
			{
				ProductGrossEntity productGrossEntity = new ProductGrossEntity();
				productGrossEntity.ParentSmallId = Convert.ToString(data["ITEM_GRP"]);
				productGrossEntity.ParentSmallName = Convert.ToString(data["GROUP_DESC"]);
				productGrossEntity.SmallClass = Convert.ToString(data["TYPE_DESC"]);
				productGrossEntity.SmallClassId = Convert.ToString(data["ITEM_TYPE"]);
				productGrossEntity.SoVatTtl = data["SoVatTtl"] is DBNull ? "" : Convert.ToDecimal(data["SoVatTtl"]).ToString("#0.00");
				productGrossEntity.TotalPoVatTtl = data["TotalPoVatTtl"] is DBNull ? "" : Convert.ToDecimal(data["TotalPoVatTtl"]).ToString("#0.00");
				productGrossEntity.VatProfit = data["VatProfit"] is DBNull ? "" : Convert.ToDecimal(data["VatProfit"]).ToString("#0.00");
				productGrossEntity.VatProfitRate = data["VatProfitRate"] is DBNull ? "--" : Convert.ToDouble(data["VatProfitRate"]).ToString("P");
				productGrossEntity.CompanyName = filter.Filter.CompanyName;
				//productGrossEntity.BusinessDivisionName = Convert.ToString(data["BusinessDivisionName"]);
				productGrossEntity.Month = Convert.ToString(data["month"]);
				productGrossEntities.Add(productGrossEntity);
			}
			response.Message = "成功";
			response.Data = productGrossEntities;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 更新发货通知单物流信息
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpDnTracking(Context ctx, string message)
		{

			UpDnTrackingInfo filter = JsonConvertUtils.DeserializeObject<UpDnTrackingInfo>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				if (filter.BillNo.Count > 0)
				{
					var sql = $@"/*dialect*/update T_SAL_DELIVERYNOTICE set FTrackingNumber='{filter.TrackingNumber}',FTrackingName='{filter.TrackingName}',FTrackingDate='{filter.TrackingDate}',FTrackingUser='{filter.TrackingUser}' where FBILLNO in ('{string.Join("','", filter.BillNo)}') ";
					DBServiceHelper.Execute(ctx, sql);
					sql = $@"/*dialect*/update T_SAL_OUTSTOCK set FTrackingNumber='{filter.TrackingNumber}',FTrackingName='{filter.TrackingName}',FTrackingDate='{filter.TrackingDate}',FTrackingUser='{filter.TrackingUser}' where FBILLNO in ('{string.Join("','", filter.BillNo)}') ";
					DBServiceHelper.Execute(ctx, sql);
				}
				response.Message = "更新成功";
				response.Code = ResponseCode.Success;

			}
			catch (Exception ex)
			{

				response.Message = "更新失败：" + ex.Message;
				response.Code = ResponseCode.Exception;
			}
			return response;
		}

		/// <summary>
		/// 根据销售订单号获取物流信息
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetSoLogisticsInfo(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			if (string.IsNullOrWhiteSpace(message))
			{
				response.Message = "销售订单号不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			List<SoLogisticsInfoEntity> entityList = new List<SoLogisticsInfoEntity>();
			var sql = $@"/*dialect*/select distinct t1.FTRACKINGNUMBER,t1.FTRACKINGNAME from T_SAL_DELIVERYNOTICE t1
                            inner join T_SAL_DELIVERYNOTICEENTRY t2 on t1.FID=t2.FID
                            inner join T_SAL_DELIVERYNOTICEENTRY_LK t3 on t2.FENTRYID=t3.FENTRYID
                            inner join T_SAL_ORDER t4 on t4.FID=t3.FSBILLID
                            where t1.FDOCUMENTSTATUS='C' and t4.FBILLNO='{message}' and isnull(t1.FTRACKINGNUMBER,'')<>'' ";

			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new SoLogisticsInfoEntity
				{
					TrackingNumber = Convert.ToString(data["FTRACKINGNUMBER"]),
					TrackingName = Convert.ToString(data["FTRACKINGNAME"])
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 更新物流方式变更申请审批信息
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpDnLogisticsChangesInfo(Context ctx, string message)
		{

			ApprovalMessageRequest entity = JsonConvertUtils.DeserializeObject<ApprovalMessageRequest>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{

				var sql = $@"/*dialect*/update T_SAL_DnLogisticsChanges set FSpStatus={entity.SpStatus},FCompleteTime='{entity.ApprovalDate}',FAduitUserName='{entity.AduitUserName}' where FApprovalNo='{entity.ApplyeventNo}' and FSpStatus=1 ";
				DBServiceHelper.Execute(ctx, sql);
				response.Message = "更新成功";
				response.Code = ResponseCode.Success;
			}
			catch (Exception ex)
			{
				response.Message = "更新失败：" + ex.Message;
				response.Code = ResponseCode.Exception;
			}
			return response;
		}

		/// <summary>
		/// 更新加急发货申请审批信息
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpDnUrgentShipmentState(Context ctx, string message)
		{
			ApprovalMessageRequest entity = JsonConvertUtils.DeserializeObject<ApprovalMessageRequest>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				List<string> strList = new List<string>();
				//更新审批状态
				var sql = $@"/*dialect*/update T_SAL_DnUrgentShipment set FSpStatus={entity.SpStatus},FCompleteTime='{entity.ApprovalDate}',FAduitUserName='{entity.AduitUserName}' where FApprovalNo='{entity.ApplyeventNo}' and FSpStatus=1 ";
				DBServiceHelper.Execute(ctx, sql);
				//审批通过
				if (entity.SpStatus == "2")
				{
					//获取送货单单号
					sql = $@"/*dialect*/select FDnNo from T_SAL_DnUrgentShipment where FApprovalNo='{entity.ApplyeventNo}' ";
					var dnNos = DBServiceHelper.ExecuteScalar<string>(ctx, sql, "");
					foreach (var item in dnNos.Split('、'))
					{
						strList.Add(item);
					}
					//更新发货通知单
					sql = $@"/*dialect*/update T_SAL_DELIVERYNOTICE set FIsUrgentShipment='1' where FBILLNO in ('{dnNos.Replace("、", "','")}') ";
					DBServiceHelper.Execute(ctx, sql);
					//更新云存储
					var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, "/api/goods/changeDnUrgentStatus", JsonConvertUtils.SerializeObject(strList));
					var returnInfo = JsonConvertUtils.DeserializeObject<ResponseCloudWarehouseMessage>(cwResult);
					if (!returnInfo.IsSuccess)
					{
						response.Code = ResponseCode.ThirdpartyError;
						response.Message = returnInfo.Message;
						return response;
					}
				}
				response.Message = "更新成功";
				response.Code = ResponseCode.Success;
			}
			catch (Exception ex)
			{
				response.Message = "更新失败：" + ex.Message;
				response.Code = ResponseCode.Exception;
			}
			return response;
		}

		public ResponseMessage<dynamic> CreateDeliveryNotice(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			//string sSql = $@"SELECT * FROM dbo.T_SAL_DELIVERYNOTICE t1
			//WHERE t1.FDOCUMENTSTATUS='C' and NOT EXISTS(SELECT 1 FROM T_STK_STKTRANSFEROUTENTRY WHERE FPENYDELIVERYNOTICE=t1.FBILLNO)";
			//foreach (var entitem in item["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
			//{
			//    var entryid = Convert.ToInt64(entitem["Id"]);
			//    //判断是否为外发仓
			//    DynamicObject material = entitem["MaterialID"] as DynamicObject;
			//    DynamicObject ckid = entitem["StockID"] as DynamicObject;
			//    var stOrgID = Convert.ToInt64(entitem["FSupplyTargetOrgId_Id"]);
			//    decimal delqty = Convert.ToDecimal(entitem["Qty"]);
			//    //广东蚂蚁
			//    if (detOrgID == 669144)
			//    {
			//        isGDMYZZ = true;
			//    }

			//    var srcbillid = "";
			//    var srcrowid = "";
			//    foreach (var itemlink in entitem["FEntity_Link"] as DynamicObjectCollection)
			//    {
			//        srcbillid = itemlink["SBillId"] as string;
			//        srcrowid = itemlink["SId"] as string;
			//    }
			//    //没有上游单据的不校验
			//    if (srcrowid.IsNullOrEmptyOrWhiteSpace()) return;

			//    //查询组织间需求单
			//    string sSql = $@"SELECT t1.FID,t1.FSUPPLYINTERID,t1.FSUPPLYORGID,t1.FBASEQTY,t1.FMATERIALID FROM T_PLN_RESERVELINKENTRY t1
			//                        LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
			//                        WHERE t2.FDEMANDINTERID={srcbillid} AND t2.FDEMANDENTRYID={srcrowid} 
			//                        AND t1.FSUPPLYFORMID='PLN_REQUIREMENTORDER' AND t1.FBASEQTY>0";
			//    var RequirementorderDatas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
			//    foreach (var reitem in RequirementorderDatas)
			//    {
			//        sSql = $@"SELECT SUM(FBASEQTY) as FBASEQTY
			//                    FROM T_PLN_RESERVELINKENTRY t1
			//                    INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
			//                    WHERE t1.FSUPPLYFORMID='STK_Inventory' AND t1.FSUPPLYORGID<>224428 
			//                    AND t2.FDEMANDINTERID={reitem["FSUPPLYINTERID"]}";
			//        var Allocateitems = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
			//        if (delqty <= 0)
			//        {
			//            continue;
			//        }
			//        decimal sqty = 0;
			//        foreach (var allitems in Allocateitems)
			//        {
			//            var baseqty = Convert.ToDecimal(reitem["FBASEQTY"]);
			//            if (delqty - baseqty > 0)
			//            {
			//                sqty = baseqty;
			//                delqty -= baseqty;
			//            }
			//            else
			//            {
			//                sqty = delqty;
			//            }
			//            //获取调出仓库
			//            BusinessInfo businessInfo = this.BusinessInfo;
			//            BaseDataField bdField = businessInfo.GetField("FStockID") as BaseDataField;
			//            QueryBuilderParemeter p = new QueryBuilderParemeter();
			//            p.FormId = "BD_STOCK";
			//            p.SelectItems = SelectorItemInfo.CreateItems("FStockId");
			//            p.FilterClauseWihtKey = $"FStockId = {Convert.ToInt64(allitems["FSTOCKID"])}";
			//            var src_ck = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p)[0];

			//            allocates.Add(new Allocate
			//            {
			//                DeliveryNoticeNumber = billno,
			//                FID = reitem["FSUPPLYINTERID"].ToString(),
			//                TargetOrgId = reitem["FSUPPLYORGID"].ToString(),
			//                //FDestMaterialID = src_material,
			//                FMaterialId = Convert.ToInt64(material["msterID"]),
			//                FQTY = sqty,
			//                FSrcStock_Id = Convert.ToInt64(src_ck["Id"]),
			//                FSrcStockId = src_ck,
			//                FDestStock_Id = Convert.ToInt64(ckid["Id"]),
			//                FDestStockId = ckid,
			//            });

			//        }
			//    }
			//}

			////foreach (var pushlist in allocates)
			////{
			////组织间需求单下推分步式调出单
			//if (allocates.Count > 0)
			//{
			//    var opresult = SalDeliveryNoticePushAllocate(this.Context, allocates);
			//    if (opresult.IsSuccess)
			//    {
			//        var createUser = Convert.ToInt64(item["CreatorId_Id"]);
			//        //创建人微信Code
			//        var cUserWxCode = GetUserWxCode(this.Context, createUser);

			//        var sContent = opresult.SuccessDataEnity.Select(p => p["BillNo"].ToString());
			//        if (!string.IsNullOrWhiteSpace(cUserWxCode))
			//        {
			//           SendTextMessageUtils.SendTextMessage(cUserWxCode, "您的发货通知已生成相关调拨单请查阅：" + string.Join(",", sContent));
			//        }
			//    }
			//}
			return response;
		}

		public void CreateSaleBillReserveLink(Context ctx, long reqid, List<resEntryInfo> resinfo)
		{
			//取组织间需求单信息
			string sSql = $@"SELECT FFORMID AS DemandFormID,fid AS DemandInterID,FBILLNO AS DemandBillNO
            ,'SAL_SaleOrder' AS SrcDemandFormId,FSALEORDERID AS SrcDemandInterId,FSALEORDERENTRYID AS SrcDemandEntryId,FSALEORDERNO AS SrcDemandBillNo
            ,FSUPPLYORGID AS DemandOrgID
            ,FSUPPLYMATERIALID AS MaterialID
            ,FUNITID AS BaseUnitID 
            FROM T_PLN_REQUIREMENTORDER WHERE FID={reqid}";
			var salbilldata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
			DemandView demandView = new DemandView();
			string demaInterid = "0";
			if (salbilldata.Count > 0)
			{
				demandView.DemandFormID = salbilldata[0]["DemandFormID"].ToString();
				demandView.DemandInterID = salbilldata[0]["DemandInterID"].ToString();
				demaInterid = salbilldata[0]["DemandInterID"].ToString();

				demandView.DemandEntryID = "";
				demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

				demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
				demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
				demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
				demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

				demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
				demandView.DemandDate = System.DateTime.Now;
				demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
				demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
				//demandView.BaseQty = resinfo.BaseActSupplyQty;
				//取即时库存
				List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
				foreach (var item in resinfo)
				{
					SupplyViewItem subRowView = new SupplyViewItem();
					subRowView.SupplyFormID_Id = item.SupplyFormID;
					subRowView.SupplyInterID = item.SupplyInterID;
					subRowView.SupplyEntryId = item.SupplyEntryID;
					subRowView.SupplyBillNO = item.SupplyBillNO;

					subRowView.SupplyMaterialID_Id = Convert.ToInt64(item.SupplyMaterialID);
					subRowView.SupplyOrgID_Id = Convert.ToInt64(item.SupplyOrgID);
					subRowView.SupplyDate = System.DateTime.Now;
					subRowView.SupplyStockID_Id = Convert.ToInt64(item.SupplyStockID);
					subRowView.SupplyAuxproID_Id = 0;
					subRowView.BaseSupplyUnitID_Id = Convert.ToInt64(item.BaseSupplyUnitID);
					//供应数量
					subRowView.BaseActSupplyQty = item.BaseSupplyQty;
					subRowView.IntsupplyID = Convert.ToInt64(item.IntsupplyID);
					subRowView.IntsupplyEntryId = Convert.ToInt64(item.IntsupplyEntryID);

					subRowView.Supplypriority = item.Supplypriority;
					subRowView.Ismto = item.Ismto;
					subRowView.YieldRate = item.YieldRate;
					subRowView.Linktype = item.Linktype;
					subRowView.EntryPkId = item.EntryPkId;
					subRowView.Consumpriority = item.Consumpriority;
					subRowView.GenerateId = item.GenerateId;

					viewItems.Add(subRowView);
				}

				demandView.supplyView = viewItems;
			}


			//创建转移行信息
			List<ReserveLinkSelectRow> lstConvertInfo = CreateReserveRow(demandView);
			//构建预留转移参数
			ReserveArgs<ReserveLinkSelectRow> convertArgs = new ReserveArgs<ReserveLinkSelectRow>();
			//把预留转移行的信息赋给参数
			convertArgs.SelectRows = lstConvertInfo;
			//创建预留服务
			IReserveLinkService linkService = AppServiceContext.GetService<IReserveLinkService>();
			//调用预留创建接口
			var reserveOperationResult = linkService.ReserveLinkCreate(ctx, convertArgs, OperateOption.Create());
		}
		///预留关系供需行      
		private List<ReserveLinkSelectRow> CreateReserveRow(DemandView demandView)
		{
			List<ReserveLinkSelectRow> linkRows = new List<ReserveLinkSelectRow>();
			//创建表头需求信息
			ReserveLinkSelectRow seleRow = new ReserveLinkSelectRow();
			//销售订单信息
			seleRow.DemandRow.DemandInfo.FormID = demandView.DemandFormID;
			seleRow.DemandRow.DemandInfo.InterID = demandView.DemandInterID;
			seleRow.DemandRow.DemandInfo.EntryID = demandView.DemandEntryID; ;
			seleRow.DemandRow.DemandInfo.BillNo = demandView.DemandBillNO;
			//销售订单信息
			seleRow.DemandRow.SrcDemandInfo.FormID = demandView.SrcDemandFormId;
			seleRow.DemandRow.SrcDemandInfo.InterID = demandView.SrcDemandInterId;
			seleRow.DemandRow.SrcDemandInfo.EntryID = demandView.SrcDemandEntryId;
			seleRow.DemandRow.SrcDemandInfo.BillNo = demandView.SrcDemandBillNo;
			seleRow.DemandRow.ReserveType = ((int)Enums.PLN_ReserveModel.Enu_ReserveType.KdStrong).ToString();
			//需求组织
			seleRow.DemandRow.DemandOrgID = demandView.DemandOrgID_Id;
			//需求日期
			seleRow.DemandRow.DemandDate = demandView.DemandDate;
			//物料内码
			seleRow.DemandRow.DemandMaterialID = demandView.MaterialID_Id;

			//基本单位内码
			seleRow.DemandRow.BaseUnitId = demandView.BaseUnitID_Id;
			//基本单位需求数量(基本单据数量-己出库的数量)
			seleRow.DemandRow.BaseDemandQty = demandView.BaseQty;

			//添加供应信息
			List<ReserveLinkSupplyRow> supplyRows = this.GetLinkSupplyRow(demandView.supplyView);
			seleRow.SupplyRows.AddRange(supplyRows);


			linkRows.Add(seleRow);
			return linkRows;
		}
		//创建供应行
		private List<ReserveLinkSupplyRow> GetLinkSupplyRow(List<SupplyViewItem> supplyView)
		{
			List<ReserveLinkSupplyRow> supplyRows = new List<ReserveLinkSupplyRow>();
			foreach (SupplyViewItem subRowView in supplyView)
			{
				ReserveLinkSupplyRow row = new ReserveLinkSupplyRow();
				//供应单据信息，单据标识，内码
				row.SupplyBillInfo.FormID = subRowView.SupplyFormID_Id;
				row.SupplyBillInfo.InterID = subRowView.SupplyInterID;
				row.SupplyBillInfo.EntryID = subRowView.SupplyEntryId;
				row.SupplyBillInfo.BillNo = subRowView.SupplyBillNO;
				row.SupplyMaterialID = subRowView.SupplyMaterialID_Id;
				row.SupplyOrgID = subRowView.SupplyOrgID_Id;
				row.SupplyDate = subRowView.SupplyDate;
				row.SupplyStockID = subRowView.SupplyStockID_Id;
				//row.SupplyStockLocID = subRowView.SupplyStockLocID_Id;

				row.SupplyAuxpropID = subRowView.SupplyAuxproID_Id;
				row.BaseSupplyUnitID = subRowView.BaseSupplyUnitID_Id;
				//供应数量
				row.BaseSupplyQty = subRowView.BaseActSupplyQty;
				row.LinkType = Enums.PLN_ReserveModel.Enu_ReserveBuildType.KdByManual;
				row.SupplyPriority = subRowView.Supplypriority;
				row.YieldRate = subRowView.YieldRate;
				row.SubItemEntryId = subRowView.EntryPkId;
				supplyRows.Add(row);
			}
			return supplyRows;
		}

		/// <summary>
		/// MES下推生成销售出库单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> MesGenerateOutStock(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrWhiteSpace(message))
			{
				response.Message = "参数信息不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			MesGenerateOutStockRequest data = JsonConvertUtils.DeserializeObject<MesGenerateOutStockRequest>(message);
			return SalesOrderServiceHelper.MesGenerateOutStockService(ctx, data);
		}

        /// <summary>
        /// MES下推生成销售退货单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesGenerateReturnStock(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            MesGenerateReturnStockRequest data = JsonConvertUtils.DeserializeObject<MesGenerateReturnStockRequest>(message);
            if (string.IsNullOrWhiteSpace(data.RetBillNo))
            {
                response.Message = "退货单据编号不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return SalesOrderServiceHelper.MesGenerateReturnStockService(ctx, data);
        }

        public ResponseMessage<AfterSalesResponse> ChangingOrRefunding(Context ctx, string data)
        {
            ResponseMessage<AfterSalesResponse> response = new ResponseMessage<AfterSalesResponse>() { Code = ResponseCode.Success };
            if (string.IsNullOrWhiteSpace(data))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.Empty;
                return response;
            }
            AfterSalesRequest request = JsonConvertUtils.DeserializeObject<AfterSalesRequest>(data);
            if (string.IsNullOrWhiteSpace(request.ThirdOrderId))
            {
                response.Message = "销售订单号不能为空";
                response.Code = ResponseCode.Empty;
                return response;
            }

            if (request.Skus == null || request.Skus.Count == 0)
            {
                response.Message = "Skus信息不能为空";
                response.Code = ResponseCode.Empty;
                return response;
            }
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            // 销售出库单下推退货通知单
            var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_OUTSTOCK", "SAL_RETURNNOTICE");
            var rule = rules.FirstOrDefault(t => t.IsDefault) ?? throw new Exception("没有从销售出库单下推退货通知单的转换关系");

            var sql = @"/*dialect*/ select o.FID,e.FENTRYID,m.FNUMBER,ef.FSALBASEQTY-er.FBaseReturnQty FQty
from T_SAL_OUTSTOCK o
	inner join T_SAL_OUTSTOCKENTRY e on o.FID = e.FID
	inner join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	inner join T_SAL_OUTSTOCKENTRY_F ef on e.FENTRYID = ef.FENTRYID
	inner join T_SAL_OUTSTOCKENTRY_R er on e.FENTRYID = er.FENTRYID
where er.FSoorDerno =  @OrderNo and o.FDOCUMENTSTATUS = 'C' and o.FCANCELSTATUS = 'A' and ef.FSALBASEQTY > er.FBaseReturnQty ";
            var sqlParams = new SqlParam[]
            {
                new SqlParam("@OrderNo",KDDbType.String, request.ThirdOrderId)
            };
            DynamicObjectCollection saleEntitys = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: sqlParams);
            foreach (var sku in request.Skus)
            {
                var outstocks = saleEntitys.Where(x => sku.SkuId.EqualsIgnoreCase(Convert.ToString(x["FNUMBER"])));
                var qty = sku.SkuNum;
                foreach (var outstock in outstocks)
                {
                    if (qty > 0)
                    {
                        selectedRows.Add(new ListSelectedRow(outstock["FID"].ToString(), outstock["FENTRYID"].ToString(), 0, "SAL_OUTSTOCK") { EntryEntityKey = "FEntity" });
                        qty -= Convert.ToInt32(outstock["FQty"]);
                    }
                }

                if (qty > 0)
                {
                    response.Message = $"Sku:{sku.SkuId}数量不满足售后申请!";
                    response.Code = ResponseCode.UpperLimit;
                    return response;
                }
            }

            //有数据才需要下推
            if (selectedRows.Count > 0)
            {
                PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                {
                    TargetBillTypeId = "",     // 请设定目标单据单据类型
                    TargetOrgId = 0,            // 请设定目标单据主业务组织
                };
                //执行下推操作，并获取下推结果
                var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                if (operationResult.IsSuccess)
                {
                    var returnView = FormMetadataUtils.CreateBillView(ctx, "SAL_RETURNNOTICE");
                    var entity = returnView.BusinessInfo.GetEntity("FEntity");
                    returnView.Model.DataObject = operationResult.TargetDataEntities.First().DataEntity;
                    var entryDatas = returnView.Model.GetEntityDataObject(entity);
                    returnView.Model.SetItemValueByNumber("FPENYRmType", request.AfsType == 1 ? "THLX01_SYS" : "THLX02_SYS", 0);
                    returnView.Model.SetItemValueByNumber("FReturnReason", "BX", 0);
                    //修改数量
                    foreach (var sku in request.Skus)
                    {
                        var outstocks = entryDatas.Where(x => sku.SkuId.EqualsIgnoreCase(Convert.ToString((x["MaterialId"] as DynamicObject)["Number"])));
                        var qty = sku.SkuNum;
                        foreach (var outstock in outstocks)
                        {
                            var index = entryDatas.IndexOf(outstock);
                            var returnQty = Convert.ToInt32(outstock["Qty"]);
                            returnView.Model.SetValue("FEntryDescription", request.Reason, index);
                            if (qty >= returnQty)
                            {
                                qty -= returnQty;
                            }
                            else
                            {
                                returnView.Model.SetValue("FQty", sku.SkuNum, index);
                                returnView.InvokeFieldUpdateService("FQty", index);
                                qty = 0;
                                break;
                            }
                        }
                        if (qty > 0)
                        {
                            response.Message = $"Sku:{sku.SkuId}数量不满足售后申请!";
                            response.Code = ResponseCode.UpperLimit;
                            return response;
                        }
                    }
                    //保存
                    var opers = MymoooBusinessDataServiceHelper.SaveBill(ctx, returnView.BusinessInfo, new DynamicObject[] { returnView.Model.DataObject }.ToArray());
                    if (opers.IsSuccess)
                    {
                        response.Data = new AfterSalesResponse()
                        {
                            AfsApplyId = returnView.Model.DataObject["BillNo"].ToString()
                        };
                        //清除释放网控
                        returnView.CommitNetworkCtrl();
                        returnView.InvokeFormOperation(FormOperationEnum.Close);
                        returnView.Close();
                    }
                    else
                    {
                        if (opers.ValidationErrors.Count > 0)
                        {
                            throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                        }
                        else
                        {
                            throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                        }
                    }
                }
                else
                {
                    response.Code = ResponseCode.NoExistsData;
                    response.ErrorMessage = string.Join(",", operationResult.ValidationErrors.Select(p => p.Message));
                }
            }
            else
            {
                response.Code = ResponseCode.NoExistsData;
                response.ErrorMessage = "部分DetailId在系统中不存在!";
            }
            return response;
        }
    }
}
