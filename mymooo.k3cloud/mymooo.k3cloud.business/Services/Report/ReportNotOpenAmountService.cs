using mymooo.core;
using mymooo.core.Attributes;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.ReportModel;
using DateTime = System.DateTime;

namespace mymooo.k3cloud.business.Services.Report
{
    [AutoInject(InJectType.Scope)]
    public class ReportNotOpenAmountService(KingdeeContent kingdeeContent)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        /// <summary>
        /// 获取开票合计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<NotOpenAmountTotals> GetNotOpenAccountTotals(CrmReportRequestModel request)
        {
            ResponseMessage<NotOpenAmountTotals> response = new();
            try
            {
                NotOpenAmountTotals datas = new();
                DateTime now = DateTime.Now;
                // 当月
                DateTime DateAS = new DateTime(now.Year, now.Month, 1);
                DateTime DateAE = new DateTime(now.AddMonths(1).Year, now.AddMonths(1).Month, 1);
                //string sDateAText = "当月";

                // 前2月
                DateTime DateBS = new DateTime(now.AddMonths(-2).Year, now.AddMonths(-2).Month, 1);
                DateTime DateBE = DateAS;
                string sDateBText = $"{DateBS.Month}-{DateBE.AddDays(-1).Month}月";

                // 前5月 - 前2月
                DateTime DateCS = new DateTime(now.AddMonths(-5).Year, now.AddMonths(-5).Month, 1);
                DateTime DateCE = DateBS;
                //string sDateCText = $"{DateCS.Month}-{DateCE.AddDays(-1).Month}月";

                // 前2年-前5月
                DateTime DateDS = new DateTime(now.AddMonths(-24).Year, now.AddMonths(-24).Month, 1);
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
WHERE t2.FCUSTOMERID=sc.FCUSTOMERID AND ep.FWECHATCODE IN ({wechatCodes})
AND bdc.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')
)";
                    }
                }
                else
                {
                    wechatFilter = $@"AND t3.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')";
                }

                string sSql = $@"SELECT SUM(FNOINVOICEAMOUNT) AS FAMOUNT FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateAS}' AND t2.FDATE<'{DateAE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountA = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT SUM(FNOINVOICEAMOUNT) AS FAMOUNT FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateBS}' AND t2.FDATE<'{DateBE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountB = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT SUM(FNOINVOICEAMOUNT) AS FAMOUNT FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateCS}' AND t2.FDATE<'{DateCE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountC = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT SUM(FNOINVOICEAMOUNT) AS FAMOUNT FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateDS}' AND t2.FDATE<'{DateDE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountD = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM 
(
SELECT SUM(t1.FNOINVOICEAMOUNT) AS FNOINVOICEAMOUNT,t2.FCUSTOMERID FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateAS}' AND t2.FDATE<'{DateAE}'
AND t1.FNOINVOICEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter +
"GROUP BY t2.FCUSTOMERID) t1 WHERE t1.FNOINVOICEAMOUNT>0";
                datas.CusCountA = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM 
(
SELECT SUM(t1.FNOINVOICEAMOUNT) AS FNOINVOICEAMOUNT,t2.FCUSTOMERID FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateBS}' AND t2.FDATE<'{DateBE}'
AND t1.FNOINVOICEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter +
"GROUP BY t2.FCUSTOMERID) t1 WHERE t1.FNOINVOICEAMOUNT>0";
                datas.CusCountB = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM 
(
SELECT SUM(t1.FNOINVOICEAMOUNT) AS FNOINVOICEAMOUNT,t2.FCUSTOMERID FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateCS}' AND t2.FDATE<'{DateCE}'
AND t1.FNOINVOICEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter +
"GROUP BY t2.FCUSTOMERID) t1 WHERE t1.FNOINVOICEAMOUNT>0";
                datas.CusCountC = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT FCUSTOMERID),0) AS CusCount FROM 
(
SELECT SUM(t1.FNOINVOICEAMOUNT) AS FNOINVOICEAMOUNT,t2.FCUSTOMERID FROM dbo.T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.fid
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateDS}' AND t2.FDATE<'{DateDE}'
AND t1.FNOINVOICEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter +
"GROUP BY t2.FCUSTOMERID) t1 WHERE t1.FNOINVOICEAMOUNT>0";
                datas.CusCountD = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);

                response.Data = datas;
            }
            catch (Exception err)
            {
                response.Code = ResponseCode.Exception;
                response.ErrorMessages = new List<string>() { err.Message };
            }
            return response;
        }
        /// <summary>
        /// 获取未开票明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<NotOpenAmountList>> GetNotOpenAccountList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<NotOpenAmountList>> response = new();
            PageResponse<NotOpenAmountList> page = new();
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
WHERE t2.FCUSTOMERID=sc.FCUSTOMERID AND ep.FWECHATCODE IN ({wechatCodes})
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
            string sSql = $@"SELECT * FROM (
SELECT t6.FNUMBER CustNumber,t7.FNAME CustName,t5l.FNAME SalerName,COUNT(t1.FENTRYID) AS ItemCount
,SUM(t1.FNOINVOICEQTY) AS NotOpenQty
,SUM(t1.FNOINVOICEAMOUNT) AS NotOpenAmount
FROM T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t1.FID=t2.FID
INNER JOIN V_BD_SALESMAN t3 ON t2.FSALEERID=t3.fid
INNER JOIN V_BD_SALESMAN_L t5l ON t3.fid=t5l.fid
INNER JOIN T_BD_CUSTOMER t6 ON t2.FCUSTOMERID=t6.FCUSTID
INNER JOIN T_BD_CUSTOMER_L t7 ON t6.FCUSTID=t7.FCUSTID
WHERE t2.FDATE >= '{request.Filter?.DateS}' AND t2.FDATE<'{request.Filter?.DateE}'
AND t6.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
{wechatFilter}
GROUP BY t6.FNUMBER,t7.FNAME,t5l.FNAME
) t1
WHERE t1.NotOpenAmount>0";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<NotOpenAmountList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
        /// <summary>
        /// 获取客户未开票明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<CustNotOpenAmountList>> GetCustNotOpenAccountList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<CustNotOpenAmountList>> response = new();
            PageResponse<CustNotOpenAmountList> page = new();
            int total = 0;
            string sSql = $@"SELECT t2.FBILLNO AS BillNo,t1.FENTRYID,t2.FDATE AS FDate,t1.FORDERNUMBER,ml.FNUMBER AS ItemNumber,
t1.FNOINVOICEAMOUNT AS NotOpenAmount
,t1.FNOINVOICEQTY AS NotOpenQty
FROM T_AR_RECEIVABLEENTRY t1
INNER JOIN T_AR_RECEIVABLE t2 ON t2.FID=t1.FID
INNER JOIN T_BD_MATERIAL ml ON t1.FMATERIALID=ml.FMATERIALID
INNER JOIN T_BD_CUSTOMER t6 ON t2.FCUSTOMERID=t6.FCUSTID
WHERE t2.FDATE >= '{request.Filter?.DateS}' AND t2.FDATE<'{request.Filter?.DateE}'
AND t1.FNOINVOICEAMOUNT<>0
AND t6.FNUMBER='{request.Filter?.CompanyCode}'";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<CustNotOpenAmountList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
    }
}
