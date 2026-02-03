using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Utils
{
    /// <summary>
    /// 元数据
    /// </summary>
    public class FormMetadataUtils
    {
        /// <summary>
        /// 创建一个单据视图，后续将利用此视图的各种方法，设置物料字段值
        /// </summary>
        /// <remarks>
        /// 理论上，也可以直接修改物料的数据包达成修改数据的目的
        /// 但是，利用单据视图更具有优势：
        /// 1. 视图会自动触发插件，这样逻辑更加完整；
        /// 2. 视图会自动利用单据元数据，填写字段默认值，不用担心字段值不符合逻辑；
        /// 3. 字段改动，会触发实体服务规则；
        /// 
        /// 而手工修改数据包的方式，所有的字段值均需要自行填写，非常麻烦
        /// </remarks>
        public static IBillView CreateBillView(Context ctx, string FormId, object id = null)
        {
            //"STK_InStock"
            // 读取物料的元数据
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, FormId) as FormMetadata;
            Form form = meta.BusinessInfo.GetForm();
            // 创建用于引入数据的单据view
            Type type = Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web");
            var billView = (IDynamicFormViewService)Activator.CreateInstance(type);
            // 开始初始化billView：
            // 创建视图加载参数对象，指定各种参数，如FormId, 视图(LayoutId)等
            BillOpenParameter openParam = CreateOpenParameter(ctx, meta, id);
            // 动态领域模型服务提供类，通过此类，构建MVC实例
            var provider = form.GetFormServiceProvider();
            billView.Initialize(openParam, provider);
            ((IBillViewService)billView).LoadData();
            DynamicFormViewPlugInProxy eventProxy = (billView as IBillView).GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();
            return billView as IBillView;
        }

        /// <summary>
        /// 创建视图加载参数对象，指定各种初始化视图时，需要指定的属性
        /// </summary>
        /// <param name="meta">元数据</param>
        /// <returns>视图加载参数对象</returns>
        public static BillOpenParameter CreateOpenParameter(Context ctx, FormMetadata meta, object id)
        {
            Form form = meta.BusinessInfo.GetForm();
            // 指定FormId, LayoutId
            BillOpenParameter openParam = new BillOpenParameter(form.Id, meta.GetLayoutInfo().Id);
            // 数据库上下文
            openParam.Context = ctx;
            // 本单据模型使用的MVC框架
            openParam.ServiceName = form.FormServiceName;
            // 随机产生一个不重复的PageId，作为视图的标识
            openParam.PageId = Guid.NewGuid().ToString();
            // 元数据
            openParam.FormMetaData = meta;
            if (id == null)
            {
                // 界面状态：新增 (修改、查看)
                openParam.Status = OperationStatus.ADDNEW;
                // 单据主键：本案例演示新建物料，不需要设置主键
                openParam.PkValue = null;
            }
            else
            {
                // 界面状态：新增 (修改、查看)
                openParam.Status = OperationStatus.EDIT;
                openParam.PkValue = id;
            }
            // 界面创建目的：普通无特殊目的 （为工作流、为下推、为复制等）
            openParam.CreateFrom = CreateFrom.Default;
            // 基础资料分组维度：基础资料允许添加多个分组字段，每个分组字段会有一个分组维度
            // 具体分组维度Id，请参阅 form.FormGroups 属性
            openParam.GroupId = "";
            // 基础资料分组：如果需要为新建的基础资料指定所在分组，请设置此属性
            openParam.ParentId = 0;
            // 单据类型
            openParam.DefaultBillTypeId = "";
            // 业务流程
            openParam.DefaultBusinessFlowId = "";
            // 主业务组织改变时，不用弹出提示界面
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            // 插件
            List<AbstractDynamicFormPlugIn> plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(ctx, openParam);
            foreach (var plug in plugs)
            {
                // 触发插件PreOpenForm事件，供插件确认是否允许打开界面
                plug.PreOpenForm(args);
            }
            if (args.Cancel == true)
            {
                // 插件不允许打开界面
                // 本案例不理会插件的诉求，继续....
            }
            // 返回
            return openParam;
        }

        public static void SaveBill(Context ctx, IBillView billView)
        {
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            var oper = BusinessDataServiceHelper.Save(ctx, billView.BillBusinessInfo, billView.Model.DataObject, operateOption, "Save");

            if (!oper.IsSuccess)
            {
                if (oper.ValidationErrors.Count > 0)
                {
                    throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }
        }

        /// <summary>
        /// 根据编码获取Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static T GetIdForNumber<T>(Context ctx, T info) where T : BaseInfo
        {
            QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
            queryParam.FormId = info.FormId;
            queryParam.SelectItems = SelectorItemInfo.CreateItems(string.Format("{0} as {0}", info.IdFieldNmber));
            if (!string.IsNullOrWhiteSpace(info.DocumentStatusFieldName))
            {
                queryParam.SelectItems.Add(new SelectorItemInfo(info.DocumentStatusFieldName));
            }
            if (!string.IsNullOrWhiteSpace(info.ForbidStatusFieldName))
            {
                queryParam.SelectItems.Add(new SelectorItemInfo(info.ForbidStatusFieldName));
            }
            if (!string.IsNullOrWhiteSpace(info.MasterFieldName))
            {
                queryParam.SelectItems.Add(new SelectorItemInfo(info.MasterFieldName));
            }
            queryParam.FilterClauseWihtKey = string.Format(" {0} = @FNumber", info.NumberFieldName);
            List<SqlParam> paras = new List<SqlParam>();
            paras.Add(new SqlParam("@FNumber", info.NumberKDDbType, info.Code));
            if (info.UseOrgId > 0)
            {
                queryParam.FilterClauseWihtKey += " and " + info.OrgNumberFieldName + "= @FUseOrgId";
                paras.Add(new SqlParam("@FUseOrgId", KDDbType.Int64, info.UseOrgId));
            }
            var datas = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryParam, paras);
            if (datas.Count > 0)
            {
                info.Id = Convert.ToInt64(datas[0][info.IdFieldNmber]);
                if (!string.IsNullOrWhiteSpace(info.DocumentStatusFieldName))
                {
                    info.DocumentStatus = Convert.ToString(datas[0][info.DocumentStatusFieldName]);
                }
                if (!string.IsNullOrWhiteSpace(info.ForbidStatusFieldName))
                {
                    info.ForbidStatus = Convert.ToString(datas[0][info.ForbidStatusFieldName]);
                }
                if (!string.IsNullOrWhiteSpace(info.MasterFieldName))
                {
                    info.MasterId = Convert.ToInt64(datas[0][info.MasterFieldName]);
                }
            }
            return info;
        }
    }
}
