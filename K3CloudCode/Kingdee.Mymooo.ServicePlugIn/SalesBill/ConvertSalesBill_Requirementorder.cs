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
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单-组织间需求单-单据转换更新供应组织物料"), HotUpdate]
    public class ConvertSalesBill_Requirementorder : AbstractConvertPlugIn
    {
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            base.OnAfterCreateLink(e);

            BusinessInfo businessInfo = e.TargetBusinessInfo;
            BaseDataField bdField = businessInfo.GetField("FMaterialID") as BaseDataField;
            QueryBuilderParemeter p = new QueryBuilderParemeter();
            p.FormId = "BD_Material";
            p.SelectItems = SelectorItemInfo.CreateItems("FMaterialID");

            // 目标单单据体元数据
            Entity entity = e.TargetBusinessInfo.GetEntity("FEntity");
            // 读取已经生成的发货通知单
            ExtendedDataEntity[] bills = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");
            // 对目标单据进行循环
            foreach (var bill in bills)
            {
                var mid = bill.DataEntity["MaterialID"] as DynamicObject;
                var devorgid = Convert.ToInt64(bill.DataEntity["SupplyOrgId_Id"]);
                p.FilterClauseWihtKey = $"FMASTERID={mid["msterID"]} AND FUSEORGID={devorgid}";
                p.OrderByClauseWihtKey = " FNumber";
                var obj_ck = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p);
                if (obj_ck.Count() > 0)
                {
                    bill.DataEntity["SupplyMaterialId_Id"] = obj_ck[0]["Id"];
                    bill.DataEntity["SupplyMaterialId"] = obj_ck[0];
                }
                else
                {
                    throw new Exception("无法获取对应供货组织物料ID!");
                }
            }

        }

        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            BusinessInfo businessInfo = e.TargetBusinessInfo;
            BaseDataField bdField = businessInfo.GetField("FMaterialID") as BaseDataField;
            QueryBuilderParemeter p = new QueryBuilderParemeter();
            p.FormId = "BD_Material";
            p.SelectItems = SelectorItemInfo.CreateItems("FMaterialID");

            // 目标单单据体元数据
            Entity entity = e.TargetBusinessInfo.GetEntity("FEntity");
            // 读取已经生成的发货通知单
            ExtendedDataEntity[] bills = e.Result.FindByEntityKey("FBillHead");
            // 对目标单据进行循环
            foreach (var bill in bills)
            {
                var mid = bill.DataEntity["MaterialID"] as DynamicObject;
                var devorgid = Convert.ToInt64(bill.DataEntity["SupplyOrgId_Id"]);
                p.FilterClauseWihtKey = $"FMASTERID={mid["msterID"]} AND FUSEORGID={devorgid}";
                p.OrderByClauseWihtKey = " FNumber";
                var obj_ck = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p);
                if (obj_ck.Count() > 0)
                {
                    bill.DataEntity["SupplyMaterialId_Id"] = obj_ck[0]["Id"];
                    bill.DataEntity["SupplyMaterialId"] = obj_ck[0];
                }
                else
                {
                    throw new Exception("无法获取对应供货组织物料ID!");
                }
            }
        }
    }

}
