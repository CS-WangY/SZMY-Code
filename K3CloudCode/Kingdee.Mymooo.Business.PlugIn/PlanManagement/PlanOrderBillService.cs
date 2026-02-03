using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.ServiceHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.K3.MFG.App;
using Kingdee.BOS.BusinessEntity.ThirdSystem.MessageLog;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.BusinessEntity.ClearAndInitData;
using Kingdee.K3.MFG.ServiceHelper.PLN;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.K3.MFG.ServiceHelper;
using Newtonsoft.Json;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.BOS.WebApi.DataEntities;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.BOS.BusinessEntity.CloudPlatform;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Orm;
using Kingdee.K3.SCM.Core;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.K3.Core.MFG.Utils;
using static Kingdee.Mymooo.Core.SalesManagement.SalesOrderBillRequest;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.Core.BusinessFlow;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.ReserveLinkManagement;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Contracts;
using System.Runtime.CompilerServices;
using static Kingdee.K3.Core.MFG.MFGBillTypeConst;
using Kingdee.BOS.Web.Bill;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
	/// <summary>
	/// 计划订单
	/// </summary>
	public class PlanOrderBillService
	{
		/// <summary>
		/// mes手工生产订单接口
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> Mes2ErpCreateMo(Context ctx, PlanOrderSplitEntity request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			IOperationResult oper = new OperationResult();
			oper = CreateMoBillView(ctx, request, 2);
			response.Data = oper.OperateResult;

			return response;
		}
		/// <summary>
		/// MES回传计划拆分服务
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> SyncMES2PlanSplitService(Context ctx, PlanOrderSplitEntity request)
		{
			//ResponseMessage<PlanOrderSplitEntity> response = new ResponseMessage<PlanOrderSplitEntity>() { Data = request };
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			IOperationResult oper = new OperationResult();
			//using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			//{
			switch (request.ReleaseType)
			{
				case 1:
					oper = CreatePrBillView(ctx, request);

					break;
				case 2:
					oper = CreateMoBillView(ctx, request, 1);
					break;
			}
			try
			{
				CreateReserve(ctx, request.ReleaseType, oper, request.PlanPkId);
			}
			catch (Exception err)
			{
				response.Code = ResponseCode.Warning;
				response.ErrorMessage = err.ToString();
			}
			response.Data = oper.OperateResult;
			//    cope.Complete();
			//}
			return response;
		}
		private IOperationResult CreatePrBillView(Context ctx, PlanOrderSplitEntity request)
		{
			if (DBServiceHelper.ExecuteScalar<bool>(ctx, "select 1 from T_PUR_Requisition where FBILLNO = @FBILLNO", false, new SqlParam("@FBILLNO", KDDbType.String, request.BillNo)))
			{
				throw new Exception("采购订单号已经存在！");
			}
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PUR_Requisition") as FormMetadata;
			var billView = FormMetadataUtils.CreateBillView(ctx, "PUR_Requisition");
			//billView.Model.SetValue("FBillTypeID", bill.FBillTypeID, 0);
			billView.Model.SetValue("FBillNo", request.BillNo, 0);
			billView.Model.SetValue("FApplicationDate", request.Date, 0);
			billView.Model.SetValue("FPENYBackupBillNO", request.BillNo, 0);
			billView.Model.SetItemValueByNumber("FApplicationOrgId", "HDFI", 0);

			//根据微信编号获取员工id
			//string sSql = $@"SELECT FNUMBER FROM T_HR_EMPINFO
			//                WHERE FWECHATCODE='{bill.FApplicantId}'";
			//var userid = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "");
			//billView.Model.SetItemValueByNumber("FApplicantId", userid, 0);
			//billView.InvokeFieldUpdateService("FApplicantId", 0);

			billView.Model.DeleteEntryData("FEntity");
			var rowcount = 0;
			foreach (var item in request.DetailEntity)
			{
				billView.Model.CreateNewEntryRow("FEntity");
				billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
				billView.InvokeFieldUpdateService("FMaterialId", rowcount);
				billView.Model.SetValue("FReqQty", item.Qty, rowcount);
				billView.InvokeFieldUpdateService("FReqQty", rowcount);

				billView.Model.SetValue("FSrcBillTypeId", "PLN_PLANORDER");
				billView.Model.SetValue("FSrcBillNo", request.PlanBillNo);
				//销售信息
				var sSql = $@"/*dialect*/SELECT DISTINCT t6.FSALERID,t7.FNAME FROM T_PLN_PLANORDER_B t1
                                    LEFT JOIN T_SAL_ORDER t6 ON t1.FSALEORDERID=t6.FID
                                    LEFT JOIN V_BD_SALESMAN_L t7 ON t6.FSALERID=t7.fid
                                    WHERE t6.FBILLNO='@FBILLNO'";
				List<SqlParam> pars = new List<SqlParam>();
				pars.Add(new SqlParam("@FBILLNO", KDDbType.String, item.SaleOrderNo));
				var data = DBServiceHelper.ExecuteDynamicObject(ctx, sSql, paramList: pars.ToArray());
				var salUser = string.Join(",", data.Select(x => x["FNAME"]).Distinct());

				billView.Model.SetValue("FSoNo", item.SaleOrderNo);
				billView.Model.SetValue("FSoSeq", item.SaleOrderEntrySeq);
				billView.Model.SetValue("FSoUnitPrice", item.PenyPrice);
				billView.Model.SetValue("FPENYDELIVERYDATE", item.PenySalDatetime);
				billView.Model.SetValue("FPENYSALERS", salUser);
				billView.Model.SetValue("FDEMANDTYPE", 1);
				billView.Model.SetValue("FDEMANDBILLNO", item.SaleOrderNo);
				billView.Model.SetValue("FDEMANDBILLENTRYSEQ", item.SaleOrderEntrySeq);
				billView.Model.SetValue("FDEMANDBILLENTRYID", item.SaleOrderEntryId);
				//关联关系
				//明细子单据体
				var subEntity = (SubEntryEntity)billView.BillBusinessInfo.GetEntity("FENTITY_Link");
				var subEntityObjs = billView.Model.GetEntityDataObject(subEntity);
				billView.Model.CreateNewEntryRow("FENTITY_Link");
				subEntityObjs[0]["SBILLID"] = request.PlanPkId;
				subEntityObjs[0]["SID"] = request.PlanPkId;
				subEntityObjs[0]["RULEID"] = "PlanOrder_PurRequest";
				subEntityObjs[0]["STABLENAME"] = "T_PLN_PLANORDER";
				subEntityObjs[0]["REQSTOCKBASEQTY"] = item.Qty;
				subEntityObjs[0]["REQSTOCKBASEQTYOLD"] = 0;

				rowcount++;
			}


			var oper = SaveAndAuditBill(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject });
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
			else
			{
				return oper;
			}
		}
		private IOperationResult CreateMoBillView(Context ctx, PlanOrderSplitEntity request, int type)
		{
			if (DBServiceHelper.ExecuteScalar<bool>(ctx, "select 1 from T_PRD_MO where FBILLNO = @FBILLNO", false, new SqlParam("@FBILLNO", KDDbType.String, request.BillNo)))
			{
				throw new Exception("生产订单号已经存在！");
			}
			var orgid = GetOrgIdByOrgNumber(ctx, request.ApplicationOrgId);
			var orgidlist = GetOrgidList(ctx, orgid);

			foreach (var item in request.DetailEntity)
			{
				if (string.IsNullOrEmpty(item.ProcessID))
				{
					List<long> materials = new List<long>();
					foreach (var bomitem in item.BomInfoEntity)
					{
						var materialCode = bomitem.BomMaterialID;
						var materialName = bomitem.FBBomName;
						var materialInfo = new MaterialInfo(bomitem.BomMaterialID, bomitem.FBBomName);

						string baseUnit = "";
						string storeUnit = "";
						string purchaseUnit = "";
						string saleUnit = "";
						switch (bomitem.VolumeUnitid.ToUpper())
						{
							case "CM":
								baseUnit = "m";
								storeUnit = "cm";
								purchaseUnit = "cm";
								saleUnit = "cm";
								break;
							case "MM":
								baseUnit = "m";
								storeUnit = "mm";
								purchaseUnit = "mm";
								saleUnit = "mm";
								break;
							default:
								baseUnit = bomitem.VolumeUnitid;
								storeUnit = bomitem.VolumeUnitid;
								purchaseUnit = bomitem.VolumeUnitid;
								saleUnit = bomitem.VolumeUnitid;
								break;
						}

						materialInfo.FBaseUnitId = baseUnit;
						materialInfo.FStoreUnitID = storeUnit;
						materialInfo.FPurchaseUnitId = purchaseUnit;
						materialInfo.FPurchasePriceUnitId = purchaseUnit;
						materialInfo.FSaleUnitId = saleUnit;

						materialInfo.Textures = bomitem.Textures;
						materialInfo.MaterialType = bomitem.MaterialType;
						materialInfo.Weight = bomitem.Weight;
						materialInfo.Length = bomitem.Length;
						materialInfo.Width = bomitem.Width;
						materialInfo.Height = bomitem.Height;
						materialInfo.Volume = bomitem.VOLUME;

						materialInfo.WeightUnitid = Convert.ToString(GetUnitId(ctx, bomitem.WeightUnitid));
						materialInfo.VolumeUnitid = Convert.ToString(GetUnitId(ctx, bomitem.VolumeUnitid));
						SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
						productsmallclass.Id = GetSMALLID(ctx, bomitem.SMALLID);//华东-原材料
						materialInfo.ProductSmallClass = productsmallclass;
						MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, orgidlist);
						materials.Add(materialInfo.MasterId);
					}
					//新增BOM
					List<ENGBomInfo> boms = new List<ENGBomInfo>();
					GetBOMInfos(item.BomInfoEntity, item.MaterialId, item.ProcessID, boms);
					var reqbom = ENGBomServiceHelper.TryGetOrAdds(ctx, boms.ToArray());
					item.BomNumber = reqbom.Select(x => x.FNUMBER).FirstOrDefault();
					//分配BOM
					MaterialServiceHelper.MaterialAllocate(ctx, materials);
					ENGBomServiceHelper.BomAllocate(ctx, reqbom.ToList<ENGBomInfo>());
				}
				else
				{
					var sSql = $@"SELECT FNUMBER FROM dbo.T_ENG_BOM WHERE FPENYPROCESSID='{item.ProcessID}' ORDER BY FID DESC";
					item.BomNumber = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "");
					if (string.IsNullOrEmpty(item.BomNumber))
					{
						List<long> materials = new List<long>();
						foreach (var bomitem in item.BomInfoEntity)
						{
							var materialCode = bomitem.BomMaterialID;
							var materialName = bomitem.FBBomName;
							var materialInfo = new MaterialInfo(bomitem.BomMaterialID, bomitem.FBBomName);

							string baseUnit = "";
							string storeUnit = "";
							string purchaseUnit = "";
							string saleUnit = "";
							switch (bomitem.VolumeUnitid.ToUpper())
							{
								case "CM":
									baseUnit = "m";
									storeUnit = "cm";
									purchaseUnit = "cm";
									saleUnit = "cm";
									break;
								case "MM":
									baseUnit = "m";
									storeUnit = "mm";
									purchaseUnit = "mm";
									saleUnit = "mm";
									break;
								default:
									baseUnit = bomitem.VolumeUnitid;
									storeUnit = bomitem.VolumeUnitid;
									purchaseUnit = bomitem.VolumeUnitid;
									saleUnit = bomitem.VolumeUnitid;
									break;
							}

							materialInfo.FBaseUnitId = baseUnit;
							materialInfo.FStoreUnitID = storeUnit;
							materialInfo.FPurchaseUnitId = purchaseUnit;
							materialInfo.FPurchasePriceUnitId = purchaseUnit;
							materialInfo.FSaleUnitId = saleUnit;

							materialInfo.Textures = bomitem.Textures;
							materialInfo.MaterialType = bomitem.MaterialType;
							materialInfo.Weight = bomitem.Weight;
							materialInfo.Length = bomitem.Length;
							materialInfo.Width = bomitem.Width;
							materialInfo.Height = bomitem.Height;
							materialInfo.Volume = bomitem.VOLUME;

							materialInfo.WeightUnitid = Convert.ToString(GetUnitId(ctx, bomitem.WeightUnitid));
							materialInfo.VolumeUnitid = Convert.ToString(GetUnitId(ctx, bomitem.VolumeUnitid));
							SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
							productsmallclass.Id = GetSMALLID(ctx, bomitem.SMALLID);//华东-原材料
							materialInfo.ProductSmallClass = productsmallclass;
							MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, orgidlist);
							materials.Add(materialInfo.MasterId);
						}
						//新增BOM
						List<ENGBomInfo> boms = new List<ENGBomInfo>();
						GetBOMInfos(item.BomInfoEntity, item.MaterialId, item.ProcessID, boms);
						var reqbom = ENGBomServiceHelper.TryGetOrAdds(ctx, boms.ToArray());
						item.BomNumber = reqbom.Select(x => x.FNUMBER).FirstOrDefault();
						//分配BOM
						MaterialServiceHelper.MaterialAllocate(ctx, materials);
						ENGBomServiceHelper.BomAllocate(ctx, reqbom.ToList<ENGBomInfo>());
					}
				}
			}
			FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PRD_MO") as FormMetadata;
			var billView = FormMetadataUtils.CreateBillView(ctx, "PRD_MO");
			billView.Model.SetValue("FBillNo", request.BillNo, 0);
			billView.Model.SetValue("FDate", request.Date, 0);
			billView.Model.SetValue("FPENYIsMulti", request.IsMulti, 0);
			//设置五部生产订单不控制领料类型
			billView.Model.SetValue("FBILLTYPE", "6724892b6ed288");
			billView.Model.SetItemValueByNumber("FPrdOrgId", "HDFI", 0);
			billView.InvokeFieldUpdateService("FPrdOrgId", 0);

			billView.Model.DeleteEntryData("FTreeEntity");
			var rowcount = 0;
			foreach (var item in request.DetailEntity)
			{
				billView.Model.CreateNewEntryRow("FTreeEntity");
				billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
				billView.InvokeFieldUpdateService("FMaterialId", rowcount);
				billView.Model.SetItemValueByNumber("FWorkShopID", item.WorkShopID, rowcount);
				billView.Model.SetValue("FQty", item.Qty, rowcount);
				billView.InvokeFieldUpdateService("FQty", rowcount);
				billView.Model.SetValue("FPlanStartDate", item.PlanStartDate, rowcount);
				billView.Model.SetValue("FPlanFinishDate", item.PlanFinishDate, rowcount);
				billView.Model.SetItemValueByNumber("FBomId", item.BomNumber, rowcount);
				billView.Model.SetValue("FCheckProduct", 0);
				//计划需求数量
				billView.Model.SetValue("FPlanDemandQty", item.PlanDemandQty, rowcount);
				//1代表计划下推，2代表手工创建
				if (type == 1)
				{
					billView.Model.SetValue("FREQSRC", 1, rowcount);
					billView.Model.SetValue("FSaleOrderNo", item.SaleOrderNo, rowcount);
					billView.Model.SetValue("FSaleOrderEntrySeq", item.SaleOrderEntrySeq, rowcount);
					billView.Model.SetValue("FSaleOrderId", item.SaleOrderId, rowcount);
					billView.Model.SetValue("FSaleOrderEntryId", item.SaleOrderEntryId, rowcount);

					billView.Model.SetValue("FSrcBillType", "PLN_PLANORDER");
					billView.Model.SetValue("FSrcBillNo", request.PlanBillNo);
					billView.Model.SetValue("FSrcBILLID", request.PlanPkId);
					billView.Model.SetValue("FSrcBillEntryId", request.PlanPkId);
					//明细子单据体
					var subEntity = (SubEntryEntity)billView.BillBusinessInfo.GetEntity("FTREEENTITY_Link");
					var subEntityObjs = billView.Model.GetEntityDataObject(subEntity);
					billView.Model.CreateNewEntryRow("FTREEENTITY_Link");
					subEntityObjs[0]["SBILLID"] = request.PlanPkId;
					subEntityObjs[0]["SID"] = request.PlanPkId;
					subEntityObjs[0]["RULEID"] = "PlanOrder_MO";
					subEntityObjs[0]["STABLENAME"] = "T_PLN_PLANORDER";
					subEntityObjs[0]["BASEUNITQTY"] = item.Qty;
					subEntityObjs[0]["BASEUNITQTYOLD"] = 0;
				}

				rowcount++;
			}
			var oper = SaveBill(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject });
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
			//提交审核
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			SubmitService submitService = new SubmitService();
			var submitResult = submitService.Submit(ctx, billView.BusinessInfo, new string[] { oper.OperateResult.Select(x => x.PKValue).First().ToString() }, "Submit", operateOption);
			if (!submitResult.IsSuccess)
			{
				if (submitResult.ValidationErrors.Count > 0)
				{
					throw new Exception(string.Join(";", submitResult.ValidationErrors.Select(p => p.Message)));
				}
				else
				{
					throw new Exception(string.Join(";", submitResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
				}
			}
			else
			{
				AuditService auditService = new AuditService();
				var auditResult = auditService.Audit(ctx, billView.BusinessInfo, new string[] { oper.OperateResult.Select(x => x.PKValue).First().ToString() }, operateOption);
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
			}

			return oper;
		}

		private List<long> GetOrgidList(Context ctx, long orgId)
		{
			List<long> accountList = new List<long>();
			string sql = $@"/*dialect*/select distinct tt1.FORGID,tt3.FNAME from T_ORG_ORGANIZATIONS tt1
                    inner join (
                   select FSUBORGID OrgID from T_ORG_ACCTSYSDETAIL where FSUBORGID={orgId}
					union
					select FMAINORGID OrgID from T_ORG_ACCTSYSDETAIL t1
					inner join T_ORG_ACCTSYSENTRY t2 on t1.FENTRYID=t2.FENTRYID
					where FSUBORGID={orgId} ) tt2 
                    on  tt1.FORGID=tt2.OrgID
                    inner join T_ORG_ORGANIZATIONS_L tt3 on tt1.FORGID = tt3.FORGID and tt3.FLOCALEID = 2052
                    where tt1.FFORBIDSTATUS='A' ";
			var accountDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in accountDatas)
			{
				accountList.Add(Convert.ToInt64(data["FORGID"]));
			}
			return accountList;
		}
		private void CreateReserve(Context ctx, int type, IOperationResult oper, long plnid)
		{
			foreach (var item in oper.SuccessDataEnity)
			{
				string sSql = $"SELECT FID FROM dbo.T_PLN_RESERVELINKENTRY WHERE FSUPPLYINTERID='{plnid}'";
				var _resid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
				if (_resid > 0)
				{
					var reserveLinkEntry = new ReserveLinkEntry();
					switch (type)
					{
						case 1:
							foreach (var entity in item["ReqEntry"] as DynamicObjectCollection)
							{
								reserveLinkEntry.FSUPPLYFORMID = Convert.ToString(item["FFormId"]);
								reserveLinkEntry.FSUPPLYINTERID = Convert.ToString(item["Id"]);
								reserveLinkEntry.FSUPPLYBILLNO = Convert.ToString(item["BillNo"]);
								reserveLinkEntry.FSUPPLYENTRYID = Convert.ToString(entity["Id"]);
								reserveLinkEntry.FMATERIALID = Convert.ToInt64(entity["MaterialId_Id"]);
								reserveLinkEntry.FSUPPLYORGID = Convert.ToInt64(item["ApplicationOrgId_Id"]);
								reserveLinkEntry.FSUPPLYDATE = Convert.ToDateTime(item["CreateDate"]);
								reserveLinkEntry.FSUPPLYBOMID = Convert.ToInt64(entity["BOMNoId_Id"]);
								reserveLinkEntry.FBASESUPPLYUNITID = Convert.ToInt64(entity["BaseUnitId_Id"]);
								reserveLinkEntry.FBASEQTY = Convert.ToDecimal(entity["RemainQty"]);
								reserveLinkEntry.FINTSUPPLYID = Convert.ToInt64(item["Id"]);
								reserveLinkEntry.FINTSUPPLYENTRYID = Convert.ToInt64(entity["Id"]);
								reserveLinkEntry.FSupplyRemarks = "mes回传erp生成采购预留";
							}
							break;
						case 2:
							foreach (var entity in item["TreeEntity"] as DynamicObjectCollection)
							{
								reserveLinkEntry.FSUPPLYFORMID = Convert.ToString(item["FFormId"]);
								reserveLinkEntry.FSUPPLYINTERID = Convert.ToString(item["Id"]);
								reserveLinkEntry.FSUPPLYBILLNO = Convert.ToString(item["BillNo"]);
								reserveLinkEntry.FSUPPLYENTRYID = Convert.ToString(entity["Id"]);
								reserveLinkEntry.FMATERIALID = Convert.ToInt64(entity["MaterialId_Id"]);
								reserveLinkEntry.FSUPPLYORGID = Convert.ToInt64(item["PrdOrgId_Id"]);
								reserveLinkEntry.FSUPPLYDATE = Convert.ToDateTime(item["CreateDate"]);
								reserveLinkEntry.FSUPPLYBOMID = Convert.ToInt64(entity["BomId_Id"]);
								reserveLinkEntry.FBASESUPPLYUNITID = Convert.ToInt64(entity["BaseUnitId_Id"]);
								reserveLinkEntry.FBASEQTY = Convert.ToDecimal(entity["BaseUnitQty"]);
								reserveLinkEntry.FINTSUPPLYID = Convert.ToInt64(item["Id"]);
								reserveLinkEntry.FINTSUPPLYENTRYID = Convert.ToInt64(entity["Id"]);
								reserveLinkEntry.FLINKTYPE = 2;
								reserveLinkEntry.FSupplyRemarks = "mes回传erp生成生产预留";

							}
							insertReserve(ctx, _resid, reserveLinkEntry);
							break;
					}
				}
				UpdatePln(ctx, _resid, plnid);
				//RecalculationReserve(ctx, plnid, type);
			}
		}
		private void insertReserve(Context ctx, long _resid, ReserveLinkEntry reserveLinkEntry)
		{
			var billView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", _resid);
			billView.Model.CreateNewEntryRow("FEntity");
			var seq = billView.Model.GetEntryRowCount("FEntity") - 1;
			//billView.Model.SetValue("FSUPPLYFORMID", "STK_Inventory", seq);
			//billView.Model.SetItemValueByID("FSUPPLYINTERID", Convert.ToString(item["FSUPPLYINTERID"]), seq);
			billView.Model.SetValue("FSUPPLYFORMID", reserveLinkEntry.FSUPPLYFORMID, seq);
			billView.Model.SetValue("FSUPPLYINTERID", reserveLinkEntry.FSUPPLYINTERID, seq);
			billView.Model.SetValue("FSUPPLYBILLNO", reserveLinkEntry.FSUPPLYBILLNO, seq);
			billView.Model.SetValue("FSUPPLYENTRYID", reserveLinkEntry.FSUPPLYENTRYID, seq);
			billView.Model.SetItemValueByID("FSUPPLYMATERIALID", reserveLinkEntry.FMATERIALID, seq);
			billView.Model.SetItemValueByID("FSUPPLYORGID", reserveLinkEntry.FSUPPLYORGID, seq);
			billView.Model.SetValue("FSUPPLYDATE", reserveLinkEntry.FSUPPLYDATE, seq);
			billView.Model.SetItemValueByID("FSUPPLYSTOCKID", reserveLinkEntry.FSUPPLYSTOCKID, seq);
			billView.Model.SetItemValueByID("FSUPPLYSTOCKLOCID", reserveLinkEntry.FSUPPLYSTOCKLOCID, seq);
			billView.Model.SetItemValueByID("FSUPPLYBOMID", reserveLinkEntry.FSUPPLYBOMID, seq);
			billView.Model.SetValue("FSUPPLYLOT", reserveLinkEntry.FSUPPLYLOT, seq);
			billView.Model.SetItemValueByID("FBASESUPPLYUNITID", reserveLinkEntry.FBASESUPPLYUNITID, seq);
			billView.Model.SetValue("FSTOCKFORMID", reserveLinkEntry.FSTOCKFORMID, seq);
			billView.Model.SetValue("FBASESUPPLYQTY", reserveLinkEntry.FBASEQTY, seq);
			billView.Model.SetValue("FSTOCKINTERID", reserveLinkEntry.FSTOCKINTERID, seq);
			billView.Model.SetValue("FSTOCKENTRYID", reserveLinkEntry.FSTOCKENTRYID, seq);
			billView.Model.SetValue("FSUPPLYPRIORITY", reserveLinkEntry.FSUPPLYPRIORITY, seq);
			billView.Model.SetValue("FISMTO", reserveLinkEntry.FISMTO, seq);
			billView.Model.SetValue("FYieldRate", reserveLinkEntry.FYieldRate, seq);
			billView.Model.SetValue("FLINKTYPE", reserveLinkEntry.FLINKTYPE, seq);
			billView.Model.SetValue("FEntryPkId", reserveLinkEntry.FEntryPkId, seq);
			billView.Model.SetValue("FCONSUMPRIORITY", reserveLinkEntry.FCONSUMPRIORITY, seq);
			billView.Model.SetItemValueByID("FSecUnitId", reserveLinkEntry.FSecUnitId, seq);
			billView.Model.SetValue("FGenerateId", reserveLinkEntry.FGenerateId, seq);
			billView.Model.SetValue("FSecQty", reserveLinkEntry.FSecQty, seq);
			billView.Model.SetValue("FINTSUPPLYID", reserveLinkEntry.FINTSUPPLYID, seq);
			billView.Model.SetValue("FINTSUPPLYENTRYID", reserveLinkEntry.FINTSUPPLYENTRYID, seq);
			billView.Model.SetValue("FReserveDate", reserveLinkEntry.FReserveDate, seq);
			billView.Model.SetValue("FReserveDays", reserveLinkEntry.FReserveDays, seq);
			billView.Model.SetValue("FReleaseDate", reserveLinkEntry.FReleaseDate, seq);
			billView.Model.SetValue("FSupplyRemarks", reserveLinkEntry.FSupplyRemarks, seq);

			var reoper = SaveBill(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject });
		}
		private void UpdatePln(Context ctx, long _resid, long plnid)
		{
			//取计划订单剩余数量
			var sSql = $"SELECT FBASEDEMANDQTY FROM dbo.T_PLN_PLANORDER WHERE FID={plnid}";
			var demandQty = DBServiceHelper.ExecuteScalar<decimal>(ctx, sSql, 0);
			sSql = $"SELECT FBILLNO FROM dbo.T_PLN_PLANORDER WHERE FID={plnid}";
			var plnBillNo = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "");
			//取计划订单已投放总数量
			sSql = $@"SELECT SUM(FREMAINQTY) AS FREMAINQTY FROM 
            (
            SELECT t2.FREQQTY AS FREMAINQTY FROM dbo.T_PUR_ReqEntry_R t1
            INNER JOIN T_PUR_ReqEntry t2 ON t1.FENTRYID=t2.FENTRYID
            WHERE FSRCBILLTYPEID='PLN_PLANORDER' AND FSRCBILLNO='{plnBillNo}'
            UNION ALL
            SELECT FQTY FROM T_PRD_MOENTRY
            WHERE FSRCBILLTYPE='PLN_PLANORDER' AND FSRCBILLNO='{plnBillNo}'
            ) t1";
			var baseQty = DBServiceHelper.ExecuteScalar<decimal>(ctx, sSql, 0);

			var plnBillView = FormMetadataUtils.CreateBillView(ctx, "PLN_PLANORDER", plnid);
			plnBillView.Model.SetValue("FFirmQty", demandQty - baseQty);
			plnBillView.Model.SetValue("FBaseFirmQty", demandQty - baseQty);
			//获取计划订单信息防止丢失
			var reserveLinkEntry = new ReserveLinkEntry();
			reserveLinkEntry.FSUPPLYFORMID = Convert.ToString(plnBillView.Model.DataObject["FFormId"]);
			reserveLinkEntry.FSUPPLYINTERID = Convert.ToString(plnBillView.Model.DataObject["Id"]);
			reserveLinkEntry.FSUPPLYBILLNO = Convert.ToString(plnBillView.Model.DataObject["BillNo"]);
			//reserveLinkEntry.FSUPPLYENTRYID = Convert.ToString(plnBillView.Model.DataObject["Id"]);
			reserveLinkEntry.FMATERIALID = Convert.ToInt64(plnBillView.Model.DataObject["MaterialId_Id"]);
			reserveLinkEntry.FSUPPLYORGID = Convert.ToInt64(plnBillView.Model.DataObject["SupplyOrgId_Id"]);
			reserveLinkEntry.FSUPPLYDATE = Convert.ToDateTime(plnBillView.Model.DataObject["CreateDate"]);
			reserveLinkEntry.FSUPPLYBOMID = Convert.ToInt64(plnBillView.Model.DataObject["BomId_Id"]);
			reserveLinkEntry.FBASESUPPLYUNITID = Convert.ToInt64(plnBillView.Model.DataObject["BaseUnitId_Id"]);
			reserveLinkEntry.FBASEQTY = Convert.ToDecimal(plnBillView.Model.DataObject["FirmQty"]);
			reserveLinkEntry.FINTSUPPLYID = Convert.ToInt64(plnBillView.Model.DataObject["Id"]);
			//reserveLinkEntry.FINTSUPPLYENTRYID = Convert.ToInt64(plnBillView.Model.DataObject["Id"]);
			reserveLinkEntry.FSupplyRemarks = "计划保存丢失预留回写";
			SaveBill(ctx, plnBillView.BusinessInfo, new DynamicObject[] { plnBillView.Model.DataObject });
			if (demandQty - baseQty <= 0)
			{
				var operateOption = OperateOption.Create();
				operateOption.SetIgnoreWarning(true);
				object[] ids = new object[] { plnid };
				List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
				SetStatusService setStatusService = new SetStatusService();
				var oper = setStatusService.SetBillStatus(ctx, plnBillView.BusinessInfo, pkEntryIds, null, "HandClose", operateOption);
			}
			//清除释放网控
			plnBillView.CommitNetworkCtrl();
			plnBillView.InvokeFormOperation(FormOperationEnum.Close);
			plnBillView.Close();
			sSql = $"/*dialect*/UPDATE T_PLN_PLANORDER_B SET FISMRP=1,FISMRPCAL=1 WHERE FID={plnid}";
			DBServiceHelper.Execute(ctx, sSql);
			//分多次排产重新挂上原计划订单预留
			if (_resid > 0)
			{
				sSql = $"SELECT FENTRYID FROM T_PLN_RESERVELINKENTRY WHERE FID={_resid} AND FSUPPLYINTERID='{plnid}'";
				var plndo = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
				if (plndo.Count <= 0)
				{
					insertReserve(ctx, _resid, reserveLinkEntry);
				}
			}
			sSql = $@"/*dialect*/UPDATE T_PLN_RESERVELINKENTRY SET FBASEQTY={demandQty - baseQty}
            WHERE FSUPPLYINTERID='{plnid}'";
			DBServiceHelper.Execute(ctx, sSql);
		}
		private long GetUnitId(Context ctx, string number)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UnitId", KDDbType.String, number) };
			var sql = $@"/*dialect*/SELECT FUNITID FROM dbo.T_BD_UNIT WHERE FNUMBER=@UnitId";
			return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
		}
		private long GetSMALLID(Context ctx, string Number)
		{
			string sSql = $"SELECT FID FROM dbo.T_BD_MATERIALGROUP WHERE FNUMBER='{Number}'";
			return DBServiceHelper.ExecuteScalar(ctx, sSql, 0);
		}
		private IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
		{
			SaveService saveService = new SaveService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			operationResult = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);

			return operationResult;
		}
		private IOperationResult SaveAndAuditBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
		{
			SaveService saveService = new SaveService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			operationResult = saveService.SaveAndAudit(ctx, businessInfo, dynamicObjects, operateOption);
			return operationResult;
		}
		private IOperationResult SaveAndAuditBillWorkflows(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
		{
			SaveService saveService = new SaveService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			operationResult = saveService.SaveAndAudit(ctx, businessInfo, dynamicObjects, operateOption);
			return operationResult;
		}
		private List<ENGBomInfo> GetBOMInfos(List<BomInfoEntity> bomentity, string parent, string processid, List<ENGBomInfo> bomlist)
		{
			ENGBomInfo bom = new ENGBomInfo(parent);
			bom.FMATERIALID = parent;
			bom.ProcessID = processid;
			var dataLines = bomentity.Where(o => o.ParentSpecification.ToString().Equals(parent)).ToList();
			if (dataLines.Count > 0)
			{
				List<BomEntity> entitylist = new List<BomEntity>();
				foreach (var item in dataLines)
				{
					BomEntity ent = new BomEntity();
					ent.FMATERIALIDCHILD = item.BomMaterialID;
					ent.FNUMERATOR = item.mtlQty;
					ent.FDENOMINATOR = item.denominatorQty;
					ent.FUnitNumber = item.VolumeUnitid;
					ent.FSCRAPRATE = 0;
					ent.SendMes = true;
					ent.IsSueType = true;
					ent.EntryNote = item.EntryNote;
					GetBOMInfos(bomentity, Convert.ToString(item.BomMaterialID), processid, bomlist);
					entitylist.Add(ent);
				}
				bom.Entity = entitylist;
				bomlist.Add(bom);
			}
			return bomlist;
		}
		public int GetOrgIdByOrgNumber(Context ctx, string orgNumber)
		{
			var sql = $@" select top 1 FORGID from t_org_organizations where FNUMBER='{orgNumber}' ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			if (datas.Count == 0)
			{
				return 1;
			}
			else
			{
				return Convert.ToInt32(datas[0]["FORGID"]);
			}
		}

		public ResponseMessage<PlanOrderSplitEntity> mes_SyncOrderMaterialService(Context ctx, PlanOrderSplitEntity request)
		{
			ResponseMessage<PlanOrderSplitEntity> response = new ResponseMessage<PlanOrderSplitEntity>() { Data = request };
			var orgid = GetOrgIdByOrgNumber(ctx, request.ApplicationOrgId);
			switch (request.ReleaseType)
			{
				case 1:
					//foreach (var item in request.DetailEntity)
					//{
					//    billView.Model.CreateNewEntryRow("FEntity");
					//    billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
					//}
					break;
				case 2:
					foreach (var item in request.DetailEntity)
					{
						try
						{
							foreach (var bomitem in item.BomInfoEntity)
							{
								var materialCode = bomitem.BomMaterialID;
								var materialName = bomitem.FBBomName;
								var materialInfo = new MaterialInfo(bomitem.BomMaterialID, bomitem.FBBomName);

								string baseUnit = "";
								string storeUnit = "";
								string purchaseUnit = "";
								string saleUnit = "";
								switch (bomitem.VolumeUnitid.ToUpper())
								{
									case "CM":
										baseUnit = "m";
										storeUnit = "cm";
										purchaseUnit = "cm";
										saleUnit = "cm";
										break;
									case "MM":
										baseUnit = "m";
										storeUnit = "mm";
										purchaseUnit = "mm";
										saleUnit = "mm";
										break;
									default:
										baseUnit = bomitem.VolumeUnitid;
										storeUnit = bomitem.VolumeUnitid;
										purchaseUnit = bomitem.VolumeUnitid;
										saleUnit = bomitem.VolumeUnitid;
										break;
								}
								materialInfo.FBaseUnitId = baseUnit;
								materialInfo.FStoreUnitID = storeUnit;
								materialInfo.FPurchaseUnitId = purchaseUnit;
								materialInfo.FPurchasePriceUnitId = purchaseUnit;
								materialInfo.FSaleUnitId = saleUnit;

								materialInfo.Textures = bomitem.Textures;
								materialInfo.MaterialType = bomitem.MaterialType;
								materialInfo.Weight = bomitem.Weight;
								materialInfo.Length = bomitem.Length;
								materialInfo.Width = bomitem.Width;
								materialInfo.Height = bomitem.Height;
								materialInfo.Volume = bomitem.VOLUME;

								materialInfo.WeightUnitid = Convert.ToString(GetUnitId(ctx, bomitem.WeightUnitid));
								materialInfo.VolumeUnitid = Convert.ToString(GetUnitId(ctx, bomitem.VolumeUnitid));
								SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
								productsmallclass.Id = GetSMALLID(ctx, bomitem.SMALLID);//华东-原材料
								materialInfo.ProductSmallClass = productsmallclass;
								var material = MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, new List<long>() { orgid });
								bomitem.ChildMaterialID = material.MasterId;
							}
						}
						catch (Exception err)
						{
							if (err.ToString().Contains("相同编码"))
							{
								//continue;
							}
							else
							{
								throw new Exception(err.ToString());
							}
						}
						var sSql = $@"SELECT FNUMBER FROM dbo.T_ENG_BOM WHERE FPENYPROCESSID='{item.ProcessID}' ORDER BY FID DESC";
						item.BomNumber = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "");
						if (string.IsNullOrEmpty(item.BomNumber))
						{
							//新增BOM
							List<ENGBomInfo> boms = new List<ENGBomInfo>();
							GetBOMInfos(item.BomInfoEntity, item.MaterialId, item.ProcessID, boms);
							var reqbom = ENGBomServiceHelper.TryGetOrAddsOrg(ctx, boms.ToArray(), new long[] { orgid });
							item.BomNumber = reqbom.Select(x => x.FNUMBER).FirstOrDefault();
							item.BomId = reqbom.Select(x => x.MasterId).FirstOrDefault();
						}
						//分配BOM
						//MaterialServiceHelper.MaterialAllocate(ctx, materials);
						//ENGBomServiceHelper.BomAllocate(ctx, reqbom.ToList<ENGBomInfo>());
					}
					break;
			}

			response.Code = ResponseCode.Success;
			return response;
		}
		public ResponseMessage<PlanOrderSplitEntity> mes_SyncOrderService(Context ctx, PlanOrderSplitEntity request)
		{
			ResponseMessage<PlanOrderSplitEntity> response = new ResponseMessage<PlanOrderSplitEntity>() { Data = request };
			FormMetadata meta = new FormMetadata();
			IBillView billView = null;
			switch (request.ReleaseType)
			{
				case 1:
					if (DBServiceHelper.ExecuteScalar<bool>(ctx, "select 1 from T_PUR_Requisition where FBILLNO = @FBILLNO", false, new SqlParam("@FBILLNO", KDDbType.String, request.BillNo)))
					{
						throw new Exception("采购订单号已经存在！");
					}
					meta = MetaDataServiceHelper.Load(ctx, "PUR_Requisition") as FormMetadata;
					billView = FormMetadataUtils.CreateBillView(ctx, "PUR_Requisition");
					//billView.Model.SetValue("FBillTypeID", bill.FBillTypeID, 0);
					billView.Model.SetValue("FBillNo", request.BillNo, 0);
					billView.Model.SetValue("FApplicationDate", request.Date, 0);
					billView.Model.SetValue("FPENYBackupBillNO", request.BillNo, 0);
					billView.Model.SetItemValueByNumber("FApplicationOrgId", request.ApplicationOrgId, 0);

					billView.Model.DeleteEntryData("FEntity");
					var rowcount = 0;
					foreach (var item in request.DetailEntity)
					{
						billView.Model.CreateNewEntryRow("FEntity");
						billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
						billView.InvokeFieldUpdateService("FMaterialId", rowcount);
						billView.Model.SetValue("FReqQty", item.Qty, rowcount);
						billView.InvokeFieldUpdateService("FReqQty", rowcount);

						billView.Model.SetValue("FSrcBillTypeId", "PLN_PLANORDER");
						billView.Model.SetValue("FSrcBillNo", request.PlanBillNo);
						//销售信息
						var sSql = $@"/*dialect*/SELECT DISTINCT t6.FSALERID,t7.FNAME FROM T_PLN_PLANORDER_B t1
                                    LEFT JOIN T_SAL_ORDER t6 ON t1.FSALEORDERID=t6.FID
                                    LEFT JOIN V_BD_SALESMAN_L t7 ON t6.FSALERID=t7.fid
                                    WHERE t6.FBILLNO='@FBILLNO'";
						List<SqlParam> pars = new List<SqlParam>();
						pars.Add(new SqlParam("@FBILLNO", KDDbType.String, item.SaleOrderNo));
						var data = DBServiceHelper.ExecuteDynamicObject(ctx, sSql, paramList: pars.ToArray());
						var salUser = string.Join(",", data.Select(x => x["FNAME"]).Distinct());

						billView.Model.SetValue("FSoNo", item.SaleOrderNo);
						billView.Model.SetValue("FSoSeq", item.SaleOrderEntrySeq);
						billView.Model.SetValue("FSoUnitPrice", item.PenyPrice);
						billView.Model.SetValue("FPENYDELIVERYDATE", item.PenySalDatetime);
						billView.Model.SetValue("FPENYSALERS", salUser);
						billView.Model.SetValue("FDEMANDTYPE", 1);
						billView.Model.SetValue("FDEMANDBILLNO", item.SaleOrderNo);
						billView.Model.SetValue("FDEMANDBILLENTRYSEQ", item.SaleOrderEntrySeq);
						billView.Model.SetValue("FDEMANDBILLENTRYID", item.SaleOrderEntryId);
						//关联关系
						//明细子单据体
						var subEntity = (SubEntryEntity)billView.BillBusinessInfo.GetEntity("FENTITY_Link");
						var subEntityObjs = billView.Model.GetEntityDataObject(subEntity);
						billView.Model.CreateNewEntryRow("FENTITY_Link");
						subEntityObjs[0]["SBILLID"] = request.PlanPkId;
						subEntityObjs[0]["SID"] = request.PlanPkId;
						subEntityObjs[0]["RULEID"] = "PlanOrder_PurRequest";
						subEntityObjs[0]["STABLENAME"] = "T_PLN_PLANORDER";
						subEntityObjs[0]["REQSTOCKBASEQTY"] = item.Qty;
						subEntityObjs[0]["REQSTOCKBASEQTYOLD"] = 0;

						rowcount++;
					}
					break;
				case 2:
					meta = MetaDataServiceHelper.Load(ctx, "PRD_MO") as FormMetadata;
					billView = FormMetadataUtils.CreateBillView(ctx, "PRD_MO");
					billView.Model.SetValue("FBillNo", request.BillNo, 0);
					billView.Model.SetValue("FDate", request.Date, 0);
					billView.Model.SetValue("FPENYIsMulti", request.IsMulti, 0);
					if (request.ApplicationOrgId == "HDFI")
					{
						//设置五部生产订单不控制领料类型
						billView.Model.SetValue("FBILLTYPE", "6724892b6ed288");
					}
					billView.Model.SetItemValueByNumber("FPrdOrgId", request.ApplicationOrgId, 0);

					billView.InvokeFieldUpdateService("FPrdOrgId", 0);

					billView.Model.DeleteEntryData("FTreeEntity");
					rowcount = 0;
					foreach (var item in request.DetailEntity)
					{
						billView.Model.CreateNewEntryRow("FTreeEntity");
						billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
						billView.InvokeFieldUpdateService("FMaterialId", rowcount);
						billView.Model.SetItemValueByNumber("FWorkShopID", item.WorkShopID, rowcount);
						billView.Model.SetValue("FQty", item.Qty, rowcount);
						billView.InvokeFieldUpdateService("FQty", rowcount);
						billView.Model.SetValue("FPlanStartDate", item.PlanStartDate, rowcount);
						billView.Model.SetValue("FPlanFinishDate", item.PlanFinishDate, rowcount);
						billView.Model.SetItemValueByNumber("FBomId", item.BomNumber, rowcount);
						billView.Model.SetValue("FCheckProduct", 0);
						//计划需求数量
						billView.Model.SetValue("FPlanDemandQty", item.PlanDemandQty, rowcount);

						billView.Model.SetValue("FREQSRC", 1, rowcount);
						billView.Model.SetValue("FSaleOrderNo", item.SaleOrderNo, rowcount);
						billView.Model.SetValue("FSaleOrderEntrySeq", item.SaleOrderEntrySeq, rowcount);
						billView.Model.SetValue("FSaleOrderId", item.SaleOrderId, rowcount);
						billView.Model.SetValue("FSaleOrderEntryId", item.SaleOrderEntryId, rowcount);

						billView.Model.SetValue("FSrcBillType", "PLN_PLANORDER");
						billView.Model.SetValue("FSrcBillNo", request.PlanBillNo);
						billView.Model.SetValue("FSrcBILLID", request.PlanPkId);
						billView.Model.SetValue("FSrcBillEntryId", request.PlanPkId);
						//明细子单据体
						var subEntity = (SubEntryEntity)billView.BillBusinessInfo.GetEntity("FTREEENTITY_Link");
						var subEntityObjs = billView.Model.GetEntityDataObject(subEntity);
						billView.Model.CreateNewEntryRow("FTREEENTITY_Link");
						subEntityObjs[0]["SBILLID"] = request.PlanPkId;
						subEntityObjs[0]["SID"] = request.PlanPkId;
						subEntityObjs[0]["RULEID"] = "PlanOrder_MO";
						subEntityObjs[0]["STABLENAME"] = "T_PLN_PLANORDER";
						subEntityObjs[0]["BASEUNITQTY"] = item.Qty;
						subEntityObjs[0]["BASEUNITQTYOLD"] = 0;

						rowcount++;
					}
					break;
			}


			var oper = SaveBill(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject });
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
			//提交审核
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			SubmitService submitService = new SubmitService();
			var submitResult = submitService.Submit(ctx, billView.BusinessInfo, new string[] { oper.OperateResult.Select(x => x.PKValue).First().ToString() }, "Submit", operateOption);
			if (!submitResult.IsSuccess)
			{
				if (submitResult.ValidationErrors.Count > 0)
				{
					throw new Exception(string.Join(";", submitResult.ValidationErrors.Select(p => p.Message)));
				}
				else
				{
					throw new Exception(string.Join(";", submitResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
				}
			}
			else
			{
				AuditService auditService = new AuditService();
				var auditResult = auditService.Audit(ctx, billView.BusinessInfo, new string[] { oper.OperateResult.Select(x => x.PKValue).First().ToString() }, operateOption);
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
			}

			response.Code = ResponseCode.Success;
			try
			{
				CreateReserve(ctx, request.ReleaseType, oper, request.PlanPkId);
			}
			catch (Exception err)
			{
				response.Code = ResponseCode.Warning;
				response.ErrorMessage = err.ToString();
			}

			return response;
		}
		public ResponseMessage<PlanOrderSplitEntity> mes_AllocateMaterialAllService(Context ctx, PlanOrderSplitEntity request)
		{
			ResponseMessage<PlanOrderSplitEntity> response = new ResponseMessage<PlanOrderSplitEntity>() { Data = request };
			switch (request.ReleaseType)
			{
				case 1:
					//foreach (var item in request.DetailEntity)
					//{
					//    billView.Model.CreateNewEntryRow("FEntity");
					//    billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
					//}
					break;
				case 2:
					foreach (var item in request.DetailEntity)
					{
						List<long> longs = new List<long>();
						foreach (var bomitem in item.BomInfoEntity)
						{
							longs.Add(bomitem.ChildMaterialID);
						}
						//分配BOM
						MaterialServiceHelper.MaterialAllocate(ctx, longs);
						//ENGBomServiceHelper.BomAllocate(ctx, reqbom.ToList<ENGBomInfo>());
					}
					break;
			}

			response.Code = ResponseCode.Success;
			return response;
		}
		public ResponseMessage<PlanOrderSplitEntity> mes_AllocateBomAllService(Context ctx, PlanOrderSplitEntity request)
		{
			ResponseMessage<PlanOrderSplitEntity> response = new ResponseMessage<PlanOrderSplitEntity>() { Data = request };
			switch (request.ReleaseType)
			{
				case 1:
					//foreach (var item in request.DetailEntity)
					//{
					//    billView.Model.CreateNewEntryRow("FEntity");
					//    billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialId, rowcount);
					//}
					break;
				case 2:
					List<long> longs = new List<long>();
					foreach (var item in request.DetailEntity)
					{
						longs.Add(item.BomId);
						//分配BOM
						//MaterialServiceHelper.MaterialAllocate(ctx, longs);
						ENGBomServiceHelper.BomAllocateByID(ctx, longs.ToArray());
					}
					break;
			}

			response.Code = ResponseCode.Success;
			return response;
		}
	}
}
