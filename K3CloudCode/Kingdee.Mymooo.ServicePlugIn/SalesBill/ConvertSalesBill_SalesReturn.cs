using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单下推销售退货拆分退补行"), HotUpdate]
    public class ConvertSalesBill_SalesReturn : AbstractConvertPlugIn
    {
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            base.OnAfterCreateLink(e);
            // 目标单单据体元数据
            Entity entity = e.TargetBusinessInfo.GetEntity("FSaleOrderEntry");
            // 读取已经生成的发货通知单
            ExtendedDataEntity[] bills = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");
            // 定义一个集合，存储新拆分出来的单据体行
            List<ExtendedDataEntity> newRows = new List<ExtendedDataEntity>();
            // 对目标单据进行循环
            foreach (var bill in bills)
            {
                // 取单据体集合
                DynamicObjectCollection rowObjs = entity.DynamicProperty.GetValue(bill.DataEntity)
                as DynamicObjectCollection;
                // 对单据体进行循环：从后往前循环，新拆分的行，避开循环
                int rowCount = rowObjs.Count;
                int newRowCount = 1;
                for (int i = rowCount - 1; i >= 0; i--)
                {
                    DynamicObject rowObj = rowObjs[i];
                    DynamicObject newRowObj = (DynamicObject)rowObj.Clone(false, true);
                    newRowObj["ReturnType"] = "RETURN";

                    newRowObj["Qty"] = Math.Abs(Convert.ToDecimal(newRowObj["Qty"])) * -1;
                    newRowObj["BaseUnitQty"] = Math.Abs(Convert.ToDecimal(newRowObj["BaseUnitQty"])) * -1;
                    //FOLDQTY
                    newRowObj["StockQty"] = Math.Abs(Convert.ToDecimal(newRowObj["StockQty"])) * -1;
                    newRowObj["StockBaseQty"] = Math.Abs(Convert.ToDecimal(newRowObj["StockBaseQty"])) * -1;
                    newRowObj["ALLAMOUNTEXCEPTDISCOUNT"] = Math.Abs(Convert.ToDecimal(newRowObj["ALLAMOUNTEXCEPTDISCOUNT"])) * -1;

                    newRowObj["PriceUnitQty"] = Math.Abs(Convert.ToDecimal(newRowObj["PriceUnitQty"])) * -1;
                    newRowObj["PriceBaseQty"] = Math.Abs(Convert.ToDecimal(newRowObj["PriceBaseQty"])) * -1;
                    newRowObj["TaxAmount"] = Math.Abs(Convert.ToDecimal(newRowObj["TaxAmount"])) * -1;
                    newRowObj["TaxAmount_LC"] = Math.Abs(Convert.ToDecimal(newRowObj["TaxAmount_LC"])) * -1;
                    newRowObj["AllAmount"] = Math.Abs(Convert.ToDecimal(newRowObj["AllAmount"])) * -1;
                    newRowObj["AllAmount_LC"] = Math.Abs(Convert.ToDecimal(newRowObj["AllAmount_LC"])) * -1;
                    newRowObj["Amount"] = Math.Abs(Convert.ToDecimal(newRowObj["Amount"])) * -1;
                    newRowObj["Amount_LC"] = Math.Abs(Convert.ToDecimal(newRowObj["Amount_LC"])) * -1;

                    newRowObj["DeliveryMaxQty"] = Math.Abs(Convert.ToDecimal(newRowObj["DeliveryMaxQty"])) * -1;
                    newRowObj["BaseDeliveryMaxQty"] = Math.Abs(Convert.ToDecimal(newRowObj["BaseDeliveryMaxQty"])) * -1;
                    newRowObj["DeliveryMinQty"] = Math.Abs(Convert.ToDecimal(newRowObj["DeliveryMinQty"])) * -1;
                    newRowObj["BaseDeliveryMinQty"] = Math.Abs(Convert.ToDecimal(newRowObj["BaseDeliveryMinQty"])) * -1;

                    newRowObj["CanOutQty"] = 0;
                    newRowObj["BaseCanOutQty"] = 0;
                    newRowObj["StockBaseCanOutQty"] = 0;
                    // 把新行，插入到单据中，排在当前行之后
                    rowObjs.Insert(i + 1, newRowObj);

                    // 为新行创建一个ExtendedDataEntity对象，表单服务策略需要此对象
                    ExtendedDataEntity newRow = new ExtendedDataEntity(
                    newRowObj, bill.DataEntityIndex, rowCount + newRowCount);
                    newRows.Add(newRow);
                    newRowCount++;
                }
                newRowCount = 1;
                foreach (var item in rowObjs)
                {
                    item["Seq"] = newRowCount;
                    newRowCount++;
                }
            }
            // 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
            e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());
        }
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            if (e.Result == null)
                return;
            string formId = e.TargetBusinessInfo.GetForm().Id;
            var headEntity = e.TargetBusinessInfo.Entrys.FirstOrDefault(x => x != null && x.ElementType == 34);
            if (headEntity == null)
                return;
            var targetBillResults = e.Result.FindByEntityKey(headEntity.Key);
            for (int i = 0, sz = targetBillResults.Length; i < sz; ++i)
            {
                var tgtBillResult = targetBillResults[i];
                if (tgtBillResult.DataEntity == null)
                {
                    continue;
                }
                CreateBillViewModelData(this.Context, formId, tgtBillResult.DataEntity);
            }
        }

        #region 创建视图

        /// <summary>
        /// 构造视图创建参数
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private static BillOpenParameter CreateOpenParameter(Context ctx, FormMetadata metadata)
        {
            var form = metadata.BusinessInfo.GetForm();
            BillOpenParameter openPara = new BillOpenParameter(form.Id, metadata.GetLayoutInfo().Id);
            openPara.Context = ctx;
            openPara.PageId = Guid.NewGuid().ToString();
            //openPara.Status = OperationStatus.VIEW;
            //openPara.CreateFrom = CreateFrom.Default;
            openPara.DefaultBillTypeId = string.Empty;
            openPara.PkValue = null;
            openPara.FormMetaData = metadata;
            openPara.SetCustomParameter(Kingdee.BOS.Core.FormConst.PlugIns, form.CreateFormPlugIns());
            openPara.ServiceName = form.FormServiceName;
            return openPara;
        }

        /// <summary>
        /// 创建按照表单视图逻辑加载的数据包（触发表单层的新建模型数据相关事件）
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId"></param>
        /// <param name="pkValue"></param>
        /// <returns></returns>
        private static bool CreateBillViewModelData(Context ctx, string formId, DynamicObject billObj)
        {
            FormMetadata metadata = MetaDataServiceHelper.Load(ctx, formId) as FormMetadata;
            if (metadata == null)
                return false;

            var parentView = CreateView(ctx, metadata, null);
            var billView = CreateView(ctx, metadata, (openParameter) =>
            {
                //下推的表单层处理逻辑
                openParameter.ParentPageId = parentView.PageId;
                openParameter.Status = OperationStatus.ADDNEW;
                openParameter.CreateFrom = CreateFrom.Push;
                openParameter.SetCustomParameter(FormConst.ConvParamKey_OneResultSessionKey, FormConst.ConvParamKey_OneResultSessionName);
                //openParameter.SetCustomParameter("_ConvertSessionKey", "ConverOneResult");
            });
            billView.ParentFormView = parentView;
            parentView.Session[FormConst.ConvParamKey_OneResultSessionName] = billObj;
            try
            {
                Kingdee.BOS.Log.Logger.Info("CreateNewModelData Start", string.Empty);
                billView.CreateNewModelData();
                Kingdee.BOS.Log.Logger.Info("CreateNewModelData End", string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                //todo 表单插件的异常需要处理
                Kingdee.BOS.Log.Logger.Error("Custom", "单据转换触发表单插件", ex);
            }
            return false;
        }

        private static IBillView CreateView(Context ctx, FormMetadata metadata, Action<BillOpenParameter> opAct)
        {
            var openParameter = CreateOpenParameter(ctx, metadata);
            if (opAct != null)
            {
                opAct(openParameter);
            }
            var provider = metadata.BusinessInfo.GetForm().GetFormServiceProvider();
            string importViewClass = "Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web";
            Type type = Type.GetType(importViewClass);
            IDynamicFormViewService billViewService = (IDynamicFormViewService)Activator.CreateInstance(type);
            IBillView billView = billViewService as IBillView;
            billViewService.Initialize(openParameter, provider);
            return billView;

        }
        #endregion
    }
}
