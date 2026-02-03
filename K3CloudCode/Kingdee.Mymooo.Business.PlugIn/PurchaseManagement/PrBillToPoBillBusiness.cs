using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.Excel;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using UserServiceHelper = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
	[Description("采购申请转采购订单单据插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class PrBillToPoBillBusiness : AbstractDynamicFormPlugIn
	{
		//左侧供应商和小类信息
		List<PrToPoLeftSupplierEntity> supplierEntities = new List<PrToPoLeftSupplierEntity>();

		//右侧Pr转Po需求单信息
		List<PrToPoPurchaseRequireEntity> prToPoPurchaseRequireEntities = new List<PrToPoPurchaseRequireEntity>();
		string wxUserCode = "";
		long orgId = 0;
		string orgCode = "";
		/// <summary>
		/// 初始化事件
		/// </summary>
		/// <param name="e"></param>
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		/// <summary>
		/// 数据绑定完成后
		/// </summary>
		/// <param name="e"></param>
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);

			wxUserCode = UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
			if (string.IsNullOrWhiteSpace(wxUserCode))
			{
				this.View.ShowErrMessage("请先配置您的微信Code");
				return;
			}
			orgId = this.Context.CurrentOrganizationInfo.ID;
			orgCode = GetOrgCode();
			//获取Pr转Po左侧供应商信息
			var list = GetLeftSupplierList("");
			supplierEntities = list;
			for (int i = 0; i < list.Count; i++)
			{
				this.Model.CreateNewEntryRow("FLEntity");
				this.Model.SetValue("FLSupplierCode", list[i].SupplierCode, i);
				this.Model.SetValue("FLProductSmallClassName", list[i].ProductSmallClassName, i);
				this.Model.SetValue("FLSupplierName", list[i].SupplierName, i);
			}
			this.View.UpdateView("FLEntity");
			// 获取Pr转Po右侧需求单信息
			prToPoPurchaseRequireEntities = GetPurchaseRequireModelList(this.Context);

			//绑定右侧Pr转Po需求单信息
			BDPurchaseRequireModel(prToPoPurchaseRequireEntities);
			this.View.GetMainBarItem("BtnMatchPrice").Enabled = false;
			//华南三部和华东三部才显示
			if (orgId.Equals(7639512) || orgId.Equals(8166435))
			{
				this.View.GetControl("FRawMaterialPrice").Visible = true;
				this.View.GetControl("FProcessFee").Visible = true;
			}
			else
			{
				this.View.GetControl("FRawMaterialPrice").Visible = false;
				this.View.GetControl("FProcessFee").Visible = false;
			}
		}

		//按钮点击事件
		public override void AfterButtonClick(AfterButtonClickEventArgs e)
		{
			base.AfterButtonClick(e);
			//左侧供应商和小类信息
			if (e.Key.ToUpper().Equals("FLB_SERACH"))
			{
				this.Model.DeleteEntryData("FLEntity");
				var serachFilter = Convert.ToString(this.View.Model.GetValue("FLS_SupplierCode"));
				var list = supplierEntities;
				if (!string.IsNullOrEmpty(serachFilter))
				{
					list = supplierEntities.Where(x => (!string.IsNullOrWhiteSpace(x.ProductSmallClassName) && x.ProductSmallClassName.Contains(serachFilter)) || (!string.IsNullOrWhiteSpace(x.SupplierName) && x.SupplierName.Contains(serachFilter))).ToList();
				}
				for (int i = 0; i < list.Count; i++)
				{
					this.Model.CreateNewEntryRow("FLEntity");
					this.Model.SetValue("FLSupplierCode", list[i].SupplierCode, i);
					this.Model.SetValue("FLProductSmallClassName", list[i].ProductSmallClassName, i);
					this.Model.SetValue("FLSupplierName", list[i].SupplierName, i);
				}
				this.View.UpdateView("FLEntity");
			}
			else if (e.Key.ToUpper().Equals("FRB_SERACH"))//右侧Pr转Po需求单信息
			{
				// 保存采购需求单临时数据
				SavePrTempData();
				this.Model.DeleteEntryData("FREntity");
				//绑定右侧Pr转Po需求单信息

				var list = prToPoPurchaseRequireEntities;
				list = list.OrderByDescending(x => x.Selected).ToList();
				BDPurchaseRequireModel(list);
			}
		}

		/// <summary>
		/// 绑定右侧Pr转Po需求单信息
		/// </summary>
		public void BDPurchaseRequireModel(List<PrToPoPurchaseRequireEntity> list)
		{
			var serachSmallClass = Convert.ToString(this.View.Model.GetValue("FRS_SmallClass"));
			var serachProduct = Convert.ToString(this.View.Model.GetValue("FRS_Product"));
			var serachSupplierName = Convert.ToString(this.View.Model.GetValue("FRS_SupplierName"));
			if (!string.IsNullOrWhiteSpace(serachSmallClass))
			{
				list = list.Where(x => (!string.IsNullOrWhiteSpace(x.SmallClass) && x.SmallClass.Contains(serachSmallClass))).ToList();
			}
			if (!string.IsNullOrWhiteSpace(serachProduct))
			{
				list = list.Where(x => (!string.IsNullOrWhiteSpace(x.ItemNo) && x.ItemNo.Contains(serachProduct)) || (!string.IsNullOrWhiteSpace(x.ItemDesc) && x.ItemDesc.Contains(serachProduct))).ToList();
			}
			if (!string.IsNullOrWhiteSpace(serachSupplierName))
			{
				list = list.Where(x => (!string.IsNullOrWhiteSpace(x.VendorName) && x.VendorName.Contains(serachSupplierName)) || (!string.IsNullOrWhiteSpace(x.VendorCode) && x.ItemDesc.Contains(serachProduct))).ToList();
			}

			var entity = this.View.BusinessInfo.GetEntity("FREntity");
			for (int i = 0; i < list.Count; i++)
			{
				this.Model.CreateNewEntryRow("FREntity");
				var entityObject = this.View.Model.GetEntityDataObject(entity, i);
				this.Model.SetValue("F_CheckBoxPrNo", list[i].Selected, i);
				this.Model.SetValue("FPrNo", list[i].PrNo, i);
				this.Model.SetValue("FSEQ", list[i].PrSeq, i);
				this.Model.SetValue("FSmallClass", list[i].SmallClass, i);
				this.Model.SetValue("FItemBrand", list[i].ItemBrand, i);
				this.Model.SetValue("FItemID", list[i].ItemID, i);
				this.Model.SetValue("FItemNo", list[i].ItemNo, i);
				this.Model.SetValue("FItemDesc", list[i].ItemDesc, i);
				this.Model.SetValue("FQty", list[i].Qty, i);
				this.Model.SetValue("FUomId", list[i].UomId, i);
				//this.Model.SetValue("FPurchaseNum", list[i].PurchaseNum, i);
				entityObject["FPurchaseNum"] = list[i].PurchaseNum;
				this.Model.SetValue("FVendorId", list[i].VendorId, i);
				this.Model.SetValue("FPrice", list[i].Price, i);
				this.Model.SetValue("FMinOrderQuantity", list[i].MinOrderQuantity, i);
				this.Model.SetValue("FRequiredDeliveryDate", list[i].RequiredDeliveryDate, i);
				this.Model.SetValue("FRemark", list[i].Remark, i);
				this.Model.SetValue("FSoActualDate", list[i].SoActualDate, i);
				this.Model.SetValue("FSoRtd", list[i].SoRtd, i);
				this.Model.SetValue("FSoNo", list[i].SoNo, i);
				this.Model.SetValue("FSoSeq", list[i].SoSeq, i);
				this.Model.SetValue("FId", list[i].FId, i);
				this.Model.SetValue("FentryId", list[i].FentryId, i);
				this.Model.SetValue("FRawMaterialPrice", list[i].RawMaterialPrice, i);
				this.Model.SetValue("FProcessFee", list[i].ProcessFee, i);
				this.Model.SetValue("FWeightRate", list[i].WeightRate, i);
				this.Model.SetValue("FPriceSource", list[i].PriceSource, i);
				this.Model.SetValue("FSUGGESTSUPPLIERID", list[i].SuggestSupplierId, i);
				this.Model.SetValue("FSuggestSupplierPrice", list[i].SuggestSupplierPrice, i);
				this.Model.SetValue("FBILLTYPENAME", list[i].BillTypeName, i);
				this.Model.SetValue("FSupplierProductCode", list[i].SupplierProductCode, i);
			}
			this.View.UpdateView("FREntity");

		}


		/// <summary>
		/// 值更新事件
		/// </summary>
		/// <param name="e"></param>
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);

			if (e.Field.Key == "FPurchaseNum")
			{
				this.View.ShowErrMessage("您修改了采购数量，请重新匹配供应商和价格");
				return;
			}
		}

		/// <summary>
		/// 获取Pr转Po左侧供应商信息
		/// </summary>
		/// <param name="serachFilter"></param>
		/// <returns></returns>
		public List<PrToPoLeftSupplierEntity> GetLeftSupplierList(string serachFilter)
		{
			try
			{
				List<PrToPoLeftSupplierEntity> list = new List<PrToPoLeftSupplierEntity>();
				list.Add(new PrToPoLeftSupplierEntity
				{
					SupplierCode = "",
					SupplierName = "所有供应商",
					ProductSmallClassName = "所有小类"
				});
				bool getAll = true;
				if (string.IsNullOrWhiteSpace(wxUserCode))
				{
					return list;
				}
				var roles = GetUserRoles(wxUserCode);
				if (!roles.Contains("purchasermanager") && !roles.Contains("admin")) // 采购主管或管理员看全部的
				{
					getAll = false;
				}
				//获取采购员和其有权限的用户
				string url = $"workbench/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Privilege/GetAuthorityUserList";
				var getAuthorityUserComm = ApigatewayUtils.InvokePostWebService(url, "", userCode: wxUserCode);
				var getAuthorityUserResult = JsonConvertUtils.DeserializeObject<ResponseMessage<AuthorityUser[]>>(getAuthorityUserComm);

				List<long> userList = new List<long>();
				if (getAuthorityUserResult != null && getAuthorityUserResult.Code == "success")
				{
					foreach (var item in getAuthorityUserResult.Data)
					{
						userList.Add(item.UserId);
					}
				}
				var data = new
				{
					UserList = userList,
					CompanyCode = orgCode,
					IsGetAll = getAll,
					DataFilter = serachFilter
				};
				var requestData = JsonConvertUtils.SerializeObject(data);
				//获取有权限的供应商列表
				string scmUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetAuthoritySupplierList";
				var resultJson = ApigatewayUtils.InvokePostWebService(scmUrl, requestData, userCode: wxUserCode);
				var GetPscResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<PrToPoLeftSupplierEntity>>>(resultJson);
				if (GetPscResult != null && GetPscResult.Data != null)
				{
					list.AddRange(GetPscResult.Data);
				}

				return list;
			}
			catch (Exception ex)
			{

				throw new Exception(ex.Message);
			}
		}

		/// <summary>
		/// 获取Pr转Po右侧需求单信息
		/// </summary>
		/// <param name="serachFilter"></param>
		/// <returns></returns>
		public List<PrToPoPurchaseRequireEntity> GetPurchaseRequireModelList(Context ctx)
		{

			List<PrToPoPurchaseRequireEntity> list = new List<PrToPoPurchaseRequireEntity>();
			PrToPoAuthoritySetting permission = new PrToPoAuthoritySetting();
			bool getAll = true;
			if (string.IsNullOrWhiteSpace(wxUserCode))
			{
				return list;
			}
			string modelFilter = "";
			var roles = GetUserRoles(wxUserCode);
			if (!roles.Contains("purchasermanager") && !roles.Contains("admin"))
			{
				getAll = false;
				permission = GetSupplierAndModelPermission(wxUserCode);
				if (permission != null && !string.IsNullOrWhiteSpace(permission.SmallClassStr))
				{
					modelFilter = permission.SmallClassStr;
				}
			}

			var sqlAdd = "";
			if (!getAll)
			{
				sqlAdd = " and reqDet.FSMALLID = 0 ";
				if (!string.IsNullOrWhiteSpace(modelFilter))
				{
					sqlAdd = $" and (reqDet.FSMALLID = 0 or reqDet.FSMALLID in ('{modelFilter}')  ) ";
				}

			}
			var sql = $@"/*dialect*/select req.FID,reqDet.FENTRYID,req.FBILLTYPEID,typel.FNAME FBILLTYPENAME,req.FBILLNO,reqDet.FSEQ,mat.FMATERIALID,
                    mat.FNUMBER,matL.FNAME ItemDesc,reqDet.FSMALLID SmallClassId,gl.FNAME SmallClass,reqDet.FREQQTY,reqDet.FUNITID UomId,
                    reqDet.FENTRYNOTE,reqDet.FSupplierProductCode,
					case when isnull(reqDetR.FDEMANDBILLNO,'')<>'' then reqDetR.FDEMANDBILLNO  else so.FSALEORDERNO end FDEMANDBILLNO,
					case when isnull(reqDetR.FDEMANDBILLENTRYSEQ,'')<>'' then reqDetR.FDEMANDBILLENTRYSEQ  else so.FSALEORDERENTRYSEQ end FDEMANDBILLENTRYSEQ,
					so.FDATE,so.FFIRMFINISHDATE FDELIVERYDATE,
                    reqDet.FSUGGESTSUPPLIERID,reqDet.FSUPPLIERUNITPRICE,
                    (select top 1 t2.FDAY from T_ENG_WORKCAL t1
					inner join T_ENG_WORKCALDATA t2 on t1.FID=t2.FID
					where t1.FFORMID='ENG_WorkCal' and t1.FUSEORGID=req.FAPPLICATIONORGID and t2.FISWORKTIME=1 and t2.FDAY<=DATEADD(DAY,-1,reqDet.FARRIVALDATE)
					order by t2.FDAY desc) RequiredDeliveryDate
                    from T_PUR_Requisition req
                    inner join T_PUR_ReqEntry reqDet on req.FID=reqDet.FID
                    inner join T_PUR_ReqEntry_S reqDetS on reqDet.FENTRYID=reqDetS.FENTRYID
                    inner join T_PUR_REQENTRY_R reqDetR on reqDet.FENTRYID=reqDetR.FENTRYID
                    inner join T_BD_MATERIAL mat on reqDet.FMATERIALID=mat.FMATERIALID
                    inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = {ctx.UserLocale.LCID}
					left join T_BD_MATERIALGROUP_L gl on reqDet.FSMALLID = gl.FID and gl.FLOCALEID = {ctx.UserLocale.LCID}
                    left join T_BAS_BILLTYPE_L typel on req.FBILLTYPEID=typel.FBILLTYPEID and typel.FLOCALEID = {ctx.UserLocale.LCID}
                    left join (
						select distinct t1.FSALEORDERNO,t1.FSALEORDERENTRYSEQ,t1.FDATE,t1.FFIRMFINISHDATE,t2.FNUMBER ItemNo from (
						select  case when t1.FDEMANDTYPE='0' then  t1.FBILLNO else t1.FSALEORDERNO end FSALEORDERNO,
						case when t1.FDEMANDTYPE='0' then  0 else t1.FSALEORDERENTRYSEQ end FSALEORDERENTRYSEQ,
						t1.FDEMANDDATE  FDATE,t1.FFIRMFINISHDATE,t1.FMATERIALID
						from T_PLN_REQUIREMENTORDER  t1 where FDEMANDTYPE in ('0','8')  
						union 
						select  t1.FBILLNO,t2.FSEQ,FDATE,FDELIVERYDATE,t2.FMATERIALID from T_SAL_ORDER t1
						inner join T_SAL_ORDERENTRY t2 on t1.FID =t2.FID
						inner join T_SAL_ORDERENTRY_D t3 on t2.FENTRYID=t3.FENTRYID
						) t1 
						inner join T_BD_MATERIAL t2 on t1.FMATERIALID=t2.FMATERIALID
					) so on ((so.FSALEORDERNO=reqDetR.FDEMANDBILLNO and so.FSALEORDERENTRYSEQ=reqDetR.FDEMANDBILLENTRYSEQ) or (so.FSALEORDERNO=reqDet.FSoNo and so.FSALEORDERENTRYSEQ=reqDet.FSoSeq))  and mat.FNUMBER=so.ItemNo
                    where  reqDetS.FPurchaseOrgId={orgId} and req.FDOCUMENTSTATUS='C' and req.FCANCELSTATUS='A' and reqDet.FMRPTERMINATESTATUS='A' and req.FCLOSESTATUS='A'
                    and not exists(select top 1 FSBILLID from T_PUR_POORDERENTRY_LK polk where polk.FSTABLENAME='T_PUR_ReqEntry' and polk.FSBILLID=reqDet.FID and polk.FSID=reqDet.FENTRYID) {sqlAdd}";

			var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{

				list.Add(new PrToPoPurchaseRequireEntity()
				{
					Selected = false,
					PrNo = Convert.ToString(data["FBILLNO"]),
					FId = Convert.ToInt64(string.IsNullOrWhiteSpace(Convert.ToString(data["FID"])) ? 0 : data["FID"]),
					PrSeq = Convert.ToInt32(string.IsNullOrWhiteSpace(Convert.ToString(data["FSEQ"])) ? 0 : data["FSEQ"]),
					FentryId = Convert.ToInt32(string.IsNullOrWhiteSpace(Convert.ToString(data["FENTRYID"])) ? 0 : data["FENTRYID"]),
					SoNo = Convert.ToString(data["FDEMANDBILLNO"]),
					SoSeq = Convert.ToInt32(string.IsNullOrWhiteSpace(Convert.ToString(data["FDEMANDBILLENTRYSEQ"])) ? 0 : data["FDEMANDBILLENTRYSEQ"]),
					SoActualDate = string.IsNullOrWhiteSpace(Convert.ToString(data["FDATE"])) ? "" : Convert.ToDateTime(data["FDATE"].ToString()).ToString("yyyy-MM-dd"),
					SoRtd = string.IsNullOrWhiteSpace(Convert.ToString(data["FDELIVERYDATE"])) ? "" : Convert.ToDateTime(data["FDELIVERYDATE"].ToString()).ToString("yyyy-MM-dd"),
					ItemID = Convert.ToInt64(string.IsNullOrWhiteSpace(Convert.ToString(data["FMATERIALID"])) ? 0 : data["FMATERIALID"]),
					ItemNo = Convert.ToString(data["FNUMBER"]),
					ItemDesc = Convert.ToString(data["ItemDesc"]),
					SmallClass = Convert.ToString(data["SmallClass"]),
					SmallClassId = Convert.ToString(data["SmallClassId"]),
					Qty = Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["FREQQTY"])) ? 0 : data["FREQQTY"]),
					UomId = Convert.ToInt32(string.IsNullOrWhiteSpace(Convert.ToString(data["UomId"])) ? 0 : data["UomId"]),
					Uom = "",
					ItemBrand = "",
					PurchaseNum = Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["FREQQTY"])) ? 0 : data["FREQQTY"]),
					RequiredDeliveryDate = string.IsNullOrWhiteSpace(Convert.ToString(data["RequiredDeliveryDate"])) ? "" : Convert.ToDateTime(data["RequiredDeliveryDate"].ToString()).ToString("yyyy-MM-dd"),
					Remark = Convert.ToString(data["FENTRYNOTE"]),
					SupplierProductCode = Convert.ToString(data["FSupplierProductCode"]),
					VendorId = 0,
					VendorCode = "",
					VendorName = "",
					Price = 0,
					MinOrderQuantity = 0,
					Simili = false,
					RawMaterialPrice = 0,
					ProcessFee = 0,
					WeightRate = 0,
					PriceSource = "",
					SuggestSupplierId = Convert.ToInt64(data["FSUGGESTSUPPLIERID"]),
					SuggestSupplierPrice = Convert.ToDecimal(data["FSUPPLIERUNITPRICE"]),
					BillTypeId = Convert.ToString(data["FBILLTYPEID"]),
					BillTypeName = Convert.ToString(data["FBILLTYPENAME"]),
				});
			}
			return list;
		}

		// 获取供应商和型号权限
		public PrToPoAuthoritySetting GetSupplierAndModelPermission(string wxUserCode)
		{
			PrToPoAuthoritySetting authoritySetting = new PrToPoAuthoritySetting();
			if (!string.IsNullOrWhiteSpace(wxUserCode))
			{
				//获取采购员和其有权限的用户
				string url = $"workbench/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Privilege/GetAuthorityUserList";
				var getAuthorityUserComm = ApigatewayUtils.InvokePostWebService(url, "", userCode: wxUserCode);
				var getAuthorityUserResult = JsonConvertUtils.DeserializeObject<ResponseMessage<AuthorityUser[]>>(getAuthorityUserComm);

				List<long> userList = new List<long>();
				if (getAuthorityUserResult != null && getAuthorityUserResult.Code == "success")
				{
					foreach (var item in getAuthorityUserResult.Data)
					{
						userList.Add(item.UserId);
					}
				}
				var requestData = JsonConvertUtils.SerializeObject<object>(userList);
				//获取采购员有权限的产品小类
				string scmPscUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetAuthorityProductSmallClass";
				var resultJson = ApigatewayUtils.InvokePostWebService(scmPscUrl, requestData, userCode: wxUserCode);
				var GetPscResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<long>>>(resultJson);
				if (GetPscResult != null && GetPscResult.Data != null && GetPscResult.Data.Count > 0)
				{
					authoritySetting.SmallClassList = GetPscResult.Data;
					authoritySetting.SmallClassStr = string.Join("','", GetPscResult.Data);
				}
				//获取采购员有权限的供应商
				string scmSpUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetAuthoritySupplier";
				var resultSpJson = ApigatewayUtils.InvokePostWebService(scmSpUrl, requestData, userCode: wxUserCode);
				var GetSpResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<string>>>(resultSpJson);
				if (GetSpResult != null && GetSpResult.Data != null && GetSpResult.Data.Count > 0)
				{
					authoritySetting.SupplierStr = string.Join("','", GetSpResult.Data);
				}
			}
			return authoritySetting;
		}

		/// <summary>
		/// 获取用户角色
		/// </summary>
		/// <returns></returns>
		public List<string> GetUserRoles(string wxUserCode)
		{
			List<string> roles = new List<string>();
			if (!string.IsNullOrWhiteSpace(wxUserCode))
			{
				//获取采购员和其有权限的用户
				string url = $"workbench/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Privilege/GetUserRoles";
				var getAuthorityRoles = ApigatewayUtils.InvokePostWebService(url, "", userCode: wxUserCode);
				var getAuthorityRolesResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<AuthorityRoles>>>(getAuthorityRoles);

				if (getAuthorityRolesResult != null && getAuthorityRolesResult.Code == "success")
				{
					foreach (var item in getAuthorityRolesResult.Data)
					{
						roles.Add(item.Code.ToLower());
					}
				}
			}
			return roles;
		}

		/// <summary>
		/// 获取小类按钮弹窗
		/// </summary>
		/// <param name="e"></param>
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			if (e.FieldKey.ToUpper().Equals("FLS_SUPPLIERCODE"))
			{
				DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
				formParameter.FormId = "PENY_PrBillToPoBillDialogClass";

				this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
				{
					if (result.ReturnData != null)
					{
						DynamicObject resdata = result.ReturnData as DynamicObject;
						this.Model.SetValue("FLS_SupplierCode", resdata["FName"].ToString());
					}
				}));
			}
			else if (e.FieldKey.ToUpper().Equals("FRS_SMALLCLASS"))
			{
				DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
				formParameter.FormId = "PENY_PrBillToPoBillDialogClass";

				this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
				{
					if (result.ReturnData != null)
					{
						DynamicObject resdata = result.ReturnData as DynamicObject;
						this.Model.SetValue("FRS_SmallClass", resdata["FName"].ToString());
					}
				}));
			}
		}

		/// <summary>
		/// 菜单点击事件
		/// </summary>
		/// <param name="e"></param>
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);

			//匹配供应商价格
			if (e.BarItemKey == "BtnMatchPrice")
			{
				int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FLEntity");
				DynamicObject selectedEntityObj = (this.View.Model.DataObject["FLEntity"] as DynamicObjectCollection)[rowIndex];
				if (selectedEntityObj == null || string.IsNullOrWhiteSpace(Convert.ToString(selectedEntityObj["FLSupplierCode"])))
				{
					this.View.ShowErrMessage("请选择供应商后再匹配价格");
					return;
				}
				MatchSupplierPriceRequest request = new MatchSupplierPriceRequest();
				request.supplierCode = Convert.ToString(selectedEntityObj["FLSupplierCode"]);

				//保存采购需求单临时数据
				SavePrTempData();
				//获取选中的需求单数据
				var selectedData = prToPoPurchaseRequireEntities.Where(x => x.Selected).ToList();
				if (selectedData.Count == 0)
				{
					this.View.ShowErrMessage("请勾选匹配的需求单数据");
					return;
				}
				var productSmallClassName = Convert.ToString(selectedEntityObj["FLProductSmallClassName"]);
				selectedData = selectedData.Where(x => x.SmallClass.Equals(productSmallClassName)).ToList();
				if (selectedData.Count == 0)
				{
					this.View.ShowErrMessage($"勾选的数据中没有包含小类为[{productSmallClassName}]");
					return;
				}

				request.choiceList = selectedData;
				//选定供应商匹配价格
				var getMatchDataList = MatchSupplierPrice(request);

				//弹出同源供应商价格比较
				if (getMatchDataList.LowerPriceCompareList.Count > 0)
				{
					DialogSupplierPriceParity(getMatchDataList);
				}

				//获取行情价格
				List<MatchRawMaterialPriceRequest> ReqRawMaterialPrice = new List<MatchRawMaterialPriceRequest>();
				foreach (var item in getMatchDataList.Result)
				{
					ReqRawMaterialPrice.Add(new MatchRawMaterialPriceRequest
					{
						CompanyCode = orgCode,
						ProductModel = item.ItemNo,
						SupplierCode = item.VendorCode,
						Qty = item.Qty
					});
				}
				var MatchRawMaterialPriceList = GetMatchRawMaterialPrice(ReqRawMaterialPrice);

				//更改匹配的供应商价格
				for (int i = 0; i < getMatchDataList.Result.Count; i++)
				{
					//获取历史记录
					var historyList = prToPoPurchaseRequireEntities.FirstOrDefault(x => x.PrNo.Equals(getMatchDataList.Result[i].PrNo) && x.PrSeq.Equals(getMatchDataList.Result[i].PrSeq));
					historyList.VendorId = getMatchDataList.Result[i].VendorId;
					historyList.VendorCode = getMatchDataList.Result[i].VendorCode;
					historyList.VendorName = getMatchDataList.Result[i].VendorName;
					historyList.Price = getMatchDataList.Result[i].Price;
					historyList.MinOrderQuantity = getMatchDataList.Result[i].MinOrderQuantity;
					historyList.Remark = getMatchDataList.Result[i].Remark;
					//historyList.RequiredDeliveryDate = getMatchDataList.Result[i].RequiredDeliveryDate;
					historyList.PriceSource = getMatchDataList.Result[i].PriceSource;
					//绑定行情价、加工费、米重
					if (MatchRawMaterialPriceList != null)
					{
						var matchRawMaterialPrice = MatchRawMaterialPriceList.FirstOrDefault(x => x.ProductModel.Equals(getMatchDataList.Result[i].ItemNo)
						&& x.Qty.Equals(getMatchDataList.Result[i].Qty) && (x.SupplierCode ?? "").Equals(getMatchDataList.Result[i].VendorCode ?? ""));
						if (matchRawMaterialPrice != null)
						{
							historyList.RawMaterialPrice = matchRawMaterialPrice.RawMaterialPrice;
							historyList.ProcessFee = matchRawMaterialPrice.ProcessFee;
							historyList.WeightRate = matchRawMaterialPrice.WeightRate;
						}
					}
				}

				//重新绑定数据
				var list = prToPoPurchaseRequireEntities.Where(x => x.SmallClass.Equals(productSmallClassName)).OrderByDescending(x => x.Selected).ToList();
				this.Model.DeleteEntryData("FREntity");
				BDPurchaseRequireModel(list);

			}
			else if (e.BarItemKey == "BtnToPo")
			{
				//PrToPo
				//保存采购需求单临时数据
				SavePrTempData();
				var list = prToPoPurchaseRequireEntities.Where(x => x.Selected).ToList();
				if (list.Count == 0)
				{
					this.View.ShowErrMessage("请勾选数据");
					return;
				}
				if (list.Where(x => x.PurchaseNum <= 0).ToList().Count > 0)
				{
					this.View.ShowErrMessage("采购数量不能小于等于0");
					return;
				}
				if (list.Where(x => x.VendorId == 0).ToList().Count > 0)
				{
					this.View.ShowErrMessage("供应商不能为空");
					return;
				}
				if (list.Where(x => x.Price == 0).ToList().Count > 0)
				{
					this.View.ShowErrMessage("价格不能小于等于0");
					return;
				}
				if (list.Where(x => string.IsNullOrWhiteSpace(x.RequiredDeliveryDate)).ToList().Count > 0)
				{
					this.View.ShowErrMessage("要求交货日期不能为空");
					return;
				}
				if (list.Where(x => Convert.ToDateTime(x.RequiredDeliveryDate) < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))).ToList().Count > 0)
				{
					this.View.ShowErrMessage("要求交货日期不能小于当前日期");
					return;
				}

				//供应商列表
				var suppList = list.GroupBy(p => new { p.VendorCode, p.VendorName })
					.Select(o => new { VendorCode = o.Key.VendorCode, VendorName = o.Key.VendorName, CountNum = o.Count() }).ToList();
				IOperationResult opSuppResult = new OperationResult();
				foreach (var item in suppList)
				{
					//获取临时供应商采购订单数
					int orderNum = GetPoOrderCount(item.VendorCode);
					if (orderNum >= 5)
					{
						opSuppResult.OperateResult.Add(new OperateResult()
						{
							Name = "转PO失败",
							Message = $"有{item.CountNum}条勾选的采购申请单存在临时供应商[{item.VendorName}]超过最大下单次数。",
							MessageType = MessageType.FatalError,
							SuccessStatus = false
						});
					}
				}
				if (opSuppResult.OperateResult.Where(x => x.SuccessStatus == false).ToList().Count > 0)
				{
					this.View.ShowOperateResult(opSuppResult.OperateResult);
					return;
				}
				this.View.ShowMessage("确认要生成PO单吗?", MessageBoxOptions.YesNo,
				  new Action<MessageBoxResult>((results) =>
				  {
					  if (results == MessageBoxResult.Yes)
					  {
						  //PrToPo服务
						  var opResultService = PrBillToPoBillServiceHelper.PrBillToPoBillAction(this.Context, list);
						  //不存在错误才执行移除数据和提交
						  if (opResultService.OperateResult.Where(x => x.SuccessStatus == false).ToList().Count == 0)
						  {
							  //移除数据
							  var modelList = this.Model.DataObject["FREntity"] as DynamicObjectCollection;
							  for (int i = modelList.Count - 1; i >= 0; i--)
							  {
								  if (bool.Parse(Convert.ToString(modelList[i]["F_CheckBoxPrNo"])))
								  {
									  this.Model.DeleteEntryRow("FREntity", i);
								  }
							  }
							  foreach (var p in list)
							  {
								  prToPoPurchaseRequireEntities.Remove(p);
							  }
						  }
						  this.View.ShowOperateResult(opResultService.OperateResult);
					  }
				  }));
			}
			else if (e.BarItemKey == "BtnMatchSupplierAndPrice")
			{
				//保存采购需求单临时数据
				SavePrTempData();
				//获取选中的需求单数据
				var selectedData = prToPoPurchaseRequireEntities.Where(x => x.Selected).ToList();
				if (selectedData.Count == 0)
				{
					this.View.ShowErrMessage("请勾选匹配的需求单数据");
					return;
				}
				//获取匹配供应商和价格的数据
				var getMatchDataList = MatchSupplierAndPrice(selectedData, this.Context);

				//弹出同源供应商价格比较
				if (getMatchDataList.LowerPriceCompareList.Count > 0)
				{
					DialogSupplierPriceParity(getMatchDataList);
				}
				//获取行情价格
				List<MatchRawMaterialPriceRequest> ReqRawMaterialPrice = new List<MatchRawMaterialPriceRequest>();
				foreach (var item in getMatchDataList.Result)
				{
					ReqRawMaterialPrice.Add(new MatchRawMaterialPriceRequest
					{
						CompanyCode = orgCode,
						ProductModel = item.ItemNo,
						SupplierCode = item.VendorCode,
						Qty = item.Qty
					});
				}
				var MatchRawMaterialPriceList = GetMatchRawMaterialPrice(ReqRawMaterialPrice);

				for (int i = 0; i < getMatchDataList.Result.Count; i++)
				{
					//获取历史记录
					var historyList = prToPoPurchaseRequireEntities.FirstOrDefault(x => x.PrNo.Equals(getMatchDataList.Result[i].PrNo) && x.PrSeq.Equals(getMatchDataList.Result[i].PrSeq));
					historyList.VendorId = getMatchDataList.Result[i].VendorId;
					historyList.VendorCode = getMatchDataList.Result[i].VendorCode;
					historyList.VendorName = getMatchDataList.Result[i].VendorName;
					historyList.Price = getMatchDataList.Result[i].Price;
					historyList.MinOrderQuantity = getMatchDataList.Result[i].MinOrderQuantity;
					historyList.Remark = getMatchDataList.Result[i].Remark;
					//historyList.RequiredDeliveryDate = getMatchDataList.Result[i].RequiredDeliveryDate;
					historyList.PriceSource = getMatchDataList.Result[i].PriceSource;
					//绑定行情价、加工费、米重
					if (MatchRawMaterialPriceList != null)
					{
						var matchRawMaterialPrice = MatchRawMaterialPriceList.FirstOrDefault(x => x.ProductModel.Equals(getMatchDataList.Result[i].ItemNo)
						&& x.Qty.Equals(getMatchDataList.Result[i].Qty) && (x.SupplierCode ?? "").Equals(getMatchDataList.Result[i].VendorCode ?? ""));
						if (matchRawMaterialPrice != null)
						{
							historyList.RawMaterialPrice = matchRawMaterialPrice.RawMaterialPrice;
							historyList.ProcessFee = matchRawMaterialPrice.ProcessFee;
							historyList.WeightRate = matchRawMaterialPrice.WeightRate;
						}
					}
				}
				//重新绑定数据
				this.Model.DeleteEntryData("FREntity");
				var list = prToPoPurchaseRequireEntities.OrderByDescending(x => x.Selected).ToList();

				BDPurchaseRequireModel(list);

			}
		}

		//弹出同源供应商价格比较
		public void DialogSupplierPriceParity(MatchSupplierAndPriceResponse getMatchDataList)
		{
			DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
			formParameter.FormId = "PENY_MatchSupplierAndPrice";
			formParameter.CustomComplexParams["datas"] = getMatchDataList.LowerPriceCompareList;
			//匹配类型(0匹配供应商和价格/1选定供应商)
			formParameter.CustomParams.Add("MatchType", getMatchDataList.MatchType.ToString());
			this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
			{
				if (result.ReturnData != null)
				{
					List<LowerPriceCompare> lowerPriceList = result.ReturnData as List<LowerPriceCompare>;
					this.View.ShowMessage("确认更改供应商吗?", MessageBoxOptions.YesNo,
					   new Action<MessageBoxResult>((results) =>
					   {
						   if (results == MessageBoxResult.Yes)
						   {
							   //获取当前显示的数据
							   var thisList = this.Model.DataObject["FREntity"] as DynamicObjectCollection;
							   //获取行情价格
							   List<MatchRawMaterialPriceRequest> ReqRawMaterialPrice = new List<MatchRawMaterialPriceRequest>();
							   foreach (var item in lowerPriceList)
							   {
								   ReqRawMaterialPrice.Add(new MatchRawMaterialPriceRequest
								   {
									   CompanyCode = orgCode,
									   ProductModel = item.ItemNo,
									   SupplierCode = item.LowerPriceVendorCode,
									   Qty = item.Qty
								   });

							   }
							   var MatchRawMaterialPriceList = GetMatchRawMaterialPrice(ReqRawMaterialPrice);
							   //更新数据
							   for (int i = 0; i < thisList.Count; i++)
							   {
								   if (thisList[i]["F_CheckBoxPrNo"].ToString().EqualsIgnoreCase("true"))
								   {
									   var lowerPrice = lowerPriceList.FirstOrDefault(o => o.PrNo.Equals(thisList[i]["FPrNo"]) && o.PrSeq.ToString().Equals(Convert.ToString(thisList[i]["FSeq"])));
									   if (lowerPrice != null)
									   {
										   //更新修改的供应商价格
										   this.Model.SetValue("FVendorId", lowerPrice.LowerPriceVendorId, i);
										   this.Model.SetValue("FPrice", lowerPrice.LowerPrice, i);
										   this.Model.SetValue("FMinOrderQuantity", lowerPrice.LowerPriceMinOrderQuantity, i);
										   this.Model.SetValue("FPriceSource", lowerPrice.LowerPriceSource, i);
										   //绑定行情价、加工费、米重
										   if (MatchRawMaterialPriceList != null)
										   {
											   var matchRawMaterialPrice = MatchRawMaterialPriceList.FirstOrDefault(x => x.ProductModel.Equals(lowerPrice.ItemNo)
											   && x.Qty.Equals(lowerPrice.Qty) && x.SupplierCode.Equals(lowerPrice.LowerPriceVendorCode));
											   if (matchRawMaterialPrice != null)
											   {
												   this.Model.SetValue("FRawMaterialPrice", matchRawMaterialPrice.RawMaterialPrice, i);
												   this.Model.SetValue("FProcessFee", matchRawMaterialPrice.ProcessFee, i);
												   this.Model.SetValue("FWeightRate", matchRawMaterialPrice.WeightRate, i);
											   }
										   }
									   }
								   }
							   }
							   this.View.UpdateView("FREntity");

							   this.View.ShowMessage("需要导出刚更改的供应商列表以备后续查验吗?", MessageBoxOptions.YesNo,
								new Action<MessageBoxResult>((resultss) =>
								{
									if (resultss == MessageBoxResult.Yes)
									{
										// 导出非同源供应商价格Excel
										ExportExcel(lowerPriceList, getMatchDataList.MatchType);
									}
								}));
						   }
					   }));
				}
			}));

		}

		/// <summary>
		/// 导出非同源供应商价格Excel
		/// </summary>
		public void ExportExcel(List<LowerPriceCompare> lowerPriceList, int MatchType)
		{

			DataTable dt = new DataTable();
			if (MatchType == 0)
			{
				dt.Columns.AddRange(new DataColumn[] {
												new DataColumn("产品型号",typeof(string)),
												new DataColumn("产品名称",typeof(string)),
												new DataColumn("同源供应商",typeof(string)),
												new DataColumn("同源供应商价格",typeof(string)),
												new DataColumn("同源供应商起订量",typeof(string)),
												new DataColumn("同源价格来源",typeof(string)),
												new DataColumn("非同源供应商",typeof(string)),
												new DataColumn("非同源供应商价格",typeof(string)),
												new DataColumn("非同源供应商起订量",typeof(string)),
												new DataColumn("非同源价格来源",typeof(string)),});
				DataRow dr = dt.NewRow();
				dr["产品型号"] = "产品型号";
				dr["产品名称"] = "产品名称";
				dr["同源供应商"] = "同源供应商";
				dr["同源供应商价格"] = "同源供应商价格";
				dr["同源供应商起订量"] = "同源供应商起订量";
				dr["同源价格来源"] = "同源价格来源";
				dr["非同源供应商"] = "非同源供应商";
				dr["非同源供应商价格"] = "非同源供应商价格";
				dr["非同源供应商起订量"] = "非同源供应商起订量";
				dr["非同源价格来源"] = "非同源价格来源";
				dt.Rows.Add(dr);
				foreach (var item in lowerPriceList)
				{
					dr = dt.NewRow();
					dr["产品型号"] = item.ItemNo;
					dr["产品名称"] = item.ItemDesc;
					dr["同源供应商"] = item.SelectVendorName;
					dr["同源供应商价格"] = item.SelectPrice;
					dr["同源供应商起订量"] = item.SelectMinOrderQuantity;
					dr["同源价格来源"] = item.SelectPriceSource;
					dr["非同源供应商"] = item.LowerPriceVendorName;
					dr["非同源供应商价格"] = item.LowerPrice;
					dr["非同源供应商起订量"] = item.LowerPriceMinOrderQuantity;
					dr["非同源价格来源"] = item.LowerPriceSource;
					dt.Rows.Add(dr);
				}
			}
			else
			{

				dt.Columns.AddRange(new DataColumn[] {
												new DataColumn("产品型号",typeof(string)),
												new DataColumn("产品名称",typeof(string)),
												new DataColumn("所选供应商",typeof(string)),
												new DataColumn("所选供应商价格",typeof(string)),
												new DataColumn("所选供应商起订量",typeof(string)),
												new DataColumn("所选价格来源",typeof(string)),
												new DataColumn("更换的供应商",typeof(string)),
												new DataColumn("更换的供应商价格",typeof(string)),
												new DataColumn("更换的供应商起订量",typeof(string)),
												new DataColumn("更换的价格来源",typeof(string))
				});
				DataRow dr = dt.NewRow();
				dr["产品型号"] = "产品型号";
				dr["产品名称"] = "产品名称";
				dr["所选供应商"] = "所选供应商";
				dr["所选供应商价格"] = "所选供应商价格";
				dr["所选供应商起订量"] = "所选供应商起订量";
				dr["所选价格来源"] = "所选价格来源";
				dr["更换的供应商"] = "更换的供应商";
				dr["更换的供应商价格"] = "更换的供应商价格";
				dr["更换的供应商起订量"] = "更换的供应商起订量";
				dr["更换的价格来源"] = "更换的价格来源";
				dt.Rows.Add(dr);
				foreach (var item in lowerPriceList)
				{
					dr = dt.NewRow();
					dr["产品型号"] = item.ItemNo;
					dr["产品名称"] = item.ItemDesc;
					dr["所选供应商"] = item.SelectVendorName;
					dr["所选供应商价格"] = item.SelectPrice;
					dr["所选供应商起订量"] = item.SelectMinOrderQuantity;
					dr["所选价格来源"] = item.SelectPriceSource;
					dr["更换的供应商"] = item.LowerPriceVendorName;
					dr["更换的供应商价格"] = item.LowerPrice;
					dr["更换的供应商起订量"] = item.LowerPriceMinOrderQuantity;
					dr["更换的价格来源"] = item.LowerPriceSource;
					dt.Rows.Add(dr);
				}
			}

			//金蝶对Excel操作

			string fileName = string.Format("{0}{1}.xlsx", "ExChangedOriginalSupplierList", DateTime.Now.ToString("hhmmssffffff"));

			//获取路径
			string filePath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, fileName);

			//获取服务器Url地址,把文件传到服务器上面,然后下载
			string fileUrl = PathUtils.GetServerPath(KeyConst.TEMPFILEPATH, fileName);

			using (ExcelOperation excelHelper = new ExcelOperation(this.View))
			{
				excelHelper.BeginExport();
				//数据内容 dt,单据编号
				excelHelper.ExportToFile(dt);
				//路径,保存为excel文件
				excelHelper.EndExport(filePath, SaveFileType.XLSX);
			}

			//打开文件下载界面
			DynamicFormShowParameter showParameter = new DynamicFormShowParameter();
			showParameter.FormId = "BOS_FileDownload";
			showParameter.OpenStyle.ShowType = ShowType.Modal;
			showParameter.CustomComplexParams.Add("url", fileUrl);
			//显示
			this.View.ShowForm(showParameter);

		}

		//行单击事件
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (e.Key == "FLENTITY")
			{
				int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FLEntity");
				DynamicObject selectedEntityObj = (this.View.Model.DataObject["FLEntity"] as DynamicObjectCollection)[rowIndex];
				if (selectedEntityObj == null || string.IsNullOrWhiteSpace(Convert.ToString(selectedEntityObj["FLSupplierCode"])))
				{
					this.View.GetMainBarItem("BtnMatchPrice").Enabled = false;
				}
				else
				{
					this.View.GetMainBarItem("BtnMatchPrice").Enabled = true;
				}
			}
		}

		/// <summary>
		/// 保存采购需求单临时数据
		/// </summary>
		public void SavePrTempData()
		{
			//点击前去保存修改的数据
			var list = this.Model.DataObject["FREntity"] as DynamicObjectCollection;
			for (int i = 0; i < list.Count; i++)
			{
				//获取历史记录
				var historyList = prToPoPurchaseRequireEntities.FirstOrDefault(x => x.PrNo.Equals(list[i]["FPrNo"].ToString()) && x.PrSeq.Equals(Int32.Parse(list[i]["FSeq"].ToString())));
				if (historyList != null)
				{
					//采购数量
					historyList.PurchaseNum = Convert.ToDecimal(Convert.ToString(list[i]["FPurchaseNum"]));

					//供应商
					if (!string.IsNullOrWhiteSpace(Convert.ToString(list[i]["FVendorId"])))
					{
						//var doc = this.Model.DataObject["FREntity"] as DynamicObjectCollection;
						//var dy = doc[i]["FVendorId"] as DynamicObject;
						//historyList.VendorId = int.Parse(Convert.ToString(dy["Id"]));
						//historyList.VendorCode = Convert.ToString(dy["Number"]);
						//historyList.VendorName = Convert.ToString(dy["Name"]);
						historyList.VendorId = Convert.ToInt64(list[i]["FVendorId_id"]);
						historyList.VendorCode = Convert.ToString(((DynamicObject)list[i]["FVendorId"])["Number"]);
						historyList.VendorName = Convert.ToString(((DynamicObject)list[i]["FVendorId"])["Name"]);
					}
					else
					{
						historyList.VendorId = 0;
						historyList.VendorCode = "";
						historyList.VendorName = "";
					}
					//单位
					if (!string.IsNullOrWhiteSpace(Convert.ToString(list[i]["FUomId"])))
					{
						var doc = this.Model.DataObject["FREntity"] as DynamicObjectCollection;
						var dy = doc[i]["FUomId"] as DynamicObject;
						historyList.Uom = Convert.ToString(dy["Name"]);
					}
					//价格
					historyList.Price = Convert.ToDecimal(Convert.ToString(list[i]["FPrice"]));

					//起订量
					historyList.MinOrderQuantity = Convert.ToInt32(Convert.ToString(list[i]["FMinOrderQuantity"]));

					//要求交货日期
					if (!string.IsNullOrWhiteSpace(Convert.ToString(list[i]["FRequiredDeliveryDate"])) && !Convert.ToString(list[i]["FRequiredDeliveryDate"]).Contains("0001-01-01"))
					{
						historyList.RequiredDeliveryDate = DateTime.Parse(Convert.ToString(list[i]["FRequiredDeliveryDate"])).ToString("yyyy-MM-dd");
					}
					else
					{
						historyList.RequiredDeliveryDate = "";
					}

					//选择
					historyList.Selected = bool.Parse(Convert.ToString(list[i]["F_CheckBoxPrNo"]));

					//备注
					if (!string.IsNullOrWhiteSpace(Convert.ToString(list[i]["FRemark"])))
					{
						historyList.Remark = Convert.ToString(list[i]["FRemark"]);
					}
					else
					{
						historyList.Remark = "";
					}
					//销售下单日期
					if (Convert.ToString(list[i]["FSoActualDate"]).Contains("0001-01-01") || string.IsNullOrWhiteSpace(Convert.ToString(list[i]["FSoActualDate"])))
					{
						historyList.SoActualDate = "";
					}
					else
					{
						historyList.SoActualDate = DateTime.Parse(Convert.ToString(list[i]["FSoActualDate"])).ToString("yyyy-MM-dd");
					}
					//销售发货日期
					if (Convert.ToString(list[i]["FSoRtd"]).Contains("0001-01-01") || string.IsNullOrWhiteSpace(Convert.ToString(list[i]["FSoRtd"])))
					{
						historyList.SoRtd = "";
					}
					else
					{
						historyList.SoRtd = DateTime.Parse(Convert.ToString(list[i]["FSoRtd"])).ToString("yyyy-MM-dd");
					}
					//行情价
					historyList.RawMaterialPrice = Convert.ToDecimal(Convert.ToString(list[i]["FRawMaterialPrice"]));
					//加工费
					historyList.ProcessFee = Convert.ToDecimal(Convert.ToString(list[i]["FProcessFee"]));
					//米重
					historyList.WeightRate = Convert.ToDecimal(Convert.ToString(list[i]["FWeightRate"]));
					//价格来源
					historyList.PriceSource = Convert.ToString(list[i]["FPriceSource"]);
				}

			}

		}

		/// <summary>
		/// 匹配供应商和价格
		/// </summary>
		/// <param name="choiceList"></param>
		/// <returns></returns>
		public MatchSupplierAndPriceResponse MatchSupplierAndPrice(List<PrToPoPurchaseRequireEntity> choiceList, Context ctx)
		{

			MatchSupplierAndPriceResponse response = new MatchSupplierAndPriceResponse();
			response.Result = new List<PrToPoPurchaseRequireEntity>();
			response.LowerPriceCompareList = new List<LowerPriceCompare>();
			//response.MyProperty = "";
			response.MatchType = 0;
			try
			{
				//获取物料信息
				var itemInfos = GetItemInfos(choiceList);
				var matchPls = GetMatrixPrice(choiceList, itemInfos, "");
				foreach (var item in choiceList)
				{
					var matchPl = matchPls.FirstOrDefault(p => p.Number == item.ItemNo && p.Qty == item.PurchaseNum);
					PrToPoPurchaseRequireEntity resItem = GetSupplierAndPriceItem(response, item, matchPl, ctx);
					response.Result.Add(resItem);
				}
				response.Code = "success";
			}
			catch (Exception e)
			{
				response.Code = "fail";
				response.ErrorMessge = e.ToString();
			}
			return response;
		}

		/// <summary>
		/// 选定供应商匹配价格
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public MatchSupplierAndPriceResponse MatchSupplierPrice(MatchSupplierPriceRequest request)
		{
			MatchSupplierAndPriceResponse response = new MatchSupplierAndPriceResponse();

			PrToPoAuthoritySetting permission = new PrToPoAuthoritySetting();
			bool getAll = true;
			string supplierFilter = "";
			response.MatchType = 1;
			if (string.IsNullOrWhiteSpace(wxUserCode))
			{
				return response;
			}
			var roles = GetUserRoles(wxUserCode);
			if (!roles.Contains("purchasermanager") && !roles.Contains("admin"))
			{
				getAll = false;
				permission = GetSupplierAndModelPermission(wxUserCode);
				if (permission != null && !string.IsNullOrWhiteSpace(permission.SupplierStr))
				{
					supplierFilter = permission.SupplierStr;
				}
			}
			response.Result = new List<PrToPoPurchaseRequireEntity>();
			response.LowerPriceCompareList = new List<LowerPriceCompare>();

			//获取供应商列表
			var spList = GetSupplierList(request.supplierCode, supplierFilter, getAll);
			if (spList.Count <= 0)
			{
				response.Code = "fail";
				response.ErrorMessge = "供应商不存在或您没有权限";
				return response;
			}

			try
			{
				// 获取供应商对应的型号
				if (!string.IsNullOrWhiteSpace(wxUserCode))
				{
					string scmPscUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetAuthoritySmallList?SupplierCode={request.supplierCode}";
					var resultJson = ApigatewayUtils.InvokePostWebService(scmPscUrl, "", userCode: wxUserCode);
					var GetPscResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<string>>>(resultJson);
					if (GetPscResult != null && GetPscResult.Data != null && GetPscResult.Data.Count > 0)
					{
						request.choiceList = request.choiceList.Where(c => GetPscResult.Data.Contains(c.SmallClassId)).ToList();
					}
				}
				//获取物料
				var itemInfos = GetItemInfos(request.choiceList); // 物料信息
																  //有供应商的价目表
				var matchPls = GetMatrixPrice(request.choiceList, itemInfos, request.supplierCode);
				//无供应商的价目表
				var matchNoSupplierPls = GetMatrixPrice(request.choiceList, itemInfos, "");
				foreach (var item in request.choiceList)
				{
					var matchPl = matchPls.FirstOrDefault(p => p.Number == item.ItemNo && p.Qty == item.PurchaseNum);
					var matchNoSupplierPl = matchNoSupplierPls.FirstOrDefault(p => p.Number == item.ItemNo && p.Qty == item.PurchaseNum);
					PrToPoPurchaseRequireEntity resItem = GetSupplierPriceItem(request.supplierCode, response, item, matchPl, matchNoSupplierPl);
					response.Result.Add(resItem);
				}
				response.Code = "success";
				return response;
			}
			catch (Exception e)
			{
				response.Code = "fail";
				response.ErrorMessge = e.ToString();
				return response;
			}
		}

		/// <summary>
		/// 获取供应商和价格
		/// </summary>
		/// <param name="dataFilter"></param>
		/// <param name="response"></param>
		/// <param name="item"></param>
		/// <param name="priceList"></param>
		/// <param name="ctx"></param>
		/// <returns></returns>
		private PrToPoPurchaseRequireEntity GetSupplierAndPriceItem(MatchSupplierAndPriceResponse response, PrToPoPurchaseRequireEntity item, CompanyProductPriceResponse<PriceListEntity> priceList, Context ctx)
		{
			List<PriceListEntity> matchPl = priceList == null ? new List<PriceListEntity>() : priceList.SuppliserPrice;
			PrToPoPurchaseRequireEntity resItem = new PrToPoPurchaseRequireEntity();
			//if (!string.IsNullOrWhiteSpace(item.RequiredDeliveryDate))
			//{
			//    resItem.RequiredDeliveryDate = item.RequiredDeliveryDate;
			//}
			//else
			//{
			//    resItem.RequiredDeliveryDate = string.IsNullOrWhiteSpace(item.SoRtd) ? "" : Convert.ToDateTime(item.SoRtd).AddDays(-1).ToString("yyyy-MM-dd");
			//}
			resItem.ItemNo = item.ItemNo;
			resItem.ItemDesc = item.ItemDesc;
			resItem.SmallClass = item.SmallClass;
			resItem.SmallClassId = item.SmallClassId;
			resItem.SoNo = item.SoNo;
			resItem.SoSeq = item.SoSeq;
			resItem.ItemBrand = item.ItemBrand;
			resItem.Uom = item.Uom;
			resItem.Qty = item.Qty;
			resItem.PurchaseNum = item.PurchaseNum;
			resItem.SoActualDate = item.SoActualDate;
			resItem.SoRtd = item.SoRtd;
			resItem.Remark = item.Remark;
			resItem.PrNo = item.PrNo;
			resItem.PrSeq = item.PrSeq;
			resItem.PriceSource = item.PriceSource;
			//resItem.index = item.index;

			// 1-先用型号匹配全部价目表,获取价格最低的项。再用型号匹配采购记录中最低价格的项(供应商和价格)。两者比较价格，获取更低价格的项
			// 2-获取同源供应商--根据PR中型号关联的销售单号SONo找对应的客户，然后根据客户找该客户最近订购的该型号对应的供应商和采购价格
			// 3-如果1,2是同一供应商，取1；如果不是同一供应商并且1的价格比2低,列表默认展示同源供应商2，弹出框显示同源供应商和非同源供应商价格比较，让用户选择
			// 匹配采购记录最低价
			var mathchhs = MatchPurchaseHistoryMinPrice(item, true, "");
			matchPl.AddRange(mathchhs);

			//获取产品经理填写的成本价. 供应商编码,供应商名称,成本价 都不为空的才算
			var cost = GetCostPrice(item.ItemNo);

			if (cost != null)
			{
				matchPl.Add(cost);
			}
			var min = matchPl.OrderBy(p => p.Price).FirstOrDefault();
			if (min != null)
			{
				resItem.VendorId = min.VendorId;
				resItem.VendorCode = min.VendorCode;
				resItem.VendorName = min.VendorName;
				resItem.Price = min.Price;
				resItem.MinOrderQuantity = min.MinOrderQuantity;
				resItem.PriceSource = min.PriceSource;
			}
			// 同源客户采购历史最低价 
			PriceListEntity sameSourceCustHistory = null;
			var pars = new List<SqlParam>() {
				new SqlParam("@ItemNo", KDDbType.String, item.ItemNo),
				new SqlParam("@SoNo", KDDbType.String, item.SoNo ?? ""),
				new SqlParam("@LCID", KDDbType.Int32, ctx.UserLocale.LCID),
				new SqlParam("@OrgId", KDDbType.Int64, orgId)
			};
			var sql = $@"/*dialect*/SELECT top 1 mpm.FBILLNO PoNo,ven.FSUPPLIERID,ven.FNUMBER VDR_CODE,isnull(ven_l.FNAME,'') VendorName,pt.FNUMBER ITEM_NO, mpdf.FTAXPRICE VAT_PRICE,ms.FORDERQTY
						                    FROM t_PUR_POOrder mpm 
						                    inner join t_PUR_POOrderEntry mpd on mpm.FID = mpd.FID
						                    inner join T_PUR_POORDERENTRY_F mpdf on mpd.FENTRYID = mpdf.FENTRYID
						                    inner join T_PUR_POORDERENTRY_R mpdr on mpd.FENTRYID = mpdr.FENTRYID  
											inner join T_SAL_ORDER sal on sal.FBILLNO=mpdr.FDEMANDBILLNO
						                    inner join  t_BD_Supplier ven on  ven.FSUPPLIERID=mpm.FSUPPLIERID and ven.FDOCUMENTSTATUS='C' and ven.FFORBIDSTATUS='A' and ven.FCorrespondOrgId=0
						                    inner join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID and ven_l.FLOCALEID = @LCID
						                    inner join T_BD_MATERIAL pt on  pt.FMATERIALID=mpd.FMATERIALID
						                    left join t_BD_MaterialSale	 ms on ms.FMATERIALID=pt.FMATERIALID
											inner join T_SAL_ORDER sal2 on sal2.FCUSTID=sal.FCUSTID and  sal2.FBILLNO = @SoNo
                                            inner join T_SAL_ORDERENTRY sald2 on sal2.FID=sald2.FID and sald2.FSupplyTargetOrgId=@OrgId
						                    WHERE mpm.FPURCHASEORGID=@OrgId and mpm.FDOCUMENTSTATUS='C' and mpdf.FTAXPRICE != 0 
						                    and pt.FNUMBER=@ItemNo
						                    and mpm.FDATE >=CONVERT(varchar(12),DATEADD(year,-1,getdate()),23)
											order by mpdf.FTAXPRICE asc ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				sameSourceCustHistory = new PriceListEntity();
				sameSourceCustHistory.ItemNo = Convert.ToString(data["ITEM_NO"]);
				sameSourceCustHistory.VendorId = Convert.ToInt64(data["FSUPPLIERID"].ToString());
				sameSourceCustHistory.VendorCode = Convert.ToString(data["VDR_CODE"]);
				sameSourceCustHistory.VendorName = Convert.ToString(data["VendorName"]);
				sameSourceCustHistory.Price = Convert.ToDecimal(data["VAT_PRICE"]);
				sameSourceCustHistory.MinOrderQuantity = Convert.ToInt32(data["FORDERQTY"]);
				sameSourceCustHistory.PriceSource = $"采购历史价（采购单号：{Convert.ToString(data["PoNo"])}）";
			}

			if (sameSourceCustHistory != null)
			{
				if (sameSourceCustHistory.VendorId == 0)
				{
					sameSourceCustHistory.VendorId = GetSupplierId(sameSourceCustHistory.VendorCode);
				}
			}
			if (min != null)
			{
				if (min.VendorId == 0)
				{
					min.VendorId = GetSupplierId(min.VendorCode);
				}
			}
			if (sameSourceCustHistory != null && min != null)
			{
				if (!resItem.VendorCode.Equals(sameSourceCustHistory.VendorCode, StringComparison.CurrentCultureIgnoreCase) && resItem.Price < sameSourceCustHistory.Price)
				{
					// 同源非同源比较信息
					LowerPriceCompare compare = new LowerPriceCompare();
					compare.ItemNo = item.ItemNo;
					compare.ItemDesc = item.ItemDesc;
					compare.SelectVendorId = sameSourceCustHistory.VendorId;
					compare.SelectVendorCode = sameSourceCustHistory.VendorCode;
					compare.SelectPrice = sameSourceCustHistory.Price;
					compare.SelectMinOrderQuantity = sameSourceCustHistory.MinOrderQuantity;
					compare.SelectPriceSource = sameSourceCustHistory.PriceSource;
					compare.LowerPriceVendorId = min.VendorId;
					compare.LowerPriceVendorCode = min.VendorCode;
					compare.LowerPrice = min.Price;
					compare.LowerPriceMinOrderQuantity = min.MinOrderQuantity;
					compare.LowerPriceSource = min.PriceSource;
					compare.PrNo = item.PrNo;
					compare.PrSeq = item.PrSeq;
					compare.Qty = item.Qty;
					response.LowerPriceCompareList.Add(compare);

					// 列表展示同源供应商信息
					resItem.VendorId = compare.SelectVendorId;
					resItem.VendorCode = sameSourceCustHistory.VendorCode;
					resItem.VendorName = sameSourceCustHistory.VendorName;
					resItem.Price = sameSourceCustHistory.Price;
					resItem.MinOrderQuantity = sameSourceCustHistory.MinOrderQuantity;
					resItem.PriceSource = sameSourceCustHistory.PriceSource;
				}
			}
			if (resItem.VendorId == 0)
			{
				resItem.VendorId = GetSupplierId(resItem.VendorCode);
			}
			return resItem;
		}

		/// <summary>
		/// 获取价目表价格
		/// </summary>
		/// <param name="items"></param>
		/// <param name="itemInfos"></param>
		/// <param name="supplierCode"></param>
		/// <param name="orgCode"></param>
		/// <param name="ctx"></param>
		/// <returns></returns>
		private List<CompanyProductPriceResponse<PriceListEntity>> GetMatrixPrice(List<PrToPoPurchaseRequireEntity> items, List<PrToPoItemEntity> itemInfos, string supplierCode)
		{
			List<CompanyProductPriceResponse<PriceListEntity>> matchPl = new List<CompanyProductPriceResponse<PriceListEntity>>();
			// 获取矩阵价目表价格
			if (!string.IsNullOrWhiteSpace(wxUserCode))
			{
				var request = new
				{
					Company = orgCode,
					SupplierCode = supplierCode,
					DataSource = 2,   //0缓存1数据库2缓存查不到再查数据库
					NoNumberLimit = true,
					ProductCodes = items.Select(p =>
					{
						var itemInfo = itemInfos.FirstOrDefault(c => c.ItemNo == p.ItemNo);
						return new
						{
							ProductId = (itemInfo == null ? 0 : itemInfo.ProductId),
							Number = p.ItemNo,
							ShortNumber = (itemInfo == null ? "" : itemInfo.ShortNumber),
							Qty = p.PurchaseNum
						};
					})
				};
				var requestData = JsonConvertUtils.SerializeObject<dynamic>(request);

				string scmPscUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Matrix/GetCompanyMatrixProductPrices";
				var resultJson = ApigatewayUtils.InvokePostWebService(scmPscUrl, requestData, userCode: wxUserCode);
				var getResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<CompanyProductPriceResponse<MatrixProductPriceResponse>>>>(resultJson);
				if (getResult != null && getResult.Data != null && getResult.Data.Count > 0)
				{
					foreach (var matrix in getResult.Data)
					{
						if (matrix.SuppliserPrice.Where(x => x.Price > 0).ToList().Count > 0)
						{
							matchPl.Add(new CompanyProductPriceResponse<PriceListEntity>()
							{
								Number = matrix.Number,
								ProductId = matrix.ProductId,
								ShortNumber = matrix.ShortNumber,
								Qty = matrix.Qty,
								SuppliserPrice = matrix.SuppliserPrice.Where(x => x.Price > 0).Select(p =>
								new PriceListEntity()
								{
									VendorCode = p.Code,
									VendorName = p.Name,
									ItemNo = matrix.Number,
									Price = p.Price,
									DeliveryDays = p.DeliveryDay,
									MinOrderQuantity = p.Moq,
									QuantityUpperLimit = p.NumberLimit,
									ProcessFee = p.ProcessFee,
									WeightRate = p.WeightRate,
									RawMaterialPrice = p.RawMaterialPrice,
									PriceSource = "供应商价目表"
								}).ToList()
							});
						}
					}
				}
			}
			return matchPl;
		}

		/// <summary>
		/// 获取行情价
		/// </summary>
		/// <returns></returns>
		private List<MatchRawMaterialPriceEntity> GetMatchRawMaterialPrice(List<MatchRawMaterialPriceRequest> request)
		{
			List<MatchRawMaterialPriceEntity> list = new List<MatchRawMaterialPriceEntity>();
			var requestData = JsonConvertUtils.SerializeObject<dynamic>(request);

			string scmPscUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/PriceList/GetRawMaterialPrice";
			var resultJson = ApigatewayUtils.InvokePostWebService(scmPscUrl, requestData, userCode: wxUserCode);
			var getResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<MatchRawMaterialPriceEntity>>>(resultJson);
			if (getResult != null && getResult.Data != null && getResult.Data.Count > 0)
			{
				list = getResult.Data;
			}
			return list;
		}

		/// <summary>
		/// 获取供应商的价格
		/// </summary>
		/// <param name="supplierCode"></param>
		/// <param name="response"></param>
		/// <param name="dataFilter"></param>
		/// <param name="spList"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		private PrToPoPurchaseRequireEntity GetSupplierPriceItem(string supplierCode, MatchSupplierAndPriceResponse response, PrToPoPurchaseRequireEntity item, CompanyProductPriceResponse<PriceListEntity> priceList, CompanyProductPriceResponse<PriceListEntity> matchPriceList)
		{
			List<PriceListEntity> matchPl = priceList == null ? new List<PriceListEntity>() : priceList.SuppliserPrice;
			PrToPoPurchaseRequireEntity resItem = new PrToPoPurchaseRequireEntity();
			//if (!string.IsNullOrWhiteSpace(item.RequiredDeliveryDate))
			//{
			//    resItem.RequiredDeliveryDate = item.RequiredDeliveryDate;
			//}
			//else
			//{
			//    resItem.RequiredDeliveryDate = string.IsNullOrWhiteSpace(item.SoRtd) ? "" : Convert.ToDateTime(item.SoRtd).AddDays(-1).ToString("yyyy-MM-dd");
			//}
			//根据供应商编号获取供应商ID
			long thisSupplierId = GetSupplierId(supplierCode);
			resItem.ItemNo = item.ItemNo;
			resItem.ItemDesc = item.ItemDesc;
			resItem.SmallClass = item.SmallClass;
			resItem.SmallClassId = item.SmallClassId;
			resItem.SoNo = item.SoNo;
			resItem.SoSeq = item.SoSeq;
			resItem.ItemBrand = item.ItemBrand;
			resItem.Uom = item.Uom;
			resItem.Qty = item.Qty;
			resItem.PurchaseNum = item.PurchaseNum;
			resItem.SoActualDate = item.SoActualDate;
			resItem.SoRtd = item.SoRtd;
			resItem.Remark = item.Remark;
			resItem.PrNo = item.PrNo;
			resItem.PrSeq = item.PrSeq;
			resItem.VendorId = thisSupplierId;
			resItem.VendorCode = supplierCode;
			resItem.PriceSource = item.PriceSource;
			// 1-先用型号匹配该供应商价目表,获取价格最低的项。P1
			// 2-再用型号匹配采购记录中最近一年最低价格的项(供应商和价格)。两者比较价格，获取更低价格的项 P2
			// 3-如果价格P1和P2都不为空，且P1小于等于P2，使用P1;
			//   如果价格P1和P2都不为空，且P1大于P2中的价格，若P1和P2是同一供应商，使用P2；若P1和P2不是同一供应商，使用P1，同时提示该条记录有更低价格供应商
			//   如果P1为空, P2不为空，则取该供应商采购记录历史价格P3与P2比较
			//   如果P1不为空，P2为空，则取P1
			var mathchhs = MatchPurchaseHistoryMinPrice(item, true, ""); // 匹配采购记录最近一年最低价

			//获取产品经理填写的成本价. 供应商编码,供应商名称,成本价 都不为空的才算
			var cost = GetCostPrice(item.ItemNo);

			if (cost != null)
			{
				mathchhs.Add(cost);
			}
			//获取价目表非指定供应商最低价
			if (matchPriceList != null)
			{
				mathchhs.Add(matchPriceList.SuppliserPrice[0]);
			}

			// 匹配该供应商采购记录最近一年最低价
			var mathchSpHs = MatchPurchaseHistoryMinPrice(item, true, supplierCode);
			if (mathchSpHs.Count > 0)
			{
				matchPl.Add(mathchSpHs[0]);
			}

			var min = mathchhs.OrderBy(p => p.Price).FirstOrDefault();
			var supplierMin = matchPl.OrderBy(p => p.Price).FirstOrDefault();

			if (supplierMin != null)
			{
				if (supplierMin.VendorId == 0)
				{
					supplierMin.VendorId = GetSupplierId(supplierMin.VendorCode);
				}
			}
			if (min != null)
			{
				if (min.VendorId == 0)
				{
					min.VendorId = GetSupplierId(min.VendorCode);
				}
			}
			if (supplierMin != null && min != null)
			{
				if (supplierMin.Price > min.Price)
				{
					if (supplierMin.VendorCode.Equals(min.VendorCode, StringComparison.CurrentCultureIgnoreCase)) // 是同一供应商
					{
						resItem.VendorCode = min.VendorCode;
						resItem.VendorName = min.VendorName;
						resItem.Price = min.Price;
						resItem.MinOrderQuantity = min.MinOrderQuantity;
						resItem.PriceSource = min.PriceSource;
					}
					else // 不是同一供应商
					{
						// 使用P1
						resItem.VendorId = supplierMin.VendorId;
						resItem.VendorCode = supplierMin.VendorCode;
						resItem.VendorName = supplierMin.VendorName;
						resItem.Price = supplierMin.Price;
						resItem.MinOrderQuantity = supplierMin.MinOrderQuantity;
						resItem.DeliveryDays = supplierMin.DeliveryDays;
						resItem.PriceSource = supplierMin.PriceSource;
						// 记录更低价格供应商差异列表
						LowerPriceCompare compare = new LowerPriceCompare();
						compare.ItemNo = item.ItemNo;
						compare.ItemDesc = item.ItemDesc;
						compare.SelectVendorId = resItem.VendorId;
						compare.SelectVendorCode = resItem.VendorCode;
						compare.SelectPrice = resItem.Price;
						compare.SelectMinOrderQuantity = resItem.MinOrderQuantity;
						compare.SelectPriceSource = resItem.PriceSource;
						compare.LowerPriceVendorId = min.VendorId;
						compare.LowerPriceVendorCode = min.VendorCode;
						compare.LowerPrice = min.Price;
						compare.LowerPriceMinOrderQuantity = min.MinOrderQuantity;
						compare.LowerPriceSource = min.PriceSource;
						compare.PrNo = item.PrNo;
						compare.PrSeq = item.PrSeq;
						compare.Qty = item.Qty;
						response.LowerPriceCompareList.Add(compare);
					}
				}
				else
				{
					resItem.VendorId = supplierMin.VendorId; ;
					resItem.VendorCode = supplierMin.VendorCode;
					resItem.VendorName = supplierMin.VendorName;
					resItem.Price = supplierMin.Price;
					resItem.MinOrderQuantity = supplierMin.MinOrderQuantity;
					resItem.DeliveryDays = supplierMin.DeliveryDays;
					resItem.PriceSource = supplierMin.PriceSource;
				}
			}
			else if (supplierMin == null && min != null)
			{
				// 该供应商价目表中没有数据，采购历史也没有数据，记录更低价格供应商差异列表
				if (supplierCode.Equals(min.VendorCode, StringComparison.CurrentCultureIgnoreCase)) // 是同一供应商
				{
					resItem.VendorCode = min.VendorCode;
					resItem.VendorName = min.VendorName;
					resItem.Price = min.Price;
					resItem.MinOrderQuantity = min.MinOrderQuantity;
					resItem.PriceSource = min.PriceSource;
				}
				else
				{
					LowerPriceCompare compare = new LowerPriceCompare();
					compare.ItemNo = item.ItemNo;
					compare.ItemDesc = item.ItemDesc;
					compare.SelectVendorId = resItem.VendorId;
					compare.SelectVendorCode = resItem.VendorCode;
					compare.SelectPrice = 0;
					compare.SelectMinOrderQuantity = 0;
					compare.SelectPriceSource = "";
					compare.LowerPriceVendorId = min.VendorId;
					compare.LowerPriceVendorCode = min.VendorCode;
					compare.LowerPrice = min.Price;
					compare.LowerPriceMinOrderQuantity = min.MinOrderQuantity;
					compare.LowerPriceSource = min.PriceSource;
					compare.PrNo = item.PrNo;
					compare.PrSeq = item.PrSeq;
					compare.Qty = item.Qty;
					response.LowerPriceCompareList.Add(compare);
				}
			}

			if (matchPl.Count > 0 && mathchhs.Count <= 0)
			{
				resItem.VendorId = supplierMin.VendorId;
				resItem.VendorCode = supplierMin.VendorCode;
				resItem.VendorName = supplierMin.VendorName;
				resItem.Price = supplierMin.Price;
				resItem.MinOrderQuantity = supplierMin.MinOrderQuantity;
				resItem.DeliveryDays = supplierMin.DeliveryDays;
				resItem.PriceSource = supplierMin.PriceSource;
			}
			if (resItem.VendorId == 0)
			{
				resItem.VendorId = GetSupplierId(resItem.VendorCode);
			}
			return resItem;
		}

		/// <summary>
		/// 匹配采购记录最低价
		/// </summary>
		/// <param name="item"></param>
		/// <param name="isFilterDate"></param>
		/// <param name="supplierCode"></param>
		/// <returns></returns>
		public List<PriceListEntity> MatchPurchaseHistoryMinPrice(PrToPoPurchaseRequireEntity item, bool isFilterDate, string supplierCode)
		{
			List<PriceListEntity> list = new List<PriceListEntity>();
			var pars = new List<SqlParam>();
	
			var jdSqlAdd = "";
			pars.Add(new SqlParam("@ItemNo", KDDbType.String, item.ItemNo));
			pars.Add(new SqlParam("@LCID", KDDbType.Int32, this.Context.UserLocale.LCID));
			pars.Add(new SqlParam("@OrgId", KDDbType.Int64, orgId));
			if (isFilterDate) // 匹配价格 匹配规则为相同型号，最近一年，最低最近的采购价格
			{
				jdSqlAdd = $@" and m.FDATE>CONVERT(varchar(12),DATEADD(year,-1,getdate()),23) ";
			}
			if (!string.IsNullOrWhiteSpace(supplierCode)) // 过滤供应商
			{
				pars.Add(new SqlParam("@SupplierCode", KDDbType.String, supplierCode));
				jdSqlAdd += $@" and ven.FNUMBER = @SupplierCode ";
			}

			var sql = $@"/*dialect*/select top 1 m.FBILLNO PoNo,ven.FSUPPLIERID,ven.FNUMBER VDR_CODE,ven_l.FNAME VDR_NAMEC,
				ma.FNUMBER ITEM_NO,f.FTAXPRICE VAT_PRICE,ma.FMATERIALID FMATERIALID,ms.FORDERQTY
                from t_PUR_POOrderEntry d
                inner join t_PUR_POOrder m on m.FID=d.FID 
                inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                inner join t_BD_Supplier ven on ven.FSUPPLIERID=m.FSUPPLIERID  and ven.FCorrespondOrgId=0 and ven.FDOCUMENTSTATUS='C' and ven.FFORBIDSTATUS='A'
                left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID and ven_l.FLOCALEID = @LCID
                left join  T_BD_MATERIAL ma on d.FMATERIALID=ma.FMATERIALID
				left join t_BD_MaterialSale	 ms on ms.FMATERIALID=ma.FMATERIALID
                where m.FPURCHASEORGID=@OrgId and ma.FNUMBER=@ItemNo and m.FDOCUMENTSTATUS='C'  and f.FTAXPRICE != 0 {jdSqlAdd}
                order by f.FTAXPRICE asc ";
			var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				list.Add(new PriceListEntity()
				{
					VendorId = Convert.ToInt64(string.IsNullOrWhiteSpace(Convert.ToString(data["FSUPPLIERID"])) ? 0 : data["FSUPPLIERID"]),
					VendorCode = Convert.ToString(data["VDR_CODE"]),
					VendorName = Convert.ToString(data["VDR_NAMEC"]),
					ItemNo = Convert.ToString(data["ITEM_NO"]),
					Price = Convert.ToDecimal(string.IsNullOrWhiteSpace(Convert.ToString(data["VAT_PRICE"])) ? 0 : data["VAT_PRICE"]),
					MinOrderQuantity = Convert.ToInt32(string.IsNullOrWhiteSpace(Convert.ToString(data["FORDERQTY"])) ? 0 : data["FORDERQTY"]),
					QuantityUpperLimit = 0,
					PriceSource = $"采购历史价（采购单号：{Convert.ToString(data["PoNo"])}）"
				});
			}
			return list;
		}

		/// <summary>
		/// 获取供应商列表
		/// </summary>
		/// <returns></returns>
		public List<PrToPoSupplierEntity> GetSupplierList(string supplierCode, string supplierFilter, bool getAll)
		{
			List<PrToPoSupplierEntity> spList = new List<PrToPoSupplierEntity>();
			var sqlAdd = "";
			if (!getAll)
			{
				sqlAdd = $@" and ven.FNUMBER in ('{supplierFilter}')";
			}

			if (!string.IsNullOrWhiteSpace(supplierCode))
			{
				sqlAdd += $@" AND ven.FNUMBER='{supplierCode}' ";
			}

			var sql = $@"/*dialect*/select ven.FSUPPLIERID,ven.FNUMBER,ven_l.FNAME from t_BD_Supplier ven 
            inner join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID  and ven_l.FLOCALEID = {this.Context.UserLocale.LCID}
            where ven.FUSEORGID={orgId} and ven.FDOCUMENTSTATUS='C' and ven.FFORBIDSTATUS='A' {sqlAdd} ";
			var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
			foreach (var data in datas)
			{
				spList.Add(new PrToPoSupplierEntity()
				{
					SupplierId = Convert.ToInt64(Convert.ToString(data["FSUPPLIERID"])),
					SupplierCode = Convert.ToString(data["FNUMBER"]),
					SupplierName = Convert.ToString(data["FNAME"])
				});
			}
			return spList;
		}

		/// <summary>
		/// 获取物料信息
		/// </summary>
		/// <returns></returns>
		public List<PrToPoItemEntity> GetItemInfos(List<PrToPoPurchaseRequireEntity> choiceList)
		{
			List<PrToPoItemEntity> itemInfos = new List<PrToPoItemEntity>();
			var pars = new List<SqlParam>();
			var param = string.Empty;
			if (choiceList.Count > 0)
			{
				int i = 1;
				foreach (var item in choiceList)
				{
					if (i == 1)
						param = "@FNUMBER" + i;
					else
						param += ",@FNUMBER" + i;
					pars.Add(new SqlParam("@FNUMBER" + i++, KDDbType.String, item.ItemNo));
				}
			}
			else
			{
				param += "@FNUMBER";
				pars.Add(new SqlParam("@FNUMBER", KDDbType.String, "0"));
			}

			var sql = $@" SELECT FMATERIALID,FNUMBER,FPRODUCTID,FSHORTNUMBER FROM T_BD_MATERIAL WHERE  FUSEORGID={orgId} and FNUMBER IN ({param})";
			var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				itemInfos.Add(new PrToPoItemEntity()
				{
					ItemID = Convert.ToInt64(data["FMATERIALID"]),
					ProductId = Convert.ToInt64(data["FPRODUCTID"]),
					ItemNo = Convert.ToString(data["FNUMBER"]),
					ShortNumber = Convert.ToString(data["FSHORTNUMBER"])
				});
			}
			return itemInfos;
		}

		//获取产品经理填写的成本价. 供应商编码,供应商名称,成本价 都不为空的才算
		public PriceListEntity GetCostPrice(string ItemNo)
		{
			List<SqlParam> pars = new List<SqlParam>() {
				new SqlParam("@ItemNo", KDDbType.String, ItemNo),
				new SqlParam("@LCID", KDDbType.Int32, this.Context.UserLocale.LCID),
				new SqlParam("@OrgId", KDDbType.Int64, orgId)};
			PriceListEntity cost = null;
			var sql = $@"/*dialect*/select top 1 * from (
            select a.FBILLNO SoNo,ven2.FSUPPLIERID,ven.FNUMBER SupplierCode,ven_l.FNAME SupplierName,FSUPPLIERUNITPRICE SupplierUnitPrice,
			'供应商成本价维护页面（销售单号：'+a.FBILLNO+'）' PriceSource
			from T_SAL_ORDER a
            inner join T_SAL_ORDERENTRY b on a.FID=b.FID
            inner join  t_BD_Supplier ven on ven.FSUPPLIERID=b.FSUPPLIERID and ven.FCorrespondOrgId=0 and ven.FDOCUMENTSTATUS='C' and ven.FFORBIDSTATUS='A'
            inner join  t_BD_Supplier ven2 on ven.FNUMBER=ven2.FNUMBER and ven2.FUSEORGID=@OrgId
            inner join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID and ven_l.FLOCALEID = @LCID
            left join  T_BD_MATERIAL ma on b.FMATERIALID=ma.FMATERIALID
            where a.FDOCUMENTSTATUS='C' and  a.FCANCELSTATUS='A' and b.FSupplyTargetOrgId=@OrgId and ma.FNUMBER=@ItemNo and a.FMODIFYDATE>=CONVERT(varchar(12),DATEADD(year,-1,getdate()),23) 
            and b.FSUPPLIERID>0 and FSUPPLIERUNITPRICE>0
			union all
            select pln.FBILLNO SoNo,ven2.FSUPPLIERID,ven.FNUMBER SupplierCode,ven_l.FNAME SupplierName,FSUPPLIERUNITPRICE SupplierUnitPrice,
			'组织间需求单（需求单号：'+pln.FBILLNO+'）' PriceSource
			from T_PLN_REQUIREMENTORDER pln
            inner join  t_BD_Supplier ven on ven.FSUPPLIERID=pln.FSUPPLIERID  and ven.FDOCUMENTSTATUS='C' and ven.FFORBIDSTATUS='A'
			inner join  t_BD_Supplier ven2 on ven.FNUMBER=ven2.FNUMBER and ven2.FUSEORGID=@OrgId
            left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID and ven_l.FLOCALEID = @LCID
			left join  T_BD_MATERIAL ma on pln.FMATERIALID=ma.FMATERIALID
            where pln.FDemandType=8 and pln.FIsClosed='A' and pln.FDocumentStatus='C' and pln.FSupplyOrganId=@OrgId 
			and ma.FNUMBER=@ItemNo and pln.FDemandDate>=CONVERT(varchar(12),DATEADD(year,-1,getdate()),23) and pln.FSUPPLIERID>0 and FSUPPLIERUNITPRICE>0
            ) datas
            Order By SupplierUnitPrice";
			var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				cost = new PriceListEntity();
				cost.VendorId = Convert.ToInt64(data["FSUPPLIERID"]);
				cost.VendorCode = Convert.ToString(data["SupplierCode"]);
				cost.VendorName = Convert.ToString(data["SupplierName"]);
				cost.Price = Convert.ToDecimal(data["SupplierUnitPrice"]);
				cost.PriceSource = Convert.ToString(data["PriceSource"]);
			}
			return cost;
		}

		/// <summary>
		/// 获取临时供应商的下单次数
		/// </summary>
		/// <param name="vendorCode"></param>
		/// <returns></returns>
		private int GetPoOrderCount(string vendorCode)
		{
			var sql = $@"select case when FISTEMPORARY=1 then count(1) else 0 end orderNum from T_PUR_POORDER t1
                                inner join t_BD_Supplier t2 on t1.FSUPPLIERID=t2.FSUPPLIERID
                                where t1.FCANCELSTATUS='A' and t2.FNUMBER='{vendorCode}'
                                group by FISTEMPORARY ";
			return DBServiceHelper.ExecuteScalar<int>(this.Context, sql, 0);
		}

		/// <summary>
		/// 根据供应商编号获取供应商ID
		/// </summary>
		/// <param name="SupplierCode"></param>
		/// <returns></returns>
		private long GetSupplierId(string supplierCode)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UseOrgId", KDDbType.Int64, orgId) ,
				new SqlParam("@SupplierCode", KDDbType.String, supplierCode) };
			var sql = $@"select top 1 FSUPPLIERID from t_BD_Supplier where FUSEORGID=@UseOrgId and  FNUMBER=@SupplierCode and FDOCUMENTSTATUS='C' and FFORBIDSTATUS='A' ";
			return DBServiceHelper.ExecuteScalar<long>(this.Context, sql, 0, paramList: pars.ToArray());
		}


		/// <summary>
		/// 根据登录ID获取OrgCode
		/// </summary>
		/// <param name="SupplierCode"></param>
		/// <returns></returns>
		private string GetOrgCode()
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@OrgId", KDDbType.Int64, orgId) };
			var sql = $@"select top 1 FNUMBER from t_org_organizations where FORGID=@OrgId";
			return DBServiceHelper.ExecuteScalar<string>(this.Context, sql, "", paramList: pars.ToArray());
		}
	}

	[Description("采购申请转采购订单弹窗小类插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class PrBillToPoBillDialogClass : AbstractDynamicFormPlugIn
	{
		/// <summary>
		/// 初始化绑定
		/// </summary>
		/// <param name="e"></param>
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);

			var list = GetSmallClassList(this.Context.UserId);
			foreach (var item in list)
			{
				int rowcount = this.Model.GetEntryRowCount("FEntity");
				this.Model.CreateNewEntryRow("FEntity");
				this.Model.SetValue("FParentName", item.ParentName, rowcount);
				this.Model.SetValue("FName", item.Name, rowcount);
			}
			this.View.UpdateView("FEntity");
		}

		//按钮点击事件（查询）
		public override void AfterButtonClick(AfterButtonClickEventArgs e)
		{
			base.AfterButtonClick(e);
			if (e.Key.ToUpper().Equals("FB_SERACH"))
			{
				var serachFilter = this.View.Model.GetValue("FProductClass").ToString();
				var list = GetSmallClassList(this.Context.UserId);
				list = list.Where(x => x.Name.Contains(serachFilter)).ToList();
				this.Model.DeleteEntryData("FEntity");
				foreach (var item in list)
				{
					int rowcount = this.Model.GetEntryRowCount("FEntity");
					this.Model.CreateNewEntryRow("FEntity");
					this.Model.SetValue("FParentName", item.ParentName, rowcount);
					this.Model.SetValue("FName", item.Name, rowcount);
				}
				this.View.UpdateView("FEntity");
			}
		}

		/// <summary>
		/// 双击事件
		/// </summary>
		/// <param name="e"></param>
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FEntity");
			DynamicObject selectedEntityObj = (this.View.Model.DataObject["FEntity"] as DynamicObjectCollection)[rowIndex];
			this.View.ReturnToParentWindow(selectedEntityObj);
			this.View.Close();
		}

		/// <summary>
		/// 获取产品小类列表
		/// </summary>
		/// <returns></returns>
		public List<ProductSmallClass> GetSmallClassList(long userCode)
		{
			List<ProductSmallClass> list = new List<ProductSmallClass>();
			var resultJson = "";
			List<long> smallClassFilter = new List<long>();
			PrToPoAuthoritySetting permission = new PrToPoAuthoritySetting();
			var wxUserCode = UserServiceHelper.GetUserWxCode(this.Context, userCode);

			if (string.IsNullOrWhiteSpace(wxUserCode))
			{
				return list;
			}
			try
			{
				string url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/ProductSmallClass/GetProductSmallClassList";
				resultJson = ApigatewayUtils.InvokeWebService(url);
				var result = JsonConvert.DeserializeObject<ResponseMessage<List<ProductSmallClass>>>(resultJson);
				if (result != null && result.Data != null && result.Data.Count > 0)
				{
					var roles = GetUserRoles(wxUserCode);
					if (!roles.Contains("purchasermanager") && !roles.Contains("admin"))
					{
						permission = GetSupplierAndModelPermission(wxUserCode);
						if (permission != null && !string.IsNullOrWhiteSpace(permission.SmallClassStr))
						{
							smallClassFilter = permission.SmallClassList;
							result.Data = result.Data.Where(c => smallClassFilter.Contains(c.Id)).ToList();
						}
					}
					list = result.Data;
				}
			}
			catch (Exception ex)
			{
				//string类型，模块名称，按模块领域填
				Kingdee.BOS.Log.Logger.Info("采购管理", "PrToPo弹窗获取小类接口：" + ex.Message.ToString());
			}
			return list.Where(x => x.ParentId > 0).ToList();
		}


		// 获取供应商和型号权限
		public PrToPoAuthoritySetting GetSupplierAndModelPermission(string wxUserCode)
		{
			PrToPoAuthoritySetting authoritySetting = new PrToPoAuthoritySetting();
			if (!string.IsNullOrWhiteSpace(wxUserCode))
			{
				//获取采购员和其有权限的用户
				string url = $"workbench/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Privilege/GetAuthorityUserList";
				var getAuthorityUserComm = ApigatewayUtils.InvokePostWebService(url, "", userCode: wxUserCode);
				var getAuthorityUserResult = JsonConvertUtils.DeserializeObject<ResponseMessage<AuthorityUser[]>>(getAuthorityUserComm);

				List<long> userList = new List<long>();
				if (getAuthorityUserResult != null && getAuthorityUserResult.Code == "success")
				{
					foreach (var item in getAuthorityUserResult.Data)
					{
						userList.Add(item.UserId);
					}
				}
				var requestData = JsonConvertUtils.SerializeObject(userList);
				//获取采购员有权限的产品小类
				string scmPscUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetAuthorityProductSmallClass";
				var resultJson = ApigatewayUtils.InvokePostWebService(scmPscUrl, requestData, userCode: wxUserCode);
				var GetPscResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<long>>>(resultJson);
				if (GetPscResult != null && GetPscResult.Data != null && GetPscResult.Data.Count > 0)
				{
					authoritySetting.SmallClassList = GetPscResult.Data;
					authoritySetting.SmallClassStr = string.Join("','", GetPscResult.Data);
				}
				//获取采购员有权限的供应商
				string scmSpUrl = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Common/GetAuthoritySupplier";
				var resultSpJson = ApigatewayUtils.InvokePostWebService(scmSpUrl, requestData, userCode: wxUserCode);
				var GetSpResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<string>>>(resultSpJson);
				if (GetSpResult != null && GetSpResult.Data != null && GetSpResult.Data.Count > 0)
				{
					authoritySetting.SupplierStr = string.Join("','", GetSpResult.Data);
				}
			}
			return authoritySetting;
		}

		/// <summary>
		/// 获取用户角色
		/// </summary>
		/// <returns></returns>
		public List<string> GetUserRoles(string wxUserCode)
		{
			List<string> roles = new List<string>();
			if (!string.IsNullOrWhiteSpace(wxUserCode))
			{
				//获取采购员和其有权限的用户
				string url = $"workbench/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Privilege/GetUserRoles";
				var getAuthorityRoles = ApigatewayUtils.InvokePostWebService(url, "", userCode: wxUserCode);
				var getAuthorityRolesResult = JsonConvertUtils.DeserializeObject<ResponseMessage<List<AuthorityRoles>>>(getAuthorityRoles);

				if (getAuthorityRolesResult != null && getAuthorityRolesResult.Code == "success")
				{
					foreach (var item in getAuthorityRolesResult.Data)
					{
						roles.Add(item.Code.ToLower());
					}
				}
			}
			return roles;
		}
	}


	[Description("采购申请转采购订单弹窗供应商比价插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class PrToPoDialogSupplierPriceParity : AbstractDynamicFormPlugIn
	{
		/// <summary>
		/// 初始化绑定
		/// </summary>
		/// <param name="e"></param>
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			var list = this.View.OpenParameter.GetCustomParameter("datas") as List<LowerPriceCompare>;
			//匹配类型(0匹配供应商和价格/1选定供应商)
			var MatchType = this.View.OpenParameter.GetCustomParameter("MatchType");
			if (MatchType.Equals("1"))
			{
				this.View.SetFormTitle(new LocaleValue("找到比所选供应商提供更低价格的供应商"));
				var entryGrid = this.View.GetControl<EntryGrid>("FEntity");
				entryGrid.UpdateHeader("FSelectVendorId", "所选供应商");
				entryGrid.UpdateHeader("FLowerPriceVendorId", "更低价格供应商");
			}

			foreach (var item in list)
			{
				int rowcount = this.Model.GetEntryRowCount("FEntity");
				this.Model.CreateNewEntryRow("FEntity");
				this.Model.SetValue("FPrNo", item.PrNo, rowcount);
				this.Model.SetValue("FPrSeq", item.PrSeq, rowcount);
				this.Model.SetValue("FItemNo", item.ItemNo, rowcount);
				this.Model.SetValue("FItemDesc", item.ItemDesc, rowcount);
				this.Model.SetValue("FSelected", false, rowcount);
				this.Model.SetValue("FSelectVendorId", item.SelectVendorId, rowcount);
				this.Model.SetValue("FSelectPrice", item.SelectPrice, rowcount);
				this.Model.SetValue("FSelectMinOrderQuantity", item.SelectMinOrderQuantity, rowcount);
				this.Model.SetValue("FSelectPriceSource", item.SelectPriceSource, rowcount);
				this.Model.SetValue("FLowerSelected", true, rowcount);
				this.Model.SetValue("FLowerPriceVendorId", item.LowerPriceVendorId, rowcount);
				this.Model.SetValue("FLowerPrice", item.LowerPrice, rowcount);
				this.Model.SetValue("FLowerPriceMinOrderQuantity", item.LowerPriceMinOrderQuantity, rowcount);
				this.Model.SetValue("FLowerPriceSource", item.LowerPriceSource, rowcount);
				this.Model.SetValue("FQty", item.Qty, rowcount);
			}
			this.View.UpdateView("FEntity");
		}


		//按钮点击事件
		public override void AfterButtonClick(AfterButtonClickEventArgs e)
		{
			base.AfterButtonClick(e);
			//确认更换供应商
			if (e.Key.Equals("F_BUTTON"))
			{
				var list = this.Model.DataObject["FEntity"] as DynamicObjectCollection;
				List<LowerPriceCompare> lowerPriceList = new List<LowerPriceCompare>();
				foreach (var item in list)
				{
					if (Convert.ToString(item["FLowerSelected"]).EqualsIgnoreCase("true"))
					{
						var dy = item["FSelectVendorId"] as DynamicObject;
						var LowerDy = item["FLowerPriceVendorId"] as DynamicObject;

						lowerPriceList.Add(new LowerPriceCompare
						{
							PrNo = Convert.ToString(item["FPrNo"]),
							PrSeq = Int32.Parse(Convert.ToString(item["FPrSeq"])),
							ItemNo = Convert.ToString(item["FItemNo"]),
							ItemDesc = Convert.ToString(item["FItemDesc"]),
							SelectVendorId = Int32.Parse(Convert.ToString(dy["Id"])),
							SelectVendorCode = Convert.ToString(dy["Number"]),
							SelectVendorName = Convert.ToString(dy["Name"]),
							SelectPrice = Convert.ToDecimal(Convert.ToString(item["FSelectPrice"])),
							SelectMinOrderQuantity = Int32.Parse(Convert.ToString(item["FSelectMinOrderQuantity"])),
							SelectPriceSource = Convert.ToString(item["FSelectPriceSource"]),
							LowerPriceVendorId = Int32.Parse(Convert.ToString(LowerDy["Id"])),
							LowerPriceVendorCode = Convert.ToString(LowerDy["Number"]),
							LowerPriceVendorName = Convert.ToString(LowerDy["Name"]),
							LowerPrice = Convert.ToDecimal(Convert.ToString(item["FLowerPrice"])),
							LowerPriceMinOrderQuantity = Int32.Parse(Convert.ToString(item["FLowerPriceMinOrderQuantity"])),
							LowerPriceSource = Convert.ToString(item["FLowerPriceSource"]),
							Qty = Convert.ToDecimal(Convert.ToString(item["FQty"])),
						});
					}
				}

				this.View.ReturnToParentWindow(lowerPriceList);
				this.View.Close();
			}
		}

		/// <summary>
		/// 值更新事件
		/// </summary>
		/// <param name="e"></param>
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			//同源
			if (e.Field.Key == "FSelected")
			{
				var IsSelected = bool.Parse(e.NewValue.ToString());

				DynamicObject selectedEntityObj = (this.View.Model.DataObject["FEntity"] as DynamicObjectCollection)[e.Row];
				if (Convert.ToString(selectedEntityObj["FSelected"]).Equals(Convert.ToString(selectedEntityObj["FLowerSelected"])))
				{
					if (IsSelected)
					{
						this.Model.SetValue("FLowerSelected", false, e.Row);
					}
					else
					{
						this.Model.SetValue("FLowerSelected", true, e.Row);
					}
					this.View.UpdateView("FREntity");
					var list = this.Model.DataObject["FEntity"] as DynamicObjectCollection;

					var IsExist = false;
					for (int i = 0; i < list.Count; i++)
					{
						if (bool.Parse(Convert.ToString(list[i]["FLowerSelected"])))
						{
							IsExist = true;
							break;
						}
					}
					if (IsExist)
					{
						this.View.GetControl("F_Button").Enabled = true;
					}
					else
					{
						this.View.GetControl("F_Button").Enabled = false;
					}
				}
			}
			else if (e.Field.Key == "FLowerSelected")//非同源
			{
				DynamicObject selectedEntityObj = (this.View.Model.DataObject["FEntity"] as DynamicObjectCollection)[e.Row];
				if (Convert.ToString(selectedEntityObj["FSelected"]).Equals(Convert.ToString(selectedEntityObj["FLowerSelected"])))
				{
					var IsSelected = bool.Parse(e.NewValue.ToString());
					if (IsSelected)
					{
						this.Model.SetValue("FSelected", false, e.Row);
					}
					else
					{
						this.Model.SetValue("FSelected", true, e.Row);
					}
					this.View.UpdateView("FREntity");
					var list = this.Model.DataObject["FEntity"] as DynamicObjectCollection;

					var IsExist = false;
					for (int i = 0; i < list.Count; i++)
					{
						if (bool.Parse(Convert.ToString(list[i]["FLowerSelected"])))
						{
							IsExist = true;
							break;
						}
					}
					if (IsExist)
					{
						this.View.GetControl("F_Button").Enabled = true;
					}
					else
					{
						this.View.GetControl("F_Button").Enabled = false;
					}
				}
			}
		}
	}
}

