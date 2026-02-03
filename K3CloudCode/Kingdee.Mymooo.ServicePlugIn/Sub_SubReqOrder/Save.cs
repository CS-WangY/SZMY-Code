using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.PLN.Reserved.ReserveArgs;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN.Reserve;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.ApigatewayConfiguration.EnterpriseWeChat;
using Kingdee.Mymooo.Core.ReserveLinkManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.Mymooo.App.Core.SalesManagement;
using SupplyViewItem = Kingdee.Mymooo.Core.SalesManagement.SupplyViewItem;
using DemandView = Kingdee.Mymooo.Core.SalesManagement.DemandView;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.Sub_SubReqOrder
{
    [Description("采购申请订单下推委外订单保存转移预留插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("SrcBillNo");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var billitem in e.DataEntitys)
            {
                //获取明细数据
                var detDynamicObject = billitem["TreeEntity"] as DynamicObjectCollection;
                foreach (var item in detDynamicObject)
                {
                    var srcbillno = Convert.ToString(item["SrcBillNo"]);
                    string sSql = $"SELECT FID FROM T_PLN_RESERVELINKENTRY WHERE FSUPPLYENTRYID={Convert.ToInt64(item["Id"])}";
                    var resid = DBUtils.ExecuteScalar<long>(this.Context, sSql, 0);
                    if (resid > 0)
                    {
                        continue;
                    }
                    var reserveLinkEntry = new ReserveLinkEntry();
                    sSql = $"SELECT FID,FENTRYID FROM dbo.T_PLN_RESERVELINKENTRY WHERE FSUPPLYBILLNO='{srcbillno}'";
                    var plndata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                    long srcresId = 0, srcresEntId = 0;
                    if (plndata.Count > 0)
                    {
                        srcresId = Convert.ToInt64(plndata.First()["FID"]);
                        srcresEntId = Convert.ToInt64(plndata.First()["FENTRYID"]);
                    }
                    else
                    {
                        continue;
                    }

                    reserveLinkEntry.FSUPPLYFORMID = "SUB_SUBREQORDER";
                    reserveLinkEntry.FSUPPLYINTERID = Convert.ToString(billitem["Id"]);
                    reserveLinkEntry.FSUPPLYBILLNO = Convert.ToString(billitem["BillNo"]);
                    reserveLinkEntry.FSUPPLYENTRYID = Convert.ToString(item["Id"]);
                    reserveLinkEntry.FMATERIALID = Convert.ToInt64(item["MaterialId_Id"]);
                    reserveLinkEntry.FSUPPLYORGID = Convert.ToInt64(billitem["SubOrgId_Id"]);
                    reserveLinkEntry.FSUPPLYDATE = Convert.ToDateTime(System.DateTime.Now);
                    reserveLinkEntry.FSUPPLYBOMID = Convert.ToInt64(item["BomId_Id"]);
                    reserveLinkEntry.FBASESUPPLYUNITID = Convert.ToInt64(item["BaseUnitId_Id"]);
                    reserveLinkEntry.FBASEQTY = Convert.ToDecimal(item["Qty"]);
                    reserveLinkEntry.FLINKTYPE = 2;
                    reserveLinkEntry.FSUPPLYLOT = 80;
                    reserveLinkEntry.FYieldRate = 100;
                    reserveLinkEntry.FINTSUPPLYID = Convert.ToInt64(billitem["Id"]);
                    reserveLinkEntry.FINTSUPPLYENTRYID = Convert.ToInt64(item["Id"]);
                    reserveLinkEntry.FSupplyRemarks = "采购申请转委外生成委外预留";

                    insertReserve(this.Context, srcresId, srcresEntId, reserveLinkEntry);
                }
            }
        }

        private void insertReserve(Context ctx, long _resid, long _resEntID, ReserveLinkEntry reserveLinkEntry)
        {
            var billView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", _resid);
            //取上游预留信息
            string sSql = $@"SELECT FDEMANDFORMID AS DemandFormID,FDEMANDINTERID AS DemandInterID,FDEMANDENTRYID AS DemandEntryID,FDEMANDBILLNO AS DemandBillNO,
FSRCFORMID AS SrcDemandFormId,FSRCINTERID AS SrcDemandInterId,FSRCENTRYID AS SrcDemandEntryId,FSRCBILLNO AS SrcDemandBillNo,
FDEMANDORGID AS DemandOrgID,FMATERIALID AS MaterialID,FBASEDEMANDUNITID AS BaseUnitID,FBASEDEMANDQTY AS BaseQty
FROM T_PLN_RESERVELINK WHERE FID={_resid}";
            var salbilldata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            var reserveLink = new ReserveLink();
            if (salbilldata.Count > 0)
            {
                reserveLink.FDEMANDFORMID = salbilldata[0]["DemandFormID"].ToString();
                reserveLink.FDEMANDINTERID = salbilldata[0]["DemandInterID"].ToString();
                //demandView.DemandEntryID = string.Empty;
                reserveLink.FDemandENTRYID = salbilldata[0]["DemandEntryID"].ToString();
                reserveLink.FDEMANDBILLNO = salbilldata[0]["DemandBillNO"].ToString();

                reserveLink.FSRCFORMID = salbilldata[0]["SrcDemandFormId"].ToString();
                reserveLink.FSRCINTERID = salbilldata[0]["SrcDemandInterId"].ToString();
                reserveLink.FSRCENTRYID = salbilldata[0]["SrcDemandEntryId"].ToString();
                reserveLink.FSRCBILLNO = salbilldata[0]["SrcDemandBillNo"].ToString();

                reserveLink.FDEMANDORGID = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
                reserveLink.FDEMANDDATE = System.DateTime.Now;
                reserveLink.FMATERIALID = Convert.ToInt64(salbilldata[0]["MaterialID"]);
                reserveLink.FBASEDEMANDUNITID = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
                reserveLink.FBASEDEMANDQTY = Convert.ToDecimal(salbilldata[0]["BaseQty"]);
            }

            BusinessDataServiceHelper.Delete(this.Context, billView.BusinessInfo, new object[] { _resid });

            CreateReserveLink(reserveLink, reserveLinkEntry);
        }

        public void CreateReserveLink(ReserveLink reserveLink, ReserveLinkEntry reserveLinkEntry)
        {

            DemandView demandView = new DemandView();

            demandView.DemandFormID = reserveLink.FDEMANDFORMID;
            demandView.DemandInterID = reserveLink.FDEMANDINTERID;
            //demandView.DemandEntryID = string.Empty;
            demandView.DemandEntryID = reserveLink.FDemandENTRYID;
            demandView.DemandBillNO = reserveLink.FDEMANDBILLNO;

            demandView.SrcDemandFormId = reserveLink.FSRCFORMID;
            demandView.SrcDemandInterId = reserveLink.FSRCINTERID;
            demandView.SrcDemandEntryId = reserveLink.FSRCENTRYID;
            demandView.SrcDemandBillNo = reserveLink.FSRCBILLNO;

            demandView.DemandOrgID_Id = reserveLink.FDEMANDORGID;
            demandView.DemandDate = System.DateTime.Now;
            demandView.MaterialID_Id = reserveLink.FMATERIALID;
            demandView.BaseUnitID_Id = reserveLink.FBASEDEMANDUNITID;
            demandView.BaseQty = reserveLink.FBASEDEMANDQTY;

            //取下游预留信息
            List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
            SupplyViewItem subRowView = new SupplyViewItem();

            //subRowView.SupplyFormID_Id = "STK_Inventory";
            subRowView.SupplyFormID_Id = reserveLinkEntry.FSUPPLYFORMID;
            subRowView.SupplyInterID = reserveLinkEntry.FSUPPLYINTERID;
            subRowView.SupplyEntryId = reserveLinkEntry.FSUPPLYENTRYID;
            subRowView.SupplyBillNO = reserveLinkEntry.FSUPPLYBILLNO;
            subRowView.SupplyMaterialID_Id = reserveLinkEntry.FMATERIALID;
            subRowView.BaseSupplyUnitID_Id = reserveLinkEntry.FBASESUPPLYUNITID;
            subRowView.SupplyBomID_Id = reserveLinkEntry.FSUPPLYBOMID;
            //供应数量
            subRowView.BaseActSupplyQty = reserveLinkEntry.FBASEQTY;
            subRowView.SupplyOrgID_Id = reserveLinkEntry.FSUPPLYORGID;

            subRowView.SupplyDate = System.DateTime.Now;
            //subRowView.SupplyStockID_Id = 0;
            subRowView.SupplyAuxproID_Id = 0;

            viewItems.Add(subRowView);

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
                row.SupplyBillInfo.EntryID = subRowView.SupplyEntryId;
                row.SupplyBillInfo.BillNo = subRowView.SupplyBillNO;
                row.SupplyMaterialID = subRowView.SupplyMaterialID_Id;
                row.SupplyOrgID = subRowView.SupplyOrgID_Id;
                row.SupplyDate = subRowView.SupplyDate;
                row.SupplyStockID = subRowView.SupplyStockID_Id;
                //row.SupplyStockLocID = subRowView.SupplyStockLocID_Id;
                row.SupplyBomID = subRowView.SupplyBomID_Id;
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
}
