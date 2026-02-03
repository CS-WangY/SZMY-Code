using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.core.Attributes;
using mymooo.core;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.ReportModel;
using SqlSugar;

namespace mymooo.k3cloud.business.Services.Report
{
    [AutoInject(InJectType.Scope)]
    public class ReportNotReceiveService(KingdeeContent kingdeeContent)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        /// <summary>
        /// 获取未收款核销合计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<NotReceiveTotals> GetNotReceiveTotals(CrmReportRequestModel request)
        {
            ResponseMessage<NotReceiveTotals> response = new();
            try
            {
                NotReceiveTotals datas = new();
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

                string sSql = $@"SELECT SUM(FNORECEIVEAMOUNT) AS FAMOUNT
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateAS}' AND t2.FDATE<'{DateAE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountA = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT SUM(FNORECEIVEAMOUNT) AS FAMOUNT
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateBS}' AND t2.FDATE<'{DateBE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountB = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT SUM(FNORECEIVEAMOUNT) AS FAMOUNT
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateCS}' AND t2.FDATE<'{DateCE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountC = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT SUM(FNORECEIVEAMOUNT) AS FAMOUNT
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateDS}' AND t2.FDATE<'{DateDE}'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.AmountD = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT COUNT(DISTINCT t2.FCUSTOMERID) AS CusCount
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateAS}' AND t2.FDATE<'{DateAE}'
AND t1.FNORECEIVEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.CusCountA = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT COUNT(DISTINCT t2.FCUSTOMERID) AS CusCount
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateBS}' AND t2.FDATE<'{DateBE}'
AND t1.FNORECEIVEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.CusCountB = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT COUNT(DISTINCT t2.FCUSTOMERID) AS CusCount
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateCS}' AND t2.FDATE<'{DateCE}'
AND t1.FNORECEIVEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
                datas.CusCountC = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT COUNT(DISTINCT t2.FCUSTOMERID) AS CusCount
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t3 ON t2.FCUSTOMERID=t3.FCUSTID
WHERE t2.FDATE >= '{DateDS}' AND t2.FDATE<'{DateDE}'
AND t1.FNORECEIVEAMOUNT<>0
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'" + wechatFilter;
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
        /// 获取未收款核销明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<NotReceiveList>> GetNotReceiveList(PageRequest<CrmReportRequestModel> request)
        {

            ResponseMessage<PageResponse<NotReceiveList>> response = new();
            PageResponse<NotReceiveList> page = new();
            string wechatFilter = "";
            if (request.Filter?.IsAll == 0)
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
            else
            {
                wechatFilter = $@"AND t6.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')";
            }

            int total = 0;
            string sSql = $@"SELECT t6.FNUMBER CustNumber,t7.FNAME CustName,t5l.FNAME SalerName,COUNT(t1.FENTRYID) AS ItemCount
,SUM(t1.FALLAMOUNTFOR-t1.FRECEIVEAMOUNT) AS NotReceiveAmount
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN dbo.T_SAL_ORDERENTRY t3 ON t1.FORDERENTRYID=t3.FENTRYID
LEFT JOIN T_SAL_ORDER t4 ON t3.FID=t4.FID
LEFT JOIN T_BD_CUSTOMER t6 ON t4.FCUSTID=t6.FCUSTID
LEFT JOIN T_BD_CUSTOMER_L t7 ON t6.FCUSTID=t7.FCUSTID
LEFT JOIN V_BD_SALESMAN_L t5l ON t4.FSALERID=t5l.fid
WHERE t2.FDOCUMENTSTATUS='C' AND t1.FORDERENTRYID>0
AND t2.FDATE >= '{request.Filter?.DateS}' AND t2.FDATE<'{request.Filter?.DateE}'
AND t6.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
{wechatFilter}
GROUP BY t6.FNUMBER,t7.FNAME,t5l.FNAME
HAVING SUM(t1.FALLAMOUNTFOR-t1.FRECEIVEAMOUNT) > 0";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<NotReceiveList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
        /// <summary>
        /// 获取客户未收款核销明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<CustNotReceiveList>> GetCustNotReceiveList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<CustNotReceiveList>> response = new();
            PageResponse<CustNotReceiveList> page = new();
            int total = 0;
            string sSql = $@"SELECT t2.FBILLNO AS BillNo,t2.FDATE AS FDate,COUNT(t1.FENTRYID) AS ItemCount,
SUM(t1.FALLAMOUNTFOR-t1.FRECEIVEAMOUNT) AS NotReceiveAmount
FROM t_AR_receivableEntry t1
LEFT JOIN t_AR_receivable t2 ON t1.FID=t2.FID
LEFT JOIN T_BD_CUSTOMER t6 ON t2.FCUSTOMERID=t6.FCUSTID
WHERE t2.FDOCUMENTSTATUS='C' AND t1.FORDERENTRYID>0
AND t2.FDATE >= '{request.Filter?.DateS}' AND t2.FDATE<'{request.Filter?.DateE}'
AND t6.FNUMBER='{request.Filter?.CompanyCode}'
GROUP BY t2.FBILLNO,t2.FDATE
HAVING SUM(t1.FALLAMOUNTFOR-t1.FRECEIVEAMOUNT) > 0
";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<CustNotReceiveList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
    }
}
