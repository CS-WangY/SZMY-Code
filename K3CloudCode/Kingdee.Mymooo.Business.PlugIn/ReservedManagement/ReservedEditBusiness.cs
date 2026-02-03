using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter.ConditionVariableAnalysis;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Mq;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.QM.ParamOption;
using Kingdee.K3.MFG.ServiceHelper.PLN;
using Kingdee.K3.SCM.Core;
using Kingdee.K3.SCM.Core.KDBigData;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using static NPOI.HSSF.Util.HSSFColor;
using TreeNode = Kingdee.BOS.Core.Metadata.TreeNode;
using TreeView = Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel.TreeView;

namespace Kingdee.Mymooo.Business.PlugIn.ReservedManagement
{
	[Description("预留综合修改动态表单插件"), HotUpdate]
	public class ReservedEditBusiness : AbstractDynamicFormPlugIn
	{
		public string _billID;
		public TreeNode root;
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			_billID = this.View.OpenParameter.GetCustomParameter("BillID").ToString();
			LoadSalBillTree();
		}
		public void LoadSalBillTree()
		{
			var sSql = $@"/*dialect*/SELECT t2.FBILLNO,t1.FSEQ,t1.FENTRYID
                        FROM dbo.T_SAL_ORDERENTRY t1
                            LEFT JOIN dbo.T_SAL_ORDER t2
                                ON t1.FID = t2.FID
                        WHERE t1.FENTRYID IN ({_billID})
                        ORDER BY CHARINDEX(',' + RTRIM(CAST(t1.FENTRYID AS VARCHAR(10))) + ',', ',{_billID},')";
			var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
			var treeView = this.View.GetControl<TreeView>("FTreeView");    //根据Key获取树控件
			treeView.SetRootVisible(false); //可以设置是否显示根节点
			treeView.SetExpanded(true); //可以设置树是否展开

			root = new TreeNode() { id = "0", text = "TreeView" }; //创建一个根节点
			treeView.SetRootNode(root);//设置为根节点
			foreach (var item in datas)
			{
				TreeNode tn = new TreeNode()
				{
					id = Convert.ToString(item["FENTRYID"]),
					text = Convert.ToString(item["FBILLNO"]) + "/" + Convert.ToString(item["FSEQ"]),
					//icon = "image base64"
				};
				root.children.Add(tn);
			}

			treeView.RefreshNode("0", root);
		}

		public static DynamicObjectCollection _materqtys;
		public static decimal _SupplyQty = 0;
		public static long _resid = 0;
		public static long _materialid = 0;
		public static long _masterid = 0;
		public static decimal _materqty = 0;
		public static long _targetOrgID = 0;

		public static string _rootNode = "";
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			_rootNode = e.NodeId;
			//if (this.Model.GetEntryRowCount("FREQUIREMENTORDEREntity") > 0)
			//{
			//    TipsSaveBill();
			//}
			//加载单据信息
			var sSql = $@"/*dialect*/SELECT t2.FBILLNO,
                               t1.FSEQ,
                               t2.FCUSTID,
                               t1.FMATERIALID,
                               t1.FMAPID,
                               t1.FUNITID,
                               td.FDELIVERYDATE,
                               t1.FParentSmallId,
                               t1.FSmallId,
                               t1.FSUPPLYTARGETORGID,
                               tf.FSETTLEORGID,
                               tm.FMASTERID,
                               t1.FQTY,
							   tr.FBASECANOUTQTY
                        FROM dbo.T_SAL_ORDERENTRY t1
                            INNER JOIN T_SAL_ORDERENTRY_R tr
						        ON t1.FENTRYID=tr.FENTRYID
                            INNER JOIN T_SAL_ORDERENTRY_F tf
                                ON t1.FENTRYID = tf.FENTRYID
                            INNER JOIN dbo.T_SAL_ORDERENTRY_D td
                                ON t1.FENTRYID = td.FENTRYID
                            INNER JOIN dbo.T_BD_MATERIAL tm
                                ON t1.FMATERIALID = tm.FMATERIALID
                            INNER JOIN dbo.T_SAL_ORDER t2
                                ON t1.FID = t2.FID
                        WHERE t1.FENTRYID = {e.NodeId}";
			var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
			foreach (var item in datas)
			{
				this.View.Model.SetValue("FSALBILLNO", Convert.ToString(item["FBILLNO"]));
				this.View.Model.SetValue("FSALBILLSEQ", Convert.ToString(item["FSEQ"]));
				this.View.Model.SetItemValueByID("FCUTSID", Convert.ToInt64(item["FCUSTID"]), 0);
				_materialid = Convert.ToInt64(item["FMATERIALID"]);
				_masterid = Convert.ToInt64(item["FMASTERID"]);
				this.View.Model.SetItemValueByID("FHMaterialID", _materialid, 0);
				this.View.Model.SetItemValueByID("FCustMatID", Convert.ToString(item["FMAPID"]), 0);
				this.View.Model.SetItemValueByID("FUnitID", Convert.ToInt64(item["FUNITID"]), 0);
				this.View.Model.SetValue("FDeliveryDate", Convert.ToString(item["FDELIVERYDATE"]));
				this.View.Model.SetItemValueByID("FPARENTSMALLID", Convert.ToInt64(item["FParentSmallId"]), 0);
				this.View.Model.SetItemValueByID("FSMALLID", Convert.ToInt64(item["FSmallId"]), 0);
				_targetOrgID = Convert.ToInt64(item["FSUPPLYTARGETORGID"]);
				this.View.Model.SetItemValueByID("FSupplyTargetOrgId", _targetOrgID, 0);
				this.View.Model.SetItemValueByID("FSETTLEORGID", Convert.ToInt64(item["FSETTLEORGID"]), 0);
				this.View.Model.SetValue("FSalQty", Convert.ToString(item["FQTY"]));
				this.View.Model.SetValue("FSalOutQty", Convert.ToString(item["FBASECANOUTQTY"]));
			}
			//加载可用库存
			LoadStockQty();

			this.Model.DeleteEntryData("FREQUIREMENTORDEREntity");
			this.Model.DeleteEntryData("FReservedEntity");
			//加载组织间需求单信息
			sSql = $@"SELECT t1.FID,
                           t1.FDEMANDORGID,
                           t1.FMATERIALID FDEMANDMATERIALID,
                           t1.FBASEDEMANDUNITID,
                           t2.FBASEDEMANDQTY,
                           t3.FBASEQTY,
                           t1.FDEMANDDATE,
                           t1.FDEMANDFORMID,
                           t1.FDEMANDBILLNO,
                           CASE t1.FCOMPUTEID
                               WHEN NULL THEN
                                   '手工录入'
                               ELSE
                                   '运算生成'
                           END AS FCREATETYPE
                    FROM T_PLN_RESERVELINK t1
                        INNER JOIN
                        (
                            SELECT t1.FSUPPLYINTERID,t2.FBASEDEMANDQTY
                            FROM T_PLN_RESERVELINKENTRY t1
                                INNER JOIN T_PLN_RESERVELINK t2
                                    ON t1.FID = t2.FID
                            WHERE t2.FSRCENTRYID = '{e.NodeId}'
                                  AND t1.FSUPPLYFORMID = 'PLN_REQUIREMENTORDER'
                        ) t2
                            ON t1.FDEMANDINTERID = t2.FSUPPLYINTERID
                        LEFT JOIN
                        (
                            SELECT FID,
                                   SUM(FREMAINQTY) FBASEQTY
                            FROM T_PLN_REQUIREMENTORDER
                            GROUP BY FID
                        ) t3
                            ON t1.FDEMANDINTERID = t3.FID;";
			datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
			foreach (var item in datas)
			{
				int rowcount = this.Model.GetEntryRowCount("FREQUIREMENTORDEREntity");
				this.Model.CreateNewEntryRow("FREQUIREMENTORDEREntity");
				this.Model.SetValue("FID", Convert.ToString(item["FID"]), rowcount);
				this.Model.SetItemValueByID("FDEMANDORGID", Convert.ToInt64(item["FDEMANDORGID"]), rowcount);
				this.Model.SetItemValueByID("FDEMANDMATERIALID", Convert.ToInt64(item["FDEMANDMATERIALID"]), rowcount);
				this.Model.SetItemValueByID("FBASEDEMANDUNITID", Convert.ToInt64(item["FBASEDEMANDUNITID"]), rowcount);
				this.Model.SetValue("FBASEDEMANDQTY", Convert.ToDecimal(item["FBASEQTY"]), rowcount);
				this.Model.SetValue("FBASEQTY", Convert.ToDecimal(item["FBASEQTY"]), rowcount);
				this.Model.SetValue("FDEMANDDATE", Convert.ToDateTime(item["FDEMANDDATE"]), rowcount);
				this.Model.SetItemValueByNumber("FDEMANDFORMID", Convert.ToString(item["FDEMANDFORMID"]), rowcount);
				this.Model.SetValue("FDEMANDBILLNO", Convert.ToString(item["FDEMANDBILLNO"]), rowcount);
				this.Model.SetValue("FCREATETYPE", Convert.ToString(item["FCREATETYPE"]), rowcount);

			}
			this.View.UpdateView("FREQUIREMENTORDEREntity");
		}
		private void Tips_PLN_REQUIREMENTORDER(long resid)
		{

		}
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);

			if (e.Key.EqualsIgnoreCase("FREQUIREMENTORDEREntity"))
			{
				this.Model.DeleteEntryData("FReservedEntity");
				_resid = Convert.ToInt64(this.Model.GetValue("FID", e.Row));
				_SupplyQty = Convert.ToInt64(this.Model.GetValue("FBASEDEMANDQTY", e.Row));
				try
				{
					var billView = FormMetadataUtils.CreateBillView(this.Context, "PLN_RESERVELINK", _resid);
					var entryView = billView.Model.DataObject["Entity"] as DynamicObjectCollection;
					var sumBillView = entryView.GroupBy(p => p["SUPPLYINTERID"]).Select(t => new
					{
						Id = Convert.ToString(t.First()["SupplyFormID_Id"]).EqualsIgnoreCase("STK_Inventory") ? 0 : t.First()["Id"],
						SupplyFormID_Id = t.First()["SupplyFormID_Id"],
						SupplyInterID = t.First()["SupplyInterID"],
						SupplyOrgId_Id = t.First()["SupplyOrgId_Id"],
						SupplyBillNO = t.First()["SupplyBillNO"],
						SupplyMaterialID_Id = t.First()["SupplyMaterialID_Id"],
						BaseSupplyUnitID_Id = t.First()["BaseSupplyUnitID_Id"],
						BaseSupplyQty = t.Sum(s => (decimal)s["BaseSupplyQty"]),
						SupplyDate = t.First()["SupplyDate"],
						SupplyStockID_Id = t.First()["SupplyStockID_Id"],
					}).ToList();


					foreach (var item in sumBillView)
					{
						//if (Convert.ToString(item.Id) == "0")
						//{
						//    continue;
						//}
						int rowcount = this.Model.GetEntryRowCount("FReservedEntity");
						this.Model.CreateNewEntryRow("FReservedEntity");
						this.Model.SetValue("FEntryID", Convert.ToString(item.Id), rowcount);
						this.Model.SetItemValueByID("FSupplyFormID", Convert.ToString(item.SupplyFormID_Id), rowcount);
						this.Model.SetValue("FSupplyInterID", Convert.ToString(item.SupplyInterID), rowcount);
						this.Model.SetItemValueByID("FSupplyOrgId", Convert.ToInt64(item.SupplyOrgId_Id), rowcount);
						this.Model.SetValue("FSupplyBillNO", Convert.ToString(item.SupplyBillNO), rowcount);
						this.Model.SetItemValueByID("FSupplyMaterialID", Convert.ToInt64(item.SupplyMaterialID_Id), rowcount);
						this.Model.SetItemValueByID("FBaseSupplyUnitID", Convert.ToInt64(item.BaseSupplyUnitID_Id), rowcount);
						var mqty = _materqtys.Where(x => Convert.ToString(x["FID"]) == Convert.ToString(item.SupplyInterID)).ToList();
						if (mqty.Count() > 0)
						{
							var qty = Convert.ToDecimal(item.BaseSupplyQty) + Convert.ToDecimal(mqty[0]["FBASEQTY"]);
							this.Model.SetValue("FBaseSupplyQty", qty, rowcount);
							this.Model.SetValue("FQty", Convert.ToDecimal(item.BaseSupplyQty), rowcount);
							this.Model.SetValue("FStockQty", Convert.ToDecimal(mqty[0]["FBASEQTY"]), rowcount);
						}
						else
						{
							this.Model.SetValue("FBaseSupplyQty", Convert.ToDecimal(item.BaseSupplyQty), rowcount);
							this.Model.SetValue("FQty", Convert.ToDecimal(item.BaseSupplyQty), rowcount);
							//this.Model.SetValue("FStockQty", Convert.ToDecimal(mqty[0]["FBASEQTY"]), rowcount);
						}
						this.Model.SetValue("FSupplyDate", Convert.ToString(item.SupplyDate), rowcount);
						this.Model.SetItemValueByID("FSupplyStockID", Convert.ToInt64(item.SupplyStockID_Id), rowcount);
					}

					var entitycount = this.Model.GetEntryRowCount("FReservedEntity");
					foreach (var item in _materqtys)
					{
						var lockid = Convert.ToString(item["FID"]);
						var lockqty = Convert.ToDecimal(item["FBASEQTY"]);
						if (lockqty <= 0) continue;

						var iis = ((DynamicObjectCollection)this.Model.DataObject["FReservedEntity"])
							.Where(x => Convert.ToString(x["FSupplyInterID"]) == lockid).ToList();
						if (iis.Count > 0)
						{
							continue;
						}
						this.Model.CreateNewEntryRow("FReservedEntity");
						//this.Model.SetValue("FEntryID", Convert.ToString(item["Id"]), rowcount);
						this.Model.SetItemValueByNumber("FSupplyFormID", "STK_Inventory", entitycount);
						this.Model.SetValue("FSupplyInterID", lockid, entitycount);
						this.Model.SetItemValueByID("FSupplyOrgId", Convert.ToInt64(item["FSTOCKORGID"]), entitycount);
						//this.Model.SetValue("FSupplyBillNO", Convert.ToString(item["SupplyBillNO"]), rowcount);
						this.Model.SetItemValueByID("FSupplyMaterialID", Convert.ToInt64(item["FMATERIALID"]), entitycount);
						this.Model.SetItemValueByID("FBaseSupplyUnitID", Convert.ToInt64(item["FBASEUNITID"]), entitycount);
						this.Model.SetValue("FBaseSupplyQty", Convert.ToString(item["FBASEQTY"]), entitycount);
						//this.Model.SetValue("FQty", Convert.ToString(item["BaseSupplyQty"]), rowcount);
						this.Model.SetValue("FStockQty", Convert.ToString(item["FBASEQTY"]), entitycount);
						this.Model.SetValue("FSupplyDate", Convert.ToString(System.DateTime.Now), entitycount);
						this.Model.SetItemValueByID("FSupplyStockID", Convert.ToInt64(item["FSTOCKID"]), entitycount);

					}
					var resQty = ((DynamicObjectCollection)this.Model.DataObject["FReservedEntity"]).Sum(x => decimal.Parse(x["FQty"].ToString()));
					this.Model.SetValue("FBaseQTY", resQty, e.Row);
				}
				catch { }

				this.View.UpdateView("FReservedEntity");
			}
		}
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			if (e.BarItemKey.EqualsIgnoreCase("PENY_Save"))
			{
				this.View.ShowMessage("是否确定修改当前预留数据？", MessageBoxOptions.OKCancel, new Action<MessageBoxResult>(result =>
				{
					if (result == MessageBoxResult.OK)
					{
						var billView = FormMetadataUtils.CreateBillView(this.Context, "PLN_RESERVELINK", _resid);

						//删除原库存预留以修改界面最后修改数量为准
						var rowcount = billView.Model.GetEntryRowCount("FEntity");
						for (int i = rowcount; i >= 0; i--)
						{
							var eformid = billView.Model.GetValue<string>("FSupplyFormID", i, string.Empty);
							if (eformid.EqualsIgnoreCase("STK_Inventory"))
							{
								billView.Model.DeleteEntryRow("FEntity", i);
							}
						}
						List<DynamicObject> list = new List<DynamicObject>();
						foreach (var item in this.Model.DataObject["FReservedEntity"] as DynamicObjectCollection)
						{
							var entryid = Convert.ToInt64(item["FEntryID"]);
							var qty = Convert.ToDecimal(item["FQty"]);
							var rowid = Convert.ToString(item["FSupplyInterID"]);

							if (entryid == 0)
							{
								if (qty <= 0) continue;
								billView.Model.CreateNewEntryRow("FEntity");
								var seq = billView.Model.GetEntryRowCount("FEntity") - 1;
								billView.Model.SetValue("FSUPPLYFORMID", "STK_Inventory", seq);
								billView.Model.SetItemValueByID("FSUPPLYINTERID", Convert.ToString(item["FSUPPLYINTERID"]), seq);
								billView.Model.SetItemValueByID("FSUPPLYMATERIALID", Convert.ToInt64(item["FSUPPLYMATERIALID_Id"]), seq);
								billView.Model.SetItemValueByID("FSUPPLYORGID", Convert.ToInt64(item["FSUPPLYORGID_Id"]), seq);
								billView.Model.SetValue("FSUPPLYDATE", System.DateTime.Now, seq);
								billView.Model.SetItemValueByID("FSUPPLYSTOCKID", Convert.ToInt64(item["FSUPPLYSTOCKID_Id"]), seq);
								billView.Model.SetItemValueByID("FBASESUPPLYUNITID", Convert.ToInt64(item["FBASESUPPLYUNITID_Id"]), seq);
								//供应数量
								billView.Model.SetValue("FBASESUPPLYQTY", qty, seq);
								//billView.Model.SetItemValueByID("FINTSUPPLYID", Convert.ToInt64(item["FINTSUPPLYID"]), seq);
							}
							else
							{
								for (int i = 0; i < billView.Model.GetEntryRowCount("FEntity"); i++)
								{
									var eid = Convert.ToInt64(billView.Model.GetEntryPKValue("FEntity", i));
									if (eid == entryid)
									{
										if (qty <= 0)
										{
											billView.Model.DeleteEntryRow("FEntity", i);
										}
										else
										{
											billView.Model.SetValue("FBASESUPPLYQTY", qty, i);
										}
									}
								}
							}
						}
						if (billView.Model.GetEntryRowCount("FEntity") <= 0)
						{
							OperateOption option = OperateOption.Create();
							BusinessDataServiceHelper.Delete(this.Context, billView.BusinessInfo, new object[] { _resid }, option);
						}
						else
						{
							list.Add(billView.Model.DataObject);
							SaveBill(this.Context, billView.BusinessInfo, list.ToArray());
						}

						var BillEntityRowIndex = this.View.Model.GetEntryCurrentRowIndex("FREQUIREMENTORDEREntity");
						EntryGrid entryGrid = (EntryGrid)this.View.GetControl("FREQUIREMENTORDEREntity");
						entryGrid.SetFocusRowIndex(BillEntityRowIndex);
						//加载可用库存
						LoadStockQty();

						//LoadSalBillTree();
					}

				}));

			}
		}
		public void LoadStockQty()
		{
			_materqtys = StockQuantityServiceHelper.InventoryQty(this.Context, _masterid, new List<long> { _targetOrgID });

			if (_materqtys.Count > 0)
			{
				_materqty = _materqtys.GroupBy(p => p["FMATERIALID"]).Select(t => new
				{
					AVBQTY = t.Sum(s => (decimal)s["FBASEQTY"])
				}).ToList()[0].AVBQTY;
			}
			this.View.GetControl("FLabel").Text = "可用库存:" + Math.Round(_materqty, 4);
		}
		private void SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
		{
			SaveService saveService = new SaveService();
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
			{
				var oper = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);
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
				SaveLog();
				cope.Complete();
			}
		}

		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			if (StringUtils.EqualsIgnoreCase(e.Key, "FQty"))
			{
				e.Cancel = IsCancelUpateSupplyQty(e.Row, Convert.ToDecimal(e.Value));
			}
		}
		private bool IsCancelUpateSupplyQty(int rowIndex, decimal newValue)
		{
			DynamicObjectCollection resEntity = this.Model.DataObject["FReservedEntity"] as DynamicObjectCollection;
			var supplyValue = Convert.ToDecimal(resEntity[rowIndex]["FBASESUPPLYQTY"]);
			var oldValue = Convert.ToDecimal(resEntity[rowIndex]["FQty"]);
			decimal num2 = newValue - oldValue;
			var baseqty = resEntity.Sum(x => decimal.Parse(x["FQty"].ToString())) + num2;
			if (newValue <= _materqty + supplyValue && baseqty <= _SupplyQty)
			{
				var msg = "单据:" + this.Model.GetValue("FSALBILLNO", 0) + ",行号:" + this.Model.GetValue("FSALBILLSEQ", 0);
				msg += "," + (resEntity[rowIndex]["FSUPPLYMATERIALID"] as DynamicObject)?["Number"] + ",从" + oldValue + "至" + newValue;
				WriteOperaterLog(Convert.ToString(resEntity[rowIndex]["FSUPPLYINTERID"]), "修改预留信息", msg);
				return false;
			}

			return true;
		}
		List<LogObject> logs = new List<LogObject>();
		private void WriteOperaterLog(string pkvalue, string operateName, string description)
		{
			var logitem = logs.Where(x => x.pkValue == pkvalue).FirstOrDefault();
			if (logitem != null)
			{
				logs.Remove(logitem);
			}
			LogObject log = new LogObject();
			log.pkValue = pkvalue;
			log.Environment = OperatingEnvironment.BizOperate;
			log.OperateName = operateName;
			log.ObjectTypeId = this.View.Model.BillBusinessInfo.GetForm().Id;  //操作的业务对象ID
			log.SubSystemId = this.View.OpenParameter.SubSystemId; //子系统Id
			log.Description = description;
			logs.Add(log);
		}
		private void SaveLog()
		{
			LogServiceHelper.BatchWriteLog(this.Context, logs);
		}
	}
}
