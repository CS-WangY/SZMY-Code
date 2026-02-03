using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("计划订单转采购订单携带销售员"), HotUpdate]
    public class ConvertPlanBillToPOOrder : AbstractConvertPlugIn
    {
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            base.OnAfterCreateLink(e);
            // 目标单单据体元数据
            Entity entity = e.TargetBusinessInfo.GetEntity("FEntity");
            // 读取已经生成的发货通知单
            ExtendedDataEntity[] bills = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");
            // 对目标单据进行循环
            foreach (var bill in bills)
            {
                // 取单据体集合
                DynamicObjectCollection rowObjs = entity.DynamicProperty.GetValue(bill.DataEntity)
                as DynamicObjectCollection;
                foreach (var item in rowObjs)
                {
                    var itemNo = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"].ToString());
                    if (item["SrcBillTypeId"].ToString().EqualsIgnoreCase("PLN_PLANORDER"))
                    {
                        string sSql = $@"SELECT t2.FID FROM T_PLN_PLANORDER_B t1
                        INNER JOIN T_PLN_PLANORDER t2 ON t1.FID=t2.FID
                        WHERE t2.FBILLNO='{item["SrcBillNo"]}' AND t1.FDEMANDTYPE=1";
                        var salordata= DBUtils.ExecuteDynamicObject(this.Context, sSql);
                        if (salordata.Count() > 0)
                        {
                            sSql = $@"/*dialect*/SELECT DISTINCT t5.FDELIVERYDATE,t6.FSALERID,t7.FNAME FROM T_PLN_PLANORDER_LK t1
                                    LEFT JOIN T_PLN_PLANORDER t2 ON t1.FID=t2.FID
                                    LEFT JOIN T_PLN_PLANORDER t3 ON t1.FSBILLID=t3.FID
                                    LEFT JOIN T_PLN_PLANORDER_B t4 ON t3.FID=t4.FID
                                    LEFT JOIN T_SAL_ORDERENTRYDELIPLAN t5 ON t4.FSALEORDERENTRYID=t5.FENTRYID
                                    LEFT JOIN T_SAL_ORDER t6 ON t4.FSALEORDERID=t6.FID
                                    LEFT JOIN V_BD_SALESMAN_L t7 ON t6.FSALERID=t7.fid
									LEFT JOIN T_SAL_ORDERENTRY t8 ON t6.FID=t8.FID
									LEFT JOIN T_BD_MATERIAL t9 on t8.FMATERIALID=t9.FMATERIALID
                                    WHERE t2.FBILLNO='{item["SrcBillNo"]}' and t9.FNUMBER='{itemNo}' ";
                            var data = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            var salUser = string.Join(",", data.Select(x => x["FNAME"]).Distinct());
                            if (data.Count > 0)
                            {
                                item["FPENYDELIVERYDATE"] = data[0]["FDELIVERYDATE"];
                                item["FPENYSALERS"] = salUser;
                            }
                            else
                            {
                                sSql = $@"/*dialect*/SELECT DISTINCT t5.FDELIVERYDATE,t6.FSALERID,t7.FNAME FROM T_PLN_PLANORDER t2
                                    LEFT JOIN T_PLN_PLANORDER_B t4 ON t2.FID=t4.FID
                                    LEFT JOIN T_SAL_ORDERENTRYDELIPLAN t5 ON t4.FSALEORDERENTRYID=t5.FENTRYID
                                    LEFT JOIN T_SAL_ORDER t6 ON t4.FSALEORDERID=t6.FID
                                    LEFT JOIN V_BD_SALESMAN_L t7 ON t6.FSALERID=t7.fid
									LEFT JOIN T_SAL_ORDERENTRY t8 ON t6.FID=t8.FID
									LEFT JOIN T_BD_MATERIAL t9 on t8.FMATERIALID=t9.FMATERIALID
                                    WHERE t2.FBILLNO='{item["SrcBillNo"]}' and t9.FNUMBER='{itemNo}' ";
                                data = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                                salUser = string.Join(",", data.Select(x => x["FNAME"]).Distinct());
                                if (data.Count > 0)
                                {
                                    item["FPENYDELIVERYDATE"] = data[0]["FDELIVERYDATE"];
                                    item["FPENYSALERS"] = salUser;
                                }
                            }

                            sSql = $@"/*dialect*/SELECT DISTINCT t5.FCustMaterialNo,t5.FCustItemNo,t5.FCustItemName FROM T_PLN_PLANORDER_LK t1
                                    LEFT JOIN T_PLN_PLANORDER t2 ON t1.FID=t2.FID
                                    LEFT JOIN T_PLN_PLANORDER t3 ON t1.FSBILLID=t3.FID
                                    LEFT JOIN T_PLN_PLANORDER_B t4 ON t3.FID=t4.FID
                                    LEFT JOIN T_SAL_ORDERENTRY t5 ON t4.FSALEORDERENTRYID=t5.FENTRYID
                                    LEFT JOIN T_BD_MATERIAL t9 on t5.FMATERIALID=t9.FMATERIALID
                                    WHERE t2.FBILLNO='{item["SrcBillNo"]}' and t9.FNUMBER='{itemNo}' ";
                            data = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            var lists = string.Join(",", data.Select(x => x["FCustMaterialNo"]));
                            var erpcustno = string.Join(",", lists.Split(',').Distinct());
                            if (erpcustno.Length > 200)
                            {
                                erpcustno = erpcustno.Substring(0, 200);
                            }
                            lists = string.Join(",", data.Select(x => x["FCustItemNo"]));
                            var custno = string.Join(",", lists.Split(',').Distinct());
                            lists = string.Join(",", data.Select(x => x["FCustItemName"]));
                            var custname = string.Join(",", lists.Split(',').Distinct());
                            if (data.Count > 0)
                            {
                                item["FCUSTMATERIALNO"] = erpcustno;
                                item["FPENYMAPCODE"] = custno;
                                item["FPENYMAPNAME"] = custname;
                            }
                            else
                            {
                                sSql = $@"/*dialect*/SELECT DISTINCT t5.FCustMaterialNo,t5.FCustItemNo,t5.FCustItemName FROM T_PLN_PLANORDER t2
                                    LEFT JOIN T_PLN_PLANORDER_B t4 ON t2.FID=t4.FID
                                    LEFT JOIN T_SAL_ORDERENTRY t5 ON t4.FSALEORDERENTRYID=t5.FENTRYID
                                    LEFT JOIN T_BD_MATERIAL t9 on t5.FMATERIALID=t9.FMATERIALID
                                    WHERE t2.FBILLNO='{item["SrcBillNo"]}' and t9.FNUMBER='{itemNo}' ";
                                data = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                                lists = string.Join(",", data.Select(x => x["FCustMaterialNo"]));
                                erpcustno = string.Join(",", lists.Split(',').Distinct());
                                if (erpcustno.Length > 200)
                                {
                                    erpcustno = erpcustno.Substring(0, 200);
                                }
                                lists = string.Join(",", data.Select(x => x["FCustItemNo"]));
                                custno = string.Join(",", lists.Split(',').Distinct());
                                lists = string.Join(",", data.Select(x => x["FCustItemName"]));
                                custname = string.Join(",", lists.Split(',').Distinct());
                                if (data.Count > 0)
                                {
                                    item["FCUSTMATERIALNO"] = erpcustno;
                                    item["FPENYMAPCODE"] = custno;
                                    item["FPENYMAPNAME"] = custname;
                                }
                            }

                            //携带销售订单客户
                            var saleid = Convert.ToInt64(item["DEMANDBILLENTRYID"]);
                            if (saleid > 0)
                            {
                                sSql = $@"SELECT t3.FNAME FROM T_SAL_ORDERENTRY t1
                                    INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
                                    LEFT JOIN dbo.T_BD_CUSTOMER_L t3 ON t2.FCUSTID=t3.FCUSTID
                                    WHERE t1.FENTRYID={saleid}";
                                var cusname = DBUtils.ExecuteScalar<string>(this.Context, sSql, "");
                                item["FPENYSALCUSTNAME"] = cusname;
                            }
                            else
                            {
                                lists = "";
                                foreach (var link in item["FEntity_Link"] as DynamicObjectCollection)
                                {
                                    if (link["STableName"].ToString().EqualsIgnoreCase("T_PLN_PLANORDER"))
                                    {
                                        var plnid = Convert.ToInt64(link["SBillId"]);
                                        sSql = $@"SELECT t4.FNAME FROM T_PLN_PLANORDER_LK t1
                                        LEFT JOIN T_PLN_PLANORDER_B t2 ON t1.FSBILLID=t2.FID
                                        LEFT JOIN dbo.T_SAL_ORDER t3 ON t3.FID=t2.FSALEORDERID
                                        LEFT JOIN dbo.T_BD_CUSTOMER_L t4 ON t3.FCUSTID=t4.FCUSTID
                                        WHERE t1.FID={plnid}";
                                        data = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                                        lists = lists + string.Join(",", data.Select(x => x["FNAME"]));
                                    }
                                }
                                item["FPENYSALCUSTNAME"] = string.Join(",", lists.Split(',').Distinct());
                            }
                        }
                        
                    }

                }

            }

        }
    }
}
