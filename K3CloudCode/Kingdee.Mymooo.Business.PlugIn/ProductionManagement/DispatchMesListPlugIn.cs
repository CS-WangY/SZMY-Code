using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    [Description("生产订单派产Mes单据列表插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class DispatchMesListPlugIn : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            //生产订单派产Mes
            if (e.BarItemKey == "PENY_DispatchMes")
            {
                //var context = LoginServiceUtils.SignLogin(null);
                var list = this.ListView.SelectedRowsInfo;
                if (list.Count == 0)
                {
                    this.View.ShowMessage("没有选择派产Mes的生产订单");
                }
                foreach (var item in list)
                {

                    var sqlIsExist = @" select top 1 c.FNUMBER,b.FQTY,b.FPLANFINISHDATE,b.FBOMID,d.FNUMBER as FBOMVER from T_PRD_MO a inner join T_PRD_MOENTRY b 
                                        on a.FID=b.FID left join T_BD_MATERIAL c on b.FMATERIALID=c.FMATERIALID left join T_ENG_BOM d on b.FBOMID=d.FID
                                        where a.FBILLNO='" + item.BillNo + "'";
                    //List<SqlParam> paramList = new List<SqlParam>()
                    //        {
                    //            new SqlParam("@FBILLNO", KDDbType.String, item.BillNo)
                    //        };
                    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sqlIsExist);

                    ErpToMesForOrder dataMes = new ErpToMesForOrder();
                    dataMes.woNo = Convert.ToString(item.BillNo);
                    dataMes.woSeqNo = Convert.ToString(1);
                    dataMes.drawNum = Convert.ToString("");//暂时
                    dataMes.drawNumVersion = "A";//暂时
                    dataMes.orderNum = Convert.ToInt32(0);//暂时
                    dataMes.deadLine = Convert.ToDateTime(DateTime.Now).AddMonths(1).ToString("yyyy-MM-dd");//暂时
                    dataMes.remarks = "";

                    if (datas.Count > 0)
                    {
                        dataMes.drawNum = datas[0]["FNUMBER"].ToString();//从表中获取
                        int index = datas[0]["FBOMVER"].ToString().LastIndexOf('_')+1;
                        dataMes.drawNumVersion = datas[0]["FBOMVER"].ToString().Substring(index, datas[0]["FBOMVER"].ToString().Length-index);
                        dataMes.orderNum = Convert.ToInt32(datas[0]["FQTY"]);//从表中获取
                        dataMes.deadLine = Convert.ToDateTime(datas[0]["FPLANFINISHDATE"]).ToString("yyyy-MM-dd");//从表中获取
                    }

                    var package = new
                    {
                        action = "mymoooErpToMesForOrder",
                        id = Guid.NewGuid(),
                        data = new dynamic[] { dataMes }
                    };

                    var timestamp = WebApiSignature.CreateTimestamp();
                    var nonce = "b80b9c1148434c8fb975185238a7965c";
                    var signature = WebApiSignature.Sign(timestamp, nonce, "");
                    var msg = CommonApiRequest.WorkbenchSignatureInvokeWebService(timestamp, signature, WebUIConfigHelper.WorkbenchForMqMesUrl, JsonConvertUtils.SerializeObject(package), "post");

                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        var result = JsonConvertUtils.DeserializeObject<MessageHelpMes>(msg);
                        if (result.IsSuccess)
                        {
                            var updateSql = $@"/*dialect*/update me set me.FISDISPATCHMES=1 from T_PRD_MOENTRY me inner join T_PRD_MO m on me.FID=m.FID where m.FBILLNO='{item.BillNo}'";
                            DBServiceHelper.Execute(this.Context, updateSql);
                        }
                        else
                        {
                            var err = new Exception($"/mymoooErpToMesForOrder 生产订单派产Mes错误：“{result.errorMessage}”");
                            //LogHelper.Exception(err);
                            this.View.ShowMessage($"/mymoooErpToMesForOrder 生产订单派产Mes错误：“{result.errorMessage}”");
                        }
                    }
                }
                this.View.ShowMessage("生产订单派产Mes成功");
                this.View.Refresh();
            }

            //this.View.ShowMessage("生产订单派产Mes菜单点击");
        }
    }
}
