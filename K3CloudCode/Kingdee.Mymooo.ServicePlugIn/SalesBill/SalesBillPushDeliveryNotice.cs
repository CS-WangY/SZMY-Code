using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{

    [Description("销售订单下推发货通知单插件（获取月结客户的信用额度）")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SalesBillPushDeliveryNotice : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] headEntitys = e.Result.FindByEntityKey("FBillHead");
            //获取外发仓的列表
            //var stockList = GetOutSourceStock();
            //FormMetadata formMetadata = MetaDataServiceHelper.Load(this.Context, "STK_InStock") as FormMetadata;
            //BusinessInfo businessInfo = formMetadata.BusinessInfo;
            //BaseDataField bdField = businessInfo.GetField("FStockID") as BaseDataField;
            //QueryBuilderParemeter p = new QueryBuilderParemeter();
            //p.FormId = "BD_STOCK";
            //p.SelectItems = SelectorItemInfo.CreateItems("FStockId");
            foreach (var headEntity in headEntitys)
            {
                //客户信息集合
                var customerDynamic = headEntity["CustomerID"] as DynamicObject;
                //财务信息集合
                var delfin = headEntity["SAL_DELIVERYNOTICEFIN"] as DynamicObjectCollection;
                //价税合计
                decimal billAllAmount = 0;
                foreach (var item in delfin)
                {
                    billAllAmount = Decimal.Parse(item["BillAllAmount"].ToString());
                }
                //收款条件信息集合
                var collectionTerms = headEntity["FRECEIPTCONDITIONID"] as DynamicObject;
                //获取客户Code
                var customerCode = "";
                long customerId = 0;
                //获取收款条件的类型
                var collectionTermsType = "";

                if (customerDynamic != null)
                {
                    customerCode = customerDynamic["Number"].ToString();
                    customerId = Convert.ToInt64(customerDynamic["Id"]);
                    headEntity.DataEntity["FSpecialDelivery"] = Convert.ToBoolean(customerDynamic["FSpecialDelivery"]);
                    headEntity.DataEntity["FPackagingReq"] = Convert.ToString(customerDynamic["FPackagingReq"]);
                }
                if (collectionTerms != null)
                {
                    collectionTermsType = collectionTerms["RECMETHOD"].ToString();
                }
                //月结客户才需要判断额度,并且金额大于0
                if (!string.IsNullOrEmpty(customerCode) && collectionTermsType.Equals("1") && billAllAmount > 0)
                {
                    var request = new
                    {
                        CustId = customerCode,
                        Amount = billAllAmount
                    };
                    var response = ApigatewayUtils.InvokePostWebService($"credit/{ApigatewayUtils.ApigatewayConfig.EnvCode}/credit/query", JsonConvertUtils.SerializeObject(request));
                    var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(response);
                    if (!returninfo.IsSuccess)
                    {
                        if (!string.IsNullOrWhiteSpace(returninfo.Message))
                        {
                            throw new Exception("获取信用接口出错：" + returninfo.Message);
                        }
                        else
                        {
                            throw new Exception("获取信用接口出错：" + returninfo.ErrorMessage);
                        }
                    }
                    else
                    {
                        headEntity.DataEntity["FCreditLine"] = returninfo.Data.CreditLine;
                        headEntity.DataEntity["FAvailableCredit"] = returninfo.Data.AvailableCredit;
                        headEntity.DataEntity["FOccupyCredit"] = returninfo.Data.OccupyCredit;
                        headEntity.DataEntity["FExpiryDay"] = returninfo.Data.ExpiryDay;
                        headEntity.DataEntity["FExpiryAmount"] = returninfo.Data.ExpiryAmount;
                        if (returninfo.Data.ExpiryDay > 0 || returninfo.Data.ExpiryAmount > 0)
                        {
                            var createUser = Convert.ToInt64(headEntity["CreatorId_Id"]);
                            //创建人微信Code
                            var cUserWxCode = GetUserWxCode(this.Context, createUser);

                            if (!string.IsNullOrWhiteSpace(cUserWxCode))
                            {
                                string sends = $@"客户:{customerDynamic["Name"]} 
逾期金额:{returninfo.Data.ExpiryAmount}逾期天数:{returninfo.Data.ExpiryDay}天
现需要发货: 金额:{billAllAmount} 请核查，并批核!";
                                SendTextMessageUtils.SendTextMessage(cUserWxCode, sends);
                            }
                        }

                    }
                }

                //是否出货检验
                //bool isShipInspect = false;
                List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@CustId", KDDbType.Int64, customerId) };
                var sSql = $@"/*dialect*/select top 1 FNUMBER,FNOTE from PENY_T_ShipInspectCust where FDOCUMENTSTATUS='C' and FFORBIDSTATUS='A' and FCUSTID=@CustId ";
                using (var dr = DBUtils.ExecuteReader(this.Context, sSql, pars))
                {
                    while (dr.Read())
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(dr["FNUMBER"])))
                        {
                            //isShipInspect = true;
                            headEntity.DataEntity["FIsShipInspectCust"] = true;
                            headEntity.DataEntity["FIsShipInspect"] = true;
                            headEntity.DataEntity["FPENYNOTE"] = Convert.ToString(dr["FNOTE"]).Trim().TrimStart('\n').TrimEnd('\n');
                        }
                    }
                }
                //if (!isShipInspect)
                //{
                //    //获取最后一次发货通知单的备注携带上去,(同一个供货组织，同一个客户，按审核日期)
                //    //表头供货组织信息集合
                //    var orgDynamic = headEntity["FHEADSUPPLYTARGETORGID"] as DynamicObject;
                //    if (orgDynamic != null)
                //    {
                //        var orgId = Convert.ToInt64(orgDynamic["Id"]);
                //        pars = new List<SqlParam>() { new SqlParam("@CustId", KDDbType.Int64, customerId), new SqlParam("@OrgId", KDDbType.Int64, orgId) };
                //        sSql = $@"/*dialect*/select top 1 FPENYNOTE from T_SAL_DELIVERYNOTICE 
                //                where FDOCUMENTSTATUS='C' and FHEADSUPPLYTARGETORGID=@OrgId and FCUSTOMERID=@CustId order by FAPPROVEDATE desc";
                //        headEntity.DataEntity["FPENYNOTE"] = DBUtils.ExecuteScalar<string>(this.Context, sSql, "", pars.ToArray());
                //    }
                //}
                //存在外发仓的，默认绑定外发仓
                //if (stockList.Count > 0)
                //{
                //    //获取明细数据
                //    var detDynamicObject = headEntity["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection;
                //    foreach (var item in detDynamicObject)
                //    {
                //        var stock = stockList.Where(x => x.FOUTSOURCESTOCKLOC == "asdf").FirstOrDefault();
                //        if (stock != null)
                //        {
                //            p.FilterClauseWihtKey = $"FSTOCKID={stock.StockID} ";
                //            var obj_ck = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p)[0];
                //            item["StockID_Id"] = obj_ck["Id"];
                //            item["StockID"] = obj_ck;
                //        }
                //    }

                //}
            }
        }

        private string GetUserWxCode(Context ctx, long userId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
            string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";

            return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
        }
        /// <summary>
        /// 获取外发仓
        /// </summary>
        /// <param name="choiceList"></param>
        /// <returns></returns>
        public List<OutSourceStock> GetOutSourceStock()
        {
            List<OutSourceStock> stockList = new List<OutSourceStock>();

            var sql = $@"select sto.FSTOCKID,sto.FUseOrgId,sto.FOUTSOURCESTOCKLOC from T_BD_STOCK sto
					  inner join t_BD_StockStatus stos on sto.FDEFSTOCKSTATUSID=stos.FSTOCKSTATUSID
					  where  stos.FAvailable='1' and sto.FISOUTSOURCESTOCK='1' ";
            var datas = DBUtils.ExecuteDynamicObject(this.Context, sql);
            foreach (var data in datas)
            {
                stockList.Add(new OutSourceStock()
                {
                    StockID = Convert.ToInt64(data["FSTOCKID"]),
                    UseOrgId = Convert.ToInt64(data["FUseOrgId"]),
                    FOUTSOURCESTOCKLOC = Convert.ToString(data["FOUTSOURCESTOCKLOC"]),
                });
            }
            return stockList;
        }

        /// <summary>
        /// 外发仓
        /// </summary>
        public class OutSourceStock
        {
            /// <summary>
            /// 外发仓ID
            /// </summary>
            public long StockID { get; set; }
            /// <summary>
            /// 使用组织
            /// </summary>
            public long UseOrgId { get; set; }
            public string FOUTSOURCESTOCKLOC { get; set; }

        }

    }
}
