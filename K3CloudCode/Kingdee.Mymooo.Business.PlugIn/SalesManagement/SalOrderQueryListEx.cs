using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("销售订单列表增加可发量字段"), HotUpdate]
    public class SalOrderQueryListEx : AbstractListPlugIn
    {
        public DataSet ylobjects { get; set; }
        public DataSet ztobjects { get; set; }
        public DataSet kcobjects { get; set; }
        public bool checkColor { get; set; }
        public string parameterColor { get; set; }
        public string deliveryColor { get; set; }

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
            checkColor = Convert.ToBoolean(ReadUserParameter("FPENYCheckBoxColor"));
            parameterColor = Convert.ToString(ReadUserParameter("FPENYColor"));
            deliveryColor = Convert.ToString(ReadUserParameter("FDeliveryDateColor"));
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
                //string starorgid = Convert.ToString(args.DataRow["FSupplyTargetOrgId_Id"]);
                //string stockorgid= Convert.ToString(args.DataRow["FStockOrgId_Id"]);
                string billid = Convert.ToString(args.DataRow["FID"]);
                string entryid = Convert.ToString(args.DataRow["t2_FENTRYID"]);
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
                string billid = Convert.ToString(args.DataRow["FID"]);
                string entryid = Convert.ToString(args.DataRow["t2_FENTRYID"]);
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

        /// <summary>
        /// 准备过滤参数（此事件主要用于附加各种SQL过滤条件）
        /// </summary>
        /// <param name="e"></param>
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);
            //e.SQLType = 0;
            // 文本字段
            //e.FilterString = e.FilterString.JoinFilterString("FBillNo LIKE 'CG%'");
            // 基础资料字段(基础资料字段标识.引用字段标识)
            //e.FilterString = e.FilterString.JoinFilterString("FMaterialId.FNumber Like N'%0.0000.01%'");
            //// 单据体主键字段(单据体标识_分录主键标识)
            //e.FilterString = e.FilterString.JoinFilterString("FPOOrderEntry_FEntryID NOT IN (1,2,3)");
            //// 子单据体主键字段(单据体标识_分录主键标识)
            //e.FilterString = e.FilterString.JoinFilterString("FEntryDeliveryPlan_FDetailId NOT IN (5,6,7)");
        }

        /// <summary>
        /// 在构建完sql取数参数之后进一步干预列表过滤（）
        /// 1.此事件在PrepareFilterParameter事件之后执行
        /// 2.整个SqlBuilderParameter都当成参数传入事件中，相比PrepareFilterParameter事件，此事件中插件可操控范围更大了
        /// 3.此事件是能对过滤动作进行干预的最后的机会
        /// </summary>
        /// <param name="e"></param>
        public override void AfterCreateSqlBuilderParameter(SqlBuilderParameterArgs e)
        {
            base.AfterCreateSqlBuilderParameter(e);
            // 附加常规过滤条件
            //e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.JoinFilterString("FBillNo LIKE '%DD%'");
            var sOperator = "";
            if (e.sqlBuilderParameter.FilterClauseWihtKey.Contains("FMeetFilter = '0'"))
            {
                sOperator = "<=";
                e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.Replace("FMeetFilter = '0'", "1=1");
            }
            if (e.sqlBuilderParameter.FilterClauseWihtKey.Contains("FMeetFilter = '1'"))
            {
                sOperator = ">";
                e.sqlBuilderParameter.FilterClauseWihtKey = e.sqlBuilderParameter.FilterClauseWihtKey.Replace("FMeetFilter = '1'", "1=1");
            }
            // 表连接之表变量
            if (sOperator == "") return;
            //var sqlParam = new SqlParam("@PKValue", KDDbType.udt_inttable, pks);
            var pkTmpTable = $@"SELECT t1.FENTRYID,t1.FMATERIALID
                            ,t1.FQTY
                            ,ISNULL(t2.FBASEQTY,0) AS FBASEQTY,ISNULL(t3.FAVBQTY,0) AS FAVBQTY FROM T_SAL_ORDERENTRY t1
                            LEFT JOIN T_BD_MATERIAL M ON t1.FMATERIALID=M.FMATERIALID
                            LEFT JOIN (
                            SELECT SUM(t1.FBASEQTY) as FBASEQTY,t2.FSRCINTERID,t2.FSRCENTRYID,m.FMASTERID
                            FROM T_PLN_RESERVELINKENTRY t1
                            LEFT JOIN T_BD_MATERIAL m ON t1.FMATERIALID=m.FMATERIALID
							LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                            where t1.FSUPPLYFORMID='STK_Inventory'
                            GROUP BY t2.FSRCINTERID,t2.FSRCENTRYID,m.FMASTERID
							) t2 ON t1.FID =t2.FSRCINTERID AND t1.FENTRYID=t2.FSRCENTRYID AND t2.FMASTERID=m.FMASTERID
                            LEFT JOIN
                            (
                            SELECT t1.FMATERIALID,t1.FSTOCKORGID,SUM(ISNULL(t1.FAVBQTY,0)) as FAVBQTY from V_STK_INVENTORY_CUS t1
                            GROUP BY t1.FMATERIALID,t1.FSTOCKORGID
                            ) t3 ON M.FMASTERID=t3.FMATERIALID AND t3.FSTOCKORGID=t1.FSUPPLYTARGETORGID
                            WHERE ISNULL(t2.FBASEQTY,0) {sOperator} 0";
            var joinTable = new ExtJoinTableDescription
            {
                //JoinOption = QueryBuilderParemeter.JoinOption.LeftJoin,
                TableName = pkTmpTable,
                TableNameAs = "tp801",
                FieldName = "FENTRYID",
                ScourceKey = "t2.FENTRYID",
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

        public override void OnFormatRowConditions(ListFormatConditionArgs args)
        {
            base.OnFormatRowConditions(args);
            if (!checkColor)
            {
                return;
            }
            if (args.DataRow["FCloseStatus"].ToString() == "B" || args.DataRow["FMrpCloseStatus"].ToString() == "B")
            {
                return;
            }
            //为判断日期,需要设置2个日期
            DateTime dt1;//单据日期
            DateTime dt2 = Convert.ToDateTime(DateTime.Now);//当前系统时间

            //定义FormatCondition 类型字段
            FormatCondition fc = new FormatCondition();
            //打开单据的时候,触发
            fc.ApplayRow = true;
            //加载后,如果,单据状态FDOCUMENTSTATUS字段,不等于审核C,即未审核单据
            if (args.DataRow["FDOCUMENTSTATUS"].ToString() == "C")
            {
                //取单据日期,转换成日期格式,赋值给dt1
                dt1 = Convert.ToDateTime(args.DataRow["FDELIVERYDATE"].ToString());
                var canoutqty = Convert.ToDecimal(args.DataRow["FCANOUTQTY"]);
                TimeSpan timeSpan = dt1.Date.Subtract(dt2.Date);
                //int subday = dt1.Day - dt2.Day;
                if (timeSpan.Days <= 2)
                {
                    fc.ForeColor = deliveryColor;
                }
                if (dt2 > dt1 && canoutqty > 0)
                {
                    fc.ForeColor = parameterColor;
                }
                args.FormatConditions.Add(fc);
            }
        }
        /// <summary>
        /// 读取用户参数（从Model中读取）
        /// </summary>
        /// <param name="parameterName">用户参数的实体属性名</param>
        /// <returns></returns>

        private object ReadUserParameter(string parameterName)
        {
            // 从用户参数包中获取某一个参数
            if (this.View.Model.ParameterData != null && this.View.Model.ParameterData.DynamicObjectType.Properties.Contains(parameterName))
            {
                return this.View.Model.ParameterData[parameterName];
            }
            return null;
        }
    }
}
