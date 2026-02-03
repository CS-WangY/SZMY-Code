using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("大小类单据修改物料时携带"), HotUpdate]
    public class MaterialParentSmall : AbstractBillPlugIn
    {
        //物料编码
        static public string[] materialString =
         {
                "FMaterialId",
                "FMaterialID_Sal",
                "FMaterialIDSETY"
         };
        //货主
        string[] ownerString =
         {
                "FOwnerId",
                "OwnerID",
                "FOwnerIDSETY",
                //"FSupplyOrgId",
                "FReceiveOrgId",
                "ReceiveOrgId",
                "FInStockOwnerId",
                "InStockOwnerId",
                "FSupplyTargetOrgId",
         };
        /// <summary>
        /// 值更新
        /// </summary>
        /// <param name="e"></param>
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (materialString.Contains(e.Field.Key, StringComparer.OrdinalIgnoreCase))
            {
                var material = this.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
                if (material == null)
                {
                    if (e.Field.OriginKey.EqualsIgnoreCase("FMaterialIDSETY"))
                    {
                        this.View.Model.SetValue("FSmallIdSETY", 0, e.Row);
                        this.View.Model.SetValue("FParentSmallIdSETY", 0, e.Row);
                        //this.View.Model.SetValue("FBusinessDivisionIdSETY", "", e.Row);
                    }
                    else
                    {
                        this.View.Model.SetValue("FSmallId", 0, e.Row);
                        this.View.Model.SetValue("FParentSmallId", 0, e.Row);
                        //this.View.Model.SetValue("FBusinessDivisionId", "", e.Row);
                    }

                }
                else
                {
                    var data = MaterialServiceHelper.GetMaterialSmallBusinessDivision(this.View.Context, Convert.ToInt64(material["Id"]));
                    if (e.Field.OriginKey.EqualsIgnoreCase("FMaterialIDSETY"))
                    {
                        this.View.Model.SetValue("FSmallIdSETY", data.SmallId, e.Row);
                        this.View.Model.SetValue("FParentSmallIdSETY", data.ParentSmallId, e.Row);
                        //this.View.Model.SetValue("FBusinessDivisionIdSETY", data.BusinessDivision, e.Row);
                    }
                    else
                    {
                        this.View.Model.SetValue("FSmallId", data.SmallId, e.Row);
                        this.View.Model.SetValue("FParentSmallId", data.ParentSmallId, e.Row);
                        //this.View.Model.SetValue("FBusinessDivisionId", data.BusinessDivision, e.Row);
                    }
                }
            }

            if (ownerString.Contains(e.Field.Key, StringComparer.OrdinalIgnoreCase))
            {
                Int64 ownerid = 0;
                switch (this.Model.BillBusinessInfo.GetForm().Id)
                {
                    case "PUR_ReceiveBill":
                        ownerid = Convert.ToInt64(this.View.Model.DataObject["StockOrgId_Id"]);
                        break;
                    default:
                        var owner = this.View.Model.GetValue(e.Field.Key, e.Row) as DynamicObject;
                        if (owner != null)
                        {
                            ownerid = Convert.ToInt64(owner["Id"]);
                        }
                        break;
                }

                if (ownerid == 0)
                {
                    if (e.Field.OriginKey.EqualsIgnoreCase("SETY"))
                    {
                        this.View.Model.SetValue("FBusinessDivisionIdSETY", "", e.Row);
                    }
                    else
                    {
                        this.View.Model.SetValue("FBusinessDivisionId", "", e.Row);
                    }
                }
                else
                {
                    var businessDivision = SupplyOrgServiceHelper.GetSupplyOrgBusinessDivision(this.View.Context, ownerid);

                    if (e.Field.OriginKey.IndexOf("SETY", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        this.View.Model.SetValue("FBusinessDivisionIdSETY", businessDivision, e.Row);
                    }
                    else
                    {
                        this.View.Model.SetValue("FBusinessDivisionId", businessDivision, e.Row);
                    }

                }
            }

        }

        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
            switch (this.Model.BillBusinessInfo.GetForm().Id)
            {
                case "PUR_Requisition":
                    this.View.RuleContainer.AddPluginRule("FEntity", RaiseEventType.ItemAdded, (ctx) =>
                    {
                        var executeContext = (BOSActionExecuteContext)ctx;
                        var currentEntryIndex = executeContext.View.Model.GetEntryCurrentRowIndex("FEntity");
                        setOrgID("FReceiveOrgId", currentEntryIndex);
                        //this.View.ShowMessage(string.Format("订单明细新增了一行，行号为：{0}", currentEntryIndex + 1));

                    }, "FReceiveOrgId");
                    break;
            }

        }
        private void setOrgID(string fieldkey, int rowIndex)
        {
            if (ownerString.Contains(fieldkey, StringComparer.OrdinalIgnoreCase))
            {
                Int64 ownerid = 0;
                switch (this.Model.BillBusinessInfo.GetForm().Id)
                {
                    case "PUR_ReceiveBill":
                        ownerid = Convert.ToInt64(this.View.Model.DataObject["StockOrgId_Id"]);
                        break;
                    default:
                        var owner = this.View.Model.GetValue(fieldkey, rowIndex) as DynamicObject;
                        if (owner != null)
                        {
                            ownerid = Convert.ToInt64(owner["Id"]);
                        }
                        break;
                }

                if (ownerid == 0)
                {
                    if (fieldkey.EqualsIgnoreCase("SETY"))
                    {
                        this.View.Model.SetValue("FBusinessDivisionIdSETY", "", rowIndex);
                    }
                    else
                    {
                        this.View.Model.SetValue("FBusinessDivisionId", "", rowIndex);
                    }
                }
                else
                {
                    var businessDivision = SupplyOrgServiceHelper.GetSupplyOrgBusinessDivision(this.View.Context, ownerid);

                    if (fieldkey.IndexOf("SETY", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        this.View.Model.SetValue("FBusinessDivisionIdSETY", businessDivision, rowIndex);
                    }
                    else
                    {
                        this.View.Model.SetValue("FBusinessDivisionId", businessDivision, rowIndex);
                    }

                }
            }
        }
        private void CallBack(DynamicObject arg1, dynamic arg2)
        {
            this.View.UpdateView("FReceiveOrgId");
        }

        public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
        {
            base.AfterCreateNewEntryRow(e);

        }
    }
}
