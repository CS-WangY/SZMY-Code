using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Quotation;
using mymooo.core.Account;
using mymooo.core.Attributes;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace com.mymooo.mall.business.Service.SalesService
{
    /// <summary>
    /// 报价单缓存服务
    /// </summary>
    /// <param name="mymoooContext"></param>
    [AutoInject(InJectType.Scope)]
    public class QuotationCacheService(MallContext mymoooContext)
    {
        private readonly MallContext _mymoooContext = mymoooContext;


        public void UpdateCache(string inquiryNo)
        {
            UpdateQuotationCache(inquiryNo);
            UpdateFullQuotationCache(inquiryNo);
            UpdateMinQuotationCache(inquiryNo);
        }

        public void ReloadCache()
        {
            ReloadQuotationCache();
            ReloadFullCache();
            ReloadMinCache();
        }

        private void ReloadQuotationCache()
        {

            string sql = $@"select  
                                inq.FIM_INQ_CODE AS InquiryNo,quo.FQM_GENERATE_DATE AS AuditTime,quoDetail.Quantity AS Qty,quoDetail.FQD_UNIT_TAX_PRICE AS QuotationTaxPrice,
                                quoDetail.FQD_DELIVE_DAYS AS DeliveDays,inqDetail.FID_PRD_ID AS ProductId,inqDetail.FID_PRD_CODE AS ProductNumber,c.Code AS CompanyCode,c.Name AS CompanyName,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel
                                from F_CUST_INQUIRY_MSTR inq
                                inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
                                inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
                                inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
                                inner join Company c on CompanyId=c.Id
								left join Grade e on c.GradeLevel = e.Id
								left join GradeRule f on c.OccupancyId = f.Id
								left join GradeRule g on c.SensitivityId = g.Id
								inner join (
									select  
										MAX(quoDetail.FQD_INQ_DETAIL_ID) AS QuoDetailId,CompanyId,inqDetail.FID_PRD_CODE
										--INTO #QuotationTempResult
										from F_CUST_INQUIRY_MSTR inq
										inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
										inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
										inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
										where inq.FDeleted=0 and inqDetail.FStatus in(2,3) and quo.FQM_GENERATE_DATE >= @StartDate and quoDetail.FQD_UNIT_TAX_PRICE>0  
										Group By CompanyId,inqDetail.FID_PRD_CODE
								)tb  on quoDetail.FQD_INQ_DETAIL_ID =tb.QuoDetailId";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<QuotationHistoryPrice>(sql, new SugarParameter("@StartDate", startDate)).ToList();
            if (datas != null && datas.Count > 0) 
            {
                foreach (var item in datas)
                {
                    item.PriceSource = PriceSource.quotaion;
                    _mymoooContext.RedisCache.HashSet(item);
                }
            }
        }

        private void UpdateQuotationCache(string inquiryNo)
        {
            string sql = $@"select  
                                inq.FIM_INQ_CODE AS InquiryNo,quo.FQM_GENERATE_DATE AS AuditTime,quoDetail.Quantity AS Qty,quoDetail.FQD_UNIT_TAX_PRICE AS QuotationTaxPrice,
                                quoDetail.FQD_DELIVE_DAYS AS DeliveDays,inqDetail.FID_PRD_ID AS ProductId,inqDetail.FID_PRD_CODE AS ProductNumber,c.Code AS CompanyCode,c.Name AS CompanyName,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel
                                from F_CUST_INQUIRY_MSTR inq
                                inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
                                inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
                                inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
                                inner join Company c on CompanyId=c.Id
								left join Grade e on c.GradeLevel = e.Id
								left join GradeRule f on c.OccupancyId = f.Id
								left join GradeRule g on c.SensitivityId = g.Id
                                where inq.FDeleted=0 and quoDetail.FQD_UNIT_TAX_PRICE>0  and inqDetail.FStatus in(2,3)  
								and inq.FIM_INQ_CODE=@InquiryNo";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<QuotationHistoryPrice>(sql, new SugarParameter("@InquiryNo", inquiryNo)).ToList();
            if (datas != null && datas.Count() > 0)
            {
                foreach (var item in datas)
                {
                    item.PriceSource = PriceSource.quotaion;
                    _mymoooContext.RedisCache.HashSet(item);
                }
            }
        }

        private void ReloadFullCache()
        {

            string sql = $@"select  
                                inq.FIM_INQ_CODE AS InquiryNo,quo.FQM_GENERATE_DATE AS AuditTime,quoDetail.Quantity AS Qty,quoDetail.FQD_UNIT_TAX_PRICE AS QuotationTaxPrice,
                                quoDetail.FQD_DELIVE_DAYS AS DeliveDays,inqDetail.FID_PRD_ID AS ProductId,inqDetail.FID_PRD_CODE AS ProductNumber,c.Code AS CompanyCode,c.Name AS CompanyName,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel
                                from F_CUST_INQUIRY_MSTR inq
                                inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
                                inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
                                inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
                                inner join Company c on CompanyId=c.Id
								left join Grade e on c.GradeLevel = e.Id
								left join GradeRule f on c.OccupancyId = f.Id
								left join GradeRule g on c.SensitivityId = g.Id
								inner join (
									select  
										MAX(quoDetail.FQD_INQ_DETAIL_ID) AS QuoDetailId,inqDetail.FID_PRD_CODE
										--INTO #QuotationTempResult
										from F_CUST_INQUIRY_MSTR inq
										inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
										inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
										inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
										where inq.FDeleted=0 and inqDetail.FStatus in(2,3) and quo.FQM_GENERATE_DATE >= @StartDate and quoDetail.FQD_UNIT_TAX_PRICE>0  
										Group By inqDetail.FID_PRD_CODE
								)tb  on quoDetail.FQD_INQ_DETAIL_ID =tb.QuoDetailId";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<QuotationFullHistoryPrice>(sql, new SugarParameter("@StartDate", startDate)).ToList();
            if (datas != null && datas.Count > 0)
            {
                foreach (var item in datas)
                {
                    item.PriceSource = PriceSource.fquotaion;
                    _mymoooContext.RedisCache.HashSet(item);
                }
            }
        }

        private void UpdateFullQuotationCache(string inquiryNo)
        {
            string sql = $@"select  
                                inq.FIM_INQ_CODE AS InquiryNo,quo.FQM_GENERATE_DATE AS AuditTime,quoDetail.Quantity AS Qty,quoDetail.FQD_UNIT_TAX_PRICE AS QuotationTaxPrice,
                                quoDetail.FQD_DELIVE_DAYS AS DeliveDays,inqDetail.FID_PRD_ID AS ProductId,inqDetail.FID_PRD_CODE AS ProductNumber,c.Code AS CompanyCode,c.Name AS CompanyName,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel
                                from F_CUST_INQUIRY_MSTR inq
                                inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
                                inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
                                inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
                                inner join Company c on CompanyId=c.Id
								left join Grade e on c.GradeLevel = e.Id
								left join GradeRule f on c.OccupancyId = f.Id
								left join GradeRule g on c.SensitivityId = g.Id
								inner join (
									select  
										MAX(quoDetail.FQD_INQ_DETAIL_ID) AS QuoDetailId,inqDetail.FID_PRD_CODE
										--INTO #QuotationTempResult
										from F_CUST_INQUIRY_MSTR inq
										inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
										inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
										inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
										where inq.FDeleted=0 and inqDetail.FStatus in(2,3) and quoDetail.FQD_UNIT_TAX_PRICE>0 and inq.FIM_INQ_CODE=@InquiryNo
										Group By inqDetail.FID_PRD_CODE
								)tb  on quoDetail.FQD_INQ_DETAIL_ID =tb.QuoDetailId";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<QuotationFullHistoryPrice>(sql, new SugarParameter("@InquiryNo", inquiryNo)).ToList();
            if (datas != null && datas.Count() > 0)
            {
                foreach (var item in datas)
                {
                    item.PriceSource = PriceSource.fquotaion;
                    _mymoooContext.RedisCache.HashSet(item);
                }
            }
        }

        private void ReloadMinCache()
        {

            string sql = $@"select *
                    from (
                    select  
                        inq.FIM_INQ_CODE AS InquiryNo,quo.FQM_GENERATE_DATE AS AuditTime,quoDetail.Quantity AS Qty,quoDetail.FQD_UNIT_TAX_PRICE AS QuotationTaxPrice,
                        quoDetail.FQD_DELIVE_DAYS AS DeliveDays,inqDetail.FID_PRD_ID AS ProductId,inqDetail.FID_PRD_CODE AS ProductNumber,c.Code AS CompanyCode,c.Name AS CompanyName,
	                    row_number() over (partition by replace(replace(inqDetail.FID_PRD_CODE,'-',''), ' ', '') order by quoDetail.FQD_UNIT_TAX_PRICE asc) RowIndex,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel
                        from F_CUST_INQUIRY_MSTR inq
                        inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
                        inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
                        inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
                        inner join Company c on CompanyId=c.Id
						left join Grade e on c.GradeLevel = e.Id
						left join GradeRule f on c.OccupancyId = f.Id
						left join GradeRule g on c.SensitivityId = g.Id
	                    where inq.FDeleted=0 and inqDetail.FStatus in(2,3) and quo.FQM_GENERATE_DATE >= @StartDate and quoDetail.FQD_UNIT_TAX_PRICE>0 and quoDetail.AuditStatus=2
		                    ) t 
where t.RowIndex = 1";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<QuotationFullHistoryPrice>(sql, new SugarParameter("@StartDate", startDate)).ToList();
            if (datas != null && datas.Count() > 0)
            {
                foreach (var item in datas)
                {
                    item.PriceSource = PriceSource.minquotaion;
                    _mymoooContext.RedisCache.HashSet(item);
                }
            }
        }

        private void UpdateMinQuotationCache(string inquiryNo)
        {
            string sql = $@"select *
                    from (
                    select  
                        inq.FIM_INQ_CODE AS InquiryNo,quo.FQM_GENERATE_DATE AS AuditTime,quoDetail.Quantity AS Qty,quoDetail.FQD_UNIT_TAX_PRICE AS QuotationTaxPrice,
                        quoDetail.FQD_DELIVE_DAYS AS DeliveDays,inqDetail.FID_PRD_ID AS ProductId,inqDetail.FID_PRD_CODE AS ProductNumber,c.Code AS CompanyCode,c.Name AS CompanyName,
	                    row_number() over (partition by replace(replace(inqDetail.FID_PRD_CODE,'-',''), ' ', '') order by quoDetail.FQD_UNIT_TAX_PRICE asc) RowIndex,ISNULL(f.Value,'')+e.Level+ISNULL(g.Value,'') CompanyLevel
                        from F_CUST_INQUIRY_MSTR inq
                        inner join F_CUST_QUOTATION_MSTR quo on inq.FIM_INQ_ID = quo.FQM_INQ_ID
                        inner join F_CUST_QUOTATION_DETAIL quoDetail on quo.FQM_QUO_ID=quoDetail.FQD_QUO_MSTR_ID
                        inner join F_CUST_INQUIRY_DETAIL inqDetail on quoDetail.FQD_INQ_DETAIL_ID = inqDetail.FID_DETAIL_ID
                        inner join Company c on CompanyId=c.Id
                        left join Grade e on c.GradeLevel = e.Id
						left join GradeRule f on c.OccupancyId = f.Id
						left join GradeRule g on c.SensitivityId = g.Id
	                    where inq.FDeleted=0 and inqDetail.FStatus in(2,3) and  quoDetail.FQD_UNIT_TAX_PRICE>0 and inq.FIM_INQ_CODE=@InquiryNo and quoDetail.AuditStatus=2
		                    ) t 
where t.RowIndex = 1";
            var startDate = DateTime.Now.Date.AddYears(-1);
            var datas = _mymoooContext.SqlSugar.Ado.SqlQuery<QuotationFullHistoryPrice>(sql, new SugarParameter("@InquiryNo", inquiryNo)).ToList();
            if (datas != null && datas.Count() > 0)
            {
                foreach (var item in datas)
                {
                    item.PriceSource = PriceSource.minquotaion;
                    _mymoooContext.RedisCache.HashSet(item);
                }
            }
        }
    }
}
