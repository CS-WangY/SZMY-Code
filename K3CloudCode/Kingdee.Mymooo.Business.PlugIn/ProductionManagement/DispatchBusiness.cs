using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.ServiceHelper;


namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    public class DispatchBusiness
    {
        /// <summary>
        /// 获取派产信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetDispatchInfo(Context ctx, string message, string[] soNoList)
        {
            List<string> request = JsonConvert.DeserializeObject<List<string>>(message);

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (request.Count == 0)
            {
                response.Message = "请传派产销售订单";
                return response;
            }
            
            DispatchEntity dispatchInfo = new DispatchEntity();
            dispatchInfo.SalesOrderNumber = soNoList[0];
            dispatchInfo.Parts = null;
            dispatchInfo.DispatchDateTime = DateTime.Now;
            dispatchInfo.DispatchUser = "System";
            response.Data = dispatchInfo;
            response.Code = ResponseCode.Success;
            response.Message = "获取成功";
            return response;
        }

        /// <summary>
        /// 创建工序 (自定义页面)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public ResponseMessage<dynamic> CreateProcess(Context ctx, DispatchInfo request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            foreach (var item in request.process.processLines)
            {
                var sqlIsExist = @"select * from T_PRD_Process where FNUMBER like '%" + item.processId + "_" + item.operationNum + "%'";
                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sqlIsExist);
                if (datas.Count == 0)
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_PRD_Process");
                    billView.Model.SetValue("FNumber", item.processId + "_" + item.operationNum);
                    billView.Model.SetValue("FName", item.processName);

                    var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
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
                
            }

            response.Code = ResponseCode.Success;
            response.Message = string.Empty;
            response.Data = string.Empty;
            return response;
        }

        /// <summary>
        /// 创建图号工序 (自定义页面)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public ResponseMessage<dynamic> CreateMaterialProcess(Context ctx, DispatchInfo request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            foreach (var item in request.process.processLines)
            {
                var sqlIsExist = @"select * from T_PRD_MaterialProcess where FMATERIAL='" + request.part.drawNum + "' and FVERSION='" + request.part.drawNumVersion + "' and FNUMBER like '%" + item.processId + "_" + item.operationNum + "%'";
                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sqlIsExist);
                if (datas.Count == 0)
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_PRD_MaterialProcess");
                    billView.Model.SetValue("FNumber", item.processId + "_" + item.operationNum);
                    billView.Model.SetValue("FName", item.processName);

                    billView.Model.SetValue("FMATERIAL", request.part.drawNum);
                    billView.Model.SetValue("FVERSION", request.part.drawNumVersion);

                    var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
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

            }

            response.Code = ResponseCode.Success;
            response.Message = string.Empty;
            response.Data = string.Empty;
            return response;
        }

        /// <summary>
        /// 根据工单号和序号判断工单是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsExistMesProcessId(Context ctx, string id)
        {
            var sql = $@" SELECT TOP 1 * FROM T_PRD_ProcessReport WHERE FMESID='{id}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }     
        }

        /// <summary>
        /// 根据工单号和序号判断工单是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsExistWo(Context ctx, string woNo)
        {
            var sql = $@" SELECT TOP 1 * FROM T_PRD_MO WHERE FBILLNO='{woNo}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 判断工序是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsExistProcess(Context ctx, string process)
        {
            var sql = $@" SELECT TOP 1 * FROM T_PRD_Process WHERE FNUMBER='{process}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 判断车间是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsExistWorkshop(Context ctx, string workshop)
        {
            var sql = $@" SELECT TOP 1 * FROM T_BD_DEPARTMENT WHERE FNUMBER='{workshop}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 根据工单编码和工单序号获取物料编码
        /// </summary>
        /// <returns></returns>
        public string GetItemNoByWoAndWoSeq(Context ctx, string woNo)
        {
            var sql = $@" select top 1 c.FNUMBER from T_PRD_MO a inner join T_PRD_MOENTRY b on a.FID=b.FID inner join T_BD_MATERIAL c on b.FMATERIALID=c.FMATERIALID where a.FBILLNO='{woNo}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return "";
            }
            else
            {
                return datas[0]["FNUMBER"].ToString();
            }
        }

        /// <summary>
        /// 接收mes传递过来的工序汇报数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool WriteProcessReport(Context ctx, ProcessReportEntity entity)
        {
            if (entity.ReportType == "1") //正常汇报
            {
                #region 写入数据
                var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_PRD_ProcessReport");
                billView.Model.SetValue("FBILLNO", entity.WoNo + "_" + DateTime.Now.ToLongDateString() + "_" + DateTime.Now.ToLongTimeString());
                billView.Model.SetValue("FWoNo", entity.WoNo);
                billView.Model.SetValue("FWoSeqNo", entity.WoSeqNo);
                billView.Model.SetValue("FProcessNo", entity.ProcessNo);
                billView.Model.SetValue("FOperation", entity.Operation);
                billView.Model.SetValue("FWorkWorkshop", entity.WorkWorkshop);
                billView.Model.SetValue("FMaterial", entity.Material);
                billView.Model.SetValue("FOperator", entity.Operator);
                billView.Model.SetValue("FUsageDevice", entity.UsageDevice);
                billView.Model.SetValue("FWorkTime", entity.WorkTime);
                billView.Model.SetValue("FCompleteTime", entity.CompleteTime);
                billView.Model.SetValue("FRunMinute", entity.RunMinute);
                billView.Model.SetValue("FCompleteAmount", entity.CompleteAmount);
                billView.Model.SetValue("FMesId", entity.Id);
                billView.Model.SetValue("FReportType", entity.ReportType);
                billView.Model.SetValue("FCompanyCode", entity.CompanyCode);

                var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
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
                #endregion
            }
            else if (entity.ReportType == "3") //报废
            {
                #region 写入数据
                var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_PRD_ProcessReport");
                billView.Model.SetValue("FBILLNO", entity.WoNo + "_" + DateTime.Now.ToLongDateString() + "_" + DateTime.Now.ToLongTimeString());
                billView.Model.SetValue("FWoNo", entity.WoNo);
                billView.Model.SetValue("FWoSeqNo", entity.WoSeqNo);
                billView.Model.SetValue("FProcessNo", entity.ProcessNo);
                billView.Model.SetValue("FOperation", entity.Operation);
                billView.Model.SetValue("FWorkWorkshop", entity.WorkWorkshop);
                billView.Model.SetValue("FMaterial", entity.Material);
                billView.Model.SetValue("FOperator", entity.Operator);
                billView.Model.SetValue("FUsageDevice", entity.UsageDevice);
                billView.Model.SetValue("FWorkTime", entity.WorkTime);
                billView.Model.SetValue("FCompleteTime", entity.CompleteTime);
                billView.Model.SetValue("FRunMinute", entity.RunMinute);
                billView.Model.SetValue("FCompleteAmount", entity.CompleteAmount);
                billView.Model.SetValue("FMesId", entity.Id);
                billView.Model.SetValue("FReportType", entity.ReportType);
                billView.Model.SetValue("FCompanyCode", entity.CompanyCode);

                var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
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
                #endregion
            }
            else { }

            return true;
        }

        /// <summary>
        /// 接收mes传递过来的工序取消数据
        /// </summary>
        /// <returns></returns>
        public bool WriteProcessCancel(Context ctx, ProcessCancelEntity entity)
        {
            try
            {
                foreach (var item in entity.Id)
                {
                    var sql = $@" delete from T_PRD_ProcessReport where FMESID='{item}' ";
                    var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
            
        }

        /// <summary>
        /// 根据工单编码获取销售订单号
        /// </summary>
        /// <returns></returns>
        public string GetSoNoByWoNo(Context ctx, string woNo)
        {
            var sql = $@" select top 1 FSALEORDERNO from T_PRD_MOENTRY a inner join T_PRD_MO b on a.FID=b.FID where b.FBILLNO='{woNo}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return "";
            }
            else
            {
                return datas[0]["FSALEORDERNO"].ToString();
            }
        }

        /// <summary>
        /// 根据父物料编码和BOM版本判断是否唯一
        /// </summary>
        /// <returns></returns>
        public bool IsExistMaterialBomVer(Context ctx, string material, string bomVer)
        {
            var sql = $@" select top 1 * from T_ENG_BOM b inner join T_BD_MATERIAL m on b.FMATERIALID=m.FMATERIALID where m.FUSEORGID=1 and b.FNUMBER='{bomVer}' and m.FNUMBER='{material}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 根据物料编码获取物料名称
        /// </summary>
        /// <returns></returns>
        public string GetMaterialNameByCode(Context ctx, string materialCode)
        {
            var sql = $@" select top 1 ml.FNAME from T_BD_MATERIAL_L ml inner join T_BD_MATERIAL m on ml.FMATERIALID=m.FMATERIALID where m.FUSEORGID=1 and m.FNUMBER='{materialCode}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return materialCode;
            }
            else
            {
                return datas[0]["FNAME"].ToString();
            }
        }

        /// <summary>
        /// 修改物料属性
        /// </summary>
        /// <returns></returns>
        public bool UpdateMaterialAttribute(Context ctx, int mId)
        {
            try
            {
                var sql = $@"update T_BD_MATERIALBASE set FERPCLSID=2 where FMATERIALID={mId} ";
                var datas = DBServiceHelper.Execute(ctx, sql);

                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }

        /// <summary>
        /// 根据组织编码获取组织ID
        /// </summary>
        /// <returns></returns>
        public int GetOrgIdByOrgNumber(Context ctx, string orgNumber)
        {
            var sql = $@" select top 1 FORGID from t_org_organizations where FNUMBER='{orgNumber}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return 1;
            }
            else
            {
                return Convert.ToInt32(datas[0]["FORGID"]);
            }
        }

        /// <summary>
        /// 根据组织ID和物料编码获取物料ID
        /// </summary>
        /// <returns></returns>
        public int GetMIdByOrgIdAndNumber(Context ctx, string fNumber, int orgId)
        {
            var sql = $@" select top 1 FMATERIALID from T_BD_MATERIAL where FNUMBER='{fNumber}' and FUSEORGID={orgId} ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count == 0)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(datas[0]["FMATERIALID"]);
            }
        }

        /// <summary>
        /// 根据图号和版本获取生产工单所有的工序
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllProcessByMaterialVer(Context ctx, string material, string version)
        {
            List<string> lst = new List<string>();
            var sql = $@" select FNUMBER from T_PRD_MaterialProcess where FMATERIAL='{material}' and FVERSION='{version}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count > 0)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    lst.Add(datas[i]["FNUMBER"].ToString());
                }
            }

            return lst;
        }

        /// <summary>
        /// 根据工单号获取工序汇报数据
        /// </summary>
        /// <returns></returns>
        public List<ProcessReportEntity> GetProcessReportByWoNo(Context ctx, string woNo)
        {
            List<ProcessReportEntity> lst = new List<ProcessReportEntity>();
            var sql = $@" select * from T_PRD_ProcessReport where FWONO='{woNo}' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (datas.Count > 0)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    ProcessReportEntity entity = new ProcessReportEntity();
                    entity.WoNo = woNo;
                    entity.ProcessNo = datas[i]["FPROCESSNO"].ToString();
                    entity.Operation = datas[i]["FOPERATION"].ToString();
                    entity.Material = datas[i]["FMATERIAL"].ToString();
                    entity.CompleteAmount = Convert.ToInt32(datas[i]["FCOMPLETEAMOUNT"]);
                    entity.ReportType = datas[i]["FREPORTTYPE"].ToString();
                    lst.Add(entity);
                    //lst.Add(datas[i]["FNUMBER"].ToString());
                }
            }

            return lst;
        }

    }
}
