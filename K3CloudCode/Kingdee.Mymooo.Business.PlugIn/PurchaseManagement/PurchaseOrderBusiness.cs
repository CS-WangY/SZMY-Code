using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.PreInsertData.NetWorkCtrl;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI.WebControls;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Model.ReportFilter;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.ComponentModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.K3.Core.SCM.Mobile;
using Kingdee.K3.Core.SCM;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.WebApi.FormService;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    /// <summary>
    /// 采购订单
    /// </summary>
    public class PurchaseOrderBusiness
    {
        /// <summary>
        /// mes原材料采购申请
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="bill"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> Add_Mes_PUR_Requisition(Context ctx, PUR_Requisition bill)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PUR_Requisition") as FormMetadata;
            var billView = FormMetadataUtils.CreateBillView(ctx, "PUR_Requisition");
            billView.Model.SetValue("FBillTypeID", bill.BillTypeID, 0);
            billView.Model.SetValue("FBillNO", bill.BillNo, 0);
            billView.Model.SetValue("FPENYBackupBillNO", bill.BillNo, 0);
            billView.Model.SetItemValueByNumber("FApplicationOrgId", bill.FApplicationOrgId, 0);

            //根据微信编号获取员工id
            string sSql = $@"SELECT FNUMBER FROM T_HR_EMPINFO
                            WHERE FWECHATCODE='{bill.FApplicantId}'";
            var userid = DBServiceHelper.ExecuteScalar<string>(ctx, sSql, "");
            billView.Model.SetItemValueByNumber("FApplicantId", userid, 0);
            billView.InvokeFieldUpdateService("FApplicantId", 0);

            billView.Model.DeleteEntryData("FEntity");
            var rowcount = 0;
            foreach (var item in bill.Entry)
            {
                billView.Model.CreateNewEntryRow("FEntity");
                billView.Model.SetItemValueByNumber("FMaterialId", item.FMaterialId, rowcount);
                billView.InvokeFieldUpdateService("FMaterialId", rowcount);
                billView.Model.SetValue("FReqQty", item.FReqQty, rowcount);
                billView.InvokeFieldUpdateService("FReqQty", rowcount);
                billView.Model.SetValue("FSUPPLIERUNITPRICE", item.FSUPPLIERUNITPRICE, rowcount);
                billView.Model.SetValue("FEntryNote", item.FEntryNote, rowcount);
                billView.Model.SetItemValueByNumber("FSrcBillTypeId", item.SrcBillTypeId, rowcount);
                billView.Model.SetValue("FSrcBillNo", item.SrcBillNo, rowcount);
                billView.Model.SetValue("FDEMANDTYPE", item.DEMANDTYPE, rowcount);
                billView.Model.SetValue("FDEMANDBILLNO", item.DEMANDBILLNO, rowcount);
                billView.Model.SetValue("FDEMANDBILLENTRYSEQ", item.DEMANDBILLENTRYSEQ, rowcount);
                billView.Model.SetValue("FDEMANDBILLENTRYID", item.DEMANDBILLENTRYID, rowcount);
                billView.Model.SetValue("FBUSINESSDIVISIONID", item.BUSINESSDIVISIONID, rowcount);
                billView.Model.SetValue("FSoNo", item.SoNo, rowcount);
                billView.Model.SetValue("FSoSeq", item.SoSeq, rowcount);
                billView.Model.SetValue("FPENYSALERS", item.salCreatorName, rowcount);
                billView.Model.SetValue("FPENYDELIVERYDATE", item.PENYDELIVERYDATE, rowcount);
                billView.Model.SetValue("FCUSTMATERIALNO", item.CUSTMATERIALNO, rowcount);
                billView.Model.SetValue("FPENYMAPCODE", item.PENYMAPCODE, rowcount);
                billView.Model.SetValue("FPENYMAPNAME", item.PENYMAPNAME, rowcount);
                rowcount++;
            }

            IOperationResult oper = new OperationResult();
            response.Code = ResponseCode.Success;
            response.Message = "创建成功";
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);

                SaveService saveService = new SaveService();
                oper = saveService.SaveAndAudit(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
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
                cope.Complete();
            }
            response.Data = oper.OperateResult.Select(x => new { x.PKValue, x.Number, x.Message });
            return response;
        }
        public ResponseMessage<PUR_Requisition> SyncMes_PUR_RequisitionMaterial(Context ctx, PUR_Requisition request)
        {
            ResponseMessage<PUR_Requisition> response = new ResponseMessage<PUR_Requisition>() { Data = request };
            Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var orgid = GetOrgIdByOrgNumber(ctx, request.FApplicationOrgId);
                List<long> materialss = new List<long>();
                foreach (var detail in request.Entry)
                {
                    detail.FMaterialId = detail.FMaterialId.Trim();
                    detail.FMaterialName = detail.FMaterialName.Trim();

                    var materialInfo = new MaterialInfo(detail.FMaterialId, detail.FMaterialName);
                    if (materials.ContainsKey(detail.FMaterialId))
                    {
                        materialInfo = materials[detail.FMaterialId];
                    }
                    else
                    {
                        SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
                        productsmallclass.Id = GetSMALLID(ctx, detail.ProductSmallClassId);

                        string baseUnit = detail.VolumeUnitid == "PCS" ? "Pcs" : detail.VolumeUnitid;
                        string stockUnit = detail.VolumeUnitid == "PCS" ? "Pcs" : detail.VolumeUnitid;
                        string purchaseUnit = detail.VolumeUnitid == "PCS" ? "Pcs" : detail.VolumeUnitid;
                        string saleUnit = detail.VolumeUnitid == "PCS" ? "Pcs" : detail.VolumeUnitid;
                        if (detail.VolumeUnitid.EqualsIgnoreCase("mm"))
                        {
                            baseUnit = "m";
                            stockUnit = "mm";
                            purchaseUnit = "mm";
                            saleUnit = "mm";
                        }
                        materialInfo.FBaseUnitId = baseUnit;
                        materialInfo.FStoreUnitID = stockUnit;
                        materialInfo.FPurchaseUnitId = purchaseUnit;
                        materialInfo.FPurchasePriceUnitId = purchaseUnit;
                        materialInfo.FSaleUnitId = saleUnit;
                        materialInfo.Length = detail.Length;
                        materialInfo.Width = detail.Width;
                        materialInfo.Height = detail.Height;
                        materialInfo.Weight = detail.Weight;
                        materialInfo.Volume = detail.Volume;
                        materialInfo.Textures = detail.Textures;
                        materialInfo.ProductId = detail.ProductId;
                        materialInfo.ProductSmallClass = productsmallclass;
                        materialInfo.WeightUnitid = Convert.ToString(GetUnitId(ctx, detail.WeightUnitid));
                        materialInfo.VolumeUnitid = Convert.ToString(GetUnitId(ctx, detail.WeightUnitid));

                        materialInfo = MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, new List<long>() { orgid });
                        materials[detail.FMaterialId] = materialInfo;
                    }
                    //detail.MaterialId = materialInfo.Id;
                    //detail.MaterialMasterId = materialInfo.MasterId;
                    materialss.Add(materialInfo.MasterId);
                }
                MaterialServiceHelper.MaterialAllocateToAll(ctx, materialss);
                response.Code = ResponseCode.Success;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Warning;
                response.Message = ex.ToString();
            }

            return response;
        }
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
        private long GetUnitId(Context ctx, string number)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UnitId", KDDbType.String, number) };
            var sql = $@"/*dialect*/SELECT FUNITID FROM dbo.T_BD_UNIT WHERE FNUMBER=@UnitId";
            return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }
        private long GetSMALLID(Context ctx, string Number)
        {
            string sSql = $"SELECT FID FROM dbo.T_BD_MATERIALSALGROUP WHERE FNUMBER='{Number}'";
            return DBServiceHelper.ExecuteScalar(ctx, sSql, 0);
        }
        public ResponseMessage<PUR_Requisition> SyncPUR_RequisitionMaterial(Context ctx, PUR_Requisition request)
        {
            ResponseMessage<PUR_Requisition> response = new ResponseMessage<PUR_Requisition>() { Data = request };
            Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>(StringComparer.OrdinalIgnoreCase);
            var orgid = GetOrgIdByOrgNumber(ctx, request.FApplicationOrgId);
            List<long> materialss = new List<long>();
            foreach (var detail in request.Entry)
            {
                detail.FMaterialId = detail.FMaterialId.Trim();
                detail.FMaterialName = detail.FMaterialName.Trim();

                var materialInfo = new MaterialInfo(detail.FMaterialId, detail.FMaterialName);
                if (materials.ContainsKey(detail.FMaterialId))
                {
                    materialInfo = materials[detail.FMaterialId];
                }
                else
                {
                    SalesOrderBillRequest.Productsmallclass productsmallclass = new SalesOrderBillRequest.Productsmallclass();
                    productsmallclass.Id = Convert.ToInt64(detail.ProductSmallClassId);
                    materialInfo.ProductId = detail.ProductId;
                    //materialInfo.UseOrgId = request.OrgId;
                    materialInfo.ProductSmallClass = productsmallclass;
                    materialInfo = MaterialServiceHelper.TryGetOrAdd(ctx, materialInfo, new List<long>() { orgid });
                    materials[detail.FMaterialId] = materialInfo;
                }
                //detail.MaterialId = materialInfo.Id;
                //detail.MaterialMasterId = materialInfo.MasterId;
                materialss.Add(materialInfo.MasterId);
            }

            MaterialServiceHelper.MaterialAllocateToAll(ctx, materialss);
            response.Code = ResponseCode.Success;
            return response;
        }
        /// <summary>
        /// 获取产品采购记录
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetPurchaseOrderSimple(Context ctx, string message)
        {
            List<string> request = JsonConvert.DeserializeObject<List<string>>(message);

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (request.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "请传物料号";
                return response;
            }
            return PurchaseOrderServiceHelper.GetPurchaseOrderSimpleAction(ctx, request);
        }

        /// <summary>
        /// 获取采购单列表
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetPurchaseOrder(Context ctx, string message)
        {
            PurchaseProductFilter request = JsonConvert.DeserializeObject<PurchaseProductFilter>(message);
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            response.Data = GetPurchaseOrderData(ctx, request); ;
            response.Code = ResponseCode.Success;
            response.Message = "获取成功";
            return response;

        }

        /// <summary>
        /// 获取采购单列表
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<PurchaseOrderDto> GetPurchaseOrderData(Context ctx, PurchaseProductFilter request)
        {
            var purchaseOrderList = new List<PurchaseOrderDto>();
            var localeId = ctx.UserLocale.LCID;

            var pars = new List<SqlParam>
                    {
                        new SqlParam("@PoDateBegin", KDDbType.DateTime,request.PurchaseDateBegin),
                        new SqlParam("@PoDateEnd", KDDbType.DateTime, request.PurchaseDateEnd),
                        new SqlParam("@localeId", KDDbType.Int64, localeId)
                    };

            var whereSql = "";
            if (!string.IsNullOrEmpty(request.ProductModel))
            {
                whereSql += " and ITEM_NO=@ITEM_NO";
                pars.Add(new SqlParam("@ITEM_NO", KDDbType.String, request.ProductModel));
            }
            if (!string.IsNullOrEmpty(request.ProductName))
            {
                whereSql += " and ITEM_DESC like'%'+@ProductName+'%'";
                pars.Add(new SqlParam("@ProductName", KDDbType.String, request.ProductName));
            }
            if (request.ClassId.HasValue)
            {
                whereSql += " and ITEM_GRP=@ITEM_GRP";
                pars.Add(new SqlParam("@ITEM_GRP", KDDbType.String, request.ClassId.Value.ToString()));
            }
            if (request.SmallClassId.HasValue)
            {
                whereSql += " and ITEM_TYPE=@ITEM_TYPE";
                pars.Add(new SqlParam("@ITEM_TYPE", KDDbType.String, request.SmallClassId.Value.ToString()));
            }

            if (!string.IsNullOrEmpty(request.PurchaseSupplierName))
            {
                whereSql += $" and VDR_NAMEC like'%{request.PurchaseSupplierName}%'";
            }
            string sql = $@"/*dialect*/select * from (
                            --select  convert(int,d.ProductId) ProductId,d.ITEM_GRP,d.GROUP_DESC,d.ITEM_TYPE,d.TYPE_DESC
                            -- ,m.PO_NO,PO_DATE,d.ITEM_NO,d.ITEM_DESC,m.VDR_CODE,m.VDR_NAMEC,
                            -- d.QTY,d.VAT_PRICE,ISNULL(d.UOM,sod.UOM) as UOM,convert(nvarchar(500),sod.SIDE_MARKB) SIDE_MARKB,DATEDIFF(DAY,m.PO_DATE,d.FTD) AS DeliveryDay
                            -- from M_POD_DET d
                            -- inner join M_PO_MSTR m on m.New_PO_NO=d.New_PO_NO
                            -- left join M_SOD_DET sod on  d.SO_NO=sod.SO_NO and d.SO_SEQ=sod.SEQ_NO and sod.COMP_CODE=d.COMP_CODE
                            -- where  m.APPROVE='Y' and m.VDR_INTERIOR=0 
                            -- union all
                                         select  mat.FPRODUCTID ProductId,convert(nvarchar,d.FPARENTSMALLID) ITEM_GRP
                                        ,gll.FNAME GROUP_DESC,convert(nvarchar,d.FSMALLID) ITEM_TYPE,gl.FNAME TYPE_DESC
                                         ,m.FBILLNO PO_NO,m.FDATE PO_DATE,mat.FNUMBER ITEM_NO,matL.FNAME ITEM_DESC, ven.FNUMBER VDR_CODE,ven_l.FNAME VDR_NAMEC,
                                         d.FQTY QTY,f.FTAXPRICE VAT_PRICE,u.FNUMBER UOM,d.FPENYMAPCODE SIDE_MARKB,DATEDIFF(DAY,m.FDATE,dd.FDELIVERYDATE) AS DeliveryDay
                                         from t_PUR_POOrderEntry d
                                         inner join t_PUR_POOrder m on m.FID=d.FID
                                         inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                                         inner join T_PUR_POORDERENTRY_D dd on dd.FENTRYID=d.FENTRYID
                                         left join  t_BD_Supplier ven on ven.FSUPPLIERID=m.FSUPPLIERID and ven.FCorrespondOrgId=0
                                         left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID
                                         left join  T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
                                         inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = @localeId
                                         left join T_BD_MATERIALGROUP_L gl on d.FSMALLID = gl.FID and gl.FLOCALEID = @localeId
				                         left join T_BD_MATERIALGROUP_L gll on d.FPARENTSMALLID = gll.FID and gll.FLOCALEID = @localeId
				                         left join t_BD_Unit u on d.FBASEUNITID=u.FUNITID
                                         where  m.FDOCUMENTSTATUS='C' 
		                    ) t
                        where PO_DATE between @PoDateBegin and @PoDateEnd " + whereSql;
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
            foreach (var reader in datas)
            {
                var purchaseOrder = new PurchaseOrderDto();
                purchaseOrder.ProductModel = Convert.ToString(reader["ITEM_NO"]);
                if (string.IsNullOrEmpty(purchaseOrder.ProductModel))
                {
                    continue;
                }
                purchaseOrder.ClassId = ConvertInt(reader["ITEM_GRP"]);
                purchaseOrder.ClassName = Convert.ToString(reader["GROUP_DESC"]);
                purchaseOrder.SmallClassId = ConvertInt(reader["ITEM_TYPE"]);
                purchaseOrder.SmallClassName = Convert.ToString(reader["TYPE_DESC"]);
                purchaseOrder.PurchaseOrderNumber = Convert.ToString(reader["PO_NO"]);
                purchaseOrder.PurchaseDate = Convert.ToDateTime(reader["PO_DATE"]).ToString("yyyy-MM-dd");  //采购日期
                purchaseOrder.ProductName = Convert.ToString(reader["ITEM_DESC"]);
                purchaseOrder.ProductId = Convert.ToInt32(reader["ProductId"]);
                purchaseOrder.SupplierCode = Convert.ToString(reader["VDR_CODE"]);
                purchaseOrder.SupplierName = Convert.ToString(reader["VDR_NAMEC"]);
                purchaseOrder.Qty = Convert.ToInt32(reader["QTY"]);
                purchaseOrder.Price = Convert.ToDecimal(reader["VAT_PRICE"]);
                purchaseOrder.CustomerModel = Convert.ToString(reader["SIDE_MARKB"]);
                purchaseOrder.Unit = Convert.ToString(reader["UOM"]);
                purchaseOrder.DeliveryDay = Convert.ToInt32(reader["DeliveryDay"]);
                purchaseOrderList.Add(purchaseOrder);
            }
            return purchaseOrderList;

        }

        public int ConvertInt(object val)
        {
            int returnValue = 0;
            try
            {
                returnValue = Convert.ToInt32(val);
            }
            catch (Exception)
            {
            }
            return returnValue;
        }

        /// <summary>
        /// 采购库存信息查询
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetPurchaseStockInfo(Context ctx, string message)
        {
            PageResqust<PurchaseStockRequest> filter = JsonConvertUtils.DeserializeObject<PageResqust<PurchaseStockRequest>>(message);
            PageReponse<PurchaseStockEntity> reponse = new PageReponse<PurchaseStockEntity>();
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            reponse.Data = new List<PurchaseStockEntity>();
            var localeId = ctx.UserLocale.LCID;

            string sql = $@"/*dialect*/select m.FPURCHASEORGID OrgId,orgl.FNAME OrgName,
                                        ven_l.FNAME SupplierName,m.FBILLNO PoNo,d.FSEQ PoSeq,m.FDATE PoDate,mat.FNUMBER ItemNo,matL.FNAME ItemDesc, 
                                        d.FPENYMAPCODE CustItemNo,gll.FNAME ParentSmallDesc,gl.FNAME SmallClassDesc,f.FTAXPRICE VatPrice,d.FSupplierUnitPrice SupplierUnitPrice,
                                        d.FQTY PoQty,por.FStockInQty QtyRecd,por.FRemainStockINQty UnQtyRecd,ptloc.FAVBQTY UsableQty,
                                        d.FSupplierReplyDate VdrDnDate,d.FSupplierDescriptions DelayText,case when m.FCloseStatus='A' then '未关闭' else '已关闭' end Status,
                                        dd.FDeliveryDate PoArrivalDate,ord.FBILLNO SoNo,d.FSoSeq SoSeq,
                                        ord.FDATE SoDate,d.FPENYDeliveryDate SoArrivalDate,buyl.FNAME BuyerName,d.FPENYSALERS SalesName
                                         from t_PUR_POOrderEntry d
                                         inner join t_PUR_POOrder m on m.FID=d.FID
                                         inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
										 inner join T_PUR_POORDERENTRY_D dd on dd.FENTRYID=d.FENTRYID
                                         inner join T_ORG_ORGANIZATIONS_L orgl on m.FPURCHASEORGID=orgl.FORGID and orgl.FLOCALEID=2052
                                         left join  t_BD_Supplier ven on ven.FSUPPLIERID=m.FSUPPLIERID 
                                         left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID
                                         left join  T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
                                         inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = 2052
                                         left join T_BD_MATERIALGROUP_L gl on d.FSMALLID = gl.FID and gl.FLOCALEID = 2052
				                         left join T_BD_MATERIALGROUP_L gll on d.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
                                         left join T_PUR_POORDERENTRY_R por on por.FENTRYID=d.FENTRYID
										 left join T_SAL_ORDER ord on ord.FBILLNO=d.FSoNo
										 left join V_BD_BUYER buy on buy.fid=m.FPurchaserId
										 left join V_BD_BUYER_L buyl on buy.fid=buyl.fid
										 left join (select ItemNo,SUM(FAVBQTY) FAVBQTY from (
											select t1.FAVBQTY,m.FNUMBER ItemNo from V_STK_INVENTORY_CUS t1
											inner join T_BD_MATERIAL m on t1.FMATERIALID=m.FMASTERID and m.FUSEORGID=t1.FSTOCKORGID
											where  t1.FAVBQTY>0
											) t1
											group by ItemNo) ptloc on ptloc.ItemNo=mat.FNUMBER
                                         where d.FQTY>0 ";

            string countSql = $@"/*dialect*/select 
                                       Count(1)
                                         from t_PUR_POOrderEntry d
										inner join t_PUR_POOrder m on m.FID=d.FID
                                         inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
										 inner join T_PUR_POORDERENTRY_D dd on dd.FENTRYID=d.FENTRYID
                                         inner join T_ORG_ORGANIZATIONS_L orgl on m.FPURCHASEORGID=orgl.FORGID and orgl.FLOCALEID=2052
                                         left join  t_BD_Supplier ven on ven.FSUPPLIERID=m.FSUPPLIERID 
                                         left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID
                                         left join  T_BD_MATERIAL mat on d.FMATERIALID=mat.FMATERIALID
                                         inner join T_BD_MATERIAL_L matL on mat.FMATERIALID=matL.FMATERIALID and matL.FLOCALEID = 2052
                                         left join T_BD_MATERIALGROUP_L gl on d.FSMALLID = gl.FID and gl.FLOCALEID = 2052
				                         left join T_BD_MATERIALGROUP_L gll on d.FPARENTSMALLID = gll.FID and gll.FLOCALEID = 2052
                                         left join T_PUR_POORDERENTRY_R por on por.FENTRYID=d.FENTRYID
										 left join T_SAL_ORDER ord on ord.FBILLNO=d.FSoNo
										 left join V_BD_BUYER buy on buy.fid=m.FPurchaserId
										 left join V_BD_BUYER_L buyl on buy.fid=buyl.fid 
                                        where  d.FQTY>0  ";

            var whereSql = "";

            if (!string.IsNullOrWhiteSpace(filter.Filter.OrgId))
            {
                whereSql += $" and m.FPURCHASEORGID = '{filter.Filter.OrgId}' ";
            }
            if (filter.Filter.OrderStartDate != null)
            {
                whereSql += $" and m.FDATE >= '{filter.Filter.OrderStartDate}' ";
            }
            if (filter.Filter.OrderEndDate != null)
            {
                whereSql += $" and m.FDATE <= '{filter.Filter.OrderEndDate.Value.ToString("yyyy-MM-dd 23:59:59")}' ";
            }
            if (!string.IsNullOrEmpty(filter.Filter.SupplierName))
            {
                whereSql += $" and ven_l.FNAME like '%{filter.Filter.SupplierName.Trim()}%' ";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.ParentSmallId))
            {
                whereSql += $" and d.FPARENTSMALLID = '{filter.Filter.ParentSmallId}'";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassId))
            {
                whereSql += $" and d.FSMALLID='{filter.Filter.SmallClassId}'";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.SmallClassFilter))
            {
                whereSql += $" and d.FSMALLID in ('{filter.Filter.SmallClassFilter}')";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.ItemNo))
            {
                whereSql += $" and (mat.FNUMBER like '%{filter.Filter.ItemNo.Trim()}%' or d.FPENYMAPCODE like '%{filter.Filter.ItemNo.Trim()}%' ) ";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.ItemDesc))
            {
                whereSql += $" and matL.FNAME like '%{filter.Filter.ItemDesc.Trim()}%'";
            }

            if (!string.IsNullOrWhiteSpace(filter.Filter.PoNo))
            {
                whereSql += $" and m.FBILLNO = '{filter.Filter.PoNo.Trim()}'";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.SoNo))
            {
                whereSql += $" and d.FSoNo = '{filter.Filter.SoNo.Trim()}'";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.BuyerName))
            {
                whereSql += $" and buyl.FNAME = '{filter.Filter.BuyerName.Trim()}'";
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.SalesName))
            {
                whereSql += $" and d.FPENYSALERS like '%{filter.Filter.SalesName.Trim()}%'";
            }
            countSql += whereSql;
            var retCount = DBServiceHelper.ExecuteScalar<int>(ctx, countSql, 0);
            reponse.Count = retCount;
            if (retCount == 0)
            {
                response.Data = reponse;
                response.Code = ResponseCode.Success;
                response.Message = "获取成功";
                return response;
            }
            whereSql += $" order by m.FDATE,d.FID,d.FENTRYID offset (({filter.PageIndex}-1)*{filter.PageSize}) rows fetch next {filter.PageSize} rows only ";
            sql += whereSql;
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            foreach (var reader in datas)
            {
                var purchaseOrder = new PurchaseStockEntity();
                purchaseOrder.OrgId = Convert.ToString(reader["OrgId"]);
                purchaseOrder.OrgName = Convert.ToString(reader["OrgName"]);
                purchaseOrder.SupplierName = Convert.ToString(reader["SupplierName"]);
                purchaseOrder.PoNo = Convert.ToString(reader["PoNo"]);
                purchaseOrder.PoSeq = ConvertInt(reader["PoSeq"]);
                purchaseOrder.PoDate = reader["PoDate"] is DBNull ? "" : Convert.ToDateTime(reader["PoDate"]).ToString("yyyy-MM-dd");
                purchaseOrder.ItemNo = Convert.ToString(reader["ItemNo"]);
                purchaseOrder.ItemDesc = Convert.ToString(reader["ItemDesc"]);
                purchaseOrder.CustItemNo = Convert.ToString(reader["CustItemNo"]);
                purchaseOrder.ParentSmallDesc = Convert.ToString(reader["ParentSmallDesc"]);
                purchaseOrder.SmallClassDesc = Convert.ToString(reader["SmallClassDesc"]);
                purchaseOrder.VatPrice = Convert.ToDecimal(reader["VatPrice"]);
                purchaseOrder.SupplierUnitPrice = Convert.ToDecimal(reader["SupplierUnitPrice"]);
                purchaseOrder.PoQty = Convert.ToDecimal(reader["PoQty"]);
                purchaseOrder.QtyRecd = Convert.ToDecimal(reader["QtyRecd"]);
                purchaseOrder.UnQtyRecd = Convert.ToDecimal(reader["UnQtyRecd"]);
                purchaseOrder.UsableQty = Convert.ToDecimal(reader["UsableQty"]);
                purchaseOrder.VdrDnDate = Convert.ToDateTime(reader["VdrDnDate"]).ToString("yyyy-MM-dd");
                if (purchaseOrder.VdrDnDate.Contains("0001-01-01"))
                {
                    purchaseOrder.VdrDnDate = "";
                }
                purchaseOrder.DelayText = Convert.ToString(reader["DelayText"]);
                purchaseOrder.Status = Convert.ToString(reader["Status"]);
                purchaseOrder.PoArrivalDate = Convert.ToDateTime(reader["PoArrivalDate"]).ToString("yyyy-MM-dd");
                if (purchaseOrder.PoArrivalDate.Contains("0001-01-01"))
                {
                    purchaseOrder.PoArrivalDate = "";
                }
                purchaseOrder.SoNo = Convert.ToString(reader["SoNo"]);
                purchaseOrder.SoSeq = ConvertInt(reader["SoSeq"]);
                purchaseOrder.SoDate = Convert.ToDateTime(reader["SoDate"]).ToString("yyyy-MM-dd");
                if (purchaseOrder.SoDate.Contains("0001-01-01"))
                {
                    purchaseOrder.SoDate = "";
                }
                purchaseOrder.SoArrivalDate = Convert.ToDateTime(reader["SoArrivalDate"]).ToString("yyyy-MM-dd");
                if (purchaseOrder.SoArrivalDate.Contains("0001-01-01"))
                {
                    purchaseOrder.SoArrivalDate = "";
                }
                purchaseOrder.BuyerName = Convert.ToString(reader["BuyerName"]);
                purchaseOrder.SalesName = Convert.ToString(reader["SalesName"]);
                reponse.Data.Add(purchaseOrder);
            }
            response.Data = reponse;
            response.Code = ResponseCode.Success;
            response.Message = "获取成功";
            return response;

        }

        /// <summary>
        /// 可用库存数明细
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetAvailableInventoryDetail(Context ctx, string message)
        {
            AvailableInventoryDetailRequest request = JsonConvertUtils.DeserializeObject<AvailableInventoryDetailRequest>(message);
            List<AvailableInventoryDetailEntity> list = new List<AvailableInventoryDetailEntity>();
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrEmpty(request.ItemNo))
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "获取失败:型号不能为空";
                response.ErrorMessage = "型号不能为空";
                return response;
            }
            var whereSql = $" and m.FNUMBER='{request.ItemNo}' ";
            if (!string.IsNullOrWhiteSpace(request.WarehouseId))
            {
                whereSql += $" and t1.FSTOCKID={request.WarehouseId} ";
            }
            string sql = $@"/*dialect*/select orgl.FNAME OrgName,t1.FSTOCKID StoID,stol.FNAME StoName,t1.FAVBQTY,m.FNUMBER MaterialNum,
                    ml.FNAME MaterialName,ul.FNAME as UnitName from V_STK_INVENTORY_CUS t1
                    inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID=org.FORGID
                    left  join T_ORG_ORGANIZATIONS_L orgl on orgl.FORGID=org.FORGID
                    inner join T_BD_MATERIAL m on t1.FMATERIALID=m.FMASTERID  and m.FUSEORGID=t1.FSTOCKORGID
                    inner join T_BD_MATERIAL_L ml on ml.FMATERIALID=m.FMASTERID
                    inner join t_BD_Stock  sto on sto.FSTOCKID=t1.FSTOCKID
                    inner join T_BD_STOCK_L  stol on sto.FSTOCKID=stol.FSTOCKID
                    inner join T_BD_UNIT u on t1.FBASEUNITID=u.FUNITID
                    inner join T_BD_UNIT_L ul on u.FUNITID=ul.FUNITID
                    where  t1.FAVBQTY>0 {whereSql}
                    order by org.FNUMBER ";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            foreach (var reader in datas)
            {
                var entity = new AvailableInventoryDetailEntity();
                entity.Unit = Convert.ToString(reader["UnitName"]);
                entity.UsableQty = Convert.ToDecimal(reader["FAVBQTY"]);
                entity.ItemNo = Convert.ToString(reader["MaterialNum"]);
                entity.ItemDesc = Convert.ToString(reader["MaterialName"]);
                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["OrgName"])))
                {
                    entity.WarehouseName = Convert.ToString(reader["StoName"]) + "(" + Convert.ToString(reader["OrgName"]) + ")";
                }
                else
                {
                    entity.WarehouseName = Convert.ToString(reader["StoName"]);
                }
                list.Add(entity);
            }
            response.Data = list;
            response.Code = ResponseCode.Success;
            response.Message = "获取成功";
            return response;

        }

        /// <summary>
        /// 物料收发明细
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetStockDetailRpt(Context ctx, string message)
        {

            StockDetailRptRequest request = JsonConvertUtils.DeserializeObject<StockDetailRptRequest>(message);
            int Rn = 0;
            List<StockDetailRptEntity> list = new List<StockDetailRptEntity>();
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrEmpty(request.ItemNo))
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "型号不能为空";
                return response;
            }
            if (request.FilterType > 2)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "筛选类型错误";
                return response;
            }
            if (request.OrderStartDate == null)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "交易开始日期不能为空";
                return response;
            }
            if (request.OrderEndDate == null)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "交易结束日期不能为空";
                return response;
            }
            //获取全部组织
            var orgList = GetOrgMaterialList(ctx, request.ItemNo);
            //单个组织查询
            if (!string.IsNullOrWhiteSpace(request.OrgId))
            {
                orgList = orgList.Where(x => x.OrgId.Equals(request.OrgId)).ToList();
            }
            if (orgList.Count == 0)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "匹配不到组织信息";
                return response;
            }

            ISysReportService sysReporSservice = ServiceFactory.GetSysReportService(ctx);
            var filterMetadata = FormMetaDataCache.GetCachedFilterMetaData(ctx);//加载字段比较条件元数据。
            var reportMetadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "STK_StockDetailRpt");//加载报表
            var reportFilterMetadata = FormMetaDataCache.GetCachedFormMetaData(ctx, "STK_StockDetailFilter");//加载报表过滤条件元数据。
            var reportFilterServiceProvider = reportFilterMetadata.BusinessInfo.GetForm().GetFormServiceProvider();
            var model = new SysReportFilterModel();

            model.SetContext(ctx, reportFilterMetadata.BusinessInfo, reportFilterServiceProvider);
            model.FormId = reportFilterMetadata.BusinessInfo.GetForm().Id;
            model.FilterObject.FilterMetaData = filterMetadata;
            model.InitFieldList(reportMetadata, reportFilterMetadata);
            model.GetSchemeList();
            //查询默认过滤方案id
            string Sql = "SELECT fschemeid FROM T_BAS_FILTERSCHEME where fformid='STK_StockDetailRpt' and fschemename='Default Scheme'";
            string fschemeid = "";
            using (IDataReader reader = DBServiceHelper.ExecuteReader(ctx, Sql))
            {
                if (reader.Read())
                {
                    fschemeid = reader["fschemeid"].ToString();
                }
            }
            if (string.IsNullOrEmpty(fschemeid))
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.Message = "过滤方案为空，请配置物料收发存明细表的过滤方案！";
                return response;
            }
            //过滤方案的主键值，可通过该SQL语句查询得到：SELECT * FROM T_BAS_FILTERSCHEME

            //var entity = model.Load("62a859a944add8");

            var entity = model.Load(fschemeid);

            foreach (var item in orgList)
            {
                //赋值过滤方案条件
                var filterParam = model.GetFilterParameter();

                //开始物料
                FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "STK_StockDetailFilter") as FormMetadata;
                BusinessInfo businessInfo = formMetadata.BusinessInfo;
                BaseDataField bdField = businessInfo.GetField("FBeginMaterialId") as BaseDataField;
                QueryBuilderParemeter qbp = new QueryBuilderParemeter();
                qbp.FormId = "BD_MATERIAL";
                qbp.SelectItems = SelectorItemInfo.CreateItems("FMaterialId");
                qbp.FilterClauseWihtKey = $"FMATERIALID={item.MaterialId} ";
                var obj_ck = BusinessDataServiceHelper.Load(ctx, bdField.RefFormDynamicObjectType, qbp)[0];

                BaseDataField bdField2 = businessInfo.GetField("FEndMaterialId") as BaseDataField;
                QueryBuilderParemeter qbp2 = new QueryBuilderParemeter();
                qbp2.FormId = "BD_MATERIAL";
                qbp2.SelectItems = SelectorItemInfo.CreateItems("FMaterialId");
                qbp2.FilterClauseWihtKey = $"FMATERIALID={item.MaterialId} ";
                var obj_ck2 = BusinessDataServiceHelper.Load(ctx, bdField2.RefFormDynamicObjectType, qbp2)[0];

                filterParam.CustomFilter["StockOrgId"] = item.OrgId;
                filterParam.CustomFilter["BeginDate"] = request.OrderStartDate;
                filterParam.CustomFilter["EndDate"] = request.OrderEndDate;
                //filterParam.CustomFilter["ShowForbidMaterial"] = true;
                filterParam.CustomFilter["BeginMaterialId_Id"] = item.MaterialId;
                filterParam.CustomFilter["BeginMaterialId"] = obj_ck;
                filterParam.CustomFilter["EndMaterialId_Id"] = item.MaterialId;
                filterParam.CustomFilter["EndMaterialId"] = obj_ck2;
                IRptParams p = new RptParams();
                p.FormId = reportFilterMetadata.BusinessInfo.GetForm().Id;
                //StartRow和EndRow是报表数据分页的起始行数和截至行数，一般取所有数据，所以EndRow取int最大值。
                p.StartRow = 1;
                p.EndRow = int.MaxValue;
                p.FilterParameter = filterParam;
                p.FilterFieldInfo = model.FilterFieldInfo;
                p.CustomParams.Add("OpenParameter", "");//此参数不能缺少，即使为空也要保留

                MoveReportServiceParameter param = new MoveReportServiceParameter(ctx, reportMetadata.BusinessInfo, Guid.NewGuid().ToString(), p);
                var rpt = sysReporSservice.GetListAndReportData(param);//简单账表使用GetReportData方法
                if (rpt.DataSource != null)
                {
                    for (int i = 0; i < rpt.DataSource.Rows.Count; i++)
                    {
                        StockDetailRptEntity entityRp = new StockDetailRptEntity();
                        entityRp.OrgName = Convert.ToString(rpt.DataSource.Rows[i]["FOWNERNAME"]);
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(rpt.DataSource.Rows[i]["FBILLNO"])))
                        {
                            entityRp.DocNo = Convert.ToString(rpt.DataSource.Rows[i]["FBILLNO"]) + "-" + Convert.ToString(rpt.DataSource.Rows[i]["FBILLSEQID"]);
                        }
                        else
                        {
                            entityRp.DocNo = "";
                        }

                        if (!string.IsNullOrWhiteSpace(Convert.ToString(rpt.DataSource.Rows[i]["FAPPROVEDATE"])))
                        {
                            entityRp.TranDate = Convert.ToDateTime(rpt.DataSource.Rows[i]["FAPPROVEDATE"].ToString()).ToString("yyyy-MM-dd");
                        }
                        else if (!string.IsNullOrWhiteSpace(Convert.ToString(rpt.DataSource.Rows[i]["FDATE"])))
                        {
                            entityRp.TranDate = Convert.ToDateTime(rpt.DataSource.Rows[i]["FDATE"].ToString()).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            entityRp.TranDate = "";
                        }

                        entityRp.TranType = Convert.ToString(rpt.DataSource.Rows[i]["FBILLNAME"]);
                        entityRp.ItemNo = Convert.ToString(rpt.DataSource.Rows[i]["FMATERIALNUMBER"]);
                        entityRp.ItemDesc = Convert.ToString(rpt.DataSource.Rows[i]["FMATERIALNAME"]);
                        //结余
                        entityRp.LQoh = Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKJCQTY"]);
                        if (Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKQCQTY"]) > 0)
                        {
                            //期初(入库)
                            entityRp.Qty = Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKQCQTY"]);
                            entityRp.InOutStatus = "In";
                            //仓存数量
                            entityRp.PQoh = (entityRp.LQoh - entityRp.Qty);
                        }
                        else if (Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKQCQTY"]) < 0)
                        {
                            //期初(出库)
                            entityRp.Qty = Math.Abs(Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKQCQTY"]));
                            entityRp.InOutStatus = "Out";
                            //仓存数量
                            entityRp.PQoh = (entityRp.LQoh + entityRp.Qty);
                        }
                        else if (Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKINQTY"]) > 0)
                        {
                            //收入库存(入库)
                            entityRp.Qty = Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKINQTY"]);
                            entityRp.InOutStatus = "In";
                            entityRp.PQoh = (entityRp.LQoh - entityRp.Qty);
                        }
                        else if (Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKINQTY"]) < 0)
                        {
                            //收入库存(出库)
                            entityRp.Qty = Math.Abs(Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKINQTY"]));
                            entityRp.InOutStatus = "Out";
                            entityRp.PQoh = (entityRp.LQoh + entityRp.Qty);
                        }
                        else if (Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKOUTQTY"]) > 0)
                        {
                            //发出&数量(出库)
                            entityRp.Qty = Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKOUTQTY"]);
                            entityRp.InOutStatus = "Out";
                            entityRp.PQoh = (entityRp.LQoh + entityRp.Qty);
                        }
                        else if (Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKOUTQTY"]) < 0)
                        {
                            //发出&数量(入库)
                            entityRp.Qty = Math.Abs(Convert.ToDecimal(rpt.DataSource.Rows[i]["FSTOCKOUTQTY"]));
                            entityRp.InOutStatus = "In";
                            entityRp.PQoh = (entityRp.LQoh - entityRp.Qty);
                        }
                        entityRp.StockName = Convert.ToString(rpt.DataSource.Rows[i]["FSTOCKNAME"]);
                        entityRp.Unit = Convert.ToString(rpt.DataSource.Rows[i]["FSTOCKUNITNAME"]);
                        entityRp.RefNo = Convert.ToString(rpt.DataSource.Rows[i]["FSRCBILLNO"]);
                        Rn += 1;
                        entityRp.Rn = Rn;
                        list.Add(entityRp);
                    }
                }
            }
            string listHead = JsonConvert.SerializeObject(list);
            //只看出库
            if (request.FilterType == 1)
            {
                list = list.Where(x => x.InOutStatus == "Out").OrderByDescending(x => x.Rn).ToList();
            }
            else if (request.FilterType == 2)
            {
                //只看入库
                list = list.Where(x => x.InOutStatus == "In").OrderByDescending(x => x.Rn).ToList();
            }
            else
            {
                //全部（出加入）
                list = list.Where(x => x.InOutStatus != "").OrderByDescending(x => x.Rn).ToList();
            }
            response.Data = list;
            response.Code = ResponseCode.Success;
            response.Message = "获取成功！";
            return response;
        }

        /// <summary>
        /// 获取组织ID和物料ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public List<OrgIdAndMaterialIdEntity> GetOrgMaterialList(Context ctx, string ItemNo)
        {
            List<OrgIdAndMaterialIdEntity> entityList = new List<OrgIdAndMaterialIdEntity>();
            var sql = $@"/*dialect*/select org.FORGID,mat.FMATERIALID from T_ORG_ORGANIZATIONS org 
                inner join T_BD_MATERIAL mat on mat.FUSEORGID=org.FORGID
                where org.FDOCUMENTSTATUS='C' and org.FFORBIDSTATUS='A' and org.FORGID not in(1,3886615,4093663,1043841) and mat.FNUMBER='{ItemNo}'
                order by FORGID asc";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            foreach (var data in datas)
            {
                entityList.Add(new OrgIdAndMaterialIdEntity
                {
                    OrgId = Convert.ToString(data["FORGID"]),
                    MaterialId = Convert.ToString(data["FMATERIALID"]),
                });
            }
            return entityList;
        }

        public ResponseMessage<dynamic> PurchaseOrderSyncStatus(Context ctx, SaleOrderRequest request)
        {
            var sql = "update T_PUR_POOrder set FElectricSyncStatus='sync' where FBillNo = @FBillNo";

            DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FBillNo", KDDbType.String, request.SalesOrderNo));
            return new ResponseMessage<dynamic>()
            {
                Code = ResponseCode.Success
            };
        }

        /// <summary>
        /// 费用采购订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> CostPurchaseOrder(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            CostPurchaseOrderRequest request = JsonConvertUtils.DeserializeObject<CostPurchaseOrderRequest>(message);
            if (string.IsNullOrWhiteSpace(request.BillNo))
            {
                response.Message = "单据编号不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return PurchaseOrderServiceHelper.CostPurchaseOrderService(ctx, request);
        }

        /// <summary>
        /// MES收料下推采购入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesReceiveGenerateInStock(Context ctx, string message)
        {

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            MesReceiveInStockRequest request = JsonConvertUtils.DeserializeObject<MesReceiveInStockRequest>(message);
            if (string.IsNullOrWhiteSpace(request.InStockBillNo))
            {
                response.Message = "入库单据编号不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return PurchaseOrderServiceHelper.MesReceiveGenerateInStockService(ctx, request);
        }

        /// <summary>
        /// MES退料申请下推采购退料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesMrAppGenerateMrb(Context ctx, string message)
        {

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            MesMrAppToMrbRequest request = JsonConvertUtils.DeserializeObject<MesMrAppToMrbRequest>(message);
            if (string.IsNullOrWhiteSpace(request.MrbBillNo))
            {
                response.Message = "采购退料单编号不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return PurchaseOrderServiceHelper.MesMrAppGenerateMrbService(ctx, request);
        }

        /// <summary>
        /// 收料免检下推采购入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> ReceiveExemptionInStock(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            List<ReceiveExemptionInStockEntity> list = JsonConvertUtils.DeserializeObject<List<ReceiveExemptionInStockEntity>>(message);
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            foreach (var item in list)
            {
                selectedRows.Add(new ListSelectedRow(item.PrimaryKeyValue, item.EntryPrimaryKeyValue, 0, item.FormID) { EntryEntityKey = "FDetailEntity" });
            }
            try
            {
                var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_ReceiveBill", "STK_InStock");
                var rule = rules.FirstOrDefault(t => t.IsDefault);
                if (rule == null)
                {
                    throw new Exception("没有从收料通知单到销售入库单的转换关系");
                }
                //有数据才需要下推
                if (selectedRows.Count > 0)
                {
                    PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                    {
                        TargetBillTypeId = "a1ff32276cd9469dad3bf2494366fa4f",     // 请设定目标单据单据类型
                        TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                    //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                    };
                    //执行下推操作，并获取下推结果
                    var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                    if (operationResult.IsSuccess)
                    {
                        var view = FormMetadataUtils.CreateBillView(ctx, "STK_InStock");
                        foreach (var item in operationResult.TargetDataEntities)
                        {
                            view.Model.DataObject = item.DataEntity;
                            //保存批核
                            var opers = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
                            if (opers.IsSuccess)
                            {
                                //清除释放网控
                                view.CommitNetworkCtrl();
                                view.InvokeFormOperation(FormOperationEnum.Close);
                                view.Close();
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
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = "物料免检的下推采购入库失败：" + ex.Message;
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            response.Message = "操作成功";
            response.Code = ResponseCode.Success;
            return response;
        }
    }
}
