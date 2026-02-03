using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using System.Data;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.Business.PlugIn.PLN_Forecast
{
    [Description("预测单列表插件"), HotUpdate]
    public class PLN_ForecastListPlugIn : AbstractListPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            switch (e.BarItemKey)
            {
                case "PENY_tbBillClose":
                    DynamicFormShowParameter showParam = new DynamicFormShowParameter();
                    showParam.FormId = "PENY_BillClosedSubmitApproval";
                    //取销售员微信code
                    var saluserid = Convert.ToInt64(this.ListView.SelectedRowsInfo.Select(x => x.DataRow["FPENYSalerId_Id"]).First());
                    var sSql = $@"SELECT t1.FWECHATCODE FROM V_BD_SALESMAN t1 INNER JOIN T_BD_STAFF t2 ON t1.FSTAFFID=t2.FSTAFFID
                                INNER JOIN dbo.T_HR_EMPINFO t3 ON t2.FPERSONID=t3.FPERSONID
                                WHERE t1.fid={saluserid} AND t3.FFORBIDSTATUS='A'";
                    var wxSalUserCode = DBServiceHelper.ExecuteScalar<string>(this.Context, sSql, "");
                    if (string.IsNullOrWhiteSpace(wxSalUserCode))
                    {
                        wxSalUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
                        if (string.IsNullOrWhiteSpace(wxSalUserCode))
                        {
                            throw new Exception("请先配置您的微信Code");
                        }
                    }

                    var Description = this.ListView.SelectedRowsInfo.Select(x => x.DataRow["FDescription"]).First();
                    var rowinfos = this.ListView.SelectedRowsInfo.Select(x => x.PrimaryKeyValue).Distinct().ToArray();
                    var rowinfo = this.ListView.SelectedRowsInfo.Select(x => x.EntryPrimaryKeyValue).Distinct().ToArray();
                    if (rowinfos.Length > 1)
                    {
                        throw new Exception("不允许多张订单同时申请取消!");
                    }
                    this.View.ShowForm(showParam, new Action<FormResult>((result) =>
                    {
                        if (!result.ReturnData.IsNullOrEmpty())
                        {
                            K3CloudClosedForecastOrderRequest request = new K3CloudClosedForecastOrderRequest();
                            sSql = $@"SELECT t4.FNAME CustName,t2.FENTRYID,(t2.FPENYReferencePrice*t2.FQTY-t2.FWRITEOFFQTY) AS FALLAMOUNT,t2.FQTY-t2.FWRITEOFFQTY AS FBASECANOUTQTY,
CASE
WHEN t2.FSUPPLYTARGETORGID = 7401780 OR t2.FSUPPLYTARGETORGID = 7401781
THEN '第一二事业部'
WHEN t2.FSUPPLYTARGETORGID = 7207688
THEN '第三事业部'
WHEN t2.FSUPPLYTARGETORGID = 7401821
THEN '华南事业部'
ELSE ''
END AS BusinessDivision
,t1.FCREATEDATE,t1.FBILLNO,t1.FID,t5.FNUMBER,t6.FNAME
FROM dbo.T_PLN_FORECAST t1
LEFT JOIN dbo.T_PLN_FORECASTENTRY t2 ON t1.FID=t2.FID AND t2.FCLOSESTATUS='A'
LEFT JOIN dbo.T_BD_CUSTOMER_L t4 ON t1.FPENYCUSTID=t4.FCUSTID
LEFT JOIN dbo.T_BD_MATERIAL t5 ON t2.FMATERIALID=t5.FMATERIALID
LEFT JOIN dbo.T_BD_MATERIAL_L t6 ON t5.FMATERIALID=t6.FMATERIALID
                                WHERE t2.FENTRYID IN ({string.Join(",", rowinfo)})";
                            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                            var varAllBills = datas.GroupBy(o => new
                            {
                                FBILLNO = Convert.ToString(o["FBILLNO"])
                            }).Select(g => new
                            {
                                FBILLNO = g.Key.FBILLNO
                            });
                            foreach (var item in varAllBills)
                            {
                                var applist = datas.Where(x => x["FBILLNO"].Equals(item.FBILLNO)).ToList();
                                request.ForecastOrderID = Convert.ToInt64(applist.Select(x => x["FID"]).First());
                                request.CustName = Convert.ToString(applist.Select(x => x["CustName"]).First());
                                request.ClosedAmount = applist.Sum(x => (decimal)x["FALLAMOUNT"]);
                                request.PlaceDate = Convert.ToDateTime(applist.Select(x => x["FCREATEDATE"]).First());
                                request.BillNo = Convert.ToString(applist.Select(x => x["FBILLNO"]).First());
                                request.Material = string.Join(",", applist.Select(x => x["FNUMBER"] + "/" + x["FNAME"]).ToArray());
                                request.OrderQty = applist.Sum(x => (decimal)x["FBASECANOUTQTY"]);
                                //request.OrgID = Convert.ToString(((DynamicObject)dataEntity["SaleOrgId"])["Name"]);
                                request.OrgID = Convert.ToString(applist.Select(x => x["BusinessDivision"]).First());
                                request.Remarks = result.ReturnData.ToString();
                                request.ForecastOrderEntrys = applist.Select(x => Convert.ToInt64(x["FENTRYID"])).ToList();
                                request.SummaryInfo = new string[] {
                            "订单单号：" + Convert.ToString(applist.Select(x => x["FBILLNO"]).First()),
                            "客户名称：" + Convert.ToString(applist.Select(x => x["CustName"]).First()),
                            "备注：" + result.ReturnData.ToString() };
                                ValidatorApprove(request, wxSalUserCode);
                            }
                            this.ListView.Refresh();
                        }
                    }));
                    break;
            }
        }


        public void ValidatorApprove(K3CloudClosedForecastOrderRequest request, string creatorid)
        {
            try
            {
                //赋值抄送人
                var wxUserCode = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserWxCode(this.Context, this.Context.UserId);
                if (string.IsNullOrWhiteSpace(wxUserCode))
                {
                    throw new Exception("请先配置您的微信Code");
                }
                List<string> notifyerlist = new List<string>();
                notifyerlist.Add(creatorid);
                notifyerlist.Add(wxUserCode);
                request.Notifyer = notifyerlist;

                //执行调用审批流
                //AttachmentMaterials
                request.Creator_userid = wxUserCode;
                request.SendRabbitCode = "mymooo_weixin_Approval_ClosedForecastOrder_" + ApigatewayUtils.ApigatewayConfig.EnvCode;
                request.EnvCode = ApigatewayUtils.ApigatewayConfig.EnvCode;

                var result = ApigatewayUtils.InvokePostWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Approval/K3CloudClosedForecastBillApproval", JsonConvertUtils.SerializeObject(request));
                var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(result);
                if (!returninfo.Code.EqualsIgnoreCase("success"))
                {
                    throw new Exception("发起审批流失败：" + returninfo.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("发起审批流失败：" + ex.ToString());
            }
        }

        public DataSet ylobjects { get; set; }
        public DataSet ztobjects { get; set; }
        public DataSet kcobjects { get; set; }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string sSql = $@"/*dialect*/SELECT * FROM v_ReservedStockAll";
            ylobjects = DBServiceHelper.ExecuteDataSet(this.Context, sSql);
            sSql = $@"/*dialect*/SELECT * FROM v_TranmitStockAll";
            ztobjects = DBServiceHelper.ExecuteDataSet(this.Context, sSql);
            sSql = $@"/*dialect*/if object_id(N'tempdb..#STK_INVENTORY_TEMP',N'U') is not null
                    DROP TABLE #STK_INVENTORY_TEMP;
                    SELECT FMATERIALID,FSTOCKID,FSTOCKORGID,FAVBQTY INTO #STK_INVENTORY_TEMP
                    FROM V_STK_INVENTORY_CUS WHERE FAVBQTY>0
                    CREATE INDEX IX_#STK_INVENTORY_TEMP_FMATERIALID ON #STK_INVENTORY_TEMP (FMATERIALID);
                    SELECT t6.FNAME as ORGNAME,t1.FSTOCKID,ISNULL(t1.FAVBQTY,0) as FBASEQTY
                    ,t4.FNAME,t1.FMATERIALID,t1.FSTOCKORGID FROM #STK_INVENTORY_TEMP t1
                    LEFT JOIN T_BD_STOCK_L t4 on t1.FSTOCKID=t4.FSTOCKID
                    LEFT JOIN T_ORG_ORGANIZATIONS_L t6 ON t1.FSTOCKORGID=t6.FORGID";
            kcobjects = DBServiceHelper.ExecuteDataSet(this.Context, sSql);
        }
        public override void CreateListHeader(CreateListHeaderEventArgs e)
        {
            base.CreateListHeader(e);
            // 创建动态列1
            var header = e.ListHeader.AddChild();// 将动态列放在列表的最后面
            //var header = e.ListHeader.AddChild(0);// 将动态列放在列表的指定位置
            header.Caption = new LocaleValue("库存预留量");
            header.Key = "FDynamicColumn1";
            header.FieldName = "FDynamicColumn1";
            header.ColType = SqlStorageType.SqlText;
            header.Width = 200;
            header.Visible = true;
            header.ColIndex = e.ListHeader.GetChilds().Max(o => o.ColIndex) + 1;// 注意：列的显示顺序不是ColIndex决定的，而是由该列在ListHeader的childs集合中的位置决定的。
            // 创建动态列1
            header = e.ListHeader.AddChild();// 将动态列放在列表的最后面
            //var header = e.ListHeader.AddChild(0);// 将动态列放在列表的指定位置
            header.Caption = new LocaleValue("在途量");
            header.Key = "FDynamicColumn2";
            header.FieldName = "FDynamicColumn2";
            header.ColType = SqlStorageType.SqlText;
            header.Width = 200;
            header.Visible = true;
            header.ColIndex = e.ListHeader.GetChilds().Max(o => o.ColIndex) + 1;
            // 创建动态列2
            header = e.ListHeader.AddChild();// 将动态列放在列表的最后面
            //header = e.ListHeader.AddChild(1);// 将动态列放在列表的指定位置
            header.Key = "FDynamicColumn3";
            header.FieldName = "FDynamicColumn3";
            header.Caption = new LocaleValue("库存可用量");
            header.ColType = SqlStorageType.SqlText;
            header.Width = 300;
            header.Visible = true;
            header.ColIndex = e.ListHeader.GetChilds().Max(o => o.ColIndex) + 1;
        }
        public override void FormatCellValue(FormatCellValueArgs args)
        {
            base.FormatCellValue(args);
            string strlist = "";
            string billid = Convert.ToString(args.DataRow["FID"]);
            string entryid = Convert.ToString(args.DataRow["t1_FENTRYID"]);
            if (args.Header.Key.Equals("FDynamicColumn1", StringComparison.OrdinalIgnoreCase))
            {
                var dolist = ylobjects.Tables[0].Select($"FSRCENTRYID='{entryid}'");
                var grouped =
                    from grouplist in dolist
                    group grouplist by grouplist["FNUMBER"];

                foreach (var groupByGender in grouped.ToList())
                {
                    var gender = groupByGender.Key;
                    DataRow gendy = groupByGender.Select(x => x).ToList().First();
                    strlist += "{";
                    if (Convert.ToString(gendy["ORGNAME"]) == "深圳蚂蚁工场科技有限公司")
                    {
                        strlist += "深圳蚂蚁";
                    }
                    else
                    {
                        strlist += gendy["ORGNAME"];
                    }


                    foreach (var element in groupByGender)
                    {
                        strlist += "[";
                        strlist += element["stockname"];
                        strlist += ":";
                        strlist += Convert.ToDecimal(element["FBASEQTY"]).ToString("0.####");
                        strlist += element["FRESERVETYPE"];
                        strlist += "]";
                    }
                    strlist += "}";
                }
                args.FormateValue = string.Format("{0}", strlist);
            }

            if (args.Header.Key.Equals("FDynamicColumn2", StringComparison.OrdinalIgnoreCase))
            {
                var materialref = args.DataRow["FMaterialId_Ref"] as DynamicObject;
                string msterID = Convert.ToString(materialref["msterID"]);
                var dolist = ztobjects.Tables[0].Select($"FMATERIALID={msterID} and FSRCENTRYID='{entryid}'");
                var grouped =
                    from grouplist in dolist
                    group grouplist by grouplist["FNUMBER"];

                foreach (var groupByGender in grouped.ToList())
                {
                    var gender = groupByGender.Key;
                    DataRow gendy = groupByGender.Select(x => x).ToList().First();
                    strlist += "{";
                    strlist += gendy["ORGNAME"];

                    foreach (var element in groupByGender)
                    {
                        strlist += "[";
                        strlist += "计划在途";
                        strlist += ":";
                        strlist += Convert.ToDecimal(element["FBASEQTY"]).ToString("0.####");
                        strlist += "]";
                    }
                    strlist += "}";
                }
                args.FormateValue = string.Format("{0}", strlist);
            }

            if (args.Header.Key.Equals("FDynamicColumn3", StringComparison.OrdinalIgnoreCase))
            {
                var materialref = args.DataRow["FMaterialId_Ref"] as DynamicObject;
                //var orgid = args.DataRow["FStockOrgId_Id"];
                string starorgid = Convert.ToString(args.DataRow["FSupplyTargetOrgId_Id"]);
                string msterID = Convert.ToString(materialref["msterID"]);
                var dolist = kcobjects.Tables[0].Select($"FMATERIALID={msterID} and FSTOCKORGID={starorgid}");
                var grouped =
                    from grouplist in dolist
                    group grouplist by grouplist["FSTOCKORGID"];

                foreach (var groupByGender in grouped.ToList())
                {
                    var gender = groupByGender.Key;
                    DataRow gendy = groupByGender.Select(x => x).ToList().First();
                    strlist += "{";
                    strlist += gendy["ORGNAME"];

                    foreach (var element in groupByGender)
                    {
                        strlist += "[";
                        strlist += element["FNAME"];
                        strlist += ":";
                        strlist += Convert.ToDecimal(element["FBASEQTY"]).ToString("0.####");
                        strlist += "]";
                    }
                    strlist += "}";
                }
                args.FormateValue = string.Format("{0}", strlist);
            }
        }
    }
}
