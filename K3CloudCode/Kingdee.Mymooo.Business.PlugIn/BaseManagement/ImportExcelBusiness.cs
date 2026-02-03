using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.JSON;
using Kingdee.BOS.ServiceHelper.Excel;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using System.Text.RegularExpressions;
using Kingdee.BOS.WebApi.FormService;
using Kingdee.BOS.App.Data;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.ThirdSystem.MessageLog;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("导入EXCEL文件，创建BOM动态表单"), HotUpdate]
    public class ImportExcelBusiness : AbstractDynamicFormPlugIn
    {
        public string materialNumber;
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            materialNumber = this.View.OpenParameter.GetCustomParameter("MaterialNumber").ToString();
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            if (e.Key.EqualsIgnoreCase("FImportData"))
            {
                Impoort();
                this.View.Close();
            }
        }
        public string _filePath;
        public override void CustomEvents(CustomEventsArgs e)
        {
            base.CustomEvents(e);
            if (e.Key.EqualsIgnoreCase("FFileUpdate"))
            {
                if (e.EventName.EqualsIgnoreCase("FILECHANGED"))
                {
                    JObject jsonObj = JsonConvert.DeserializeObject<JObject>(e.EventArgs);
                    if (jsonObj != null)
                    {
                        var jArray = jsonObj["NewValue"];
                        string _fileName = jArray[0]["ServerFileName"].ToString();
                        if (CheckFile(_fileName))
                        {
                            _filePath = GetFilePath(_fileName);
                            EnableButton("FImportData", true);
                        }
                        else
                        {
                            EnableButton("FImportData", false);
                        }
                    }
                    else
                    {
                        EnableButton("FImportData", false);
                    }
                }
                else if (e.EventName.EqualsIgnoreCase("STATECHANGED"))
                {
                    JObject jsonObj = JsonConvert.DeserializeObject<JObject>(e.EventArgs);
                    if (jsonObj["State"].ToString() != "2")
                    {
                        EnableButton("FImportData", false);
                    }
                }
            }
        }
        public bool CheckFile(string fname)
        {
            var array = fname.Split('.');
            if (array.Contains("xls", StringComparer.OrdinalIgnoreCase) || array.Contains("xlsx", StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            //if (array.Length == 2 && array[1] == "xls" || array[1] == "xlsx")
            //{
            //    return true;
            //}
            else
            {
                this.View.ShowWarnningMessage("请选择正确的文件进行引入");
                return false;
            }
        }
        public string GetFilePath(string serverFileName)
        {
            string directory = "FileUpLoadServices\\UploadFiles";
            return PathUtils.GetPhysicalPath(directory, serverFileName);
        }
        public void EnableButton(string key, Boolean bEnable)
        {
            this.View.GetControl(key).Enabled = bEnable;
            this.View.StyleManager.SetVisible(key, null, bEnable);
        }
        public void Impoort()
        {
            ExcelOperation excelOperation = new ExcelOperation();
            DataSet datas = excelOperation.ReadFromFile(_filePath, 0, 0);

            if (datas == null) { this.View.ShowWarnningMessage("请选择正确的文件进行引入"); }
            List<BomImportEntity> list = new List<BomImportEntity>();
            for (int i = 0; i < datas.Tables[0].Rows.Count; i++)
            {
                BomImportEntity entity = new BomImportEntity();
                if (datas.Tables[0].Rows[i]["Column2"].ToString() == "" || datas.Tables[0].Rows[i]["Column2"].ToString() == "物料号") continue;
                if (datas.Tables[0].Rows[i]["Column11"].ToString() == "")
                {
                    entity.FParentID = materialNumber;
                }
                else
                {
                    var pnumber = datas.Tables[0].Rows[i]["Column11"].ToString().Trim().Replace("\r", "")?.Replace("\n", "")?.Replace("\t", "")?.Replace(" ", "");
                    entity.FParentID = pnumber;
                }
                var number = datas.Tables[0].Rows[i]["Column2"].ToString().Trim().Replace("\r", "")?.Replace("\n", "")?.Replace("\t", "")?.Replace(" ", "");
                if (HasChinese(number))
                {
                    throw new Exception("物料编码不允许中文!");
                }
                if (number == materialNumber)
                {
                    throw new Exception("父级嵌套检查错误，子项中不要出现父项物料，请检查!");
                }
                entity.FNumber = number;
                var name = datas.Tables[0].Rows[i]["Column1"].ToString().Trim().Replace("\r", "")?.Replace("\n", "")?.Replace("\t", "")?.Replace(" ", "");
                entity.FName = name;
                entity.FCount = datas.Tables[0].Rows[i]["Column3"].ToString();
                entity.FDENOMINATOR = Convert.ToDecimal(datas.Tables[0].Rows[i]["Column4"]);
                entity.FBaseUnitId = datas.Tables[0].Rows[i]["Column5"].ToString();
                entity.ProductSmallClassName = datas.Tables[0].Rows[i]["Column8"].ToString();
                list.Add(entity);
            }
            List<long> materials = new List<long>();
            foreach (var item in list)
            {
                string code = item.FNumber;
                string name = item.FName;

                var scid = GetMaterialSmallIDByName(this.Context, item.ProductSmallClassName);
                SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
                if (scid != null)
                {
                    productsmallclass.Id = Convert.ToInt64(scid["FID"]);
                }

                string baseUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
                string stockUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
                string purchaseUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
                string saleUnit = item.FBaseUnitId == "PCS" ? "Pcs" : item.FBaseUnitId;
                if (item.FBaseUnitId.EqualsIgnoreCase("mm"))
                {
                    baseUnit = "m";
                    stockUnit = "mm";
                    purchaseUnit = "mm";
                    saleUnit = "mm";
                }

                var material = new MaterialInfo(code, name);
                material.FBaseUnitId = baseUnit;
                material.FStoreUnitID = stockUnit;
                material.FPurchaseUnitId = purchaseUnit;
                material.FPurchasePriceUnitId = purchaseUnit;
                material.FSaleUnitId = saleUnit;
                material.ProductSmallClass = productsmallclass;
                //如果是导入BOM来源的物料
                material.IsBom = true;

                var results = MaterialServiceHelper.TryGetOrAdd(this.Context, material, new List<long>() { this.Context.CurrentOrganizationInfo.ID });
                if (results != null)
                {
                    item.material = results;
                    materials.Add(results.MasterId);
                }
            }
            MaterialServiceHelper.MaterialAllocateToAll(this.Context, materials);
            ENGBomInfo[] reqbom = null;
            if (list.Where(x => x.material == null).ToArray().Count() == 0)
            {
                List<ENGBomInfo> boms = new List<ENGBomInfo>();
                GetBOMInfos(list, materialNumber, boms);
                if (boms.Count() <= 0)
                {
                    throw new Exception($"当前导入上级[{materialNumber}]不存在任何匹配的下级资料，请检查!");
                }
                reqbom = ENGBomServiceHelper.TryGetOrAdds(this.Context, boms.ToArray());
                //分配BOM
                ENGBomServiceHelper.SendMQAllocate(this.Context, reqbom.ToList<ENGBomInfo>());
            }
            if (this.View.ParentFormView.GetFormId().EqualsIgnoreCase("PENY_SaleBOMManagement"))
            {
                this.View.ReturnToParentWindow(reqbom.Where(x => x.Name == materialNumber).Select(x => x).First());
            }
            if (this.View.ParentFormView.GetFormId().EqualsIgnoreCase("BD_MATERIAL"))
            {
                this.View.ReturnToParentWindow(reqbom.Where(x => x.Name == materialNumber).Select(x => x).First());
            }
        }

        public DynamicObject GetMaterialSmallIDByName(Context ctx, string sname)
        {
            string sql = $@"SELECT t1.FID,t1.FNUMBER,t2.FNAME from T_BD_MATERIALGROUP t1
                            INNER JOIN T_BD_MATERIALGROUP_L t2 on t1.FID=t2.FID
                            WHERE t2.FNAME='{sname}'";
            var data = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (data.Count > 0)
            {
                return data[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 判断字符串中是否包含中文
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>判断结果</returns>
        public bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public List<ENGBomInfo> GetBOMInfos(List<BomImportEntity> bomentity, string parent, List<ENGBomInfo> bomlist)
        {
            ENGBomInfo bom = new ENGBomInfo(parent);
            bom.FMATERIALID = parent;
            var dataLines = bomentity.Where(o => o.FParentID.Equals(parent)).ToList();
            if (dataLines.Count > 0)
            {
                List<BomEntity> entitylist = new List<BomEntity>();
                foreach (var item in dataLines)
                {
                    BomEntity ent = new BomEntity();
                    //DynamicObject materialId = item["FMATERIALID"] as DynamicObject;

                    ent.FMATERIALIDCHILD = item.material.Code;
                    ent.FUnitNumber = item.FStockUnitId;
                    ent.FNUMERATOR = Convert.ToDecimal(item.FCount);
                    ent.FDENOMINATOR = Convert.ToDecimal(item.FDENOMINATOR);
                    GetBOMInfos(bomentity, item.material.Code, bomlist);
                    entitylist.Add(ent);
                }
                bom.Entity = entitylist;
                bomlist.Add(bom);
            }
            return bomlist;
        }
    }
}
