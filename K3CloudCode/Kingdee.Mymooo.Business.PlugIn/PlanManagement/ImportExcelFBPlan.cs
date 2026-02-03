using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.ServiceHelper.Excel;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.DynamicForm;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
	[Description("导入excel文件"), HotUpdate]
	public class ImportExcelFBPlan : AbstractDynamicFormPlugIn
	{
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
		}
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			if (e.Key.EqualsIgnoreCase("FImportData"))
			{
				Impoort();
				//this.View.Close();
			}
		}
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
		public void Impoort()
		{
			ExcelOperation excelOperation = new ExcelOperation();
			DataSet datas = excelOperation.ReadFromFileCustom(_filePath, 0, 0);

			if (datas == null) { this.View.ShowWarnningMessage("请选择正确的文件进行引入"); }

			if (datas != null && datas.Tables.Count > 0)
			{
				DataTable resdata = datas.Tables[0] as DataTable;
				List<string> msgs = new List<string>();
				var groupedData = from row in resdata.AsEnumerable().Where(x => Convert.ToString(x["供应商"]) != "供应商"
								  && Convert.ToString(x["供应商"]) != "")
								  group row by new
								  {
									  Category = row.Field<string>("供应商"),
									  TenderBill = row.Field<string>("分标单号")
								  } into g
								  select new
								  {
									  Category = g.Key.Category,
									  TenderBill = g.Key.TenderBill,
									  Count = g.Count()
								  };
				//bool isgroup = false;
				foreach (var itemgroup in groupedData)
				{
					if (groupedData.Where(x => x.TenderBill == itemgroup.TenderBill).Count() > 1)
					{
						if (!msgs.Exists(x => x.Contains(itemgroup.TenderBill)))
						{
							msgs.Add("分标单号:" + itemgroup.TenderBill + "分配给多个供应商!");
						}

					}
				}
				groupedData = from row in resdata.AsEnumerable().Where(x => Convert.ToString(x["车间"]) != "车间"
								  && Convert.ToString(x["车间"]) != "")
							  group row by new
							  {
								  Category = row.Field<string>("车间"),
								  TenderBill = row.Field<string>("分标单号")
							  } into g
							  select new
							  {
								  Category = g.Key.Category,
								  TenderBill = g.Key.TenderBill,
								  Count = g.Count()
							  };
				foreach (var itemgroup in groupedData)
				{
					if (groupedData.Where(x => x.TenderBill == itemgroup.TenderBill).Count() > 1)
					{
						if (!msgs.Exists(x => x.Contains(itemgroup.TenderBill)))
						{
							msgs.Add("分标单号:" + itemgroup.TenderBill + "分配给多个车间!");
						}
					}
				}
				if (msgs.Count > 0)
				{
					this.View.ShowMessage(string.Join(",", msgs) + ",是否继续", MessageBoxOptions.YesNo, new Action<MessageBoxResult>(result =>
					{
						if (result == MessageBoxResult.Yes)
						{
							resdata.Columns.Add("FSupplierID", typeof(Int64));
							resdata.Columns.Add("FWorkshopID", typeof(Int64));
							for (int i = 1; i < resdata.Rows.Count; i++)
							{
								string suppliname = Convert.ToString(resdata.Rows[i]["供应商"]);
								string sSql = $"SELECT FSUPPLIERID FROM dbo.T_BD_SUPPLIER_L WHERE FNAME='{suppliname}'";
								long supplierid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0, null);
								string workname = Convert.ToString(resdata.Rows[i]["车间"]);
								string orgname = Convert.ToString(resdata.Rows[i]["供货组织"]);
								sSql = $@"SELECT t1.FDEPTID FROM dbo.T_BD_DEPARTMENT_L t1 
                                    INNER JOIN T_BD_DEPARTMENT t2 ON t1.FDEPTID=t2.FDEPTID
                                    LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L t3 ON t2.FUSEORGID=t3.FORGID 
                                    WHERE t1.FNAME='{workname}' AND t3.FNAME='{orgname}'";
								long workshopid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0, null);
								resdata.Rows[i]["FSupplierID"] = supplierid;
								resdata.Rows[i]["FWorkshopID"] = workshopid;
							}
							int trues = 0;
							int falses = 0;
							for (int i = 1; i < resdata.Rows.Count; i++)
							{
								string sSql = $@"SELECT FDOCUMENTSTATUS FROM T_PLN_PLANORDER WHERE FBILLNO='{resdata.Rows[i]["计划单号"]}'";
								var billstatus = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "", null);
								if (billstatus != "B")
								{
									ShowLog(resdata.Rows[i]["计划单号"] + " 状态非提交，处理失败！");
									falses++;
									continue;
								}

								sSql = $@"/*dialect*/UPDATE T_PLN_PLANORDER 
                            SET FTenderBillNo='{resdata.Rows[i]["分标单号"]}',FRecordType='{resdata.Rows[i]["记录类别"]}'
                            ,FSUPPLIERID={resdata.Rows[i]["FSupplierID"]},FPRDDEPTID={resdata.Rows[i]["FWorkshopID"]}
                            WHERE FBILLNO='{resdata.Rows[i]["计划单号"]}'";
								DBServiceHelper.Execute(this.Context, sSql);
								ShowLog(resdata.Rows[i]["计划单号"] + " " + resdata.Rows[i]["FWorkshopID"] + " 更新成功！");
								trues++;
							}
							ShowLog("更新完成！共" + (resdata.Rows.Count - 1) + "条,成功" + trues + "条," + "失败" + falses + "条.");
						}
					}));
				}
				else
				{
					resdata.Columns.Add("FSupplierID", typeof(Int64));
					resdata.Columns.Add("FWorkshopID", typeof(Int64));
					for (int i = 1; i < resdata.Rows.Count; i++)
					{
						string suppliname = Convert.ToString(resdata.Rows[i]["供应商"]);
						string sSql = $"SELECT FSUPPLIERID FROM dbo.T_BD_SUPPLIER_L WHERE FNAME='{suppliname}'";
						long supplierid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0, null);
						string workname = Convert.ToString(resdata.Rows[i]["车间"]);
						string orgname = Convert.ToString(resdata.Rows[i]["供货组织"]);
						sSql = $@"SELECT t1.FDEPTID FROM dbo.T_BD_DEPARTMENT_L t1 
                                    INNER JOIN T_BD_DEPARTMENT t2 ON t1.FDEPTID=t2.FDEPTID
                                    LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L t3 ON t2.FUSEORGID=t3.FORGID 
                                    WHERE t1.FNAME='{workname}' AND t3.FNAME='{orgname}'";
						long workshopid = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0, null);
						resdata.Rows[i]["FSupplierID"] = supplierid;
						resdata.Rows[i]["FWorkshopID"] = workshopid;
					}
					int trues = 0;
					int falses = 0;
					for (int i = 1; i < resdata.Rows.Count; i++)
					{
						string sSql = $@"SELECT FDOCUMENTSTATUS FROM T_PLN_PLANORDER WHERE FBILLNO='{resdata.Rows[i]["计划单号"]}'";
						var billstatus = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "", null);
						if (billstatus != "B")
						{
							ShowLog(resdata.Rows[i]["计划单号"] + " 状态非提交，处理失败！");
							falses++;
							continue;
						}

						sSql = $@"/*dialect*/UPDATE T_PLN_PLANORDER 
                            SET FTenderBillNo='{resdata.Rows[i]["分标单号"]}',FRecordType='{resdata.Rows[i]["记录类别"]}'
                            ,FSUPPLIERID={resdata.Rows[i]["FSupplierID"]},FPRDDEPTID={resdata.Rows[i]["FWorkshopID"]}
                            WHERE FBILLNO='{resdata.Rows[i]["计划单号"]}'";
						DBServiceHelper.Execute(this.Context, sSql);
						ShowLog(resdata.Rows[i]["计划单号"] + " " + resdata.Rows[i]["FWorkshopID"] + " 更新成功！");
						trues++;
					}
					ShowLog("更新完成！共" + (resdata.Rows.Count - 1) + "条,成功" + trues + "条," + "失败" + falses + "条.");
				}


			}



			//this.View.ReturnToParentWindow(datas.Tables[0]);
		}

		private void ShowLog(string log)
		{
			var text = this.View.Model.GetValue("FRemarks");
			this.View.Model.SetValue("FRemarks", text + "\r\n" + log);
			this.View.UpdateView("FRemarks");
		}
	}
}
