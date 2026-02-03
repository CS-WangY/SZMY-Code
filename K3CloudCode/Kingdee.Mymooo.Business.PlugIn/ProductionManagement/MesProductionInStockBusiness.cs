using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.Bill;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    /// <summary>
    /// MES生产入库
    /// </summary>
    public class MesProductionInStockBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            MesProductionInStockRequest request = JsonConvertUtils.DeserializeObject<MesProductionInStockRequest>(message);

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                if (string.IsNullOrWhiteSpace(request.InStockBillNo))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "生产入库单号不能为空！";
                    return response;
                }
                if (request.Details.Count == 0)
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "生产订单明细不能为空！";
                    return response;
                }
                if (IsExistsBillNo(ctx, request.InStockBillNo))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = $"生产入库单号[{request.InStockBillNo}]已存在！";
                    return response;
                }
                foreach (var items in request.Details)
                {
                    List<SqlParam> pars = new List<SqlParam>() {
                     new SqlParam("@FMOID", KDDbType.Int64, request.Id),
                     new SqlParam("@FMOENTRYID", KDDbType.Int64, items.EntryId) };

                    var sql = $@"/*dialect*/select t1.FPRDORGID,t1.FDOCUMENTSTATUS,t2.FQTY from T_PRD_MO t1
                        inner join T_PRD_MOENTRY t2 on t1.FID=t2.FID
                        where t1.Fid=@FMOID and t2.FENTRYID=@FMOENTRYID ";
                    var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                    if (datas.Count == 0)
                    {
                        response.Code = ResponseCode.ModelError;
                        response.Message = $"生产订单[{request.BillNo}-{items.BillSeq}]不存在！";
                        return response;
                    }
                    if (!datas[0]["FDOCUMENTSTATUS"].ToString().EqualsIgnoreCase("C"))
                    {
                        response.Code = ResponseCode.ModelError;
                        response.Message = $"生产订单[{request.BillNo}]不是已批核状态！";
                        return response;
                    }
                    //验证物料和数量是否足够
                    //if (items.MustQty > Convert.ToDecimal(datas[0]["FQTY"]))
                    //{
                    //    response.Code = ResponseCode.ModelError;
                    //    response.Message = $"生产订单[{request.BillNo + "-" + items.BillSeq}]的数量不足。";
                    //    return response;
                    //}
     
                }
                List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                foreach (var item in request.Details)
                {
                    selectedRows.Add(new ListSelectedRow(Convert.ToString(request.Id), Convert.ToString(item.EntryId), 0, "PRD_MO") { EntryEntityKey = "FTreeEntity" });
                }

                // 生产订单下推生产入库单
                var rules = ConvertServiceHelper.GetConvertRules(ctx, "PRD_MO", "PRD_INSTOCK");
                var rule = rules.FirstOrDefault(t => t.IsDefault);
                if (rule == null)
                {
                    throw new Exception("没有从生产订单下推生产入库单的转换关系");
                }

                PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                {
                    TargetBillTypeId = "de29f16214744c21b374044d629595f2", // 请设定目标单据单据类型
                    TargetOrgId = 0,            // 请设定目标单据主业务组织
                };
                //执行下推操作，并获取下推结果
                var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                if (operationResult.IsSuccess)
                {
                    var view = FormMetadataUtils.CreateBillView(ctx, "PRD_INSTOCK");
                    foreach (var item in operationResult.TargetDataEntities)
                    {
                        view.Model.DataObject = item.DataEntity;
                        view.Model.SetValue("FBillNo", request.InStockBillNo);
                        var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
                        var rowIndex = 0;
                        foreach (var entry in entrys)
                        {
                            var thisList = request.Details.Where(x => x.EntryId.Equals(Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]))).FirstOrDefault();
                            view.Model.SetValue("FMustQty", thisList.MustQty, rowIndex);
                            view.InvokeFieldUpdateService("FMustQty", rowIndex);
                            view.Model.SetItemValueByNumber("FStockId", thisList.StockCode, rowIndex);
                            view.InvokeFieldUpdateService("FStockId", rowIndex);
                            rowIndex++;
                        }
                    }
                    //保存批核
                    var opers = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
                    if (opers.IsSuccess)
                    {
                        //清除释放网控
                        view.CommitNetworkCtrl();
                        view.InvokeFormOperation(FormOperationEnum.Close);
                        view.Close();
                        response.Code = ResponseCode.Success;
                        response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
                    }
                    else
                    {
                        if (opers.ValidationErrors.Count > 0)
                        {
                            throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                        }
                        else
                        {
                            throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                        }
                    }
                }
                else
                {
                    if (operationResult.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }

            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 验证生产入库单是否存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgCode"></param>
        /// <returns></returns>
        private bool IsExistsBillNo(Context ctx, string BillNo)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@BillNo", KDDbType.String, BillNo) };
            var sql = $@"select count(1) from T_PRD_INSTOCK where FBILLNO=@BillNo ";
            return DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;
        }
    }
}
