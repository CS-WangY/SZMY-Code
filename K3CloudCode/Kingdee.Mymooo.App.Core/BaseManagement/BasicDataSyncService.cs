using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Core.BaseManagement;
using System;
using Kingdee.BOS.Orm.DataEntity;
using System.Text.RegularExpressions;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.DataEntity.Metadata.Service;
using Kingdee.BOS.Core.Util;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.Core.SalesManagement;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using Kingdee.BOS.App.Core.Utils;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.Mymooo.App.Core.BaseManagement
{
    public class BasicDataSyncService : IBasicDataSyncService
    {

        public ResponseMessage<dynamic> UnBindSaleCust(Context ctx, long custId)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            string sql = @"select e.fid,a.FNUMBER,a.FENTRYID,e.FSALEORGID from T_SAL_SCSALERCUST e
                                left join T_BD_OPERATORGROUPENTRY a on e.FSALERGROUPID = a.FENTRYID
                                                    where e.FCUSTOMERID=@custId ";
            List<SqlParam> pars = new List<SqlParam>();
            pars.Add(new SqlParam("@custId", KDDbType.Int32, custId));
            var salesids = new List<string>();
            using (var salesreader = DBUtils.ExecuteReader(ctx, sql, pars))
            {
                while (salesreader.Read())
                {
                    salesids.Add(salesreader["fid"].ToString());
                }
            }

            ///如果存在对应关系，则解除
            if (salesids.Count() > 0)
            {
                FormMetadata meta = MetaDataServiceHelper.Load(ctx, "SAL_SC_SalerCust") as FormMetadata;
                var oper = DeleteBill(ctx, meta.BusinessInfo, salesids.ToArray());
                if (!oper.IsSuccess)
                {
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                    response.Code = ResponseCode.ModelError;
                    return response;
                }
            }
            response.Code = ResponseCode.Success;
            return response;
        }
        /// <summary>
        /// 同步销售员与客户的关系
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custId"></param>
        /// <param name="sourceUserCode"></param>
        /// <param name="salesId"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncSaleCust(Context ctx, long custId, List<KeyValuePair<long, long>> salesId, bool isFirstSync, List<string> userCode, List<string> OrderNumber, string TransferUserCode)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
            {
                ///查询客户的绑定关系
                string sql = @"select e.fid,a.FNUMBER,a.FENTRYID,e.FSALEORGID from T_SAL_SCSALERCUST e
                                left join T_BD_OPERATORGROUPENTRY a on e.FSALERGROUPID = a.FENTRYID
                                                    where e.FCUSTOMERID=@custId ";
                List<SqlParam> pars = new List<SqlParam>();
                pars.Add(new SqlParam("@custId", KDDbType.Int32, custId));
                var salesids = new List<string>();
                using (var salesreader = DBUtils.ExecuteReader(ctx, sql, pars))
                {
                    while (salesreader.Read())
                    {
                        salesids.Add(salesreader["fid"].ToString());
                    }
                }

                ///如果存在对应关系，则解除
                if (!isFirstSync && salesids.Count() > 0)
                {
                    FormMetadata meta = MetaDataServiceHelper.Load(ctx, "SAL_SC_SalerCust") as FormMetadata;
                    var oper = DeleteBill(ctx, meta.BusinessInfo, salesids.ToArray());
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                        }
                        else
                        {
                            response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        }
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                }


                ///绑定新的销售员
                var billView = FormMetadataUtils.CreateBillView(ctx, "SAL_SC_SalerCust");
                ///苏州蚂蚁/深圳蚂蚁 都要建立销售员和客户的对应关系(组织中有维护业务员的情况下)
                List<DynamicObject> list = new List<DynamicObject>();
                foreach (var item in salesId)
                {

                    billView.CreateNewModelData();
                    billView.Model.SetValue("FSALEORGID", item.Key);
                    billView.Model.SetValue("FSalerId", 0);
                    billView.Model.SetValue("FSalerDeptId", 0);
                    billView.Model.SetValue("FSalerGroupId", item.Value);
                    billView.Model.SetValue("FCustomerId", custId);
                    billView.Model.SetValue("FCustItem", 1);
                    billView.InvokeFieldUpdateService("FCustomerId", 0);
                    list.Add(billView.Model.DataObject);
                }

                var oper2 = SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                if (!oper2.IsSuccess)
                {
                    if (oper2.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                    else
                    {
                        response.Message += string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                }
                if (OrderNumber != null && OrderNumber.Any())
                {
                    string salesSql = @"/*dialect*/select  b.FOPERATORGROUPID, a.fid,a.FDEPTID,e.FNUMBER from V_BD_SALESMAN a
                                    left join V_BD_SALESMANENTRY b on a.fid = b.fid
                                    left join T_ORG_ORGANIZATIONS e on a.FBIZORGID = e.FORGID
                                        where  a.FFORBIDSTATUS='A' and a.FDOCUMENTSTATUS='C' and FFORBIDDENSTATUS=0
                                        and a.fempnumber=@Code and a.FISUSE=1 and b.FISDEFAULT=1";
                    List<SqlParam> par2 = new List<SqlParam>();

                    List<SyncSalesCustRequest> req = new List<SyncSalesCustRequest>();
                    par2.Add(new SqlParam("@Code", KDDbType.String, TransferUserCode));
                    using (var salesreader = DBUtils.ExecuteReader(ctx, salesSql, par2))
                    {
                        while (salesreader.Read())
                        {
                            SyncSalesCustRequest request = new SyncSalesCustRequest();
                            request.SalesUserId = Convert.ToInt32(salesreader["fid"]);
                            request.DeptId = Convert.ToInt32(salesreader["FDEPTID"]);
                            request.SaleGroupId = Convert.ToInt32(salesreader["FOPERATORGROUPID"]);
                            request.OrgNumber = salesreader["FNUMBER"].ToString();
                            req.Add(request);
                        }
                    }
                    //string saleorderSql = @"select FID,FSALEORGID from T_SAL_ORDER t
                    //                        left join T_ORG_ORGANIZATIONS s on t.FSALEORGID = s.FORGID
                    //                        where FCUSTID=@custId and FCLOSESTATUS='A' and FCANCELSTATUS='A' and s.FNUMBER = 'SZMYGC' ";
                    List<SqlParam> par = new List<SqlParam>();
                    string str = "";
                    for (int i = 0; i < OrderNumber.Count; i++)
                    {
                        str += "@billNo" + i + ",";
                        par.Add(new SqlParam("@billNo" + i, KDDbType.String, OrderNumber[i]));
                    }
                    string selsql = @"/*dialect*/select  distinct s.FNUMBER,l.FNAME from T_SAL_ORDER t
                 left join T_ORG_ORGANIZATIONS s on t.FSALEORGID = s.FORGID 
                 left join T_ORG_ORGANIZATIONS_L l on s.FORGID = l.FORGID
                where   FCLOSESTATUS='A' and FCANCELSTATUS='A'  and FBILLNO in({0})";
                    selsql = string.Format(selsql, str.TrimEnd(','));
                    List<KeyValuePair<string, string>> listOrg = new List<KeyValuePair<string, string>>();
                    var reader = DBUtils.ExecuteReader(ctx, selsql, par);
                    while (reader.Read())
                    {
                        listOrg.Add(new KeyValuePair<string, string>(reader["FNUMBER"].ToString(), reader["FNAME"].ToString()));
                    }
                    reader.Close();
                    var orgNumber = listOrg.Select(it => it.Key).Except(req.Select(it => it.OrgNumber));
                    if (orgNumber.Any())
                    {
                        var data = string.Join(",", listOrg.Where(it => orgNumber.Contains(it.Key)));
                        response.Code = ResponseCode.ModelError;
                        response.Message = $"业务员({TransferUserCode})在组织({data})中不存在";
                        return response;
                    }
                    int inx = 0;
                    foreach (var item in req)
                    {
                        string updateSaleOrderSql = $@"/*dialect*/update t set t.FSALEDEPTID=@saleDeptId{inx},t.FSALERID=@salerId{inx},t.FSALEGROUPID=@saleGroupId{inx}
										from T_SAL_ORDER t
                                        left join T_ORG_ORGANIZATIONS s on t.FSALEORGID = s.FORGID
                                        where  FCLOSESTATUS='A' and FCANCELSTATUS='A' and s.FNUMBER =@fnumber{inx} and FBILLNO in( ";
                        par.Add(new SqlParam($"@saleDeptId{inx}", KDDbType.Int32, item.DeptId));
                        par.Add(new SqlParam($"@salerId{inx}", KDDbType.Int32, item.SalesUserId));
                        par.Add(new SqlParam($"@saleGroupId{inx}", KDDbType.Int32, item.SaleGroupId));
                        par.Add(new SqlParam($"@fnumber{inx}", KDDbType.String, item.OrgNumber));

                        updateSaleOrderSql += str.TrimEnd(',') + ")";

                        List<KeyValuePair<int, int>> salesOrder = new List<KeyValuePair<int, int>>();
                        DBUtils.Execute(ctx, updateSaleOrderSql, par);
                        inx++;
                    }
                }


                billView.CommitNetworkCtrl();
                billView.InvokeFormOperation(FormOperationEnum.Close);
                billView.Close();
                //string salesSql = @"select top 1 a.fid from V_BD_SALESMAN a
                //                        left join V_BD_SALESMAN_L b on a.fid= b.fid
                //                        left join T_HR_EMPINFO_L c on c.FNAME = b.FNAME
                //                        left join T_HR_EMPINFO d on c.FID = d.FID and d.FDOCUMENTSTATUS='C' and d.FFORBIDSTATUS='A'
                //                        left join T_ORG_ORGANIZATIONS e on a.FBIZORGID = e.FORGID
                //                        where e.FNUMBER = 'SZMYGC'
                //                        and a.FFORBIDSTATUS='A' and a.FDOCUMENTSTATUS='C' and FFORBIDDENSTATUS=0
                //                        and d.FWECHATCODE=@Code and a.FISUSE=1";
                //List<SqlParam> par2 = new List<SqlParam>();
                //par2.Add(new SqlParam("@Code", KDDbType.String, userCode));
                //var salesUserId = DBUtils.ExecuteScalar(ctx, salesSql, 0, par2.ToArray());
                //string saleorderSql = @"select FID,FSALEORGID from T_SAL_ORDER t
                //                        left join T_ORG_ORGANIZATIONS s on t.FSALEORGID = s.FORGID
                //                        where FCUSTID=@custId and FCLOSESTATUS='A' and FCANCELSTATUS='A' and s.FNUMBER = 'SZMYGC' ";
                //List<SqlParam> par = new List<SqlParam>();
                //par.Add(new SqlParam("@custId", KDDbType.Int32, custId));
                //List<KeyValuePair<int, int>> salesOrder = new List<KeyValuePair<int, int>>();
                //using (var salesreader = DBUtils.ExecuteReader(ctx, saleorderSql, par))
                //{
                //    while (salesreader.Read())
                //    {
                //        salesOrder.Add(new KeyValuePair<int, int>(Convert.ToInt32(salesreader["FSALEORGID"]), Convert.ToInt32(salesreader["FID"])));
                //    }
                //}
                //foreach (var item in salesOrder)
                //{

                //    var salesbillView = FormMetadataUtils.CreateBillView(ctx, "SAL_SaleOrder", item.Value);
                //    salesbillView.Model.SetValue("FSaleGroupId", 0);
                //    salesbillView.Model.SetValue("FSaleDeptId", 0);
                //    salesbillView.Model.SetValue("FSalerId", salesUserId);
                //    salesbillView.InvokeFieldUpdateService("FSalerId", 0);
                //    var oper3 = SaveBill(ctx, salesbillView.BusinessInfo, new DynamicObject[] { salesbillView.Model.DataObject });
                //    if (!oper3.IsSuccess)
                //    {
                //        if (oper3.ValidationErrors.Count > 0)
                //        {
                //            response.Message += string.Join(";", oper3.ValidationErrors.Select(p => p.Message));
                //            response.Code = ResponseCode.ModelError;
                //            return response;
                //        }
                //        else
                //        {
                //            response.Message += string.Join(";", oper3.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                //            response.Code = ResponseCode.ModelError;
                //            return response;
                //        }
                //    }

                //}
                cope.Complete();
            }
            response.Code = ResponseCode.Success;
            return response;

        }

        /// <summary>
        /// 根据企业微信查找业务员Id
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgId"></param>
        /// <param name="weChatCode">企业微信编码</param>
        /// <returns></returns>
        public long GetSaleId(Context ctx, long orgId, string weChatCode)
        {
            var sql = @"SELECT B.FENTRYID
                        FROM T_BD_OPERATOR A 
	                        INNER JOIN T_BD_OPERATORENTRY B ON A.FOPERATORID = B.FOPERATORID 
	                        INNER JOIN T_BD_STAFF C ON B.FSTAFFID = C.FSTAFFID 
	                        inner join T_HR_EMPINFO s on c.FEMPINFOID = s.FID
                        WHERE B.FOPERATORTYPE = 'XSY' and b.FFORBIDDENSTATUS = '0' and B.FBIZORGID = @FBIZORGID and s.FWECHATCODE = @FWECHATCODE";

            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, new SqlParam("@FBIZORGID", KDDbType.Int64, orgId), new SqlParam("@FWECHATCODE", KDDbType.String, weChatCode));
        }

        /// <summary>
        /// 新增或修改客户信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> AddOrMotifyCustomer(Context ctx, CustomerRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.Code, ""));
            if (cust.Id == 0)
            {
                ///新增客户信息
                using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer");
                    billView.Model.SetValue("FNumber", request.Code.Replace(" ", "").Replace("\r\n", "").Replace("\t", ""));
                    billView.Model.SetValue("FName", request.Name);
                    billView.Model.SetValue("FADDRESS", request.Address);
                    billView.Model.SetValue("FSOCIALCRECODE", request.BusinessLicenceCode);
                    billView.Model.SetValue("FLegalPerson", request.Corporation);
                    billView.Model.SetValue("FRegisterFund", request.RegisteredCapital);
                    billView.Model.SetValue("FDecimalPlacesOfUnitPrice", request.DecimalPlacesOfUnitPrice);
                    billView.Model.SetValue("FPENYCustomerLevel", request.CustomerLevel);

					billView.Model.SetValue("FSpecialDelivery", request.SpecialDelivery);
					billView.Model.SetValue("FPackagingReq", request.PackagingReq);

					List<DynamicObject> dynamicObjects = new List<DynamicObject>();
                    dynamicObjects.Add(billView.Model.DataObject);
                    var oper = SaveAndAuditBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            response.Code = ResponseCode.ModelError;
                            return response;
                        }
                        else
                        {
                            response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            response.Code = ResponseCode.ModelError;
                            return response;
                        }
                    }
                    if (request.IsAddLink)
                    {
                        var cust2 = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.Code, ""));
                        if (cust2.Id > 0)
                        {
                            //新增联系人 
                            var linkBillView = FormMetadataUtils.CreateBillView(ctx, "BD_CustContact");
                            linkBillView.Model.SetValue("FNumber", string.IsNullOrWhiteSpace(request.LinkCode) ? request.Code : request.Code + "_" + request.LinkCode);
                            linkBillView.Model.SetValue("FName", request.Linkman);
                            linkBillView.Model.SetValue("FBizAddress", request.Address);
                            linkBillView.Model.SetValue("FMobile", request.Mobile);
                            linkBillView.Model.SetValue("FCompanyType", "BD_Customer");
                            linkBillView.Model.SetValue("FForbidStatus", request.IsValid ? "A" : "B");
                            linkBillView.Model.SetValue("FCustId", cust2.Id);
                            linkBillView.Model.SetValue("FCompany", cust2.Id);
                            linkBillView.Model.SetValue("Fex", request.Sex == "男" ? "eb28b65afd62405299f061c9173a81de" : "b589b771e6004d2282fddb03accc7b43");

                            List<DynamicObject> dynamics = new List<DynamicObject>();
                            dynamics.Add(linkBillView.Model.DataObject);
                            var oper2 = SaveBill(ctx, linkBillView.BusinessInfo, dynamics.ToArray());
                            //清除释放网控
                            linkBillView.CommitNetworkCtrl();
                            linkBillView.InvokeFormOperation(FormOperationEnum.Close);
                            linkBillView.Close();
                            if (!oper2.IsSuccess)
                            {
                                if (oper2.ValidationErrors.Count > 0)
                                {
                                    response.Message += string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                                    response.Code = ResponseCode.ModelError;
                                    return response;
                                }
                                else
                                {
                                    response.Message += string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                    response.Code = ResponseCode.ModelError;
                                    return response;
                                }
                            }
                        }
                    }
                    cope.Complete();
                    response.Code = ResponseCode.Success;
                    response.Data = string.Empty;
                    return response;
                }
            }
            else
            {
                using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer", cust.Id.ToString());
                    billView.Model.SetValue("FName", request.Name);
                    billView.Model.SetValue("FADDRESS", request.Address);
                    billView.Model.SetValue("FSOCIALCRECODE", request.BusinessLicenceCode);
                    billView.Model.SetValue("FLegalPerson", request.Corporation);
                    billView.Model.SetValue("FRegisterFund", request.RegisteredCapital);
                    billView.Model.SetValue("FZIP", request.ZipCode);
                    billView.Model.SetValue("FDecimalPlacesOfUnitPrice", request.DecimalPlacesOfUnitPrice);
                    billView.Model.SetValue("FPENYCustomerLevel", request.CustomerLevel);

					billView.Model.SetValue("FSpecialDelivery", request.SpecialDelivery);
					billView.Model.SetValue("FPackagingReq", request.PackagingReq);

					List<DynamicObject> list = new List<DynamicObject>();
                    list.Add(billView.Model.DataObject);
                    var oper = SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            response.Code = ResponseCode.ModelError;
                            return response;
                        }
                        else
                        {
                            response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            response.Code = ResponseCode.ModelError;
                            return response;
                        }
                    }
                    cope.Complete();
                    response.Code = ResponseCode.Success;
                    response.Data = string.Empty;
                    return response;
                }
            }
        }

        /// <summary>
        /// 新增或修改供应商信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> AddOrMotifySupplier(Context ctx, SupplierRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            int supplierId = 0;
            string sql = "select top 1 FSupplierId from t_BD_Supplier where FNUMBER=@code and FCREATEORGID=1 and FUSEORGID=1 ";
            supplierId = DBUtils.ExecuteScalar(ctx, sql, 0, new SqlParam("@code", KDDbType.String, request.Code));
            int PayId = 0;
            if (!string.IsNullOrWhiteSpace(request.PayMethod))
            {
                string sql2 = @"select * from T_BD_PaymentCondition where FNUMBER=@Number";
                PayId = DBUtils.ExecuteScalar(ctx, sql2, 0, new SqlParam("@Number", KDDbType.String, request.PayMethod));
            }
            int TaxId = 0;
            if (!string.IsNullOrWhiteSpace(request.VatCode))
            {
                string Taxsql = "select FID from T_BD_TAXRATE where FNUMBER=@Number";
                TaxId = DBUtils.ExecuteScalar(ctx, Taxsql, 0, new SqlParam("@Number", KDDbType.String, request.VatCode));
            }

            if (supplierId == 0)
            {

                var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier");
                //创建和使用组织固定是范思德
                billView.Model.SetValue("FCREATEORGID", 1);
                billView.Model.SetValue("FUSEORGID", 1);
                billView.Model.SetValue("FNumber", request.Code);
                billView.Model.SetValue("FName", request.Name);
                billView.Model.SetValue("FAddress", request.RegisterAddress);
                billView.Model.SetValue("FWebSite", request.InternetAddress);
                billView.Model.SetValue("FSOCIALCRECODE", request.BusinessLicenceCode);
                billView.Model.SetValue("FLegalPerson", request.Corporation);
                billView.Model.SetValue("FRegisterFund", Regex.Replace(request.RegisteredCapital, @"[^0-9]+", ""));
                billView.Model.SetValue("FRegisterCode", request.BusinessRegistrationNo);
                billView.Model.SetValue("FSupplyClassify", request.SupplierType);
                billView.Model.SetValue("FInvoiceType", request.InvoiceType);
                billView.Model.SetValue("FTaxRateId", TaxId);
                billView.Model.SetValue("FPayCondition", PayId);
                billView.Model.SetValue("FIsTemporary", request.IsTemporary ? "1" : "0");

                if (request.SupplierContact != null)
                {
                    billView.Model.CreateNewEntryRow("FSupplierContact");
                    billView.Model.SetValue("FCONTACT", request.SupplierContact.Name, 0);
                    billView.Model.SetValue("FPost", request.SupplierContact.Post, 0);
                    billView.Model.SetValue("FMobile", request.SupplierContact.Mobile, 0);
                    billView.Model.SetValue("FEMail", request.SupplierContact.Email, 0);
                    billView.Model.SetValue("FFax", request.SupplierContact.Fax, 0);
                    billView.Model.SetValue("FTel", request.SupplierContact.Tel, 0);
                    billView.Model.SetValue("FGender", request.SupplierContact.Sex == "1" ? "eb28b65afd62405299f061c9173a81de" : "b589b771e6004d2282fddb03accc7b43");
                    billView.Model.SetValue("FContactNumber", request.SupplierContact.Id);
                }
                //if (request.SupplierBank != null)
                //{
                //    billView.Model.SetValue("FOpenBankName", request.SupplierBank.BankName, 0);
                //    billView.Model.SetValue("FBANKCODE", request.SupplierBank.BankCode.Replace(" ", "").Replace("\r\n", "").Replace("\t", ""), 0);
                //}
                List<DynamicObject> dynamicObjects = new List<DynamicObject>();
                dynamicObjects.Add(billView.Model.DataObject);
                var oper = SaveAndAuditBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
                //清除释放网控
                billView.CommitNetworkCtrl();
                billView.InvokeFormOperation(FormOperationEnum.Close);
                billView.Close();
                if (!oper.IsSuccess)
                {
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                    else
                    {
                        response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                }
                response.Code = ResponseCode.Success;
                response.Data = string.Empty;

                //发送MES
                SupplyOrgService supplyOrgService = new SupplyOrgService();
                supplyOrgService.SenMesSupplyInfo(ctx, request.Code);
                return response;
            }
            else
            {
                using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier", supplierId);

                    billView.Model.SetValue("FName", request.Name);
                    billView.Model.SetValue("FRegisterAddress", request.RegisterAddress);
                    billView.Model.SetValue("FWebSite", request.InternetAddress);
                    billView.Model.SetValue("FSOCIALCRECODE", request.BusinessLicenceCode);
                    billView.Model.SetValue("FLegalPerson", request.Corporation);
                    billView.Model.SetValue("FRegisterFund", Regex.Replace(request.RegisteredCapital, @"[^0-9]+", ""));
                    billView.Model.SetValue("FRegisterCode", request.BusinessRegistrationNo);
                    billView.Model.SetValue("FSupplyClassify", request.SupplierType);
                    billView.Model.SetValue("FInvoiceType", request.InvoiceType);
                    billView.Model.SetValue("FPayCondition", PayId);
                    billView.Model.SetValue("FTaxRateId", TaxId);
                    billView.Model.SetValue("FIsTemporary", request.IsTemporary ? "1" : "0");
                    List<DynamicObject> list = new List<DynamicObject>();
                    list.Add(billView.Model.DataObject);
                    var oper = SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            response.Code = ResponseCode.ModelError;
                            return response;
                        }
                        else
                        {
                            response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            response.Code = ResponseCode.ModelError;
                            return response;
                        }
                    }
                    cope.Complete();
                    response.Code = ResponseCode.Success;
                    response.Data = string.Empty;
                }
                //发送MES
                SupplyOrgService supplyOrgService = new SupplyOrgService();
                supplyOrgService.SenMesSupplyInfo(ctx, request.Code);
                return response;
            }
        }


        /// <summary>
        /// 供应商分配组织
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> AddSupplierAllotOrg(Context ctx, List<SupplierAllotOrgRequest> request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            List<OrgAccountSystemEntity> accountList = new List<OrgAccountSystemEntity>();
            long orgSupplierId = 0;
            foreach (var item in request)
            {
                //1.获取总公司供应商ID
                string sql = "/*dialect*/select top 1 FSupplierId from t_BD_Supplier where FNUMBER=@code and FUSEORGID=1 and FFORBIDSTATUS='A' and FDOCUMENTSTATUS='C' ";
                orgSupplierId = DBUtils.ExecuteScalar(ctx, sql, 0, new SqlParam("@code", KDDbType.String, item.Code));
                if (orgSupplierId == 0)
                {
                    response.Message = $"范思德不存在该供应商";
                    response.Code = ResponseCode.ModelError;
                    return response;
                }
                //2.获取组织ID
                sql = @"/*dialect*/select top 1 FORGID from T_ORG_ORGANIZATIONS where FFORBIDSTATUS='A' and FNUMBER=@companyCode ";
                long orgId = DBUtils.ExecuteScalar(ctx, sql, 0, new SqlParam("@companyCode", KDDbType.String, item.CompanyCode));
                if (orgId == 0)
                {
                    response.Message = $"组织机构[{item.CompanyCode}]不存在或者已作废";
                    response.Code = ResponseCode.ModelError;
                    return response;
                }
                //3.获取会计核算体系
                sql = $@"/*dialect*/select distinct tt1.FORGID,tt3.FNAME from T_ORG_ORGANIZATIONS tt1
                    inner join (
                   select FSUBORGID OrgID from T_ORG_ACCTSYSDETAIL where FSUBORGID={orgId}
					union
					select FMAINORGID OrgID from T_ORG_ACCTSYSDETAIL t1
					inner join T_ORG_ACCTSYSENTRY t2 on t1.FENTRYID=t2.FENTRYID
					where FSUBORGID={orgId} ) tt2 
                    on  tt1.FORGID=tt2.OrgID
                    inner join T_ORG_ORGANIZATIONS_L tt3 on tt1.FORGID = tt3.FORGID and tt3.FLOCALEID = 2052
                    where tt1.FFORBIDSTATUS='A' ";
                var accountDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                foreach (var data in accountDatas)
                {
                    accountList.Add(new OrgAccountSystemEntity
                    {
                        OrgId = Convert.ToInt64(data["FORGID"]),
                        OrgName = Convert.ToString(data["FNAME"]),
                    });
                }
                if (accountList.Count == 0)
                {
                    response.Message = $"会计核算体系不存在";
                    response.Code = ResponseCode.ModelError;
                    return response;
                }

                //4.基础资料控制策略
                List<OrgEntity> orgList = new List<OrgEntity>();
                var orgSql = $@"/*dialect*/select e.FTARGETORGID,ol.FNAME
                            from T_ORG_BDCtrlPolicy o
	                        inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
	                        inner join T_ORG_ORGANIZATIONS org  on e.FTARGETORGID=org.FORGID
	                        inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
                            where o.FBASEDATATYPEID = 'BD_Supplier' and o.FCREATEORGID=1 and org.FFORBIDSTATUS='A'  ";
                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, orgSql);
                foreach (var data in datas)
                {
                    orgList.Add(new OrgEntity
                    {
                        OrgId = Convert.ToString(data["FTARGETORGID"]),
                        OrgName = Convert.ToString(data["FNAME"]),
                    });
                }
                if (orgList.Count == 0)
                {
                    response.Message = $"范思德的基础资料控制策略不存在";
                    response.Code = ResponseCode.ModelError;
                    return response;
                }

                //5.验证会计核算体系是否存在基础资料控制策略
                foreach (var itemacc in accountList)
                {
                    var orgInfo = orgList.FirstOrDefault(x => x.OrgId.Equals(itemacc.OrgId.ToString()));
                    if (orgInfo == null)
                    {
                        response.Message = $"范思德的基础资料控制策略不存在[{itemacc.OrgName}]";
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                }


            }

            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_Supplier") as FormMetadata;
            var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier");
            AllocateService allocateService = new AllocateService();
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                //6.开始分配供应商去重复
                foreach (var item in accountList.GroupBy(p => p.OrgId).Select(q => q.First()).ToList())
                {
                    //判断当前组织是否已经存在该供应商
                    List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@code", KDDbType.String, request.FirstOrDefault().Code), new SqlParam("@orgId", KDDbType.String, item.OrgId) };
                    int supplierId = 0;
                    var sql = @"select top 1 t1.FSUPPLIERID from T_BD_SUPPLIER t1 where t1.FNUMBER=@code and t1.FUSEORGID=@orgId and t1.FFORBIDSTATUS='A' ";
                    supplierId = DBUtils.ExecuteScalar(ctx, sql, 0, paramList: pars.ToArray());
                    if (supplierId == 0)
                    {
                        AllocateParameter allocateParameter = new AllocateParameter(billView.BusinessInfo, meta.InheritPath, 1, BOS.Core.Enums.BOSEnums.Enu_AllocateType.Allocate, BOS.Core.OperationNumberConst.OperationNumber_Allocate)
                        {
                            PkId = new List<object>() { orgSupplierId },
                            AutoSubmitAndAudit = true,
                            AllocateUserId = ctx.UserId,
                            DestOrgId = item.OrgId,
                            DestOrgName = item.OrgName
                        };
                        var oper = allocateService.Allocate(ctx, allocateParameter);
                        //清除释放网控
                        billView.CommitNetworkCtrl();
                        billView.InvokeFormOperation(FormOperationEnum.Close);
                        billView.Close();
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
                cope.Complete();
            }
            //发送MES
            SupplyOrgService supplyOrgService = new SupplyOrgService();
            supplyOrgService.SenMesSupplyInfo(ctx, request.FirstOrDefault().Code);
            response.Code = ResponseCode.Success;
            response.Data = string.Empty;
            return response;
        }

        /// <summary>
        /// 获取当前需要分配的供应商
        /// </summary>
        /// <returns></returns>
        public List<SupplierAllotOrgRequest> GetAllotOrgSupplier(Context ctx)
        {
            List<SupplierAllotOrgRequest> list = new List<SupplierAllotOrgRequest>();
            string sql = @"/*dialect*/select t1.FSETTLEORGID,t3.FNUMBER OrgCode,t2.FNAME orgName,t1.FSUPPLIERID,t4.FNUMBER SupplierCode from (
                            select distinct t1.FSETTLEORGID,t1.FSUPPLIERID from T_AP_PAYABLE t1 where t1.FSETTLEORGID<>1
                            union
                            select distinct t1.FPAYORGID,t1.FCONTACTUNIT from T_AP_PAYBILL t1 where  t1.FCONTACTUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            union
                            select distinct t1.FPAYORGID,t1.FRECTUNIT from T_AP_PAYBILL t1 where  t1.FRECTUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            union
                            select distinct t1.FPAYORGID,t1.FCONTACTUNIT from T_AP_REFUNDBILL t1 where  t1.FCONTACTUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            union
                            select distinct t1.FPAYORGID,t1.FPAYUNIT from T_AP_REFUNDBILL t1 where  t1.FPAYUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            union
                            select distinct t1.FCONTACTUNIT,t1.FPAYORGID from T_CN_PAYAPPLY t1 where  t1.FCONTACTUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            union
                            select distinct t1.FRECTUNIT,t1.FPAYORGID from T_CN_PAYAPPLY t1 where  t1.FRECTUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            union
                            select distinct t1.FPAYORGID,t2.FCONTACTUNIT from T_AP_Matck t1
                            inner join T_AP_MatckEntry t2 on t1.FID=t2.FID
                            where t2.FCONTACTUNITTYPE='BD_Supplier' and t1.FPAYORGID<>1
                            ) t1 
                            inner join t_org_organizations_l t2  on t1.FSETTLEORGID=t2.FORGID and t2.FLOCALEID=2052
							inner join t_org_organizations t3 on t2.FORGID=t3.FORGID 
							inner join T_BD_Supplier t4 on t4.FSUPPLIERID=t1.FSUPPLIERID where t3.FFORBIDSTATUS='A' and t4.FFORBIDSTATUS='A' and t1.FSETTLEORGID<>4093663 ";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            foreach (var data in datas)
            {
                list.Add(new SupplierAllotOrgRequest
                {
                    Code = Convert.ToString(data["SupplierCode"]),
                    CompanyCode = Convert.ToString(data["OrgCode"]),
                });
            }
            return list;
        }

        /// <summary>
        /// 删除重复客户
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> deleteCust(Context ctx)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            string sql = $@"/*dialect*/select * from (select row_number() over (partition by FNUMBER order by FNUMBER,FCUSTID asc) i,FCUSTID,FNUMBER 
                            from T_BD_CUSTOMER where FNUMBER in(
                            select FNUMBER from T_BD_CUSTOMER
                            group by FNUMBER
                            having count(1)>1)
                            and FFORBIDSTATUS='A') t1 where i>1  ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_Customer") as FormMetadata;
            List<object> ids = new List<object>();
            int i = 1;
            foreach (var data in datas)
            {

                ids.Add(data["FCUSTID"]);

                if (i % 100 == 0)
                {
                    var oper = UnAudit(ctx, meta.BusinessInfo, ids.ToArray());
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                        }
                        else
                        {
                            response.Message = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        }
                        //return response;
                    }
                    var oper2 = DeleteBill(ctx, meta.BusinessInfo, ids.ToArray());
                    if (!oper2.IsSuccess)
                    {
                        if (oper2.ValidationErrors.Count > 0)
                        {
                            response.Message = string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                        }
                        else
                        {
                            response.Message = string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        }
                        //return response;
                    }
                    ids.Clear();
                    i = 1;
                }
                i += 1;
            }

            response.Code = ResponseCode.Success;
            response.Data = string.Empty;
            return response;
        }

        private IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
        {
            SaveService saveService = new SaveService();
            IOperationResult operationResult;
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            operationResult = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);
            return operationResult;
        }
        private IOperationResult SaveAndAuditBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
        {
            SaveService saveService = new SaveService();
            IOperationResult operationResult;
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            operationResult = saveService.SaveAndAudit(ctx, businessInfo, dynamicObjects, operateOption);
            return operationResult;
        }
        private IOperationResult DeleteBill(Context ctx, BusinessInfo businessInfo, object[] ids)
        {
            DeleteService deleteService = new DeleteService();
            IOperationResult operationResult;
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            operationResult = deleteService.Delete(ctx, businessInfo, ids, operateOption);
            return operationResult;
        }
        private IOperationResult UnAudit(Context ctx, BusinessInfo businessInfo, object[] ids)
        {
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
            SetStatusService setStatusService = new SetStatusService();
            return setStatusService.SetBillStatus(ctx, businessInfo, pkEntryIds, null, FormOperationEnum.UnAudit.ToString(), operateOption);
        }


        /// <summary>
        /// CRM同步客诉到金蝶
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SynCompanyComplaintService(Context ctx, CompanyComplaintRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                var pars = new List<SqlParam>() {
                new SqlParam("@ComplaintNo", KDDbType.String, request.ComplaintNo),
                new SqlParam("@ComplaintDate", KDDbType.DateTime, request.ComplaintDate),
                new SqlParam("@SaleMan", KDDbType.String, request.SaleMan),
                new SqlParam("@CompanyCode", KDDbType.String, request.CompanyCode),
                new SqlParam("@CompanyName", KDDbType.String, request.CompanyName),
                new SqlParam("@AntComplaintModel", KDDbType.String, request.AntComplaintModel),
                new SqlParam("@ComplaintProduct", KDDbType.String, request.ComplaintProduct),
                new SqlParam("@CustComplaintModel", KDDbType.String, request.CustComplaintModel),
                new SqlParam("@ComplaintOrder", KDDbType.String, request.ComplaintOrder),
                new SqlParam("@OrderDate", KDDbType.DateTime, request.OrderDate),
                new SqlParam("@ComplaintDetailContent", KDDbType.String, request.ComplaintDetailContent),
                new SqlParam("@CreateOn", KDDbType.DateTime, request.CreateOn),
                new SqlParam("@CreateByName", KDDbType.String, request.CreateByName),
                new SqlParam("@SmallClassId", KDDbType.Int64, request.ProductSmallClassId)
                };
                var sql = $@"select top 1 FComplaintNo from PENY_T_CompanyComplaint where FComplaintNo=@ComplaintNo ";
                var complaintNo = DBServiceHelper.ExecuteScalar<string>(ctx, sql, "", paramList: pars.ToArray());
                if (string.IsNullOrEmpty(complaintNo))
                {
                    using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        sql = $@"/*dialect*/insert into PENY_T_CompanyComplaint(FComplaintNo,FComplaintDate,FSaleMan,FCompanyCode,FCompanyName,
									FAntComplaintModel,FComplaintProduct,FCustComplaintModel, 
									FComplaintOrder,FOrderDate,FComplaintDetailContent,FCreateOn,FCreateByName,FSynCreateOn,FSmallClassId)
									values(@ComplaintNo,@ComplaintDate,@SaleMan,@CompanyCode,@CompanyName,@AntComplaintModel,@ComplaintProduct,@CustComplaintModel, 
									@ComplaintOrder,@OrderDate,@ComplaintDetailContent,@CreateOn,@CreateByName,getdate(),@SmallClassId)";

                        int i = DBServiceHelper.Execute(ctx, sql, pars);
                        if (i > 0)
                        {
                            //根据客诉的蚂蚁物料扣30天内采购订单对应型号的全部供应商），-2分 
                            //1.获取存在采购的组织和供应商，大小类
                            sql = $@"/*dialect*/select * from (
										select t1.FBILLNO,t1.FPURCHASEORGID,t1.FSUPPLIERID,t2.FSEQ,t2.FPARENTSMALLID,t2.FSMALLID,
										ROW_NUMBER() OVER (PARTITION BY t1.FPURCHASEORGID,t1.FSUPPLIERID ORDER BY t1.FDATE DESC) AS rn
										from T_PUR_POORDER t1
										inner join T_PUR_POORDERENTRY t2 on t1.FID=t2.FID
										inner join T_BD_MATERIAL t3 on t2.FMATERIALID=t3.FMATERIALID
										where t1.FPURCHASEORGID in (7401780,7401781) and t1.FDATE>=DATEADD(DAY,-30,getdate()) 
										and t3.FNUMBER=@AntComplaintModel and t1.FDOCUMENTSTATUS='C'
										) datas where rn=1 ";
                            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                            foreach (var data in datas)
                            {
                                pars = new List<SqlParam>() {
                                new SqlParam("@PurchaseOrgId", KDDbType.Int64, Convert.ToInt64(data["FPURCHASEORGID"])),
                                new SqlParam("@SupplierId", KDDbType.Int64, Convert.ToInt64(data["FSUPPLIERID"])),
                                new SqlParam("@ParentSmallId", KDDbType.Int64, Convert.ToInt64(data["FPARENTSMALLID"])),
                                new SqlParam("@SmallId", KDDbType.Int64, Convert.ToInt64(data["FSMALLID"]))
                                };

                                //2.判断是否存在供应商小类检验评分
                                sql = $@"/*dialect*/select top 1 * from PENY_T_SupplierClassInspectScore where FORGID=@PurchaseOrgId and FSUPPLIERID=@SupplierId and FParentSmallId=@ParentSmallId and FSmallId=@SmallId ";
                                var inspectScoreData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                                if (inspectScoreData.Count() == 0)
                                {
                                    //不存在就添加
                                    sql = $@"/*dialect*/insert into PENY_T_SupplierClassInspectScore(FORGID,FSUPPLIERID,FParentSmallId,FSmallId,FFraction,FStringencyId,FMODIFYDATE,FCREATEDATE)
											values (@PurchaseOrgId,@SupplierId,@ParentSmallId,@SmallId,0,2,getdate(),getdate())";
                                    DBServiceHelper.Execute(ctx, sql, pars);
                                    //获取检验评分数据
                                    sql = $@"/*dialect*/select top 1 * from PENY_T_SupplierClassInspectScore where FORGID=@PurchaseOrgId and FSUPPLIERID=@SupplierId and FParentSmallId=@ParentSmallId and FSmallId=@SmallId ";
                                    inspectScoreData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                                }
                                decimal oldFraction = Convert.ToDecimal(inspectScoreData[0]["FFraction"]);
                                decimal newFraction = oldFraction - 2;
                                pars.Add(new SqlParam("@Id", KDDbType.Int64, Convert.ToInt64(inspectScoreData[0]["FID"])));
                                pars.Add(new SqlParam("@OldFraction", KDDbType.Decimal, oldFraction));
                                pars.Add(new SqlParam("@OldStringencyId", KDDbType.Int32, Convert.ToInt32(inspectScoreData[0]["FStringencyId"])));
                                pars.Add(new SqlParam("@Fraction", KDDbType.Decimal, -2));//客诉扣2分
                                pars.Add(new SqlParam("@NewFraction", KDDbType.Decimal, newFraction));
                                pars.Add(new SqlParam("@Node", KDDbType.String, $"客诉单号[{request.ComplaintNo}],物料编码[{request.AntComplaintModel}]，采购订单[{Convert.ToString(data["FBILLNO"])}-{Convert.ToString(data["FSEQ"])}]"));

                                //重新计算严格度
                                int newStringencyId = 0;
                                //< 0 全检（4） 无下限；
                                //>= 0 & < 6 加严检（2）；
                                //>= 6 & < 9 正常检（1）；
                                //>= 9 免检（5） 10分封顶
                                if (newFraction < 0)
                                {
                                    newStringencyId = 4;
                                }
                                else if (newFraction < 6)
                                {
                                    newStringencyId = 2;
                                }
                                else if (newFraction < 9)
                                {
                                    newStringencyId = 1;
                                }
                                pars.Add(new SqlParam("@NewStringencyId", KDDbType.Int32, newStringencyId));
                                //3.表头扣分和更新严格度
                                sql = $@"/*dialect*/update PENY_T_SupplierClassInspectScore set FFraction=@NewFraction,FStringencyId=@NewStringencyId,FMODIFYDATE=getdate() where FID=@Id";
                                DBServiceHelper.Execute(ctx, sql, pars);
                                //4.添加扣分明细
                                sql = $@"/*dialect*/insert into PENY_T_InspectScoreChangeDetails(FParentId,FORGID,FSUPPLIERID,FParentSmallId,FSmallId,FOldFraction,FFraction,FNewFraction,FOldStringencyId,FNewStringencyId,FractionSource,FNode,FCREATEDATE)
								values(@Id,@PurchaseOrgId,@SupplierId,@ParentSmallId,@SmallId,@OldFraction,@Fraction,@NewFraction,@OldStringencyId,@NewStringencyId,'Complaint',@Node,getdate())";
                                DBServiceHelper.Execute(ctx, sql, pars);
                            }
                        }
                        cope.Complete();

                    }
                }
                response.Message = "同步成功";
                response.Code = ResponseCode.Success;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// 周合格率统计
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> WeeklyPassRateStatisticsService(Context ctx)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                //6天内存在记录，则不更新
                var sql = @"/*dialect*/select Count(1) from PENY_T_InspectScoreChangeDetails where  FractionSource='WeekPassRate' and FCREATEDATE>=DATEADD(DAY,-6,getdate())";
                if (DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0) == 0)
                {
                    //获取严格度标准设置
                    sql = @"/*dialect*/select FStringencyId,FPassRateAlgorithm,FPassRateValue,FFraction from PENY_T_StrictnessStandardsSet where FFraction>0";
                    var stringencyData = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                    using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        //提前7天存在数据的才更新
                        sql = $@"/*dialect*/select distinct t1.FSTOCKORGID,t1.FSUPPLIERID,t2.FPARENTSMALLID,t2.FSMALLID  from T_PUR_RECEIVE t1
								inner join T_PUR_RECEIVEENTRY t2 on t1.FID=t2.FID
								inner join T_PUR_RECEIVEENTRY_R t3 on t3.FENTRYID=t2.FENTRYID
								where t1.FSTOCKORGID in (7401780,7401781) 
								and t1.FDOCUMENTSTATUS='C' and t3.FENTRYSTATUS='B'  
								and t1.FDATE>=DATEADD(DAY,-7,getdate())  ";
                        var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                        foreach (var data in datas)
                        {
                            var pars = new List<SqlParam>() {
                                new SqlParam("@OrgId", KDDbType.Int64, Convert.ToInt64(data["FSTOCKORGID"])),
                                new SqlParam("@SupplierId", KDDbType.Int64, Convert.ToInt64(data["FSUPPLIERID"])),
                                new SqlParam("@ParentSmallId", KDDbType.Int64, Convert.ToInt64(data["FPARENTSMALLID"])),
                                new SqlParam("@SmallId", KDDbType.Int64, Convert.ToInt64(data["FSMALLID"]))
                                };
                            //2.判断是否存在供应商小类检验评分
                            sql = $@"/*dialect*/select top 1 * from PENY_T_SupplierClassInspectScore where FORGID=@OrgId and FSUPPLIERID=@SupplierId and FParentSmallId=@ParentSmallId and FSmallId=@SmallId ";
                            var inspectScoreData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                            if (inspectScoreData.Count() == 0)
                            {
                                //不存在就添加
                                sql = $@"/*dialect*/insert into PENY_T_SupplierClassInspectScore(FORGID,FSUPPLIERID,FParentSmallId,FSmallId,FFraction,FStringencyId,FMODIFYDATE,FCREATEDATE)
											values (@OrgId,@SupplierId,@ParentSmallId,@SmallId,0,2,getdate(),getdate())";
                                DBServiceHelper.Execute(ctx, sql, pars);
                                //获取检验评分数据
                                sql = $@"/*dialect*/select top 1 * from PENY_T_SupplierClassInspectScore where FORGID=@OrgId and FSUPPLIERID=@SupplierId and FParentSmallId=@ParentSmallId and FSmallId=@SmallId ";
                                inspectScoreData = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
                            }
                            //当前严格度
                            int stringencyId = Convert.ToInt32(inspectScoreData[0]["FStringencyId"]);
                            //旧积分
                            decimal oldFraction = Convert.ToDecimal(inspectScoreData[0]["FFraction"]);
                            if (oldFraction >= 10)
                            {
                                continue;
                            }
                            //实际最终分值
                            decimal actualValue = 0;
                            //实际最终分值（全检、加严检、正常检）
                            if (stringencyId.Equals(1) || stringencyId.Equals(2) || stringencyId.Equals(4))
                            {
                                //合格率：入库数量/收料数量*100%
                                sql = $@"/*dialect*/select SUM(FINSTOCKQTY)/SUM(t2.FACTRECEIVEQTY)*100  from T_PUR_RECEIVE t1
								inner join T_PUR_RECEIVEENTRY t2 on t1.FID=t2.FID
								inner join T_PUR_RECEIVEENTRY_R t3 on t3.FENTRYID=t2.FENTRYID
								inner join T_PUR_RECEIVEENTRY_S t4 on t4.FENTRYID=t3.FENTRYID
								where t1.FSTOCKORGID =@OrgId and FSUPPLIERID=@SupplierId and t2.FPARENTSMALLID=@ParentSmallId and t2.FSMALLID=@SmallId
								and t1.FDOCUMENTSTATUS='C' and t3.FENTRYSTATUS='B'  
								and t1.FDATE>=DATEADD(DAY,-90,getdate()) ";
                                decimal thisPassRate = DBServiceHelper.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
                                actualValue = GetActualValue(thisPassRate, stringencyData.Where(x => Convert.ToInt32(x["FStringencyId"]).Equals(stringencyId)).FirstOrDefault());
                                pars.Add(new SqlParam("@Node", KDDbType.String, $"周期检验合格率[{thisPassRate}]"));
                            }
                            else if (stringencyId.Equals(5))
                            {
                                //免检的，看大小类是否包含客诉
                                actualValue = GetActualValueV2(ctx, Convert.ToInt64(data["FSMALLID"]), stringencyData.Where(x => Convert.ToInt32(x["FStringencyId"]).Equals(stringencyId)).FirstOrDefault());
                                if (actualValue > 0)
                                {
                                    pars.Add(new SqlParam("@Node", KDDbType.String, $"周统计无客诉加分"));
                                }
                            }
                            decimal newFraction = oldFraction + actualValue;
                            //10分封顶
                            if (actualValue != 0 && newFraction <= 10)
                            {
                                pars.Add(new SqlParam("@Id", KDDbType.Int64, Convert.ToInt64(inspectScoreData[0]["FID"])));
                                pars.Add(new SqlParam("@OldFraction", KDDbType.Decimal, oldFraction));
                                pars.Add(new SqlParam("@OldStringencyId", KDDbType.Int32, stringencyId));
                                pars.Add(new SqlParam("@Fraction", KDDbType.Decimal, actualValue));//实际加减分值
                                pars.Add(new SqlParam("@NewFraction", KDDbType.Decimal, newFraction));

                                //重新计算严格度
                                int newStringencyId = 0;
                                //< 0 全检（4） 无下限；
                                //>= 0 & < 6 加严检（2）；
                                //>= 6 & < 9 正常检（1）；
                                //>= 9 免检（5） 10分封顶
                                if (newFraction < 0)
                                {
                                    newStringencyId = 4;
                                }
                                else if (newFraction < 6)
                                {
                                    newStringencyId = 2;
                                }
                                else if (newFraction < 9)
                                {
                                    newStringencyId = 1;
                                }
                                else if (newFraction >= 9)
                                {
                                    newStringencyId = 5;
                                }
                                pars.Add(new SqlParam("@NewStringencyId", KDDbType.Int32, newStringencyId));
                                //3.表头扣分和更新严格度
                                sql = $@"/*dialect*/update PENY_T_SupplierClassInspectScore set FFraction=@NewFraction,FStringencyId=@NewStringencyId,FMODIFYDATE=getdate() where FID=@Id";
                                DBServiceHelper.Execute(ctx, sql, pars);
                                //4.添加扣分明细
                                sql = $@"/*dialect*/insert into PENY_T_InspectScoreChangeDetails(FParentId,FORGID,FSUPPLIERID,FParentSmallId,FSmallId,FOldFraction,FFraction,FNewFraction,FOldStringencyId,FNewStringencyId,FractionSource,FNode,FCREATEDATE)
								values(@Id,@OrgId,@SupplierId,@ParentSmallId,@SmallId,@OldFraction,@Fraction,@NewFraction,@OldStringencyId,@NewStringencyId,'WeekPassRate',@Node,getdate())";
                                DBServiceHelper.Execute(ctx, sql, pars);
                            }
                        }
                        cope.Complete();
                    }
                }
                response.Message = "同步成功";
                response.Code = ResponseCode.Success;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// 获取最终分值（全检、加严检、正常检）
        /// </summary>
        /// <param name="stringencyId">当前严格度</param>
        /// <param name="thisPassRate">合格率</param>
        /// <param name="dynamic">当前严格度标准设置</param>
        /// <returns></returns>
        private decimal GetActualValue(decimal thisPassRate, DynamicObject dynamic)
        {
            //实际最终分值
            decimal actualValue = 0;
            //合格率算法
            string passRateAlgorithm = Convert.ToString(dynamic["FPassRateAlgorithm"]);
            //合格率值
            decimal passRateValue = Convert.ToDecimal(dynamic["FPassRateValue"]);
            //分值
            decimal fraction = Convert.ToDecimal(dynamic["FFraction"]);

            if (passRateAlgorithm.Equals(">="))
            {
                if (thisPassRate >= passRateValue)
                {
                    //加分
                    actualValue = fraction;
                }
                else
                {
                    //减分
                    actualValue = -System.Math.Abs(fraction);
                }
            }
            else if (passRateAlgorithm.Equals(">"))
            {
                if (thisPassRate > passRateValue)
                {
                    //加分
                    actualValue = fraction;
                }
                else
                {
                    //减分
                    actualValue = -System.Math.Abs(fraction);
                }
            }
            else if (passRateAlgorithm.Equals("="))
            {
                if (thisPassRate == passRateValue)
                {
                    //加分
                    actualValue = fraction;
                }
                else
                {
                    //减分
                    actualValue = -System.Math.Abs(fraction);
                }
            }
            else if (passRateAlgorithm.Equals("<="))
            {
                if (thisPassRate <= passRateValue)
                {
                    //加分
                    actualValue = fraction;
                }
                else
                {
                    //减分
                    actualValue = -System.Math.Abs(fraction);
                }
            }
            else if (passRateAlgorithm.Equals("<"))
            {
                if (thisPassRate < passRateValue)
                {
                    //加分
                    actualValue = fraction;
                }
                else
                {
                    //减分
                    actualValue = -System.Math.Abs(fraction);
                }
            }
            return actualValue;
        }

        /// <summary>
        /// 获取最终分值（免检）
        /// </summary>
        /// <returns></returns>
        private decimal GetActualValueV2(Context ctx, long smallId, DynamicObject dynamic)
        {
            //实际最终分值
            decimal actualValue = 0;
            //分值
            decimal fraction = Convert.ToDecimal(dynamic["FFraction"]);
            //>= 9 免检（5） 10分封顶 期间无客诉记录 +0.5
            var sSql = $@"select count(1) from PENY_T_CompanyComplaint where FComplaintDate>=DATEADD(DAY,-90,getdate()) and FSmallClassId={smallId} ";
            if (DBServiceHelper.ExecuteScalar<int>(ctx, sSql, 0) == 0)
            {
                actualValue = fraction;
            }
            return actualValue;
        }

        /// <summary>
        /// 同步员工认岗业务员业务组信息
        /// </summary>
        public List<KeyValuePair<long, long>> SyncSAL_SC_SalerCust(Context ctx, List<SalerCustList> keys)
        {
            List<KeyValuePair<long, long>> salesId = new List<KeyValuePair<long, long>>();
            using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
            {
                foreach (var item in keys)
                {
                    string sSql = "SELECT FOPERATORID FROM T_BD_OPERATOR WHERE FOPERATORTYPE='XSY'";
                    var operatorid = DBUtils.ExecuteScalar<long>(ctx, sSql, 0, null);
                    //创建新的业务员
                    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_OPERATOR", operatorid);
                    List<DynamicObject> list = new List<DynamicObject>();
                    var salerow = ((DynamicObjectCollection)billView.Model.DataObject["BD_OPERATORENTRY"])
                        .Where(x => Convert.ToInt64(x["StaffId_Id"]) == item.Staffid).FirstOrDefault();
                    var entity = billView.Model.BillBusinessInfo.GetEntryEntity("FBD_OPERATORDETAILS") as SubEntryEntity;

                    item.Operatorgroupid = SalerGrop(ctx, item.UserCode, item.Bizorgid, item.BizOrgNumber, item.UserName);
                    //var rowcount = ((DynamicObjectCollection)salerow["BD_OPERATORDETAILS"]).Count;
                    //for (int i = rowcount; i >= 0; i--)
                    //{
                    //    var groupid = Convert.ToInt64(((DynamicObjectCollection)salerow["BD_OPERATORDETAILS"])[i - 1]["OperatorGroupId_Id"]);
                    //    if (groupid == 0)
                    //    {
                    //        billView.Model.DeleteEntryRow("BD_OPERATORDETAILS", i - 1);
                    //    }
                    //}

                    if (salerow != null)
                    {
                        var selsalerow = ((DynamicObjectCollection)salerow["BD_OPERATORDETAILS"]);

                        if (selsalerow.Where(x => Convert.ToInt64(x["OperatorGroupId_Id"]) == item.Operatorgroupid).ToList().Count > 0)
                        {
                            continue;
                        }
                        DynamicObject newRow = new DynamicObject(entity.DynamicObjectType);
                        newRow["OperatorGroupId_Id"] = item.Operatorgroupid;
                        newRow["IsDefault"] = 1;
                        ((DynamicObjectCollection)salerow["BD_OPERATORDETAILS"]).Add(newRow);
                    }
                    else
                    {
                        var entryrows = billView.Model.GetEntryRowCount("FEntity");
                        billView.Model.CreateNewEntryRow("FEntity");
                        billView.Model.SetValue("FOperatorType_ETY", "XSY", entryrows);
                        billView.Model.SetValue("FBizOrgId", item.Bizorgid, entryrows);
                        billView.Model.SetValue("FStaffId", item.Staffid, entryrows);

                        billView.Model.CreateNewEntryRow(salerow, entity, 0);
                        billView.Model.SetValue("FOperatorGroupId", item.Operatorgroupid, 0);
                        billView.Model.SetValue("FIsDefault", 1, 0);
                    }
                    list.Add(billView.Model.DataObject);

                    var oper2 = SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                    if (!oper2.IsSuccess)
                    {
                        if (oper2.ValidationErrors.Count > 0)
                        {
                            throw new Exception(string.Join(";", oper2.ValidationErrors.Select(p => p.Message)));
                        }
                        else
                        {
                            throw new Exception(string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                        }
                    }
                }
                cope.Complete();
            }
            salesId = keys.Select(t => new KeyValuePair<long, long>(t.Bizorgid, t.Operatorgroupid)).ToList();
            return salesId;
        }

        private int SalerGrop(Context ctx, string usercode, long orgid, string orgcode, string UserName)
        {
            int salerGroupid = 0;
            string sSql = $@"SELECT FENTRYID FROM T_BD_OPERATORGROUPENTRY 
            WHERE FOPERATORGROUPTYPE='XSZ' AND FBIZORGID={orgid} AND FNUMBER='{usercode + "_" + orgcode}' AND FISUSE=1";
            var operdatas = DBUtils.ExecuteDynamicObject(ctx, sSql);
            if (operdatas.Count() > 0)
            {
                salerGroupid = Convert.ToInt32(operdatas.FirstOrDefault()["FENTRYID"]);
            }
            else
            {
                //创建新的业务组
                var billView = FormMetadataUtils.CreateBillView(ctx, "BD_OPERATORGROUPBILL");
                List<DynamicObject> list = new List<DynamicObject>();
                var entryrows = billView.Model.GetEntryRowCount("FEntity");
                billView.Model.CreateNewEntryRow("FEntity");
                billView.Model.SetValue("FBizOrgId", orgid, entryrows);
                billView.Model.SetValue("FNumber", usercode + "_" + orgcode, entryrows);
                billView.Model.SetValue("FName", UserName + "_" + orgcode, entryrows);
                billView.Model.SetValue("FOperatorGroupType_EYR", "XSZ", entryrows);
                billView.Model.SetValue("FIsUse", 1, entryrows);
                list.Add(billView.Model.DataObject);

                IOperationResult oper2 = SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                if (!oper2.IsSuccess)
                {
                    if (oper2.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", oper2.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }
                sSql = $@"SELECT FENTRYID FROM T_BD_OPERATORGROUPENTRY 
            WHERE FOPERATORGROUPTYPE='XSZ' AND FBIZORGID={orgid} AND FNUMBER='{usercode + "_" + orgcode}' AND FISUSE=1";
                salerGroupid = DBUtils.ExecuteScalar<int>(ctx, sSql, 0, null);
            }
            return salerGroupid;
        }
    }
}
