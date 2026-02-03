using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    public class SalesCustBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            SalesCustRequest list = JsonConvertUtils.DeserializeObject<SalesCustRequest>(message);
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (list == null)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "同步信息不能为空";
                return response;
            }
            CheckBill(ctx, list, out response);
            return response;
        }

        public void CreateBill(Context ctx, SalesCustRequest request, out ResponseMessage<dynamic> response)
        {
            response = new ResponseMessage<dynamic>();
            try
            {
                if (!request.IsUnBind && request.UserCode.Count <= 0)
                {
                    response.Message += $"销售员微信编码不能为空";
                    response.Code = ResponseCode.ModelError;
                    return;
                }
                if (string.IsNullOrWhiteSpace(request.CustCode))
                {
                    response.Message += $"销售员[{request.TransferUserCode}]对应的客户不能为空";
                    response.Code = ResponseCode.ModelError;
                    return;
                }

                List<KeyValuePair<long, long>> salesId = new List<KeyValuePair<long, long>>();
                if (!request.IsUnBind)
                {
                    foreach (var item in request.UserCode)
                    {

                        ///判断销售组是否存在
                        string salessql = @"/*dialect*/SELECT ope.FOPERATORGROUPID,op.FBIZORGID FROM T_BD_PERSON b-- 人员表
                                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                                            INNER JOIN T_BD_STAFF s ON b.FPERSONID=s.FPERSONID
                                            INNER JOIN T_BD_OPERATORENTRY op ON s.FSTAFFID=op.FSTAFFID
                                            INNER JOIN T_BD_OPERATORDETAILS ope ON op.FENTRYID=ope.FENTRYID
                                            where  ope.FISDEFAULT=1 and c.FWECHATCODE= @userCode ";

                        using (var salesReader = DBServiceHelper.ExecuteReader(ctx, salessql, new List<SqlParam> { new SqlParam("@userCode", KDDbType.String, item.Trim()) }))
                        {
                            while (salesReader.Read())
                            {
                                salesId.Add(new KeyValuePair<long, long>(Convert.ToInt32(salesReader["FBIZORGID"]), Convert.ToInt32(salesReader["FOPERATORGROUPID"])));
                            }
                        }

                        if (salesId.Count <= 0)
                        {
                            response.Message += $"销售组{item}未在金蝶中维护";
                            response.Code = ResponseCode.ModelError;
                            return;
                        }

                    }
                }


                string custsql = "/*dialect*/select FCUSTID from T_BD_CUSTOMER where FNUMBER =@custCode";
                var custId = DBServiceHelper.ExecuteScalar(ctx, custsql, 0, new SqlParam("@custCode", KDDbType.String, request.CustCode));
                if (custId > 0)
                {
                    if (!request.IsUnBind)
                    {

                        var res = BasicDataSyncServiceHelper.SyncSaleCust(ctx, custId, salesId, request.IsFirstSync, request.UserCode, request.OrderNumber, request.TransferUserCode);
                        response.Message = res.Message;
                        response.Code = res.Code;
                        return;
                    }
                    else
                    {
                        var res = BasicDataSyncServiceHelper.UnBindSaleCust(ctx, custId);
                        response.Message = res.Message;
                        response.Code = res.Code;
                        return;
                    }
                }
                else
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message += $"客户信息不存在";
                    return;
                }

            }
            catch (Exception ex)
            {

                response.Message += ex.Message;
            }

        }

        public void CheckBill(Context ctx, SalesCustRequest request, out ResponseMessage<dynamic> response)
        {
            response = new ResponseMessage<dynamic>();
            try
            {
                if (!request.IsUnBind && request.UserCode.Count <= 0)
                {
                    response.Message += $"销售员微信编码不能为空";
                    response.Code = ResponseCode.ModelError;
                    return;
                }
                if (string.IsNullOrWhiteSpace(request.CustCode))
                {
                    response.Message += $"销售员[{request.TransferUserCode}]对应的客户不能为空";
                    response.Code = ResponseCode.ModelError;
                    return;
                }

                List<KeyValuePair<long, long>> salesId = new List<KeyValuePair<long, long>>();
                List<SalerCustList> salerCustLists = new List<SalerCustList>();
                if (!request.IsUnBind)
                {
                    foreach (var item in request.UserCode)
                    {
                        //获取员工所有认岗信息
                        string ssql = @"/*dialect*/SELECT e.FNUMBER,el.FNAME,s.FSTAFFID,s.FPostId,s.FWORKORGID,o.FNUMBER OrgNumber,st.*
                        FROM T_HR_EMPINFO e 
                        INNER JOIN T_BD_STAFFTEMP s ON e.FID=s.FID
                        INNER JOIN T_ORG_ORGANIZATIONS o ON s.FWORKORGID=o.FORGID
                        INNER JOIN T_HR_EMPINFO_L el ON e.FID=el.FID
						INNER JOIN T_BD_STAFF st ON s.FSTAFFID=st.FSTAFFID
                        WHERE st.FDOCUMENTSTATUS='C' AND st.FFORBIDSTATUS='A' AND s.FOPERATORTYPE='XSY' AND e.FWECHATCODE=@userCode ";
                        using (var salesReader = DBServiceHelper.ExecuteReader(ctx, ssql, new List<SqlParam> { new SqlParam("@userCode", KDDbType.String, item.Trim()) }))
                        {
                            while (salesReader.Read())
                            {
                                salerCustLists.Add(
                                    new SalerCustList()
                                    {
                                        UserCode = Convert.ToString(salesReader["FNUMBER"]),
                                        Staffid = Convert.ToInt32(salesReader["FSTAFFID"]),
                                        Bizorgid = Convert.ToInt32(salesReader["FWORKORGID"]),
                                        BizOrgNumber = Convert.ToString(salesReader["OrgNumber"]),
                                        UserName = Convert.ToString(salesReader["FNAME"]),
                                    }
                                    );
                            }
                        }
                        if (salerCustLists.Count <= 0)
                        {
                            response.Message += $"员工{item}未维护任何岗位";
                            response.Code = ResponseCode.ModelError;
                            return;
                        }
                        if (salerCustLists.Select(x => x.Bizorgid == 224428).ToList().Count <= 0)
                        {
                            response.Message += $"员工{item}在[深圳蚂蚁]组织下未维护任何岗位";
                            response.Code = ResponseCode.ModelError;
                            return;
                        }
                        if (salerCustLists.Select(x => x.Bizorgid == 1043841).ToList().Count <= 0)
                        {
                            response.Message += $"员工{item}在[北京蚂蚁]组织下未维护任何岗位";
                            response.Code = ResponseCode.ModelError;
                            return;
                        }
                        if (salerCustLists.Select(x => x.Bizorgid == 7348029).ToList().Count <= 0)
                        {
                            response.Message += $"员工{item}在[江苏蚂蚁]组织下未维护任何岗位";
                            response.Code = ResponseCode.ModelError;
                            return;
                        }
                    }
                    salesId = BasicDataSyncServiceHelper.SyncSAL_SC_SalerCust(ctx, salerCustLists);
                }


                string custsql = "/*dialect*/select FCUSTID from T_BD_CUSTOMER where FNUMBER =@custCode";
                var custId = DBServiceHelper.ExecuteScalar(ctx, custsql, 0, new SqlParam("@custCode", KDDbType.String, request.CustCode));
                if (custId > 0)
                {
                    if (!request.IsUnBind)
                    {
                        response = BasicDataSyncServiceHelper.SyncSaleCust(ctx, custId, salesId, request.IsFirstSync, request.UserCode, request.OrderNumber, request.TransferUserCode);
                    }
                    else
                    {
                        response = BasicDataSyncServiceHelper.UnBindSaleCust(ctx, custId);
                    }
                }
                else
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message += $"客户信息不存在";
                }

            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ModelError;
                response.Message += ex.Message;
            }

        }
    }
}
