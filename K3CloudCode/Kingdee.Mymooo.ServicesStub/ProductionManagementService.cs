using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Log;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Business.PlugIn.ProductionManagement;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ProductionManagement.Dispatch;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.PrdMoManagement;
using Kingdee.Mymooo.ServiceHelper.ProductionManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    [Kingdee.BOS.Util.HotUpdate]
    public class ProductionManagementService : KDBaseService
    {
        public ProductionManagementService(KDServiceContext context) : base(context)
        {
        }

        public string PrdMoChangeWorkShop()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                var request = JsonConvertUtils.DeserializeObject<PrdMoChangeWorkShopRequest>(data);
                response = PrdMoServiceHelper.PrdMoChangeWorkShop(ctx, request);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
        public string PrdPPBomChange()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                var request = JsonConvertUtils.DeserializeObject<MesPrd_PPBOMRequest>(data);
                response = PrdMoServiceHelper.PrdPPBomChange(ctx, request);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
        /// <summary>
        /// 生产订单关闭
        /// </summary>
        /// <returns></returns>
        public string StatusMoOrderAction()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                var request = JsonConvertUtils.DeserializeObject<PrdMoStatus>(data);
                response = PrdMoServiceHelper.StatusMoOrderAction(ctx, request);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        public string PrdMoChange()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PrdMoChangeRequest>(data);
            PrdMOOrderBusiness order = new PrdMOOrderBusiness();
            var response = order.PrdMoChange(ctx, request);
            return JsonConvertUtils.SerializeObject(response);
        }

        public string SendMakeDispatchs()
        {
            string data = string.Empty;
            IOperationResult response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                response = ProductionMoServiceHelper.SendMakeDispatchs(ctx, JsonConvertUtils.DeserializeObject<SendMakeDispatchRequest>(data));
            }
            catch (Exception ex)
            {
                response = new OperationResult();
                response.ValidationErrors.Add(new ValidationErrorInfo("", "0", 0, 0, "0", ex.Message, "Exception"));
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        public string MesProductionMoStatus()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                ProductionMoBussiness productionMoBussiness = new ProductionMoBussiness();
                response = productionMoBussiness.MesProductionMoStatus(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    //Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        public string MesCancelMoStatus()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                ProductionMoBussiness productionMoBussiness = new ProductionMoBussiness();
                response = productionMoBussiness.MesCancelMoStatus(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    //Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 部门
        /// </summary>
        /// <returns></returns>
        public string Department()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                DepartmentBusiness departmentBusiness = new DepartmentBusiness();
                response = departmentBusiness.Execute(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    //Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        // 1、写入bom和工序数据到erp的bom和工序表
        // 2、写入云平台传入的所有到物料表，物料表新增一个字段，根据物料存储
        /// <summary>
        /// 接收云平台传递过来的bom和工序等数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Dispatch()
        {
            DispatchInfo entity = new DispatchInfo();
            ResponseMessage<string> response = new ResponseMessage<string>();
            response.Code = ResponseCode.Success;
            //response.Data = string.Empty;
            response.Message = "成功";

            try
            {
                string data = string.Empty;
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }

                entity = JsonConvert.DeserializeObject<DispatchInfo>(data);

                Logger.Info("生产制造", "接收云平台DispatchInfo数据包，开始...");

                if (entity.part.drawNumVersion != entity.process.drawNumVersion)
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "part里的drawNumVersion和process里的drawNumVersion不一致";
                    return JsonConvert.SerializeObject(response);
                }

                //默认蚂蚁制造（顺德）
                if (string.IsNullOrWhiteSpace(entity.CompanyCode))
                {
                    entity.CompanyCode = "MYZZ";
                }

                var bDispatch = new DispatchBusiness();

                //记录云平台DispatchInfo数据包日志
                Logger.Info("生产制造", "云平台回传ERP工序、Bom等信息集合：" + JsonConvertUtils.SerializeObject(entity));

                //生产资料导入MES的数据接口
                var package = new
                {
                    action = "mymoooErpToMesForRp",
                    id = Guid.NewGuid(),
                    data = new dynamic[] { entity.process }
                };
                var timestamp = WebApiSignature.CreateTimestamp();
                var nonce = "b80b9c1148434c8fb975185238a7965c";
                var signature = WebApiSignature.Sign(timestamp, nonce, "");

                Logger.Info("生产制造", "生产资料导入MES，开始...");
                var msg = CommonApiRequest.WorkbenchSignatureInvokeWebService(timestamp, signature, WebUIConfigHelper.WorkbenchForMqMesUrl, JsonConvertUtils.SerializeObject(package), "post");
                Logger.Info("生产制造", "生产资料导入MES，结束...");

                //接收云平台传递过来的bom和工序等数据
                Logger.Info("生产制造", "写入bom和工序等数据，开始...");

                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                //创建主物料
                MaterialInfo main = new MaterialInfo(entity.part.drawNum, bDispatch.GetMaterialNameByCode(ctx, entity.part.drawNum));
                main.UseOrgId = 1;
                main.ErpClsID = 2; //自制
                MaterialServiceHelper.TryGetOrAdd(ctx, main, new List<long>() { 0 });

                //主物料物料属性改成自制（只针对广东蚂蚁制造）
                //var userInfo = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(ctx, ctx.UserId);

                //int orgID = bDispatch.GetOrgIdByOrgNumber(ctx, "GDMYZZ");
                //int mID = bDispatch.GetMIdByOrgIdAndNumber(ctx, entity.part.drawNum, orgID);
                //bDispatch.UpdateMaterialAttribute(ctx, mID);

                //创建BOM子物料
                MaterialInfo detail = new MaterialInfo(entity.bom.specification, entity.bom.name);
                main.UseOrgId = 1;
                detail.ErpClsID = 1; //外购
                MaterialServiceHelper.TryGetOrAdd(ctx, detail, new List<long>() { 0 });

                //创建BOM前判断是否存在
                if (bDispatch.IsExistMaterialBomVer(ctx, entity.part.drawNum, entity.bom.specification + "_" + entity.part.drawNumVersion))
                {
                    //response.IsSuccess = false;
                    //response.Message = "物料" + entity.part.drawNum + "已经存在" + entity.bom.specification + "_" + entity.part.drawNumVersion + "的BOM版本，请重新设置！";
                    //return JsonConvert.SerializeObject(response);
                }
                else
                {
                    //创建BOM
                    ENGBomInfo bom = new ENGBomInfo(entity.part.drawNum);
                    bom.FMATERIALID = entity.part.drawNum;
                    bom.FNUMBER = entity.bom.specification + "_" + entity.part.drawNumVersion; //图号版本对应BOM版本

                    List<BomEntity> entitylist = new List<BomEntity>();
                    BomEntity ent = new BomEntity();
                    ent.FMATERIALIDCHILD = entity.bom.specification;
                    ent.FNUMERATOR = Convert.ToDecimal(entity.bom.weight);
                    entitylist.Add(ent);

                    bom.Entity = entitylist;

                    ENGBomServiceHelper.TryGetOrAdd(ctx, bom);
                }

                //工序
                bDispatch.CreateProcess(ctx, entity);

                //图号工序
                bDispatch.CreateMaterialProcess(ctx, entity);

                Logger.Info("生产制造", "写入bom和工序等数据，结束...");
                Logger.Info("生产制造", "接收云平台DispatchInfo数据包，结束...");

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception e)
            {
                Logger.Info("生产制造", "接收云平台DispatchInfo数据包，异常...");
                Logger.Info("生产制造", e.Message);
                response.Code = ResponseCode.Exception;
                response.Message = e.Message;
                return JsonConvert.SerializeObject(response);
            }

        }

        /// <summary>
        /// 接收mes传递过来的工序汇报数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public MessageHelp ProcessReport()
        {
            try
            {
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                string data = string.Empty;
                ProcessReportEntity entity = new ProcessReportEntity();
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }

                entity = JsonConvert.DeserializeObject<ProcessReportEntity>(data);
                if (string.IsNullOrWhiteSpace(entity.CompanyCode))
                {
                    entity.CompanyCode = "MYZZ";
                }

                StringBuilder sb = new StringBuilder();
                //var bDispatch = new DispatchBLL();
                var bDispatch = new DispatchBusiness();
                //1、参数必填校验
                #region 参数必填校验
                if (string.IsNullOrEmpty(entity.Id))
                {
                    sb.Append("Id主键唯一值不能为空；");
                }
                if (string.IsNullOrEmpty(entity.ReportType))
                {
                    sb.Append("汇报类型不能为空；");
                }
                if (string.IsNullOrEmpty(entity.WoNo))
                {
                    sb.Append("工单编号不能为空；");
                }
                if (string.IsNullOrEmpty(entity.WoSeqNo))
                {
                    sb.Append("工单序号不能为空；");
                }
                if (string.IsNullOrEmpty(entity.ProcessNo))
                {
                    sb.Append("工序编号不能为空；");
                }
                if (string.IsNullOrEmpty(entity.WorkWorkshop))
                {
                    sb.Append("开工车间不能为空；");
                }
                if (string.IsNullOrEmpty(entity.Operator))
                {
                    sb.Append("操作员不能为空；");
                }
                if (string.IsNullOrEmpty(entity.UsageDevice))
                {
                    sb.Append("使用设备不能为空；");
                }
                if (string.IsNullOrEmpty(entity.WorkTime.ToString()))
                {
                    sb.Append("开工时间不能为空；");
                }
                if (string.IsNullOrEmpty(entity.CompleteTime.ToString()))
                {
                    sb.Append("完工时间不能为空；");
                }
                if (string.IsNullOrEmpty(entity.RunMinute.ToString()))
                {
                    sb.Append("运行时长（分钟）不能为空；");
                }
                if (string.IsNullOrEmpty(entity.CompleteAmount.ToString()))
                {
                    sb.Append("完工数量不能为空；");
                }
                if (string.IsNullOrEmpty(entity.Operation))
                {
                    sb.Append("加工顺序不能为空；");
                }
                #endregion

                if (sb.Length > 0)
                {
                    return new MessageHelp() { IsSuccess = false, Message = sb.ToString(), Data = entity.Id };
                }

                //2、数据有效性校验
                #region 数据有效性校验
                //判断mes工序回传过来的Id主键唯一值是否唯一
                bool resultId = bDispatch.IsExistMesProcessId(ctx, entity.Id);
                if (resultId)
                {
                    return new MessageHelp() { IsSuccess = false, Message = entity.WoNo + "工单和" + entity.WoSeqNo + "工单序号的Id主键唯一值" + entity.Id + "已存在！", Data = entity.Id };
                }
                //判断工单唯一性
                bool resultWo = bDispatch.IsExistWo(ctx, entity.WoNo);
                if (!resultWo)
                {
                    return new MessageHelp() { IsSuccess = false, Message = entity.WoNo + "工单不存在！", Data = entity.Id };
                }
                //判断工序编号是否存在
                bool resultProcess = bDispatch.IsExistProcess(ctx, entity.ProcessNo + "_" + entity.Operation);
                if (!resultProcess)
                {
                    return new MessageHelp() { IsSuccess = false, Message = entity.ProcessNo + "_" + entity.Operation + "工序编号不存在！", Data = entity.Id };
                }
                //判断车间是否存在
                bool resultWorkshop = bDispatch.IsExistWorkshop(ctx, entity.WorkWorkshop);
                if (!resultWorkshop)
                {
                    return new MessageHelp() { IsSuccess = false, Message = entity.WorkWorkshop + "开工车间不存在！", Data = entity.Id };
                }
                //老ERP写法，金蝶是否需要 待定
                ////判断工单工序汇报中工序代码和加工顺序组合是否存在
                //bool resultWoNoProcessNoOperation = bDispatch.IsExistWoNoProcessNoOperation(entity.WoNo, entity.ProcessNo + "_" + entity.Operation, entity.CompanyCode);
                //if (!resultWoNoProcessNoOperation)
                //{
                //    return new MessageHelp() { IsSuccess = false, Message = entity.WoNo + "工单的" + entity.ProcessNo + "工序编号和" + entity.Operation + "加工顺序不存在！", Data = entity.Id };
                //}
                #endregion

                entity.Material = bDispatch.GetItemNoByWoAndWoSeq(ctx, entity.WoNo);

                Logger.Info("生产制造", $"接收mes传递过来的工序汇报数据{JsonConvertUtils.SerializeObject(entity)}");

                #region 判断
                ////第一步当前工单是否完工 否写入 是返回
                //var sqlIsExist = @" select top 1 c.FNUMBER,b.FQTY,b.FPLANFINISHDATE,b.FBOMID,d.FNUMBER as FBOMVER from T_PRD_MO a inner join T_PRD_MOENTRY b 
                //                        on a.FID=b.FID left join T_BD_MATERIAL c on b.FMATERIALID=c.FMATERIALID left join T_ENG_BOM d on b.FBOMID=d.FID
                //                        where a.FBILLNO='" + entity.WoNo + "'";
                //var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sqlIsExist);
                //string version = "";
                //int orderNum = 0;
                //if (datas.Count > 0)
                //{
                //    int index = datas[0]["FBOMVER"].ToString().LastIndexOf('_') + 1;
                //    version = datas[0]["FBOMVER"].ToString().Substring(index, datas[0]["FBOMVER"].ToString().Length - index);
                //    orderNum = Convert.ToInt32(datas[0]["FQTY"]);
                //}
                ////获取图号工序基础数据
                //List<string> processs = bDispatch.GetAllProcessByMaterialVer(ctx, entity.Material, version);
                ////获取工单工序汇报数据
                //List<ProcessReportEntity> processReport = bDispatch.GetProcessReportByWoNo(ctx, entity.WoNo);

                //bool flag = true;
                //int count = 0;
                //foreach (var p in processs)
                //{
                //    //processReport.FindAll(a=> a.ProcessNo+"_"+a.Operation == item).
                //    foreach (var pr in processReport)
                //    {
                //        if (pr.ProcessNo + "_" + pr.Operation == p)
                //        {
                //            count += pr.CompleteAmount;
                //        }
                //    }
                //    if (count != orderNum)
                //    {
                //        flag = false;
                //    }
                //}
                #endregion

                //获取版本和工单数
                var sqlIsExist = @" select top 1 c.FNUMBER,b.FQTY,b.FPLANFINISHDATE,b.FBOMID,d.FNUMBER as FBOMVER from T_PRD_MO a inner join T_PRD_MOENTRY b 
                                        on a.FID=b.FID left join T_BD_MATERIAL c on b.FMATERIALID=c.FMATERIALID left join T_ENG_BOM d on b.FBOMID=d.FID
                                        where a.FBILLNO='" + entity.WoNo + "'";
                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sqlIsExist);
                string version = "";
                int orderNum = 0;
                if (datas.Count > 0)
                {
                    int index = datas[0]["FBOMVER"].ToString().LastIndexOf('_') + 1;
                    version = datas[0]["FBOMVER"].ToString().Substring(index, datas[0]["FBOMVER"].ToString().Length - index);
                    orderNum = Convert.ToInt32(datas[0]["FQTY"]);
                }

                if (this.IsComp(ctx, entity, version, orderNum)) //完工
                {
                    return new MessageHelp() { IsSuccess = false, Message = "工单：" + entity.WoNo + "已完工，不允许再次汇报！", Data = entity.Id };
                }
                else //未完工
                {
                    //传递数据给云平台成功后，开始写入erp工序汇报数据
                    bool msgResult = bDispatch.WriteProcessReport(ctx, entity);
                    if (msgResult)
                    {
                        if (this.IsComp(ctx, entity, version, orderNum))//完工
                        {
                            #region 先把mes传递过来的工序汇报数据，直接传递给云平台

                            //先把mes传递过来的工序汇报数据，直接传递给云平台。（另外加上销售订单号SalesOrderNumber和零件编号PartNumber） 
                            ProcessReportForCloudEntity cloudEntity = new ProcessReportForCloudEntity();
                            cloudEntity.SalesNumber = bDispatch.GetSoNoByWoNo(ctx, entity.WoNo);
                            cloudEntity.PartNumber = entity.Material;
                            cloudEntity.CompleteAmount = orderNum; // entity.CompleteAmount;

                            var timestamp = WebApiSignature.CreateTimestamp();
                            var nonce = "b80b9c1148434c8fb975185238a7965c";
                            var signature = WebApiSignature.Sign(timestamp, nonce, "");
                            //var msg = CommonApiRequest.WorkbenchSignatureInvokeWebService(timestamp, signature, WebUIConfigHelper.DispatchToCloudUrl + "api/cnc/dispatch/reports", JsonConvertUtils.SerializeObject(cloudEntity), "post");

                            //if (!string.IsNullOrWhiteSpace(msg))
                            //{
                            //    var result = JsonConvertUtils.DeserializeObject<MessageHelpProcessResultReturn>(msg);
                            //    if (result.success)
                            //    {
                            //        //赞不做处理
                            //    }
                            //    else
                            //    {
                            //        Logger.Info("生产制造", $"ERP传递MES的工序汇报数据给云平台后，发现错误：“{result.msg}”");
                            //        return new MessageHelp() { IsSuccess = false, Message = "ERP传递MES的工序汇报数据给云平台后，发现错误：“" + result.msg + "”" };
                            //    }
                            //}
                            //else
                            //{
                            //    Logger.Info("生产制造", $"ERP传递MES的工序汇报数据给云平台后，云平台未回传ERP结果");
                            //    return new MessageHelp() { IsSuccess = false, Message = "ERP传递MES的工序汇报数据给云平台后，云平台未回传ERP结果" };
                            //}
                            #endregion
                        }

                        return new MessageHelp() { IsSuccess = true, Message = "成功", Data = entity.Id };
                    }
                    else
                    {
                        return new MessageHelp() { IsSuccess = false, Message = "ERP系统错误！", Data = entity.Id };
                    }
                }

                //第二步写入当前工单汇报
                //第三步当前工单是否完工 完工给非标
                #region 同步云平台工序汇报结果

                #endregion
            }
            catch (Exception e)
            {
                Logger.Info("生产制造", "工序汇报异常：" + e.Message);
                return new MessageHelp() { IsSuccess = false, Message = "工序汇报异常：" + e.Message };
            }
        }

        /// <summary>
        /// 是否完工
        /// </summary>
        /// <returns></returns>
        public bool IsComp(Context ctx, ProcessReportEntity entity, string version, int orderNum)
        {
            var bDispatch = new DispatchBusiness();

            //获取图号工序基础数据
            List<string> processs = bDispatch.GetAllProcessByMaterialVer(ctx, entity.Material, version);
            //获取工单工序汇报数据
            List<ProcessReportEntity> processReport = bDispatch.GetProcessReportByWoNo(ctx, entity.WoNo);

            bool flag = true;

            foreach (var p in processs)
            {
                int count = 0;
                //processReport.FindAll(a=> a.ProcessNo+"_"+a.Operation == item).
                foreach (var pr in processReport)
                {
                    if (pr.ProcessNo + "_" + pr.Operation == p)
                    {
                        count += pr.CompleteAmount;
                    }
                }
                if (count != orderNum)
                {
                    flag = false;
                }
            }

            return flag;

        }

        /// <summary>
        /// 接收mes传递过来的工序取消数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public MessageHelp ProcessCancel()
        {
            try
            {
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                string data = string.Empty;
                ProcessCancelEntity entity = new ProcessCancelEntity();
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }

                entity = JsonConvert.DeserializeObject<ProcessCancelEntity>(data);
                Logger.Info("生产制造", $"接收mes传递过来的工序取消数据{JsonConvertUtils.SerializeObject(entity)}");

                var bDispatch = new DispatchBusiness();
                bool result = bDispatch.WriteProcessCancel(ctx, entity);
                if (result)
                {
                    return new MessageHelp() { IsSuccess = true, Message = "成功" };
                }
                else
                {
                    return new MessageHelp() { IsSuccess = false, Message = "ERP系统错误！" };
                }
            }
            catch (Exception e)
            {
                Logger.Info("生产制造", "工序取消异常" + e.Message);
                return new MessageHelp() { IsSuccess = false, Message = "ERP系统错误！" + e.Message };
            }

        }

        /// <summary>
        /// MES生产领料
        /// </summary>
        /// <returns></returns>
        public string MesProductionPicking()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                MesProductionPickingBusiness business = new MesProductionPickingBusiness();
                response = business.Execute(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// MES生产补料
        /// </summary>
        /// <returns></returns>
        public string MesProductionFeedMtrl()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                MesProductionFeedMtrlBusiness business = new MesProductionFeedMtrlBusiness();
                response = business.Execute(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// MES生产退料
        /// </summary>
        /// <returns></returns>
        public string MesProductionReturnMtrl()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                MesProductionReturnMtrlBusiness business = new MesProductionReturnMtrlBusiness();
                response = business.Execute(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// MES生产入库
        /// </summary>
        /// <returns></returns>
        public string MesProductionInStock()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                MesProductionInStockBusiness business = new MesProductionInStockBusiness();
                response = business.Execute(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
        /// <summary>
        /// Mes一键退料
        /// </summary>
        /// <returns></returns>
        public string MesOneClickReturnMaterial()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                ProductionMoBussiness productionMoBussiness = new ProductionMoBussiness();
                response = productionMoBussiness.MesOneClickReturnMaterial(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
    }
}
