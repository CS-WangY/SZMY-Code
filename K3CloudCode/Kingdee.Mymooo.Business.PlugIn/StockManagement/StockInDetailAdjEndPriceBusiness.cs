using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Model.ReportFilter;
using Kingdee.BOS.Serialization;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Newtonsoft.Json;
using System.Data;
using static Kingdee.K3.Core.BD.Enums;
using System.Drawing;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.BOS.App.Core.Convertible.UnitConvert;
using Kingdee.BOS.Core.Validation;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.BD;
using NPOI.SS.Formula.Functions;
using Kingdee.K3.MFG.App;
namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    [Description("存货收发存明细期末单价插件"), HotUpdate]
    public class StockInDetailAdjEndPriceBusiness : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            //if (e.Field.Key.EqualsIgnoreCase("FMaterialId"))
            //{
            //    long materialId = Convert.ToInt64(e.NewValue);
            //    var orgId = Convert.ToInt64((this.View.Model.GetValue("FStockOrgId") as DynamicObject)["id"]);
            //    var lastMonth = DateTime.Now.AddMonths(-1);
            //    long destUnitId = 0;
            //    if (this.View.Model.GetValue("FUNITID") != null)
            //    {
            //        destUnitId = Convert.ToInt64((this.View.Model.GetValue("FUNITID") as DynamicObject)["id"]);
            //    }
            //    else if (this.View.Model.GetValue("FBaseUnitId") != null)
            //    {
            //        destUnitId = Convert.ToInt64((this.View.Model.GetValue("FBaseUnitId") as DynamicObject)["id"]);
            //    }
            //    var endPrice = GetEndPrice(this.Context, GetAcctgOrgID(this.Context, orgId), lastMonth.Year, lastMonth.Month, materialId, destUnitId);
            //    this.View.Model.SetValue("FEstimatePrice", endPrice, e.Row);
            //}
            if (e.Field.Key.EqualsIgnoreCase("FUNITID"))
            {
                long destUnitId = Convert.ToInt64(e.NewValue);
                if (this.View.Model.GetValue("FMaterialId") != null)
                {
                    long materialId = Convert.ToInt64((this.View.Model.GetValue("FMaterialId") as DynamicObject)["id"]);
                    var orgId = Convert.ToInt64((this.View.Model.GetValue("FStockOrgId") as DynamicObject)["id"]);
                    var lastMonth = DateTime.Now.AddMonths(-1);
                    var endPrice = GetEndPrice(this.Context, GetAcctgOrgID(this.Context, orgId), lastMonth.Year, lastMonth.Month, materialId, destUnitId);
                    this.View.Model.SetValue("FEstimatePrice", endPrice, e.Row);
                }
            }

        }
        //获取存货收发存明细期末单价
        public decimal GetEndPrice(Context ctx, long acctgOrgID, int year, int period, long materialId, long destUnitId)
        {
            var filterMetadata = FormMetaDataCache.GetCachedFilterMetaData(ctx);//加载字段比较条件元数据。
            var reportMetadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "HS_INOUTSTOCKDETAILRPT");//加载报表
            var reportFilterMetadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "HS_INOUTSTOCKDETAILFILTER");//加载报表过滤条件元数据。
            var reportFilterServiceProvider = reportFilterMetadata.BusinessInfo.GetForm().GetFormServiceProvider();
            var model = new SysReportFilterModel();
            model.SetContext(ctx, reportFilterMetadata.BusinessInfo, reportFilterServiceProvider);
            model.FormId = reportFilterMetadata.BusinessInfo.GetForm().Id;
            model.FilterObject.FilterMetaData = filterMetadata;
            model.InitFieldList(reportMetadata, reportFilterMetadata);
            model.GetSchemeList();
            //查询默认过滤方案id
            string Sql = "SELECT fschemeid FROM T_BAS_FILTERSCHEME where fformid='HS_INOUTSTOCKDETAILRPT' and fschemename='Default Scheme'";
            string fschemeid = "";
            using (IDataReader reader = DBServiceHelper.ExecuteReader(ctx, Sql))
            {
                if (reader.Read())
                {
                    fschemeid = reader["fschemeid"].ToString();
                }
            }
            var entity = model.Load(fschemeid);

            //赋值过滤方案条件
            var filterParam = model.GetFilterParameter();

            //开始物料
            FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "HS_INOUTSTOCKDETAILFILTER") as FormMetadata;
            BusinessInfo businessInfo = formMetadata.BusinessInfo;
            BaseDataField bdfMaterial = businessInfo.GetField("FMATERIALID") as BaseDataField;
            QueryBuilderParemeter qbpMaterial = new QueryBuilderParemeter();
            qbpMaterial.FormId = "BD_MATERIAL";
            qbpMaterial.SelectItems = SelectorItemInfo.CreateItems("FMaterialId");
            qbpMaterial.FilterClauseWihtKey = $"FMATERIALID={materialId} ";
            var material_ck = BusinessDataServiceHelper.Load(ctx, bdfMaterial.RefFormDynamicObjectType, qbpMaterial)[0];

            BaseDataField bdfEndMaterial = businessInfo.GetField("FENDMATERIALID") as BaseDataField;
            QueryBuilderParemeter qbpEndMaterial = new QueryBuilderParemeter();
            qbpEndMaterial.FormId = "BD_MATERIAL";
            qbpEndMaterial.SelectItems = SelectorItemInfo.CreateItems("FMaterialId");
            qbpEndMaterial.FilterClauseWihtKey = $"FMATERIALID={materialId} ";
            var endMaterial_ck = BusinessDataServiceHelper.Load(ctx, bdfEndMaterial.RefFormDynamicObjectType, qbpEndMaterial)[0];

            //会计核算体系
            BaseDataField bdfAccount = businessInfo.GetField("FACCTGSYSTEMID") as BaseDataField;
            QueryBuilderParemeter qbpAccount = new QueryBuilderParemeter();
            qbpAccount.FormId = "Org_AccountSystem";
            qbpAccount.SelectItems = SelectorItemInfo.CreateItems("FACCTSYSTEMID");
            qbpAccount.FilterClauseWihtKey = $"FACCTSYSTEMID=1 ";
            var account_ck = BusinessDataServiceHelper.Load(ctx, bdfAccount.RefFormDynamicObjectType, qbpAccount)[0];

            //核算组织
            BaseDataField bdfAcctgorg = businessInfo.GetField("FACCTGORGID") as BaseDataField;
            QueryBuilderParemeter qbpAcctgOrg = new QueryBuilderParemeter();
            qbpAcctgOrg.FormId = "ORG_Organizations";
            qbpAcctgOrg.SelectItems = SelectorItemInfo.CreateItems("FORGID");
            qbpAcctgOrg.FilterClauseWihtKey = $"FORGID={acctgOrgID} ";
            var acctgOrg_ck = BusinessDataServiceHelper.Load(ctx, bdfAcctgorg.RefFormDynamicObjectType, qbpAcctgOrg)[0];

            //会计政策
            BaseDataField bdfAcctpolicy = businessInfo.GetField("FACCTPOLICYID") as BaseDataField;
            QueryBuilderParemeter qbpAcctpolicy = new QueryBuilderParemeter();
            qbpAcctpolicy.FormId = "BD_ACCTPOLICY";
            qbpAcctpolicy.SelectItems = SelectorItemInfo.CreateItems("FACCTPOLICYID");
            qbpAcctpolicy.FilterClauseWihtKey = $"FACCTPOLICYID=1 ";
            var acctpolicy_ck = BusinessDataServiceHelper.Load(ctx, bdfAcctpolicy.RefFormDynamicObjectType, qbpAcctpolicy)[0];

            filterParam.CustomFilter["MATERIALID_Id"] = materialId;
            filterParam.CustomFilter["MATERIALID"] = material_ck;
            filterParam.CustomFilter["ENDMATERIALID_Id"] = materialId;
            filterParam.CustomFilter["ENDMATERIALID"] = endMaterial_ck;
            filterParam.CustomFilter["ACCTGSYSTEMID_Id"] = 1;
            filterParam.CustomFilter["ACCTGSYSTEMID"] = account_ck;
            filterParam.CustomFilter["Year"] = year;
            filterParam.CustomFilter["Period"] = period;
            filterParam.CustomFilter["ENDYEAR"] = year;
            filterParam.CustomFilter["EndPeriod"] = period;
            filterParam.CustomFilter["ACCTGORGID_Id"] = acctgOrgID;
            filterParam.CustomFilter["ACCTGORGID"] = acctgOrg_ck;
            filterParam.CustomFilter["ACCTPOLICYID_Id"] = 1;
            filterParam.CustomFilter["ACCTPOLICYID"] = acctpolicy_ck;
            filterParam.CustomFilter["FDimType"] = 0;
            filterParam.CustomFilter["PagBasis"] = 2;
            IRptParams p = new RptParams();
            p.FormId = reportFilterMetadata.BusinessInfo.GetForm().Id;
            //StartRow和EndRow是报表数据分页的起始行数和截至行数，一般取所有数据，所以EndRow取int最大值。
            p.StartRow = 1;
            p.EndRow = int.MaxValue;
            p.FilterParameter = filterParam;
            p.FilterFieldInfo = model.FilterFieldInfo;
            p.CustomParams.Add("OpenParameter", "");//此参数不能缺少，即使为空也要保留

            MoveReportServiceParameter param = new MoveReportServiceParameter(ctx, reportMetadata.BusinessInfo, Guid.NewGuid().ToString(), p);
            var rpt = SysReportServiceHelper.GetListAndReportData(param);//简单账表使用GetReportData方法
            decimal endPrice = 0;
            if (rpt.DataSource != null)
            {
                foreach (DataRow dr in rpt.DataSource.Rows)
                {
                    if (Convert.ToString(dr["FBUSINESSTYPE"]).Equals("期末结存"))
                    {
                        endPrice = Convert.ToDecimal(dr["FEndPrice"]);
                        long unitId = GetUnitId(ctx, Convert.ToString(dr["funitname"]));
                        long masterId = Convert.ToInt64(dr["FMASTERID"]);
                        if (destUnitId != unitId)
                        {
                            GetUnitConvertRateArgs val6 = new GetUnitConvertRateArgs();
                            val6.MasterId = (masterId);
                            val6.SourceUnitId = (unitId);
                            val6.DestUnitId = (destUnitId);
                            UnitConvert unitConvertRate = AppServiceContext.BDService.GetUnitConvertRate(this.Context, val6);
                            if (unitConvertRate != null)
                            {
                                //米转换毫米
                                if (unitConvertRate.ConvertNumerator > 1)
                                {
                                    endPrice = endPrice / unitConvertRate.ConvertNumerator;
                                }
                                else if (unitConvertRate.ConvertDenominator > 1)
                                {
                                    //毫米转换米
                                    endPrice = endPrice * unitConvertRate.ConvertNumerator;
                                }
                            }
                        }
                    }
                }
            }
            return endPrice;
        }

        /// <summary>
        /// 获取所属法人
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public long GetAcctgOrgID(Context ctx, long orgId)
        {
            List<SqlParam> pars = new List<SqlParam>() {
                new SqlParam("@OrgId", KDDbType.Int64, orgId) };
            string sql = $"select top 1 FParentID from T_ORG_ORGANIZATIONS where FORGID=@OrgId";
            return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }

        /// <summary>
        /// 获取单位ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public long GetUnitId(Context ctx, string unitName)
        {
            List<SqlParam> pars = new List<SqlParam>() {
                new SqlParam("@UnitName", KDDbType.String, unitName) };
            string sql = $@"select top 1 t1.FUNITID from T_BD_UNIT t1
                            inner join T_BD_UNIT_L t2 on t1.FUNITID=t2.FUNITID and t2.FLOCALEID=2052
                            where t2.FNAME=@UnitName and t1.FDOCUMENTSTATUS='C'";
            return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }
    }
}
