using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter.ConditionVariableAnalysis;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Mq;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.CMCAjax;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.QM.ParamOption;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.PLN;
using Kingdee.K3.SCM.Core;
using Kingdee.K3.SCM.Core.KDBigData;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Kingdee.Mymooo.Business.PlugIn.SalDeliveryNotice
{
    [Description("发货通知单表单插件"), HotUpdate]
    public class SalDeliveryNoticeEditBusiness : AbstractBillPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            if (e.BarItemKey == "tbDraw")
            {
                //switch (this.View.OpenParameter.CreateFrom)
                //{
                //    case CreateFrom.Push:
                //        this.View.ShowErrMessage("由下推发货不允许继续选单！");
                //        e.Cancel = true;
                //        break;
                //}
            }

        }

        /// <summary>
        /// 值更新事件
        /// </summary>
        /// <param name="e"></param>
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key == "FIsShipInspectCust")
            {
                if (Convert.ToBoolean(this.Model.GetValue("FIsShipInspectCust")))
                {
                    
                    this.View.GetControl("FIsShipInspect").Enabled = false;
                }
            }
        }

        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);

        }
        /// <summary>
        /// 调用接口判断是否下推过
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="bInfo"></param>
        /// <param name="parentEntity">关联主实体</param>
        /// <param name="parentObj">关联主实体对应的主数据包</param>
        /// <returns></returns>
        public bool IsLinkByInterface(Context ctx, BusinessInfo bInfo, Entity parentEntity, DynamicObject parentObj)
        {
            IResourceServiceProvider provider = bInfo.GetForm().GetFormServiceProvider();
            IDynamicFormModelService modelService = provider.GetService(typeof(IDynamicFormModelService)) as IDynamicFormModelService;
            modelService.SetContext(ctx, bInfo, provider);
            IDynamicFormModel model = modelService as IDynamicFormModel;
            var dynamicRow = new BOSDynamicRow(parentObj, parentEntity.Key, model);
            var isPush = ConvertServiceHelper.IsPush(ctx, bInfo, dynamicRow);
            return isPush;
        }

        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            //选择收件信息
            if (e.Key.ToUpper().Equals("FPENYSELECTADDRESS"))
            {
                var status = this.View.Model.GetValue("FDocumentStatus");
                if (status.Equals("A") || status.Equals("Z") || status.Equals("D"))
                {
                    var serachFilter = this.View.Model.GetValue("FReceiverID");
                    if (serachFilter == null)
                    {
                        this.View.ShowErrMessage("请选择收货方");
                        return;
                    }
                    DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
                    formParameter.FormId = "PENY_DNSelectCustInfo";
                    formParameter.CustomParams["ReceiverID"] = ((DynamicObject)serachFilter)["Id"].ToString();
                    this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
                    {
                        if (result.ReturnData != null)
                        {
                            this.Model.SetValue("FReceiveAddress", ((DynamicObject)result.ReturnData)["FADDRESS"], 0);
                            this.Model.SetValue("FLinkMan", ((DynamicObject)result.ReturnData)["FRECEIVER"], 0);
                            this.Model.SetValue("FLinkPhone", ((DynamicObject)result.ReturnData)["FMOBILE"], 0);
                            this.View.UpdateView("FBillHead");
                        }
                    }));
                }

            }
        }
    }
}
