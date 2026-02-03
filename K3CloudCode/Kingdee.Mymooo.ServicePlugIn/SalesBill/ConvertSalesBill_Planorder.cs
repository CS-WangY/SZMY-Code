using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单下推计划订单触发实体"), HotUpdate]
    public class ConvertSalesBill_Planorder : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            BusinessInfo businessInfo = e.TargetBusinessInfo;
            BaseDataField bdField = businessInfo.GetField("FReleaseBillType") as BaseDataField;
            QueryBuilderParemeter p = new QueryBuilderParemeter();
            p.FormId = "BOS_BillType";
            //p.SelectItems = SelectorItemInfo.CreateItems("FMaterialID");
            p.FilterClauseWihtKey = $"FNUMBER='SCDD03_SYS'";
            p.OrderByClauseWihtKey = " FNumber";
            var bill_type = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p);

            // 目标单单据体元数据
            Entity entity = e.TargetBusinessInfo.GetEntity("FBillHead");
            // 读取已经生成的发货通知单
            ExtendedDataEntity[] bills = e.Result.FindByEntityKey("FBillHead");
            bills[0].DataEntity["ReleaseType"] = 1;
            var plentry = bills[0].DataEntity["PLSubHead"] as DynamicObjectCollection;
            //对目标单据进行循环
            foreach (var bill in plentry)
            {
                if (bill_type.Count() > 0)
                {
                    bill["ReleaseBillType_Id"] = bill_type[0]["Id"];
                    bill["ReleaseBillType"] = bill_type[0];
                }
                else
                {
                    throw new Exception("无法获取对应生产订单类型!");
                }
            }
        }
    }
}
