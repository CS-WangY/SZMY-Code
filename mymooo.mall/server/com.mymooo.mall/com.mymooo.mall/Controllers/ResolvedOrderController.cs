using com.mymooo.mall.business.Service;
using com.mymooo.mall.business.Service.OldPlatformAdmin;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.ResolveOrder;
using com.mymooo.mall.core.SqlSugarCore.SalesBusiness;
using com.mymooo.mall.wcf.InquiryServices;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;

namespace com.mymooo.mall.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mymoooContext"></param>
    /// <param name="scmService"></param>
    /// <param name="inquiryService"></param>
	public class ResolvedOrderController(MallContext mymoooContext, ScmService scmService, InquiryServiceClient inquiryService) : BaseController
    {

     
        private readonly MallContext _mymoooContext = mymoooContext;
        private readonly ScmService _scmService = scmService;
        private readonly InquiryServiceClient _inquiryService = inquiryService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
		[HttpPost]
        public async  Task<ResponseMessage<PageResponse<ResoleMapResp>>> ProductCodeMap([FromBody] ResolveItemsReq req)
        {
            ResponseMessage<PageResponse<ResoleMapResp>> response = new();
            if (req == null)
            {
                response.Code = ResponseCode.ModelError;
                response.ErrorMessage = "出错啦,入参数据为空";
                return response;
            }
            List<ResoleMapResp> result = [];
            //1,查询SCM维护的客户替换型号
            //2,查询资料部门维护的系统产品通用替换型号
            //3,查询SCM维护的通用替换型号
            //4,分解单产品工程师改过的替换型号.
            var replaceProductResult = await _scmService.QueryRelationModelList(req.CompanyCode, [req.SearchPrdCode]);
            foreach (var item in replaceProductResult)
            {
                ResoleMapResp rmp = new ResoleMapResp
                {
                    ProductId = item.ProductId,
                    Remark = string.IsNullOrEmpty(item.DifferenceRemark) ? string.Empty : item.DifferenceRemark,
                    ResolvedOrderNumber = string.Empty,
                    CustomerCode = item.CompanyCode,
                    CustPrdCode = item.ReplaceModel,
                    PrdCode = item.AntModel,
                    ReplaceType = item.DataType,
                    Brand = string.IsNullOrEmpty(item.ReplaceBrand) ? string.Empty : item.ReplaceBrand,
                    AuditTime = string.Empty  //SCM 替换型号没有所谓的分解单审核
                };

                if (string.IsNullOrEmpty(req.ReplaceType))
                {
                    result.Add(rmp);
                }
                else if (req.ReplaceType == item.DataType)
                {
                    result.Add(rmp);
                }  
            }

            if (string.IsNullOrEmpty(req.ReplaceType) || req.ReplaceType == "standard")
            {
                // 查询资料部门维护的系统产品通用替换型号
                List<InqDetailInfo> inqDetailReq = [];

                    InqDetailInfo inqDetail = new()
					{
                        PrdCode = req.SearchPrdCode,
                       // CustItem = item.CustPrdCode,
                        BrandId = 104499,
                        Num = 1
                    };
                    inqDetailReq.Add(inqDetail);
             

                var validateRequest = new InquiryInfo
                {
                    IsPass = true,
                    FacList =
                    [
                        new FntFacInfo()
                    ],
                    InqDetailList = inqDetailReq
                };
                var analyseResult = await _inquiryService.ValidateInquiryAsync(validateRequest);
                if (analyseResult.InqDetailList != null)
                {
                    foreach (var item in analyseResult.InqDetailList)
                    {
                        ResoleMapResp rmp = new ResoleMapResp
                        {
                            ProductId = item.PrdId,
                            Remark = string.IsNullOrEmpty(item.Memo) ? string.Empty : item.Memo,
                            ResolvedOrderNumber = string.Empty,
                            CustomerCode = string.Empty,
                            CustPrdCode = item.CustItem,
                            PrdCode = item.PrdCode,
                            ReplaceType = "standard",
                            Brand = string.IsNullOrEmpty(item.BrandCode) ? string.Empty : item.BrandCode,
                            AuditTime = string.Empty
                        };
                        result.Add(rmp);
                    }
                }
            }

            List<ResoleMapResp> engResult = [];
            // 查询分解历史
            if (string.IsNullOrEmpty(req.ReplaceType) || req.ReplaceType  == "resolve")
            {
                    var searchCondition = _mymoooContext.SqlSugar.Queryable<ResolvedHistory>()
                        .InnerJoin<ResolvedOrderItem>((rh, roi) => rh.ResolvedItemId == roi.Id)
                        .Where((rh, roi) => rh.PrdCode != rh.CustPrdCode);

                    if (!string.IsNullOrEmpty(req.SearchPrdCode))
                    {
                       searchCondition.Where((rh, roi) => rh.CustPrdCode == req.SearchPrdCode || rh.PrdCode == req.SearchPrdCode); 
                    }
                    if (!string.IsNullOrEmpty(req.CompanyCode))
                    {
                        searchCondition.Where((rh, roi) => rh.CustomerCode == req.CompanyCode);
                    }
                    if (req.AuditDateStart != null)
                    {
                        searchCondition.Where((rh, roi) => rh.AuditTime >= req.AuditDateStart);
                    }
                    if (req.AuditDateEnd != null)
                    {
                        searchCondition.Where((rh, roi) => rh.AuditTime <= req.AuditDateEnd.Value.AddDays(1));
                    }

                    var resolveList = searchCondition.OrderByDescending((rh,roi) => rh.AuditTime)
                    .Select((rh,roi) => new
                    {
                        rh.AuditTime,
                        roi.ResolvedOrderNumber,
                        roi.ProductId,
                        rh.PrdCode,
                        rh.CustPrdCode,
                        rh.Remark,
                        rh.CustomerCode
                    })
                    .ToList();
                foreach (var item in resolveList)
                {

                    ResoleMapResp rmp = new ResoleMapResp
                    {
                        ProductId = item.ProductId,
                        Remark = string.IsNullOrEmpty(item.Remark) ? string.Empty : item.Remark,
                        ResolvedOrderNumber = item.ResolvedOrderNumber,
                        CustomerCode = item.CustomerCode,
                        CustPrdCode = item.CustPrdCode,
                        PrdCode = item.PrdCode,
                        ReplaceType = "resolve",
                        Brand = string.Empty,
                        AuditTime = item.AuditTime.HasValue ? item.AuditTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty
                    };
                    engResult.Add(rmp);
                }
            }

            // 需求,如果前面检索到了.则用前面检索的替换类型描述
            foreach (var it in engResult)
            {
                var t = result.Where(r=> r.PrdCode == it.PrdCode).FirstOrDefault();
                if (t != null)
                {
                    it.ReplaceType = t.ReplaceType;
                }
                result.Add(it);
            }

            // 整理一下数据
            foreach (var it in result)
            {
                if (it.ReplaceType == "resolve")
                {
                    it.ReplaceType = "产品工程师替换";
                }
                else if (it.ReplaceType == "common")
                {
                    it.ReplaceType = "通用替换";
                }
                else if (it.ReplaceType == "standard")
                {
                    it.ReplaceType = "资料部门替换";

                }
                else if (it.ReplaceType == "customer")
                {
                    it.ReplaceType = "客户替换";
                }                
            }

            var total = result.Count;
            result = result.Skip((req.PageIndex - 1) * req.PageSize).Take(req.PageSize).ToList();
           
            response.Code = ResponseCode.Success;
            response.Data = new PageResponse<ResoleMapResp>(req.PageIndex, req.PageSize, total);
            response.Data.Rows = result;
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseMessage<List<ResolveProductItem>>> ProductCodeMapFlag([FromBody] List<ResolveProductItem> req)
        {
            ResponseMessage<List<ResolveProductItem>> response = new();

            if (req == null)
            {
                response.Code = ResponseCode.ModelError;
                response.ErrorMessage = "出错啦,入参数据为空";
                return response;
            }
            List<ResolveProductItem> result = [];
            //1,查询SCM维护的客户替换型号
            //2,查询资料部门维护的系统产品通用替换型号
            //3,查询SCM维护的通用替换型号
            //4,分解单产品工程师改过的替换型号.

            // 这个 PrdCode 不一定是蚂蚁型号.
            var replaceProductResult = await _scmService.QueryRelationModelList(string.Empty, req.Select(r => r.PrdCode).ToList());
            foreach (var item in replaceProductResult)
            {
                ResolveProductItem rmp = new ResolveProductItem
                {
                    CustPrdCode = item.ReplaceModel,
                    PrdCode = item.AntModel
                };
                result.Add(rmp);
                req.Remove(rmp);
            }

            // 查询资料部门维护的系统产品通用替换型号
            List<InqDetailInfo> inqDetailReq = new List<InqDetailInfo>();
            foreach (var item in req)
            {
                InqDetailInfo inqDetail = new InqDetailInfo
                {
                    PrdCode = item.PrdCode,
                    //CustItem = item.CustPrdCode,
                    BrandId = 104499,
                    Num = 1
                };
                inqDetailReq.Add(inqDetail);
            }

            var validateRequest = new InquiryInfo
            {
                IsPass = true,
                FacList =
                [
                    new FntFacInfo()
                ],
                InqDetailList = inqDetailReq
            };
            var analyseResult = await _inquiryService.ValidateInquiryAsync(validateRequest);
            if (analyseResult.InqDetailList != null)
            {
                foreach (var item in analyseResult.InqDetailList)
                {
                    ResolveProductItem rmp = new ResolveProductItem
                    {
                        //CustPrdCode = item.CustItem,
                        PrdCode = item.PrdCode
                    };
                    result.Add(rmp);
                    req.Remove(rmp);
                }
            }
            // 查询分解历史
            var searchCondition = _mymoooContext.SqlSugar.Queryable<ResolvedHistory>()
                .InnerJoin<ResolvedOrderItem>((rh, roi) => rh.ResolvedItemId == roi.Id)
                .Where((rh, roi) => rh.PrdCode != rh.CustPrdCode);
//req.Select(r=>r.CustPrdCode).ToList().Contains(rh.PrdCode) ||
            searchCondition.Where((rh, roi) =>   req.Select(r =>r.PrdCode).ToList().Contains(rh.CustPrdCode) || req.Select(r => r.PrdCode).ToList().Contains(rh.PrdCode));
            searchCondition.Where((rh, roi) => rh.AuditTime >= DateTime.Now.AddYears(-3));
            searchCondition.Where((rh, roi) => rh.AuditTime <= DateTime.Now.AddDays(1));
            var resolveList = searchCondition
            .Select((rh, roi) => new
            {
                rh.PrdCode,
             //   rh.CustPrdCode
            })
            .Distinct()
            .ToList();
            foreach (var item in resolveList)
            {

                ResolveProductItem rmp = new ResolveProductItem
                {
                   // CustPrdCode = item.CustPrdCode,
                    PrdCode = item.PrdCode,
                };
                result.Add(rmp);
            }

            // 分解历史.替换类型要变一下,根据前面2中中存在的,应用前面2钟类型.
            response.Code = ResponseCode.Success;
            response.Data = result;
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseMessage<List<ResoleMapResp>> ProductResolveMappingRecord([FromBody] List<ResoleMapResp> req)
        {
            ResponseMessage<List<ResoleMapResp>> response = new();

            if (req == null)
            {
                response.Code = ResponseCode.ModelError;
                response.ErrorMessage = "出错啦,入参数据为空";
                return response;
            }
            List<ResoleMapResp> result = [];
            // 查询分解历史
            var searchCondition = _mymoooContext.SqlSugar.Queryable<ResolvedHistory>()
                .InnerJoin<ResolvedOrderItem>((rh, roi) => rh.ResolvedItemId == roi.Id)
                .Where((rh, roi) => rh.PrdCode != rh.CustPrdCode);
            //req.Select(r=>r.CustPrdCode).ToList().Contains(rh.PrdCode) ||
            searchCondition.Where((rh, roi) => req.Select(r => r.PrdCode).ToList().Contains(rh.CustPrdCode) || req.Select(r => r.PrdCode).ToList().Contains(rh.PrdCode));
            searchCondition.Where((rh, roi) => rh.AuditTime >= DateTime.Now.AddYears(-3));
            searchCondition.Where((rh, roi) => rh.AuditTime <= DateTime.Now.AddDays(1));
            var resolveList = searchCondition
            .Select((rh, roi) => new ProductResolveMappingDto
            {
                PrdCode=rh.PrdCode,
                AuditTime=rh.AuditTime,
                Brand=rh.Brand,
                ProductId=roi.ProductId,
                  CustPrdCode=rh.CustPrdCode,
                   ResolvedOrderNumber=roi.ResolvedOrderNumber,
                    
            })
            .Distinct()
            .ToList();
            // 分解历史.替换类型要变一下,根据前面2中中存在的,应用前面2钟类型.
            response.Code = ResponseCode.Success;
            response.Data = result;
            return response;
        }
    }
}
