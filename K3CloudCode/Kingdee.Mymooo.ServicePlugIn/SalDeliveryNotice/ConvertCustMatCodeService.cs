using Kingdee.BOS.App.Core.Convertible.UnitConvert;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Kingdee.BOS.Core.Metadata;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单单据转换携带客户物料"), HotUpdate]
    public class ConvertCustMatCodeService : AbstractConvertPlugIn
    {
        //原单据转换不要动，在原单据转换的基础上修改客户物料号对应的行发货组织
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            string propertyName = "";
            //string custMapIdKey = "";
            string text = "";
            //string text2 = "";
            //string propertyName2 = "";
            string propertyName3 = "";
            string id = e.TargetBusinessInfo.GetForm().Id;
            ExtendedDataEntity[] array = e.Result.FindByEntityKey("FBillHead");
            switch (id.ToUpperInvariant())
            {
                case "SAL_DELIVERYNOTICE":
                    propertyName = "SAL_DELIVERYNOTICEENTRY";
                    //custMapIdKey = "FCustMatID";
                    text = "CustMatID";
                    //text2 = "MaterialId_Id";
                    //propertyName2 = "CustomerId_Id";
                    propertyName3 = "DeliveryOrgID_Id";
                    //propertyName3 = "DeliveryOrgId_Id";
                    break;
            }
            DynamicObjectCollection dynamicObjectCollection = null;

            if (array == null)
            {
                return;
            }
            ExtendedDataEntity[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                ExtendedDataEntity extendedDataEntity = array2[i];

                dynamicObjectCollection = extendedDataEntity.DataEntity[propertyName] as DynamicObjectCollection;

                if (dynamicObjectCollection == null || dynamicObjectCollection.Count <= 0)
                {
                    continue;
                }
                long orgId = Convert.ToInt64(extendedDataEntity[propertyName3]);
                string text3 = "";
                foreach (DynamicObject item4 in dynamicObjectCollection)
                {
                    text3 = ((!(item4[text] is DynamicObject dynamicObject6)) ? "" : Convert.ToString(dynamicObject6["Id"]));
                    if (item4["MaterialID"] is DynamicObject dynamicObject7)
                    {
                        Convert.ToString(dynamicObject7["Id"]);
                    }

                    if (!text3.IsNullOrEmptyOrWhiteSpace())
                    {
                        string[] array3 = text3.Split('&');

                        text3 = array3[0] + "&" + array3[1] + "&" + orgId;
                        if (array3.Length >= 4)
                        {
                            for (int j = 3; j < array3.Length; j++)
                            {
                                text3 = text3 + "&" + array3[j];
                            }
                        }
                        item4[text + "_Id"] = text3;
                    }
                }
            }
        }
    }
}
