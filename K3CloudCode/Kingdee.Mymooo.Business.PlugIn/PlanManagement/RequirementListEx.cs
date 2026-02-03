using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.PlanManagement
{
    [Description("组织间需求单列表增加预留数量显示"), HotUpdate]
    public class RequirementListEx : AbstractListPlugIn
    {
        public DynamicObjectCollection ylobjects { get; set; }
        public DynamicObjectCollection ztobjects { get; set; }
        public DynamicObjectCollection kcobjects { get; set; }
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string sSql = $@"/*dialect*/SELECT * FROM v_ReservedStockAllRO";
            ylobjects = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
            sSql = $@"/*dialect*/SELECT * FROM v_TranmitStockAllRO";
            ztobjects = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
            sSql = $@"SELECT t5.FNUMBER,t6.FNAME as ORGNAME,t1.FSTOCKID,ISNULL(t1.FAVBQTY,0) as FBASEQTY
                        ,t4.FNAME,t1.FMATERIALID,t1.FSTOCKORGID FROM V_STK_INVENTORY_CUS t1
                        LEFT JOIN T_BD_STOCK_L t4 on t1.FSTOCKID=t4.FSTOCKID
                        LEFT JOIN T_ORG_ORGANIZATIONS t5 ON t1.FSTOCKORGID=t5.FORGID
                        LEFT JOIN T_ORG_ORGANIZATIONS_L t6 ON t5.FORGID=t6.FORGID
                        WHERE t1.FAVBQTY>0
                        ORDER BY t5.FNUMBER";
            kcobjects = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
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
            if (args.Header.Key.Equals("FDynamicColumn1", StringComparison.OrdinalIgnoreCase))
            {
                string billid = Convert.ToString(args.DataRow["FID"]);

                var dolist = ylobjects.Where(x => Convert.ToString(x["FDEMANDINTERID"]) == billid).ToList();
                var grouped =
                    from grouplist in dolist
                    group grouplist by grouplist["FNUMBER"];

                foreach (var groupByGender in grouped.ToList())
                {
                    var gender = groupByGender.Key;
                    DynamicObject gendy = groupByGender.Select(x => x).ToList().First();
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
                string billid = Convert.ToString(args.DataRow["FID"]);
                var dolist = ztobjects.Where(x => Convert.ToString(x["FDEMANDINTERID"]) == billid).ToList();
                var grouped =
                    from grouplist in dolist
                    group grouplist by grouplist["FNUMBER"];

                foreach (var groupByGender in grouped.ToList())
                {
                    var gender = groupByGender.Key;
                    DynamicObject gendy = groupByGender.Select(x => x).ToList().First();
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
                string starorgid = Convert.ToString(args.DataRow["FSupplyOrganId_Id"]);
                string msterID = Convert.ToString(materialref["msterID"]);
                var dolist = kcobjects.Where(x => Convert.ToString(x["FMATERIALID"]) == msterID

                && Convert.ToString(x["FSTOCKORGID"]) == starorgid).ToList();
                //string sSql = $@"SELECT t5.FNUMBER,t6.FNAME as ORGNAME,t1.FSTOCKID,ISNULL(t1.FAVBQTY,0) as FBASEQTY,t4.FNAME FROM V_STK_INVENTORY_CUS t1
                //        LEFT JOIN T_BD_STOCK_L t4 on t1.FSTOCKID=t4.FSTOCKID
                //        LEFT JOIN T_ORG_ORGANIZATIONS t5 ON t1.FSTOCKORGID=t5.FORGID
                //        LEFT JOIN T_ORG_ORGANIZATIONS_L t6 ON t5.FORGID=t6.FORGID
                //        WHERE t1.FMATERIALID='{msterID}' AND t1.FSTOCKORGID IN ('{starorgid}') AND t1.FAVBQTY>0
                //        ORDER BY t5.FNUMBER";
                //var dolist = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                var grouped =
                    from grouplist in dolist
                    group grouplist by grouplist["FNUMBER"];

                foreach (var groupByGender in grouped.ToList())
                {
                    var gender = groupByGender.Key;
                    DynamicObject gendy = groupByGender.Select(x => x).ToList().First();
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

        public override void AfterCreateSqlBuilderParameter(SqlBuilderParameterArgs e)
        {
            base.AfterCreateSqlBuilderParameter(e);
			if (e.sqlBuilderParameter.FilterClauseWihtKey.Contains("FMeetFilter = '0'"))
            {
                e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.Replace("FMeetFilter = '0'", "ISNULL(tp801.fybaseqty,0)>0");
            }
            if (e.sqlBuilderParameter.FilterClauseWihtKey.Contains("FMeetFilter = '1'"))
            {
                e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.Replace("FMeetFilter = '1'", "ISNULL(tp801.fybaseqty,0)<=0");
            }
            // 表连接之表变量
            //if (sOperator == "") return;
            //var sqlParam = new SqlParam("@PKValue", KDDbType.udt_inttable, pks);
            var pkTmpTable = $@"
                            SELECT ISNULL(SUM(t1.FBASEQTY),0) AS FYBASEQTY,t2.FDEMANDINTERID
                            FROM T_PLN_RESERVELINKENTRY t1
                            INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                            WHERE FDEMANDFORMID IN ('PLN_REQUIREMENTORDER')
                            GROUP BY t2.FDEMANDINTERID";
            var joinTable = new ExtJoinTableDescription
            {
                JoinOption = QueryBuilderParemeter.JoinOption.LeftJoin,
                TableName = pkTmpTable,
                TableNameAs = "tp801",
                FieldName = "FDEMANDINTERID",
                ScourceKey = "FID",
            };
            //e.sqlBuilderParameter.SqlParams.Add(sqlParam);
            e.sqlBuilderParameter.ExtJoinTables.Add(joinTable);
            //// 表连接之物理表
            //var joinTable2 = new ExtJoinTableDescription
            //{
            //    TableName = "T_PUR_POCHANGE",
            //    TableNameAs = "tp802",
            //    FieldName = "FID",
            //    ScourceKey = "FID"
            //};
            //e.sqlBuilderParameter.ExtJoinTables.Add(joinTable2);
            //// 修改隔离组织
            //e.sqlBuilderParameter.IsolationOrgList.Add(100001);
        }
    }
}
