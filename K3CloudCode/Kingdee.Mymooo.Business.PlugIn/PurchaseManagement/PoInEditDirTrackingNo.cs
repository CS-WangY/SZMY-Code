using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using System.Security.Cryptography;
using System.Linq;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Log;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.BOS.Core.Bill;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.Web.Bill;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    [Description("采购入库订单-打开修改直发物流单号列表插件"), HotUpdate]
    public class PoInEditDirTrackingNo : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
      
            if (e.BarItemKey.EqualsIgnoreCase("PENY_tbEditDirTrackingNo"))
            {
                //选择的行,获取所有信息,放在listcoll里面
                ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;
                //获取所选行的主键,赋值给一个数组listKey
                //接收返回的数组值
                string[] listKey = listcoll.GetPrimaryKeyValues();

                DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
                formParameter.FormId = "PENY_PoInEditDirTrackingNo";
                formParameter.CustomComplexParams["FId"] = listKey[0];
                this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
                {
                    this.ListView.Refresh();
                }));
            }

        }
    }


    [Description("修改直发物流单号列表插件"), HotUpdate]
    public class PoInEditDirTrackingNoList : AbstractDynamicFormPlugIn
    {
        /// <summary>
        /// 初始化绑定
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var FId = this.View.OpenParameter.GetCustomParameter("FId");
            var view = FormMetadataUtils.CreateBillView(this.Context, "STK_InStock", FId);
            this.Model.SetValue("FDirNo", view.Model.GetValue("FDirNo"));
            this.Model.SetValue("FOldDirTrackingNumber", view.Model.GetValue("FDirTrackingNumber"));
            this.View.UpdateView("FDirNo");
            this.View.UpdateView("FOldDirTrackingNumber");
            //清除释放网控
            view.CommitNetworkCtrl();
            view.InvokeFormOperation(FormOperationEnum.Close);
            view.Close();
        }

        //按钮点击事件（查询）
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            if (e.Key.ToUpper().Equals("FOK"))
            {
                var dirNo = Convert.ToString(this.View.Model.GetValue("FDirNo"));
                var dirTrackingNumber = Convert.ToString(this.View.Model.GetValue("FDirTrackingNumber"));
                if (string.IsNullOrEmpty(dirNo))
                {
                    this.View.ShowErrMessage("只有直发申请的单据才能修改！");
                    return;
                }
                if (string.IsNullOrEmpty(dirTrackingNumber))
                {
                    this.View.ShowErrMessage("新直发物流单号不能为空！");
                    return;
                }
                var upSql = $@"/*dialect*/UPDATE t_STK_InStock SET FDirTrackingNumber='{dirTrackingNumber}' WHERE FDirNo='{dirNo}' ";
                DBServiceHelper.Execute(this.Context, upSql);
                upSql = $@"/*dialect*/UPDATE T_SAL_DELIVERYNOTICE SET FTrackingNumber='{dirTrackingNumber}' WHERE FDirNo='{dirNo}' ";
                DBServiceHelper.Execute(this.Context, upSql);
                upSql = $@"/*dialect*/UPDATE T_SAL_OUTSTOCK SET FTrackingNumber='{dirTrackingNumber}' WHERE FDirNo='{dirNo}' ";
                DBServiceHelper.Execute(this.Context, upSql);

                this.View.Close();
            }
        }

    }
}
