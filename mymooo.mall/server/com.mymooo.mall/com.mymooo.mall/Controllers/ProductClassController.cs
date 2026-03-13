using com.mymooo.mall.business.Service;
using com.mymooo.mall.business.Service.BaseService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.InquiryOrder;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using Newtonsoft.Json;

namespace com.mymooo.mall.Controllers
{
	public class ProductClassController ( MallContext mymoooContext, ScmService scmService, ProductSmallService productSmallClass) : BaseController
    {

        private readonly MallContext _mymoooContext = mymoooContext;
        private readonly ScmService _scmService = scmService;
        private readonly ProductSmallService _productSmallClass = productSmallClass;
        /// <summary>
        /// 获取小类的事业部和供货组织, 只返回默认的.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseMessage<ProductSmallClassAndDivsionModel> GetSmallClassDeivisonAndSupplyOrg([FromBody] SmallClassReq req)
        {
            ResponseMessage<ProductSmallClassAndDivsionModel> response = new();

            if (req == null)
            {
                response.Code = ResponseCode.ModelError;
                response.ErrorMessage = "出错啦,没有提交的数据";
                return response;
            }

            // 加入一些服务器端校验.
            if (req?.Id == null)
            {
                response.Code = ResponseCode.ModelError;
                response.ErrorMessage = "出错啦,小类没有值";
                return response;
            }

            var smallClass = _mymoooContext.RedisCache.HashGet<ProductSmallClass>(new ProductSmallClass() { Id = req.Id });
            DivsionSupplyResponse  result = new DivsionSupplyResponse();
            List <string> engineers = [];
            if (smallClass != null)
            {
                // 小类可确定唯一的事业部.
                result.BusinessDivisionId = smallClass.BusinessDivisionId;
                result.BusinessDivisionNumber = smallClass.BusinessDivisionNumber;
                result.BusinessDivisionName = smallClass.BusinessDivisionName;
                result.SmallClassId = smallClass.Id;
                result.SmallClassName = smallClass.Name;
                result.IsDefault = true;  // 只返回缺省的.

                if ( smallClass.SupplyOrgs.Count > 0)
                {

                    var tempString = _mymoooContext.SqlSugar.Queryable<BusinessParamConfig>()
                      .Where(p => p.BKey == "SpecialSupplyOrgConfig")
                      .Select(p => p.BValue)
                      .First();
                    List<SpecialSupplyOrgResponse>? specialSupplyOrg = [];
                    if (!string.IsNullOrEmpty(tempString))
                    {
                        specialSupplyOrg = JsonConvert.DeserializeObject<List<SpecialSupplyOrgResponse>>(tempString);
                    }


                    if (specialSupplyOrg != null)
                    {
                        foreach (var it in specialSupplyOrg)
                        {
                            if (smallClass.Id == it.SmallClassId)
                            {
                                for (int i = 0; i < smallClass.SupplyOrgs.Count; i++)
                                {
                                    smallClass.SupplyOrgs[i].IsDefault = false;
                                    if (smallClass.SupplyOrgs[i].SupplyOrgNumber == it.SupplyOrgNumber)
                                    {
                                        smallClass.SupplyOrgs[i].IsDefault = true;
                                    }
                                }
                            }
                        }
                    }
                  
                    result.SupplyOrgId = smallClass.SupplyOrgs.Where(r => r.IsDefault).First().SupplyOrgId;
                    result.SupplyOrgName = smallClass.SupplyOrgs.Where(r => r.IsDefault).First().SupplyOrgName;
                    result.SupplyOrgNumber = smallClass.SupplyOrgs.Where(r => r.IsDefault).First().SupplyOrgNumber;


                    if (!string.IsNullOrEmpty(req.CompanyCode))
                    {
                        // 如果有客户和事业部对应
                        var customerSupplyOrg = _mymoooContext.SqlSugar.Queryable<CompanyMapSupplyOrg>()                        
                         .LeftJoin<BusinessDivisionSupplyOrg>((c,b) => c.SupplyOrgCode == b.SupplyOrgNumber )
                         .Where(c => c.CompanyCode == req.CompanyCode && c.BusinessDivisionNumber==smallClass.BusinessDivisionNumber && !c.IsDeleted)
                         .Select((c,b) => new { b.SupplyOrgNumber,b.SupplyOrgId,b.SupplyOrgName })
                         .First();
                        if (customerSupplyOrg!= null)
                        {
                            result.SupplyOrgId = customerSupplyOrg.SupplyOrgId;
                            result.SupplyOrgName = customerSupplyOrg.SupplyOrgName;
                            result.SupplyOrgNumber = customerSupplyOrg.SupplyOrgNumber;
                        }
                    }
                }

                // 获取该小类的 产品人员列表  调用SCM
                engineers = _scmService.GetUserBySmallClassId(smallClass.Id);
            }

            response.Data = new ProductSmallClassAndDivsionModel();
            response.Data.ProductSmallClass = smallClass;
            response.Data.DivsionSupplyDtos = result;
            response.Data.Engineers = engineers;
            response.Code = ResponseCode.Success;
	        return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ResponseMessage<List<ClassTreeModel>> GetClassTree()
        {
            var response = new ResponseMessage<List<ClassTreeModel>>();
            response.Data = _productSmallClass.GetClassTree();
            response.Code = ResponseCode.Success;
            return response;
        }
    }
}
