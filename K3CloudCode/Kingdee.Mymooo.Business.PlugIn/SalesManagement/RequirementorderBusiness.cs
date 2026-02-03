using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args.WizardForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售订单获取BOM插件"), HotUpdate]
    public class RequirementorderBusiness : AbstractListPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            if (e.BarItemKey.Equals("PENY_GetBom", StringComparison.OrdinalIgnoreCase))
            {
                var selectedRows = this.ListView.SelectedRowsInfo;

                if (selectedRows == null || selectedRows.Count == 0)
                {
                    this.View.ShowMessage("当前没有行被选中！");
                    return;
                }
                else
                {
                    var fid = selectedRows[0].PrimaryKeyValue;
                    var entryid = selectedRows[0].EntryPrimaryKeyValue;
                    DynamicObjectDataRow datarow = selectedRows[0].DataRow as DynamicObjectDataRow;
                    //var materialid = datarow.DynamicObject["FMaterialId_Id"].ToString();
                    var material = datarow.DynamicObject["FMaterialId_Ref"] as DynamicObject;
                    string materialNumber = material["Number"].ToString();
                    long productid = (Int64)material["FProductId"];
                    long msterid = (Int64)material["msterID"];
                    long supplyTargetOrgid = (Int64)datarow["FSupplyTargetOrgId_Id"];
                    long saleOrgid = (Int64)datarow["FSaleOrgId_Id"];

                    DynamicFormShowParameter param = new DynamicFormShowParameter();
                    param.Resizable = false;
                    param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
                    param.FormId = "PENY_GetBom";
                    param.CustomComplexParams.Add("MaterialNumber", materialNumber);
                    param.CustomComplexParams.Add("MaterialName", material["Name"].ToString());
                    param.CustomComplexParams.Add("ProductId", productid);
                    param.CustomComplexParams.Add("SupplyTargetOrgid", supplyTargetOrgid);
                    param.CustomComplexParams.Add("SaleOrgid", saleOrgid);
                    param.CustomComplexParams.Add("MsterId", msterid);
                    this.View.ShowForm(param, new Action<FormResult>((result) =>
                    {
                        if (result.ReturnData != null)
                        {
                            ENGBomInfo resdata = result.ReturnData as ENGBomInfo;
                            //this.Model.SetValue("FComboTextCode", resdata["FCode"].ToString(), rowIndex);
                            //this.Model.SetValue("FComboText", resdata["FName"].ToString(), rowIndex);
                            var sSql = $@"update T_SAL_ORDERENTRY set FBOMID={resdata.Id} where FID={fid} and FEntryID={entryid}";
                            DBServiceHelper.Execute(this.Context, sSql);
                        }
                    }));
                }
            }
        }
    }

    [Description("获取BOM动态表单插件"), HotUpdate]
    public class RequirementorderWizardPlugIn : AbstractWizardFormPlugIn
    {
        public string materialNumber { get; set; }
        public string materialName { get; set; }
        public long productId { get; set; }
        public long supplyTargetOrgid { get; set; }
        public long saleOrgid { get; set; }
        public long msterid { get; set; }
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.View.GetControl("FBOMSave").Visible = false;
            materialNumber = this.View.OpenParameter.GetCustomParameter("MaterialNumber").ToString();
            materialName = this.View.OpenParameter.GetCustomParameter("MaterialName").ToString();
            productId = (long)this.View.OpenParameter.GetCustomParameter("ProductId");
            //saleOrgid = (long)this.View.OpenParameter.GetCustomParameter("SaleOrgid");
            //msterid = (long)this.View.OpenParameter.GetCustomParameter("MsterId");
            //supplyTargetOrgid = (long)this.View.OpenParameter.GetCustomParameter("SupplyTargetOrgid");

            var usercode = "";
            //todo 获取当前登录用户绑定员工信息
            var userinfo = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, this.Context.UserId);
            //var userinfo = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, 6559423);
            if (userinfo != null)
            {
                usercode = userinfo.FWECHATCODE;
            }
            else
            {
                //this.View.ShowErrMessage("", "当前用户未绑定员工信息,无法获取相关信息!", MessageBoxType.Error);
                this.View.GetControl("FNext").Enabled = false;
                this.View.GetControl("FFinish").Enabled = false;
                this.View.ShowWarnningMessage("当前用户未绑定员工信息,无法获取相关信息!");
            }

            if (productId == 0)
            {
                string url = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/BomList/Query";
                var pairs = new
                {
                    PageIndex = 1,
                    PageSize = 10000,
                    Filter = new
                    {
                        TemplateCode = "",
                        TemplateName = "",
                        Version = ""
                    }
                };
                string spairs = JsonConvertUtils.SerializeObject(pairs);
                BomTemplateMaster bomTemplate = new BomTemplateMaster();
                BomApiRequest request = JsonConvertUtils.DeserializeObject<BomApiRequest>(ApigatewayUtils.InvokePostWebService(url, spairs));
                if (request.code == "success")
                {
                    bomTemplate = JsonConvertUtils.DeserializeObject<BomTemplateMaster>(request.data.ToString());
                }
                else
                {
                    this.View.ShowErrMessage(request.errorMessage);
                }
                for (int i = 0; i < bomTemplate.rows.Count; i++)
                {
                    var item = bomTemplate.rows[i];
                    this.Model.CreateNewEntryRow("FTemplateEntity");
                    this.Model.SetValue("FTemplateID", item.Id, i);
                    this.Model.SetValue("FTemplateCode", item.TemplateCode, i);
                    this.Model.SetValue("FTemplateName", item.TemplateName, i);
                    this.Model.SetValue("FVersion", item.Version, i);
                }
                this.View.GetControl("FTemplateID").Visible = false;
                this.View.GetControl("FTemplateID").Enabled = false;
                this.View.GetControl("FTemplateCode").Enabled = false;
                this.View.GetControl("FTemplateName").Enabled = false;
                this.View.GetControl("FVersion").Enabled = false;
                this.View.UpdateView("FTemplateEntity");
            }
            else
            {
                this.View.JumpToWizardStep("FWizard2", true);
            }

        }

        /// <summary>
        /// 操作步骤切换前事件
        /// </summary>
        /// <param name="e"></param>
        public override void WizardStepChanging(WizardStepChangingEventArgs e)
        {
            base.WizardStepChanging(e);
            if (e.UpDownEnum == 1)
            {
                //第一页
                //if (e.OldWizardStep.Key.Equals("FWizard0", StringComparison.OrdinalIgnoreCase))
                //{
                //    // 跳转
                //    //    e.JumpWizardStepKey = "FWizard2";
                //}
            }
            else if (e.UpDownEnum == 2)
            {
                // 上一步
                // TODO
                this.View.GetControl("FBOMSave").Visible = false;
                this.View.GetControl("FFINISH").Visible = true;
            }
        }
        /// <summary>
        /// 操作步骤切换后事件
        /// </summary>
        /// <param name="e"></param>
        public override void WizardStepChanged(WizardStepChangedEventArgs e)
        {
            base.WizardStepChanged(e);
            if (e.WizardStep.Key.Equals("FWizard1", StringComparison.OrdinalIgnoreCase))
            {
                this.Model.DeleteEntryData("FParameterEntity");
                // 下一步
                int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FTemplateEntity");
                DynamicObject selectedEntityObj = (this.View.Model.DataObject["FTemplateEntity"] as DynamicObjectCollection)[rowIndex];
                int templateID = Convert.ToInt32(selectedEntityObj["FTemplateID"]);
                if (selectedEntityObj == null)
                {
                    this.View.ShowWarnningMessage("请选择模板！");
                }
                //this.View.ShowMessage(msg);
                //获取模板参数
                string url = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/BomList/GetBomTemplateParameter";
                var pairs = new
                {
                    id = templateID
                };

                List<BomTemplateParameterEntity> bomTemplate = new List<BomTemplateParameterEntity>();
                BomApiRequest request = JsonConvertUtils.DeserializeObject<BomApiRequest>(ApigatewayUtils.InvokePostWebService(url, JsonConvertUtils.SerializeObject(pairs)));
                if (request.code == "success")
                {
                    bomTemplate = JsonConvertUtils.DeserializeObject<List<BomTemplateParameterEntity>>(request.data.ToString());
                }
                else
                {
                    this.View.ShowErrMessage(request.errorMessage);
                }

                for (int i = 0; i < bomTemplate.Count; i++)
                {
                    var template = bomTemplate[i];
                    this.Model.CreateNewEntryRow("FParameterEntity");
                    this.Model.SetValue("FParamCode", template.ParamCode, i);
                    this.Model.SetValue("FParamName", template.ParamName, i);
                    this.Model.SetValue("FType", template.Type, i);

                    DynamicObject currenrow = (this.View.Model.DataObject["FParameterEntity"] as DynamicObjectCollection)[i];
                    switch (template.Type)
                    {
                        case "numerical":
                            this.Model.SetValue("FDecimal", 0, i);
                            break;
                        case "enumeration":
                            this.Model.SetValue("FCOMBOTEXTCODE", template.Scope.First().Code, i);
                            this.Model.SetValue("FComboText", template.Scope.First().Name, i);
                            this.Model.SetValue("FScopeText", JsonConvertUtils.SerializeObject(template.Scope), i);
                            break;
                    }
                }

                this.View.GetControl("FParamCode").Enabled = false;
                this.View.GetControl("FParamName").Enabled = false;
                this.View.GetControl("FType").Enabled = false;
                this.View.GetControl("FType").Visible = false;
                this.View.UpdateView("FParameterEntity");
                //设置参数类型编辑状态
                int rowcount = this.Model.GetEntryRowCount("FParameterEntity");
                for (int i = 0; i < rowcount; i++)
                {
                    switch (this.View.Model.GetValue("FType", i))
                    {
                        case "numerical":
                            this.View.GetFieldEditor("FComboText", i).SetEnabled("", false);
                            break;
                        case "enumeration":
                            this.View.GetFieldEditor("FDecimal", i).SetEnabled("", false);
                            break;
                    }
                }
            }
            if (e.WizardStep.Key.Equals("FWizard2", StringComparison.OrdinalIgnoreCase))
            {
                this.Model.DeleteEntryData("FBomMaterialEntity");
                BomMaterialEntity bomTemplate = new BomMaterialEntity();
                if (productId > 0)
                {
                    //根据产品ID获取BOM
                    string url = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/ProductList/GetProductParameterValue";

                    var pairs = new
                    {
                        productId = productId,
                        number = materialNumber
                    };

                    var redata = ApigatewayUtils.InvokePostWebService(url, JsonConvertUtils.SerializeObject(pairs));
                    BomApiRequest request = JsonConvertUtils.DeserializeObject<BomApiRequest>(redata);
                    if (request.code == "success")
                    {
                        bomTemplate = JsonConvertUtils.DeserializeObject<BomMaterialEntity>(request.data.ToString());
                        this.View.GetControl("FPrevious").Enabled = false;
                        this.View.GetControl("FFINISH").Visible = false;
                        this.View.GetControl("FBOMSave").Visible = true;
                    }
                    else
                    {
                        bomTemplate = null;
                        this.View.ShowErrMessage(request.errorMessage);
                        this.View.GetControl("FPrevious").Enabled = false;
                        this.View.GetControl("FFINISH").Visible = true;
                        this.View.GetControl("FBOMSave").Visible = false;
                    }
                }
                else
                {
                    int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FTemplateEntity");
                    DynamicObject selectedEntityObj = (this.View.Model.DataObject["FTemplateEntity"] as DynamicObjectCollection)[rowIndex];
                    int templateID = Convert.ToInt32(selectedEntityObj["FTemplateID"]);
                    //根据参数获取BOM
                    string url = $"srm/{ApigatewayUtils.ApigatewayConfig.EnvCode}/BomList/TransformBomTemplateParameter";

                    List<object> list = new List<object>();
                    int rowcount = this.Model.GetEntryRowCount("FParameterEntity");
                    for (int i = 0; i < rowcount; i++)
                    {
                        string sVal = "";
                        switch (this.View.Model.GetValue("FType", i))
                        {
                            case "numerical":
                                sVal = this.View.Model.GetValue("FDecimal", i).ToString();
                                break;
                            case "enumeration":
                                sVal = this.View.Model.GetValue("FComboTextCode", i).ToString();
                                break;
                        }
                        var obj = new
                        {
                            Code = this.View.Model.GetValue("FParamCode", i),
                            Value = sVal
                        };
                        list.Add(obj);
                    }
                    var pairs = new
                    {
                        TemplateId = templateID,
                        ModelNumber = materialNumber,
                        Parameters = list
                    };

                    BomApiRequest request = JsonConvertUtils.DeserializeObject<BomApiRequest>(ApigatewayUtils.InvokePostWebService(url, JsonConvertUtils.SerializeObject(pairs)));
                    if (request.code == "success")
                    {
                        bomTemplate = JsonConvertUtils.DeserializeObject<BomMaterialEntity>(request.data.ToString());
                        this.View.GetControl("FFINISH").Visible = false;
                        this.View.GetControl("FBOMSave").Visible = true;
                    }
                    else
                    {
                        bomTemplate = null;
                        this.View.ShowErrMessage(request.errorMessage);
                        this.View.GetControl("FFINISH").Visible = true;
                        this.View.GetControl("FBOMSave").Visible = false;
                    }
                }
                if (!(bomTemplate is null))
                {
                    this.View.Model.SetValue("FProductCode", materialNumber);
                    this.View.Model.SetValue("FProductName", materialName);
                    this.View.Model.SetValue("FHeadAutoCraft", bomTemplate?.AutoCraft);
                    this.View.Model.SetValue("FISSUETYPE", bomTemplate?.IsSueType);
                    this.View.Model.SetValue("FErpClsID", bomTemplate?.ErpClsID);
                    LoadBomList(bomTemplate, materialNumber);
                    this.View.UpdateView("FBillHead");
                    this.View.UpdateView("FBomMaterialEntity");
                }
            }

        }

        public void LoadBomList(BomMaterialEntity bomtemplate, string FParentID)
        {
            foreach (var item in bomtemplate.materials)
            {
                this.Model.CreateNewEntryRow("FBomMaterialEntity");
                var rowIndex = this.Model.GetEntryRowCount("FBomMaterialEntity") - 1;
                this.Model.SetValue("FParentID", FParentID, rowIndex);
                this.Model.SetValue("FCode", item.Code, rowIndex);
                this.Model.SetValue("FName", item.Name, rowIndex);

                var Uom = "";
                //decimal count = 0;
                if (item.Uom == "mm")
                {
                    Uom = "m";
                    //count = item.Count / Convert.ToDecimal(1000);
                }
                else
                {
                    Uom = item.Uom;
                    //count = item.Count;
                }
                this.Model.SetValue("FCount", item.Count, rowIndex);

                this.Model.SetItemValueByNumber("FBaseUnitID", Uom, rowIndex);
                this.Model.SetValue("FlossRate", item.LossRate, rowIndex);
                this.Model.SetValue("FautoCraft", item.AutoCraft, rowIndex);
                this.Model.SetValue("FSendMes", item.SendMes, rowIndex);
                this.Model.SetValue("FTexture", item.Texture, rowIndex);

                this.View.Model.SetValue("FISSUETYPE", item.IsSueType, rowIndex);
                this.View.Model.SetValue("FErpClsID", item.ErpClsID, rowIndex);

                this.Model.SetValue("FUom", item.Uom, rowIndex);
                this.Model.SetValue("FPoUom", item.PoUom, rowIndex);
                this.Model.SetValue("FSoUom", item.SoUom, rowIndex);
                this.Model.SetValue("FProductId", item.ProductId, rowIndex);
                this.Model.SetItemValueByID("FclassId", item.ClassId, rowIndex);
                this.Model.SetItemValueByID("Fsmallclassid", item.SmallClassId, rowIndex);
                if (item.materials != null)
                {
                    LoadBomList(item, item.Code);
                }
            }
        }

        /// <summary>
        /// 点击完成按钮后，触发窗体关闭事件
        /// </summary>
        /// <param name="e"></param>
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            if (e.Key.Equals("FFINISH", StringComparison.OrdinalIgnoreCase))
            {
                // 点击完成按钮
            }
            if (e.Key.Equals("FBOMSave", StringComparison.OrdinalIgnoreCase))
            {
                //点击导入按钮
                bool flag = true;

                //MaterialService materialService = new MaterialService();
                EntryGrid entryGrid = this.View.GetControl<EntryGrid>("FBomMaterialEntity");
                DynamicObjectCollection bomEntity = this.Model.DataObject["FBomMaterialEntity"] as DynamicObjectCollection;
                int rowcount = bomEntity.Count;

                if (bomEntity.Where(x => Convert.ToBoolean(x["FErpClsID"]) == true).Count() > 0)
                {
                    //更新主物料为委外属性
                    UpdateMaterial();
                }
                //新增物料
                for (int i = 0; i < rowcount; i++)
                {
                    var materialid = this.View.Model.GetValue("FMATERIALID", i);
                    if (!(materialid is null))
                    {
                        continue;
                    }

                    string code = this.View.Model.GetValue("FCode", i).ToString();
                    string name = this.View.Model.GetValue("FName", i).ToString();

                    string baseUnit = ((DynamicObject)this.View.Model.GetValue("FBaseUnitID", i))["Number"].ToString();
                    string storeUnit = this.View.Model.GetValue("FUom", i).ToString() == "PCS" ? "Pcs" : this.View.Model.GetValue("FUom", i).ToString();
                    string purchaseUnit = this.View.Model.GetValue("FPoUom", i).ToString() == "PCS" ? "Pcs" : this.View.Model.GetValue("FPoUom", i).ToString();
                    string saleUnit = this.View.Model.GetValue("FSoUom", i).ToString() == "PCS" ? "Pcs" : this.View.Model.GetValue("FSoUom", i).ToString();

                    int productID = Convert.ToInt32(this.View.Model.GetValue("FProductId", i));
                    SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();

                    productsmallclass.Id = Convert.ToInt64(((DynamicObject)this.View.Model.GetValue("Fsmallclassid", i))["Id"]);
                    //productsmallclass.ParentId = Convert.ToInt64(((DynamicObject)this.View.Model.GetValue("FclassId", i))["Id"]);

                    var material = new MaterialInfo(code, name);
                    material.FBaseUnitId = baseUnit;
                    material.FStoreUnitID = storeUnit;
                    material.FPurchaseUnitId = purchaseUnit;
                    material.FPurchasePriceUnitId = purchaseUnit;
                    material.FSaleUnitId = saleUnit;
                    material.ProductId = productID;
                    material.ProductSmallClass = productsmallclass;
                    material.Textures = this.View.Model.GetValue("FTexture", i)?.ToString();
                    if (Convert.ToBoolean(this.View.Model.GetValue("FErpClsID", i)))
                    {
                        material.ErpClsID = 3;
                    }
                    else
                    {
                        material.ErpClsID = 1;
                    }

                    var results = MaterialServiceHelper.TryBomGetOrAdd(this.Context, material);
                    if (results != null)
                    {
                        this.Model.SetValue("FResults", "OK", i);
                        this.Model.SetItemValueByID("FMATERIALID", results.Id, i);
                        this.Model.SetItemValueByNumber("FBaseUnitID", results.FBaseUnitId, i);
                        this.Model.SetItemValueByID("FclassId", results.ProductSmallClass?.ParentId, i);
                        this.Model.SetItemValueByID("Fsmallclassid", results.ProductSmallClass?.Id, i);
                        entryGrid.SetForecolor("FResults", "#00A600", i);
                        this.View.UpdateView("FBomMaterialEntity", i);
                    }
                    else
                    {
                        flag = false;
                        this.Model.SetValue("FResults", "物料错误", i);
                        entryGrid.SetForecolor("FResults", "#FF2525", i);
                    }
                }
                for (int i = 0; i < rowcount; i++)
                {
                    var smallclassid = this.View.Model.GetValue("Fsmallclassid", i);
                    if (smallclassid is null)
                    {
                        throw new Exception("第" + (i + 1) + "行,物料大小类为必填项,请检查!");
                    }
                }

                //新增BOM
                List<ENGBomInfo> boms = new List<ENGBomInfo>();
                GetBOMInfos(bomEntity, materialNumber, Convert.ToBoolean(this.View.Model.GetValue("FHeadAutoCraft")), boms);
                //ENGBomService bomService = new ENGBomService();
                var bominfo = ENGBomServiceHelper.TryGetOrAdds(this.Context, boms.ToArray());
                //分配BOM
                ENGBomServiceHelper.SendMQAllocate(this.Context, bominfo.ToList<ENGBomInfo>());

                if (flag)
                {
                    this.View.GetControl("FFINISH").Visible = true;
                    this.View.GetControl("FBOMSave").Visible = false;
                    this.View.ReturnToParentWindow(bominfo.Where(x => x.Name == materialNumber).Select(x => x).First());
                }
            }
        }
        public List<ENGBomInfo> GetBOMInfos(DynamicObjectCollection bomentity, string parent, bool autoCraft, List<ENGBomInfo> bomlist)
        {
            ENGBomInfo bom = new ENGBomInfo(parent);
            bom.FMATERIALID = parent;
            bom.AutoCraft = autoCraft;
            var dataLines = bomentity.Where(o => o["FParentID"].ToString().Equals(parent)).ToList();
            if (dataLines.Count > 0)
            {
                List<BomEntity> entitylist = new List<BomEntity>();
                foreach (var item in dataLines)
                {
                    BomEntity ent = new BomEntity();
                    DynamicObject materialId = item["FMATERIALID"] as DynamicObject;
                    //DynamicObjectCollection materialbase = materialId["MaterialBase"] as DynamicObjectCollection;
                    //DynamicObject materialunit = materialbase[0]["BaseUnitId"] as DynamicObject;

                    ent.FMATERIALIDCHILD = materialId["Number"].ToString();
                    ent.FNUMERATOR = Convert.ToDecimal(item["FCount"]);
                    ent.FDENOMINATOR = 1;
                    ent.FUnitNumber = Convert.ToString(item["FUom"]) == "PCS" ? "Pcs" : Convert.ToString(item["FUom"]);
                    ent.FSCRAPRATE = Convert.ToDecimal(item["FlossRate"]);
                    ent.SendMes = Convert.ToBoolean(item["FSendMes"]);
                    ent.IsSueType = Convert.ToBoolean(item["FIsSueType"]);
                    ent.ErpClsID = Convert.ToBoolean(item["FErpClsID"]);

                    GetBOMInfos(bomentity, materialId["Number"].ToString(), Convert.ToBoolean(item["FautoCraft"]), bomlist);
                    entitylist.Add(ent);
                }
                bom.Entity = entitylist;
                bomlist.Add(bom);
            }
            return bomlist;
        }
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
        }

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            if (e.FieldKey.EqualsIgnoreCase("FComboText"))
            {
                int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FParameterEntity");
                DynamicObject selectedEntityObj = (this.View.Model.DataObject["FParameterEntity"] as DynamicObjectCollection)[rowIndex];

                DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
                formParameter.FormId = "PENY_BOMParameter";
                //FID通过字符串传递过去
                formParameter.CustomParams.Add("ScopeText", selectedEntityObj["FScopeText"].ToString());
                this.View.ShowForm(formParameter, new Action<FormResult>((result) =>
                {
                    if (result.ReturnData != null)
                    {
                        DynamicObject resdata = result.ReturnData as DynamicObject;
                        this.Model.SetValue("FComboTextCode", resdata["FCode"].ToString(), rowIndex);
                        this.Model.SetValue("FComboText", resdata["FName"].ToString(), rowIndex);
                    }
                }));
            }
        }
        protected void UpdateMaterial()
        {
            long[] orgs = new long[] { 1, supplyTargetOrgid };
            //更新物料属性为委外
            var sSql = $@"/*dialect*/UPDATE T_BD_MATERIALBASE SET FERPCLSID=3 FROM T_BD_MATERIALBASE t1 INNER JOIN T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
WHERE t2.FMASTERID={msterid} AND t1.FUSEORGID IN ({string.Join(",", orgs)})";
            DBServiceHelper.Execute(this.Context, sSql);
        }
    }

    [Description("获取BOM参数表单插件"), HotUpdate]
    public class RequirementorderParameter : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            string spara = this.Model.OpenParameter.GetCustomParameter("ScopeText").ToString();
            var data = JArray.Parse(spara);


            foreach (JToken item in data)
            {
                int rowcount = this.View.Model.GetEntryRowCount("FEntity");
                this.View.Model.CreateNewEntryRow("FEntity");

                this.View.Model.SetValue("FCode", Convert.ToString(item["code"]), rowcount);
                this.View.Model.SetValue("FName", Convert.ToString(item["name"]), rowcount);
            }
            this.View.GetControl("FCode").Enabled = false;
            this.View.GetControl("FName").Enabled = false;
            this.View.UpdateView("FEntity");
        }
        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            int rowIndex = this.View.Model.GetEntryCurrentRowIndex("FEntity");
            DynamicObject selectedEntityObj = (this.View.Model.DataObject["FEntity"] as DynamicObjectCollection)[rowIndex];
            this.View.ReturnToParentWindow(selectedEntityObj);
            this.View.Close();
        }
    }
}
