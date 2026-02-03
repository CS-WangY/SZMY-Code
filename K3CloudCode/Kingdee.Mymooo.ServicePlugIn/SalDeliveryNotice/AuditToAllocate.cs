using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Log;
using Kingdee.Mymooo.Core.BomManagement;
using System.Web.UI.WebControls;
using Kingdee.BOS;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Orm;
using Kingdee.Mymooo.Core;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.Bill;
using System.Data;
using System.Xml.Linq;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Contracts;
using static Kingdee.Mymooo.App.Core.BaseManagement.BasicDataSyncService;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.K3.Core.Mobile.Elements;
using Kingdee.K3.Core.MFG.PLN.Reserved.ReserveArgs;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN.Reserve;
using static Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice.SubmitLock;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.BOS.TCP;
using Kingdee.Mymooo.Core.SalesManagement;
using System.Security.Cryptography;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.BOS.Web;
using System.Web.UI.WebControls.WebParts;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单审核生成调拨单插件"), HotUpdate]
    public class AuditToAllocate : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FIsOutSourceStock");
            e.FieldKeys.Add("FSaleOrgId");//销售组织
            e.FieldKeys.Add("FDate");//发货通知单时间
            e.FieldKeys.Add("FCustomerID");//客户
            e.FieldKeys.Add("FReceiverContactID");//收货方联系人
            e.FieldKeys.Add("FReceiveAddress");//收货方地址
            e.FieldKeys.Add("FLinkMan");//收货人姓名
            e.FieldKeys.Add("FLinkPhone");//联系电话
            e.FieldKeys.Add("FCustMatID");//客户物料编码
            e.FieldKeys.Add("FCustMatName");//客户物料名称
            e.FieldKeys.Add("FCustItemNo");//客户物料编码(新)
            e.FieldKeys.Add("FCustItemName");//客户物料名称(新)
            e.FieldKeys.Add("FMateriaModel");//规格型号
            e.FieldKeys.Add("FSrcBillNo");//销售单号
            e.FieldKeys.Add("FSOEntryId");//销售订单EntryId
            e.FieldKeys.Add("FPROJECTNO");//项目号
            e.FieldKeys.Add("FCustMaterialNo");//客户料号
            e.FieldKeys.Add("FStockFeatures");//库存管理特征
            e.FieldKeys.Add("FBUSINESSDIVISIONID");//事业部
            e.FieldKeys.Add("FDeliveryOrgID");//发货组织
            e.FieldKeys.Add("FStockID");//出货仓库
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FOwnerID");//货主
            e.FieldKeys.Add("FSupplyTargetOrgId");//供货组织
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCUSTPURCHASENO");//客户采购单号
            e.FieldKeys.Add("FPENYNOTE");//表头备注(用于云存储送货单备注)
            e.FieldKeys.Add("FNoteEntry");//明细销售备注(用于云存储销售备注)
            e.FieldKeys.Add("FSalesManID");//销售员
            e.FieldKeys.Add("FTaxPrice");//含税单价
            e.FieldKeys.Add("FCreatorId");//创建人
            e.FieldKeys.Add("FOrderNo");
            e.FieldKeys.Add("FOrderSEQ");
            e.FieldKeys.Add("FSMALLID");
            e.FieldKeys.Add("FPARENTSMALLID");
            e.FieldKeys.Add("FSpecialDelivery");//特殊发货
            e.FieldKeys.Add("FPackagingReq");//包装要求
        }
        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                //这个跟其他的验证无关，都需要传云存储。
                var detOrgID = Convert.ToInt64(item["DeliveryOrgID_Id"]);
                long salOrg = Convert.ToInt64(item["SaleOrgId_Id"]);
                var fid = Convert.ToInt64(item["Id"]);
                var billtype = item["BillTypeID_Id"].ToString();
                var billno = item["BillNo"].ToString();
                var creatorid = Convert.ToInt64(item["CreatorId_Id"]);
                var customerName = Convert.ToString(((DynamicObject)item["CustomerID"])["Name"]);
                //add 240604去掉直发不传云存储
                // if (((DynamicObject)item["BillTypeID"])["Number"].ToString() != "FHTZD01_PENY")
                //{

                //add 240716除了销售组织是深圳蚂蚁的，其他发货组织按仓库设置的“同步云存储”来判断是否传到云仓储
                if (salOrg == 224428 || Convert.ToBoolean(((DynamicObject)((DynamicObjectCollection)item["SAL_DELIVERYNOTICEENTRY"])[0]["StockID"])["FSyncToWarehouse"]))
                {
                    var whDn = (ArrayList)(GetDelivery(item));
                    var requestData = JsonConvertUtils.SerializeObject(whDn);
                    //同步云仓储
                    AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockUpdateTempDeliveryArea", Convert.ToString(item["BillNo"]));
                }

                //直发的发货通知单审核不生成调拨单 add 241210
                if (((DynamicObject)item["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY")
                {
                    continue;
                }
                //华东五部的，需要传给MES
                if (IsHDFI(this.Context, fid))
                {
                    SenMes(this.Context, fid);
                }
                //下推销售出库单（只有广东蚂蚁的才需要）
                //if (detOrgID == 669144)
                //{
                //    PushSaleOutStorage(this.Context, item);
                //}
                //}
                //如果销售组织不是则跳出 深圳224428广东蚂蚁669144华东五部7401803
                long[] SalOrgList = new long[] { 224428, 669144, 7401803, 1043841, 7348029 };

                if (!SalOrgList.Contains(salOrg))
                {
                    continue;
                }

                DynamicObject creator = item["CreatorId"] as DynamicObject;

                //需要调拨的数据集合
                List<Allocate> allocates = new List<Allocate>();
                foreach (var entitem in item["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
                {
                    var supplyid = Convert.ToInt64(entitem["FSupplyTargetOrgId_Id"]);
                    //销售组织与发货组织相同不调拨1
                    if (salOrg == supplyid)
                    {
                        continue;
                    }
                    var entryid = Convert.ToInt64(entitem["Id"]);
                    var seq = Convert.ToInt64(entitem["Seq"]);

                    var orderno = Convert.ToString(entitem["OrderNo"]);
                    var orderseq = Convert.ToString(entitem["OrderSeq"]);
                    //判断是否为外发仓
                    DynamicObject material = entitem["MaterialID"] as DynamicObject;
                    var srcunitID = Convert.ToInt64(entitem["UnitID_Id"]);
                    DynamicObject ckid = entitem["StockID"] as DynamicObject;
                    //云仓储回调下架
                    var isCallBack = Convert.ToBoolean(((DynamicObject)entitem["FSupplyTargetOrgId"])["FIsCloudStockCallBack"]);
                    decimal delqty = Convert.ToDecimal(entitem["Qty"]);
                    //7401780全国一部7401781华南二部7401822华东二部 
                    //long[] supplyOrgList = new long[] { 7401780, 7401781 };
                    if (isCallBack)
                    {
                        continue;
                    }

                    var srcbillid = "";
                    var srcrowid = "";
                    foreach (var itemlink in entitem["FEntity_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = itemlink["SBillId"] as string;
                        srcrowid = itemlink["SId"] as string;
                    }
                    //没有上游单据的不校验
                    if (srcrowid.IsNullOrEmptyOrWhiteSpace()) return;
                    string sSql = "";
                    if (SalOrgList.Contains(salOrg))
                    {
                        //查询组织间需求单
                        sSql = $@"SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FSTOCKID,t2.FDEMANDINTERID,t1.FSUPPLYORGID,t1.FBASEUNITID,t1.FSUPPLYINTERID
                        FROM T_PLN_RESERVELINKENTRY t1
                        INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        INNER JOIN T_PLN_REQUIREMENTORDER t3 ON t2.FDEMANDINTERID=t3.FID
						LEFT JOIN T_BD_STOCK t4 ON t1.FSTOCKID=t4.FSTOCKID
                        WHERE t2.FSRCINTERID='{srcbillid}' AND t2.FSRCENTRYID='{srcrowid}'
                        AND t1.FSUPPLYFORMID='STK_Inventory' AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                        AND t3.FDEMANDQTY-t3.FBASETRANOUTQTY>=0
						AND t4.FNOTALLOWDELIVERY=0
                        GROUP BY t1.FSTOCKID,t2.FDEMANDINTERID,t1.FSUPPLYORGID,t1.FBASEUNITID,t1.FSUPPLYINTERID";
                        //华东五部
                        //case 7401803:
                        //    //查询销售单预留信息
                        //    sSql = $@"SELECT SUM(t1.FBASEQTY) AS FBASEQTY FROM T_PLN_RESERVELINKENTRY t1
                        //INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        //INNER JOIN T_BD_STOCK t3 ON t1.FSTOCKID=t3.FSTOCKID
                        //WHERE t2.FSRCENTRYID='226847'
                        //AND t1.FSUPPLYFORMID='STK_Inventory' AND t3.FISOUTSOURCESTOCK=1";
                        //    break;
                    }
                    if (sSql.IsNullOrEmpty())
                    {
                        continue;
                    }
                    var RequirementorderDatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                    foreach (var reitem in RequirementorderDatas)
                    {
                        if (delqty <= 0)
                        {
                            continue;
                        }
                        decimal sqty = 0;
                        //如果存在单位不一致的情况，做单位转换数量再比较
                        var resunitID = Convert.ToInt64(reitem["FBASEUNITID"]);
                        var baseqty = Convert.ToDecimal(reitem["FBASEQTY"]);
                        IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                        if (srcunitID != resunitID)
                        {
                            baseqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(material["Id"]), resunitID, srcunitID, baseqty);
                        }
                        if (delqty - baseqty > 0)
                        {
                            sqty = baseqty;
                            delqty -= baseqty;
                        }
                        else
                        {
                            sqty = delqty;
                            delqty -= sqty;
                        }
                        //获取调出仓库
                        BusinessInfo businessInfo = this.BusinessInfo;
                        BaseDataField bdField = businessInfo.GetField("FStockID") as BaseDataField;
                        QueryBuilderParemeter p = new QueryBuilderParemeter();
                        p.FormId = "BD_STOCK";
                        p.SelectItems = SelectorItemInfo.CreateItems("FStockId");
                        p.FilterClauseWihtKey = $"FStockId = {Convert.ToInt64(reitem["FSTOCKID"])}";
                        var src_ck = BusinessDataServiceHelper.Load(this.Context, bdField.RefFormDynamicObjectType, p)[0];
                        #region VMI直接调拨
                        sSql = $@"SELECT t3.FSUPPLIERID,t2.FMATERIALID,FSTOCKID FROM V_STK_INVENTORY_CUS t1
                                INNER JOIN dbo.T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMASTERID AND t1.FSTOCKORGID=t2.FUSEORGID
                                INNER JOIN t_BD_Supplier t3 ON t1.FOWNERID=t3.FMASTERID AND t3.FUSEORGID=t1.FKEEPERID
                                WHERE FID='{reitem["FSUPPLYINTERID"]}' AND t1.FOWNERTYPEID='BD_Supplier'";
                        var vmiDatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                        if (vmiDatas.Count > 0)
                        {
                            var vmiTransfer = new VmiItem()
                            {
                                FDelNoticeEID = Convert.ToString(entryid),
                                FSalSrcrowid = srcrowid,
                                FRequirementId = reitem["FDEMANDINTERID"].ToString(),
                                FOwnerId = Convert.ToInt64(vmiDatas[0]["FSUPPLIERID"]),
                                FMaterialId = Convert.ToInt64(vmiDatas[0]["FMATERIALID"]),
                                FSrcStockId_Id = Convert.ToInt64(src_ck["Id"]),
                                FSrcStockId = src_ck,
                                FQty = sqty,
                                FBaseQty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(material["Id"]), srcunitID, resunitID, sqty),
                            };
                            var result = RequirementPushTransfer(this.Context, vmiTransfer);
                            sSql = $@"SELECT * FROM dbo.T_STK_INVENTORY
                            WHERE FMATERIALID={material["msterID"]} 
                            AND FOWNERID={reitem["FSUPPLYORGID"]} 
                            AND FSTOCKID={src_ck["Id"]} AND FBASEQTY-FLOCKQTY>={sqty}";
                            var stkdatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            CreateReserveLink(Convert.ToInt64(reitem["FDEMANDINTERID"]),
                                new lockinfo
                                {
                                    id = stkdatas.First()["FID"].ToString(),
                                    stockorgid = stkdatas.First()["FSTOCKORGID"].ToString(),
                                    stockid = stkdatas.First()["FSTOCKID"].ToString(),
                                    baseunitid = stkdatas.First()["FBASEUNITID"].ToString()
                                },
                                sqty
                                );
                            //var saveResult = PushReq(this.Context, srcbillid, srcrowid, sqty);
                            //创建与销售订单的预留关系
                            //var pkids = saveResult.SuccessDataEnity.Select(x => Convert.ToInt64(x["Id"])).FirstOrDefault();
                            //CreateReserveLink(Convert.ToInt64(srcrowid), pkids);
                        }
                        #endregion


                        allocates.Add(new Allocate
                        {
                            CreatorId_Id = creatorid,
                            CreatorId = creator,
                            SalBillNo = orderno,
                            SalBillSEQ = orderseq,
                            DeliveryNoticeNumber = billno,
                            DeliveryNoticeSEQ = seq,
                            DeliveryNoticeID = fid,
                            DeliveryNoticeEntryID = entryid,
                            FID = reitem["FDEMANDINTERID"].ToString(),
                            TargetOrgId = reitem["FSUPPLYORGID"].ToString(),
                            //FDestMaterialID = src_material,
                            FMaterialId = Convert.ToInt64(material["msterID"]),
                            FBASEQTY = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(material["Id"]), srcunitID, resunitID, sqty),
                            FQTY = sqty,
                            FSrcStock_Id = Convert.ToInt64(src_ck["Id"]),
                            FSrcStockId = src_ck,
                            FDestStock_Id = Convert.ToInt64(ckid["Id"]),
                            FDestStockId = ckid,
                        });

                    }
                    if (delqty > 0)
                    {
                        throw new Exception($"第{seq}行物料{material["Number"]},可调拨数量不足!");
                    }
                }
                //组织间需求单下推分步式调出单
                if (allocates.Count > 0)
                {
                    var opresult = SalDeliveryNoticePushAllocate(this.Context, allocates);
                    if (opresult.IsSuccess)
                    {
                        var createUser = Convert.ToInt64(item["CreatorId_Id"]);
                        //创建人微信Code
                        var cUserWxCode = GetUserWxCode(this.Context, createUser);

                        var sContent = opresult.SuccessDataEnity.Select(p => p["BillNo"].ToString());
                        if (!string.IsNullOrWhiteSpace(cUserWxCode))
                        {
                            SendTextMessageUtils.SendTextMessage(cUserWxCode, $"客户:{customerName},发货通知[{billno}]已生成相关调拨单请查阅：" + string.Join(",", sContent));
                        }
                    }
                    var sumBaseQty = from grouplist in allocates.GroupBy(x => new Tuple<string>(x.FSrcStockId["Name"].ToString()))
                                     select new
                                     {
                                         StockName = grouplist.Key.Item1,
                                         BaseQty = grouplist.Sum(x => x.FBASEQTY),
                                     };
                    string sText = string.Join(";", sumBaseQty.Select(p => p.StockName + ":" + p.BaseQty));
                    string sSql = $"UPDATE T_SAL_DELIVERYNOTICE SET FPENYTransferText='{sText}' WHERE FID={fid}";
                    DBUtils.Execute(this.Context, sSql);

                }
                //原五部独立流程
                switch (salOrg)
                {
                    //销售组织为华东五部自动下推销售出库单
                    case 7401803:
                        var opresult = SalDeliveryNoticePushOutStock(this.Context, fid);
                        var createUser = Convert.ToInt64(item["CreatorId_Id"]);
                        //创建人微信Code
                        var cUserWxCode = GetUserWxCode(this.Context, createUser);
                        var sContent = opresult.SuccessDataEnity.Select(p => p["BillNo"].ToString());
                        if (!string.IsNullOrWhiteSpace(cUserWxCode))
                        {
                            SendTextMessageUtils.SendTextMessage(cUserWxCode, "您的发货通知已生成出库单请查阅：" + string.Join(",", sContent));
                        }
                        break;
                }
            }
        }

        //获取创建人微信Code
        private string GetUserWxCode(Context ctx, long userId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userId) };
            string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";
            return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
        }

        //判断明细是否华东五部
        private bool IsHDFI(Context ctx, long fId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FID", KDDbType.Int64, fId) };
            string sql = $@"select count(1) from T_SAL_DELIVERYNOTICEENTRY where FID=@FID and FSupplyTargetOrgId=7401803 ";
            return DBUtils.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;
        }
        /// <summary>
        /// 发送给MES
        /// </summary>
        public void SenMes(Context ctx, long fId)
        {
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            DeliveryToMesEntity entity = new DeliveryToMesEntity();
            var sSql = $@"/*dialect*/select top 1 t1.FID Id,t1.FBILLNO BillNo,t1.FDATE Date,t1.FAPPROVEDATE ApproveDate,ORG.FNUMBER DeliveryOrgCode,ORGL.FNAME DeliveryOrgName,
                                t1.FCustomerID CustomerId,CUS.FNUMBER  CustomerCode,CUSL.FNAME CustomerName,
                                SALM.FWECHATCODE SalesManCode ,SALML.FNAME SalesManName,
                                t1.FNote PenyNote,t1.FLinkMan LinkMan,t1.FLinkPhone LinkPhone,t1.FReceiveAddress ReceiveAddress from T_SAL_DELIVERYNOTICE t1
                                inner join T_ORG_ORGANIZATIONS ORG on t1.FDeliveryOrgID=ORG.FORGID
                                inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
                                left join T_BD_CUSTOMER CUS on t1.FCustomerID=CUS.FCUSTID
                                left join T_BD_CUSTOMER_L CUSL on CUSL.FCUSTID=CUS.FCUSTID AND CUSL.FLOCALEID=2052
                                left join V_BD_SALESMAN SALM on t1.FSalesManID=SALM.fid
                                left join V_BD_SALESMAN_L SALML on SALM.fid=SALML.fid and SALML.FLOCALEID=2052
                                where t1.FID={fId}";
            var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            foreach (var item in datas)
            {
                entity.Id = Convert.ToInt64(item["Id"]);
                entity.BillNo = Convert.ToString(item["BillNo"]);
                entity.Date = Convert.ToDateTime(item["Date"]);
                entity.ApproveDate = Convert.ToDateTime(item["ApproveDate"]);
                entity.DeliveryOrgCode = Convert.ToString(item["DeliveryOrgCode"]);
                entity.DeliveryOrgName = Convert.ToString(item["DeliveryOrgName"]);
                entity.CustomerId = Convert.ToInt64(item["CustomerId"]);
                entity.CustomerCode = Convert.ToString(item["CustomerCode"]);
                entity.CustomerName = Convert.ToString(item["CustomerName"]);
                entity.SalesManCode = Convert.ToString(item["SalesManCode"]);
                entity.SalesManName = Convert.ToString(item["SalesManName"]);
                entity.PenyNote = Convert.ToString(item["PenyNote"]);
                entity.LinkMan = Convert.ToString(item["LinkMan"]);
                entity.LinkPhone = Convert.ToString(item["LinkPhone"]);
                entity.ReceiveAddress = Convert.ToString(item["ReceiveAddress"]);
            }

            sSql = $@"/*dialect*/select t1.FENTRYID EntryId,t1.FSEQ BillSeq,BDM.FNUMBER MaterialCode,BDML.FNAME MaterialName,t1.FCustItemNo CustItemNo,t1.FCustItemName CustItemName,t1.FCUSTMATERIALNO CustMaterialNo,t1.FCUSTPURCHASENO CustPurchaseNo,
                    pg.FID ParentSmallId,pg.FNUMBER ParentSmallCode,pgl.FNAME  ParentSmallName,g.FID SmallId,g.FNUMBER SmallCode,gl.FNAME SmallName,
                    t1.FQTY Qty,t1.FDeliveryDate DeliveryDate,sto.FNUMBER StockCode,stol.FNAME StockName,ORG.FNUMBER SupplyTargetOrgCode,orgl.FNAME SupplyTargetOrgName,
                    t1.FInsideRemark InsideRemark,t1.FProjectNo ProjectNo,t1.FStockFeatures StockFeatures,t1.FLocFactory LocFactory,t1.FNOTE NoteEntry,
                    t4.FID SaleOrderId,t4.FBILLNO SaleOrderNo,t3.FENTRYID OrderEntryId,t3.FSEQ OrderEntrySeq
                    from T_SAL_DELIVERYNOTICEENTRY t1
                    left join T_SAL_DELIVERYNOTICEENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                    left join T_SAL_ORDERENTRY t3 on t2.FSID=t3.FENTRYID
                    left join T_SAL_ORDER t4 on t3.FID=t4.FID
                    left join T_BD_MATERIAL BDM on t1.FMATERIALID=BDM.FMATERIALID
                    left join T_BD_MATERIAL_L BDML on BDM.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                    left join T_BD_MATERIALGROUP g on t1.FSMALLID = g.FID 
                    left join T_BD_MATERIALGROUP_L gl on t1.FSMALLID = gl.FID and gl.FLOCALEID = 2052
                    left join T_BD_MATERIALGROUP pg on t1.FPARENTSMALLID = pg.FID 
                    left join T_BD_MATERIALGROUP_L pgl on t1.FPARENTSMALLID = pgl.FID and pgl.FLOCALEID = 2052
                    left join t_BD_Stock sto on sto.FStockId=t1.FSHIPMENTSTOCKID
                    left join T_BD_STOCK_L stol on sto.FStockId=stol.FStockId and stol.FLOCALEID = 2052
                    inner join T_ORG_ORGANIZATIONS ORG on t1.FSupplyTargetOrgId=ORG.FORGID
                    inner join T_ORG_ORGANIZATIONS_L ORGL on ORG.FORGID=ORGL.FORGID and ORGL.FLOCALEID=2052
                     where t1.FID={fId} order by t1.FSEQ ";
            datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            entity.Details = new List<DeliveryToMesDetEntity>();
            foreach (var item in datas)
            {
                entity.Details.Add(new DeliveryToMesDetEntity
                {
                    EntryId = Convert.ToInt64(item["EntryId"]),
                    BillSeq = Convert.ToInt32(item["BillSeq"]),
                    MaterialCode = Convert.ToString(item["MaterialCode"]),
                    MaterialName = Convert.ToString(item["MaterialName"]),
                    CustItemNo = Convert.ToString(item["CustItemNo"]),
                    CustItemName = Convert.ToString(item["CustItemName"]),
                    CustMaterialNo = Convert.ToString(item["CustMaterialNo"]),
                    CustPurchaseNo = Convert.ToString(item["CustPurchaseNo"]),
                    ParentSmallId = Convert.ToInt32(item["ParentSmallId"]),
                    ParentSmallCode = Convert.ToString(item["ParentSmallCode"]),
                    ParentSmallName = Convert.ToString(item["ParentSmallName"]),
                    SmallId = Convert.ToInt32(item["SmallId"]),
                    SmallCode = Convert.ToString(item["SmallCode"]),
                    SmallName = Convert.ToString(item["SmallName"]),
                    Qty = Convert.ToDecimal(item["Qty"]),
                    DeliveryDate = Convert.ToDateTime(item["DeliveryDate"]),
                    StockCode = Convert.ToString(item["StockCode"]),
                    StockName = Convert.ToString(item["StockName"]),
                    SupplyTargetOrgCode = Convert.ToString(item["SupplyTargetOrgCode"]),
                    SupplyTargetOrgName = Convert.ToString(item["SupplyTargetOrgName"]),
                    InsideRemark = Convert.ToString(item["InsideRemark"]),
                    ProjectNo = Convert.ToString(item["ProjectNo"]),
                    StockFeatures = Convert.ToString(item["StockFeatures"]),
                    LocFactory = Convert.ToString(item["LocFactory"]),
                    NoteEntry = Convert.ToString(item["NoteEntry"]),
                    SaleOrderId = Convert.ToInt64(item["SaleOrderId"]),
                    SaleOrderNo = Convert.ToString(item["SaleOrderNo"]),
                    OrderEntryId = Convert.ToInt64(item["OrderEntryId"]),
                    OrderEntrySeq = Convert.ToInt32(item["OrderEntrySeq"]),
                });
            }

            entity.OperationNumber = "Audit";
            entity.FormId = "SAL_DELIVERYNOTICE";
            messages.Add(new RabbitMQMessage
            {
                Exchange = "salesManagement",
                Routingkey = entity.FormId,
                Keyword = entity.BillNo,
                Message = JsonConvertUtils.SerializeObject(entity)
            });

            KafkaProducerService kafkaProducer = new KafkaProducerService();
            kafkaProducer.AddMessage(this.Context, messages.ToArray());

        }

        /// <summary>
        /// 立即执行MQ
        /// </summary>
        /// <param name="e"></param>
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            Task.Factory.StartNew(() =>
            {
                //晚2个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(2000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
            });
        }

        /// <summary>
        /// 构建发货通知单接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            // 获取销售订单明细信息
            var salDetailInfo = GetSalDetailInfo(Convert.ToInt64(data["id"]));

            foreach (var item in data["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
            {
                string businessDivisionid = "";
                //获取销售订单数据
                string salesOrderNumber = "";//销售订单号
                int salesOrderSeq = 0;//销售订单序号
                decimal salesOrderQty = 0;//销售订单数量
                decimal deliveryTotalQty = 0;//累计发货数量(出库)
                decimal returnsTotalQty = 0;//累计退货数量
                if (salDetailInfo.Count > 0)
                {
                    var salesInfo = salDetailInfo.Where(x => Convert.ToInt64(x["FENTRYID"]).Equals(Convert.ToInt64(item["id"]))).FirstOrDefault();
                    if (salesInfo != null)
                    {
                        salesOrderNumber = Convert.ToString(salesInfo["FBILLNO"]);//销售订单号
                        salesOrderSeq = Convert.ToInt16(salesInfo["FSEQ"]);//销售订单序号
                        salesOrderQty = Convert.ToDecimal(salesInfo["FQTY"]);//销售订单数量
                        deliveryTotalQty = Convert.ToDecimal(salesInfo["FStockOutQty"]);//累计发货数量(出库)
                        returnsTotalQty = Convert.ToDecimal(salesInfo["FReturnQty"]);//累计退货数量
                    }
                }

                if (item["FBUSINESSDIVISIONID"] != null)
                {
                    businessDivisionid = Convert.ToString(((DynamicObject)item["FBUSINESSDIVISIONID"])["FNumber"]);
                }
                ////货主
                //var ownerCode = Convert.ToString(((DynamicObject)item["OwnerID"])["Number"]);
                //var ownerName = Convert.ToString(((DynamicObject)item["OwnerID"])["Name"]);
                //if (item["DeliveryOrgID"] != null)
                //{
                //    //如果调拨单云仓储自动上下架，则取供货组织。否则取货主
                //    if (((DynamicObject)item["DeliveryOrgID"])["FTransIsSynCloudStock"].Equals(true))
                //    {
                //        if (item["FSupplyTargetOrgId"] != null)
                //        {
                //            ownerCode = Convert.ToString(((DynamicObject)item["FSupplyTargetOrgId"])["Number"]);
                //            ownerName = Convert.ToString(((DynamicObject)item["FSupplyTargetOrgId"])["Name"]);
                //        }
                //    }
                //}

                string parentSmallCode = "";
                string parentSmallName = "";
                string smallCode = "";
                string smallName = "";
                if (item["FPARENTSMALLID"] != null)
                {
                    parentSmallCode = Convert.ToString(((DynamicObject)item["FPARENTSMALLID"])["Number"]);//大类编号
                    parentSmallName = Convert.ToString(((DynamicObject)item["FPARENTSMALLID"])["Name"]);//大类名称
                }
                if (item["FSMALLID"] != null)
                {
                    smallCode = Convert.ToString(((DynamicObject)item["FSMALLID"])["Number"]);//小类编号
                    smallName = Convert.ToString(((DynamicObject)item["FSMALLID"])["Name"]);//小类名称
                }
                //只打单，自动下架出库
                var IsAutoHandle = Convert.ToBoolean(((DynamicObject)item["StockID"])["FPrintAutoStockOut"]);
                var result = new PutToTempDeliveryAreaRequest
                {
                    FId = Convert.ToInt64(data["id"]),
                    EntryId = Convert.ToInt64(item["id"]),
                    ItemId = Convert.ToString(data["BillNo"] + "-" + item["Seq"]),
                    ExWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
                    SalesOrderNumber = salesOrderNumber,//销售单号
                    ModelNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),//物料编号
                    Name = Convert.ToString(((DynamicObject)item["MaterialID"])["Name"]),//物料名称
                    Specification = Convert.ToString(((DynamicObject)item["MaterialID"])["Specification"]),//物料规格型号
                    Quantity = Convert.ToDecimal(item["Qty"]),//发货数量
                                           //exWarehouseOnUtc = data["Date"],//发货通知单时间
                    ExWarehouseOnUtc = DateTime.Now.ToUniversalTime(),
                    ParentSmall = new ParentSmallModel
                    {
                        Code = parentSmallCode,//大类编号
                        Name = parentSmallName//大类名称
                    },
                    Small = new SmallModel
                    {
                        Code = smallCode,//小类编号
                        Name = smallName//小类名称
                    },
                    Unit = new NewUnitModel
                    {
                        Name = Convert.ToString(((DynamicObject)item["UnitID"])["Number"])//单位编号
                    },
                    Customer = new DeliverGoodsToCustomerModel
                    {
                        Coding = Convert.ToString(((DynamicObject)data["CustomerID"])["Number"]),//客户编码
                        Name = Convert.ToString(((DynamicObject)data["CustomerID"])["Name"]),//客户名称
                        Phone = Convert.ToString(data["FLinkPhone"]),//收货人电话
                        LinkMan = Convert.ToString(data["FLinkMan"]),//收货人
                        ReceiveAddress = Convert.ToString(data["ReceiveAddress"]),//收货地址
                        SpecialDelivery = Convert.ToBoolean(data["FSpecialDelivery"]),//特殊发货
                        PackagingReq = Convert.ToString(data["FPackagingReq"])//包装要求
                    },
                    Type = new ExternalTypeModel
                    {
                        Value = "ISO",
                        Description = "销售出仓"
                    },
                    CustMaterialNo = Convert.ToString(item["FCustMaterialNo"]), //TODO 客户料号                              
                    Remark = Convert.ToString(data["FPENYNOTE"]), //表头备注(用于云存储送货单备注)
                    SoRemark = Convert.ToString(item["NoteEntry"]),//明细销售备注(用于云存储销售备注)
                    DnRemark = Convert.ToString(item["NoteEntry"]),//明细销售备注(用于云存储销售备注)
                    CustPo = Convert.ToString(item["FCUSTPURCHASENO"]),//TODO 客户采购单号，待定
                    CustItemNo = Convert.ToString(item["FCustItemNo"]),
                    CustItemName = Convert.ToString(item["FCustItemName"]),
                    ProjectNo = Convert.ToString(item["FProjectNo"]),
                    DnAdd = Convert.ToString(((DynamicObject)item["StockID"])["Address"]),//仓库地址(仓库对应的仓库地址)
                    DeliveryplaceCode = Convert.ToString(((DynamicObject)item["StockID"])["FOutSourceStockLoc"]),//仓库对应的仓库发货区域
                    LocCode = Convert.ToString(((DynamicObject)item["StockID"])["FCloudStockCode"]),//(仓库对应的云仓储仓库编码)
                    IsDirectDeliveryStock = Convert.ToBoolean(((DynamicObject)item["StockID"])["FIsDirStock"]),//(仓库对应的是否直发仓库)
                    RefDnNo = "",//TODO 分公司送货单号，打印用。如果为空，会赋值当前送货单号，待定
                                 //TODO 以下为伯恩标签打印专用字段，待定
                    StockFeatures = Convert.ToString(item["FStockFeatures"]),//库存管理特征
                    AuxWeight = "",//辅助重量
                    BatchNo = "",//批次号
                    ProductioDate = "",//生产日期
                    StorageMode = "",//存储方式
                    BarCode = "",//条码内容
                    Warehouse = "",//客户仓库名
                    PoItemNumber = "",//采购项次
                    ItemSpec = "",//专属标签物料规格
                    DeliveryDetOrgCode = Convert.ToString(((DynamicObject)item["FSupplyTargetOrgId"])["Number"]),//供货组织编码
                    DeliveryDetOrgName = Convert.ToString(((DynamicObject)item["FSupplyTargetOrgId"])["Name"]),//供货组织名称
                    BusinessDivisionid = businessDivisionid,  //事业部
                    CompCode = Convert.ToString(((DynamicObject)data["SaleOrgId"])["Number"]),//销售组织编码
                    CompName = Convert.ToString(((DynamicObject)data["SaleOrgId"])["Name"]).Replace("华东五部", "江苏蚂蚁工场制造有限公司"),//销售组织名称
                    MyAddress = Convert.ToString(((DynamicObject)item["StockID"])["Address"]),//出货仓库地址
                    MyTel = Convert.ToString(((DynamicObject)item["StockID"])["Tel"]),//出货仓库电话
                    ReportMaker = Convert.ToString(((DynamicObject)data["CreatorId"])["Name"]),//制单人取创建人名称
                    ReportMakerPhone = Convert.ToString(((DynamicObject)data["CreatorId"])["Phone"]),//制单人取创建人电话号码
                    VatPrice = Convert.ToString(item["TaxPrice"]),//含税单价
                    IsAutoHandle = IsAutoHandle,
                    SalesOrderSeq = salesOrderSeq,//销售订单序号
                    SalesOrderQty = salesOrderQty,//销售订单数量
                    DeliveryTotalQty = deliveryTotalQty,//累计发货数量(出库)
                    ReturnsTotalQty = returnsTotalQty,//累计退货数量
                };

                arrList.Add(result);
            }

            return arrList;
        }
        /// <summary>
        /// 获取销售订单明细信息
        /// </summary>
        /// <param name="fId">发货通知单明细</param>
        public DynamicObjectCollection GetSalDetailInfo(long fId)
        {
            string sSql = $@"select t1.FENTRYID,t5.FBILLNO,t3.FSEQ,t3.FQTY,t4.FStockOutQty,t4.FReturnQty  from T_SAL_DELIVERYNOTICEENTRY t1
                            inner join T_SAL_DELIVERYNOTICEENTRY_LK t2 on t1.FENTRYID=t2.FENTRYID
                            inner join T_SAL_ORDERENTRY t3 on t2.FSBILLID=t3.FID and t2.FSID=t3.FENTRYID
                            inner join T_SAL_ORDERENTRY_R  t4 on t3.FENTRYID=t4.FENTRYID
                            inner join T_SAL_ORDER t5 on t5.FID=t4.FID 
                            where t1.FID={fId}";
            return DBUtils.ExecuteDynamicObject(this.Context, sSql);
        }

        // 下推销售出库单
        public IOperationResult SalDeliveryNoticePushOutStock(Context ctx, long delnoid)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            var row = new ListSelectedRow(delnoid.ToString(), string.Empty, 0, "SAL_DELIVERYNOTICE");
            selectedRows.Add(row);

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "DeliveryNotice-OutStock";

            var result = this.BillPush(ctx, push);
            return result;
        }

        public IOperationResult RequirementPushTransfer(Context ctx, VmiItem vmiItem)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            var row = new ListSelectedRow(vmiItem.FRequirementId, string.Empty, 0, "PLN_REQUIREMENTORDER");
            //row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
            selectedRows.Add(row);

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "PENY_PLN_REQUIREMENTORDER_TRANSFER";
            push.TargetBillTypeId = "005056941128823c11e323525db18103";
            //push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
            var result = this.BillPush(ctx, push, vmiItem);

            return result;
        }
        public IOperationResult SalDeliveryNoticePushAllocate(Context ctx, List<Allocate> allocates)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            foreach (var item in allocates)
            {
                var row = new ListSelectedRow(item.FID, string.Empty, 0, "PLN_REQUIREMENTORDER");
                //row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
            }

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "PLN_REQUIREMENTORDER_2_TRANSOUT";
            //push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
            //push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);

            var result = this.BillPush(ctx, push, allocates);
            return result;
        }

        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, List<Allocate> allocates)
        {
            //得到转换规则
            var convertRule = this.GetConvertRule(ctx, pushEntity.ConvertRule);
            OperateOption pushOption = OperateOption.Create();//操作选项
                                                              //构建下推参数
                                                              //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
                                                              //单据下推参数
            PushArgs pushArgs = new PushArgs(convertRule, pushEntity.listSelectedRow.ToArray());
            //目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
            pushArgs.TargetOrgId = pushEntity.TargetOrgId;
            //目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
            pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

            // 把新拆分出来的单据体行，加入到下推结果中
            // 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
            //e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());
            foreach (DynamicObject targeEntry in targetObjs)
            {
                //赋值创建人
                targeEntry["CreatorId_Id"] = allocates.First().CreatorId_Id;
                targeEntry["CreatorId"] = allocates.First().CreatorId;
                List<DynamicObject> newRows = new List<DynamicObject>();
                var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
                var rowEntry = targeEntry["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;

                foreach (var rowlin in rowEntry)
                {
                    var materialid = Convert.ToInt64(((DynamicObject)rowlin["MaterialId"])["msterID"]);

                    var srcbillid = "";
                    foreach (var itemlink in rowlin["FSTKTSTKRANSFEROUTENTRY_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = itemlink["SBillId"] as string;
                    }

                    var aot = allocates.Where(x => x.FID == srcbillid).ToList();
                    foreach (var itemlink in aot)
                    {
                        targeEntry["FPENYStockID_Id"] = itemlink.FSrcStockId["Id"];
                        targeEntry["FPENYStockID"] = itemlink.FSrcStockId;
                        //复制出新行
                        DynamicObject newRowObj = (DynamicObject)rowlin.Clone(false, true);
                        //修改赋值
                        newRowObj["FPENYSalOrderNo"] = itemlink.SalBillNo;
                        newRowObj["FPENYSalOrderSEQ"] = itemlink.SalBillSEQ;
                        newRowObj["FPENYDeliveryNotice"] = itemlink.DeliveryNoticeNumber;
                        newRowObj["FDeliveryNoticeSEQ"] = itemlink.DeliveryNoticeSEQ;
                        newRowObj["FDeliveryNoticeID"] = itemlink.DeliveryNoticeID;
                        newRowObj["FDeliveryNoticeENTRYID"] = itemlink.DeliveryNoticeEntryID;

                        newRowObj["FQty"] = itemlink.FQTY;
                        newRowObj["BaseQty"] = itemlink.FBASEQTY;
                        //调出
                        newRowObj["SrcStockID_Id"] = itemlink.FSrcStockId["Id"];
                        newRowObj["SrcStockID"] = itemlink.FSrcStockId;
                        //调入
                        newRowObj["DestStockID_Id"] = itemlink.FDestStockId["Id"];
                        newRowObj["DestStockID"] = itemlink.FDestStockId;
                        newRowObj["SrcStockStatusID_Id"] = 10000;
                        newRowObj["DestStockStatusID_Id"] = 10004;

                        newRows.Add(newRowObj);
                    }
                }

                rowEntry.Clear();
                foreach (var item in newRows)
                {
                    rowEntry.Add(item);
                }
                DBServiceHelper.LoadReferenceObject(this.Context, rowEntry.ToArray(), rowEntry.DynamicCollectionItemPropertyType, true);


            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs, true);

        }
        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, VmiItem vmiItem)
        {
            //得到转换规则
            var convertRule = this.GetConvertRule(ctx, pushEntity.ConvertRule);
            OperateOption pushOption = OperateOption.Create();//操作选项
                                                              //构建下推参数
                                                              //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
                                                              //单据下推参数
            PushArgs pushArgs = new PushArgs(convertRule, pushEntity.listSelectedRow.ToArray());
            //目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
            pushArgs.TargetOrgId = pushEntity.TargetOrgId;
            //目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
            pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

            // 把新拆分出来的单据体行，加入到下推结果中
            // 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
            //e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());
            foreach (DynamicObject targeEntry in targetObjs)
            {
                //var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
                targeEntry["OwnerOutIdHead_Id"] = vmiItem.FOwnerId;
                var rowEntry = targeEntry["TransferDirectEntry"] as DynamicObjectCollection;
                foreach (var rowlin in rowEntry)
                {
                    rowlin["FOwnerOutId_Id"] = vmiItem.FOwnerId;
                    rowlin["KeeperId_Id"] = targeEntry["StockOrgId_Id"];
                    rowlin["KeeperOutId_Id"] = targeEntry["StockOrgId_Id"];
                    rowlin["SrcStockId_Id"] = vmiItem.FSrcStockId_Id;
                    rowlin["SrcStockId"] = vmiItem.FSrcStockId;
                    rowlin["DestStockId_Id"] = vmiItem.FSrcStockId_Id;
                    rowlin["DestStockId"] = vmiItem.FSrcStockId;
                    rowlin["Qty"] = vmiItem.FQty;
                    rowlin["BaseQty"] = vmiItem.FBaseQty;
                    rowlin["ActQty"] = vmiItem.FQty;
                    rowlin["SaleQty"] = vmiItem.FQty;
                    rowlin["SalBaseQty"] = vmiItem.FBaseQty;
                    rowlin["FPENYDeliverynoticeEID"] = vmiItem.FDelNoticeEID;
                }
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs, false);

        }

        public void CreateReserveLink(long reqid, lockinfo lockinfo, decimal qty)
        {
            //取组织间需求单信息
            string sSql = $@"SELECT FFORMID AS DemandFormID,fid AS DemandInterID,FBILLNO AS DemandBillNO
            ,'SAL_SaleOrder' AS SrcDemandFormId,FSALEORDERID AS SrcDemandInterId,FSALEORDERENTRYID AS SrcDemandEntryId,FSALEORDERNO AS SrcDemandBillNo
            ,FSUPPLYORGID AS DemandOrgID
            ,FSUPPLYMATERIALID AS MaterialID
            ,FUNITID AS BaseUnitID 
            FROM T_PLN_REQUIREMENTORDER WHERE FID={reqid}";
            var salbilldata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            DemandView demandView = new DemandView();
            string demaInterid = "0";
            if (salbilldata.Count > 0)
            {
                demandView.DemandFormID = salbilldata[0]["DemandFormID"].ToString();
                demandView.DemandInterID = salbilldata[0]["DemandInterID"].ToString();
                demaInterid = salbilldata[0]["DemandInterID"].ToString();
                //demandView.DemandEntryID = string.Empty;
                demandView.DemandEntryID = "";
                demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

                //demandView.
                demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
                demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
                demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
                demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

                demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
                demandView.DemandDate = System.DateTime.Now;
                demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
                demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
                demandView.BaseQty = qty;

                //取即时库存
                List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
                SupplyViewItem subRowView = new SupplyViewItem();

                subRowView.SupplyFormID_Id = "STK_Inventory";
                subRowView.SupplyInterID = Convert.ToString(lockinfo.id);

                subRowView.SupplyMaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
                subRowView.SupplyOrgID_Id = Convert.ToInt64(lockinfo.stockorgid);
                subRowView.SupplyDate = System.DateTime.Now;
                subRowView.SupplyStockID_Id = Convert.ToInt64(lockinfo.stockid);
                subRowView.SupplyAuxproID_Id = 0;
                subRowView.BaseSupplyUnitID_Id = Convert.ToInt64(lockinfo.baseunitid);
                //供应数量
                //subRowView.BaseActSupplyQty = lockinfo.qty;
                subRowView.BaseActSupplyQty = qty;
                subRowView.IntsupplyID = Convert.ToInt64(demaInterid);


                viewItems.Add(subRowView);
                demandView.supplyView = viewItems;
            }


            //创建转移行信息
            List<ReserveLinkSelectRow> lstConvertInfo = CreateReserveRow(demandView);
            //构建预留转移参数
            ReserveArgs<ReserveLinkSelectRow> convertArgs = new ReserveArgs<ReserveLinkSelectRow>();
            //把预留转移行的信息赋给参数
            convertArgs.SelectRows = lstConvertInfo;
            //创建预留服务
            IReserveLinkService linkService = AppServiceContext.GetService<IReserveLinkService>();
            //调用预留创建接口
            linkService.ReserveLinkCreate(this.Context, convertArgs, OperateOption.Create());
        }
        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity)
        {
            //得到转换规则
            var convertRule = this.GetConvertRule(ctx, pushEntity.ConvertRule);
            OperateOption pushOption = OperateOption.Create();//操作选项
                                                              //构建下推参数
                                                              //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
                                                              //单据下推参数
            PushArgs pushArgs = new PushArgs(convertRule, pushEntity.listSelectedRow.ToArray());
            //目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
            pushArgs.TargetOrgId = pushEntity.TargetOrgId;
            //目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
            pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            pushOption.SetVariableValue(ConvertConst.SelectByBillId, true);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs, true);
        }
        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, decimal salqty)
        {
            //得到转换规则
            var convertRule = this.GetConvertRule(ctx, pushEntity.ConvertRule);
            OperateOption pushOption = OperateOption.Create();//操作选项
                                                              //构建下推参数
                                                              //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
                                                              //单据下推参数
            PushArgs pushArgs = new PushArgs(convertRule, pushEntity.listSelectedRow.ToArray());
            //目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
            pushArgs.TargetOrgId = pushEntity.TargetOrgId;
            //目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
            pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            //pushOption.SetVariableValue(ConvertConst., false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
            foreach (DynamicObject targeEntry in targetObjs)
            {
                var mid = Convert.ToInt64(targeEntry["MaterialId_Id"]);
                var unitid = Convert.ToInt64(targeEntry["UnitID_Id"]);
                var baseunitid = Convert.ToInt64(targeEntry["BaseUnitId_Id"]);
                var baseqty = salqty;
                IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                if (unitid != baseunitid)
                {
                    baseqty = convertService.GetUnitTransQty(this.Context, mid, unitid, baseunitid, salqty);
                }
                targeEntry["DemandQty"] = salqty;
                targeEntry["BaseDemandQty"] = baseqty;
                targeEntry["FirmQty"] = salqty;
                targeEntry["BaseFirmQty"] = baseqty;
                targeEntry["ReMainQty"] = salqty;
                targeEntry["ReMainBaseQty"] = baseqty;
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs, false);
        }
        /// <summary>
        /// 保存目标单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="targetBusinessInfo"></param>
        /// <param name="targetBillObjs"></param>
        private IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs, bool SaveType)
        {
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreInteractionFlag(true);
            saveOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
            //保存
            SaveService saveService = new SaveService();
            //提交
            //SubmitService submitService = new SubmitService();

            IOperationResult saveResult = new OperationResult();
            if (SaveType)
            {
                saveResult = saveService.Save(ctx, targetBusinessInfo, targetBillObjs, saveOption);
                if (!saveResult.IsSuccess)
                {
                    if (saveResult.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", saveResult.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", saveResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }
                else
                {
                    Dictionary<string, string> pkids = new Dictionary<string, string>();
                    pkids = saveResult.SuccessDataEnity.ToDictionary(p => p["Id"].ToString(), p => p["BillNo"].ToString());
                    SubmitService submitService = new SubmitService();
                    //提交单据，若存在工作流，则提交工作流
                    var resultlist = Kingdee.K3.Core.MFG.Utils.MFGCommonUtil.SubmitWithWorkFlow(ctx, targetBusinessInfo.GetForm().Id, pkids, saveOption);
                }
            }
            else
            {
                saveResult = saveService.SaveAndAudit(ctx, targetBusinessInfo, targetBillObjs, saveOption);
                if (!saveResult.IsSuccess)
                {
                    if (saveResult.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", saveResult.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", saveResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }
            }
            return saveResult;
        }
        // 得到表单元数据
        private BusinessInfo GetBusinessInfo(Context ctx, string formId, FormMetadata metaData = null)
        {
            if (metaData != null) return metaData.BusinessInfo;
            metaData = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
            return metaData.BusinessInfo;
        }
        //得到转换规则
        private ConvertRuleElement GetConvertRule(Context ctx, string convertRuleId)
        {
            var convertRuleMeta = ConvertServiceHelper.GetConvertRule(ctx, convertRuleId);
            return convertRuleMeta.Rule;
        }

        ///预留关系供需行      
        private List<ReserveLinkSelectRow> CreateReserveRow(DemandView demandView)
        {
            List<ReserveLinkSelectRow> linkRows = new List<ReserveLinkSelectRow>();
            //创建表头需求信息
            ReserveLinkSelectRow seleRow = new ReserveLinkSelectRow();
            //销售订单信息
            seleRow.DemandRow.DemandInfo.FormID = demandView.DemandFormID;
            seleRow.DemandRow.DemandInfo.InterID = demandView.DemandInterID;
            seleRow.DemandRow.DemandInfo.EntryID = demandView.DemandEntryID; ;
            seleRow.DemandRow.DemandInfo.BillNo = demandView.DemandBillNO;
            //销售订单信息
            seleRow.DemandRow.SrcDemandInfo.FormID = demandView.SrcDemandFormId;
            seleRow.DemandRow.SrcDemandInfo.InterID = demandView.SrcDemandInterId;
            seleRow.DemandRow.SrcDemandInfo.EntryID = demandView.SrcDemandEntryId;
            seleRow.DemandRow.SrcDemandInfo.BillNo = demandView.SrcDemandBillNo;
            seleRow.DemandRow.ReserveType = ((int)Enums.PLN_ReserveModel.Enu_ReserveType.KdStrong).ToString();
            //需求组织
            seleRow.DemandRow.DemandOrgID = demandView.DemandOrgID_Id;
            //需求日期
            seleRow.DemandRow.DemandDate = demandView.DemandDate;
            //物料内码
            seleRow.DemandRow.DemandMaterialID = demandView.MaterialID_Id;

            //基本单位内码
            seleRow.DemandRow.BaseUnitId = demandView.BaseUnitID_Id;
            //基本单位需求数量(基本单据数量-己出库的数量)
            seleRow.DemandRow.BaseDemandQty = demandView.BaseQty;

            //添加供应信息
            List<ReserveLinkSupplyRow> supplyRows = this.GetLinkSupplyRow(demandView.supplyView);
            seleRow.SupplyRows.AddRange(supplyRows);


            linkRows.Add(seleRow);
            return linkRows;
        }
        //创建供应行
        private List<ReserveLinkSupplyRow> GetLinkSupplyRow(List<SupplyViewItem> supplyView)
        {
            List<ReserveLinkSupplyRow> supplyRows = new List<ReserveLinkSupplyRow>();
            foreach (SupplyViewItem subRowView in supplyView)
            {
                ReserveLinkSupplyRow row = new ReserveLinkSupplyRow();
                //供应单据信息，单据标识，内码
                row.SupplyBillInfo.FormID = subRowView.SupplyFormID_Id;
                row.SupplyBillInfo.InterID = subRowView.SupplyInterID;
                //row.SupplyBillInfo.EntryID = subRowView.SupplyEntryId;
                //row.SupplyBillInfo.BillNo = subRowView.SupplyBillNO;
                row.SupplyMaterialID = subRowView.SupplyMaterialID_Id;
                row.SupplyOrgID = subRowView.SupplyOrgID_Id;
                row.SupplyDate = subRowView.SupplyDate;
                row.SupplyStockID = subRowView.SupplyStockID_Id;
                //row.SupplyStockLocID = subRowView.SupplyStockLocID_Id;
                //row.SupplyBomID = subRowView.SupplyBomID_Id;
                //row.SupplyLotId = subRowView.SupplyLot_Id;
                //row.SupplyLot_Text = subRowView.SupplyLot_Text;
                //row.SupplyMtoNO = subRowView.SupplyMtoNO;
                //row.SupplyProjectNO = "";
                row.SupplyAuxpropID = subRowView.SupplyAuxproID_Id;
                row.BaseSupplyUnitID = subRowView.BaseSupplyUnitID_Id;
                //供应数量
                row.BaseSupplyQty = subRowView.BaseActSupplyQty;
                row.LinkType = Enums.PLN_ReserveModel.Enu_ReserveBuildType.KdByManual;
                supplyRows.Add(row);
            }
            return supplyRows;
        }
    }
    public class VmiItem
    {
        public string FDelNoticeEID { get; set; }
        public string FSalSrcrowid { get; set; }
        public string FRequirementId { get; set; }
        /// <summary>
        /// 物料内码
        /// </summary>
        public long FMaterialId { get; set; }
        public long FSrcStockId_Id { get; set; }
        public DynamicObject FSrcStockId { get; set; }
        public decimal FQty { get; set; }
        public decimal FBaseQty { get; set; }
        public long FOwnerId { get; set; }
    }
}
