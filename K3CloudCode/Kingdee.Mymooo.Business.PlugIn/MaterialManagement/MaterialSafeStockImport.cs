using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.KDThread;
using System.Threading;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.BOS.ServiceHelper.Excel;
using System.Data;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.DynamicForm;
using System.Web.UI.WebControls.WebParts;
using Kingdee.K3.SCM.Core;
using Kingdee.BOS.Core.Bill;
using System.Text.RegularExpressions;
using NPOI.OpenXmlFormats.Spreadsheet;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List.PlugIn;

namespace Kingdee.Mymooo.Business.PlugIn.ENGManagement
{
	[Description("安全库存导入表单插件"), HotUpdate]
	public class MaterialSafeStockImport : AbstractDynamicFormPlugIn
	{
		private const string ProcessRateValue = "ProcessRateValue";
		private const string barKey = "FProgressBar";
		private const string doKey = "FImportData";
		private const string logKey = "FEntity";
		private ProgressBar progressBar;

		public string _filePath;
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.Key.EqualsIgnoreCase("FFileUpdate"))
			{
				if (e.EventName.EqualsIgnoreCase("FILECHANGED"))
				{
					JObject jsonObj = JsonConvert.DeserializeObject<JObject>(e.EventArgs);
					if (jsonObj != null)
					{
						var jArray = jsonObj["NewValue"];
						string _fileName = jArray[0]["ServerFileName"].ToString();
						if (CheckFile(_fileName))
						{
							_filePath = GetFilePath(_fileName);
							EnableButton("FImportData", true);
						}
						else
						{
							EnableButton("FImportData", false);
						}
					}
					else
					{
						EnableButton("FImportData", false);
					}
				}
				else if (e.EventName.EqualsIgnoreCase("STATECHANGED"))
				{
					JObject jsonObj = JsonConvert.DeserializeObject<JObject>(e.EventArgs);
					if (jsonObj["State"].ToString() != "2")
					{
						EnableButton("FImportData", false);
					}
				}
			}
		}
		public bool CheckFile(string fname)
		{
			var array = fname.Split('.');
			if (array.Contains("xls", StringComparer.OrdinalIgnoreCase) || array.Contains("xlsx", StringComparer.OrdinalIgnoreCase))
			{
				return true;
			}
			//if (array.Length == 2 && array[1] == "xls" || array[1] == "xlsx")
			//{
			//    return true;
			//}
			else
			{
				this.View.ShowWarnningMessage("请选择正确的文件进行引入");
				return false;
			}
		}
		public string GetFilePath(string serverFileName)
		{
			string directory = "FileUpLoadServices\\UploadFiles";
			return PathUtils.GetPhysicalPath(directory, serverFileName);
		}
		public void EnableButton(string key, Boolean bEnable)
		{
			this.View.GetControl(key).Enabled = bEnable;
			this.View.StyleManager.SetVisible(key, null, bEnable);
		}

		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			progressBar = this.View.GetControl<ProgressBar>(barKey);
		}
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key.EqualsIgnoreCase(doKey))
			{
				if (string.IsNullOrEmpty(_filePath)) { this.View.ShowWarnningMessage("请选择正确的文件进行引入"); return; }
				DoSthByProgressBar();
			}
		}
		/// <summary>
		/// 更新进度
		/// </summary>
		/// <param name="e"></param>
		public override void OnQueryProgressValue(QueryProgressValueEventArgs e)
		{
			base.OnQueryProgressValue(e);
			if (!this.View.Session.ContainsKey(ProcessRateValue))
			{
				this.View.Session.Add(ProcessRateValue, 0);
			}
			// 更新进度条
			e.Value = (int)this.View.Session[ProcessRateValue];
		}


		private void DoSthByProgressBar()
		{
			ExcelOperation excelOperation = new ExcelOperation();
			DataSet datas = excelOperation.ReadFromFile(_filePath, 0, 0);

			if (datas == null) { this.View.ShowWarnningMessage("请选择正确的文件进行引入"); }

			this.View.GetControl(doKey).Enabled = false;
			this.View.Session[ProcessRateValue] = 0;
			// 启动和显示进度条
			progressBar.Visible = true;
			progressBar.Start(1);
			// 启动线程执行耗时操作，同时更新执行进度
			MainWorker.QuequeTask(this.View.Context, () =>
			{
				try
				{
					ShowLog(new Message { FMessage = "开始执行操作" });
					#region 收集所有EXCEL行数据
					List<MaterialSafeStock> list = new List<MaterialSafeStock>();
					for (int i = 0; i < datas.Tables[0].Rows.Count; i++)
					{
						Thread.Sleep(500);
						MaterialSafeStock entity = new MaterialSafeStock();
						//if (datas.Tables[0].Rows[i]["物料编码"].ToString() == "" ||
						//datas.Tables[0].Rows[i]["物料编码"].ToString() == "物料编码") continue;
						//跳过标题行
						if (i == 0)
						{
							continue;
						}
						entity.MaterialNumber = datas.Tables[0].Rows[i]["物料编码"].ToString();

						string sSql = $"SELECT FMATERIALID FROM T_BD_MATERIAL WHERE FNUMBER='{entity.MaterialNumber}' AND FUSEORGID={this.Context.CurrentOrganizationInfo.ID}";
						var mid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0);
						if (mid == 0)
						{
							throw new Exception("物料" + entity.MaterialNumber + "在系统中不存在!");
						}
						entity.MaterialId = mid;
						entity.SafeStock = Convert.ToInt64(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["安全库存量"].ToString()) ? 0 : datas.Tables[0].Rows[i]["安全库存量"]);
						entity.ReOrderGood = Convert.ToInt64(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["再订货点"].ToString()) ? 0 : datas.Tables[0].Rows[i]["再订货点"]);
						entity.EconReOrderQty = Convert.ToInt64(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["经济订货批量"].ToString()) ? 0 : datas.Tables[0].Rows[i]["经济订货批量"]);
						entity.MaxStock = Convert.ToInt64(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["最大库存"].ToString()) ? 0 : datas.Tables[0].Rows[i]["最大库存"]);
						entity.MinPOQty = Convert.ToInt64(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["最小订货量"].ToString()) ? 0 : datas.Tables[0].Rows[i]["最小订货量"]);
						entity.IncreaseQty = Convert.ToInt64(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["最小包装量"].ToString()) ? 0 : datas.Tables[0].Rows[i]["最小包装量"]);
						entity.OrderPolicy = Convert.ToInt32(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["订货策略"].ToString()) ? 0 : datas.Tables[0].Rows[i]["订货策略"]);
						list.Add(entity);
					}
					#endregion

					ShowLog(new Message { FType = "", FMessage = string.Format("开始更新物料 {0}", datas.Tables[0].Rows.Count, "条") });
					int index = 1;
					List<long> materials = new List<long>();
					foreach (var item in list)
					{
						var billView = FormMetadataUtils.CreateBillView(this.Context, "BD_SAFESTOCKBILL", item.MaterialId);
						billView.Model.DeleteEntryData("FEntity");
						//var entity = billView.Model.DataObject["SafeStockEntry"] as DynamicObjectCollection;
						//if (entity.Count <= 0)
						//{
						billView.Model.CreateNewEntryRow("FEntity");
						//}
						billView.Model.SetItemValueByID("FMaterialIdKey", item.MaterialId, 0);
						billView.InvokeFieldUpdateService("FMaterialIdKey", 0);
						var material = billView.Model.GetValue("FMaterialIdKey") as DynamicObject;
						var unit = ((DynamicObjectCollection)material["MaterialBase"])[0]["BaseUnitId_Id"];
						billView.Model.SetItemValueByID("FUnitID", unit,0);
						billView.Model.SetItemValueByID("FBaseUnitId", unit, 0);

						billView.Model.SetValue("FPlanSafeQty", item.SafeStock, 0);
						billView.Model.SetValue("FReOrderGood", item.ReOrderGood, 0);
						billView.Model.SetValue("FEconReOrderQty", item.EconReOrderQty, 0);
						billView.Model.SetValue("FMaxStock", item.MaxStock, 0);
						billView.Model.SetValue("FMinPOQty", item.MinPOQty, 0);
						billView.Model.SetValue("FIncreaseQty", item.IncreaseQty, 0);
						billView.Model.SetValue("FOrderPolicy", item.OrderPolicy, 0);
						var saveResult = SaveTargetBill(this.Context, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject });
						//清除释放网控
						billView.CommitNetworkCtrl();
						billView.InvokeFormOperation(FormOperationEnum.Close);
						billView.Close();
						//						string sSql = $@"/*dialect*/UPDATE t1 SET t1.FORDERPOLICY=0,FMinPOQty={item.MinPOQty},FIncreaseQty={item.IncreaseQty} FROM
						//T_BD_MATERIALPLAN t1 INNER JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
						//WHERE t2.FMASTERID={item.MaterialId} AND t2.FUSEORGID IN (1,224428,1043841,7348029)";
						//						DBServiceHelper.Execute(this.Context, sSql);
						string sSql = $@"/*dialect*/UPDATE t1 SET t1.FORDERPOLICY={item.OrderPolicy},FMinPOQty={item.MinPOQty},FIncreaseQty={item.IncreaseQty} FROM
T_BD_MATERIALPLAN t1 INNER JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t2.FMATERIALID={item.MaterialId} AND t2.FUSEORGID = {this.Context.CurrentOrganizationInfo.ID}";
						DBServiceHelper.Execute(this.Context, sSql);

						ShowLog(new Message
						{
							FMType = 1,
							FType = "BD_MATERIAL",
							FTypeID = item.MaterialId,
							FCode = item.MaterialNumber,
							FMessage = string.Format("检查物料{0}", item.MaterialNumber)
						});


						var rate = Convert.ToInt32(index * 100 / list.Count);
						this.View.Session[ProcessRateValue] = rate;
						index++;
					}
					ShowLog(new Message { FMessage = "执行操作结束" });
					this.View.ShowMessage("操作已完成。");
				}
				catch (Exception ex)
				{
					ShowLog(new Message { FMType = 2, FMessage = ex.Message });
					this.View.ShowErrMessage(ex.Message);
				}
				finally
				{
					this.View.GetControl(doKey).Enabled = true;
					this.View.Session[ProcessRateValue] = 100;
					this.View.SendDynamicFormAction(this.View);
				}
			}, null);
		}

		public DynamicObject GetMaterialSmallIDByName(Context ctx, string sname)
		{
			string sql = $@"SELECT t1.FID,t1.FNUMBER,t2.FNAME from T_BD_MATERIALGROUP t1
                            INNER JOIN T_BD_MATERIALGROUP_L t2 on t1.FID=t2.FID
                            WHERE t2.FNAME='{sname}'";
			var data = DBUtils.ExecuteDynamicObject(ctx, sql);
			if (data.Count > 0)
			{
				return data[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// 显示批处理的执行日志
		/// </summary>
		/// <param name="log"></param>
		private void ShowLog(Message log)
		{
			//var backColor0 = "#cfe2f3";
			//var backColor1 = "#ead1dc";
			var grid = this.View.GetControl<EntryGrid>(logKey);
			int rowcount = this.Model.GetEntryRowCount(logKey);
			this.Model.CreateNewEntryRow(logKey);
			this.Model.SetValue("FDatetime", log.FDatetime, rowcount);
			this.Model.SetValue("FType", log.FType, rowcount);
			this.Model.SetValue("FTypeID", log.FTypeID, rowcount);
			this.Model.SetValue("FCode", log.FCode, rowcount);
			this.Model.SetValue("FMessage", log.FMessage, rowcount);

			switch (log.FMType)
			{
				default:
					grid.SetForecolor("FDatetime", "#808080", rowcount);
					grid.SetForecolor("FType", "#808080", rowcount);
					grid.SetForecolor("FCode", "#808080", rowcount);
					grid.SetForecolor("FMessage", "#808080", rowcount);
					break;
				case 1:
					grid.SetForecolor("FDatetime", "#00A600", rowcount);
					grid.SetForecolor("FType", "#00A600", rowcount);
					grid.SetForecolor("FCode", "#00A600", rowcount);
					grid.SetForecolor("FMessage", "#00A600", rowcount);
					break;
				case 2:
					grid.SetForecolor("FDatetime", "#FF2525", rowcount);
					grid.SetForecolor("FType", "#FF2525", rowcount);
					grid.SetForecolor("FCode", "#FF2525", rowcount);
					grid.SetForecolor("FMessage", "#FF2525", rowcount);
					break;
			}
			//this.View.UpdateView(logKey);
		}
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			if (e.Row <= 0 || !e.FieldKey.EqualsIgnoreCase("FCode"))
			{
				return;
			}
			var grid = this.View.GetControl<EntryGrid>(logKey);
			var rowIndexs = grid.GetSelectedRows();
			var formid = Convert.ToString(this.Model.GetValue("FType", rowIndexs[0]));
			var codeid = Convert.ToString(this.Model.GetValue("FCode", rowIndexs[0]));
			switch (formid)
			{
				case "BD_MATERIAL":
					string sSql = $"SELECT FMATERIALID FROM dbo.T_BD_MATERIAL WHERE FUSEORGID={this.Context.CurrentOrganizationInfo.ID} AND FNUMBER='{codeid}'";
					codeid = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "");
					break;
				default:
					break;
			}

			var showParameter = new BillShowParameter
			{
				FormId = formid,//业务对象标识
				PKey = codeid,
				Status = OperationStatus.VIEW,//查看模式打开(EDIT编辑模式)
			};
			this.View.ShowForm(showParameter);
		}

		/// <summary>
		/// 判断字符串中是否包含中文
		/// </summary>
		/// <param name="str">需要判断的字符串</param>
		/// <returns>判断结果</returns>
		public bool HasChinese(string str)
		{
			return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
		}

		private IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
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


			return saveResult;
		}
	}

	[Description("安全库存导入列表插件"), HotUpdate]
	public class MaterialSafeStockImportListPlugIn : AbstractListPlugIn
	{
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (e.BarItemKey.EqualsIgnoreCase("PENY_ImportData"))
			{
				DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
				formParameter.FormId = "PENY_MaterialSafeStock";
				//formParameter.OpenStyle.ShowType = ShowType.Modal;
				//FID通过字符串传递过去
				//formParameter.CustomParams.Add("FID", FID.Substring(0, FID.Length - 1));
				this.View.ShowForm(formParameter);
			}
		}
	}

}
