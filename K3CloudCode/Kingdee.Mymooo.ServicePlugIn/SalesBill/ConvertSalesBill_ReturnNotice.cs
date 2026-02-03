using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单下推退货通知修改仓库"), HotUpdate]
    public class ConvertSalesBill_ReturnNotice : AbstractConvertPlugIn
    {
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
                var devorgid = Convert.ToInt64(bill.DataEntity["SaleOrgId_Id"]);
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
                        p.FilterClauseWihtKey = $"FOUTSOURCESTOCKLOC='{outsource}' and FUSEORGID={devorgid} and FDOCUMENTSTATUS='C'";
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
        /// <summary>
        /// 获取销售订单外发仓
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SoEid"></param>
        /// <returns></returns>
        private long GetSaleOrgStockId(Context ctx, long SoEid)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SoEid", KDDbType.Int64, SoEid) };
            string sql = $@"SELECT top 1 t1.FSTOCKID FROM dbo.T_BD_STOCK t1
                            INNER JOIN dbo.T_SAL_ORDERENTRY t2 ON t1.FUSEORGID=t2.FSTOCKORGID
                            WHERE t2.FENTRYID=@SoEid AND t1.FISDIRSTOCK=0 AND t1.FDOCUMENTSTATUS='C'
                            ORDER BY t1.FNUMBER";
            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }
    }
}
