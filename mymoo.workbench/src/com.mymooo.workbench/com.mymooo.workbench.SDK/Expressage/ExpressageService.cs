using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Expressage;
using com.mymooo.workbench.core.Expressage.Kuayue;
using com.mymooo.workbench.core.Expressage.ShunFeng;
using com.mymooo.workbench.core.Expressage.SuTeng;
using com.mymooo.workbench.core.Utils;
using Microsoft.Extensions.Options;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;

namespace com.mymooo.workbench.SDK.Expressage
{
    /// <summary>
    /// 物流信息
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class ExpressageService<C, U>(RedisCache redisCache,
            HttpService httpService,
            C mymoooContext) where U : UserBase, new() where C : MymoooContext<U>
    {
        private readonly RedisCache _redisCache = redisCache;
        private readonly HttpService _httpService = httpService;
        private readonly C _mymoooContext = mymoooContext;

        /// <summary>
		/// 根据物流单号获取路由信息(从缓存取)
		/// </summary>
		/// <param name="mailno">物流单号</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public List<ExpressageRouteDataEntity>? GetRouteData(string mailno)
        {
            if (string.IsNullOrWhiteSpace(mailno))
            {
                throw new ArgumentException($"“{nameof(mailno)}”不能为 null 或空白。", nameof(mailno));
            }
            return _redisCache.HashGet(new ExpressageRedisCache() { ExpressageNumber = mailno }, p => p.Routedata);
        }

        /// <summary>
        /// 根据物流单号获取路由信息(从快递公司取)
        /// </summary>
        /// <param name="mailno">物流单号</param>
        /// <param name="expressageCompany">物流公司编码，目前包含："shunfeng"、"kuayue"、"suteng"</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public async Task<List<ExpressageRouteDataEntity>>? GetRouteData(string mailno, string expressageCompany)
        {
            if (string.IsNullOrWhiteSpace(mailno))
            {
                throw new ArgumentException($"“{nameof(mailno)}”不能为 null 或空白。", nameof(mailno));
            }
            List<ExpressageRouteDataEntity> expressageRouteDataEntities = new List<ExpressageRouteDataEntity>();
            switch (expressageCompany)
            {
                case "shunfeng":
                    SearchRouteRequest searchRouteRequest = new SearchRouteRequest
                    {
                        TrackingNumber = [mailno]
                    };
                    ExpressageQueryRouteRequest expressageQueryRouteRequestShunfeng = new ExpressageQueryRouteRequest()
                    {
                        ExpressageQueryRouteRequestType = expressageCompany,
                        ExpressageQueryRouteRequestJsonStr = JsonSerializerOptionsUtils.Serialize(searchRouteRequest)
                    };
                    var result = await _httpService.InvokeWebServiceAsync($"workbench/{_mymoooContext.ApigatewayConfig.EnvCode}/Expressage/QueryRoute", JsonSerializerOptionsUtils.Serialize(expressageQueryRouteRequestShunfeng));
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        var shunFengResult = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(result);
                        if (shunFengResult != null && shunFengResult.apiResultCode == "A1000")
                        {
                            var shunFengResponse = JsonSerializerOptionsUtils.Deserialize<SearchRoutesResponse>(shunFengResult.apiResultData);
                            if (shunFengResponse != null && shunFengResponse.success && shunFengResponse.msgData != null && shunFengResponse.msgData.routeResps != null)
                            {
                                SearchRoutesResponse.Routeresp routeresp = shunFengResponse.msgData.routeResps.First(w => w.mailNo == mailno);
                                foreach (var route in routeresp.routes)
                                {
                                    ExpressageRouteDataEntity expressageRouteDataEntity = new()
                                    {
                                        Mailno = mailno,
                                        Node = Convert.ToInt32(DateTimeUtility.ConvertToTimeStamp(DateTime.Parse(route.acceptTime))),
                                        Step = route.opCode,
                                        Desc = route.remark,
                                        Time = route.acceptTime
                                    };
                                    expressageRouteDataEntities.Add(expressageRouteDataEntity);
                                }
                            }
                        }
                    }
                    return expressageRouteDataEntities;
                case "kuayue":
                    string[] waybillNumbers = new List<string> { mailno }.ToArray();
                    ExpressageQueryRouteRequest expressageQueryRouteRequestKuayue = new ExpressageQueryRouteRequest()
                    {
                        ExpressageQueryRouteRequestType = expressageCompany,
                        ExpressageQueryRouteRequestJsonStr = JsonSerializerOptionsUtils.Serialize(waybillNumbers)
                    };
                    var resultKuayue = await _httpService.InvokeWebServiceAsync($"workbench/{_mymoooContext.ApigatewayConfig.EnvCode}/Expressage/QueryRoute", JsonSerializerOptionsUtils.Serialize(expressageQueryRouteRequestKuayue));
                    if (!string.IsNullOrWhiteSpace(resultKuayue))
                    {
                        var kuayueResult = JsonSerializerOptionsUtils.Deserialize<KuaYueQueryRoute>(resultKuayue);
                        if (kuayueResult != null && kuayueResult.success && kuayueResult.data != null)
                        {
                            EsWaybillItem esWaybillItem = kuayueResult.data.esWaybill.First(f => f.waybillNumber == mailno);
                            foreach (var ex in esWaybillItem.exteriorRouteList)
                            {
                                ExpressageRouteDataEntity expressageRouteDataEntity = new()
                                {
                                    Mailno = mailno,
                                    Node = ex.id,
                                    Step = ex.routeStep,
                                    Desc = ex.routeDescription,
                                    Time = ex.uploadDate
                                };
                                expressageRouteDataEntities.Add(expressageRouteDataEntity);
                            }
                        }
                    }
                    return expressageRouteDataEntities;
                case "debang":
                    // 取消合作
                    //var deBangResult = _debangClient.InvokeWebService("sandbox-web/standard-order/newTraceQuery.action", JsonSerializerOptionsUtils.Serialize(new { mailno }));
                    //var deBangResponse = JsonSerializerOptionsUtils.Deserialize<TraceQueryResponse>(deBangResult);
                    //return Content(deBangResult);
                    break;
                case "huolala":
                    break;
                case "xinfeng":
                    break;
                case "suteng":
                    ExpressageQueryRouteRequest expressageQueryRouteRequestSuteng = new ExpressageQueryRouteRequest()
                    {
                        ExpressageQueryRouteRequestType = expressageCompany,
                        ExpressageQueryRouteRequestJsonStr = JsonSerializerOptionsUtils.Serialize(mailno)
                    };
                    var resultSuteng = await _httpService.InvokeWebServiceAsync($"workbench/{_mymoooContext.ApigatewayConfig.EnvCode}/Expressage/QueryRoute", JsonSerializerOptionsUtils.Serialize(expressageQueryRouteRequestSuteng));

                    if (!string.IsNullOrWhiteSpace(resultSuteng))
                    {
                        var suTengQueryRoute = JsonSerializerOptionsUtils.Deserialize<SuTengQueryRoute>(resultSuteng);
                        if (suTengQueryRoute != null && suTengQueryRoute.code == 0 && suTengQueryRoute.data != null)
                        {
                            List<SuTengRouteDataItem> suTengRouteDataItems = suTengQueryRoute.data;
                            foreach (var suteng in suTengRouteDataItems)
                            {
                                ExpressageRouteDataEntity expressageRouteDataEntity = new()
                                {
                                    Mailno = mailno,
                                    Node = suteng.optTime,
                                    Step = GetSuTengOptName(suteng.optType),
                                    Desc = suteng.content,
                                    Time = DateTimeOffset.FromUnixTimeSeconds(suteng.optTime).LocalDateTime.ToString()
                                };
                                expressageRouteDataEntities.Add(expressageRouteDataEntity);
                            }
                        }
                    }
                    return expressageRouteDataEntities;
                default:
                    break;
            }
            return expressageRouteDataEntities;
        }

        /// <summary>
		/// 根据物流单号获取回单图片链接
		/// </summary>
		/// <param name="mailno">物流单号</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public string? GetReceiptPictUrl(string mailno)
        {
            if (string.IsNullOrWhiteSpace(mailno))
            {
                throw new ArgumentException($"“{nameof(mailno)}”不能为 null 或空白。", nameof(mailno));
            }
            return _redisCache.HashGet(new ExpressageRedisCache() { ExpressageNumber = mailno }, p => p.Receiptpicturl);
        }

        /// <summary>
		/// 根据物流单号获取签收单图片链接
		/// </summary>
		/// <param name="mailno">物流单号</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public string? GetSignforPictUrl(string mailno)
        {
            if (string.IsNullOrWhiteSpace(mailno))
            {
                throw new ArgumentException($"“{nameof(mailno)}”不能为 null 或空白。", nameof(mailno));
            }
            return _redisCache.HashGet(new ExpressageRedisCache() { ExpressageNumber = mailno }, p => p.Signforpicturl);
        }

        /// <summary>
        /// 速腾物流根据操作码获取操作描述
        /// </summary>
        /// <param name="optCode"></param>
        /// <returns></returns>
        private string GetSuTengOptName(string optCode)
        {
            var dict = new Dictionary<string, string>()
            {
                { "01", "收件" },
                { "02", "发件" },
                { "03", "到件" },
                { "04", "派件" },
                { "08", "留仓件" },
                { "11", "发件确认" },
                { "12", "到件数量" },
                { "14", "签收" }
            };
            if (dict.Keys.Contains(optCode))
            {
                return dict[optCode];
            }
            else
            {
                return "";
            }
        }
    }
}
