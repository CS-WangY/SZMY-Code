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
    [Description("销售订单派产云平台单据列表插件")]
    public class DispatchFBListPlugIn : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            //销售订单派产云平台
            if (e.BarItemKey == "PENY_DispatchFB")
            {
                var list = this.ListView.SelectedRowsInfo;
                if (list.Count == 0)
                {
                    this.View.ShowMessage("没有选择派产云平台的销售订单");
                }

                List<DispatchEntity> des = new List<DispatchEntity>();             
                foreach (var de in list.GroupBy(a => a.BillNo))
                {
                    List<string> ps = new List<string>();
                    List<int> psID = new List<int>();
                    DispatchEntity entity = new DispatchEntity();
                    entity.SalesOrderNumber = de.Key;
                    foreach (var p in list.Where(b => b.BillNo == de.Key))
                    {
                        List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FENTRYID", KDDbType.Int32, Convert.ToInt32(p.EntryPrimaryKeyValue)) };
                        var sql = $@"select top 1 m.FNUMBER from T_BD_MATERIAL m inner join T_SAL_ORDERENTRY oe on m.FMATERIALID=oe.FMATERIALID where oe.FENTRYID=@FENTRYID";
                        var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());

                        //获取物料编码
                        var FNUMBER = "";
                        foreach (var d in datas)
                        {
                            FNUMBER = d["FNUMBER"].ToString();
                        }
                        ps.Add(FNUMBER);
                        psID.Add(Convert.ToInt32(p.EntryPrimaryKeyValue));
                    }
                    entity.Parts = ps;
                    entity.PartsID = psID;
                    entity.DispatchDateTime = DateTime.Now;
                    entity.DispatchUser = "admin";
                    des.Add(entity);
                }
                foreach (var item in des)
                {
                    //组装数据
                    DispatchEntity dEntity = new DispatchEntity();
                    dEntity.SalesOrderNumber = item.SalesOrderNumber;
                    dEntity.Parts = item.Parts;
                    dEntity.DispatchDateTime = item.DispatchDateTime;
                    dEntity.DispatchUser = item.DispatchUser;

                    //API派产销售订单数据给云平台 
                    var timestamp = WebApiSignature.CreateTimestamp();
                    var nonce = "b80b9c1148434c8fb975185238a7965c";
                    var signature = WebApiSignature.Sign(timestamp, nonce, "");
                    //var msg = CommonApiRequest.WorkbenchSignatureInvokeWebService(timestamp, signature, WebUIConfigHelper.DispatchToCloudUrl + "api/cnc/dispatch", JsonConvertUtils.SerializeObject(dEntity), "post");

                    //if (!string.IsNullOrWhiteSpace(msg))
                    //{
                    //    var result = JsonConvertUtils.DeserializeObject<MessageHelp>(msg);
                    //    if (result.IsSuccess)
                    //    {
                    //        //销售订单明细表增加了是否派产云平台字段FISDISPATCH（是和否）
                    //        //更新表字段值

                    //        string feids = "";
                    //        foreach (var feid in item.PartsID)
                    //        {
                    //            feids += feid.ToString() + ",";
                    //        }
                    //        var updateSql = $@"update T_SAL_ORDERENTRY set FISDISPATCH = '1' where FENTRYID in ({feids.TrimEnd(',')})";
                    //        //List<SqlParam> paramList = new List<SqlParam>()
                    //        //{
                    //        //    new SqlParam("@FENTRYID", KDDbType.String, feids.TrimEnd(','))
                    //        //};
                    //        DBServiceHelper.Execute(this.Context, updateSql);

                    //        this.View.ShowMessage("销售订单" + dEntity.SalesOrderNumber + "派产云平台成功");
                    //    }
                    //    else
                    //    {
                    //        this.View.ShowMessage("销售订单" + dEntity.SalesOrderNumber + "派产云平台错误：" + result.Message);
                    //    }
                    //}
                }

                this.View.ShowMessage("派产云平台完成");
                this.View.Refresh();
            }

        }
    }
}
