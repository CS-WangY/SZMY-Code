using mymooo.core;
using mymooo.core.Attributes;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.ReportModel;
using DateTime = System.DateTime;

namespace mymooo.k3cloud.business.Services.Report
{
    [AutoInject(InJectType.Scope)]
    public class ReportNotCheckAccountService(KingdeeContent kingdeeContent)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        /// <summary>
        /// 获取未对账合计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<NotCheckAccountTotals> GetNotCheckAccountTotals(CrmReportRequestModel request)
        {
            ResponseMessage<NotCheckAccountTotals> response = new();
            try
            {
                NotCheckAccountTotals datas = new();
                DateTime now = DateTime.Now;
                // 当月
                DateTime DateAS = new(now.Year, now.Month, 1);
                DateTime DateAE = new(now.AddMonths(1).Year, now.AddMonths(1).Month, 1);
                //string sDateAText = "当月";

                // 前2月
                DateTime DateBS = new(now.AddMonths(-2).Year, now.AddMonths(-2).Month, 1);
                DateTime DateBE = DateAS;
                string sDateBText = $"{DateBS.Month}-{DateBE.AddDays(-1).Month}月";

                // 前5月 - 前2月
                DateTime DateCS = new(now.AddMonths(-5).Year, now.AddMonths(-5).Month, 1);
                DateTime DateCE = DateBS;
                //string sDateCText = $"{DateCS.Month}-{DateCE.AddDays(-1).Month}月";

                // 前2年-前5月
                DateTime DateDS = new(now.AddMonths(-24).Year, now.AddMonths(-24).Month, 1);
                DateTime DateDE = DateCS;
                //string sDateDText = $"{DateDE.Month}月前";
                string wechatFilter = "";
                if (request.IsAll == 0)
                {
                    string wechatCodes = string.Join(",", request.AuthWechatCodes.Select(r => "'" + r + "'"));
                    if (!string.IsNullOrEmpty(wechatCodes))
                    {
                        wechatFilter = $@"AND EXISTS (SELECT sc.FCUSTOMERID from T_HR_EMPINFO ep
INNER JOIN T_BD_STAFFTEMP st ON ep.FID=st.FID
INNER JOIN T_BD_OPERATORENTRY op ON st.FSTAFFID=op.FSTAFFID
INNER JOIN T_BD_OPERATORDETAILS opd ON op.FENTRYID=opd.FENTRYID
INNER JOIN T_SAL_SCSALERCUST sc ON opd.FOPERATORGROUPID=sc.FSALERGROUPID
LEFT JOIN T_BD_CUSTOMER bdc ON sc.FCUSTOMERID=bdc.FCUSTID
WHERE t4.FCUSTOMERID=sc.FCUSTOMERID AND ep.FWECHATCODE IN ({wechatCodes})
AND bdc.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')
)";
                    }
                }
                else
                {
                    wechatFilter = $@"AND t5.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')";
                }

                string sSql = $@"SELECT SUM(t3.FALLAMOUNT-t2.FARJOINAMOUNT) AS FAMOUNT  FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateAS}' AND t4.FDATE<'{DateAE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountA = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT SUM(t3.FALLAMOUNT-t2.FARJOINAMOUNT) AS FAMOUNT  FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateBS}' AND t4.FDATE<'{DateBE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountB = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT SUM(t3.FALLAMOUNT-t2.FARJOINAMOUNT) AS FAMOUNT  FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateCS}' AND t4.FDATE<'{DateCE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountC = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT SUM(t3.FALLAMOUNT-t2.FARJOINAMOUNT) AS FAMOUNT  FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateDS}' AND t4.FDATE<'{DateDE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountD = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM (
SELECT ISNULL(t4.FCUSTOMERID,0) AS FCUSTOMERID FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateAS}' AND t4.FDATE<'{DateAE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter + ") _t1";
                datas.CusCountA = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM (
SELECT ISNULL(t4.FCUSTOMERID,0) AS FCUSTOMERID FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateBS}' AND t4.FDATE<'{DateBE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter + ") _t1";
                datas.CusCountB = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM (
SELECT ISNULL(t4.FCUSTOMERID,0) AS FCUSTOMERID FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateCS}' AND t4.FDATE<'{DateCE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter + ") _t1";
                datas.CusCountC = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM (
SELECT ISNULL(t4.FCUSTOMERID,0) AS FCUSTOMERID FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t5 ON t4.FCUSTOMERID=t5.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{DateDS}' AND t4.FDATE<'{DateDE}'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter + ") _t1";
                datas.CusCountD = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);

                response.Data = datas;
            }
            catch (Exception err)
            {
                response.Code = ResponseCode.Exception;
                response.ErrorMessages = [err.Message];
            }
            return response;
        }
        /// <summary>
        /// 获取未对账明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<NotCheckAccountList>> GetNotCheckAccountList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<NotCheckAccountList>> response = new();
            PageResponse<NotCheckAccountList> page = new();
            string wechatFilter = "";
            if (request.Filter?.IsAll == 0)
            {
                if (request.Filter != null)
                {
                    string wechatCodes = string.Join(",", request.Filter.AuthWechatCodes.Select(r => "'" + r + "'"));
                    if (!string.IsNullOrEmpty(wechatCodes))
                    {
                        wechatFilter = $@"AND EXISTS (SELECT sc.FCUSTOMERID from T_HR_EMPINFO ep
INNER JOIN T_BD_STAFFTEMP st ON ep.FID=st.FID
INNER JOIN T_BD_OPERATORENTRY op ON st.FSTAFFID=op.FSTAFFID
INNER JOIN T_BD_OPERATORDETAILS opd ON op.FENTRYID=opd.FENTRYID
INNER JOIN T_SAL_SCSALERCUST sc ON opd.FOPERATORGROUPID=sc.FSALERGROUPID
LEFT JOIN T_BD_CUSTOMER bdc ON sc.FCUSTOMERID=bdc.FCUSTID
WHERE t4.FCUSTOMERID=sc.FCUSTOMERID AND ep.FWECHATCODE IN ({wechatCodes})
AND bdc.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')
)";
                    }
                }
            }
            else
            {
                wechatFilter = $@"AND t6.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')";
            }

            int total = 0;
            string sSql = $@"SELECT t6.FNUMBER CustNumber,t7.FNAME CustName,t5l.FNAME SalerName,COUNT(t1.FENTRYID) AS ItemCount
,SUM(t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY) AS NotArJoinQty
,SUM(t3.FALLAMOUNT-t2.FARJOINAMOUNT) AS NotArJoinAmount
FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
INNER JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
INNER JOIN V_BD_SALESMAN t5 ON t4.FSALESMANID=t5.fid
INNER JOIN V_BD_SALESMAN_L t5l ON t5.fid=t5l.fid
INNER JOIN T_BD_CUSTOMER t6 ON t4.FCUSTOMERID=t6.FCUSTID
INNER JOIN T_BD_CUSTOMER_L t7 ON t6.FCUSTID=t7.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{request.Filter?.DateS}' AND t4.FDATE<'{request.Filter?.DateE}'
AND t6.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
{wechatFilter}
GROUP BY t6.FNUMBER,t7.FNAME,t5l.FNAME";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<NotCheckAccountList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);

            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
        /// <summary>
        /// 获取客户未对账明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<CustNotCheckAccountList>> GetCustNotCheckAccountList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<CustNotCheckAccountList>> response = new();
            PageResponse<CustNotCheckAccountList> page = new();
            int total = 0;
            string sSql = $@"SELECT t2.FSRCBILLNO as SrcBillNo,t2.FSOENTRYID as SOEntryid,
t4.FAPPROVEDATE as Approvedate,t2.FSOORDERNO as SOOrderNo,ml.FNUMBER as ItemNumber,
t3.FALLAMOUNT-t2.FARJOINAMOUNT AS NotArJoinAmount
,t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY AS NotArJoinQty
FROM T_SAL_OUTSTOCKENTRY t1
INNER JOIN T_SAL_OUTSTOCKENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_SAL_OUTSTOCKENTRY_F t3 ON t1.FENTRYID=t3.FENTRYID
INNER JOIN T_SAL_OUTSTOCK t4 ON t2.FID=t4.FID
INNER JOIN T_BD_MATERIAL ml ON t1.FMATERIALID=ml.FMATERIALID
INNER JOIN T_BD_CUSTOMER t6 ON t4.FCUSTOMERID=t6.FCUSTID
WHERE t1.FREALQTY-t2.FARJOINQTY-t2.FSUMRETSTOCKQTY>0 AND t4.FDATE >= '{request.Filter?.DateS}' AND t4.FDATE<'{request.Filter?.DateE}'
AND t6.FNUMBER='{request.Filter?.CompanyCode}'";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<CustNotCheckAccountList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
    }
}
