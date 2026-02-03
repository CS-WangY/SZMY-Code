using mymooo.core;
using mymooo.core.Attributes;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.ReportModel;
using DateTime = System.DateTime;

namespace mymooo.k3cloud.business.Services.Report
{
    [AutoInject(InJectType.Scope)]
    public class ReportNotDeliverService(KingdeeContent kingdeeContent)
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;
        /// <summary>
        /// 获取未出货汇总
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<NotDeliverTotals> GetNotDeliverTotals(CrmReportRequestModel request)
        {
            ResponseMessage<NotDeliverTotals> response = new();
            try
            {
                NotDeliverTotals datas = new NotDeliverTotals();
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
WHERE t2.FCUSTID=sc.FCUSTOMERID AND ep.FWECHATCODE IN ({wechatCodes})
AND bdc.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')
)";
                    }
                }
                else
                {
                    wechatFilter = $@"AND t3.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')";
                }

                string sSql = $@"SELECT ISNULL(SUM(t1.FCANOUTQTY*tf.FTAXPRICE),0) AS FAMOUNT FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateAS}' AND t2.FDATE<'{DateAE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.AmountA = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT ISNULL(SUM(t1.FCANOUTQTY*tf.FTAXPRICE),0) AS FAMOUNT FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateBS}' AND t2.FDATE<'{DateBE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.AmountB = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT ISNULL(SUM(t1.FCANOUTQTY*tf.FTAXPRICE),0) AS FAMOUNT FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateCS}' AND t2.FDATE<'{DateCE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.AmountC = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);
                sSql = $@"SELECT ISNULL(SUM(t1.FCANOUTQTY*tf.FTAXPRICE),0) AS FAMOUNT FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateDS}' AND t2.FDATE<'{DateDE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.AmountD = _kingdeeContent.SqlSugar.Ado.GetDecimal(sSql);

                sSql = $@"SELECT ISNULL(COUNT(DISTINCT t2.FCUSTID),0) AS CusCount FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateAS}' AND t2.FDATE<'{DateAE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.CusCountA = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT t2.FCUSTID),0) AS CusCount FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateBS}' AND t2.FDATE<'{DateBE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.CusCountB = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT t2.FCUSTID),0) AS CusCount FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateCS}' AND t2.FDATE<'{DateCE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
                datas.CusCountC = _kingdeeContent.SqlSugar.Ado.GetInt(sSql);
                sSql = $@"SELECT ISNULL(COUNT(DISTINCT t2.FCUSTID),0) AS CusCount FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER t3 ON t2.FCUSTID=t3.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{DateDS}' AND t2.FDATE<'{DateDE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t3.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
" + wechatFilter;
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
        /// 获取未出货记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<NotDeliverList>> GetNotDeliverList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<NotDeliverList>> response = new();
            PageResponse<NotDeliverList> page = new();

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
WHERE t2.FCUSTID=sc.FCUSTOMERID AND ep.FWECHATCODE IN ({wechatCodes})
AND bdc.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')
)";
                    }
                }
            }
            else
            {
                wechatFilter = $@"AND t5.FNUMBER NOT IN ('E1968','E830','E499','E4391','E414','E1967','E1187','E497','E830','E378','E5783','E829','E1050','E1052','E5577','E1586','E4391')";
            }

            int total = 0;
            string sSql = $@"SELECT t5.FNUMBER CustNumber,t4.FNAME AS CustName,t3L.FNAME AS SalerName,COUNT(t1.FENTRYID) AS ItemCount,SUM(t1.FCANOUTQTY) AS CanOutQty,SUM(t1.FCANOUTQTY*tf.FTAXPRICE) AS CanOutAmount
FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY te ON t1.FENTRYID=te.FENTRYID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN V_BD_SALESMAN t3 ON t2.FSALERID=t3.fid
INNER JOIN V_BD_SALESMAN_L t3L ON t3.fid=t3L.fid
INNER JOIN dbo.T_BD_CUSTOMER_L t4 ON t2.FCUSTID=t4.FCUSTID
INNER JOIN dbo.T_BD_CUSTOMER t5 ON t2.FCUSTID=t5.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{request.Filter?.DateS}' AND t2.FDATE<'{request.Filter?.DateE}'
AND t2.FCLOSESTATUS<>'B'
AND te.FMRPCLOSESTATUS<>'B'
AND t5.FCUSTTYPEID='673cb7c55ea24626ae639ff2ec5adf0e'
{wechatFilter}
GROUP BY t5.FNUMBER,t4.FNAME,t3L.FNAME";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<NotDeliverList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
        /// <summary>
        /// 获取客户未出货记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<PageResponse<CustNotDeliverList>> GetCustNotDeliverList(PageRequest<CrmReportRequestModel> request)
        {
            ResponseMessage<PageResponse<CustNotDeliverList>> response = new();
            PageResponse<CustNotDeliverList> page = new();

            int total = 0;
            string sSql = $@"SELECT t2.FBILLNO AS BillNo,t2.FAPPROVEDATE AS Approvedate,ml.FNUMBER AS ItemNumber,t1.FCANOUTQTY*tf.FTAXPRICE AS CanOutAmount
,t1.FCANOUTQTY AS CanoutQty,tf.FTAXPRICE AS TaxPrice
FROM dbo.T_SAL_ORDERENTRY_R t1
INNER JOIN dbo.T_SAL_ORDERENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
INNER JOIN dbo.T_SAL_ORDERENTRY tm ON t1.FENTRYID=tm.FENTRYID
INNER JOIN dbo.T_BD_MATERIAL ml ON tm.FMATERIALID=ml.FMATERIALID
INNER JOIN T_SAL_ORDER t2 ON t1.FID=t2.FID
INNER JOIN dbo.T_BD_CUSTOMER tc ON t2.FCUSTID=tc.FCUSTID
WHERE t1.FCANOUTQTY>0 AND t2.FDOCUMENTSTATUS='C' AND t2.FDATE >= '{request.Filter?.DateS}' AND t2.FDATE<'{request.Filter?.DateE}'
AND t2.FCLOSESTATUS<>'B'
AND tm.FMRPCLOSESTATUS<>'B'
AND tc.FNUMBER='{request.Filter?.CompanyCode}'";
            var datas = _kingdeeContent.SqlSugar.SqlQueryable<CustNotDeliverList>(sSql)
            .OrderBy($"{request.Filter?.SortFiled} {request.Filter?.Asc}")
            .ToPageList(request.PageIndex, request.PageSize, ref total);
            page.Total = total;
            page.Rows = datas;
            response.Data = page;
            return response;
        }
    }
}
