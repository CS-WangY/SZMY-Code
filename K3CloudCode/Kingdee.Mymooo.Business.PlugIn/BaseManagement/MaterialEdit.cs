using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
	[Description("物料表单插件"), HotUpdate]
	public class MaterialEdit : AbstractBillPlugIn
	{
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);

			if (e.Field.Key.EqualsIgnoreCase("FSalGroup"))
			{
				//税收分类编码
				var sql = "select FTAXCODEID from T_BD_MasterialGroupTaxCode where FMATERIALGROUP = @FMATERIALGROUP";
				this.View.Model.SetValue("FTaxCategoryCodeId", DBServiceHelper.ExecuteScalar<long>(this.View.Context, sql, 0, new SqlParam("@FMATERIALGROUP", KDDbType.Int64, e.NewValue)));
			}
		}

		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			var parformid = this.View.ParentFormView?.GetFormId();
			if (!parformid.IsNullOrEmpty())
			{
				long entryid = 0;
				switch (this.View.ParentFormView.GetType().Name)
				{
					case "TreeListView":
						entryid = Convert.ToInt64(((Kingdee.BOS.Web.List.ListView)this.View.ParentFormView).CurrentSelectedRowInfo?.PrimaryKeyValue);
						break;
					case "ListView":
						entryid = Convert.ToInt64(((Kingdee.BOS.Web.List.ListView)this.View.ParentFormView).CurrentSelectedRowInfo?.EntryPrimaryKeyValue);
						break;
					case "BillView":
						entryid = Convert.ToInt64(this.View.ParentFormView.Model.DataObject["Id"]);
						break;
				}

				string sSql = "";
				switch (parformid.ToUpper())
				{
					case "PRD_MO":
						sSql = $@"SELECT FINQUIRYORDER,t2.FNUMBER as FDRAWINGRECORDNO FROM T_PRD_MOENTRY t1
                                LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
                                WHERE FENTRYID={entryid}";
						break;
					case "PUR_PURCHASEORDER":
						sSql = $@"SELECT FINQUIRYORDER,t2.FNUMBER as FDRAWINGRECORDNO FROM T_PUR_POORDERENTRY t1
LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FENTRYID={entryid}";
						break;
					case "PUR_REQUISITION":
						sSql = $@"SELECT FINQUIRYORDER,t2.FNUMBER as FDRAWINGRECORDNO FROM T_PUR_ReqEntry t1
LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FENTRYID={entryid}";
						break;
					case "PLN_PLANORDER":
						sSql = $@"SELECT FINQUIRYORDER,t2.FNUMBER as FDRAWINGRECORDNO FROM T_PLN_PLANORDER t1
LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FID={entryid}";
						break;
					case "PLN_REQUIREMENTORDER":
						sSql = $@"SELECT FINQUIRYORDER,t2.FNUMBER as FDRAWINGRECORDNO FROM T_PLN_REQUIREMENTORDER t1
LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FID={entryid}";
						break;
					case "SAL_SALEORDER":
						sSql = $@"SELECT FINQUIRYORDER,t2.FNUMBER as FDRAWINGRECORDNO FROM T_SAL_ORDERENTRY t1
LEFT JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t1.FENTRYID={entryid}";
						break;
				}
				if (!sSql.IsNullOrEmpty())
				{
					var sqldata = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
					foreach (var item in sqldata)
					{
						if (Convert.ToString(item["FINQUIRYORDER"]).IsNullOrEmptyOrWhiteSpace())
						{
							return;
						}
						var Inquiryorderid = Convert.ToString(item["FINQUIRYORDER"]);
						var Drawingrecordid = Convert.ToString(item["FDRAWINGRECORDNO"]);
						var pairs = new
						{
							InquiryOrder = Inquiryorderid,
							DrawingNumber = Drawingrecordid
						};
						List<object> list = new List<object>();
						list.Add(pairs);
						var response = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/PDM/GetPDMImageinfo", JsonConvertUtils.SerializeObject(list));
						//this.View.ShowMessage($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/PDM/GetPDMImageinfo" + ":" + response);
						JObject jo = (JObject)JToken.Parse(response);
						var data = jo["data"].ToString();
						List<QuoteRequestDtocs> listdw = JsonConvertUtils.DeserializeObject<List<QuoteRequestDtocs>>(data);
						foreach (var pair in listdw)
						{
							this.Model.SetValue("FPENYDrawingRecordId", pair.DrawingRecordId);

							this.Model.SetValue("FPENYLENGTH", pair.Dimension1);
							this.Model.SetValue("FPENYWIDTH", pair.Dimension2);
							this.Model.SetValue("FPENYHEIGHT", pair.Dimension3);

							this.Model.SetValue("FPENYVolume", pair.Volume);
							this.Model.SetValue("FPENYWeight", pair.Weight);
							this.Model.SetValue("FPENYSurfaceArea", pair.SurfaceArea);

							this.Model.SetValue("FPENYPartType", pair.PartTypeName);
							this.Model.SetValue("FPENYBlankType", pair.BlankType);
							this.Model.SetValue("FPENYMaterialName", pair.MaterialName);
							this.Model.SetValue("FPENYSurfaceTreatment", pair.SurfaceTreatment);
							this.Model.SetValue("FPENYHeatTreatment", pair.HeatTreatment);
							this.Model.SetValue("FPENYMachineName", pair.MachineName);
							this.Model.SetValue("FPENYMachineId", pair.MachineId);
							this.Model.SetValue("FPENYFinalCost", pair.FinalCost);

							this.Model.SetValue("FPENYMaterialPrice", pair.MaterialPrice);
							this.Model.SetValue("FPENYCuttingHours", pair.CuttingHours);
							this.Model.SetValue("FPENYMachinePrice", pair.MachinePrice);
							this.Model.SetValue("FPENYClampingCount", pair.ClampingCount);
							this.Model.SetValue("FPENYManualCost", pair.ManualCost);
							this.Model.SetValue("FPENYSurfaceCost", pair.SurfaceCost);

							this.View.GetControl("FLinkDrawingFileUrl").SetCustomPropertyValue("Url", pair.DrawingFileUrl);
							this.View.GetControl("FLinkDrawingFileUrl").SetCustomPropertyValue("Text", pair.DrawingFileUrl);
							//this.View.GetControl("FLinkStlFileUrl").SetCustomPropertyValue("Url", pair.StlFileUrl);
							//this.View.GetControl("FLinkStlFileUrl").SetCustomPropertyValue("Text", pair.StlFileUrl);
							//this.View.GetControl("FLinkThumbnailFileUrl").SetCustomPropertyValue("Url", pair.ThumbnailFileUrl);
							//this.View.GetControl("FLinkThumbnailFileUrl").SetCustomPropertyValue("Text", pair.ThumbnailFileUrl);
						}
					}
				}

			}

		}
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			switch (e.FieldKey)
			{
				case "11":
					// 直接打开浏览器并跳转到此Url地址
					ViewCommonAction.ShowWebURL(this.View, e.FieldKey);
					this.View.SendDynamicFormAction(this.View);
					break;
			}
		}
	}


}
