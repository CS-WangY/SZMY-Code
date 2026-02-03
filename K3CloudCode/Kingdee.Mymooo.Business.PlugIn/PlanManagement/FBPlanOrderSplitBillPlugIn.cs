using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args.WizardForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.BOS.ExcelPrint.Core.Export.DataProvider;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.App.Data;
using Kingdee.BOS;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using System.Collections.ObjectModel;
using Kingdee.K3.SCM.Core;
using Kingdee.BOS.JSON;
using static Kingdee.K3.Core.MFG.MFGBillTypeConst;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using System.Web.UI.WebControls;
using Kingdee.BOS.Web;
using Kingdee.BOS.WebApi.FormService;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.BOS.KDThread;
using System.Threading;
using Kingdee.BOS.Web.Bill;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.BOS.Core.Metadata.FieldElement;
using static Kingdee.BOS.Core.Target.Enums.TargetEnums;
using System.Drawing;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.ServiceHelper.Excel;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using System.Data;
using System.IO;
using NPOI.XSSF.Streaming;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Functions;
using System.Drawing.Imaging;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Kingdee.K3.SCM.Core.PUR;
using Kingdee.BOS.Core.Permission;
using Kingdee.Mymooo.Business.PlugIn.PLN_Forecast;
using Kingdee.BOS.Orm;
using Kingdee.K3.MFG.PLN.App.Core.CascadeAdj.CascadeLogic;
using System.Web;
using ComponentAce.Compression.Libs.zlib;
using ICSharpCode.SharpZipLib.Zip;

using Kingdee.BOS.Core.Attachment;
using Kingdee.BOS.FileServer.Core.Object;
using Kingdee.BOS.FileServer.ProxyService;
using System.Reflection.Emit;
using Kingdee.BOS.BusinessEntity.CloudPlatform;
using System.Collections.Specialized;
using Kingdee.BOS.Web.FileServer;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.App.Core.Warn.Data;
using DBServiceHelper = Kingdee.BOS.ServiceHelper.DBServiceHelper;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
	public class TenParameter
	{
		public string TenderType { get; set; }
		public string TargetType { get; set; }
		public decimal TargetAmount { get; set; } = 0;
		public decimal FrontSingleAmount { get; set; } = 0;
		public decimal SingleAmount { get; set; } = 0;
		public string ParRecordType { get; set; }
	}
	public class ParColor
	{
		public string Color1 { get; set; } = "#C792EA";
		public string Color2 { get; set; } = "#FDE74C";
		public string Color3 { get; set; } = "#5BC0EB";
		public string Color4 { get; set; } = "#9BC53D";

		public decimal MergeRatio { get; set; } = 20;
	}
	public static class DecimalExtensions
	{
		public static decimal ToDecimal2(this decimal value)
			=> Math.Round(value, 2);

		public static decimal ToDecimal2(this double value)
			=> (decimal)Math.Round(value, 2);
	}
	public static class StringExtensions
	{
		/// <summary>
		/// 字符串末尾数字+1（保留前导零）
		/// </summary>
		/// <param name="input">输入字符串</param>
		/// <returns>新字符串</returns>
		public static string IncrementTrailingNumber(this string code)
		{
			int digitStart = code.Length;

			// 反向查找数字起始位置
			while (digitStart > 0 && char.IsDigit(code[digitStart - 1]))
			{
				digitStart--;
			}

			if (digitStart < code.Length)
			{
				string prefix = code.Substring(0, digitStart);
				string numberStr = code.Substring(digitStart);

				if (long.TryParse(numberStr, out long number))
				{
					string newNumber = (number + 1).ToString($"D{numberStr.Length}");
					return prefix + newNumber;
				}
			}

			return code + "1";
		}
		public static string DecrementTrailingNumber(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			// 从末尾开始查找数字部分
			int i = input.Length - 1;
			while (i >= 0 && char.IsDigit(input[i]))
			{
				i--;
			}

			int numberStart = i + 1;

			// 如果没有数字部分，直接返回原字符串
			if (numberStart >= input.Length)
				return input;

			// 提取数字部分
			string numberStr = input.Substring(numberStart);

			// 解析数字并减1
			if (int.TryParse(numberStr, out int number) && number > 0)
			{
				string newNumber = (number - 1).ToString();

				// 保持数字位数（可选）
				if (newNumber.Length < numberStr.Length)
				{
					newNumber = newNumber.PadLeft(numberStr.Length, '0');
				}

				// 返回新字符串
				return input.Substring(0, numberStart) + newNumber;
			}

			return input;
		}
	}
	public class ResolveTask
	{
		public string Code { get; set; }
		public bool isSuccess { get; set; }
		public ResolveTaskUrl Data { get; set; }
	}
	public class ResolveTaskUrl
	{
		public string previewUrl { get; set; }
		public bool hasOtherUrl { get; set; }
	}

	public class OrderAttachment
	{
		public string Code { get; set; }
		public bool isSuccess { get; set; }
		public List<AttachmentList> Data { get; set; }
	}
	public class AttachmentList
	{
		public long DetailId { get; set; }
		public string AttaUrl { get; set; }
	}

	public class FileInfo
	{
		public long FileId { get; set; }
		public string FileName { get; set; }
		public string FileUrl { get; set; }
		public string FileServicePath { get; set; }
	}
	public class Request
	{
		public long DetailId { get; set; }
		public string FileName { get; set; }
		public string TenderBillNo { get; set; }
	}

	public class Request3DView
	{
		public string FDemandOrgId { get; set; }
		public string FCustomerID { get; set; }
		public string FPlanBillNo { get; set; }
		public string FMATERIALID { get; set; }
		public string FMATERIALName { get; set; }
		public decimal FCostPrice { get; set; }
		public decimal FSalPrice { get; set; }
		public decimal FSalQty { get; set; }
		public decimal FSalAmount { get; set; }
		public decimal FGrossProfit { get; set; }
		public string FPreviewUrl2D { get; set; }
		public string FPreviewUrl3D { get; set; }
	}

	[Description("非标分标动态表单插件"), HotUpdate]
	public class FBPlanOrderSplitWizardPlugIn : AbstractWizardFormPlugIn
	{
		private List<long> filterSmallClassId = new List<long>();
		private List<TenParameter> TenParameterList = new List<TenParameter>();
		private ParColor parColor = new ParColor();
		public string TenderTypeform(string input) =>
			input == "850" ? "850"
			: input == "T7" ? "850"
			: input == "T6" ? "850"
			: input == "T5" ? "850"
			: input == "1580" ? "中龙门"
			: input == "2580" ? "中龙门"
			: input == "龙门" ? "中龙门"
			: input == "龙门4m*2.5m" ? "中龙门"
			: "";
		public string RecordTypeColorForm(string input) =>
			input == "肥肉" ? parColor.Color1
			: input == "肉" ? parColor.Color2
			: input == "排骨" ? parColor.Color3
			: input == "骨头" ? parColor.Color4
			: "";

		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);

		}
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			LoadDataParameter();
			InitSmallClass();
		}
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			switch (e.Field.Key)
			{
				case "FMachineName":
					var mac = this.View.Model.GetValue("FMachineName", e.Row);
					var tentype = TenderTypeform(Convert.ToString(mac));
					this.View.Model.SetValue("FPlanTenderType", tentype, e.Row);
					break;
			}
		}
		private void SetTips()
		{
			var entityKey = "FPENYEntityPlanorder"; // 单据体标识
			var entryGrid = this.View.GetControl<EntryGrid>(entityKey);
			var objs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity(entityKey));
			var fields = ((Kingdee.BOS.Core.Metadata.EntityElement.EntityAppearance)entryGrid.ControlAppearance).Entity.Fields;
			foreach (var field in fields)
			{
				for (var i = 0; i < objs.Count; ++i)
				{
					var rowIndex = i;
					// 生成tips文本
					var tipsInfo = new StringBuilder();
					tipsInfo.AppendLine("物料编码：").Append(objs[i]["FMATERIALID"]).Append(";");
					tipsInfo.AppendLine("物料名称：").Append(objs[i]["FMATERIALName"]).Append(";");
					tipsInfo.AppendLine("供货组织：").Append(objs[i]["FDemandOrgId"]).Append(";");
					tipsInfo.AppendLine("客户：").Append(objs[i]["FCustomerID"]).Append(";");
					var tip = new TooltipEntity();
					tip.E = true;
					tip.T = tipsInfo.ToString();
					entryGrid.SetCellTooltip(field.Key, tip, rowIndex);
				}
			}
		}

		/// <summary>
		/// 操作步骤切换前事件
		/// </summary>
		/// <param name="e"></param>
		public override void WizardStepChanging(WizardStepChangingEventArgs e)
		{
			base.WizardStepChanging(e);
			//第一页
			if (e.OldWizardStep.Key.Equals("FWizard0", StringComparison.OrdinalIgnoreCase))
			{
				// 跳转
				//    e.JumpWizardStepKey = "FWizard2";
				DynamicObjectCollection parlist =
						this.Model.DataObject["FPENYEntityParameter"] as DynamicObjectCollection;
				TenParameterList =
					parlist.Select(t => new TenParameter
					{
						TenderType = Convert.ToString(t["FTenderType"]),
						TargetType = Convert.ToString(t["FTargetType"]),
						TargetAmount = Convert.ToDecimal(t["FTargetAmount"]),
						FrontSingleAmount = Convert.ToDecimal(t["FFrontSingleAmount"]),
						SingleAmount = Convert.ToDecimal(t["FSingleAmount"]),
						ParRecordType = Convert.ToString(t["FParRecordType"]),
					}
					).ToList();
				parColor.Color1 = Convert.ToString(this.Model.GetValue("FColor1"));
				parColor.Color2 = Convert.ToString(this.Model.GetValue("FColor2"));
				parColor.Color3 = Convert.ToString(this.Model.GetValue("FColor3"));
				parColor.Color4 = Convert.ToString(this.Model.GetValue("FColor4"));
				parColor.MergeRatio = Convert.ToDecimal(this.Model.GetValue("FMergeRatio"));
			}
			//第一页
			if (e.OldWizardStep.Key.Equals("FWizard1", StringComparison.OrdinalIgnoreCase))
			{
				// 跳转
				//    e.JumpWizardStepKey = "FWizard2";
				DynamicObjectCollection parlist =
						this.Model.DataObject["FPENYEntityPlanorder"] as DynamicObjectCollection;
				if (parlist.Count <= 0)
				{
					if (e.UpDownEnum == 2)
					{

					}
					else
					{
						this.View.ShowErrMessage("未加载任何计划数据！");
						e.Cancel = true;
					}

				}
				if (e.UpDownEnum == 2)
				{

				}
			}
			else if (e.UpDownEnum == 2)
			{
				// 上一步
				// TODO
				//this.View.GetControl("FBOMSave").Visible = false;
				//this.View.GetControl("FFINISH").Visible = true;
			}
		}
		/// <summary>
		/// 操作步骤切换后事件
		/// </summary>
		/// <param name="e"></param>
		public override void WizardStepChanged(WizardStepChangedEventArgs e)
		{
			base.WizardStepChanged(e);
			if (e.WizardStep.Key.Equals("FWizard1", StringComparison.OrdinalIgnoreCase))
			{

			}
			if (e.WizardStep.Key.Equals("FWizard2", StringComparison.OrdinalIgnoreCase))
			{
				this.View.Model.DeleteEntryData("FAnalyseEntity");
				this.View.Model.SetValue("FRemarks", "");
				//this.View.GetControl("FLabel").Text = "可用库存:" + Math.Round(_materqty, 4);
				this.View.GetControl("FLabel1").Text = "单项标:0个";
				this.View.GetControl("FLabel2").Text = "大  标:0个";
				this.View.GetControl("FLabel3").Text = "中  标:0个";
				this.View.GetControl("FLabel4").Text = "小  标:0个";
				var calcbillno = GetCalcBillNo();
				CalculationProcessTenType(calcbillno);
			}

		}

		/// <summary>
		/// 点击完成按钮后，触发窗体关闭事件
		/// </summary>
		/// <param name="e"></param>
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key.Equals("FFINISH", StringComparison.OrdinalIgnoreCase))
			{
				// 点击完成按钮
			}
			if (e.Key.Equals("FButtonPlnQurey", StringComparison.OrdinalIgnoreCase))
			{
				LoadDataPlanOrder();
			}
			if (e.Key.Equals("FButtonParse", StringComparison.OrdinalIgnoreCase))
			{



			}
		}
		public override void AfterButtonClick(AfterButtonClickEventArgs e)
		{
			base.AfterButtonClick(e);
		}
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			switch (e.BarItemKey)
			{
				case "PENY_ParameterSave":
					PermissionAuthResult iResult = PermissionServiceHelper.FuncPermissionAuth
			(this.Context, new BusinessObject() { Id = "PENY_FBPlanOrderSplit" }, "f323992d896745fbaab4a2717c79ce2e");
					if (!iResult.Passed)
					{
						this.View.ShowErrMessage("没有权限");
						e.Cancel = true;
						return;
					}

					List<string> sqlObjects = new List<string>();
					string sSql = "/*dialect*/DELETE dbo.t_PENY_FBParameter";
					sqlObjects.Add(sSql);
					DynamicObjectCollection parlist =
						this.Model.DataObject["FPENYEntityParameter"] as DynamicObjectCollection;
					foreach (var item in parlist)
					{
						var sql = $@"/*dialect*/INSERT INTO t_PENY_FBParameter
                               (FTenderType,FTargetType,FTargetAmount,FFrontSingleAmount,FSingleAmount,FParRecordType)
                                VALUES
                                (
                                '{Convert.ToString(item["FTenderType"])}',
                                '{Convert.ToString(item["FTargetType"])}',
                                '{Convert.ToDecimal(item["FTargetAmount"])}',
                                '{Convert.ToDecimal(item["FFrontSingleAmount"])}',
                                '{Convert.ToDecimal(item["FSingleAmount"])}',
                                '{Convert.ToString(item["FParRecordType"])}'
                                )";
						sqlObjects.Add(sql);
					}
					DBServiceHelper.ExecuteBatch(this.Context, sqlObjects);
					break;
				case "PENY_ParameterInsert":
				case "PENY_ParameterDel":
					iResult = PermissionServiceHelper.FuncPermissionAuth
			(this.Context, new BusinessObject() { Id = "PENY_FBPlanOrderSplit" }, "f323992d896745fbaab4a2717c79ce2e");
					if (!iResult.Passed)
					{
						this.View.ShowErrMessage("没有权限");
						e.Cancel = true;
						return;
					}
					break;
				case "PENY_ToExcel":
					Export();
					break;
				case "PENY_ToFiles":
					To3dZip();
					break;
			}
		}
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
		}

		private void LoadDataParameter()
		{
			string sSql = "SELECT * FROM t_peny_FBParameter";
			var data = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql).ToList();
			this.View.Model.DeleteEntryData("FPENYEntityParameter");
			int rowcount = this.View.Model.GetEntryRowCount("FPENYEntityParameter");
			foreach (var item in data)
			{
				this.View.Model.CreateNewEntryRow("FPENYEntityParameter");
				this.View.Model.SetValue("FTenderType", item["FTenderType"], rowcount);
				this.View.Model.SetValue("FTargetType", item["FTargetType"], rowcount);
				this.View.Model.SetValue("FTargetAmount", item["FTargetAmount"], rowcount);
				this.View.Model.SetValue("FFrontSingleAmount", item["FFrontSingleAmount"], rowcount);
				this.View.Model.SetValue("FSingleAmount", item["FSingleAmount"], rowcount);
				this.View.Model.SetValue("FParRecordType", item["FParRecordType"], rowcount);
				rowcount++;
			}
			this.View.UpdateView("FPENYEntityParameter");
			this.View.Model.SetValue("FColor1", parColor.Color1);
			this.View.UpdateView("FColor1");
			this.View.Model.SetValue("FColor2", parColor.Color2);
			this.View.UpdateView("FColor2");
			this.View.Model.SetValue("FColor3", parColor.Color3);
			this.View.UpdateView("FColor3");
			this.View.Model.SetValue("FColor4", parColor.Color4);
			this.View.UpdateView("FColor4");
			this.View.Model.SetValue("FMergeRatio", parColor.MergeRatio);
			this.View.UpdateView("FMergeRatio");
		}

		private void LoadDataPlanOrder()
		{
			var smallclasslist = this.Model.GetValue("FMulComboSmallClass");
			List<Kingdee.BOS.Core.Permission.Organization> orgs = PermissionServiceHelper.GetUserOrg(this.View.Context);
			string userorgs = string.Join(",", orgs.Select(p => p.Id));
			string sSql = $@"/*dialect*/SELECT *
,(((ISNULL(_t1.FQTY,0)*ISNULL(_t1.FTaxPrice,0))-(ISNULL(_t1.FQTY,0)*ISNULL(_t1.FSupplierUnitPrice,0)))/NULLIF(ISNULL(_t1.FQTY,0)*ISNULL(_t1.FTaxPrice,0),0))*100
AS FGrossProfit
FROM 
(
SELECT t1.FBILLNO,t1.FDOCUMENTSTATUS,t5.FNUMBER AS MNumber,t6.FNAME AS MName,t7.FNAME AS ONumber,t8.FNAME AS CName,t1.FMachineName
,CASE WHEN t5.FMASTERID=tsm.FMASTERID THEN
t3.FSupplierUnitPrice
ELSE t1.FPENYPRICE
END AS FSupplierUnitPrice
,CASE WHEN t5.FMASTERID=tsm.FMASTERID THEN
t4.FTAXPRICE
ELSE t1.FPENYPRICE
END AS FTaxPrice
,t1.FFirmQty AS FQTY
,CASE WHEN t5.FMASTERID=tsm.FMASTERID THEN
t1.FFirmQty*t4.FTAXPRICE
ELSE t1.FFirmQty*t1.FPENYPRICE
END AS FSalAmount
,t5.FMASTERID,tsm.FMASTERID AS SalMaterialID
FROM dbo.T_PLN_PLANORDER t1
INNER JOIN dbo.T_PLN_PLANORDER_B t2 ON t1.FID=t2.FID
LEFT JOIN dbo.T_SAL_ORDERENTRY t3 ON t2.FSALEORDERENTRYID=t3.FENTRYID
LEFT JOIN dbo.T_SAL_ORDERENTRY_F t4 ON t3.FENTRYID=t4.FENTRYID
LEFT JOIN dbo.T_BD_MATERIAL t5 ON t1.FMATERIALID=t5.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIAL_L t6 ON t1.FMATERIALID=t6.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIAL tsm ON t3.FMATERIALID=tsm.FMATERIALID
LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L t7 ON t1.FDEMANDORGID=t7.FORGID
LEFT JOIN dbo.T_BD_CUSTOMER_L t8 ON t1.FPENYCUSTOMERID=t8.FCUSTID
WHERE t1.FDOCUMENTSTATUS='B' AND t1.FRELEASESTATUS=1 AND t1.FTENDERBILLNO = '' AND t1.FMACHINENAME<>'' AND t1.FPENYCustomerID<>0
AND t1.FSMALLID IN ({smallclasslist})
AND t1.FDEMANDORGID IN ({userorgs})
)_t1 ORDER BY _t1.FBILLNO,_t1.CName";
			var data = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql).ToList();
			this.View.Model.DeleteEntryData("FPENYEntityPlanorder");
			int rowcount = this.View.Model.GetEntryRowCount("FPENYEntityPlanorder");
			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FPENYEntityPlanorder"));
			foreach (var item in data)
			{
				this.View.Model.CreateNewEntryRow("FPENYEntityPlanorder");
				EntityObjs[rowcount]["FPlanBillNo"] = item["FBillNo"];
				EntityObjs[rowcount]["FPlanBillStatus"] = item["FDOCUMENTSTATUS"];
				EntityObjs[rowcount]["FMATERIALID"] = item["MNumber"];
				EntityObjs[rowcount]["FMATERIALName"] = item["MName"];
				EntityObjs[rowcount]["FDemandOrgId"] = item["ONumber"];
				EntityObjs[rowcount]["FCustomerID"] = item["CName"];
				EntityObjs[rowcount]["FMachineName"] = item["FMachineName"];
				EntityObjs[rowcount]["FCostPrice"] = item["FSupplierUnitPrice"];
				EntityObjs[rowcount]["FSalPrice"] = item["FTaxPrice"];
				EntityObjs[rowcount]["FSalQty"] = item["FQTY"];
				EntityObjs[rowcount]["FSalAmount"] = item["FSalAmount"];
				EntityObjs[rowcount]["FGrossProfit"] = item["FGrossProfit"];
				EntityObjs[rowcount]["FLinkOpenUrl"] = "查看";
				////this.View.Model.SetValue("FTenderBillNo", item["FMachineName"], rowcount);
				////this.View.Model.SetValue("FPLNTargetType", item["FMachineName"], rowcount);
				rowcount++;
			}
			CalculationRecordType();
			this.View.UpdateView("FPENYEntityPlanorder");
			SetRecordTypeColor("FPENYEntityPlanorder", "FMachineName", "FRecordType");
			SetTips();
		}

		private void InitSmallClass()
		{
			List<EnumItem> smallclass = GetSmallClass(this.View.Context);
			ComboFieldEditor fieldEditor = this.View.GetFieldEditor<ComboFieldEditor>("FMulComboSmallClass", 0);
			fieldEditor.SetComboItems(smallclass);
			//filterSmallClassId.Clear();
			filterSmallClassId.AddRange(new List<long> { 91, 92, 192, 193, 195, 196, 197, 199, 222, 241, 242, 243, 238, 239, 240, 241 });
			this.View.Model.SetValue("FMulComboSmallClass", (object)string.Join(",", filterSmallClassId));
			this.View.UpdateView("FMulComboSmallClass");
		}
		protected List<EnumItem> GetSmallClass(Context ctx)
		{
			List<EnumItem> list = new List<EnumItem>();
			string sSql = @"/*dialect*/SELECT t1.FID,t1.FNUMBER,t1.FNUMBER+' '+t2.FNAME as FNAME FROM T_BD_MATERIALGROUP t1
                            INNER JOIN T_BD_MATERIALGROUP_L t2 ON t1.FID=t2.FID
                            WHERE t1.FPARENTID=88 OR t1.FID IN (239,240,241)
                            ORDER BY t2.FNAME";
			var data = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
			foreach (DynamicObject item in data)
			{
				EnumItem val3 = new EnumItem(new DynamicObject(EnumItem.EnumItemType));
				val3.EnumId = (item["FID"].ToString());
				val3.Value = (item["FID"].ToString());
				val3.Caption = (new LocaleValue(Convert.ToString(item["FName"]), this.Context.UserLocale.LCID));
				//filterSmallClassId.Add(Convert.ToInt64(val3.EnumId));
				list.Add(val3);
			}
			return list;
		}

		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			if (e.FieldKey.EqualsIgnoreCase("FPlanBillNo"))
			{
				DynamicObject rowData;
				var planentity = this.View.BusinessInfo.GetEntity("FPENYEntityPlanorder");
				rowData = this.View.Model.GetEntityDataObject(planentity, e.Row);

				var billno = Convert.ToString(rowData[e.FieldKey]);
				string _formid = "";
				switch (e.FieldKey)
				{
					case string x when x.EqualsIgnoreCase("FPENYDeliveryNotice"):
						_formid = "SAL_DELIVERYNOTICE";
						break;
					case string x when x.EqualsIgnoreCase("FPENYSalOrderNo"):
						_formid = "SAL_SaleOrder";
						break;
					case string x when x.EqualsIgnoreCase("FPlanBillNo"):
						_formid = "PLN_PLANORDER";
						break;
					default:
						return;
				}

				if (billno != "")
				{
					var requisitionMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.Context, _formid);
					var objs = BusinessDataServiceHelper.Load(this.Context, requisitionMetadata.BusinessInfo,
						new List<SelectorItemInfo>(new[] { new SelectorItemInfo("FID") }),
						OQLFilter.CreateHeadEntityFilter($"FBillNo='{billno}'"));
					if (objs == null || objs.Length == 0)
					{
						return;
					}

					var pkId = objs[0]["Id"].ToString();
					var showParameter = new BillShowParameter
					{
						FormId = _formid,//业务对象标识
						PKey = pkId,
						Status = OperationStatus.VIEW,//查看模式打开(EDIT编辑模式)
					};
					this.View.ShowForm(showParameter);
				}
			}
			if (e.FieldKey.EqualsIgnoreCase("FPlanBillNo_A"))
			{
				DynamicObject rowData;
				var planentity = this.View.BusinessInfo.GetEntity("FAnalyseEntity");
				rowData = this.View.Model.GetEntityDataObject(planentity, e.Row);

				var billno = Convert.ToString(rowData[e.FieldKey]);
				string _formid = "";
				switch (e.FieldKey)
				{
					case string x when x.EqualsIgnoreCase("FPENYDeliveryNotice"):
						_formid = "SAL_DELIVERYNOTICE";
						break;
					case string x when x.EqualsIgnoreCase("FPENYSalOrderNo"):
						_formid = "SAL_SaleOrder";
						break;
					case string x when x.EqualsIgnoreCase("FPlanBillNo_A"):
						_formid = "PLN_PLANORDER";
						break;
					default:
						return;
				}

				if (billno != "")
				{
					var requisitionMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.Context, _formid);
					var objs = BusinessDataServiceHelper.Load(this.Context, requisitionMetadata.BusinessInfo,
						new List<SelectorItemInfo>(new[] { new SelectorItemInfo("FID") }),
						OQLFilter.CreateHeadEntityFilter($"FBillNo='{billno}'"));
					if (objs == null || objs.Length == 0)
					{
						return;
					}

					var pkId = objs[0]["Id"].ToString();
					var showParameter = new BillShowParameter
					{
						FormId = _formid,//业务对象标识
						PKey = pkId,
						Status = OperationStatus.VIEW,//查看模式打开(EDIT编辑模式)
					};
					this.View.ShowForm(showParameter);
				}
			}
			if (e.FieldKey.EqualsIgnoreCase("FLinkOpenUrl"))
			{
				DynamicObject rowData;
				var planentity = this.View.BusinessInfo.GetEntity("FPENYEntityPlanorder");
				rowData = this.View.Model.GetEntityDataObject(planentity, e.Row);
				var planbillno = Convert.ToString(rowData["FPlanBillNo"]);
				string sSql = $@"SELECT t2.FSALEORDERNO,t3.FORDERDETAILID FROM dbo.T_PLN_PLANORDER t1
                INNER JOIN dbo.T_PLN_PLANORDER_B t2 ON t1.FID=t2.FID
                LEFT JOIN dbo.T_SAL_ORDERENTRY t3 ON t2.FSALEORDERENTRYID=t3.FENTRYID
                WHERE t1.FBILLNO='{planbillno}'";
				var data = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql).ToList();
				if (data.Count > 0)
				{
					var request3DView = new Request3DView
					{
						FDemandOrgId = Convert.ToString(rowData["FDemandOrgId"]),
						FCustomerID = Convert.ToString(rowData["FCustomerID"]),
						FPlanBillNo = Convert.ToString(rowData["FPlanBillNo"]),
						FMATERIALID = Convert.ToString(rowData["FMATERIALID"]),
						FMATERIALName = Convert.ToString(rowData["FMATERIALName"]),
						FCostPrice = Convert.ToDecimal(rowData["FCostPrice"]),
						FSalPrice = Convert.ToDecimal(rowData["FSalPrice"]),
						FSalQty = Convert.ToDecimal(rowData["FSalQty"]),
						FSalAmount = Convert.ToDecimal(rowData["FSalAmount"]),
						FGrossProfit = Convert.ToDecimal(rowData["FGrossProfit"]),
					};
					var requestData = new
					{
						OrderBillNo = Convert.ToString(data.FirstOrDefault()["FSALEORDERNO"]),
						//OrderBillNo = "110004646",
						OrderDetailId = Convert.ToInt64(data.FirstOrDefault()["FORDERDETAILID"]),
						//OrderDetailId = 1581758,
						FileType = "2d",
					};
					var requestJsonData = JsonConvertUtils.SerializeObject(requestData);
					string url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/mallapi/FBResolveTask/GetPreviewUrlAndParamByOrderCodeAndItemId";
					var resultJson = ApigatewayUtils.InvokePostWebService(url, requestJsonData);
					var urlresult = JsonConvertUtils.DeserializeObject<ResolveTask>(resultJson);
					if (urlresult.isSuccess)
					{
						request3DView.FPreviewUrl2D = urlresult.Data.previewUrl;
						if (urlresult.Data.hasOtherUrl)
						{
							var requestData3d = new
							{
								OrderBillNo = Convert.ToString(data.FirstOrDefault()["FSALEORDERNO"]),
								//OrderBillNo = "110004646",
								OrderDetailId = Convert.ToInt64(data.FirstOrDefault()["FORDERDETAILID"]),
								//OrderDetailId = 1581758,
								FileType = "3d",
							};
							requestJsonData = JsonConvertUtils.SerializeObject(requestData3d);
							url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/mallapi/FBResolveTask/GetPreviewUrlAndParamByOrderCodeAndItemId";
							resultJson = ApigatewayUtils.InvokePostWebService(url, requestJsonData);
							urlresult = JsonConvertUtils.DeserializeObject<ResolveTask>(resultJson);
							request3DView.FPreviewUrl3D = urlresult.Data.previewUrl;
						}
					}
					//request3DView.FPreviewUrl2D = HttpUtility.UrlEncode("http://enttest.shunde.mymooo.com/2d-preview?pdfUrl=http://enttest.shunde.mymooo.com/imagemymooo/Attachment/20250911/2025-09-11-dcd65a8e-20b5-48a7-a575-9348a804dd06/6.03-Эͼֽ.pdf&highlightAreas=eyJJZCI6MTAzNzEsIk1hdGVyaWFsT2NyVGV4dCI6IuadkOi0qCw2MDYxLVQ2IiwiTWF0ZXJpYWxYIjowLjc4OTgsIk1hdGVyaWFsWSI6MC44NTg4LCJNYXRlcmlhbFdpZHRoIjowLjA0MjgsIk1hdGVyaWFsSGVpZ2h0IjowLjAxMzQsIlN1cmZhY2VPY3JUZXh0Ijoi6KGo6Z2i5pys6Imy6Ziz5p6B5YyW5aSE55CGIiwiU3VyZmFjZVgiOjAuMzM5NywiU3VyZmFjZVkiOjAuOTY0NywiU3VyZmFjZVdpZHRoIjowLjEwNDUsIlN1cmZhY2VIZWlnaHQiOjAuMDE2OCwiU3VyZmFjZU9jclRleHQyIjpudWxsLCJTdXJmYWNlWDIiOm51bGwsIlN1cmZhY2VZMiI6bnVsbCwiU3VyZmFjZVdpZHRoMiI6bnVsbCwiU3VyZmFjZUhlaWdodDIiOm51bGwsIkhlYXRPY3JUZXh0IjpudWxsLCJIZWF0WCI6bnVsbCwiSGVhdFkiOm51bGwsIkhlYXRXaWR0aCI6bnVsbCwiSGVhdEhlaWdodCI6bnVsbCwiUm91Z2huZXNzWCI6MC45MzYxNDU5NTIxMDk0NjQsIlJvdWdobmVzc1kiOjAuMDI1MDEwMDg0NzExNTc3MjUsIlJvdWdobmVzc1dpZHRoIjowLjA0Nzg5MDUzNTkxNzkwMTk0LCJSb3VnaG5lc3NIZWlnaHQiOjAuMDQ3MTk2NDUwMTgxNTI0ODEsIkFjY3VyYWN5WCI6MC43MzAzMzA2NzI3NDgwMDQ2LCJBY2N1cmFjeVkiOjAuMzU1Mzg1MjM1OTgyMjUwOSwiQWNjdXJhY3lXaWR0aCI6MC4wOTgzNDY2MzYyNTk5NzcyLCJBY2N1cmFjeUhlaWdodCI6MC4wMzc5MTg1MTU1MzA0NTU4MywiQ3JlYXRlVGltZSI6IjIwMjUtMDktMTFUMTg6MDg6NDciLCJVcGRhdGVUaW1lIjoiMjAyNS0wOS0xMVQxODowODo0NyJ9");
					//request3DView.FPreviewUrl3D = HttpUtility.HtmlEncode("http://www.deepseek.com");

					List<Request> listG1 = new List<Request>();
					listG1.Add(new Request
					{
						DetailId = Convert.ToInt64(data.FirstOrDefault()["FORDERDETAILID"]),
						FileName = "",
						TenderBillNo = ""
					});
					var fileInfos = GetAllFileInfos(listG1);

					var apiurl = $"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/3DViewUrl";
					var _3dViewUrl = ApigatewayUtils.InvokeWebService(apiurl);
					var parp = $"request3dView?FDemandOrgId={request3DView.FDemandOrgId}" +
						$"&FCustomerID={request3DView.FCustomerID}" +
						$"&FPlanBillNo={request3DView.FPlanBillNo}" +
						$"&FMATERIALID={request3DView.FMATERIALID}" +
						$"&FMATERIALName={request3DView.FMATERIALName}" +
						$"&FCostPrice={request3DView.FCostPrice}" +
						$"&FSalPrice={request3DView.FSalPrice}" +
						$"&FSalQty={request3DView.FSalQty}" +
						$"&FSalAmount={request3DView.FSalAmount}" +
						$"&FGrossProfit={request3DView.FGrossProfit}" +
						$"&FPreviewUrl2D={request3DView.FPreviewUrl2D}" +
						$"&FPreviewUrl3D={request3DView.FPreviewUrl3D}" +
						$"&{string.Join("&", fileInfos.Select(id => $"FFileInfos={id.FileUrl}"))}";
					ViewCommonAction.ShowWebURL(this.View, Path.Combine(_3dViewUrl, parp));

				}
			}
		}

		private void CalculationRecordType()
		{
			EntryGrid entryGrid = this.View.GetControl<EntryGrid>("FPENYEntityPlanorder");
			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FPENYEntityPlanorder"));
			for (int i = 0; i < EntityObjs.Count; i++)
			{
				var tentype = TenderTypeform(Convert.ToString(EntityObjs[i]["FMachineName"]));
				this.View.Model.SetValue("FPlanTenderType", tentype, i);
				var singAmount = Convert.ToDecimal(EntityObjs[i]["FSalAmount"]);
				if (tentype == "") continue;
				var itemlist = TenParameterList.Where(x => x.TenderType == tentype
				&& singAmount > x.FrontSingleAmount && singAmount <= x.SingleAmount).FirstOrDefault();
				if (itemlist is null)
				{
					this.View.Model.SetValue("FRecordType", "肥肉", i);
				}
				else
				{
					this.View.Model.SetValue("FRecordType", itemlist.ParRecordType, i);
				}
				if (singAmount <= 0)
				{
					this.View.Model.SetValue("FRecordType", "骨头", i);
				}
			}
		}
		private void SetRecordTypeColor(string EntryName, string MacName, string RecName)
		{
			EntryGrid entryGrid = this.View.GetControl<EntryGrid>(EntryName);
			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity(EntryName));
			for (int i = 0; i < EntityObjs.Count; i++)
			{
				var tentype = TenderTypeform(Convert.ToString(EntityObjs[i][MacName]));
				var recordType = Convert.ToString(EntityObjs[i][RecName]);
				if (tentype == "") continue;
				var fields = ((Kingdee.BOS.Core.Metadata.EntityElement.EntityAppearance)entryGrid.ControlAppearance).Entity.Fields;
				foreach (var field in fields)
				{
					entryGrid.SetBackcolor(field.Key, RecordTypeColorForm(recordType), i);
				}
			}
		}
		private void SetRowColor(string EntryName, string color, int rowindex)
		{
			EntryGrid entryGrid = this.View.GetControl<EntryGrid>(EntryName);
			var fields = ((Kingdee.BOS.Core.Metadata.EntityElement.EntityAppearance)entryGrid.ControlAppearance).Entity.Fields;
			foreach (var field in fields)
			{
				entryGrid.SetBackcolor(field.Key, color, rowindex);
			}
		}

		List<DynamicObject> PlanEntityObjs = new List<DynamicObject>();
		List<DynamicObject> ListG_1 = new List<DynamicObject>();
		List<DynamicObject> ListG_2 = new List<DynamicObject>();
		List<DynamicObject> ListG_3 = new List<DynamicObject>();
		private void CalculationProcessTenType(string calcBillNo)
		{
			this.View.ShowProcessForm(formResult => { }, true, "正在执行解析");
			// 启动线程执行耗时操作，同时更新执行进度
			MainWorker.QuequeTask(this.View.Context, () =>
			{
				this.View.Session["ProcessRateValue"] = 0;
				this.View.Session["ProcessTips"] = "";
				try
				{
					ShowLog("开始检查计划订单----------------------");
					CheckPlanOrder();
					ShowLog("开始分配计划订单----------------------");
					PlanEntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FPENYEntityPlanorder")).ToList();

					var OrgG1 = PlanEntityObjs.GroupBy(g => new { FDemandOrgId = Convert.ToString(g["FDemandOrgId"]) })
					.Select(g => new { DemandOrgId = g.Key.FDemandOrgId });

					var CustG1 = PlanEntityObjs.GroupBy(g => new { FCustomerID = Convert.ToString(g["FCustomerID"]) })
					.Select(g => new { CustomerID = g.Key.FCustomerID });

					foreach (var orgitem in OrgG1)
					{
						foreach (var custitem in CustG1)
						{
							var ListG1 = PlanEntityObjs.Where(p => Convert.ToString(p["FRecordType"]) == "肥肉"
							&& Convert.ToString(p["FCustomerID"]) == custitem.CustomerID
							&& Convert.ToString(p["FDemandOrgId"]) == orgitem.DemandOrgId)
							.OrderByDescending(g => Convert.ToDecimal(g["FSalAmount"])).ToList();
							ShowLog("分配单项标----------------------");
							//单项标
							foreach (var item in ListG1)
							{
								this.View.Model.CreateNewEntryRow("FAnalyseEntity");
								var aecount = this.View.Model.GetEntryRowCount("FAnalyseEntity") - 1;
								var billno = GetTenderBillNo(calcBillNo);
								this.View.Model.SetValue("FCalcBillNo", calcBillNo, aecount);
								this.View.Model.SetValue("FTenderBillNo", billno, aecount);
								this.View.Model.SetValue("FPlanBillNo_A", item["FPlanBillNo"], aecount);
								this.View.Model.SetValue("FDemandOrgId_A", item["FDemandOrgId"], aecount);
								this.View.Model.SetValue("FCustomerID_A", item["FCustomerID"], aecount);
								this.View.Model.SetValue("FPLNTargetType", "单项标", aecount);
								this.View.Model.SetValue("FMachineName_A", item["FMachineName"], aecount);
								this.View.Model.SetValue("FPlanTenderType_A", item["FPlanTenderType"], aecount);
								this.View.Model.SetValue("FMATERIALID_A", item["FMATERIALID"], aecount);
								this.View.Model.SetValue("FMATERIALName_A", item["FMATERIALName"], aecount);
								this.View.Model.SetValue("FSalQty_A", item["FSalQty"], aecount);
								this.View.Model.SetValue("FSalAmount_A", item["FSalAmount"], aecount);
								this.View.Model.SetValue("FRecordType_A", item["FRecordType"], aecount);
								ShowLog(item["FPlanBillNo"] + "金额:" + item["FSalAmount"] + " 被分配为 " + billno + " " + "单项标");
							}
							var GTenderType = TenParameterList.GroupBy(x => new { x.TenderType })
							.Select(g => new { g.Key.TenderType }).ToList();
							foreach (var gten in GTenderType)
							{
								var tar1 = TenParameterList.Where(x => x.TenderType == gten.TenderType && x.TargetType == "大标")
								.FirstOrDefault().TargetAmount;
								var tar2 = TenParameterList.Where(x => x.TenderType == gten.TenderType && x.TargetType == "中标")
								.FirstOrDefault().TargetAmount;
								var tar3 = TenParameterList.Where(x => x.TenderType == gten.TenderType && x.TargetType == "小标")
								.FirstOrDefault().TargetAmount;

								ListG_1 = PlanEntityObjs.Where(p => Convert.ToString(p["FRecordType"]) == "肉"
								&& Convert.ToString(p["FPlanTenderType"]) == gten.TenderType
								&& Convert.ToString(p["FCustomerID"]) == custitem.CustomerID
								&& Convert.ToString(p["FDemandOrgId"]) == orgitem.DemandOrgId)
								.OrderByDescending(g => Convert.ToDecimal(g["FSalAmount"])).ToList();
								ListG_2 = PlanEntityObjs.Where(p => Convert.ToString(p["FRecordType"]) == "排骨"
								&& Convert.ToString(p["FPlanTenderType"]) == gten.TenderType
								&& Convert.ToString(p["FCustomerID"]) == custitem.CustomerID
								&& Convert.ToString(p["FDemandOrgId"]) == orgitem.DemandOrgId)
								.OrderByDescending(g => Convert.ToDecimal(g["FSalAmount"])).ToList();
								ListG_3 = PlanEntityObjs.Where(p => Convert.ToString(p["FRecordType"]) == "骨头"
								&& Convert.ToString(p["FPlanTenderType"]) == gten.TenderType
								&& Convert.ToString(p["FCustomerID"]) == custitem.CustomerID
								&& Convert.ToString(p["FDemandOrgId"]) == orgitem.DemandOrgId)
								.OrderByDescending(g => Convert.ToDecimal(g["FSalAmount"])).ToList();

								// TODO
								ShowLog("分配大标----------------------");
								Thread.Sleep(100);
								// 报告下执行进度
								var rate = Convert.ToInt32(1 * 100 / 3);
								this.View.Session["ProcessRateValue"] = rate;
								// 进度条界面增加文字提示信息
								this.View.Session["ProcessTips"] = "正在处理大标数据";
								foreach (var Listitem1 in ListG_1.ToArray())
								{
									CalculationAnalyse_1(tar1, calcBillNo, "大标", ListG_1, ListG_2, ListG_3);
								}

								ShowLog("分配中标----------------------");
								Thread.Sleep(100);
								// 报告下执行进度
								rate = Convert.ToInt32(2 * 100 / 3);
								this.View.Session["ProcessRateValue"] = rate;
								// 进度条界面增加文字提示信息
								this.View.Session["ProcessTips"] = "正在处理中标数据";
								foreach (var Listitem1 in ListG_2.ToArray())
								{
									CalculationAnalyse_2(tar2, calcBillNo, "中标", ListG_2, ListG_3);
								}

								ShowLog("分配小标----------------------");
								Thread.Sleep(100);
								// 报告下执行进度
								rate = Convert.ToInt32(3 * 100 / 3);
								this.View.Session["ProcessRateValue"] = rate;
								// 进度条界面增加文字提示信息
								this.View.Session["ProcessTips"] = "正在处理小标数据";
								foreach (var Listitem1 in ListG_3.ToArray())
								{
									CalculationAnalyse_3(tar3, calcBillNo, "小标", ListG_3);
								}
								ShowLog("计算最后一行标的----------------------");
								var AnalyseObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity")).ToList();
								var AnalyseGroups = AnalyseObjs
								.Where(w => Convert.ToString(w["FDemandOrgId_A"]) == orgitem.DemandOrgId)
								.Where(w => Convert.ToString(w["FCustomerID_A"]) == custitem.CustomerID)
								.Where(w => Convert.ToString(w["FPlanTenderType_A"]) == gten.TenderType)
								.GroupBy(g => new { FBillNo = Convert.ToString(g["FTenderBillNo"]) })
									  .Select(s => new
									  {
										  FBillNo = s.Key.FBillNo,
										  Count = s.Count()
									  }).ToList();
								if (AnalyseGroups.Count() <= 1)
								{
									continue;
								}
								var AnalyseLastObjs = AnalyseObjs.LastOrDefault();
								if (AnalyseLastObjs != null)
								{
									string lastbillno = Convert.ToString(AnalyseLastObjs["FTenderBillNo"]);
									var lastAll = AnalyseObjs.Where(x => Convert.ToString(x["FTenderBillNo"]) == lastbillno).ToList();
									if (lastAll.Count == 1)
									{
										var seq = Convert.ToInt32(AnalyseLastObjs["Seq"]) - 1;
										this.View.Model.SetValue("FTenderBillNo", lastbillno.DecrementTrailingNumber(), seq);
										//AnalyseLastObjs["FTenderBillNo"] = ;
										ShowLog(AnalyseLastObjs["FPlanBillNo_A"] + "金额:" + AnalyseLastObjs["FSalAmount_A"] + " 只有最后一行被分配为 " + lastbillno.DecrementTrailingNumber());
									}
									else
									{
										string tenType = Convert.ToString(AnalyseLastObjs["FPlanTenderType_A"]);
										string tarType = Convert.ToString(AnalyseLastObjs["FPLNTargetType"]);
										decimal tarAmount = TenParameterList.Where(x => x.TenderType == tenType && x.TargetType == tarType)
										.FirstOrDefault().TargetAmount;
										decimal lastAmount = lastAll.Sum(s => Convert.ToDecimal(s["FSalAmount_A"]));
										decimal ratio = tarAmount * (parColor.MergeRatio / 100);
										if (lastAmount < ratio)
										{
											foreach (var lastitem in lastAll)
											{
												var seq = Convert.ToInt32(lastitem["Seq"]) - 1;
												this.View.Model.SetValue("FTenderBillNo", lastbillno.DecrementTrailingNumber(), seq);
												//lastitem["FTenderBillNo"] = lastbillno.DecrementTrailingNumber();
												ShowLog(lastitem["FPlanBillNo_A"] + "金额:" + lastAmount + " 总金额小于上限" + tarAmount + "*" + parColor.MergeRatio + "% " + "被分配为 " + lastbillno.DecrementTrailingNumber());
											}
										}
									}
								}

							}

							var AnalyseEntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity")).ToList();
							var d1 = AnalyseEntityObjs.Where(x => Convert.ToString(x["FPLNTargetType"]) == "单项标")
							.GroupBy(s => new { FTenderBillNo = Convert.ToString(s["FTenderBillNo"]), });
							var d2 = AnalyseEntityObjs.Where(x => Convert.ToString(x["FPLNTargetType"]) == "大标")
							.GroupBy(s => new { FTenderBillNo = Convert.ToString(s["FTenderBillNo"]), });
							var d3 = AnalyseEntityObjs.Where(x => Convert.ToString(x["FPLNTargetType"]) == "中标")
							.GroupBy(s => new { FTenderBillNo = Convert.ToString(s["FTenderBillNo"]), });
							var d4 = AnalyseEntityObjs.Where(x => Convert.ToString(x["FPLNTargetType"]) == "小标")
							.GroupBy(s => new { FTenderBillNo = Convert.ToString(s["FTenderBillNo"]), });
							this.View.GetControl("FLabel1").Text = $"单项标:{d1.Count()}个";
							this.View.GetControl("FLabel2").Text = $"大  标:{d2.Count()}个";
							this.View.GetControl("FLabel3").Text = $"中  标:{d3.Count()}个";
							this.View.GetControl("FLabel4").Text = $"小  标:{d4.Count()}个";
						}
					}

					UpdatePlanOrder();
					this.View.Model.DeleteEntryData("FPENYEntityPlanorder");
					this.View.ShowMessage("操作已完成。");
				}
				catch (Exception ex)
				{
					this.View.ShowErrMessage(ex.Message);
				}
				finally
				{
					// 此句必不可少，进度值100时进度条自动关闭
					this.View.Session["ProcessRateValue"] = 100;
					this.View.SendDynamicFormAction(this.View);
				}
			}, null);
		}

		private void UpdatePlanOrder()
		{
			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity"));
			foreach (var item in EntityObjs)
			{
				string sSql = $@"/*dialect*/UPDATE T_PLN_PLANORDER 
                SET 
                FCalcBillNo='{item["FCalcBillNo"]}',
                FTenderBillNo='{item["FTenderBillNo"]}',
                FPLNTargetType='{item["FPLNTargetType"]}',
                FPlanTenderType='{item["FPlanTenderType_A"]}',
                FMachineName='{item["FMachineName_A"]}',
                FSalAmount='{item["FSalAmount_A"]}',
                FRecordType='{item["FRecordType_A"]}',
                FTenderDateTime=GETDATE()
                WHERE FBILLNO='{item["FPlanBillNo_A"]}'";
				DBServiceHelper.Execute(this.Context, sSql);
			}
		}

		private void CheckPlanOrder()
		{
			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FPENYEntityPlanorder"));
			for (int i = EntityObjs.Count - 1; i >= 0; i--)
			{
				var planbillno = Convert.ToString(EntityObjs[i]["FPlanBillNo"]);
				var cust = Convert.ToString(EntityObjs[i]["FCustomerID"]);
				if (string.IsNullOrEmpty(cust))
				{
					ShowLog(planbillno + " 缺少客户信息被标记为删除不参与");
					this.View.Model.DeleteEntryRow("FPENYEntityPlanorder", i);
					continue;
				}
				var qty = Convert.ToDecimal(EntityObjs[i]["FSalQty"]);
				if (qty <= 0)
				{
					ShowLog(planbillno + " 数量为小于等于0被标记为删除不参与");
					this.View.Model.DeleteEntryRow("FPENYEntityPlanorder", i);
					continue;
				}
				var rectype = Convert.ToString(EntityObjs[i]["FRecordType"]);
				if (string.IsNullOrEmpty(rectype))
				{
					ShowLog(planbillno + " 缺少类型信息被标记为删除不参与");
					this.View.Model.DeleteEntryRow("FPENYEntityPlanorder", i);
					continue;
				}
			}
			//this.View.UpdateView("FPENYEntityPlanorder");
		}
		private void CalculationAnalyse_1(decimal targetAmt, string calcbillno, string tarname, List<DynamicObject> List_1, List<DynamicObject> List_2, List<DynamicObject> List_3)
		{
			var GS2 = List_1.Sum(x => Convert.ToDecimal(x["FSalAmount"]));
			var GS3 = List_2.Sum(x => Convert.ToDecimal(x["FSalAmount"]));
			var GS4 = List_3.Sum(x => Convert.ToDecimal(x["FSalAmount"]));
			var CB1 = GS2 + GS3 + GS4;
			//已全部分配完
			if (CB1 <= 0)
			{
				return;
			}
			var FactorA = decimal.Round(targetAmt / CB1, 4);
			var FactorB = decimal.Round(GS2 / CB1, 4);
			var Factor = Math.Max(FactorA, FactorB);
			var Argument = decimal.Round(targetAmt * Factor, 4);

			ShowLog("CB1:" + CB1 + " EB:" + GS2 + " FACTA:" + FactorA + " FACTB" + FactorB + " ARG:" + Argument + " TarA:" + targetAmt);
			foreach (var item in List_1.ToArray())
			{
				if (List_1.Count <= 0)
				{
					return;
				}
				var createBillNo = GetTenderBillNo(calcbillno);
				decimal NewAmount = 0;
				string softColor = GenerateSoftHighContrastHex();
				CalAnalyseAdd(List_1, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
				foreach (var item1 in List_1.ToArray())
				{
					if (NewAmount > Argument)
					{
						continue;
					}
					CalAnalyseAdd(List_1, calcbillno, createBillNo, tarname, ref NewAmount, softColor, Argument);
				}
				if (List_3.Count <= 0 && List_2.Count <= 0)
				{
					foreach (var item1 in List_1.ToArray())
					{
						if (NewAmount > targetAmt)
						{
							continue;
						}
						CalAnalyseAdd(List_1, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
					}
				}
				foreach (var item3 in List_3.ToArray())
				{
					if (NewAmount > targetAmt)
					{
						continue;
					}
					CalAnalyseAdd(List_3, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
				}
				foreach (var item2 in List_2.ToArray())
				{
					if (NewAmount > targetAmt)
					{
						continue;
					}
					CalAnalyseAdd(List_2, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
				}

			}


		}
		private void CalculationAnalyse_2(decimal targetAmt, string calcbillno, string tarname, List<DynamicObject> List_2, List<DynamicObject> List_3)
		{

			var GS3 = List_2.Sum(x => Convert.ToDecimal(x["FSalAmount"]));
			var GS4 = List_3.Sum(x => Convert.ToDecimal(x["FSalAmount"]));
			var CB1 = GS3 + GS4;
			//已全部分配完
			if (CB1 <= 0)
			{
				return;
			}
			var FactorA = decimal.Round(targetAmt / CB1, 4);
			var FactorB = decimal.Round(GS3 / CB1, 4);
			var Factor = Math.Max(FactorA, FactorB);
			var Argument = decimal.Round(targetAmt * Factor, 4);
			ShowLog("CB1:" + CB1 + " EB:" + GS3 + " FACTA:" + FactorA + " FACTB" + FactorB + " ARG:" + Argument + " TarA:" + targetAmt);
			foreach (var item in List_2.ToArray())
			{
				if (List_2.Count <= 0)
				{
					return;
				}
				var createBillNo = GetTenderBillNo(calcbillno);
				decimal NewAmount = 0;
				string softColor = GenerateSoftHighContrastHex();
				CalAnalyseAdd(List_2, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);

				foreach (var item1 in List_2.ToArray())
				{
					if (NewAmount > Argument)
					{
						continue;
					}
					CalAnalyseAdd(List_2, calcbillno, createBillNo, tarname, ref NewAmount, softColor, Argument);
				}

				if (List_3.Count <= 0)
				{
					foreach (var item1 in List_2.ToArray())
					{
						if (NewAmount > targetAmt)
						{
							continue;
						}
						CalAnalyseAdd(List_2, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
					}
				}
				foreach (var item3 in List_3.ToArray())
				{
					if (NewAmount > targetAmt)
					{
						continue;
					}
					CalAnalyseAdd(List_3, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
				}
			}


		}
		private void CalculationAnalyse_3(decimal targetAmt, string calcbillno, string tarname, List<DynamicObject> List_3)
		{
			var GS4 = List_3.Sum(x => Convert.ToDecimal(x["FSalAmount"]));
			var CB1 = GS4;
			//已全部分配完
			if (CB1 <= 0)
			{
				goto lable1;
			}
			var FactorA = decimal.Round(targetAmt / CB1, 4);
			var FactorB = decimal.Round(GS4 / CB1, 4);
			var Factor = Math.Max(FactorA, FactorB);
			var Argument = decimal.Round(targetAmt * Factor, 4);
			ShowLog("CB1:" + CB1 + " EB:" + GS4 + " FACTA:" + FactorA + " FACTB" + FactorB + " ARG:" + Argument + " TarA:" + targetAmt);
		lable1:
			foreach (var item in List_3.ToArray())
			{
				if (List_3.Count <= 0)
				{
					return;
				}
				var createBillNo = GetTenderBillNo(calcbillno);
				decimal NewAmount = 0;
				string softColor = GenerateSoftHighContrastHex();
				CalAnalyseAdd(List_3, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);

				foreach (var item3 in List_3.ToArray())
				{
					if (NewAmount > targetAmt)
					{
						continue;
					}
					CalAnalyseAdd(List_3, calcbillno, createBillNo, tarname, ref NewAmount, softColor, targetAmt);
				}

			}


		}

		private void CalAnalyseAdd(List<DynamicObject> AnalyseList, string calcbillno, string billno, string tarname, ref decimal NewAmount, string color, decimal Argument)
		{
			//取第一行
			var firstitem = AnalyseList.FirstOrDefault();
			if (firstitem == null)
			{
				return;
			}
			this.View.Model.CreateNewEntryRow("FAnalyseEntity");
			var aecount = this.View.Model.GetEntryRowCount("FAnalyseEntity") - 1;
			this.View.Model.SetValue("FCalcBillNo", calcbillno, aecount);
			this.View.Model.SetValue("FTenderBillNo", billno, aecount);
			this.View.Model.SetValue("FPlanBillNo_A", firstitem["FPlanBillNo"], aecount);
			this.View.Model.SetValue("FDemandOrgId_A", firstitem["FDemandOrgId"], aecount);
			this.View.Model.SetValue("FCustomerID_A", firstitem["FCustomerID"], aecount);
			this.View.Model.SetValue("FPLNTargetType", tarname, aecount);
			this.View.Model.SetValue("FMachineName_A", firstitem["FMachineName"], aecount);
			this.View.Model.SetValue("FPlanTenderType_A", firstitem["FPlanTenderType"], aecount);
			this.View.Model.SetValue("FMATERIALID_A", firstitem["FMATERIALID"], aecount);
			this.View.Model.SetValue("FMATERIALName_A", firstitem["FMATERIALName"], aecount);
			this.View.Model.SetValue("FSalQty_A", firstitem["FSalQty"], aecount);
			this.View.Model.SetValue("FSalAmount_A", firstitem["FSalAmount"], aecount);
			this.View.Model.SetValue("FRecordType_A", firstitem["FRecordType"], aecount);
			NewAmount += Convert.ToDecimal(firstitem["FSalAmount"]);
			SetRowColor("FAnalyseEntity", color, aecount);
			ShowLog(firstitem["FPlanBillNo"] + "金额:" + firstitem["FSalAmount"] + " 被分配为 " + billno + " " + tarname);
			AnalyseList.Remove(firstitem);
			//是否超过标准
			if (NewAmount >= Argument)
			{
				ShowLog("累计金额:" + NewAmount + " 不继续分配");
				return;
			}
			//取最后一行
			var laseitem = AnalyseList.LastOrDefault();
			if (laseitem == null || firstitem == laseitem)
			{
				return;
			}
			this.View.Model.CreateNewEntryRow("FAnalyseEntity");
			aecount = this.View.Model.GetEntryRowCount("FAnalyseEntity") - 1;
			this.View.Model.SetValue("FCalcBillNo", calcbillno, aecount);
			this.View.Model.SetValue("FTenderBillNo", billno, aecount);
			this.View.Model.SetValue("FPlanBillNo_A", laseitem["FPlanBillNo"], aecount);
			this.View.Model.SetValue("FDemandOrgId_A", laseitem["FDemandOrgId"], aecount);
			this.View.Model.SetValue("FCustomerID_A", laseitem["FCustomerID"], aecount);
			this.View.Model.SetValue("FPLNTargetType", tarname, aecount);
			this.View.Model.SetValue("FMachineName_A", laseitem["FMachineName"], aecount);
			this.View.Model.SetValue("FPlanTenderType_A", laseitem["FPlanTenderType"], aecount);
			this.View.Model.SetValue("FMATERIALID_A", laseitem["FMATERIALID"], aecount);
			this.View.Model.SetValue("FMATERIALName_A", laseitem["FMATERIALName"], aecount);
			this.View.Model.SetValue("FSalQty_A", laseitem["FSalQty"], aecount);
			this.View.Model.SetValue("FSalAmount_A", laseitem["FSalAmount"], aecount);
			this.View.Model.SetValue("FRecordType_A", laseitem["FRecordType"], aecount);
			SetRowColor("FAnalyseEntity", color, aecount);
			ShowLog(laseitem["FPlanBillNo"] + "金额:" + laseitem["FSalAmount"] + " 被分配为 " + billno + " " + tarname);
			NewAmount += Convert.ToDecimal(laseitem["FSalAmount"]);
			AnalyseList.Remove(laseitem);
		}

		private void ShowLog(string log)
		{
			var text = this.View.Model.GetValue("FRemarks");
			this.View.Model.SetValue("FRemarks", text + "\r\n" + log);
			this.View.UpdateView("FRemarks");
		}

		private string GetTenderBillNo(string calcBill)
		{
			string BillNo = "";
			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity"));
			var lastBillNo = EntityObjs.Where(x => Convert.ToString(x["FTenderBillNo"]) != "").GroupBy(s => new
			{
				FTenderBillNo = Convert.ToString(s["FTenderBillNo"])
			}).LastOrDefault();
			if (lastBillNo != null)
			{
				BillNo = lastBillNo.Key.FTenderBillNo.IncrementTrailingNumber();
			}
			else
			{
				BillNo = calcBill + "-" + "001";
			}

			return BillNo;

		}

		private string GetCalcBillNo()
		{
			string BillNo = "";

			var prefixBillno = string.Format("B{0}", System.DateTime.Now.ToString("yyyyMMdd"));
			string sSql = $@"/*dialect*/SELECT TOP 1 FCalcBillNo FROM dbo.T_PLN_PLANORDER WHERE FCalcBillNo LIKE '{prefixBillno}%' ORDER BY FCalcBillNo DESC";
			var billno = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "", null);
			if (!string.IsNullOrEmpty(billno))
			{
				BillNo = billno.IncrementTrailingNumber();
			}
			else
			{
				BillNo = prefixBillno + "01";
			}
			return BillNo;
		}

		private static readonly Random _random = new Random();

		/// <summary>
		/// 生成随机的高对比度柔和颜色（HEX格式，如 #A4D8F2）
		/// </summary>
		public static string GenerateSoftHighContrastHex()
		{
			// 1. 固定高亮度（70-90%）和中等饱和度（30-60%）
			double lightness = 0.7 + _random.NextDouble() * 0.2;  // 70-90% 亮度
			double saturation = 0.3 + _random.NextDouble() * 0.3; // 30-60% 饱和度

			// 2. 随机色相（0-360度）
			double hue = _random.NextDouble() * 360;

			// 3. 转换 HSL 到 RGB，再转 HEX
			Color color = HslToRgb(hue, saturation, lightness);
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
		}

		// HSL 转 RGB 的辅助方法
		private static Color HslToRgb(double h, double s, double l)
		{
			double c = (1 - Math.Abs(2 * l - 1)) * s;
			double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
			double m = l - c / 2;

			double r = 0, g = 0, b = 0;

			if (h < 60) { r = c; g = x; }
			else if (h < 120) { r = x; g = c; }
			else if (h < 180) { g = c; b = x; }
			else if (h < 240) { g = x; b = c; }
			else if (h < 300) { r = x; b = c; }
			else { r = c; b = x; }

			return Color.FromArgb(
				(int)((r + m) * 255),
				(int)((g + m) * 255),
				(int)((b + m) * 255)
			);
		}

		private void Export()
		{
			var tb = BuildData();

			var showParameter = new DynamicFormShowParameter();
			showParameter.FormId = "BOS_FileDownLoad";
			showParameter.OpenStyle.ShowType = ShowType.Modal;
			showParameter.CustomParams.Add("url", NPOIExport(tb));
			this.View.ShowForm(showParameter);

		}
		private string NPOIExport(DataTable tb)
		{
			var wb = new SXSSFWorkbook();
			SXSSFSheet sheet = (SXSSFSheet)wb.CreateSheet("demo");
			var tempXmlFile = sheet._writer.TemporaryFilePath();

			string fileName = string.Format("NPOI-demo-{0}.xlsx", DateTime.Now.ToString("yyyyMMddhhmmss"));
			var filePath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, fileName);
			var fileUrl = PathUtils.GetServerPath(KeyConst.TEMPFILEPATH, PathUtils.UrlEncode(fileName));

			try
			{
				// 3. 预定义颜色池（NPOI索引颜色）
				short[] colorPalette = new short[]
				{
					IndexedColors.LightTurquoise.Index,
					IndexedColors.LightGreen.Index,
					IndexedColors.LightOrange.Index,
					IndexedColors.LightYellow.Index,
					IndexedColors.LightCornflowerBlue.Index
				};

				// 4. 创建颜色字典和样式缓存
				Dictionary<string, short> codeColorMap = new Dictionary<string, short>();
				Dictionary<short, ICellStyle> styleCache = new Dictionary<short, ICellStyle>();

				for (int i = 0; i < tb.Rows.Count; i++)
				{
					IRow row = sheet.CreateRow(i);
					for (int colIndex = 0; colIndex < tb.Columns.Count; colIndex++)
					{
						string code = tb.Rows[i]["FTenderBillNo"].ToString();
						if (!codeColorMap.TryGetValue(code, out short colorIndex))
						{
							colorIndex = colorPalette[codeColorMap.Count % colorPalette.Length];
							codeColorMap.Add(code, colorIndex);
						}

						ICell cell = row.CreateCell(colIndex);
						ICellStyle textCellStyle = wb.CreateCellStyle();
						textCellStyle.DataFormat = wb.CreateDataFormat().GetFormat("@");
						cell.CellStyle = textCellStyle;
						if (!styleCache.TryGetValue(colorIndex, out ICellStyle cellStyle))
						{
							textCellStyle.FillForegroundColor = colorIndex;
							textCellStyle.FillPattern = FillPattern.SolidForeground;
						}
						cell.SetCellValue(tb.Rows[i][colIndex].ToString());
					}
				}

				//写入Excel
				using (FileStream fs = File.OpenWrite(filePath))
				{
					wb.Write(fs);
				}
			}
			finally
			{
				if (wb != null)
				{
					wb.Close();
				}
				//清理中间临时xml文件
				if (File.Exists(tempXmlFile))
				{
					File.Delete(tempXmlFile);
				}

			}
			return fileUrl;
		}
		private void NPOIToTempFileExport(DataTable tb, string filePath)
		{
			var wb = new SXSSFWorkbook();
			SXSSFSheet sheet = (SXSSFSheet)wb.CreateSheet("demo");
			var tempXmlFile = sheet._writer.TemporaryFilePath();

			//string fileName = string.Format("NPOI-demo-{0}.xlsx", DateTime.Now.ToString("yyyyMMddhhmmss"));
			try
			{
				// 3. 预定义颜色池（NPOI索引颜色）
				short[] colorPalette = new short[]
				{
					IndexedColors.LightTurquoise.Index,
					IndexedColors.LightGreen.Index,
					IndexedColors.LightOrange.Index,
					IndexedColors.LightYellow.Index,
					IndexedColors.LightCornflowerBlue.Index
				};

				// 4. 创建颜色字典和样式缓存
				Dictionary<string, short> codeColorMap = new Dictionary<string, short>();
				Dictionary<short, ICellStyle> styleCache = new Dictionary<short, ICellStyle>();

				for (int i = 0; i < tb.Rows.Count; i++)
				{
					IRow row = sheet.CreateRow(i);
					for (int colIndex = 0; colIndex < tb.Columns.Count; colIndex++)
					{
						string code = tb.Rows[i]["FTenderBillNo"].ToString();
						if (!codeColorMap.TryGetValue(code, out short colorIndex))
						{
							colorIndex = colorPalette[codeColorMap.Count % colorPalette.Length];
							codeColorMap.Add(code, colorIndex);
						}

						ICell cell = row.CreateCell(colIndex);
						ICellStyle textCellStyle = wb.CreateCellStyle();
						textCellStyle.DataFormat = wb.CreateDataFormat().GetFormat("@");
						cell.CellStyle = textCellStyle;
						if (!styleCache.TryGetValue(colorIndex, out ICellStyle cellStyle))
						{
							textCellStyle.FillForegroundColor = colorIndex;
							textCellStyle.FillPattern = FillPattern.SolidForeground;
						}
						cell.SetCellValue(tb.Rows[i][colIndex].ToString());
					}
				}

				//写入Excel
				using (FileStream fs = File.OpenWrite(filePath))
				{
					wb.Write(fs);
				}
			}
			finally
			{
				if (wb != null)
				{
					wb.Close();
				}
				//清理中间临时xml文件
				if (File.Exists(tempXmlFile))
				{
					File.Delete(tempXmlFile);
				}

			}
		}
		private DataTable BuildTempData(List<DynamicObject> EntityObjs)
		{
			var sheetName = "NPOI Demo";
			var tb = new DataTable(sheetName);
			var entryGrid = this.View.GetControl<EntryGrid>("FAnalyseEntity");
			var fields = ((Kingdee.BOS.Core.Metadata.EntityElement.EntityAppearance)entryGrid.ControlAppearance).Entity.Fields.OrderBy(x => x.Tabindex);
			foreach (var field in fields)
			{
				tb.Columns.Add(field.Key);
			}
			tb.Columns.Add("FSupplier");
			tb.Columns.Add("FWorkshop");
			//构造表头
			var newRow = tb.NewRow();
			foreach (var field in fields)
			{
				newRow[field.Key] = field.ToString();
			}
			newRow["FSupplier"] = "供应商";
			newRow["FWorkshop"] = "车间";
			tb.Rows.Add(newRow);
			//newRow["FDate"] = "日期";

			foreach (var item in EntityObjs)
			{
				var row = tb.NewRow();
				foreach (var field in fields)
				{
					row[field.Key] = item[field.Key];
				}
				tb.Rows.Add(row);
			}

			return tb;
		}

		private DataTable BuildData()
		{
			var sheetName = "NPOI Demo";
			var tb = new DataTable(sheetName);

			var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity"));
			foreach (var item in EntityObjs)
			{
				string sSql = $"SELECT CASE FISAPLACCEPT WHEN 0 THEN '否' ELSE '是' END FISAPLACCEPT FROM dbo.T_PLN_PLANORDER WHERE FBILLNO='{item["FPlanBillNo_A"]}'";
				item["FisAplAccept_A"] = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "否");
			}
			var entryGrid = this.View.GetControl<EntryGrid>("FAnalyseEntity");
			var fields = ((Kingdee.BOS.Core.Metadata.EntityElement.EntityAppearance)entryGrid.ControlAppearance).Entity.Fields.OrderBy(x => x.Tabindex);
			foreach (var field in fields)
			{
				tb.Columns.Add(field.Key);
			}
			tb.Columns.Add("FSupplier");
			tb.Columns.Add("FWorkshop");
			//构造表头
			var newRow = tb.NewRow();
			foreach (var field in fields)
			{
				newRow[field.Key] = field.ToString();
			}
			newRow["FSupplier"] = "供应商";
			newRow["FWorkshop"] = "车间";
			tb.Rows.Add(newRow);
			//newRow["FDate"] = "日期";

			foreach (var item in EntityObjs)
			{
				var row = tb.NewRow();
				foreach (var field in fields)
				{
					row[field.Key] = item[field.Key];
				}
				tb.Rows.Add(row);
			}

			return tb;
		}


		private void To3dZip()
		{
			var _temppath = HttpContext.Current.Server.MapPath(Kingdee.BOS.Core.KeyConst.TEMPFILEPATH);

			this.View.ShowProcessForm(formResult => { }, true, "正在打包下载");
			// 启动线程执行耗时操作，同时更新执行进度
			MainWorker.QuequeTask(this.View.Context, () =>
			{
				this.View.Session["ProcessRateValue"] = 0;
				this.View.Session["ProcessTips"] = "";
				try
				{
					string downloadPath = Path.Combine(_temppath, Guid.NewGuid().ToString());
					Directory.CreateDirectory(downloadPath);

					var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity"));
					var indexrows = EntityObjs.Count;
					var TenderGl = EntityObjs.GroupBy(g => new { FTenderBillNo = Convert.ToString(g["FTenderBillNo"]) })
					.Select(g => new { FTenderBillNo = g.Key.FTenderBillNo }).OrderBy(o => o.FTenderBillNo);

					var tempDt = BuildTempData(EntityObjs.ToList());
					NPOIToTempFileExport(tempDt, Path.Combine(downloadPath, string.Format("{0}.xlsx"
						, Convert.ToString(EntityObjs.FirstOrDefault()["FCalcBillNo"]))));
					int rate = 1;
					foreach (var gentry in TenderGl)
					{
						string tenderBillNo = gentry.FTenderBillNo;
						var ListG = EntityObjs.Where(p => Convert.ToString(p["FTenderBillNo"]) == gentry.FTenderBillNo).ToList();

						List<Request> listG1 = new List<Request>();
						foreach (var item in ListG)
						{
							Thread.Sleep(500);
							// 报告下执行进度
							this.View.Session["ProcessRateValue"] = Convert.ToInt32(rate * 100 / indexrows);
							// 进度条界面增加文字提示信息
							this.View.Session["ProcessTips"] = "下载:" + tenderBillNo + "-->" + Convert.ToString(item["FMATERIALID_A"]);

							string sSql = $@"SELECT t3.FORDERDETAILID FROM dbo.T_PLN_PLANORDER t1
INNER JOIN dbo.T_PLN_PLANORDER_B t2 ON t1.FID=t2.FID
LEFT JOIN dbo.T_SAL_ORDERENTRY t3 ON t2.FSALEORDERENTRYID=t3.FENTRYID
WHERE t1.FBILLNO='{item["FPlanBillNo_A"]}'";
							var saldetailid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0, null);
							//listG1.Add(new Request { DetailId = saldetailid });
							listG1.Add(new Request
							{
								DetailId = saldetailid,
								FileName = Convert.ToString(item["FMATERIALID_A"]),
								TenderBillNo = Convert.ToString(item["FTenderBillNo"])
							});
							rate++;
						}
						var fileInfos = GetAllFileInfos(listG1);
						string createPath = Path.Combine(downloadPath, tenderBillNo);
						Directory.CreateDirectory(createPath);
						tempDt = BuildTempData(ListG);
						NPOIToTempFileExport(tempDt, Path.Combine(createPath, string.Format("{0}.xlsx", tenderBillNo)));
						DownloadAttachments(fileInfos, createPath);
					}
					//等待2秒下载完
					//Thread.Sleep(2000);
					var calbillno = Convert.ToString(EntityObjs.FirstOrDefault()["FCALCBILLNO"]);
					var zipfilename = string.Format("{0}_{1}.zip", calbillno, DateTime.Now.ToString("yyyyMMddhhmmss"));
					//var zipFilePath = Path.Combine(_temppath, zipfilename);
					var filePath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, zipfilename);
					var fileUrl = PathUtils.GetServerPath(KeyConst.TEMPFILEPATH, PathUtils.UrlEncode(zipfilename));
					CreateZipFile(downloadPath, filePath);
					//删除临时文件夹
					DeleteDirectory(downloadPath);
					ShowZipDownLoadForm(fileUrl);
					this.View.ShowMessage("操作已完成。");
				}
				catch (Exception ex)
				{
					this.View.ShowErrMessage(ex.Message);
				}
				finally
				{
					// 此句必不可少，进度值100时进度条自动关闭
					this.View.Session["ProcessRateValue"] = 100;
					this.View.SendDynamicFormAction(this.View);
				}
			}, null);
		}

		/// <summary>
		/// 批量下载附件，返回下载后的文件路径
		/// </summary>
		/// <param name="fileInfos"></param>
		/// <returns></returns>
		public void DownloadAttachments(FileInfo[] fileInfos, string downloadPath)
		{
			var fileGroupList = fileInfos.GroupBy(g => g.FileId, g => g.FileName)
				.Select(g => new { FileId = g.Key }).OrderBy(b => b.FileId).ToList();
			foreach (var fileGroup in fileGroupList)
			{
				int index = 1;
				foreach (var fileInfo in fileInfos.Where(w => w.FileId == fileGroup.FileId).ToList())
				{
					var downloader = new HttpWebRequestDownloader();
					var fileName = string.Format("{0}", fileInfo.FileName);
					string extension = Path.GetExtension(fileInfo.FileUrl);
					string targetPath = downloadPath + "\\" + fileName + "_" + index + extension;
					downloader.DownloadFile(fileInfo.FileUrl, targetPath);
					fileInfo.FileServicePath = targetPath;
					index++;
				}
			}
		}

		/// <summary>
		/// 获取单据体中所有附件的FileId
		/// </summary>
		/// <param name="entityKey"></param>
		/// <returns></returns>
		FileInfo[] GetAllFileInfos(List<Request> fileInfo)
		{
			var requestJsonData = JsonConvertUtils.SerializeObject(fileInfo);
			string url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/mallapi/SalesOrder/GetOrderAttachmentList";
			var resultJson = ApigatewayUtils.InvokePostWebService(url, requestJsonData);
			var OrderAttachmentList = JsonConvertUtils.DeserializeObject<OrderAttachment>(resultJson);
			List<FileInfo> fileInfos = new List<FileInfo>();
			foreach (var item in OrderAttachmentList.Data)
			{
				fileInfos.Add(new FileInfo
				{
					FileId = item.DetailId,
					FileName = fileInfo.Where(x => x.DetailId == item.DetailId).FirstOrDefault()?.FileName,
					FileUrl = item.AttaUrl.ToString()
				});
			}
			return fileInfos.ToArray();
		}

		private void CreateZipFile(string sourceDirectory, string zipFilePath)
		{
			if (!Directory.Exists(sourceDirectory))
			{
				return;
			}
			ZipOutputStream stream = null;
			try
			{
				stream = new ZipOutputStream(File.Create(zipFilePath));
				stream.SetLevel(0); // 压缩级别 0-9
				byte[] buffer = new byte[4096]; //缓冲区大小
				string[] filenames = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
				foreach (string file in filenames)
				{
					ZipEntry entry = new ZipEntry(file.Replace(sourceDirectory, "").TrimStart('\\'));
					entry.DateTime = DateTime.Now;
					stream.PutNextEntry(entry);
					using (FileStream fs = File.OpenRead(file))
					{
						int sourceBytes;
						do
						{
							sourceBytes = fs.Read(buffer, 0, buffer.Length);
							stream.Write(buffer, 0, sourceBytes);
						} while (sourceBytes > 0);
					}
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Finish();
					stream.Close();
				}
			}
		}

		private void ShowZipDownLoadForm(string zipfilepath)
		{
			var showParameter = new DynamicFormShowParameter();
			showParameter.FormId = "BOS_FileDownLoad";
			showParameter.OpenStyle.ShowType = ShowType.Modal;
			showParameter.CustomParams.Add("url", zipfilepath);
			this.View.ShowForm(showParameter);
		}
		private void DeleteDirectory(string path)
		{
			DirectoryInfo dir = new DirectoryInfo(path);
			if (dir.Exists)
			{
				DirectoryInfo[] childs = dir.GetDirectories();
				foreach (DirectoryInfo child in childs)
				{
					child.Delete(true);
				}
				dir.Delete(true);
			}
		}
	}
}
