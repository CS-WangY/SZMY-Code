using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.Web;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using System.Threading;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
	[Description("计划订单列表插件"), HotUpdate]
	public class PlanOrderBillListPlugin : AbstractListPlugIn
	{
		long[] orglongs = new long[] { 7401803, 7401821, 14053641 };
		public override void AfterCreateSqlBuilderParameter(SqlBuilderParameterArgs e)
		{
			base.AfterCreateSqlBuilderParameter(e);
			// 附加常规过滤条件
			//e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.JoinFilterString("FBillNo LIKE '%DD%'");
			if (e.sqlBuilderParameter.FilterClauseWihtKey.Contains("FFirmQty>0"))
			{
				e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.Replace("FFirmQty>0", "1=1");
			}

		}
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			if (e.BarItemKey.Equals("PENY_FBSplit", StringComparison.OrdinalIgnoreCase))
			{
				//权限项内码，通过 T_SEC_PermissionItem 权限项表格进行查询。
				string permissionItem = "68a57cd2d9dbc8";
				PermissionAuthResult permissionAuthResult = this.View.Model.FuncPermissionAuth(
					new string[] { "" }, permissionItem, null, false).FirstOrDefault();
				if (permissionAuthResult != null && !permissionAuthResult.Passed)
				{
					this.View.ShowErrMessage("没有权限");
					e.Cancel = true;
					return;
				}
				DynamicFormShowParameter param = new DynamicFormShowParameter();
				param.Resizable = true;
				param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
				param.FormId = "PENY_FBPlanOrderSplit";
				//param.CustomComplexParams.Add("MaterialNumber", materialNumber);
				this.View.ShowForm(param, new Action<FormResult>((result) =>
				{
					if (result.ReturnData != null)
					{

					}
				}));
			}
			if (e.BarItemKey.Equals("PENY_FBSplitInExcel", StringComparison.OrdinalIgnoreCase))
			{
				//权限项内码，通过 T_SEC_PermissionItem 权限项表格进行查询。
				string permissionItem = "68a59cd6d9dc53";
				PermissionAuthResult permissionAuthResult = this.View.Model.FuncPermissionAuth(
					new string[] { "" }, permissionItem, null, false).FirstOrDefault();
				if (permissionAuthResult != null && !permissionAuthResult.Passed)
				{
					this.View.ShowErrMessage("没有权限");
					e.Cancel = true;
					return;
				}
				DynamicFormShowParameter param = new DynamicFormShowParameter();
				param.Resizable = false;
				param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
				param.FormId = "PENY_FBPlanOrderInExcel";
				this.View.ShowForm(param, new Action<FormResult>((result) =>
				{

				}));
			}
			if (e.BarItemKey.Equals("PENY_FBSplitOutExcel", StringComparison.OrdinalIgnoreCase))
			{
				//权限项内码，通过 T_SEC_PermissionItem 权限项表格进行查询。
				string permissionItem = "68ae8116458b88";
				PermissionAuthResult permissionAuthResult = this.View.Model.FuncPermissionAuth(
					new string[] { "" }, permissionItem, null, false).FirstOrDefault();
				if (permissionAuthResult != null && !permissionAuthResult.Passed)
				{
					this.View.ShowErrMessage("没有权限");
					e.Cancel = true;
					return;
				}
				var tb = BuildData();
				To3dZip(tb);
			}
			if (e.BarItemKey.Equals("tbMerge", StringComparison.OrdinalIgnoreCase))
			{
				var rowinfo = this.ListView.SelectedRowsInfo;
				string FMachineName = "";
				long FSupplierId = 0;

				foreach (var item in rowinfo)
				{
					if (!orglongs.Contains(Convert.ToInt64(item.DataRow["FSupplyOrgId"])))
					{
						continue;
					}
					if (string.IsNullOrEmpty(FMachineName))
					{
						FMachineName = Convert.ToString(item.DataRow["FMachineName"]);
						continue;
					}
					if (Convert.ToString(item.DataRow["FMachineName"]) != FMachineName)
					{
						this.View.ShowMessage("合并后的机床/供应商信息可能与之前不同，请注意检查！");
						//throw new Exception("合并后的机床/供应商信息可能与之前不同，请注意检查！");
					}
				}
				foreach (var item in rowinfo)
				{
					if (!orglongs.Contains(Convert.ToInt64(item.DataRow["FSupplyOrgId"])))
					{
						continue;
					}
					if (FSupplierId == 0)
					{
						FSupplierId = Convert.ToInt64(item.DataRow["FPENYCustomerID_Id"]);
						continue;
					}
					if (Convert.ToInt64(item.DataRow["FPENYCustomerID_Id"]) != FSupplierId)
					{
						this.View.ShowMessage("合并后的机床/供应商信息可能与之前不同，请注意检查！");
					}
				}
			}
		}

		private DynamicObjectCollection BuildData()
		{
			var rowinfo = this.ListView.SelectedRowsInfo.Select(x => x.PrimaryKeyValue).ToArray();
			if (rowinfo.Length <= 0)
			{
				throw new Exception("未选中任何行!");
			}
			string sSql = $@"SELECT t1.FCALCBILLNO,t1.FTENDERBILLNO,t1.FBILLNO FPlanBillNo_A,t7.FNAME OrgName,t8.FNAME CustName, t1.FPLNTargetType,
t1.FPlanTenderType,t1.FMachineName,t5.FNUMBER FMATERIALID_A,t6.FNAME MaName,t3.FQTY,t1.FSalAmount,t1.FRecordType,
t9.FNAME SupplierName,t10.FNAME DeptName,CASE t1.FISAPLACCEPT WHEN 0 THEN '否' ELSE '是' END FISAPLACCEPT
FROM dbo.T_PLN_PLANORDER t1
INNER JOIN dbo.T_PLN_PLANORDER_B t2 ON t1.FID=t2.FID
LEFT JOIN dbo.T_SAL_ORDERENTRY t3 ON t2.FSALEORDERENTRYID=t3.FENTRYID
LEFT JOIN dbo.T_SAL_ORDERENTRY_F t4 ON t3.FENTRYID=t4.FENTRYID
LEFT JOIN dbo.T_BD_MATERIAL t5 ON t1.FMATERIALID=t5.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIAL_L t6 ON t1.FMATERIALID=t6.FMATERIALID
LEFT JOIN dbo.T_ORG_ORGANIZATIONS_L t7 ON t1.FDEMANDORGID=t7.FORGID
LEFT JOIN dbo.T_BD_CUSTOMER_L t8 ON t1.FPENYCUSTOMERID=t8.FCUSTID
LEFT JOIN dbo.T_BD_SUPPLIER_L t9 ON t1.FSUPPLIERID=t9.FSUPPLIERID
LEFT JOIN dbo.T_BD_DEPARTMENT_L t10 ON t1.FPRDDEPTID=t10.FDEPTID
WHERE t1.FID IN ({string.Join(",", rowinfo)}) AND t1.FCalcBillNo<>''
ORDER BY t1.FTENDERBILLNO";
			return DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
		}

		private void To3dZip(DynamicObjectCollection EntityObjs)
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

					//var EntityObjs = this.Model.GetEntityDataObject(this.View.BillBusinessInfo.GetEntity("FAnalyseEntity"));
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
								//DetailId = 1582001,
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
			//var entryGrid = this.View.GetControl<EntryGrid>("FAnalyseEntity");
			//var fields = ((Kingdee.BOS.Core.Metadata.EntityElement.EntityAppearance)entryGrid.ControlAppearance).Entity.Fields.OrderBy(x => x.Tabindex);
			tb.Columns.Add("FCALCBILLNO");
			tb.Columns.Add("FTENDERBILLNO");
			tb.Columns.Add("FPlanBillNo_A");
			tb.Columns.Add("OrgName");
			tb.Columns.Add("CustName");
			tb.Columns.Add("FPLNTargetType");
			tb.Columns.Add("FPlanTenderType");
			tb.Columns.Add("FMachineName");
			tb.Columns.Add("FMATERIALID_A");
			tb.Columns.Add("MaName");
			tb.Columns.Add("FQTY");
			tb.Columns.Add("FSalAmount");
			tb.Columns.Add("FRecordType");
			tb.Columns.Add("SupplierName");
			tb.Columns.Add("DeptName");
			tb.Columns.Add("FisAplAccept");
			//构造表头
			var newRow = tb.NewRow();
			newRow["FCALCBILLNO"] = "运算单号";
			newRow["FTENDERBILLNO"] = "分标单号";
			newRow["FPlanBillNo_A"] = "计划单号";
			newRow["OrgName"] = "供货组织";
			newRow["CustName"] = "客户";
			newRow["FPLNTargetType"] = "标的类型";
			newRow["FPlanTenderType"] = "招标机型";
			newRow["FMachineName"] = "机床类型";
			newRow["FMATERIALID_A"] = "物料编码";
			newRow["MaName"] = "物料名称";
			newRow["FQTY"] = "数量";
			newRow["FSalAmount"] = "销售金额";
			newRow["FRecordType"] = "记录类别";
			newRow["SupplierName"] = "供应商";
			newRow["DeptName"] = "车间";
			newRow["FisAplAccept"] = "适合产线";
			tb.Rows.Add(newRow);

			foreach (var item in EntityObjs)
			{
				var row = tb.NewRow();
				foreach (var field in item.DynamicObjectType.Properties)
				{
					row[field.Name] = item[field.Name];
				}
				tb.Rows.Add(row);
			}

			return tb;
		}
	}
}
