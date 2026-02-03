using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.ManagementCenter;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    public class DeptUserBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            List<WXUserRequest> list = JsonConvertUtils.DeserializeObject<List<WXUserRequest>>(message);
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (list.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "用户信息不能为空";
                return response;
            }
            CreateBill(ctx, list, response);
            return response;
        }
        public void CreateBill(Context ctx, List<WXUserRequest> list, ResponseMessage<dynamic> response)
        {
            foreach (var item in list)
            {
                try
                {
                    string usersql = "select top 1 FID from T_HR_EMPINFO where FWECHATCODE=@WechatCode";
                    //string deptsql = "select top 1 FDEPTID from T_BD_DEPARTMENT where FDEPARTMENTID=@departmentId";
                    var id = DBServiceHelper.ExecuteScalar<int>(ctx, usersql, 0, paramList: new SqlParam("@WechatCode", KDDbType.String, item.UserId));
                    if (id == 0)
                    {
                        var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Empinfo");
                        billView.Model.SetValue("FWeChatCode", item.UserId);
                        billView.Model.SetValue("FName", item.Name);
                        billView.Model.SetValue("FTel", item.Telephone);
                        billView.Model.SetValue("FAddress", item.Address);
                        billView.Model.SetValue("FMobile", item.Mobile);
                        billView.Model.SetValue("FVersions", item.Versions);
                        billView.Model.SetValue("FEmail", item.Email);
                        billView.Model.SetValue("FStaffNumber", item.UserId);
                        
                        var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
                        //清除释放网控
                        billView.CommitNetworkCtrl();
                        billView.InvokeFormOperation(FormOperationEnum.Close);
                        billView.Close();
                        if (!oper.IsSuccess)
                        {
                            if (oper.ValidationErrors.Count > 0)
                            {
                                response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                                response.Code = ResponseCode.Exception;
                            }
                            else
                            {
                                response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                response.Code = ResponseCode.Exception;
                            }

                        }
                    }
                    else
                    {
                        var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Empinfo", id.ToString());
                        billView.Model.SetValue("FWeChatCode", item.UserId);
                        billView.Model.SetValue("FName", item.Name);
                        billView.Model.SetValue("FTel", item.Telephone);
                        billView.Model.SetValue("FAddress", item.Address);
                        billView.Model.SetValue("FMobile", item.Mobile);
                        billView.Model.SetValue("FVersions", item.Versions);
                        billView.Model.SetValue("FEmail", item.Email);
                        billView.Model.SetValue("FStaffNumber", item.UserId);

                        if (billView.Model.DataObject["ForbidStatus"].ToString() == "B")
                        {
                            string[] fid = new string[] { id.ToString() };
                            var oper3 = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, billView.BusinessInfo, fid, "Enable");
                            if (!oper3.IsSuccess)
                            {
                                if (oper3.ValidationErrors.Count > 0)
                                {
                                    response.Message += string.Join(";", oper3.ValidationErrors.Select(p => p.Message));
                                }
                                else
                                {
                                    response.Message += string.Join(";", oper3.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                }
                            }
                        }
                        if (billView.Model.DataObject["DocumentStatus"].ToString() != "C")
                        {
                            var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
                            if (!oper.IsSuccess)
                            {
                                if (oper.ValidationErrors.Count > 0)
                                {
                                    response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                                    response.Code = ResponseCode.Exception;
                                }
                                else
                                {
                                    response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                    response.Code = ResponseCode.Exception;
                                }
                            }
                        }
                        else
                        {
                            var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
                            if (!oper.IsSuccess)
                            {
                                if (oper.ValidationErrors.Count > 0)
                                {
                                    response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                                    response.Code = ResponseCode.Exception;
                                }
                                else
                                {
                                    response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                    response.Code = ResponseCode.Exception;
                                }
                            }
                        }
                        //清除释放网控
                        billView.CommitNetworkCtrl();
                        billView.InvokeFormOperation(FormOperationEnum.Close);
                        billView.Close();
                    }
                }
                catch (Exception ex)
                {
                    response.Message += ex.Message;
                }

            }
            string versionUsersql = "select FID from T_HR_EMPINFO where FVERSIONS!=@Versions And FFORBIDSTATUS='A' and FNUMBER not in ('gzff','ygsb','yggjj','yggs','sssdf') ";

            List<string> ids = new List<string>();
            using (var idreader = DBServiceHelper.ExecuteReader(ctx, versionUsersql, paramList: new List<SqlParam> { new SqlParam("@Versions", KDDbType.String, list.FirstOrDefault().Versions) }))
            {
                while (idreader.Read())
                {
                    ids.Add(idreader["FID"].ToString());
                }
            }

            if (ids.Count > 0)
            {
                FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_Empinfo") as FormMetadata;
                var oper2 = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, meta.BusinessInfo, ids.ToArray(), "Forbid");
                if (!oper2.IsSuccess)
                {
                    if (oper2.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.Message += string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            response.Code = ResponseCode.Success;

        }
    }
}
