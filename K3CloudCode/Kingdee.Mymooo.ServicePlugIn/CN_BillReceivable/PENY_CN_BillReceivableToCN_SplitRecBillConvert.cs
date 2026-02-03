using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.ServicePlugIn.CN_BillReceivable
{
    [Description("应收票据- 应收票据拆分单据转换插件"), HotUpdate]
    public class PENY_CN_BillReceivableToCN_SplitRecBillConvert : AbstractConvertPlugIn
    {
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            //IL_003d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0042: Unknown result type (might be due to invalid IL or missing references)
            //((AbstractConvertPlugIn)this).OnAfterCreateLink(e);
            ExtendedDataEntity[] array = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");
            Entity entity = e.TargetBusinessInfo.GetEntity("FSplitRecBillEntity");
            ExtendedDataEntity[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                ExtendedDataEntity val = array2[i];
                object obj = val.DataEntity["FSplitRecBillEntity"];
                DynamicObjectCollection val2 = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
                if (val2 == null || ((Collection<DynamicObject>)val2).Count <= 0)
                {
                    continue;
                }
                int rownumber = 0;
                foreach (DynamicObject item in (Collection<DynamicObject>)val2)
                {
                    item["FESubbillRange"] = (Convert.ToString(item["FESubbillStartNo"]) + "-" + Convert.ToString(item["FESubbillEndNo"]));
                    rownumber++;
                    //item["FERCBILLNUMBER"] = (Convert.ToString(val.DataEntity["BILLNUMBER"]) + "-" + Convert.ToString(item["FESubbillRange"]));
                    if (Convert.ToString(val.DataEntity["FRCBILLNUMBER"]).Contains("-1-"))
                    {
                        item["FERCBILLNUMBER"] = (Convert.ToString(val.DataEntity["BILLNUMBER"]) + "-" + Convert.ToString(rownumber));
                    }
                    else
                    {
                        item["FERCBILLNUMBER"] = (Convert.ToString(val.DataEntity["FRCBILLNUMBER"]) + "-" + Convert.ToString(rownumber));
                    }
                }
                object obj2 = entity.DynamicObjectType.CreateInstance();
                DynamicObject val3 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
                if (val3 != null)
                {
                    rownumber = val2.Count + 1;
                    val3["seq"] = rownumber;
                    if (Convert.ToDecimal(val.DataEntity["FStandardAmt"]) > 0m)
                    {
                        val3["FAmount"] = Convert.ToDecimal(val.DataEntity["FStandardAmt"]);
                    }
                    else
                    {
                        val3["FAmount"] = 1;
                    }
                    val3["FESUBBILLSTARTNO"] = Convert.ToInt64(val.DataEntity["FSubbillEndNo"]);
                    val3["FESubbillEndNo"] = Convert.ToInt64(val.DataEntity["FSubbillEndNo"]);
                    val3["FESubbillnumber"] = 1;
                    val3["FESubbillRange"] = (Convert.ToString(val3["FESUBBILLSTARTNO"]) + "-" + Convert.ToString(val3["FESubbillEndNo"]));
                    //val3["FERCBILLNUMBER"] = (Convert.ToString(val.DataEntity["BILLNUMBER"]) + "-" + Convert.ToString(val3["FESubbillRange"]));
                    if (Convert.ToString(val.DataEntity["FRCBILLNUMBER"]).Contains("-1-"))
                    {
                        val3["FERCBILLNUMBER"] = (Convert.ToString(val.DataEntity["BILLNUMBER"]) + "-" + Convert.ToString(rownumber));
                    }
                    else
                    {
                        val3["FERCBILLNUMBER"] = (Convert.ToString(val.DataEntity["FRCBILLNUMBER"]) + "-" + Convert.ToString(rownumber));
                    }
                    
                    ((Collection<DynamicObject>)val2).Add(val3);
                }
            }
        }
    }
}
