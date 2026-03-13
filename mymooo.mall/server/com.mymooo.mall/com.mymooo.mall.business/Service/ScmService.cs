using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.Price;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Stock;
using com.mymooo.mall.core.SqlSugarCore.SalesBusiness;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;
using Newtonsoft.Json;

namespace com.mymooo.mall.business.Service
{
	[AutoInject(InJectType.Scope)]
	public class ScmService(MallContext mymoooContext, HttpService httpService)
	{
		private readonly HttpService _httpService = httpService;
		private readonly MallContext _mymoooContext = mymoooContext;

		public async Task<List<ReplaceModelModel>> QueryModelReplace(string companyCode, List<string> productNumbers)
		{
			var request = new
			{
				companyCode,
				ModelList = productNumbers.Select(x => new { ProductModel = x })
			};
			var response = await _httpService.InvokeWebServiceAsync($"srm/{_mymoooContext.ApigatewayConfig.EnvCode}/ReplaceModel/QueryModelReplace", JsonSerializerOptionsUtils.Serialize(request));
			var result = JsonSerializerOptionsUtils.Deserialize<MessagesHelp<List<ReplaceModelModel>>>(response);
			if (result != null && result.IsSuccess && result.Data != null)
			{
				return result.Data;
			}
			return [];
		}

        // 分解单查询匹配型号.
        public async Task<List<ReplaceModelModel>> QueryRelationModelList(string companyCode, List<string> productNumbers)
        {

            List<string> companyList = [];
            if (!string.IsNullOrEmpty(companyCode))
            {
                string Sql = @" Select pc.Code From Company as c 
                            LEFT JOIN SubAndParentCompany as s on c.Id = s.CompanyId
                            LEFT JOIN Company as pc on s.ParentCompanyId = pc.Id
                           where c.Code = @companyCode And s.IsValid=1 ";
                companyList = _mymoooContext.SqlSugar.Ado.SqlQuery<string>(Sql,
                    new
                    {
                        companyCode
                    }
                    ).ToList();

                companyList.Add(companyCode);

                companyList = companyList.Distinct().ToList();
            }
            // 获取 父公司Code.
            var request = new
            {
                companyCode = companyList,
                ModelList = productNumbers.Select(x => new { ProductModel = x }).ToList()
            };
            var response = await _httpService.InvokeWebServiceAsync($"srm/{_mymoooContext.ApigatewayConfig.EnvCode}/ReplaceModel/QueryRelationModelList", JsonSerializerOptionsUtils.Serialize(request));
            var result = JsonSerializerOptionsUtils.Deserialize<MessagesHelp<List<ReplaceModelModel>>>(response);
            if (result != null && result.IsSuccess && result.Data != null)
            {
                return result.Data;
            }
            return [];
        }

        public List<SupplierPriceRep> GetSuplierPrice(List<SupplierPriceApiReq> apiDataBody)
		{
            string inputData = JsonConvert.SerializeObject(new
            {
                DataSource = 2,   //2,缓存+DB
                ProductCodes = apiDataBody
            });
            // 匹配供应商价目表价格
            string sResult = _httpService.InvokeWebService($"srm/{_mymoooContext.ApigatewayConfig.EnvCode}/Matrix/GetCompanyMatrixProductPrices", inputData);
            
            var SupplierPriceRep = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<List<SupplierPriceApiResponse>>>(sResult);

			if (SupplierPriceRep == null)
			{
				throw new Exception("调用SRM供应商价目表失败, 返回为空/Matrix/GetCompanyMatrixProductPrices");
			}
            if (!SupplierPriceRep.IsSuccess)
            {
                throw new Exception("调用SRM供应商价目表失败" + SupplierPriceRep.ErrorMessage);
            }

            List< SupplierPriceRep > reps = [];
            if (SupplierPriceRep != null && SupplierPriceRep.Data != null && SupplierPriceRep.Data.Count > 0)
            {
				foreach (var it in SupplierPriceRep.Data)
				{
					//if (it.SuppliserPrice.Count > 0)   // 多供应商, 第 0 个价格最低
                    foreach (var iP in it.SuppliserPrice)  
					{
					    SupplierPriceRep spr = new()
					    {
						    ProductCode = it.Number
					    };
						spr.SupplierCode = iP.Code;
						spr.SupplierName = iP.Name;
						spr.SupplierUnitPrice = iP.Price;
						reps.Add(spr);
					}
                }
            }
			return reps;
		}

        public List<SafetyStockQuerySimpleDto> SupplierSafetyStockQuery(SafetyStockQueryReq safetyStockQuery)
        {
            string sResult = _httpService.InvokeWebService($"srm/{_mymoooContext.ApigatewayConfig.EnvCode}/SupplierSafetyStock/QuerySafetyStock", JsonConvert.SerializeObject(safetyStockQuery));
            var SupplierStockRep = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<List<SafetyStockQuerySimpleDto>>>(sResult);
            if (SupplierStockRep != null && SupplierStockRep.Code == ResponseCode.Success)
            {
                if (SupplierStockRep.Data != null && SupplierStockRep.Data.Count > 0)
                {
                    return SupplierStockRep.Data;
                }                
            }
            return [];
        }


        public List<string> GetUserBySmallClassId(long smallClassId)
        {
            SmallClassIdReq req = new SmallClassIdReq();
            req.SmallClassId = smallClassId;
            string sResult = _httpService.InvokeWebService($"srm/{_mymoooContext.ApigatewayConfig.EnvCode}/System/GetUserBySmallClassId", JsonConvert.SerializeObject(req));
            var Users = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<List<string>>>(sResult);
            if (Users != null && Users.Code == ResponseCode.Success)
            {
                if (Users.Data != null && Users.Data.Count > 0)
                {
                    return Users.Data;
                }
            }
            //return [
            //    "黎俊杰",
            //    "李杰",
            //    "李佩珠",
            //    "李秋君",
            //    "李晚霞",
            //    "梁维",
            //    "栾海玉",
            //    "谭宗友",
            //    "王超",
            //    "温妃平",
            //    "吴仙欣",
            //    "向小红",
            //    "肖业强",
            //    "张志强",
            //    "钟斌斌"
            //    ];
            return [];
        }

    }
}
