using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.BusinessEntity.CloudPlatform;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.BusinessFlow;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.PLN.Reserved.ReserveArgs;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.Mymooo.App.Core.StockManagement;
using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN.Reserve;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.App.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.BOS.App.Core.Report;
using Kingdee.BOS.BusinessEntity.ThirdSystem.MessageLog;
using Kingdee.BOS.Core.Metadata.WNReport.DS;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.K3.SCM.App.Core.ConvertBusinessService;
using MaterialInfo = Kingdee.Mymooo.Core.BaseManagement.MaterialInfo;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.BOS.Core.Util;
using Newtonsoft.Json;

namespace Kingdee.Mymooo.App.Core.SalesManagement
{
	/// <summary>
	/// 销售订单服务
	/// </summary>
	public class SalesOrderService : ISalesOrderService
	{
		/// <summary>
		/// 关闭销售订单或行
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="salesOrderId"></param>
		/// <param name="fdocumentStatus"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> ClosedSalesOrderAction(Context ctx, ApprovalMessageRequest request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (request.SpStatus != "2")
			{
				response.Code = ResponseCode.Abort;
				response.Message = "审批不通过！";
				return response;
			}
			var apiinfo = ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Approval/K3CloudGetApproval?applyeventno={request.ApplyeventNo}");
			var salinfo = JsonConvertUtils.DeserializeObject<ResponseMessage<K3CloudClosedSalOrderRequest>>(apiinfo);
			response.Data = salinfo;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(false);
			operateOption.SetValidateFlag(false);
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "SAL_SaleOrder") as FormMetadata;

			List<KeyValuePair<object, object>> pkEntryIds = null;

			if (salinfo.Data.OperationType == SalOrderClosedOperationType.Bill)
			{
				object[] ids = new object[] { salinfo.Data.SaleOrderID };
				pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
				SetStatusService setStatusService = new SetStatusService();
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					var oper = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "YLBillClose", operateOption);
					if (!oper.IsSuccess)
					{
						if (oper.ValidationErrors.Count > 0)
						{
							response.Message = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
						}
						else
						{
							response.Message = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
						}
						return response;
					}
					else
					{
						string sSql = $@"/*dialect*/UPDATE T_SAL_ORDER SET FMANUALCLOSE=1 WHERE FID={salinfo.Data.SaleOrderID}";
						DBServiceHelper.Execute(ctx, sSql);
						response.Code = ResponseCode.Success;
						response.Message = "操作成功";
					}
					cope.Complete();
				}
			}
			else
			{
				foreach (var item in salinfo.Data.SaleOrderEntrys)
				{
					//operateOption.SetValidateFlag(true);
					object[] ids = new object[] { item };
					pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(salinfo.Data.SaleOrderID, x)).ToList();
					SetStatusService setStatusService = new SetStatusService();
					using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
					{
						var oper = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "YLMRPClose", operateOption);
						if (!oper.IsSuccess)
						{
							if (oper.ValidationErrors.Count > 0)
							{
								response.Message = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
							}
							else
							{
								response.Message = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
							}
							return response;
						}
						else
						{
							string sSql = $@"/*dialect*/UPDATE dbo.T_SAL_ORDERENTRY SET FMANUALROWCLOSE=1 WHERE FENTRYID={item}";
							DBServiceHelper.Execute(ctx, sSql);
							response.Code = ResponseCode.Success;
							response.Message = "操作成功";
						}
						cope.Complete();
					}
				}
			}
			return response;

		}
		/// <summary>
		/// 取消销售订单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="salesOrderId">销售订单ID</param>
		/// <param name="fdocumentStatus">单据状态(Z:暂存，A创建，B:审核中，C:已审核，D:重新审核)</param>
		/// <returns></returns>
		public ResponseMessage<dynamic> CancelSalesOrderAction(Context ctx, int salesOrderId, string fdocumentStatus)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "SAL_SaleOrder") as FormMetadata;
			object[] ids = new object[] { salesOrderId };
			List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
			SetStatusService setStatusService = new SetStatusService();
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				//反审批销售单（单据状态为创建和重新审核的才能作废）
				//增加反审核后删除收款计划匹配的收款记录然后保存订单
				if (fdocumentStatus.Equals("B") || fdocumentStatus.Equals("C"))
				{
					var oper = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "UnAudit", operateOption);
					if (!oper.IsSuccess)
					{
						if (oper.ValidationErrors.Count > 0)
						{
							response.Message = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
						}
						else
						{
							response.Message = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
						}
						return response;
					}

				}
				//作废
				var oper2 = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "Cancel", operateOption);
				if (!oper2.IsSuccess)
				{
					if (oper2.ValidationErrors.Count > 0)
					{
						response.Message = string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
					}
					else
					{
						response.Message = string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
					}
					return response;
				}
				else
				{
					response.Code = ResponseCode.Success;
					response.Message = "操作成功";
				}
				cope.Complete();
				return response;
			}
		}

		/// <summary>
		/// 修改发货通知单物流等信息
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> ModifyDeliveryExpressageService(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			//开始执行
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			response.Data = request;
			var view = FormMetadataUtils.CreateBillView(ctx, "SAL_DELIVERYNOTICE", request.FId);
			try
			{
				//已经关闭的不需要继续执行
				if (view.Model.GetValue("FCLOSESTATUS").Equals("B"))
				{
					request.IsEnd = true;
					response.Message = $"单据编号为“{view.Model.GetValue("FBILLNO")}”的发货通知单，单据已关闭！";
					response.Code = ResponseCode.Success;
					return response;
				}
				//已经作废的不需要继续执行
				if (view.Model.GetValue("FCANCELSTATUS").Equals("B"))
				{
					request.IsEnd = true;
					response.Message = $"单据编号为“{view.Model.GetValue("FBILLNO")}”的发货通知单，单据已作废！";
					response.Code = ResponseCode.Success;
					return response;
				}
				request.CustomerName = Convert.ToString(((DynamicObject)view.Model.GetValue("FCustomerID"))["Name"]);
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					//先更新发货通知单的拣货完成时间和包装完成时间
					view.Model.SetValue("FPickingCompleteDate", request.PickingCompleteDate);
					view.Model.SetValue("FPackagingCompleteDate", request.PackagingCompleteDate);
					view.Model.SetValue("FTrackingNumber", request.TrackingNumber);
					view.Model.SetValue("FTrackingName", request.TrackingName);
					view.Model.SetValue("FTrackingDate", request.TrackingDate);
					view.Model.SetValue("FTrackingUser", request.TrackingUser);
					//创建人微信Code
					request.CreateUserWxCode = GetUserWxCode(ctx, Int64.Parse(((DynamicObject)view.Model.DataObject["CREATORID"])["id"].ToString()));
					//业务员微信Code
					if ((DynamicObject)view.Model.DataObject["SALESMANID"] != null)
					{
						request.SalUserWxCode = GetSalUserWxCode(ctx, Int64.Parse(((DynamicObject)view.Model.DataObject["SALESMANID"])["id"].ToString()));
					}
					request.CreatorId = Int64.Parse(((DynamicObject)view.Model.DataObject["CREATORID"])["id"].ToString());
					var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
					foreach (var item in request.Det)
					{
						var entry = entrys.FirstOrDefault(x => item.EntryId == Convert.ToInt64(x["Id"]));
						if (entry != null)
						{
							var rowIndex = entrys.IndexOf(entry);
							view.Model.SetValue("FPackagingUser", item.PackagingUser, rowIndex);
							view.Model.SetValue("FPackagingDate", item.PackagingDate, rowIndex);
							view.Model.SetValue("FCloudStockStatus", item.Status, rowIndex);
							view.Model.SetValue("FPickingUser", item.PickingUser, rowIndex);
							view.Model.SetValue("FPickingDate", item.PickingDate, rowIndex);
							//获取供货组织
							item.SupplyOrgId = Int64.Parse(((DynamicObject)view.Model.GetValue("FSupplyTargetOrgId", rowIndex))["id"].ToString());
							//供货组织是否云仓储回调下架
							item.IsCloudStockCallBack = Boolean.Parse(((DynamicObject)view.Model.GetValue("FSupplyTargetOrgId", rowIndex))["FIsCloudStockCallBack"].ToString());
							//获取出货仓库ID
							item.StockId = Int64.Parse(((DynamicObject)view.Model.GetValue("FStockID", rowIndex))["id"].ToString());
							//获取物料ID
							item.MaterialId = Int64.Parse(((DynamicObject)view.Model.GetValue("FMaterialID", rowIndex))["id"].ToString());
							//获取物料MsterID
							item.MsterID = Int64.Parse(((DynamicObject)view.Model.GetValue("FMaterialID", rowIndex))["msterID"].ToString());
							//获取物料销售单位
							item.SalUnitID = Int64.Parse(((DynamicObject)view.Model.GetValue("FUnitID", rowIndex))["id"].ToString());
							if ((entry["FEntity_Link"] as DynamicObjectCollection).Count > 0)
							{
								//对应销售订单的ID
								item.SBillId = ((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SBillId"].ToString();
								item.SID = ((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SID"].ToString();
								item.OrderNo = Convert.ToString(entry["OrderNo"]);
								item.OrderSeq = Convert.ToString(entry["OrderSeq"]);
							}
						}
					}

					//验证分步式调拨单是否审核
					VerifyTransferOut(ctx, request);

					//验证调拨单没问题，则保存发货通知单
					var oper = service.SaveBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
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

					//全项关闭，走作废
					if (request.Det.Where(x => (x.Status == 0 || x.Status == 1)).Count() == 0)
					{
						//先反批核，再作废
						var unAuditOper = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { request.FId }, "UnAudit");
						if (!unAuditOper.IsSuccess)
						{
							if (unAuditOper.ValidationErrors.Count > 0)
							{
								response.Message = string.Join(";", unAuditOper.ValidationErrors.Select(p => p.Message));
							}
							else
							{
								response.Message = string.Join(";", unAuditOper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
							}
							return response;
						}
						var cancelOper = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { request.FId }, "Cancel");
						if (!cancelOper.IsSuccess)
						{
							if (cancelOper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", cancelOper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", cancelOper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
						request.IsEnd = true;
						request.IsThisCancel = true;
						response.Message = string.Join(";", cancelOper.OperateResult.Select(p => p.Message));
						response.Code = ResponseCode.Success;
						cope.Complete();
						//发送企业微信消息
						//驳回(通知业务及助理)
						List<string> bh_WxCode = new List<string>();
						if (!string.IsNullOrEmpty(request.CreateUserWxCode))
						{
							bh_WxCode.Add(request.CreateUserWxCode);
						}
						if (!string.IsNullOrEmpty(request.SalUserWxCode))
						{
							bh_WxCode.Add(request.SalUserWxCode);
						}
						var bh_WxContent = $"--------------发货通知单【{request.BillNo}】----------------" + "\r\n";
						foreach (var item in request.Det)
						{
							bh_WxContent += $"行号【{item.Seq}】物料号【{item.ItemNo}】；" + "\r\n";
						}
						bh_WxContent += "已整单作废，请重新下单发。";
						if (bh_WxCode.Count > 0)
						{
							SendTextMessageUtils.SendTextMessage(string.Join("|", bh_WxCode), bh_WxContent);
						}
					}
					else
					{
						//正常流程
						cope.Complete();
						response.Code = ResponseCode.Success;
						response.Data = request;
						response.Message = "操作成功";
					}

				}
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			finally
			{
				//清除释放网控
				view.CommitNetworkCtrl();
				view.InvokeFormOperation(FormOperationEnum.Close);
				view.Close();
			}
			return response;
		}

		/// <summary>
		/// 全国一部,华南二部调拨出库
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> DeliveryTransferOutService(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			response.Data = request;
			if (request.IsEnd)
			{
				response.Code = ResponseCode.Success;
				response.Message = "操作成功";
				return response;
			}
			//判断是否已经全部生成调拨出
			var sumQty = request.Det.Where(w => w.IsCloudStockCallBack == true).Sum(x => x.Quantity);
			//如果没有需要调拨的，直接跳过
			if (sumQty == 0)
			{
				response.Code = ResponseCode.Success;
				response.Message = "不需要生成调拨单";
				return response;

			}
			//只能查询全国一部和华南二部和华南五部的数量（只查需要云存储回调生成调拨单的组织）
			if (GetTransferOutQty(ctx, request.BillNo) == sumQty)
			{
				//获取发货通知单对应调拨出的FID（审核中）
				request.DeliveryTransfers = GetTransferOutFId(ctx, request.BillNo);
				response.Code = ResponseCode.Success;
				response.Message = "已生成调拨单";
				return response;
			}
			try
			{
				var customerName = request.CustomerName;
				//处理数据（正常和部分特结，全部一部和华南二部的数据）
				List<DeliveryNoticeDetEntity> list = request.Det.Where(x =>
				(x.Status == 0 || x.Status == 1) && x.IsCloudStockCallBack == true).ToList();
				//需要调拨的数据集合
				List<Allocate> allocates = new List<Allocate>();
				foreach (var item in list)
				{
					decimal delqty = item.Quantity;
					var orderno = Convert.ToString(item.OrderNo);
					var orderseq = Convert.ToString(item.OrderSeq);
					//查询组织间需求单
					string sSql = $@"/*dialect*/SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FSTOCKID,t2.FDEMANDINTERID,t1.FSUPPLYORGID,t1.FBASEUNITID,t1.FSUPPLYINTERID
                        FROM T_PLN_RESERVELINKENTRY t1
                        INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        INNER JOIN T_PLN_REQUIREMENTORDER t3 ON t2.FDEMANDINTERID=t3.FID
                        WHERE t2.FSRCINTERID='{item.SBillId}' AND t2.FSRCENTRYID='{item.SID}'
                        AND t1.FSUPPLYFORMID='STK_Inventory' AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                        AND t3.FDEMANDQTY-t3.FBASETRANOUTQTY>=0
                        GROUP BY t1.FSTOCKID,t2.FDEMANDINTERID,t1.FSUPPLYORGID,t1.FBASEUNITID,t1.FSUPPLYINTERID";
					var RequirementorderDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
					foreach (var reitem in RequirementorderDatas)
					{
						if (delqty <= 0)
						{
							continue;
						}
						decimal sqty = 0;
						//如果存在单位不一致的情况，做单位转换数量再比较
						var resunitID = Convert.ToInt64(reitem["FBASEUNITID"]);
						var baseqty = Convert.ToDecimal(reitem["FBASEQTY"]);
						IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
						if (item.SalUnitID != resunitID)
						{
							baseqty = convertService.GetUnitTransQty(ctx, item.MaterialId, resunitID, item.SalUnitID, baseqty);
						}

						if (delqty - baseqty > 0)
						{
							sqty = baseqty;
							delqty -= baseqty;
						}
						else
						{
							sqty = delqty;
							delqty -= sqty;
						}
						//获取调出仓库
						var src_ck = LoadBDFullObject(ctx, "BD_STOCK", Convert.ToInt64(reitem["FSTOCKID"]));
						var dest_ck = LoadBDFullObject(ctx, "BD_STOCK", item.StockId);

						sSql = $@"SELECT t3.FSUPPLIERID,t2.FMATERIALID,FSTOCKID FROM V_STK_INVENTORY_CUS t1
                                INNER JOIN dbo.T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMASTERID AND t1.FSTOCKORGID=t2.FUSEORGID
                                INNER JOIN t_BD_Supplier t3 ON t1.FOWNERID=t3.FMASTERID AND t3.FUSEORGID=t1.FKEEPERID
                                WHERE FID='{reitem["FSUPPLYINTERID"]}' AND t1.FOWNERTYPEID='BD_Supplier'";
						var vmiDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
						if (vmiDatas.Count > 0)
						{
							var vmiTransfer = new VmiItem()
							{
								FDelNoticeEID = Convert.ToString(item.EntryId),
								FSalSrcrowid = item.SID,
								FRequirementId = reitem["FDEMANDINTERID"].ToString(),
								FOwnerId = Convert.ToInt64(vmiDatas[0]["FSUPPLIERID"]),
								FMaterialId = Convert.ToInt64(vmiDatas[0]["FMATERIALID"]),
								FSrcStockId_Id = Convert.ToInt64(src_ck["Id"]),
								FSrcStockId = src_ck,
								FQty = sqty,
								FBaseQty = convertService.GetUnitTransQty(ctx, item.MaterialId, item.SalUnitID, resunitID, sqty),
							};

							var result = RequirementPushTransfer(ctx, vmiTransfer);
							sSql = $@"SELECT * FROM dbo.T_STK_INVENTORY
                            WHERE FMATERIALID={item.MsterID} 
                            AND FOWNERID={reitem["FSUPPLYORGID"]} 
                            AND FSTOCKID={src_ck["Id"]} AND FBASEQTY-FLOCKQTY>={sqty}";
							var stkdatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
							CreateReserveLink(ctx, Convert.ToInt64(reitem["FDEMANDINTERID"]),
								new lockinfo
								{
									id = stkdatas.First()["FID"].ToString(),
									stockorgid = stkdatas.First()["FSTOCKORGID"].ToString(),
									stockid = stkdatas.First()["FSTOCKID"].ToString(),
									baseunitid = stkdatas.First()["FBASEUNITID"].ToString()
								},
								sqty
								);
							//var saveResult = PushReq(this.Context, srcbillid, srcrowid, sqty);
							//创建与销售订单的预留关系
							//var pkids = saveResult.SuccessDataEnity.Select(x => Convert.ToInt64(x["Id"])).FirstOrDefault();
							//CreateReserveLink(Convert.ToInt64(srcrowid), pkids);
						}
						allocates.Add(new Allocate
						{
							CreatorId_Id = request.CreatorId,
							SalBillNo = orderno,
							SalBillSEQ = orderseq,
							DeliveryNoticeNumber = request.BillNo,
							DeliveryNoticeSEQ = item.Seq,
							DeliveryNoticeID = request.FId,
							DeliveryNoticeEntryID = item.EntryId,
							FID = reitem["FDEMANDINTERID"].ToString(),
							TargetOrgId = reitem["FSUPPLYORGID"].ToString(),
							//FDestMaterialID = src_material,
							FMaterialId = item.MsterID,
							FBASEQTY = convertService.GetUnitTransQty(ctx, item.MaterialId, item.SalUnitID, resunitID, sqty),
							FQTY = sqty,
							FSrcStock_Id = Convert.ToInt64(src_ck["Id"]),
							FSrcStockId = src_ck,
							FDestStock_Id = item.StockId,
							FDestStockId = dest_ck,
						});



					}
					if (delqty > 0)
					{
						throw new Exception($"第{item.Seq}行物料{item.ItemNo},可调拨数量不足!");
					}
				}
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					List<long> longs = new List<long>();
					//组织间需求单下推分步式调出单
					if (allocates.Count > 0)
					{
						var opresult = SalDeliveryNoticePushAllocate(ctx, allocates);
						if (opresult.IsSuccess)
						{
							var sContent = opresult.SuccessDataEnity.Select(p => p["BillNo"].ToString());
							if (!string.IsNullOrWhiteSpace(request.CreateUserWxCode))
							{
								SendTextMessageUtils.SendTextMessage(request.CreateUserWxCode, $"客户:{customerName},发货通知[{request.BillNo}]已生成相关调拨单请查阅：" + string.Join(",", sContent));
							}
							var pks = opresult.SuccessDataEnity.Select(p => Convert.ToInt64(p["Id"]));
							longs.AddRange(pks);
						}
					}
					request.DeliveryTransfers.AddRange(longs);
					cope.Complete();

				}
				if (list.Count > 0 && request.DeliveryTransfers.Count == 0)
				{
					throw new Exception("调出单生成失败");
				}
				response.Code = ResponseCode.Success;
				response.Message = "生成调拨单成功";
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			return response;
		}

		/// <summary>
		/// 全国一部,华南二部调拨入库
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> DeliveryTransferInService(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			response.Data = request;
			if (request.IsEnd)
			{
				response.Code = ResponseCode.Success;
				response.Message = "操作成功";
				return response;
			}
			try
			{
				//处理数据（正常和部分特结，全部一部和华南二部的数据）
				// 构建单据主键参数
				List<KeyValuePair<object, object>> pkEntityIds = new List<KeyValuePair<object, object>>();
				foreach (var item in request.DeliveryTransfers)
				{
					var sSql = $@"/*dialect*/SELECT DISTINCT t2.FID FROM dbo.T_STK_STKTRANSFERINENTRY_LK t1
                        INNER JOIN T_STK_STKTRANSFERINENTRY t2 ON t1.FENTRYID=t2.FENTRYID
                        INNER JOIN T_STK_STKTRANSFERIN t3 ON t3.FID=t2.FID
                        WHERE t1.FSBILLID={item}  and t3.FDOCUMENTSTATUS='B' ";
					var dataid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
					if (dataid > 0)
					{
						pkEntityIds.Add(new KeyValuePair<object, object>(dataid, ""));
					}
				}
				if (pkEntityIds.Count > 0)
				{
					using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
					{
						FormMetadata meta = MetaDataServiceHelper.Load(ctx, "STK_TRANSFERIN") as FormMetadata;
						// 构建操作可选参数对象
						OperateOption auditOption = OperateOption.Create();
						auditOption.SetIgnoreWarning(true);
						auditOption.SetIgnoreInteractionFlag(true);
						auditOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
						List<object> paras = new List<object>();
						paras.Add("1");
						paras.Add("");
						// 调用审核操作
						ISetStatusService setStatusService = ServiceFactory.GetSetStatusService(ctx);
						// 如下调用方式，需显示交互信息
						IOperationResult auditResult = setStatusService.SetBillStatus(ctx,
						meta.BusinessInfo,
						pkEntityIds,
						paras,
						"Audit",
						auditOption);
						// 判断审核结果，如果失败，则内部会抛出错误，回滚代码
						if (!auditResult.IsSuccess)
						{
							if (auditResult.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", auditResult.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", auditResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
						cope.Complete();
					}
				}
				response.Code = ResponseCode.Success;
				response.Message = "操作成功";
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			return response;
		}

		/// <summary>
		/// 下推生成销售出库单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GenerateOutStockService(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			response.Data = request;
			if (request.IsEnd)
			{
				response.Code = ResponseCode.Success;
				response.Message = "操作成功";
				return response;
			}
			//如果已经生成出库单，则跳过
			string sql = $@"select top 1 FBILLNO from T_SAL_OUTSTOCK where FBILLNO='{request.BillNo}' ";
			if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
			{
				response.Code = ResponseCode.Success;
				response.Message = "已生成出库单";
				return response;
			}
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			try
			{
				//处理数据
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
					// 下推销售出库单
					var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_DELIVERYNOTICE", "SAL_OUTSTOCK");
					var rule = rules.FirstOrDefault(t => t.IsDefault);
					if (rule == null)
					{
						throw new Exception("没有从发货通知单到销售出库单的转换关系");
					}
					foreach (var item in request.Det)
					{
						//正常和特结的才下推
						if (item.Status.Equals(0) || item.Status.Equals(1))
						{
							selectedRows.Add(new ListSelectedRow(request.FId.ToString(), item.EntryId.ToString(), 0, "SAL_DELIVERYNOTICE") { EntryEntityKey = "FEntity" });
						}
					}
					//有数据才需要下推
					if (selectedRows.Count > 0)
					{
						PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
						{
							TargetBillTypeId = "ad0779a4685a43a08f08d2e42d7bf3e9",     // 请设定目标单据单据类型
							TargetOrgId = 0,            // 请设定目标单据主业务组织
														//CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
						};
						//执行下推操作，并获取下推结果
						var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
						if (operationResult.IsSuccess)
						{
							var outView = FormMetadataUtils.CreateBillView(ctx, "SAL_OUTSTOCK");
							foreach (var item in operationResult.TargetDataEntities)
							{
								outView.Model.DataObject = item.DataEntity;
								outView.Model.SetValue("FCreatorId", request.CreatorId);
								outView.Model.SetValue("FCallbackDate", request.CallbackDate);
								var outEntrys = outView.Model.GetEntityDataObject(outView.BusinessInfo.GetEntity("FEntity"));
								List<DynamicObject> newRows = new List<DynamicObject>();
								foreach (var entry in outEntrys)
								{
									var thisList = request.Det.FirstOrDefault(x => x.EntryId == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
									var isExists = newRows.FirstOrDefault(x => Convert.ToInt64(((x["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]) == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
									if (isExists == null)
									{
										DynamicObject newRowObj = (DynamicObject)entry.Clone(false, true);
										newRowObj["RealQty"] = thisList.Quantity;
										newRows.Add(newRowObj);
									}
								}
								outEntrys.Clear();
								foreach (var entry in newRows)
								{
									outEntrys.Add(entry);
								}
								foreach (var entry in outEntrys)
								{
									var rowIndex = outEntrys.IndexOf(entry);
									outView.InvokeFieldUpdateService("FRealQty", rowIndex);
								}
							}
							//保存批核
							var opers = service.SaveAndAuditBill(ctx, outView.BusinessInfo, new DynamicObject[] { outView.Model.DataObject }.ToArray());
							if (opers.IsSuccess)
							{
								response.Code = ResponseCode.Success;
								response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
								//如果全项正常，则订单完成
								if (request.Det.Where(x => (x.Status == 1 || x.Status == 2)).Count() == 0)
								{
									request.IsEnd = true;
								}
								//清除释放网控
								outView.CommitNetworkCtrl();
								outView.InvokeFormOperation(FormOperationEnum.Close);
								outView.Close();
								cope.Complete();
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
							if (operationResult.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}
					}
					else
					{
						throw new Exception("订单异常，发货通知单下推销售出库单数据丢失。");
					}
				}
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}

			return response;
		}

		/// <summary>
		/// 发货通知单变更数量
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> ModifyDeliveryQuantityService(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			response.Data = request;
			if (request.IsEnd)
			{
				//本次作废
				if (request.IsThisCancel)
				{
					//更新销售订单预留数量
					UpYLQty(ctx, request);
				}
				response.Code = ResponseCode.Success;
				response.Message = "操作成功";
				return response;
			}
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			var view = FormMetadataUtils.CreateBillView(ctx, "SAL_DELIVERYNOTICE", request.FId);
			var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
			try
			{
				//处理数据(存在特结和关闭的才需要变更数量)
				if (request.Det.Where(x => x.Status == 1 || x.Status == 2).ToList().Count > 0)
				{
					using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
					{
						//更新发货通知单数量
						DeliveryNoticeList deliveryNoticeList = new DeliveryNoticeList();
						List<DeliveryNoticeList.DeliveryNoticeDetSetQtyEntity> setQtyEntity = new List<DeliveryNoticeList.DeliveryNoticeDetSetQtyEntity>();
						foreach (var item in request.Det.Where(x => x.Status == 1 || x.Status == 2).ToList())
						{
							setQtyEntity.Add(new DeliveryNoticeList.DeliveryNoticeDetSetQtyEntity
							{
								EntryId = item.EntryId,
								Quantity = item.Quantity
							});
							var entry = entrys.FirstOrDefault(x => item.EntryId == Convert.ToInt64(x["Id"]));
							if (entry != null)
							{
								var rowIndex = entrys.IndexOf(entry);
								view.Model.SetValue("FQTY", item.Quantity, rowIndex);
								view.InvokeFieldUpdateService("FQTY", rowIndex);
								view.Model.SetValue("FSTOCKCHANGEQTY", item.Quantity, rowIndex);
								view.InvokeFieldUpdateService("FSTOCKCHANGEQTY", rowIndex);
								//获取基础单位后的数量
								decimal baseUnitQty = decimal.Parse(view.Model.GetValue("FBaseUnitQty", rowIndex).ToString());
								view.Model.SetValue("FSTOCKCHANGEBASEQTY", baseUnitQty, rowIndex);
								view.InvokeFieldUpdateService("FSTOCKCHANGEBASEQTY", rowIndex);
							}
						}
						deliveryNoticeList.SetQty(setQtyEntity, ctx, view);
						//更新销售订单预留数量
						UpYLQty(ctx, request);
						cope.Complete();
					}

					// 仓库：丁良、罗仕兴
					//采购：李佩珠
					//销售：按发货通知单的制单人及其业务员
					//特结（通知业务及助理、仓库管理员、采购管理员）
					var tj_WxContent = "";
					List<string> tj_WxCode = new List<string>();
					//驳回(通知业务及助理)
					var bh_WxContent = "";
					List<string> bh_WxCode = new List<string>();
					//特结
					if (request.Det.Where(x => x.Status == 1).Count() > 0)
					{
						tj_WxContent = $"--------------发货通知单【{request.BillNo}】----------------" + "\r\n";

						//获取仓库不足通知单的微信Code(采购管理员和仓库管理员)
						var userList = GetUnderStockInformWxCode(ctx, String.Join(",", request.Det.Where(x => x.Status == 1).Select(p => p.EntryId).ToList()));
						foreach (var item in userList)
						{
							tj_WxCode.Add(item);
						}
					}
					//驳回
					if (request.Det.Where(x => x.Status == 2).Count() > 0)
					{
						bh_WxContent = $"--------------发货通知单【{request.BillNo}】----------------" + "\r\n";
					}
					if (!string.IsNullOrWhiteSpace(request.CreateUserWxCode))
					{
						tj_WxCode.Add(request.CreateUserWxCode);
						bh_WxCode.Add(request.CreateUserWxCode);
					}
					if (!string.IsNullOrWhiteSpace(request.SalUserWxCode))
					{
						tj_WxCode.Add(request.SalUserWxCode);
						bh_WxCode.Add(request.SalUserWxCode);
					}
					foreach (var item in request.Det)
					{
						if (item.Status.Equals(1))
						{
							//特结（通知业务及助理、仓库管理员、采购管理员）
							tj_WxContent += $"行号【{item.Seq}】物料号【{item.ItemNo}】，实际数量：{item.Quantity}；" + "\r\n";
						}
						else if (item.Status == 2)
						{
							//驳回(通知业务及助理)
							bh_WxContent += $"行号【{item.Seq}】物料号【{item.ItemNo}】；" + "\r\n";
						}
					}
					if (request.Det.Where(x => x.Status == 1).Count() > 0)
					{
						tj_WxContent += "与发货通知数量不一致，请采购及时补货，仓库及时调整库存，业务及助理注意到货后补发。";
					}
					if (request.Det.Where(x => x.Status == 2).Count() > 0)
					{
						bh_WxContent += "已驳回请重新下单发。";
					}
					//发送企业微信消息
					if (tj_WxCode.Count > 0 && !string.IsNullOrWhiteSpace(tj_WxContent))
					{
						SendTextMessageUtils.SendTextMessage(string.Join("|", tj_WxCode), tj_WxContent);
					}
					if (bh_WxCode.Count > 0 && !string.IsNullOrWhiteSpace(bh_WxContent))
					{
						SendTextMessageUtils.SendTextMessage(string.Join("|", bh_WxCode), bh_WxContent);
					}

				}
				response.Code = ResponseCode.Success;
				response.Message = "数量变更成功";

			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			finally
			{
				//清除释放网控
				view.CommitNetworkCtrl();
				view.InvokeFormOperation(FormOperationEnum.Close);
				view.Close();
			}
			return response;
		}

		/// <summary>
		/// 事业部红字调入
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> RedDeliveryTransferInService(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			response.Data = request;
			//红字调回的，作废了也要调回
			if (request.IsEnd && !request.IsThisCancel)
			{
				response.Code = ResponseCode.Success;
				response.Message = "操作成功";
				return response;
			}
			try
			{
				//红字调回头部备注
				string note = "";
				if (request.Det.Where(x => (x.Status == 0 || x.Status == 1)).Count() == 0)
				{
					note = request.BillNo + "整单特结";
				}
				else
				{
					note = request.BillNo + "部分特结";
				}
				//存在特结或者关闭的发货通知单，需要处理调拨单,
				//获取到特结和关闭的物料和数量，根据物料、仓库、供货组织分组
				var dnData = request.Det.Where(x => (x.Status == 1 || x.Status == 2) && x.IsCloudStockCallBack == false)
					.GroupBy(g => new { g.ItemNo, g.StockId, g.SupplyOrgId })
					.Select(t => new SupplyItemSumQty { ItemNo = t.Key.ItemNo, StockId = t.Key.StockId, SupplyOrgId = t.Key.SupplyOrgId, Qty = t.Sum(s => s.Qty - s.Quantity) }).ToList();
				if (dnData.Count > 0)
				{
					List<RedDeliveryTransferInMsg> msgList = new List<RedDeliveryTransferInMsg>();
					using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
					{
						var supplyOrgIdArr = dnData.Select(o => o.SupplyOrgId).Distinct();
						MymoooBusinessDataService service = new MymoooBusinessDataService();
						//调入单下推调出单
						foreach (var OrgId in supplyOrgIdArr)
						{
							var sql = $@"/*dialect*/select distinct t1.FID,t1.FBILLNO,t1.FDOCUMENTSTATUS,FSTOCKOUTORGID,t2.FDESTSTOCKID
                                from T_STK_STKTRANSFERIN t1 
                                inner join T_STK_STKTRANSFERINENTRY t2 on t1.FID=t2.FID
                                where t1.FCANCELSTATUS='A' and FPENYDELIVERYNOTICE='{request.BillNo}' and FSTOCKOUTORGID={OrgId} ";
							var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
							foreach (var items in datas)
							{
								var billNo = items["FBILLNO"].ToString();
								var itemArr = dnData.Where(x => (x.SupplyOrgId == Int64.Parse(items["FSTOCKOUTORGID"].ToString()))).ToList();
								//获取到当前组织的特结数据行
								var tjDetArr = request.Det.Where(x => (x.Status == 1 || x.Status == 2) && (x.SupplyOrgId == Int64.Parse(items["FSTOCKOUTORGID"].ToString()))).ToList();
								List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
								var view = FormMetadataUtils.CreateBillView(ctx, "STK_TRANSFERIN", items["FID"]);
								var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FSTKTRSINENTRY"));
								foreach (var item in tjDetArr)
								{
									//获取发货通知单序号相等的数据
									var seqEntrys = entrys.Where(x => item.Seq == Convert.ToInt32(x["FDeliveryNoticeSEQ"]));
									if (seqEntrys != null)
									{
										//调拨单订单项总数，总数小于等于特结数，不红字调回
										var oQty = seqEntrys.Sum(x => decimal.Parse(x["FQty"].ToString()));
										if (oQty > item.Quantity)
										{
											var tjSumQty = (oQty - item.Quantity);
											item.TjQty = tjSumQty;
											foreach (var entry in seqEntrys)
											{
												if (tjSumQty <= 0)
												{
													break;
												}
												var rowIndex = entrys.IndexOf(entry);
												var qty = decimal.Parse(view.Model.GetValue("FQty", rowIndex).ToString());
												selectedRows.Add(new ListSelectedRow(items["FID"].ToString(), entry["id"].ToString(), 0, "STK_TRANSFERIN") { EntryEntityKey = "FSTKTRSINENTRY" });
												tjSumQty -= qty;
											}
										}
										else if (oQty < item.Quantity)
										{
											throw new Exception($"调入单[{billNo}]的物料号[{item.ItemNo}]调拨单数量小于出库数");
										}
									}
								}

								if (selectedRows.Count > 0)
								{
									// 假设：上游单据FormId为sourceFormId，下游单据FormId为targetFormId
									var rules = ConvertServiceHelper.GetConvertRules(ctx, "STK_TRANSFERIN", "STK_TRANSFEROUT");
									var rule = rules.FirstOrDefault(t => t.IsDefault);
									if (rule == null)
									{
										throw new Exception("没有从分步式调入至分步式调出的转换关系");
									}
									//插件下推代码参考
									PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
									{
										TargetBillTypeId = "",     // 请设定目标单据单据类型
										TargetOrgId = 0,            // 请设定目标单据主业务组织
																	//CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
									};
									//执行下推操作，并获取下推结果
									var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
									List<DynamicObject> dynamicObjectList = new List<DynamicObject>();
									if (operationResult.IsSuccess)
									{
										//计算特结数量
										List<SupplyItemDet> newItemArr = new List<SupplyItemDet>();
										foreach (var item in tjDetArr)
										{
											newItemArr.Add(new SupplyItemDet
											{
												Qty = item.TjQty,
												ItemNo = item.ItemNo,
												Seq = item.Seq
											});
										}
										var newView = FormMetadataUtils.CreateBillView(ctx, "STK_TRANSFEROUT");
										//调回物料数量明细，一项发货通知单明细存在2个仓库
										List<long> srcStockIDList = new List<long>();
										foreach (var item in operationResult.TargetDataEntities)
										{
											newView.Model.DataObject = item.DataEntity;
											newView.Model.SetValue("FNOTE", note);
											//特结=0 退货=1
											newView.Model.SetValue("FPENYReturnType", "0");
											var newEntrys = newView.Model.GetEntityDataObject(newView.BusinessInfo.GetEntity("FSTKTRSOUTENTRY"));
											int rowIndex = 0;

											foreach (var entry in newEntrys)
											{
												var dnSeq = Int32.Parse(entry["FDeliveryNoticeSEQ"].ToString());
												//红字调回备注
												var entryNote = request.Det.Where(x => x.Seq.Equals(dnSeq)).FirstOrDefault()?.SpecialReason;
												//获取剩余特结数量
												var syQty = newItemArr.Where(x => x.Seq.Equals(dnSeq)).FirstOrDefault().Qty;
												//当前行的数量
												var thisQty = decimal.Parse(entry["FQty"].ToString());
												//如果当前行的数量大于剩余特结数量,则需要修改当前行的数量
												if (thisQty > syQty)
												{
													newView.Model.SetValue("FQty", syQty, rowIndex);
													newView.InvokeFieldUpdateService("FQty", rowIndex);
													foreach (var upItem in newItemArr.Where(x => x.Seq.Equals(dnSeq)))
													{
														upItem.Qty -= syQty;
													}
												}
												else
												{
													foreach (var upItem in newItemArr.Where(x => x.Seq.Equals(dnSeq)))
													{
														upItem.Qty -= thisQty;
													}
												}
												newView.Model.SetValue("FEntryNote", entryNote, rowIndex);
												rowIndex++;
												srcStockIDList.Add(Int64.Parse(((DynamicObject)entry["DestStockID"])["id"].ToString()));
											}
											dynamicObjectList.Add(newView.Model.DataObject);
										}

										var oper = service.SaveBill(ctx, newView.BusinessInfo, dynamicObjectList.ToArray());
										if (!oper.IsSuccess)
										{
											if (oper.ValidationErrors.Count > 0)
											{
												throw new Exception($"调入单[{billNo}]下推调出单保存失败：" + string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
											}
											else
											{

												throw new Exception($"调入单[{billNo}]下推调出单保存失败：" + string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
											}
										}

										var oper2 = service.SubmitBill(ctx, newView.BusinessInfo, new object[] { oper.OperateResult[0].PKValue });
										if (!oper2.IsSuccess)
										{
											if (oper2.ValidationErrors.Count > 0)
											{
												throw new Exception($"调入单[{billNo}]下推调出单[{oper.OperateResult[0].Number}]提交失败：" + string.Join(";", oper2.ValidationErrors.Select(p => p.Message)));
											}
											else
											{

												throw new Exception($"调入单[{billNo}]下推调出单[{oper.OperateResult[0].Number}]提交失败：" + string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
											}
										}
										else
										{
											//深圳蚂蚁仓管员(调出组织)
											var userList = GetStockInformWxCode(ctx, Int64.Parse(items["FDESTSTOCKID"].ToString()));
											var wxContent = $"分步式调出单号：{oper.OperateResult[0].Number}，" + "\r\n";
											foreach (var item in itemArr)
											{
												wxContent += $"物料【{item.ItemNo}】，数量：{item.Qty}；";
											}
											wxContent += $"需退回的数量，请退回供货组织{GetOrgName(ctx, Int64.Parse(items["FSTOCKOUTORGID"].ToString()))}。" + "\r\n";
											msgList.Add(new RedDeliveryTransferInMsg
											{
												Msg = wxContent,
												UserList = string.Join("|", userList.Distinct())
											});
											userList = new List<string>();
											//各组织仓管员
											foreach (var stockID in srcStockIDList.Distinct())
											{
												userList.AddRange(GetStockInformWxCode(ctx, stockID));
											}
											wxContent = $"分步式调出单号：{oper.OperateResult[0].Number}，" + "\r\n";
											foreach (var item in itemArr)
											{
												wxContent += $"物料【{item.ItemNo}】，数量：{item.Qty}；";
											}
											wxContent += $"被发货组织退回，请核查数量，如果没有数量请报废。";
											msgList.Add(new RedDeliveryTransferInMsg
											{
												Msg = wxContent,
												UserList = string.Join("|", userList.Distinct())
											});
										}
									}
									else
									{
										if (operationResult.ValidationErrors.Count > 0)
										{
											throw new Exception($"调入单[{billNo}]下推调出单失败：" + string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
										}
										else
										{
											throw new Exception($"调入单[{billNo}]下推调出单失败：" + string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
										}
									}
								}
							}
						}
						//调回没问题，再统一发送消息
						foreach (var msgItem in msgList)
						{
							if (!string.IsNullOrWhiteSpace(msgItem.UserList))
							{
								SendTextMessageUtils.SendTextMessage(msgItem.UserList, msgItem.Msg);
							}

						}
						cope.Complete();
					}
				}
				response.Code = ResponseCode.Success;
				response.Message = "事业部红字调回成功";

			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}

			return response;
		}

		/// <summary>
		/// MES下推生成销售出库单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> MesGenerateOutStockService(Context ctx, MesGenerateOutStockRequest request)
		{

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			//如果已经生成出库单，则跳过
			string sql = $@"select top 1 FBILLNO from T_SAL_OUTSTOCK where FBILLNO='{request.BillNo}' ";
			if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
			{
				response.Code = ResponseCode.Success;
				response.Message = "已生成出库单";
				return response;
			}
			//拣货状态
			foreach (var item in request.Details)
			{
				if (item.Quantity == 0)
				{
					item.Status = 2;
				}
				else if (item.Quantity < item.Qty)
				{
					item.Status = 1;
				}
				else if (item.Qty < item.Quantity)
				{
					response.Code = ResponseCode.ModelError;
					response.Message = "已发数量不能大于应发数量";
					return response;
				}
			}
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			string cUserWxCode = "";
			//处理数据
			try
			{
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{

					//修改发货通知单物流信息
					var view = FormMetadataUtils.CreateBillView(ctx, "SAL_DELIVERYNOTICE", request.Id);

					//已经关闭的不需要继续执行
					if (view.Model.GetValue("FCLOSESTATUS").Equals("B"))
					{
						response.Message = $"单据编号为“{view.Model.GetValue("FBILLNO")}”的发货通知单，单据已关闭！";
						response.Code = ResponseCode.Success;
						return response;
					}
					//已经作废的不需要继续执行
					if (view.Model.GetValue("FCANCELSTATUS").Equals("B"))
					{
						response.Message = $"单据编号为“{view.Model.GetValue("FBILLNO")}”的发货通知单，单据已作废！";
						response.Code = ResponseCode.Success;
						return response;
					}
					//全项关闭，走作废
					if (request.Details.Where(x => (x.Status == 0 || x.Status == 1)).Count() == 0)
					{
						//先反批核，再作废
						var unAuditOper = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { request.Id }, "UnAudit");
						if (!unAuditOper.IsSuccess)
						{
							if (unAuditOper.ValidationErrors.Count > 0)
							{
								response.Message = string.Join(";", unAuditOper.ValidationErrors.Select(p => p.Message));
							}
							else
							{
								response.Message = string.Join(";", unAuditOper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
							}
							return response;
						}
						var cancelOper = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { request.Id }, "Cancel");
						if (!cancelOper.IsSuccess)
						{
							if (cancelOper.ValidationErrors.Count > 0)
							{
								throw new Exception(string.Join(";", cancelOper.ValidationErrors.Select(p => p.Message)));
							}
							else
							{
								throw new Exception(string.Join(";", cancelOper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
							}
						}

						response.Message = string.Join(";", cancelOper.OperateResult.Select(p => p.Message));
						response.Code = ResponseCode.Success;
						cope.Complete();
						return response;
					}

					//先更新发货通知单的拣货完成时间和包装完成时间
					view.Model.SetValue("FPickingCompleteDate", request.PickingCompleteDate);
					view.Model.SetValue("FPackagingCompleteDate", request.PackagingCompleteDate);
					view.Model.SetValue("FTrackingNumber", request.TrackingNumber);
					view.Model.SetValue("FTrackingName", request.TrackingName);
					view.Model.SetValue("FTrackingDate", request.TrackingDate);
					view.Model.SetValue("FTrackingUser", request.TrackingUser);
					cUserWxCode = GetUserWxCode(ctx, Int64.Parse(((DynamicObject)view.Model.DataObject["CREATORID"])["id"].ToString()));
					var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
					foreach (var item in request.Details)
					{
						var entry = entrys.FirstOrDefault(x => item.EntryId == Convert.ToInt64(x["Id"]));
						if (entry != null)
						{
							var rowIndex = entrys.IndexOf(entry);
							view.Model.SetValue("FPackagingUser", item.PackagingUser, rowIndex);
							view.Model.SetValue("FPackagingDate", item.PackagingDate, rowIndex);
							view.Model.SetValue("FCloudStockStatus", item.Status, rowIndex);
							view.Model.SetValue("FPickingUser", item.PickingUser, rowIndex);
							view.Model.SetValue("FPickingDate", item.PickingDate, rowIndex);
							view.Model.SetValue("FCloudStockStatus", item.Status, rowIndex);
						}
					}
					//保存发货通知单
					var oper = service.SaveBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
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
					//清除释放网控
					view.CommitNetworkCtrl();
					view.InvokeFormOperation(FormOperationEnum.Close);
					view.Close();
					// 下推销售出库单
					List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
					var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_DELIVERYNOTICE", "SAL_OUTSTOCK");
					var rule = rules.FirstOrDefault(t => t.IsDefault);
					if (rule == null)
					{
						throw new Exception("没有从发货通知单到销售出库单的转换关系");
					}
					foreach (var item in request.Details)
					{
						//正常和特结的才下推
						if (item.Status.Equals(0) || item.Status.Equals(1))
						{
							selectedRows.Add(new ListSelectedRow(request.Id.ToString(), item.EntryId.ToString(), 0, "SAL_DELIVERYNOTICE") { EntryEntityKey = "FEntity" });
						}

					}
					//有数据才需要下推
					if (selectedRows.Count > 0)
					{
						PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
						{
							TargetBillTypeId = "ad0779a4685a43a08f08d2e42d7bf3e9",     // 请设定目标单据单据类型
							TargetOrgId = 0,            // 请设定目标单据主业务组织
														//CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
						};
						//执行下推操作，并获取下推结果
						var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
						if (operationResult.IsSuccess)
						{
							var outView = FormMetadataUtils.CreateBillView(ctx, "SAL_OUTSTOCK");
							foreach (var item in operationResult.TargetDataEntities)
							{
								outView.Model.DataObject = item.DataEntity;
								var outEntrys = outView.Model.GetEntityDataObject(outView.BusinessInfo.GetEntity("FEntity"));
								List<DynamicObject> newRows = new List<DynamicObject>();
								foreach (var entry in outEntrys)
								{
									var thisList = request.Details.FirstOrDefault(x => x.EntryId == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
									var isExists = newRows.FirstOrDefault(x => Convert.ToInt64(((x["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]) == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
									if (isExists == null)
									{
										DynamicObject newRowObj = (DynamicObject)entry.Clone(false, true);
										newRowObj["RealQty"] = thisList.Quantity;
										newRows.Add(newRowObj);
									}
								}
								outEntrys.Clear();
								foreach (var entry in newRows)
								{
									outEntrys.Add(entry);
								}
								foreach (var entry in outEntrys)
								{
									var rowIndex = outEntrys.IndexOf(entry);
									outView.InvokeFieldUpdateService("FRealQty", rowIndex);
								}
							}
							//保存批核
							var opers = service.SaveAndAuditBill(ctx, outView.BusinessInfo, new DynamicObject[] { outView.Model.DataObject }.ToArray());
							if (opers.IsSuccess)
							{
								response.Code = ResponseCode.Success;
								response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
								//清除释放网控
								outView.CommitNetworkCtrl();
								outView.InvokeFormOperation(FormOperationEnum.Close);
								outView.Close();
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

						//处理数据(存在特结和关闭的才需要变更数量)
						if (request.Details.Where(x => x.Status == 1 || x.Status == 2).ToList().Count > 0)
						{
							//更新发货通知单数量
							DeliveryNoticeList deliveryNoticeList = new DeliveryNoticeList();
							List<DeliveryNoticeList.DeliveryNoticeDetSetQtyEntity> setQtyEntity = new List<DeliveryNoticeList.DeliveryNoticeDetSetQtyEntity>();
							var nView = FormMetadataUtils.CreateBillView(ctx, "SAL_DELIVERYNOTICE", request.Id);
							var nEntrys = nView.Model.GetEntityDataObject(nView.BusinessInfo.GetEntity("FEntity"));
							foreach (var item in request.Details.Where(x => x.Status == 1 || x.Status == 2).ToList())
							{
								setQtyEntity.Add(new DeliveryNoticeList.DeliveryNoticeDetSetQtyEntity
								{
									EntryId = item.EntryId,
									Quantity = item.Quantity
								});
								var entry = nEntrys.FirstOrDefault(x => item.EntryId == Convert.ToInt64(x["Id"]));
								if (entry != null)
								{
									var rowIndex = nEntrys.IndexOf(entry);
									nView.Model.SetValue("FQTY", item.Quantity, rowIndex);
									nView.InvokeFieldUpdateService("FQTY", rowIndex);
									nView.Model.SetValue("FSTOCKCHANGEQTY", item.Quantity, rowIndex);
									nView.InvokeFieldUpdateService("FSTOCKCHANGEQTY", rowIndex);
									//获取基础单位后的数量
									decimal baseUnitQty = decimal.Parse(nView.Model.GetValue("FBaseUnitQty", rowIndex).ToString());
									nView.Model.SetValue("FSTOCKCHANGEBASEQTY", baseUnitQty, rowIndex);
									nView.InvokeFieldUpdateService("FSTOCKCHANGEBASEQTY", rowIndex);
								}
							}
							deliveryNoticeList.SetQty(setQtyEntity, ctx, nView);
							//清除释放网控
							nView.CommitNetworkCtrl();
							nView.InvokeFormOperation(FormOperationEnum.Close);
							nView.Close();
						}
						response.Code = ResponseCode.Success;
						response.Message += "数量变更成功。";

						if (!string.IsNullOrWhiteSpace(cUserWxCode))
						{
							SendTextMessageUtils.SendTextMessage(cUserWxCode, "您的发货通知已生成出库单请查阅：" + request.BillNo);
						}
						//提交事务
						cope.Complete();
					}
					else
					{
						throw new Exception("订单异常，发货通知单下推销售出库单数据丢失。");
					}
				}
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			return response;
		}

		/// <summary>
		/// MES下推生成销售退货单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> MesGenerateReturnStockService(Context ctx, MesGenerateReturnStockRequest request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			//如果已经生成退货单，则跳过
			string sql = $@"select top 1 FBILLNO from T_SAL_RETURNSTOCK where FBILLNO='{request.RetBillNo}' ";
			if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
			{
				response.Code = ResponseCode.Success;
				response.Message = "已生成退货单";
				return response;
			}
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			try
			{
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
					var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_RETURNNOTICE", "SAL_RETURNSTOCK");
					var rule = rules.FirstOrDefault(t => t.IsDefault);
					if (rule == null)
					{
						throw new Exception("没有从退货通知单到销售退货单的转换关系");
					}
					foreach (var item in request.Details)
					{
						selectedRows.Add(new ListSelectedRow(request.Id.ToString(), item.EntryId.ToString(), 0, "SAL_RETURNNOTICE") { EntryEntityKey = "FEntity" });
					}
					//有数据才需要下推
					if (selectedRows.Count > 0)
					{
						PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
						{
							TargetBillTypeId = "66fba68dd04b499e8860966f2ee60117",     // 请设定目标单据单据类型
							TargetOrgId = 0,            // 请设定目标单据主业务组织
														//CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
						};
						//执行下推操作，并获取下推结果
						var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
						if (operationResult.IsSuccess)
						{
							var view = FormMetadataUtils.CreateBillView(ctx, "SAL_RETURNSTOCK");
							foreach (var item in operationResult.TargetDataEntities)
							{
								view.Model.DataObject = item.DataEntity;
								view.Model.SetValue("FBillNo", request.RetBillNo);
								var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
								List<DynamicObject> newRows = new List<DynamicObject>();
								BusinessInfo businessInfo = view.BusinessInfo;
								BaseDataField bdField = businessInfo.GetField("FStockID") as BaseDataField;
								int seq = 1;
								foreach (var entry in entrys)
								{
									var thisList = request.Details.FirstOrDefault(x => x.EntryId == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
									for (int i = 0; i < thisList.SubDetails.Count; i++)
									{
										QueryBuilderParemeter p = new QueryBuilderParemeter();
										p.FormId = "BD_STOCK";
										p.SelectItems = SelectorItemInfo.CreateItems("FStockId");
										p.FilterClauseWihtKey = $"FNumber = '{Convert.ToString(thisList.SubDetails[i].StockCode)}' ";
										var stock_ck = BusinessDataServiceHelper.Load(ctx, bdField.RefFormDynamicObjectType, p)[0];

										if (i == 0)
										{
											entry["Seq"] = seq++;
											entry["RealQty"] = thisList.SubDetails[i].Qty;
											entry["StockId_Id"] = ((DynamicObject)stock_ck)["id"];
											entry["StockId"] = stock_ck;
											newRows.Add(entry);
										}
										else
										{
											DynamicObject newRowObj = (DynamicObject)entry.Clone(false, true);
											newRowObj["Seq"] = seq++;
											newRowObj["RealQty"] = thisList.SubDetails[i].Qty;
											newRowObj["StockId_Id"] = ((DynamicObject)stock_ck)["id"];
											newRowObj["StockId"] = stock_ck;
											newRows.Add(newRowObj);
										}
									}
								}
								entrys.Clear();
								foreach (var entry in newRows)
								{
									entrys.Add(entry);
								}
								foreach (var entry in entrys)
								{
									var rowIndex = entrys.IndexOf(entry);
									view.InvokeFieldUpdateService("FRealQty", rowIndex);
									view.InvokeFieldUpdateService("FStockId", rowIndex);
								}

							}
							//保存批核
							var opers = service.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
							if (opers.IsSuccess)
							{
								response.Code = ResponseCode.Success;
								response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
								//清除释放网控
								view.CommitNetworkCtrl();
								view.InvokeFormOperation(FormOperationEnum.Close);
								view.Close();
								cope.Complete();
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
					}
				}
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			return response;
		}

		/// <summary>
		/// 更新销售订单预留数量
		/// </summary>
		/// <param name="ctx"></param>
		private void UpYLQty(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			foreach (var item in request.Det.Where(x => (x.Status == 1 || x.Status == 2) && x.IsCloudStockCallBack == false).ToList())
			{
				var tjQty = (item.Qty - item.Quantity);
				string sSql = $@"/*dialect*/select FID from T_PLN_RESERVELINK t1 where t1.FSRCINTERID='{item.SBillId}' AND t1.FSRCENTRYID='{item.SID}' and FDEMANDFORMID='SAL_SaleOrder' ";
				var resid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
				if (resid > 0)
				{
					//调拨单出的数量
					var tranOutQty = GetTransferOutQty(ctx, item.SupplyOrgId, request.BillNo, item.Seq);
					if (tranOutQty > item.Quantity)
					{
						//特结数等于调拨单出的数量(库存不足，会更改调拨出的数量)-出库数量
						tjQty = (tranOutQty - item.Quantity);
						var billView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", resid);
						var entrys = billView.Model.GetEntityDataObject(billView.BusinessInfo.GetEntity("FEntity"));
						foreach (var entry in entrys)
						{
							if (tjQty == 0)
							{
								break;
							}
							var rowIndex = entrys.IndexOf(entry);
							if ((billView.Model.GetValue("FSUPPLYFORMID", rowIndex) as dynamic)["Id"].Equals("STK_Inventory"))
							{
								//预留数
								var ylQty = decimal.Parse(billView.Model.GetValue("FBASESUPPLYQTY", rowIndex).ToString());
								if (tjQty > ylQty)
								{
									billView.Model.SetValue("FBASESUPPLYQTY", 0, rowIndex);
									tjQty -= ylQty;
								}
								else
								{
									billView.Model.SetValue("FBASESUPPLYQTY", (ylQty - tjQty), rowIndex);
									tjQty = 0;
								}
							}
						}
						var oper = service.SaveBill(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject });
					}

				}
			}
		}

		/// <summary>
		/// 验证分步式调拨单是否审核
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		private void VerifyTransferOut(Context ctx, CloudWarehouseDnNoticeEntity request)
		{
			//根据物料、仓库、供货组织分组
			var dnData = request.Det.Where(x => x.IsCloudStockCallBack == false)
				.GroupBy(g => new { g.ItemNo, g.StockId, g.SupplyOrgId })
				.Select(t => new SupplyItemSumQty { ItemNo = t.Key.ItemNo, StockId = t.Key.StockId, SupplyOrgId = t.Key.SupplyOrgId, Qty = t.Sum(s => s.Qty - s.Quantity) }).ToList();

			//判断调出单是否未批核
			var supplyOrgIdArr = dnData.Select(o => o.SupplyOrgId).Distinct();
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			//验证调出单,如果整单都关闭的，调出单批核中的，反批核作废。存在特结的，则报错
			foreach (var OrgId in supplyOrgIdArr)
			{
				var sql = $@"/*dialect*/select distinct t1.FId,t1.FBILLNO,t1.FDOCUMENTSTATUS,t2.FSRCSTOCKID from T_STK_STKTRANSFEROUT t1 
                        inner join T_STK_STKTRANSFEROUTENTRY t2 on t1.FID=t2.FID
                        inner join T_BD_MATERIAL t3 on t2.FMATERIALID=t3.FMATERIALID
                        where t1.FCANCELSTATUS='A' and FPENYDELIVERYNOTICE='{request.BillNo}' and FSTOCKORGID='{OrgId}' ";
				var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
				foreach (var items in datas)
				{
					//全部关闭的
					if (request.Det.Where(x => (x.Status == 0 || x.Status == 1) && (x.SupplyOrgId == OrgId)).Count() == 0)
					{
						var view = FormMetadataUtils.CreateBillView(ctx, "STK_TRANSFEROUT", items["FId"]);
						if (!Convert.ToString(items["FDOCUMENTSTATUS"]).EqualsIgnoreCase("C"))
						{
							//填写备注
							view.Model.SetValue("FNOTE", request.BillNo + "特结作废");
							var saveOper = service.SaveBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
							//审批中，先反批核
							if (Convert.ToString(items["FDOCUMENTSTATUS"]).EqualsIgnoreCase("B"))
							{
								//反批核
								var oper = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { Convert.ToString(items["FId"]) }, "UnAudit");
								if (!oper.IsSuccess)
								{
									if (oper.ValidationErrors.Count > 0)
									{
										throw new Exception($"调出单[{Convert.ToString(items["FBILLNO"])}]反批核失败：" + string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
									}
									else
									{

										throw new Exception($"调出单[{Convert.ToString(items["FBILLNO"])}]反批核失败：" + string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
									}
								}
							}
							//作废
							var oper2 = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { Convert.ToString(items["FId"]) }, "Cancel");
							if (!oper2.IsSuccess)
							{
								if (oper2.ValidationErrors.Count > 0)
								{
									throw new Exception($"调出单[{Convert.ToString(items["FBILLNO"])}]作废失败：" + string.Join(";", oper2.ValidationErrors.Select(p => p.Message)));
								}
								else
								{

									throw new Exception($"调出单[{Convert.ToString(items["FBILLNO"])}]作废失败：" + string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
								}
							}

							var userList = GetStockInformWxCode(ctx, Int64.Parse(items["FSRCSTOCKID"].ToString()));
							var itemArr = dnData.Where(x => (x.SupplyOrgId == OrgId)).ToList();
							var wxContent = $"分步式调出单号：{Convert.ToString(items["FBILLNO"])}，" + "\r\n"; ;
							foreach (var item in itemArr)
							{
								wxContent += $"物料【{item.ItemNo}】，调出数量：{item.Qty}；";
							}
							wxContent += $"已被发货组织全部项退货，调拨单已作废。";
							SendTextMessageUtils.SendTextMessage(string.Join("|", userList), wxContent);
						}
					}
					else
					{

						if (!Convert.ToString(items["FDOCUMENTSTATUS"]).EqualsIgnoreCase("C"))
						{
							var userList = GetStockInformWxCode(ctx, Int64.Parse(items["FSRCSTOCKID"].ToString()));
							var wxContent = $"分步式调出单号：{Convert.ToString(items["FBILLNO"])}，未审核。";
							SendTextMessageUtils.SendTextMessage(string.Join("|", userList), wxContent);
							throw new Exception(wxContent);
						}
					}
				}
			}

			//调入单未批核，通知深圳蚂蚁仓管员“分步式调入单号：XX，未审核，不能特结，请核后再特结”（根据调入组织和调入仓库人员通知）
			foreach (var OrgId in supplyOrgIdArr)
			{
				var sql = $@"/*dialect*/select distinct t1.FID,t1.FBILLNO,t1.FDOCUMENTSTATUS,FSTOCKOUTORGID,t2.FSRCSTOCKID,t2.FDESTSTOCKID
                                from T_STK_STKTRANSFERIN t1 
                                inner join T_STK_STKTRANSFERINENTRY t2 on t1.FID=t2.FID
                                where FPENYDELIVERYNOTICE='{request.BillNo}' and FSTOCKOUTORGID={OrgId} ";
				var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
				foreach (var items in datas)
				{
					if (!Convert.ToString(items["FDOCUMENTSTATUS"]).EqualsIgnoreCase("C"))
					{
						var userList = GetStockInformWxCode(ctx, Int64.Parse(items["FDESTSTOCKID"].ToString()));
						var wxContent = $"分步式调入单号：{Convert.ToString(items["FBILLNO"])}，未审核。";
						SendTextMessageUtils.SendTextMessage(string.Join("|", userList), wxContent);
						throw new Exception(wxContent);
					}
				}
			}
		}

		/// <summary>
		/// 获取发货通知单对应调拨出的数量（只查需要云存储回调生成调拨单的组织）
		/// </summary>
		/// <returns></returns>
		private decimal GetTransferOutQty(Context ctx, string dnNo)
		{
			List<SqlParam> pars = new List<SqlParam>() {
				new SqlParam("@DnNo", KDDbType.String, dnNo) };
			var sql = $@"/*dialect*/select SUM(t1.FQTY) FQTY from T_STK_STKTRANSFEROUTENTRY  t1
                        inner join T_STK_STKTRANSFEROUT t2 on t1.FID=t2.FID
                        inner join t_org_organizations t3 on t2.FStockOrgID=t3.FORGID
                        where t3.FIsCloudStockCallBack=1 and FPENYDELIVERYNOTICE=@DnNo";
			return DBServiceHelper.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
		}


		//获取发货通知单序号对应调拨出的数量
		private decimal GetTransferOutQty(Context ctx, long supplyOrgId, string dnNo, int dnSeq)
		{
			List<SqlParam> pars = new List<SqlParam>() {
				new SqlParam("@SupplyOrgId", KDDbType.Int64, supplyOrgId),
				new SqlParam("@DnNo", KDDbType.String, dnNo) ,
				new SqlParam("@DnSeq", KDDbType.Int32, dnSeq) };
			var sql = $@"/*dialect*/select SUM(FQTY) FQTY from T_STK_STKTRANSFEROUTENTRY  
                where FPENYDELIVERYNOTICE=@DnNo and FDELIVERYNOTICESEQ=@DnSeq and FOWNERID=@SupplyOrgId";
			return DBServiceHelper.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
		}

		/// <summary>
		/// 获取发货通知单对应调拨出的FID（审核中）
		/// </summary>
		/// <returns></returns>
		private List<long> GetTransferOutFId(Context ctx, string dnNo)
		{
			List<long> longs = new List<long>();
			List<SqlParam> pars = new List<SqlParam>() {
				new SqlParam("@DnNo", KDDbType.String, dnNo) };
			var sql = $@"/*dialect*/select distinct t1.FID  from T_STK_STKTRANSFEROUTENTRY t1
                        inner join T_STK_STKTRANSFEROUT t2 on t1.FID=t2.FID
                        where FPENYDELIVERYNOTICE=@DnNo and t2.FDOCUMENTSTATUS='B' ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				longs.Add(Convert.ToInt64(data["FID"]));
			}
			return longs;
		}

		//根据组织ID获取组织名称
		private string GetOrgName(Context ctx, long orgid)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@Orgid", KDDbType.Int64, orgid) };
			var sql = $@"/*dialect*/select top 1 FNAME from  t_org_organizations org 
             inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=2052
			 where org.FORGID=@Orgid";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}

		//获取创建人微信Code
		private string GetUserWxCode(Context ctx, long userId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
			string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";

			return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}

		//获取销售员微信Code
		private string GetSalUserWxCode(Context ctx, long salId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SalID", KDDbType.Int64, salId) };
			string sql = $@"select FWECHATCODE from V_BD_SALESMAN  sal where sal.fid=@SalID";

			return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
		}

		//获取仓库不足通知单的微信Code(采购管理员和仓库管理员)
		private List<string> GetUnderStockInformWxCode(Context ctx, string entryIds)
		{
			List<string> list = new List<string>();
			string sql = $@"select distinct FWECHATCODE from T_SAL_DELIVERYNOTICEENTRY a
            inner join t_BD_Stock b on a.FSHIPMENTSTOCKID=b.FSTOCKID
            inner join t_BD_StockInform c on b.FSTOCKID=c.FStockId
            inner join T_HR_EMPINFO d on c.FUNDERSTOCKINFORM=d.FID
            where a.FENTRYID in({entryIds})";

			var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				list.Add(Convert.ToString(data["FWECHATCODE"]));
			}
			return list;
		}

		/// <summary>
		/// 根据ID获取实体
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="formId"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		DynamicObject LoadBDFullObject(Context ctx, string formId, long pkid)
		{
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, formId) as FormMetadata;
			// 构建查询参数，设置过滤条件
			QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
			queryParam.FormId = formId;
			queryParam.BusinessInfo = meta.BusinessInfo;
			queryParam.FilterClauseWihtKey = string.Format(" {0} = '{1}' ", meta.BusinessInfo.GetForm().PkFieldName, pkid);
			var bdObjs = BusinessDataServiceHelper.Load(ctx, meta.BusinessInfo.GetDynamicObjectType(), queryParam);
			return bdObjs[0];
		}
		DynamicObject LoadBDFullObject(Context ctx, string formId, string number, long orgid)
		{
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, formId) as FormMetadata;
			// 构建查询参数，设置过滤条件
			QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
			queryParam.FormId = formId;
			queryParam.BusinessInfo = meta.BusinessInfo;
			queryParam.FilterClauseWihtKey = string.Format(" {0} = '{1}' ", meta.BusinessInfo.GetForm().NumberFieldKey, number);
			queryParam.FilterClauseWihtKey += string.Format(" and FUseOrgId = {0}", orgid);
			var bdObjs = BusinessDataServiceHelper.Load(ctx, meta.BusinessInfo.GetDynamicObjectType(), queryParam);
			return bdObjs[0];
		}

		/// <summary>
		/// 根据仓库ID获取通知人的微信Code
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="entryIds"></param>
		/// <returns></returns>
		private List<string> GetStockInformWxCode(Context ctx, long stockId)
		{
			List<string> list = new List<string>();
			string sql = $@"select distinct FWECHATCODE from  t_BD_Stock b 
            inner join t_BD_StockInform c on b.FSTOCKID=c.FStockId
            inner join T_HR_EMPINFO d on c.FUNDERSTOCKINFORM=d.FID
			where b.FSTOCKID={stockId} ";
			var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				list.Add(Convert.ToString(data["FWECHATCODE"]));
			}
			return list;
		}
		/// <summary>
		/// 销售订单下推计划订单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> SalBillPushPlanBill(Context ctx, List<ENGBomInfo> bomlist)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			try
			{
				Dictionary<long, DynamicObject> entryids = new Dictionary<long, DynamicObject>();
				long salorgid = 0;
				foreach (var item in bomlist)
				{
					if (item.SalBillEntryId != 0)
					{
						if (!entryids.ContainsKey(item.SalBillEntryId))
						{
							var sSql = $@"SELECT t2.FSALEORGID FROM dbo.T_SAL_ORDERENTRY t1
                                        LEFT JOIN dbo.T_SAL_ORDER t2 ON t1.FID=t2.FID
                                        where t1.FENTRYID={item.SalBillEntryId}";
							salorgid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
							var engbom = LoadBDFullObject(ctx, "ENG_BOM", item.FNUMBER, salorgid);
							entryids.Add(item.SalBillEntryId, engbom);
						}
					}
				}

				//处理数据
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					foreach (var item in entryids)
					{
						List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
						StockPushEntity push = new StockPushEntity();
						var row = new ListSelectedRow("", item.Key.ToString(), 0, "SAL_SaleOrder");
						row.EntryEntityKey = "FSaleOrderEntry"; //这里最容易忘记加，是重点的重点
						selectedRows.Add(row);

						push.TargetOrgId = salorgid;
						push.listSelectedRow = selectedRows;
						push.ConvertRule = "PENY_SalOrder_PLANORDER";
						//push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
						//push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);
						var saveResult = this.BillPush(ctx, push, item.Value);
						var pkids = saveResult.SuccessDataEnity.Select(p => Convert.ToInt64(p["Id"])).FirstOrDefault();
						CreatePlnReserveLink(ctx, item.Key, pkids);
					}
					cope.Complete();
				}
				response.Code = ResponseCode.Success;
				response.Message = "创建计划预留成功";
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}

			return response;
		}
		/// <summary>
		/// 非标销售订单创建非标BOM
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> FBSalBillCreateBom(Context ctx, ChangeOrderTaskRequest request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			//response.Data = request;
			//MymoooBusinessDataService service = new MymoooBusinessDataService();
			try
			{
				List<ENGBomInfo> bominfo = new List<ENGBomInfo>();
				//处理数据
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					//var view = FormMetadataUtils.CreateBillView(ctx, "SAL_SaleOrder", 1);
					//7401803华东五部7401821华南五部
					string[] orgs = new string[] { "HNFI", "HDFI" };
					if (!orgs.Contains(request.OrganizationNumber))
					{
						response.Message = $"单据编号为“{request.SalesOrderNo}”并非五部非标订单,不处理！";
						response.Code = ResponseCode.Abort;
						return response;
					}
					//if (request.BillType != "FB")
					//{
					//    response.Message = $"单据编号为“{request.SalesOrderNo}”并非非标订单,不处理！";
					//    response.Code = ResponseCode.Success;
					//    return response;
					//}
					foreach (var item in request.Details)
					{
						string sSql = $"SELECT * FROM PENY_T_SubFBBomInfoEntity WHERE FEntryID={item.EntryId}";
						var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
						int rowcount = datas.Count;
						//新增物料
						foreach (var materialitem in datas)
						{
							string code = Convert.ToString(materialitem["FSPECIFICATION"]);
							string name = Convert.ToString(materialitem["FFBBOMNAME"]);

							string baseUnit = "Pcs";
							string storeUnit = "Pcs";
							string purchaseUnit = "Pcs";
							string saleUnit = "Pcs";

							//int productID = Convert.ToInt32(this.View.Model.GetValue("FProductId", i));
							SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
							productsmallclass.Id = item.SmallId;

							var material = new MaterialInfo(code, name);
							material.FBaseUnitId = baseUnit;
							material.FStoreUnitID = storeUnit;
							material.FPurchaseUnitId = purchaseUnit;
							material.FPurchasePriceUnitId = purchaseUnit;
							material.FSaleUnitId = saleUnit;
							material.Length = Convert.ToDecimal(materialitem["FLENGTH"]);
							material.Width = Convert.ToDecimal(materialitem["FWIDTH"]);
							material.Height = Convert.ToDecimal(materialitem["FHEIGHT"]);
							material.Weight = Convert.ToDecimal(materialitem["FWEIGHT"]);
							material.WeightUnitid = Convert.ToString(GetUnitId(ctx, Convert.ToString(materialitem["FWEIGHTUNITID"])));
							material.VolumeUnitid = Convert.ToString(GetUnitId(ctx, Convert.ToString(materialitem["FVOLUMEUNITID"])));

							//material.ProductId = productID;
							material.ProductSmallClass = productsmallclass;
							MaterialService materialService = new MaterialService();
							var results = materialService.TryBomGetOrAdd(ctx, material);
						}
						//新增BOM
						List<ENGBomInfo> boms = new List<ENGBomInfo>();
						GetBOMInfos(datas, item.MaterialNumber, boms);
						ENGBomService bomService = new ENGBomService();
						var reqbom = bomService.TryGetOrAdds(ctx, boms.ToArray());
						foreach (var bomitem in reqbom)
						{
							bomitem.SalBillId = 0;
							bomitem.SalBillEntryId = item.EntryId;
							bominfo.Add(bomitem);
						}
					}

					if (bominfo.Count > 0)
					{
						cope.Complete();
						response.Code = ResponseCode.Success;
						response.Data = bominfo;
						response.Message = "创建成功";
					}
					else
					{
						response.Code = ResponseCode.Exception;
						response.Message = "没有可创建的BOM";
					}
				}
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}

			return response;
		}
		private long GetUnitId(Context ctx, string number)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UnitId", KDDbType.String, number) };
			var sql = $@"/*dialect*/SELECT FUNITID FROM dbo.T_BD_UNIT WHERE FNUMBER=@UnitId";
			return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
		}
		/// <summary>
		/// 发货通知单下推分步式调拨单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="allocates"></param>
		/// <returns></returns>
		public IOperationResult SalDeliveryNoticePushAllocate(Context ctx, List<Allocate> allocates)
		{
			List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
			StockPushEntity push = new StockPushEntity();
			foreach (var item in allocates)
			{
				var row = new ListSelectedRow(item.FID, string.Empty, 0, "PLN_REQUIREMENTORDER");
				//row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
				selectedRows.Add(row);
			}

			push.listSelectedRow = selectedRows;
			push.ConvertRule = "PLN_REQUIREMENTORDER_2_TRANSOUT";
			var result = BillPush(ctx, push, allocates);
			return result;
		}
		public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, List<Allocate> allocates)
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
			pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

			var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
			var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

			// 把新拆分出来的单据体行，加入到下推结果中
			// 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
			//e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());

			foreach (DynamicObject targeEntry in targetObjs)
			{
				targeEntry["CreatorId_Id"] = allocates.First().CreatorId_Id;
				List<DynamicObject> newRows = new List<DynamicObject>();
				var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
				var rowEntry = targeEntry["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
				foreach (var rowlin in rowEntry)
				{
					var materialid = Convert.ToInt64(((DynamicObject)rowlin["MaterialId"])["msterID"]);

					var srcbillid = "";
					foreach (var itemlink in rowlin["FSTKTSTKRANSFEROUTENTRY_Link"] as DynamicObjectCollection)
					{
						srcbillid = itemlink["SBillId"] as string;
					}

					var aot = allocates.Where(x => x.FID == srcbillid).ToList();
					foreach (var itemlink in aot)
					{
						targeEntry["FPENYStockID_Id"] = itemlink.FSrcStockId["Id"];
						targeEntry["FPENYStockID"] = itemlink.FSrcStockId;
						//复制出新行
						DynamicObject newRowObj = (DynamicObject)rowlin.Clone(false, true);
						//修改赋值
						newRowObj["FPENYSalOrderNo"] = itemlink.SalBillNo;
						newRowObj["FPENYSalOrderSEQ"] = itemlink.SalBillSEQ;
						newRowObj["FPENYDeliveryNotice"] = itemlink.DeliveryNoticeNumber;
						newRowObj["FDeliveryNoticeSEQ"] = itemlink.DeliveryNoticeSEQ;
						newRowObj["FDeliveryNoticeID"] = itemlink.DeliveryNoticeID;
						newRowObj["FDeliveryNoticeENTRYID"] = itemlink.DeliveryNoticeEntryID;

						newRowObj["FQty"] = itemlink.FQTY;
						newRowObj["BaseQty"] = itemlink.FBASEQTY;
						//调出
						newRowObj["SrcStockID_Id"] = itemlink.FSrcStockId["Id"];
						newRowObj["SrcStockID"] = itemlink.FSrcStockId;
						//调入
						newRowObj["DestStockID_Id"] = itemlink.FDestStockId["Id"];
						newRowObj["DestStockID"] = itemlink.FDestStockId;
						newRowObj["SrcStockStatusID_Id"] = 10000;
						newRowObj["DestStockStatusID_Id"] = 10004;

						//修改货主
						//newRowObj["OwnerID_Id"] = stockorgid;
						//newRowObj["KeeperID_Id"] = stockorgid;

						newRows.Add(newRowObj);
					}
				}
				rowEntry.Clear();
				foreach (var item in newRows)
				{
					rowEntry.Add(item);
				}

				DBServiceHelper.LoadReferenceObject(ctx, rowEntry.ToArray(), rowEntry.DynamicCollectionItemPropertyType, true);
			}

			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//对转换结果进行处理
			//1. 直接调用保存接口，对数据进行保存
			return SaveTargetBill(ctx, targetBInfo, targetObjs);
		}
		public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, VmiItem vmiItem)
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
			pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

			var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
			var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

			// 把新拆分出来的单据体行，加入到下推结果中
			// 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
			//e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());
			foreach (DynamicObject targeEntry in targetObjs)
			{
				//var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
				targeEntry["OwnerOutIdHead_Id"] = vmiItem.FOwnerId;
				var rowEntry = targeEntry["TransferDirectEntry"] as DynamicObjectCollection;
				foreach (var rowlin in rowEntry)
				{
					rowlin["FOwnerOutId_Id"] = vmiItem.FOwnerId;
					rowlin["KeeperId_Id"] = targeEntry["StockOrgId_Id"];
					rowlin["KeeperOutId_Id"] = targeEntry["StockOrgId_Id"];
					rowlin["SrcStockId_Id"] = vmiItem.FSrcStockId_Id;
					rowlin["SrcStockId"] = vmiItem.FSrcStockId;
					rowlin["DestStockId_Id"] = vmiItem.FSrcStockId_Id;
					rowlin["DestStockId"] = vmiItem.FSrcStockId;
					rowlin["Qty"] = vmiItem.FQty;
					rowlin["BaseQty"] = vmiItem.FBaseQty;
					rowlin["ActQty"] = vmiItem.FQty;
					rowlin["SaleQty"] = vmiItem.FQty;
					rowlin["SalBaseQty"] = vmiItem.FBaseQty;
					rowlin["FPENYDeliverynoticeEID"] = vmiItem.FDelNoticeEID;
				}
			}
			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//对转换结果进行处理
			//1. 直接调用保存接口，对数据进行保存
			return this.SaveTargetBill(ctx, targetBInfo, targetObjs);

		}
		public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity)
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
			pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

			var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
			var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//1. 直接调用保存接口，对数据进行保存
			return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
		}
		public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, decimal salqty)
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
			}
			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//对转换结果进行处理
			//1. 直接调用保存接口，对数据进行保存
			return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
		}
		public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, DynamicObject bom)
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
				targeEntry["BomId_Id"] = bom["Id"];
				targeEntry["BomId"] = bom;
			}
			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//对转换结果进行处理
			//1. 直接调用保存接口，对数据进行保存
			return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
		}
		private IOperationResult SaveSubmitTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
		{
			OperateOption saveOption = OperateOption.Create();
			saveOption.SetIgnoreWarning(true);
			saveOption.SetIgnoreInteractionFlag(true);
			saveOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
			//保存
			SaveService saveService = new SaveService();
			//提交
			//SubmitService submitService = new SubmitService();
			IOperationResult saveResult = new OperationResult();

			saveResult = saveService.Save(ctx, targetBusinessInfo, targetBillObjs, saveOption);
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
			else
			{
				Dictionary<string, string> pkids = new Dictionary<string, string>();
				pkids = saveResult.SuccessDataEnity.ToDictionary(p => p["Id"].ToString(), p => p["BillNo"].ToString());
				SubmitService submitService = new SubmitService();
				//提交单据，若存在工作流，则提交工作流
				var resultlist = Kingdee.K3.Core.MFG.Utils.MFGCommonUtil.SubmitWithWorkFlow(ctx, targetBusinessInfo.GetForm().Id, pkids, saveOption);
			}
			return saveResult;
		}
		/// <summary>
		/// 销售订单下推生成收款单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="entry"></param>
		/// <param name="TargetOrgId"></param>
		/// <returns></returns>
		public IOperationResult SalesToReceiveBill(Context ctx, SalesOrderPushReceiveBillEntity entry, long TargetOrgId)
		{
			List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
			StockPushEntity push = new StockPushEntity();

			var row = new ListSelectedRow(entry.FID.ToString(), entry.FEntryID.ToString(), 0, "SAL_SaleOrder");
			row.EntryEntityKey = "FSaleOrderPlan"; //这里最容易忘记加，是重点的重点
			selectedRows.Add(row);

			push.listSelectedRow = selectedRows;
			push.ConvertRule = "SAL_SaleOrderToAR_ReceiveBill";
			push.TargetOrgId = TargetOrgId;
			push.TargetBillTypeId = "36cf265bd8c3452194ed9c83ec5e73d2";

			//得到转换规则
			var convertRule = this.GetConvertRule(ctx, push.ConvertRule);
			OperateOption pushOption = OperateOption.Create();//操作选项
															  //构建下推参数
															  //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
															  //单据下推参数
			PushArgs pushArgs = new PushArgs(convertRule, push.listSelectedRow.ToArray());
			//目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
			pushArgs.TargetOrgId = push.TargetOrgId;
			//目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
			pushArgs.TargetBillTypeId = push.TargetBillTypeId;
			// 自动下推，无需验证用户功能权限
			pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
			// 设置是否整单下推
			pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

			var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
			var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

			if (!(entry.rval is null))
			{
				foreach (var item in entry.rval)
				{
					DynamicObjectCollection rows = (DynamicObjectCollection)(((DynamicObject)targetObjs[0])["RECEIVEBILLENTRY"]);
					foreach (var nrows in rows)
					{
						//修改赋值
						if (item.valueType == TargetValueType.Object)
						{
							nrows[item.EntityValue + "_Id"] = (item.Val as DynamicObject)["Id"];
							nrows[item.EntityValue] = item.Val;
						}
						else
						{
							nrows[item.EntityValue] = item.Val;
						}
					}
				}
			}

			var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
			//1. 直接调用保存接口，对数据进行保存
			return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
		}
		/// <summary>
		/// 销售出库单下推生成应收单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="TargetOrgId"></param>
		/// <returns></returns>
		public IOperationResult SalesToReceivable(Context ctx, List<SalesOrderPushEntity> entry, long TargetOrgId)
		{
			List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
			StockPushEntity push = new StockPushEntity();
			foreach (var item in entry)
			{
				var row = new ListSelectedRow(item.FID.ToString(), item.FEntryID.ToString(), 0, "SAL_OUTSTOCK");
				row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
				selectedRows.Add(row);
			}

			push.listSelectedRow = selectedRows;
			push.ConvertRule = "AR_OutStockToReceivableMap";
			push.TargetOrgId = TargetOrgId;
			push.TargetBillTypeId = "180ecd4afd5d44b5be78a6efe4a7e041";

			return this.BillPush(ctx, push);
		}

		/// <summary>
		/// 保存目标单据
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="targetBusinessInfo"></param>
		/// <param name="targetBillObjs"></param>
		private IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
		{
			OperateOption saveOption = OperateOption.Create();
			saveOption.SetIgnoreWarning(true);
			saveOption.SetIgnoreInteractionFlag(true);
			SaveService saveService = new SaveService();
			var saveResult = saveService.SaveAndAudit(ctx, targetBusinessInfo, targetBillObjs, saveOption);
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
			return saveResult;
		}
		// 得到表单元数据
		private BusinessInfo GetBusinessInfo(Context ctx, string formId, FormMetadata metaData = null)
		{
			if (metaData != null) return metaData.BusinessInfo;
			metaData = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
			return metaData.BusinessInfo;
		}
		//得到转换规则
		private ConvertRuleElement GetConvertRule(Context ctx, string convertRuleId)
		{
			var convertRuleMeta = ConvertServiceHelper.GetConvertRule(ctx, convertRuleId);
			return convertRuleMeta.Rule;
		}

		public void ChangeSalesOrder(Context ctx, BusinessInfo businessInfo, params DynamicObject[] dynamicObjects)
		{
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{

				SaveService saveService = new SaveService();
				var operateOption = OperateOption.Create();
				operateOption.SetIgnoreWarning(true);
				var oper = saveService.SaveAndAudit(ctx, businessInfo, dynamicObjects, operateOption);
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
				//生效
				oper = BusinessDataServiceHelper.DoNothing(ctx, businessInfo, dynamicObjects.Select(p => p["Id"]).ToArray(), "TakeEffect", operateOption);
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
				//ChangeDownPrice   变更下游价格
				oper = BusinessDataServiceHelper.DoNothing(ctx, businessInfo, dynamicObjects.Select(p => p["Id"]).ToArray(), "ChangeDownPrice", operateOption);
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
		}

		public IOperationResult RequirementPushTransfer(Context ctx, VmiItem vmiItem)
		{
			List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
			StockPushEntity push = new StockPushEntity();
			var row = new ListSelectedRow(vmiItem.FRequirementId, string.Empty, 0, "PLN_REQUIREMENTORDER");
			//row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
			selectedRows.Add(row);

			push.listSelectedRow = selectedRows;
			push.ConvertRule = "PENY_PLN_REQUIREMENTORDER_TRANSFER";
			push.TargetBillTypeId = "005056941128823c11e323525db18103";
			//push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
			var result = this.BillPush(ctx, push, vmiItem);

			return result;
		}
		public IOperationResult PushReq(Context ctx, string billid, string entryid, decimal salqty)
		{
			List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
			StockPushEntity push = new StockPushEntity();
			var row = new ListSelectedRow(billid, entryid, 0, "SAL_SaleOrder");
			row.EntryEntityKey = "FSaleOrderEntry"; //这里最容易忘记加，是重点的重点
			selectedRows.Add(row);

			push.listSelectedRow = selectedRows;
			push.ConvertRule = "PENY_SalOrder_REQUIREMENTORDER";
			//push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
			//push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);

			var result = this.BillPush(ctx, push, salqty);
			return result;
		}

		public void CreateReserveLink(Context ctx, long salentid, long reqid)
		{
			//获取销售订单预留信息
			string sSql = $"SELECT FID FROM T_PLN_RESERVELINK WHERE FDEMANDENTRYID={salentid}";
			var _resid = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, string.Empty);

			var SalbillView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", _resid);
			//var Newentity = SalbillView.BusinessInfo.GetEntity("FEntity");
			sSql = $@"SELECT FID,FBILLNO,FSUPPLYMATERIALID,FSUPPLYORGID,FBASEUNITID,FBASEDEMANDQTY
                    ,'SAL_SaleOrder' AS SrcDemandFormId,FSALEORDERID,FSALEORDERENTRYID,FSALEORDERNO
                    FROM t_PLN_REQUIREMENTORDER WHERE FID={reqid}";
			var reqDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
			foreach (var item in reqDatas)
			{
				SalbillView.Model.CreateNewEntryRow("FEntity");
				var seq = SalbillView.Model.GetEntryRowCount("FEntity") - 1;
				SalbillView.Model.SetValue("FSUPPLYFORMID", "PLN_REQUIREMENTORDER", seq);
				SalbillView.Model.SetItemValueByID("FSUPPLYINTERID", Convert.ToString(item["FID"]), seq);
				SalbillView.Model.SetItemValueByID("FSUPPLYMATERIALID", Convert.ToInt64(item["FSUPPLYMATERIALID"]), seq);
				SalbillView.Model.SetItemValueByID("FSUPPLYORGID", Convert.ToInt64(item["FSUPPLYORGID"]), seq);
				SalbillView.Model.SetValue("FSUPPLYDATE", System.DateTime.Now, seq);
				//SalbillView.Model.SetItemValueByID("FSUPPLYSTOCKID", Convert.ToInt64(item["FSUPPLYSTOCKID_Id"]), seq);
				SalbillView.Model.SetItemValueByID("FBASESUPPLYUNITID", Convert.ToInt64(item["FBASEUNITID"]), seq);
				//供应数量
				SalbillView.Model.SetValue("FBASESUPPLYQTY", Convert.ToDecimal(item["FBASEDEMANDQTY"]), seq);

				//创建组织间需求单预留信息
				CreateReserveLink(ctx, reqid);
			}
			sSql = $"SELECT FID FROM T_PLN_RESERVELINK WHERE FDEMANDFORMID='PLN_REQUIREMENTORDER' AND FDEMANDINTERID={reqid}";
			var _roid = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, string.Empty);
			var NewReqView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", _roid);
			var Newentity = NewReqView.BillBusinessInfo.GetEntity("FEntity");
			for (int i = 0; i < SalbillView.Model.GetEntryRowCount("FEntity"); i++)
			{
				var supplyform = SalbillView.Model.GetValue("FSUPPLYFORMID", i) as DynamicObject;
				var supplyorgid = SalbillView.Model.GetValue("FSUPPLYORGID", i) as DynamicObject;
				if (supplyform["Id"].ToString().EqualsIgnoreCase("STK_Inventory")
					&& supplyorgid["Id"].ToString() != "224428")
				{
					//复制新行给组织间需求单
					var seq = NewReqView.Model.GetEntryRowCount("FEntity");
					DynamicObject newRowObj = (DynamicObject)SalbillView.Model.GetEntityDataObject(Newentity, i).Clone(false, true);
					NewReqView.Model.CreateNewEntryRow(Newentity, seq, newRowObj);
					//删除行
					SalbillView.Model.DeleteEntryRow("FEntity", i);
				}
			}
			FormMetadataUtils.SaveBill(ctx, NewReqView);

			FormMetadataUtils.SaveBill(ctx, SalbillView);
		}
		public void CreateReserveLink(Context ctx, long reqid, lockinfo lockinfo, decimal qty)
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
				//demandView.DemandEntryID = string.Empty;
				demandView.DemandEntryID = "";
				demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

				//demandView.
				demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
				demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
				demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
				demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

				demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
				demandView.DemandDate = System.DateTime.Now;
				demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
				demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
				demandView.BaseQty = qty;

				//取即时库存
				List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
				SupplyViewItem subRowView = new SupplyViewItem();

				subRowView.SupplyFormID_Id = "STK_Inventory";
				subRowView.SupplyInterID = Convert.ToString(lockinfo.id);

				subRowView.SupplyMaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
				subRowView.SupplyOrgID_Id = Convert.ToInt64(lockinfo.stockorgid);
				subRowView.SupplyDate = System.DateTime.Now;
				subRowView.SupplyStockID_Id = Convert.ToInt64(lockinfo.stockid);
				subRowView.SupplyAuxproID_Id = 0;
				subRowView.BaseSupplyUnitID_Id = Convert.ToInt64(lockinfo.baseunitid);
				//供应数量
				//subRowView.BaseActSupplyQty = lockinfo.qty;
				subRowView.BaseActSupplyQty = qty;
				subRowView.IntsupplyID = Convert.ToInt64(demaInterid);


				viewItems.Add(subRowView);
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
			linkService.ReserveLinkCreate(ctx, convertArgs, OperateOption.Create());
		}
		public void CreatePlnReserveLink(Context ctx, long salentid, long plnid)
		{
			//取销售订单信息
			string sSql = $@"SELECT t2.FOBJECTTYPEID as DemandFormID,t1.FID as DemandInterID,t2.FBILLNO as DemandBillNO,
t2.FOBJECTTYPEID as SrcDemandFormId,t1.FID as SrcDemandInterId,t1.FENTRYID as SrcDemandEntryId,t2.FBILLNO as SrcDemandBillNo,
t1.FSTOCKORGID as DemandOrgID,t1.FMATERIALID as MaterialID,t1.FBASEUNITID as BaseUnitID,t1.FQTY as BaseQty,
t1.FSUPPLYORGID
FROM T_SAL_ORDERENTRY t1
LEFT JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
WHERE t1.FENTRYID={salentid}";
			var salbilldata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
			DemandView demandView = new DemandView();
			if (salbilldata.Count > 0)
			{
				demandView.DemandFormID = salbilldata[0]["DemandFormID"].ToString();
				demandView.DemandInterID = salbilldata[0]["DemandInterID"].ToString();
				//demandView.DemandEntryID = string.Empty;
				demandView.DemandEntryID = salentid.ToString();
				demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

				demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
				demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
				demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
				demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

				demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
				demandView.DemandDate = System.DateTime.Now;
				demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
				demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
				demandView.BaseQty = Convert.ToDecimal(salbilldata[0]["BaseQty"]);
			}
			//取计划订单信息
			sSql = $@"SELECT FFORMID,FID AS SupplyInterID,FBILLNO AS SupplyBillNO,FSUPPLYMATERIALID AS SupplyMaterialID
,FUNITID AS BaseSupplyUnitID,FDEMANDQTY AS BaseActSupplyQty,FSUPPLYORGID AS SupplyOrgID
FROM T_PLN_PLANORDER WHERE FID={plnid}";
			var reqbilldata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
			List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
			SupplyViewItem subRowView = new SupplyViewItem();
			if (reqbilldata.Count > 0)
			{
				//subRowView.SupplyFormID_Id = "STK_Inventory";
				subRowView.SupplyFormID_Id = Convert.ToString(reqbilldata[0]["FFORMID"]);
				subRowView.SupplyInterID = Convert.ToString(reqbilldata[0]["SupplyInterID"]);
				subRowView.SupplyBillNO = Convert.ToString(reqbilldata[0]["SupplyBillNO"]);
				subRowView.SupplyMaterialID_Id = Convert.ToInt64(reqbilldata[0]["SupplyMaterialID"]);
				subRowView.BaseSupplyUnitID_Id = Convert.ToInt64(reqbilldata[0]["BaseSupplyUnitID"]);
				//供应数量
				subRowView.BaseActSupplyQty = Convert.ToInt64(reqbilldata[0]["BaseActSupplyQty"]);
				subRowView.SupplyOrgID_Id = Convert.ToInt64(reqbilldata[0]["SupplyOrgID"]);

				subRowView.SupplyDate = System.DateTime.Now;
				//subRowView.SupplyStockID_Id = 0;
				subRowView.SupplyAuxproID_Id = 0;

				viewItems.Add(subRowView);
			}
			demandView.supplyView = viewItems;

			//创建转移行信息
			List<ReserveLinkSelectRow> lstConvertInfo = CreateReserveRow(demandView);
			//构建预留转移参数
			ReserveArgs<ReserveLinkSelectRow> convertArgs = new ReserveArgs<ReserveLinkSelectRow>();
			//把预留转移行的信息赋给参数
			convertArgs.SelectRows = lstConvertInfo;
			//创建预留服务
			IReserveLinkService linkService = AppServiceContext.GetService<IReserveLinkService>();
			//调用预留创建接口
			linkService.ReserveLinkCreate(ctx, convertArgs, OperateOption.Create());

		}
		public ReserveOperationResult CreateReserveLink(Context ctx, long reqid)
		{
			//取组织间需求单信息
			string sSql = $@"SELECT FFORMID AS DemandFormID,fid AS DemandInterID,FBILLNO AS DemandBillNO
            ,'SAL_SaleOrder' AS SrcDemandFormId,FSALEORDERID AS SrcDemandInterId,FSALEORDERENTRYID AS SrcDemandEntryId,FSALEORDERNO AS SrcDemandBillNo
            ,FSUPPLYORGID AS DemandOrgID
            ,FSUPPLYMATERIALID AS MaterialID
            ,FUNITID AS BaseUnitID,FDEMANDQTY 
            FROM T_PLN_REQUIREMENTORDER WHERE FID={reqid}";
			var salbilldata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
			DemandView demandView = new DemandView();
			string demaInterid = "0";
			if (salbilldata.Count > 0)
			{
				demandView.DemandFormID = salbilldata[0]["DemandFormID"].ToString();
				demandView.DemandInterID = salbilldata[0]["DemandInterID"].ToString();
				demaInterid = salbilldata[0]["DemandInterID"].ToString();
				//demandView.DemandEntryID = string.Empty;
				demandView.DemandEntryID = "";
				demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

				//demandView.
				demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
				demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
				demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
				demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

				demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
				demandView.DemandDate = System.DateTime.Now;
				demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
				demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
				demandView.BaseQty = Convert.ToDecimal(salbilldata[0]["FDEMANDQTY"]);
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
			return linkService.ReserveLinkCreate(ctx, convertArgs, OperateOption.Create());
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
				//row.SupplyBillInfo.EntryID = subRowView.SupplyEntryId;
				//row.SupplyBillInfo.BillNo = subRowView.SupplyBillNO;
				row.SupplyMaterialID = subRowView.SupplyMaterialID_Id;
				row.SupplyOrgID = subRowView.SupplyOrgID_Id;
				row.SupplyDate = subRowView.SupplyDate;
				row.SupplyStockID = subRowView.SupplyStockID_Id;
				//row.SupplyStockLocID = subRowView.SupplyStockLocID_Id;
				//row.SupplyBomID = subRowView.SupplyBomID_Id;
				//row.SupplyLotId = subRowView.SupplyLot_Id;
				//row.SupplyLot_Text = subRowView.SupplyLot_Text;
				//row.SupplyMtoNO = subRowView.SupplyMtoNO;
				//row.SupplyProjectNO = "";
				row.SupplyAuxpropID = subRowView.SupplyAuxproID_Id;
				row.BaseSupplyUnitID = subRowView.BaseSupplyUnitID_Id;
				//供应数量
				row.BaseSupplyQty = subRowView.BaseActSupplyQty;
				row.LinkType = Enums.PLN_ReserveModel.Enu_ReserveBuildType.KdByManual;
				supplyRows.Add(row);
			}
			return supplyRows;
		}

		public List<ENGBomInfo> GetBOMInfos(DynamicObjectCollection bomentity, string parent, List<ENGBomInfo> bomlist)
		{
			ENGBomInfo bom = new ENGBomInfo(parent);
			bom.FMATERIALID = parent;
			var dataLines = bomentity.Where(o => o["FPARENTSPECIFICATION"].ToString().Equals(parent)).ToList();
			if (dataLines.Count > 0)
			{
				List<BomEntity> entitylist = new List<BomEntity>();
				foreach (var item in dataLines)
				{
					BomEntity ent = new BomEntity();
					//DynamicObject materialId = item["FMATERIALID"] as DynamicObject;
					//DynamicObjectCollection materialbase = materialId["MaterialBase"] as DynamicObjectCollection;
					//DynamicObject materialunit = materialbase[0]["BaseUnitId"] as DynamicObject;

					ent.FMATERIALIDCHILD = Convert.ToString(item["FSPECIFICATION"]);
					ent.FNUMERATOR = Convert.ToDecimal(item["FWEIGHT"]);
					ent.FDENOMINATOR = 1;
					ent.FUnitNumber = "Pcs";
					ent.FSCRAPRATE = 0;
					ent.SendMes = true;

					GetBOMInfos(bomentity, Convert.ToString(item["FSPECIFICATION"]), bomlist);
					entitylist.Add(ent);
				}
				bom.Entity = entitylist;
				bomlist.Add(bom);
			}
			return bomlist;
		}

		public ResponseMessage<QueryExpiryResponseList> QueryCustomerExpiryList(Context ctx, RequestExpiry message)
		{
			ResponseMessage<QueryExpiryResponseList> response = new ResponseMessage<QueryExpiryResponseList>();
			//RequestExpiry request = JsonConvert.DeserializeObject<RequestExpiry>(message);
			List<SqlParam> pars = new List<SqlParam>();
			if (message != null)
			{
				pars.Add(new SqlParam("@custcode", KDDbType.String, message.CustomerCode));
			}
			else
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "参数不能为空";
				return response;
			}
			response.Data = new QueryExpiryResponseList();
			//查询核销金额
			var sql = $@"/*dialect*/SELECT SUM(FBALANCE) AS FBALANCE
FROM (
-- 1. 应收单
SELECT P.FNOTVERIFICATEAMOUNT AS FBALANCE 
FROM T_AR_RECEIVABLE H
INNER JOIN T_AR_RECEIVABLEPLAN P ON H.FID = P.FID
INNER JOIN dbo.T_BD_CUSTOMER C ON H.FCUSTOMERID=C.FCUSTID
WHERE C.FNUMBER = @custcode
AND H.FDOCUMENTSTATUS = 'C' 
AND P.FNOTVERIFICATEAMOUNT <> 0 
--AND h.FENDDATE<GETDATE()
UNION ALL
-- 2. 其他应收单
SELECT (H.FAMOUNTFOR - H.FWRITTENOFFAMOUNTFOR) AS FBALANCE 
FROM T_AR_OTHERRECABLE H
INNER JOIN dbo.T_BD_CUSTOMER C ON H.FCONTACTUNIT=C.FCUSTID
WHERE C.FNUMBER = @custcode
AND H.FDOCUMENTSTATUS = 'C' 
AND (H.FAMOUNTFOR - H.FWRITTENOFFAMOUNTFOR) <> 0
--AND h.FENDDATE<GETDATE()
) T";
			var dbalance = DBServiceHelper.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
			var expiry = QueryCustomerExpiryTopList(ctx, message.CustomerCode).Data;
			response.Data.ExpiryAmount = dbalance;
			response.Data.ExpiryDay = expiry.ExpiryDay;
			response.Data.ExpiryBill = expiry.ExpiryBill;
			response.Code = ResponseCode.Success;
			return response;
		}

		public ResponseMessage<QueryExpiryResponseList> QueryCustomerExpiryTopList(Context ctx, string message)
		{
			ResponseMessage<QueryExpiryResponseList> response = new ResponseMessage<QueryExpiryResponseList>();
			//string request = JsonConvert.DeserializeObject<string>(message);
			string param = string.Empty;
			List<SqlParam> pars = new List<SqlParam>();
			if (!string.IsNullOrEmpty(message))
			{
				pars.Add(new SqlParam("@custcode", KDDbType.String, message));
			}
			else
			{
				response.Code = ResponseCode.ModelError;
				response.Message = "参数不能为空";
				return response;
			}
			response.Data = new QueryExpiryResponseList();

			//查询应收单
			string sql = string.Format(@"select h.FID,h.FBILLNO,h.FENDDATE as FDATE,h.FALLAMOUNTFOR,p.FNOTVERIFICATEAMOUNT
FROM T_AR_RECEIVABLE H
INNER JOIN T_AR_RECEIVABLEPLAN P ON H.FID = P.FID
INNER JOIN T_BD_CUSTOMER c on h.FCUSTOMERID = c.FCUSTID  
where c.FNUMBER =@custcode
AND H.FDOCUMENTSTATUS = 'C' 
AND P.FNOTVERIFICATEAMOUNT <> 0 
--AND h.FENDDATE<GETDATE()
order by h.FENDDATE", param);
			var receivableDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());

			var firstList = receivableDatas.FirstOrDefault();
			if (firstList != null)
			{
				response.Data.ExpiryDay = DateTime.Now.Date.Subtract(Convert.ToDateTime(firstList["FDATE"])).Days;
				response.Data.ExpiryBill = Convert.ToString(firstList["FBILLNO"]);
			}

			response.Code = ResponseCode.Success;
			return response;
		}

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
	}

	public class lockinfo
	{
		public string id { get; set; }
		public string materialid { get; set; }
		public string stockorgid { get; set; }
		public string stockid { get; set; }
		public string baseunitid { get; set; }

		public decimal qty { get; set; }
	}
	public class VmiItem
	{
		public string FDelNoticeEID { get; set; }
		public string FSalSrcrowid { get; set; }
		public string FRequirementId { get; set; }
		/// <summary>
		/// 物料内码
		/// </summary>
		public long FMaterialId { get; set; }
		public long FSrcStockId_Id { get; set; }
		public DynamicObject FSrcStockId { get; set; }
		public decimal FQty { get; set; }
		public decimal FBaseQty { get; set; }
		public long FOwnerId { get; set; }
	}
	public class SupplyViewItem
	{
		/// <summary>
		/// 
		/// </summary>
		public string SupplyFormID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyInterID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyEntryId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyBillNO { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyMaterialID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyOrgID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime SupplyDate { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyStockID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyStockLocID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyBomID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyLot_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyLot_Text { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SupplyMtoNO { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long SupplyAuxproID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long BaseSupplyUnitID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal BaseActSupplyQty { get; set; }
		public long IntsupplyID { get; set; }
		public long IntsupplyEntryId { get; set; }
	}

	public class DemandView
	{
		/// <summary>
		/// 
		/// </summary>
		public string DemandFormID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string DemandInterID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string DemandEntryID { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string DemandBillNO { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandFormId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandInterId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandEntryId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string SrcDemandBillNo { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long DemandOrgID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTime DemandDate { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long MaterialID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public long BaseUnitID_Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public decimal BaseQty { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public List<SupplyViewItem> supplyView { get; set; }
	}
}
