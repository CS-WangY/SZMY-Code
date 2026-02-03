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

namespace Kingdee.Mymooo.Business.PlugIn.ENGManagement
{
	[Description("根据EXCEL文件，多料号同时导入BOM"), HotUpdate]
	public class ImportExcelBom : AbstractDynamicFormPlugIn
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
				List<BomImportEntity> list = new List<BomImportEntity>();
				string materialNumber;
				for (int i = 0; i < datas.Tables[0].Rows.Count; i++)
				{
					Thread.Sleep(500);
					BomImportEntity entity = new BomImportEntity();
					if (datas.Tables[0].Rows[i]["成品物料"].ToString() == "" ||
					datas.Tables[0].Rows[i]["成品物料"].ToString() == "成品物料") continue;

					entity.FParentID = datas.Tables[0].Rows[i]["上级料号"].ToString();
					materialNumber = datas.Tables[0].Rows[i]["上级料号"].ToString();
					entity.FProductCode = datas.Tables[0].Rows[i]["成品物料"].ToString();

					string sSql = $"SELECT COUNT(*) FROM dbo.T_BD_MATERIAL WHERE FNUMBER='{entity.FProductCode}'";
					var parentcount = DBServiceHelper.ExecuteScalar<long>(this.Context, sSql, 0);
					if (parentcount == 0)
					{
						throw new Exception("物料" + entity.FProductCode + "在系统中不存在!");
					}

					var number = datas.Tables[0].Rows[i]["物料号"].ToString().Trim().Replace("\r", "")?.Replace("\n", "")?.Replace("\t", "")?.Replace(" ", "");
					if (HasChinese(number))
					{
						throw new Exception("物料编码不允许中文!");
					}
					if (string.IsNullOrEmpty(number))
					{
						continue;
					}
					if (number == materialNumber)
					{
						throw new Exception("父级嵌套检查错误，子项中不要出现父项物料，请检查!");
					}
					entity.FNumber = number;
					var name = datas.Tables[0].Rows[i]["名称"].ToString().Trim().Replace("\r", "")?.Replace("\n", "")?.Replace("\t", "")?.Replace(" ", "");
					entity.FName = name;
					entity.FCount = datas.Tables[0].Rows[i]["分子用量"].ToString();
					entity.FDENOMINATOR = Convert.ToDecimal(datas.Tables[0].Rows[i]["分母用量"]);
					entity.FSCRAPRATE = Convert.ToDecimal(string.IsNullOrWhiteSpace(datas.Tables[0].Rows[i]["损耗率"].ToString()) ? 0 : datas.Tables[0].Rows[i]["损耗率"]);
					entity.FStockUnitId = datas.Tables[0].Rows[i]["库存单位"].ToString();
					entity.FBaseUnitId = datas.Tables[0].Rows[i]["基本单位"].ToString();
					//if (this.Context.CurrentOrganizationInfo.ID== 7401803)
					//{
					//	var erptype = datas.Tables[0].Rows[i]["类别"].ToString();
					//	if (erptype == "外购件")
					//	{
					//		entity.ErpClsID = 1;
					//	}
					//	else
					//	{
					//		entity.ErpClsID = 2;
					//	}
					//}

					entity.ProductSmallClassName = datas.Tables[0].Rows[i]["小类"].ToString();
					entity.ImportAmount = Convert.ToDecimal(datas.Tables[0].Rows[i]["价格"]);
					list.Add(entity);
				}
				#endregion

				ShowLog(new Message { FType = "", FMessage = string.Format("开始检查物料 {0}", datas.Tables[0].Rows.Count) });
				int index = 1;
				List<long> materials = new List<long>();
				foreach (var item in list)
				{
					Thread.Sleep(500);

					string code = item.FNumber;
					string name = item.FName;

					var scid = GetMaterialSmallIDByName(this.Context, item.ProductSmallClassName);
					SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
					if (scid != null)
					{
						productsmallclass.Id = Convert.ToInt64(scid["FID"]);
					}
					else
					{
						var mesg = "[" + item.ProductSmallClassName + "]小类不存在，请检查！";
						throw new Exception(mesg);
					}

					string baseUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
					string stockUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
					string purchaseUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
					string saleUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;

					if (item.FBaseUnitId.EqualsIgnoreCase("m") || item.FBaseUnitId.EqualsIgnoreCase("mm"))
					{
						baseUnit = "m";
						stockUnit = "mm";
						purchaseUnit = "mm";
						saleUnit = "mm";
					}

					var material = new MaterialInfo(code, name);
					material.FBaseUnitId = baseUnit;
					material.FStoreUnitID = stockUnit;
					material.FPurchaseUnitId = purchaseUnit;
					material.FPurchasePriceUnitId = purchaseUnit;
					material.FSaleUnitId = saleUnit;
					material.ProductSmallClass = productsmallclass;
					material.ErpClsID = item.ErpClsID;
					//如果是导入BOM来源的物料
					material.IsBom = true;

					ShowLog(new Message
					{
						FMType = 1,
						FType = "BD_MATERIAL",
						FTypeID = material.Id,
						FCode = material.Code,
						FMessage = string.Format("检查物料{0}", material.Code)
					});
					var results = MaterialServiceHelper.TryGetOrAdd(this.Context, material, new List<long>() { this.Context.CurrentOrganizationInfo.ID });
					if (results != null)
					{
						item.material = results;
						materials.Add(results.MasterId);
						if (item.material.FBaseUnitId != baseUnit)
						{
							ShowLog(new Message
							{
								FMType = 2,
								FType = "BD_MATERIAL",
								FTypeID = item.material.Id,
								FCode = item.FNumber,
								FMessage = string.Format("检查物料{0}基本单位不一致,请修改!", item.FNumber)
							});
						}
					}
					var rate = Convert.ToInt32(index * 100 / list.Count);
					this.View.Session[ProcessRateValue] = rate;
					index++;
				}
				MaterialServiceHelper.MaterialAllocateToAll(this.Context, materials);

				ShowLog(new Message { FMessage = "开始创建BOM" });
				index = 1;
				var groupByLastNamesQuery =
					from g in list
					group g by g.FParentID into newGroup
					orderby newGroup.Key
					select newGroup;
				ENGBomInfo[] reqbom = null;
				foreach (var item in groupByLastNamesQuery)
				{
					ShowLog(new Message
					{
						FMessage = string.Format("正在创建{0}，请稍后", item.Key)
					});
					List<ENGBomInfo> boms = new List<ENGBomInfo>();
					var dataLines = list.Where(o => o.FParentID.Equals(item.Key)).ToList();
					GetBOMInfos(dataLines, item.Key, boms);

					foreach (var itembom in dataLines)
					{
						ShowLog(new Message
						{
							FType = "BD_MATERIAL",
							FTypeID = itembom.material.Id,
							FCode = itembom.FNumber,
							FMessage = string.Format("{0}", itembom.FNumber)
						});
					}
					var accountorg = GetAccountOrgId(this.Context, this.Context.CurrentOrganizationInfo.ID);
					reqbom = ENGBomServiceHelper.TryGetOrAddsOrg(this.Context, boms.ToArray(), new long[] { this.Context.CurrentOrganizationInfo.ID, accountorg });
					foreach (var itembom in reqbom)
					{
						ShowLog(new Message
						{
							FType = "ENG_BOM",
							FTypeID = itembom.Id,
							FCode = itembom.FNUMBER,
							FMessage = string.Format("BOM{0}，创建成功", itembom.FNUMBER)
						});
					}
					//if (this.Context.CurrentOrganizationInfo.ID == 7401780)
					//{
					//	//分配BOM
					//	ENGBomServiceHelper.SendMQAllocate(this.Context, reqbom.ToList<ENGBomInfo>());
					//}

					var rate = Convert.ToInt32(index * 100 / groupByLastNamesQuery.Count());
					this.View.Session[ProcessRateValue] = rate;
					index++;
				}

				ShowLog(new Message { FMessage = "执行操作结束" });
				this.View.ShowMessage("操作已完成。");
			}
			catch (Exception ex)
			{
				ShowLog(new Message
				{
					FMType = 2,
					FMessage = ex.Message
				});
				this.View.ShowErrMessage(ex.Message);
			}
			finally
			{
				this.View.GetControl(doKey).Enabled = true;
				this.View.Session[ProcessRateValue] = 100;
			}
		}, null);
		}

		public List<ENGBomInfo> GetBOMInfos(List<BomImportEntity> bomentity, string parent, List<ENGBomInfo> bomlist)
		{
			ENGBomInfo bom = new ENGBomInfo(parent);
			bom.FMATERIALID = parent;
			var dataLines = bomentity.Where(o => o.FParentID.Equals(parent)).ToList();
			if (dataLines.Count > 0)
			{
				List<BomEntity> entitylist = new List<BomEntity>();
				foreach (var item in dataLines)
				{
					if (entitylist.Select(x => x.FMATERIALIDCHILD).Contains(item.material.Code))
					{
						continue;
					}
					BomEntity ent = new BomEntity();
					//DynamicObject materialId = item["FMATERIALID"] as DynamicObject;
					ent.FMATERIALIDCHILD = item.material.Code;
					ent.FUnitNumber = item.FStockUnitId;
					ent.FNUMERATOR = Convert.ToDecimal(item.FCount);
					ent.FSCRAPRATE = item.FSCRAPRATE;
					ent.FDENOMINATOR = item.FDENOMINATOR;
					ent.ImportAmount = item.ImportAmount;
					GetBOMInfos(bomentity, item.material.Code, bomlist);
					entitylist.Add(ent);
				}
				bom.Entity = entitylist;
				bomlist.Add(bom);
			}
			return bomlist;
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
					string sSql = $"SELECT FMATERIALID FROM dbo.T_BD_MATERIAL WHERE FUSEORGID=1 AND FNUMBER='{codeid}'";
					codeid = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "");
					break;
				case "ENG_BOM":
					sSql = $"SELECT FID FROM dbo.T_ENG_BOM WHERE FUSEORGID=1 AND FNUMBER='{codeid}'";
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
		public long GetAccountOrgId(Context ctx, long supplyOrgid)
		{
			//根据供应组织找到对应的核算组织
			var sql = @"select e.FMAINORGID
from T_ORG_ACCTSYSENTRY e
	inner join T_ORG_ACCTSYSDETAIL d on e.FENTRYID = d.FENTRYID
where e.FACCTSYSTEMID = 1 and d.FSUBORGID = @FSUBORGID";
			return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, new SqlParam("@FSUBORGID", KDDbType.Int64, supplyOrgid));
		}
	}
	public class Message
	{
		public int FMType { get; set; } = 0;
		public DateTime FDatetime { get; set; } = DateTime.Now;
		public string FType { get; set; } = "";
		public long FTypeID { get; set; } = 0;
		public string FCode { get; set; } = "";
		public string FMessage { get; set; } = "";
	}
}
