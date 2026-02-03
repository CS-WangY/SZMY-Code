using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Util;
using static System.Net.WebRequestMethods;

namespace Kingdee.Mymooo.Report.Plugin.ReceivableManagement
{
    [Description("应收票据收发存明细表单插件"), HotUpdate]
    public class CN_BillRectransactReportList : AbstractSysReportPlugIn
    {
        public override void FormatCellValue(BOS.Core.Report.PlugIn.Args.FormatCellValueArgs args)
        {
            base.FormatCellValue(args);
            //var celltype = args.CellType;

            if (args.Header.Key == "FPARAMOUNTFOR")
            {
                if (Convert.ToInt64(args.DataRow["FGROUPLEVEL"]) == 1
                    && Convert.ToInt64(args.DataRow["FGROUPING"]) == 0)
                {
                    //完全拆分和部分拆分才算合计数
                    var fcunumber = Convert.ToString(args.DataRow["FCONTACTUNIT"]);
                    var paramountsubtotal = args.DataRow.Table.Compute("SUM(FPARAMOUNTFOR)", $"FCONTACTUNIT='{fcunumber}' AND FSOURRECBILL=0");
                    args.FormateValue = string.Format("{0}", Math.Round(Convert.ToDecimal(paramountsubtotal), 2));
                }
                if (Convert.ToInt64(args.DataRow["FGROUPLEVEL"]) == 1
                    && Convert.ToInt64(args.DataRow["FGROUPING"]) == 1)
                {
                    //完全拆分和部分拆分才算合计数
                    var paramountsubtotal = args.DataRow.Table.Compute("SUM(FPARAMOUNTFOR)", $"FSOURRECBILL=0");
                    args.FormateValue = string.Format("{0}", Math.Round(Convert.ToDecimal(paramountsubtotal), 2));
                }
            }
            if (args.Header.Key == "FBALANCE")
            {
                if (Convert.ToInt64(args.DataRow["FGROUPLEVEL"]) == 0 && Convert.ToInt64(args.DataRow["FSOURRECBILL"]) == 0)
                {
                    var fid = Convert.ToString(args.DataRow["FID"]);
                    var amtoun = Convert.ToDecimal(args.Value);
                    var sumpara = args.DataRow.Table.Compute("SUM(FPARAMOUNTFOR)", $"FSOURRECBILL={fid}");
                    decimal paramountsubtotal = 0;
                    if (sumpara.IsNullOrEmpty())
                    {
                        paramountsubtotal = 0;
                    }
                    else
                    {
                        paramountsubtotal = Convert.ToDecimal(sumpara);
                    }
                    args.FormateValue = string.Format("{0}", Math.Round(Convert.ToDecimal(amtoun - paramountsubtotal), 2));
                }
                if (Convert.ToInt64(args.DataRow["FGROUPLEVEL"]) == 0 && Convert.ToInt64(args.DataRow["FSOURRECBILL"]) > 0)
                {
                    args.FormateValue = string.Format("{0}", 0);
                }
            }
            if (args.CellType == BOS.Core.Enums.BOSEnums.Enu_ReportCellType.SubTotal)
            {
                var fcunumber = Convert.ToString(args.DataRow["FCONTACTUNIT"]);
                var fcunname = args.DataRow.Table.Select($"FCONTACTUNIT='{fcunumber}' AND FGROUPLEVEL=0").First()["FCONTACTUNIT_N"];
                args.DataRow["FCONTACTUNIT_N"] = fcunname;
            }
        }
    }
}
