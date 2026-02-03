using Kingdee.BOS.Core;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.Mymooo.ServicePlugIn.SalesBill;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Util;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using System.Data;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Kingdee.Mymooo.Core.StockManagement;
using System.Security.Cryptography;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Orm;
using Kingdee.K3.Core.MFG.PLN.Reserved.ReserveArgs;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN.Reserve;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using System.Web.Util;
using Kingdee.BOS.Core.Bill;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Contracts;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice
{
    [Description("发货通知单提交验证可用库存插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Submit : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FIsShipInspect");
            e.FieldKeys.Add("FPENYNOTE");
            e.FieldKeys.Add("FSaleOrgId");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FDeliveryDetOrgID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FStockID");
            e.FieldKeys.Add("FIsOutSourceStock");
            e.FieldKeys.Add("FSPLITLOCKENTRYID");
            e.FieldKeys.Add("SplitLockEntryId");
            e.FieldKeys.Add("FSrcType");
            //锁库拆分分录id
            e.FieldKeys.Add("FSPLITLOCKENTRYID");
            e.FieldKeys.Add("FSOEntryId");
            e.FieldKeys.Add("FUnitID");
        }
        //验证
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            SubmitMultipleSelValidator submitMultipleSelValidator = new SubmitMultipleSelValidator();
            submitMultipleSelValidator.AlwaysValidate = true;
            submitMultipleSelValidator.EntityKey = "FBillHead";
            e.Validators.Add(submitMultipleSelValidator);

            SubmitValidator isPoValidator = new SubmitValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }

    }

    [Description("发货通知单提交锁定预留信息"), HotUpdate]
    public class SubmitLock : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FDeliveryDetOrgID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FStockID");
            e.FieldKeys.Add("FIsOutSourceStock");
            e.FieldKeys.Add("FSPLITLOCKENTRYID");
            e.FieldKeys.Add("SplitLockEntryId");
            e.FieldKeys.Add("FSrcType");
            e.FieldKeys.Add("FSupplyTargetOrgId");
        }

        public bool IsDirStock = false;
        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {

                //如果供货组织是深圳则跳出
                //if (Convert.ToInt64(item["DeliveryOrgID_Id"]) == 224428) continue;
                //如果不是则跳出深圳224428广东蚂蚁669144北京蚂蚁1043841江苏蚂蚁7348029
                long[] SalOrgList = new long[] { 224428, 669144, 1043841, 7348029 };
                if (!SalOrgList.Contains(Convert.ToInt64(item["SaleOrgId_Id"])))
                {
                    continue;
                }
                if (((DynamicObject)item["BillTypeID"])["Number"].ToString() == "FHTZD01_PENY")
                {
                    IsDirStock = true;
                }
                var fid = Convert.ToInt64(item["Id"]);
                var billtype = item["BillTypeID_Id"].ToString();
                var billno = item["BillNo"].ToString();
                foreach (var entitem in item["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
                {
                    var entryid = Convert.ToInt64(entitem["Id"]);
                    var billseq = Convert.ToInt32(entitem["Seq"]);
                    var fqty = Convert.ToDecimal(entitem["Qty"]);
                    //取物料的mster码
                    var materialid = entitem["materialid"] as DynamicObject;
                    var msterID = Convert.ToString(materialid["msterID"]);

                    var srcunitID = Convert.ToInt64(entitem["UnitID_Id"]);
                    //取发货组织
                    var tgOrgID = Convert.ToInt64(entitem["FSupplyTargetOrgId_Id"]);
                    //var orgid = entitem["FDeliveryDetOrgID"] as DynamicObject;
                    //var orgnumber = Convert.ToString(orgid["Number"]);
                    List<long> detOrgID = new List<long>();
                    detOrgID.Add(tgOrgID);

                    long srcbillid = 0;
                    long srcrowid = 0;
                    foreach (var itemlink in entitem["FEntity_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = Convert.ToInt64(itemlink["SBillId"]);
                        srcrowid = Convert.ToInt64(itemlink["SId"]);
                    }
                    //没有上游单据的不校验
                    if (srcrowid.IsNullOrEmptyOrWhiteSpace()) return;

                    //取当前单据预留数量
                    var resqty = GetReservelinkQty(srcbillid, srcrowid, srcunitID);
                    //判断是否存在未调拨完的发货通知单 剩余需求数量+基本单位调出关联数量-需求数量
                    string sSql = $@"SELECT SUM(t3.FREMAINQTY+t3.FBASETRANOUTQTY-t3.FDEMANDQTY) FROM T_PLN_RESERVELINKENTRY t1
                                INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                                INNER JOIN T_PLN_REQUIREMENTORDER t3 ON t1.FSUPPLYINTERID=t3.FID
                                WHERE t2.FSRCINTERID='{srcbillid}' AND t2.FSRCENTRYID='{srcrowid}'
                                AND t1.FSUPPLYFORMID='PLN_REQUIREMENTORDER' AND t3.FREMAINQTY+t3.FBASETRANOUTQTY>t3.FDEMANDQTY";
                    var tranqty = DBUtils.ExecuteScalar<decimal>(this.Context, sSql, 0);
                    //取累计发货数量未审核的部分
                    sSql = $@"SELECT SUM(t2.FQTY) AS FQTY FROM T_SAL_DELIVERYNOTICEENTRY_LK t1
                                INNER JOIN T_SAL_DELIVERYNOTICEENTRY t2 ON t1.FENTRYID=t2.FENTRYID
                                INNER JOIN T_SAL_DELIVERYNOTICE t3 ON t2.FID=t3.FID
                                WHERE FSBILLID='{srcbillid}' AND FSID='{srcrowid}' AND t3.FDOCUMENTSTATUS IN ('B')";
                    var baseqty = DBUtils.ExecuteScalar<decimal>(this.Context, sSql, 0);

                    if (baseqty > resqty - tranqty)
                    {
                        //如果有预留信息，则报错
                        sSql = $@"SELECT * FROM T_PLN_RESERVELINKENTRY t1
                        INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        WHERE t2.FDEMANDINTERID='{srcbillid}' AND t2.FDEMANDENTRYID='{srcrowid}'
                        AND (t1.FSUPPLYORGID<>224428 OR t1.FSUPPLYFORMID='PLN_PLANORDER')";
                        var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                        if (datas.Count > 0)
                        {
                            throw new Exception("第" + billseq + "行" + materialid["Name"] + ",存在计划中的需求单，请按照需求单检查可发货数量！");
                        }
                        //创建组织间需求单 
                        GetSaleRequirementorder(billseq, baseqty, srcbillid, srcrowid, materialid);
                        //创建库存预留
                        var locklist = LockStock(Convert.ToInt64(materialid["Id"]), detOrgID, baseqty - resqty);
                        StockReserveLink(srcbillid, srcrowid, locklist, billseq, materialid);
                    }
                    resqty = GetReservelinkQty(srcbillid, srcrowid, srcunitID);
                    //取累计发货数量-累计出库数量
                    sSql = $@"SELECT SUM(t2.FQTY-te.FSTOCKBASEJOINOUTQTY) AS FQTY FROM T_SAL_DELIVERYNOTICEENTRY_LK t1
                                INNER JOIN T_SAL_DELIVERYNOTICEENTRY t2 ON t1.FENTRYID=t2.FENTRYID
                                INNER JOIN T_SAL_DELIVERYNOTICEENTRY_E te ON t2.FENTRYID=te.FENTRYID
                                INNER JOIN T_SAL_DELIVERYNOTICE t3 ON t2.FID=t3.FID
                                WHERE t3.FCANCELSTATUS<>'B'
								AND FSBILLID='{srcbillid}' AND FSID='{srcrowid}'";
                    var ljfh = DBUtils.ExecuteScalar<decimal>(this.Context, sSql, 0);
                    if (ljfh > resqty)
                    {
                        throw new Exception("第" + billseq + "行" + materialid["Name"] + ",存在还在进行中的发货通知单，请检查！");
                    }
                }
            }
        }

        private decimal GetReservelinkQty(long srcbillid, long srcrowid, long srcUnitID)
        {
            var sSql = $@"/*dialect*/SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FBASEUNITID,t2.FMATERIALID
                                FROM T_PLN_RESERVELINKENTRY t1
                                INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                                WHERE t2.FSRCINTERID='{srcbillid}' AND t2.FSRCENTRYID='{srcrowid}'
                                AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                                AND t1.FSUPPLYFORMID='STK_Inventory'
                                GROUP BY t1.FBASEUNITID,t2.FMATERIALID";
            var resdata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            long resunitID = 0;
            decimal resqty = 0;
            long materialid = 0;
            if (resdata.Count > 0)
            {
                resqty = Convert.ToDecimal(resdata[0]["FBASEQTY"]);
                resunitID = Convert.ToInt64(resdata[0]["FBASEUNITID"]);
                materialid = Convert.ToInt64(resdata[0]["FMATERIALID"]);
            }
            if (srcUnitID != resunitID)
            {
                IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                resqty = convertService.GetUnitTransQty(this.Context, materialid, resunitID, srcUnitID, resqty);
            }
            return resqty;
        }

        public List<lockinfo> LockStock(long msterID, List<long> orgid, decimal qty)
        {
            List<lockinfo> list = new List<lockinfo>();
            var invqty = StockQuantityServiceHelper.InventoryQty(this.Context, msterID, orgid);
            decimal dqty = qty;
            //如果是直发类型只取直发仓库存
            var resqtylist = invqty;
            if (IsDirStock)
            {
                var doclist = invqty.Where(x => Convert.ToInt16(x["FISDIRSTOCK"]) == 1).ToList();
                DynamicObjectCollection filterList = new DynamicObjectCollection(invqty.DynamicCollectionItemPropertyType);

                foreach (var item in doclist)
                {
                    filterList.Add(item);
                }
                resqtylist = filterList;
            }
            foreach (var item in resqtylist)
            {
                if (Convert.ToDecimal(item["FBASEQTY"]) > 0 && dqty > 0)
                {
                    lockinfo lockinfo = new lockinfo();
                    lockinfo.id = Convert.ToString(item["FID"]);
                    lockinfo.materialid = Convert.ToString(item["FMATERIALID"]);
                    lockinfo.stockorgid = Convert.ToString(item["FSTOCKORGID"]);
                    lockinfo.stockid = Convert.ToString(item["FSTOCKID"]);
                    lockinfo.baseunitid = Convert.ToString(item["FBASEUNITID"]);

                    if (Convert.ToDecimal(item["FBASEQTY"]) - dqty < 0)
                    {
                        dqty = dqty - Convert.ToDecimal(item["FBASEQTY"]);

                        lockinfo.qty = Convert.ToDecimal(item["FBASEQTY"]);
                        list.Add(lockinfo);
                    }
                    else
                    {
                        lockinfo.qty = dqty;
                        list.Add(lockinfo);
                        dqty = dqty - Convert.ToDecimal(item["FBASEQTY"]);
                    }
                }
            }
            return list;
        }
        public class lockinfo
        {
            public string id { get; set; }
            public string materialid { get; set; }
            public string stockorgid { get; set; }
            public string stockid { get; set; }
            public string baseunitid { get; set; }

            public decimal qty { get; set; }
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

        public void GetSaleRequirementorder(int billseq, decimal FQTY, long billid, long entryid, DynamicObject materialid)
        {
            List<long> detOrgID = new List<long>();
            //判断是否有组织间需求单需求数量
            string sSql = $@"SELECT ISNULL(SUM(t1.FBASEQTY),0) BASEQTY FROM T_PLN_RESERVELINKENTRY t1
INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
INNER JOIN T_PLN_REQUIREMENTORDER t3 ON t1.FSUPPLYINTERID=t3.FID
WHERE t2.FDEMANDINTERID='{billid}' AND t2.FDEMANDENTRYID='{entryid}' AND t1.FSUPPLYFORMID='PLN_REQUIREMENTORDER'";
            var demqty = DBUtils.ExecuteScalar<decimal>(this.Context, sSql, 0);

            if (demqty == 0)
            {
                //没有组织间需求单则下推新的组织间需求单
                var saveResult = PushReq(this.Context, billid.ToString(), entryid.ToString(), FQTY);
                //创建与销售订单的预留关系
                var pkids = saveResult.SuccessDataEnity.Select(p => Convert.ToInt64(p["Id"])).FirstOrDefault();
                CreateReserveLink(entryid, pkids);
            }
            else if (FQTY > demqty)
            {
                var saveResult = PushReq(this.Context, billid.ToString(), entryid.ToString(), FQTY - demqty);
                //创建与销售订单的预留关系
                var pkids = saveResult.SuccessDataEnity.Select(p => Convert.ToInt64(p["Id"])).FirstOrDefault();
                CreateReserveLink(entryid, pkids);
            }
        }

        public void CreateReserveLink(long salentid, long reqid)
        {
            //取销售订单信息
            string sSql = $@"SELECT t2.FOBJECTTYPEID as DemandFormID,t1.FID as DemandInterID,t2.FBILLNO as DemandBillNO,
t2.FOBJECTTYPEID as SrcDemandFormId,t1.FID as SrcDemandInterId,t1.FENTRYID as SrcDemandEntryId,t2.FBILLNO as SrcDemandBillNo,
t1.FSTOCKORGID as DemandOrgID,t1.FMATERIALID as MaterialID,t1.FBASEUNITID as BaseUnitID,t1.FQTY as BaseQty,
t1.FSUPPLYORGID
FROM T_SAL_ORDERENTRY t1
LEFT JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
WHERE t1.FENTRYID={salentid}";
            var salbilldata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            DemandView demandView = new DemandView();
            if (salbilldata.Count > 0)
            {
                demandView.DemandFormID = salbilldata[0]["DemandFormID"].ToString();
                demandView.DemandInterID = salbilldata[0]["DemandInterID"].ToString();
                //demandView.DemandEntryID = string.Empty;
                demandView.DemandEntryID = salentid.ToString();
                demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

                demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
                demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
                demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
                demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

                demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
                demandView.DemandDate = System.DateTime.Now;
                demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
                demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
                demandView.BaseQty = Convert.ToDecimal(salbilldata[0]["BaseQty"]);
            }
            //取组织间需求单信息
            sSql = $@"SELECT FFORMID,FID AS SupplyInterID,FBILLNO AS SupplyBillNO,FSUPPLYMATERIALID AS SupplyMaterialID
,FUNITID AS BaseSupplyUnitID,FDEMANDQTY AS BaseActSupplyQty,FSUPPLYORGANID AS SupplyOrgID
FROM T_PLN_REQUIREMENTORDER WHERE FID={reqid}";
            var reqbilldata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
            SupplyViewItem subRowView = new SupplyViewItem();
            if (reqbilldata.Count > 0)
            {
                //subRowView.SupplyFormID_Id = "STK_Inventory";
                subRowView.SupplyFormID_Id = Convert.ToString(reqbilldata[0]["FFORMID"]);
                subRowView.SupplyInterID = Convert.ToString(reqbilldata[0]["SupplyInterID"]);
                subRowView.SupplyBillNO = Convert.ToString(reqbilldata[0]["SupplyBillNO"]);
                subRowView.SupplyMaterialID_Id = Convert.ToInt64(reqbilldata[0]["SupplyMaterialID"]);
                subRowView.BaseSupplyUnitID_Id = Convert.ToInt64(reqbilldata[0]["BaseSupplyUnitID"]);
                //供应数量
                //subRowView.BaseActSupplyQty = Convert.ToInt64(reqbilldata[0]["BaseActSupplyQty"]);
                subRowView.SupplyOrgID_Id = Convert.ToInt64(reqbilldata[0]["SupplyOrgID"]);

                subRowView.SupplyDate = System.DateTime.Now;
                //subRowView.SupplyStockID_Id = 0;
                subRowView.SupplyAuxproID_Id = 0;

                viewItems.Add(subRowView);
            }
            demandView.supplyView = viewItems;

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

        public void StockReserveLink(long billid, long entryid, List<lockinfo> locklist, int fseq, DynamicObject materialid)
        {
            var sSql = $@"SELECT t1.FSUPPLYINTERID,t1.FBASEQTY FROM T_PLN_RESERVELINKENTRY t1
                        LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        WHERE t2.FSRCINTERID='{billid}' AND t2.FSRCENTRYID='{entryid}'
                        AND t1.FSUPPLYFORMID='PLN_REQUIREMENTORDER'";
            var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);

            foreach (var stockitem in locklist)
            {
                decimal stoqty = stockitem.qty;
                foreach (var item in datas)
                {
                    //取组织间需求单下的预留数量
                    sSql = $@"SELECT ISNULL(SUM(t1.FBASEQTY),0) AS FBASEQTY
                            FROM T_PLN_RESERVELINKENTRY t1
                            LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID WHERE t2.FDEMANDINTERID='{item["FSUPPLYINTERID"]}'
                            AND t1.FSUPPLYFORMID<>'STK_Inventory'";
                    var wayqty = DBUtils.ExecuteScalar<decimal>(this.Context, sSql, 0);
                    if (wayqty > 0)
                    {
                        throw new Exception($"第{fseq}行,物料[{materialid["Number"]}]存在在途的预留，请处理后再超发货!");
                    }
                    else
                    {
                        sSql = $@"SELECT ISNULL(SUM(t1.FBASEQTY),0) AS FBASEQTY
                            FROM T_PLN_RESERVELINKENTRY t1
                            LEFT JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID WHERE t2.FDEMANDINTERID='{item["FSUPPLYINTERID"]}'
                            AND t1.FSUPPLYFORMID='STK_Inventory' AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'";
                        var zyqty = DBUtils.ExecuteScalar<decimal>(this.Context, sSql, 0);
                        decimal sendqty = 0;
                        var kzyqty = Convert.ToDecimal(item["FBASEQTY"]) - zyqty;
                        if (kzyqty <= 0) continue;
                        if (kzyqty - stoqty < 0)
                        {
                            sendqty = kzyqty;
                            stoqty = stoqty - kzyqty;
                        }
                        else
                        {
                            sendqty = stoqty;
                        }
                        if (sendqty > 0)
                        {
                            CreateStockReserveLink(Convert.ToInt64(item["FSUPPLYINTERID"]), stockitem, sendqty);
                        }
                    }


                }
            }

        }

        public void CreateStockReserveLink(long reqid, lockinfo itemlock, decimal qty)
        {
            string sSql = $"SELECT FID FROM T_PLN_RESERVELINK WHERE FDEMANDINTERID='{reqid}' AND FDEMANDFORMID='PLN_REQUIREMENTORDER'";
            var resid = DBUtils.ExecuteScalar<long>(this.Context, sSql, 0);
            if (resid == 0)
            {
                CreateReserveLink(reqid, itemlock, qty);
            }
            else
            {
                var billView = FormMetadataUtils.CreateBillView(this.Context, "PLN_RESERVELINK", resid);
                billView.Model.CreateNewEntryRow("FEntity");
                var seq = billView.Model.GetEntryRowCount("FEntity") - 1;
                billView.Model.SetValue("FSUPPLYFORMID", "STK_Inventory", seq);
                billView.Model.SetItemValueByID("FSUPPLYINTERID", Convert.ToString(itemlock.id), seq);

                billView.Model.SetItemValueByID("FSUPPLYMATERIALID", Convert.ToInt64(itemlock.materialid), seq);
                billView.Model.SetItemValueByID("FSUPPLYORGID", Convert.ToInt64(itemlock.stockorgid), seq);
                billView.Model.SetValue("FSUPPLYDATE", System.DateTime.Now, seq);
                billView.Model.SetItemValueByID("FSUPPLYSTOCKID", Convert.ToInt64(itemlock.stockid), seq);
                //subRowView.SupplyAuxproID_Id = 0;
                billView.Model.SetItemValueByID("FBASESUPPLYUNITID", Convert.ToInt64(itemlock.baseunitid), seq);
                //供应数量
                billView.Model.SetValue("FBASESUPPLYQTY", qty, seq);
                billView.Model.SetItemValueByID("FINTSUPPLYID", Convert.ToInt64(reqid), seq);

                //billView.InvokeFieldUpdateService("FMATERIALIDCHILD", 0);
                List<DynamicObject> list = new List<DynamicObject>();
                list.Add(billView.Model.DataObject);
                var oper = SaveBill(this.Context, billView.BusinessInfo, list.ToArray());
                if (!oper.IsSuccess)
                {

                }
            }

        }


        public IOperationResult PushReq(Context ctx, string billid, string entryid, decimal salqty)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            var row = new ListSelectedRow(billid, entryid, 0, "SAL_SaleOrder");
            row.EntryEntityKey = "FSaleOrderEntry"; //这里最容易忘记加，是重点的重点
            selectedRows.Add(row);

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "PENY_SalOrder_REQUIREMENTORDER";
            //push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
            //push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);

            var result = this.BillPush(ctx, push, salqty);
            return result;
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
                targeEntry["F_PENY_Amount"] = salqty * Convert.ToDecimal(targeEntry["F_PENY_Price"]);
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
        }

        /// <summary>
        /// 保存目标单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="targetBusinessInfo"></param>
        /// <param name="targetBillObjs"></param>
        private IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
        {
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreInteractionFlag(true);
            saveOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
            //保存
            SaveService saveService = new SaveService();
            IOperationResult saveResult = new OperationResult();

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

        private IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
        {
            SaveService saveService = new SaveService();
            IOperationResult operationResult;
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            operationResult = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);
            return operationResult;
        }
    }

    
}
