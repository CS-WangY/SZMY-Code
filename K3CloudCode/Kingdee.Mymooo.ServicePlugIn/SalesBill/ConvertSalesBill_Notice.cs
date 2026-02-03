using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using static Kingdee.K3.MFG.App.AppServiceContext;
using static Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice.StockValidator;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
	[Description("发货通知单单据转换修改可发货数量"), HotUpdate]
	public class ConvertSalesBill_Notice : AbstractConvertPlugIn
	{
		/// <summary>
		/// 224428,7401803,1043841,7348029 深圳 华东五部，江苏蚂蚁，北京蚂蚁
		/// </summary>
		long[] salorgids =
		 {
				224428,
				7401803,
				1043841,
				7348029
		 };

		public override void OnAfterCreateLink(CreateLinkEventArgs e)
		{
			base.OnAfterCreateLink(e);

			BusinessInfo businessInfo = e.TargetBusinessInfo;
			BaseDataField bdField = businessInfo.GetField("FStockID") as BaseDataField;
			QueryBuilderParemeter p = new QueryBuilderParemeter();
			p.FormId = "BD_STOCK";
			p.SelectItems = SelectorItemInfo.CreateItems("FStockId");

			// 目标单单据体元数据
			Entity entity = e.TargetBusinessInfo.GetEntity("FEntity");
			// 读取已经生成的发货通知单
			ExtendedDataEntity[] bills = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");
			// 对目标单据进行循环
			foreach (var bill in bills)
			{
				var salorgid = Convert.ToInt64(bill.DataEntity["SaleOrgId_Id"]);
				var devorgid = Convert.ToInt64(bill.DataEntity["DeliveryOrgID_Id"]);
				// 取单据体集合
				DynamicObjectCollection rowObjs = entity.DynamicProperty.GetValue(bill.DataEntity) as DynamicObjectCollection;
				foreach (var item in rowObjs)
				{
					var srcbillid = "";
					var srcrowid = "";
					foreach (var itemlink in item["FEntity_Link"] as DynamicObjectCollection)
					{
						srcbillid = itemlink["SBillId"] as string;
						srcrowid = itemlink["SId"] as string;
					}
					string sSql = $@"select FOUTSOURCESTOCKLOC from T_SAL_ORDERENTRY where FID={srcbillid} and FENTRYID={srcrowid}";
					var outsource = DBUtils.ExecuteScalar<string>(this.Context, sSql, "0");
					if (string.IsNullOrWhiteSpace(outsource))
					{
						throw new Exception("销售订单发货地址错误！");
					}
					//直发获取外发仓
					if (((DynamicObject)bill["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY")
					{
						p.FilterClauseWihtKey = $"FISDIRSTOCK='1' and FUSEORGID={devorgid} and FDOCUMENTSTATUS='C'";
					}
					else
					{
						if (salorgids.Contains(salorgid))
						{
							p.FilterClauseWihtKey = $"FOUTSOURCESTOCKLOC='{outsource}' and FUSEORGID={devorgid} and FDOCUMENTSTATUS='C' AND FISOUTSOURCESTOCK=1";
						}
						else
						{
							p.FilterClauseWihtKey = $"FName like '%成品仓%' and FUSEORGID={devorgid} and FDOCUMENTSTATUS='C'";
						}

					}

					p.OrderByClauseWihtKey = " FNumber";
					var obj_ck = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p);
					if (obj_ck.Count() > 0)
					{
						item["StockID_Id"] = obj_ck[0]["Id"];
						item["StockID"] = obj_ck[0];
					}
					else
					{
						throw new Exception(this.Context.CurrentOrganizationInfo.Name + ":销售订单[" + outsource + "]发货地址错误！");
					}
				}

			}

		}
		public override void AfterConvert(AfterConvertEventArgs e)
		{
			base.AfterConvert(e);
			ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
			foreach (var headEntity in headEntitys)
			{
				//收款条件信息集合
				var collectionTerms = headEntity["FRECEIPTCONDITIONID"] as DynamicObject;

				var cunumber = Convert.ToString(((DynamicObject)headEntity.DataEntity["CustomerID"])?["Number"]);
				RequestExpiry request = new RequestExpiry() { CustomerCode = cunumber };
				var expiry = SalesOrderServiceHelper.QueryCustomerExpiryList(this.Context, request);
				headEntity.DataEntity["FPENYExpiryDay"] = expiry.Data.ExpiryDay;
				headEntity.DataEntity["FPENYExpiryAmount"] = expiry.Data.ExpiryAmount;
				//如果不是则跳出深圳224428广东蚂蚁669144
				long[] SalOrgList = new long[] { 224428, 669144, 1043841, 7348029 };
				if (!SalOrgList.Contains(Convert.ToInt64(headEntity.DataEntity["SaleOrgId_Id"])))
				{
					continue;
				}
				foreach (var item in ((DynamicObjectCollection)headEntity.DataEntity["SAL_DELIVERYNOTICEENTRY"]).OrderBy(x => x["MaterialID_Id"]).ToList())
				{
					var mid = item["MaterialID"] as DynamicObject;
					decimal qty = Convert.ToDecimal(item["Qty"]);
					var srcunitID = Convert.ToInt64(item["UnitID_Id"]);
					var srcbillid = "";
					var srcrowid = "";
					foreach (var itemlink in item["FEntity_Link"] as DynamicObjectCollection)
					{
						srcbillid = itemlink["SBillId"] as string;
						srcrowid = itemlink["SId"] as string;
					}
					//取销售订单库存组织+供货组织代码
					long stockorgid = 0;
					long supplytarorgid = 0;
					string sSql = $@"select FSTOCKORGID,FSUPPLYTARGETORGID from T_SAL_ORDERENTRY where FID='{srcbillid}' and FENTRYID='{srcrowid}'";
					using (var orgdata = DBUtils.ExecuteReader(this.Context, sSql))
					{
						while (orgdata.Read())
						{
							stockorgid = Convert.ToInt64(orgdata["FSTOCKORGID"]);
							supplytarorgid = Convert.ToInt64(orgdata["FSUPPLYTARGETORGID"]);
						}
					}
					//取当前单据预留数量
					sSql = $@"SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FBASEUNITID
                        FROM T_PLN_RESERVELINKENTRY t1
                        INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        LEFT JOIN T_BD_STOCK t3 ON t1.FSTOCKID=t3.FSTOCKID
                        WHERE t2.FSRCINTERID='{srcbillid}' AND t2.FSRCENTRYID='{srcrowid}'
                        AND t1.FSUPPLYFORMID='STK_Inventory' AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                        AND t3.FNOTALLOWDELIVERY=0 AND t1.FSUPPLYORGID={supplytarorgid}
                        GROUP BY t1.FBASEUNITID";
					var resdata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
					decimal resqty = 0;
					long resunitID = 0;
					if (resdata.Count > 0)
					{
						resqty = Convert.ToDecimal(resdata[0]["FBASEQTY"]);
						resunitID = Convert.ToInt64(resdata[0]["FBASEUNITID"]);
					}
					IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
					if (srcunitID != resunitID)
					{
						resqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(mid["Id"]), resunitID, srcunitID, resqty);
					}

					//取即时库存可用数量
					var materqtys = StockQuantityServiceHelper.InventoryQty(this.Context, Convert.ToInt64(mid["msterID"]), new List<long> { supplytarorgid });
					decimal materqty = 0;
					if (materqtys.Count > 0)
					{
						materqty = materqtys.GroupBy(p => p["FMATERIALID"]).Select(t => new
						{
							AVBQTY = t.Sum(s => (decimal)s["FBASEQTY"])
						}).ToList()[0].AVBQTY;
					}

					decimal baseqty = 0;
					decimal price = Convert.ToDecimal(item["Price"]);
					decimal atxprice = Convert.ToDecimal(item["TaxPrice"]);

					if (resqty == 0)
					{
						if (materqty - qty >= 0)
						{
							baseqty = qty;
							var baseunitqty = qty;
							if (resunitID != 0)
							{
								baseunitqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(mid["Id"]), srcunitID, resunitID, qty);
							}
							item["Qty"] = baseqty;
							item["BaseUnitQty"] = baseunitqty;
							item["PriceUnitQty"] = baseqty;
							item["PriceBaseQty"] = baseunitqty;
							item["StockQty"] = baseqty;
							item["StockBaseQty"] = baseunitqty;
							//item["RemainOutQty"] = baseqty;
							item["Amount"] = price * baseqty;
							item["Amount_LC"] = price * baseqty;
							item["AllAmount"] = atxprice * baseqty;
							item["AllAmount_LC"] = atxprice * baseqty;
							item["TAXAMOUNT"] = (atxprice * baseqty) - (price * baseqty);
							item["TAXAMOUNT_LC"] = (atxprice * baseqty) - (price * baseqty);
						}
						else
						{
							baseqty = materqty;
							var baseunitqty = materqty;
							if (resunitID != 0)
							{
								baseunitqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(mid["Id"]), srcunitID, resunitID, materqty);
							}
							item["Qty"] = baseqty;
							item["BaseUnitQty"] = baseunitqty;
							item["PriceUnitQty"] = baseqty;
							item["PriceBaseQty"] = baseunitqty;
							item["StockQty"] = baseqty;
							item["StockBaseQty"] = baseunitqty;
							//item["RemainOutQty"] = baseqty;
							item["Amount"] = price * baseqty;
							item["Amount_LC"] = price * baseqty;
							item["AllAmount"] = atxprice * baseqty;
							item["AllAmount_LC"] = atxprice * baseqty;
							item["TAXAMOUNT"] = (atxprice * baseqty) - (price * baseqty);
							item["TAXAMOUNT_LC"] = (atxprice * baseqty) - (price * baseqty);
						}
					}
					else
					{
						if (resqty - qty >= 0)
						{
							baseqty = qty;
							var baseunitqty = qty;
							if (resunitID != 0)
							{
								baseunitqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(mid["Id"]), srcunitID, resunitID, qty);
							}
							item["Qty"] = baseqty;
							item["BaseUnitQty"] = baseunitqty;
							item["PriceUnitQty"] = baseqty;
							item["PriceBaseQty"] = baseunitqty;
							item["StockQty"] = baseqty;
							item["StockBaseQty"] = baseunitqty;
							//item["RemainOutQty"] = baseqty;
							item["Amount"] = price * baseqty;
							item["Amount_LC"] = price * baseqty;
							item["AllAmount"] = atxprice * baseqty;
							item["AllAmount_LC"] = atxprice * baseqty;
							item["TAXAMOUNT"] = (atxprice * baseqty) - (price * baseqty);
							item["TAXAMOUNT_LC"] = (atxprice * baseqty) - (price * baseqty);
						}
						else
						{
							baseqty = resqty;
							var baseunitqty = resqty;
							if (resunitID != 0)
							{
								baseunitqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(mid["Id"]), srcunitID, resunitID, resqty);
							}
							item["Qty"] = baseqty;
							item["BaseUnitQty"] = baseunitqty;
							item["PriceUnitQty"] = baseqty;
							item["PriceBaseQty"] = baseunitqty;
							item["StockQty"] = baseqty;
							item["StockBaseQty"] = baseunitqty;
							//item["RemainOutQty"] = baseqty;
							item["Amount"] = price * baseqty;
							item["Amount_LC"] = price * baseqty;
							item["AllAmount"] = atxprice * baseqty;
							item["AllAmount_LC"] = atxprice * baseqty;
							item["TAXAMOUNT"] = (atxprice * baseqty) - (price * baseqty);
							item["TAXAMOUNT_LC"] = (atxprice * baseqty) - (price * baseqty);
						}
					}
				}
			}
		}

	}
}
