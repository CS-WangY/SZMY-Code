using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.Business.PlugIn.PLN_Forecast
{
    [Description("预测单获取物料价格表单插件"), HotUpdate]
    public class PLN_ForecastBillPlugIn : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.EqualsIgnoreCase("FMaterialID"))
            {
                var customid = this.View.Model.GetValue("FCustID", e.Row) as DynamicObject;
                var material = this.View.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
                //var mapid = this.View.Model.GetValue("FMapId", e.Row) as DynamicObject;
                if (customid != null && material != null)
                {
                    var requestData = new QuotationInputRequest();
                    requestData.CompanyCode = Convert.ToString(customid["Number"]);
                    List<QuotationInput> datalist = new List<QuotationInput>();
                    datalist.Add(new QuotationInput()
                    {
                        //CustomCode = Convert.ToString(this.View.Model.GetValue("FCustItemNo", e.Row)),
                        //CustItemName = Convert.ToString(this.View.Model.GetValue("FCustItemName", e.Row)),
                        ProductId = Convert.ToInt64(material["FProductId"]),
                        ProductModel = Convert.ToString(material["Number"]),
                        //ProductName = Convert.ToString(material["Name"]),
                        Qty = Convert.ToDecimal(this.View.Model.GetValue("FQty", e.Row)),
                        //SmallId = Convert.ToInt64(material["MaterialGroup_Id"]),
                    });
                    requestData.Detail = datalist;
                    var requestJsonData = JsonConvertUtils.SerializeObject(requestData);
                    try
                    {
                        string url = $"platformAdmin/{ApigatewayUtils.ApigatewayConfig.EnvCode}/mallapi/ResolvedOrder/AnalyseProductPriceForKd";
                        var resultJson = ApigatewayUtils.InvokePostWebService(url, requestJsonData);
                        var result = JsonConvertUtils.DeserializeObject<QuotationResult>(resultJson);
                        if (result.isSuccess)
                        {
                            if (result.data != null)
                            {
                                if (result.data[0].unitPriceWithTax > 0)
                                {
                                    this.View.Model.SetValue("FPENYReferencePrice", result.data[0].unitPriceWithTax, e.Row);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            if (e.Field.Key.EqualsIgnoreCase("FPENYCustID"))
            {
                //var customid = this.View.Model.GetValue("FPENYCustID", e.Row) as DynamicObject;
                //if (customid != null)
                //{
                //    string sSql = $@"SELECT e.FNUMBER FROM T_SAL_SCSALERCUST sg
                //        inner join T_BD_OPERATORDETAILS g on sg.FSALERGROUPID = g.FOPERATORGROUPID
                //        inner join T_BD_OPERATORENTRY e on g.FENTRYID = e.FENTRYID and e.FOPERATORTYPE = 'XSY'
                //        inner join T_BD_STAFF staff on e.FSTAFFID = staff.FSTAFFID
                //        where sg.FCUSTOMERID={customid["Id"]}";
                //    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                //    if (datas.Count > 0)
                //    {
                //        this.Model.SetItemValueByNumber("FPENYSalerId", datas[0]["FNUMBER"].ToString(), 0);
                //        this.View.UpdateView("FPENYSalerId");
                //    }
                //}
            }
        }
    }

    public class QuotationInputRequest
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CompanyCode { get; set; }

        public List<QuotationInput> Detail { get; set; }
    }
    public class QuotationInput
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ProductModel { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
    }

    public class QuotationResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool isSuccess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object returnObject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<QuotationResultDataItem> data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string errorMessage { get; set; }
    }
    public class QuotationResultDataItem
    {
        public string smallClassName { get; set; }
        public string businessDivisionId { get; set; }
        public string businessDivisionName { get; set; }
        public string deliverySource { get; set; }
        public string priceSource { get; set; }
        public long productEngineerId { get; set; }
        public string productEngineerName { get; set; }
        public long productManagerId { get; set; }
        public string productManagerName { get; set; }
        public long supplyOrgId { get; set; }
        public string supplyOrgName { get; set; }
        public long productId { get; set; }
        public string productImage { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public decimal qty { get; set; }
        public bool isRelease { get; set; }
        public int dispatchDays { get; set; }
        public decimal unitPriceWithTax { get; set; }
        public decimal subTotalWithTax { get; set; }
        public long smallClassId { get; set; }
    }

    [Description("预测单列表按钮插件-测试"), HotUpdate]
    public class PLN_ForecastBillListPlugIn : AbstractListPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            switch (e.BarItemKey)
            {
                case "PENY_tbButton":
                    var id = 1225024;
                    var entryid = 1225024;

                    //var billView = FormMetadataUtils.CreateBillView(this.Context, "PUR_Requisition", 139100);
                    var billView = FormMetadataUtils.CreateBillView(this.Context, "PRD_MO", 103285);
                    //删除源关联关系
                    //billView.Model.DeleteEntryData("FEntity_Link");
                    billView.Model.DeleteEntryData("FTREEENTITY_Link");
                    //foreach (var item in billView.Model.DataObject["ReqEntry"] as DynamicObjectCollection)
                    foreach (var item in billView.Model.DataObject["TreeEntity"] as DynamicObjectCollection)
                    {

                        ////item["SrcBillTypeId"] = "PLN_PLANORDER";
                        ////item["SrcBillNo"] = "MRP01863553";
                        ////item["FSRCBILLID"] = entryid;
                        ////item["ReqQty"] = 4;

                        //billView.Model.CreateNewEntryRow("FEntity_Link");
                        //((DynamicObjectCollection)item["FEntity_Link"])[0]["SBILLID"] = id;
                        //((DynamicObjectCollection)item["FEntity_Link"])[0]["SID"] = entryid;
                        ////((DynamicObjectCollection)item["FSaleOrderEntry_Link"])[0]["ENTRYID"] = item["Id"];
                        //((DynamicObjectCollection)item["FEntity_Link"])[0]["RULEID"] = "PlanOrder_PurRequest";
                        //((DynamicObjectCollection)item["FEntity_Link"])[0]["STABLENAME"] = "T_PLN_PLANORDER";
                        //((DynamicObjectCollection)item["FSaleOrderEntry_Link"])[0]["FPENY_FO2SOQTY"] = 3;
                        //((DynamicObjectCollection)item["FSaleOrderEntry_Link"])[0]["FPENY_FO2SOQTYOLD"] = 0;

                        item["SrcBillType"] = "PLN_PLANORDER";
                        item["SrcBillNo"] = "MRP01863553";
                        item["SrcBILLID"] = entryid;
                        item["Qty"] = 20;

                        billView.Model.CreateNewEntryRow("FTREEENTITY_Link");
                        ((DynamicObjectCollection)item["FTREEENTITY_Link"])[0]["SBILLID"] = id;
                        ((DynamicObjectCollection)item["FTREEENTITY_Link"])[0]["SID"] = entryid;
                        //((DynamicObjectCollection)item["FSaleOrderEntry_Link"])[0]["ENTRYID"] = item["Id"];
                        ((DynamicObjectCollection)item["FTREEENTITY_Link"])[0]["RULEID"] = "PlanOrder_MO";
                        ((DynamicObjectCollection)item["FTREEENTITY_Link"])[0]["STABLENAME"] = "T_PLN_PLANORDER";
                    }
                    //billView.Model.DeleteEntryData("FSaleOrderEntry_Link");
                    var saveResult = BusinessDataServiceHelper.Save(this.Context, billView.BusinessInfo, billView.Model.DataObject);
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                    break;
            }
        }
    }
}
