using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using com.mymooo.workbench.qichacha.ResponseData;
using Microsoft.Extensions.Logging;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static com.mymooo.workbench.qichacha.ResponseData.TaxIllegalCheckResponse;

namespace com.mymooo.workbench.qichacha
{
    [AutoInject(InJectType.Scope)]
    public class QichachaService
    {
        private readonly ILogger<QichachaService> _logger;
        private readonly WorkbenchDbContext _workbenchDbContext;
        private readonly ThirdpartyApplicationConfig _applicationConfig;
        private readonly RedisCache _redisCache;

        public QichachaService(ILogger<QichachaService> logger, WorkbenchDbContext workbenchDbContext, RedisCache redisCache)
        {
            _logger = logger;
            _workbenchDbContext = workbenchDbContext;
            _redisCache = redisCache;
            try
            {
                _applicationConfig = _workbenchDbContext.ThirdpartyApplicationConfig.FirstOrDefault(c => c.AppId == "QiChaCha");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "加载企查查配置信息出错");
            }
        }

        /// <summary>
        /// 模糊查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>> ECIV4_Search(string keyword)
        {
            if (keyword == null || keyword.Length < 5)
            {
                ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>> message = new ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>>
                {
                    Code = ResponseCode.ModelError,
                    ErrorMessage = "输入的关键字不能少于5位"
                };
                return message;
            }
            return Search(_applicationConfig.Url + string.Format(QichachaPath.ECIV4_Search, _applicationConfig.Token, keyword));
        }

        /// <summary>
        /// 多维度查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>> ECIV4_SearchWide(string keyword, string type = "")
        {
            if (keyword == null || keyword.Length < 5)
            {
                ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>> message = new ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>>
                {
                    Code = ResponseCode.ModelError,
                    ErrorMessage = "输入的关键字不能少于5位"
                };
                return message;
            }
            return Search(_applicationConfig.Url + string.Format(QichachaPath.ECIV4_SearchWide, _applicationConfig.Token, keyword, type));
        }

        /// <summary>
        /// 企业工商详情
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<ECIV4GetBasicDetailsByNameResponse.ResponseResult> ECIV4_GetBasicDetailsByName(string keyword)
        {
            ResponseMessage<ECIV4GetBasicDetailsByNameResponse.ResponseResult> message = new ResponseMessage<ECIV4GetBasicDetailsByNameResponse.ResponseResult>();
            if (keyword == null || keyword.Length < 4)
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = "输入的关键字不能少于4位";
                return message;
            }
            var url = _applicationConfig.Url + string.Format(QichachaPath.ECIV4_GetBasicDetailsByName, _applicationConfig.Token, keyword);
            var result = HttpUtils.QichachaInvokeWebService(url, _applicationConfig.Token, _applicationConfig.Nonce);
            ECIV4GetBasicDetailsByNameResponse response = JsonConvert.DeserializeObject<ECIV4GetBasicDetailsByNameResponse>(result);
            if (response.Status == "200")
            {
                message.Code = ResponseCode.Success;
                message.Data = response.Result;
            }
            else
            {
                message.Code = ResponseCode.ThirdpartyError;
                message.ErrorMessage = response.Message;
            }
            return message;
        }

        /// <summary>
        /// 被执行人检查
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<ZhixingCheckResponse.ZhixingCheckReturn> ZhixingCheck(string keyword, string code, bool isOnlyCashe)
        {
            var lastDateWeek = GetLastDateWeek();
            ResponseMessage<ZhixingCheckResponse.ZhixingCheckReturn> message = new ResponseMessage<ZhixingCheckResponse.ZhixingCheckReturn>();
            if ((keyword == null || keyword.Length < 4) && !string.IsNullOrEmpty(code))
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = "输入的关键字不能少于4位";
                return message;
            }
            var data = new ZhixingCheckResponse.ZhixingCheckReturn();
            if (isOnlyCashe)
            {
                var entry = _redisCache.HashGetEntrys(new ZhixingCheckResponse.ZhixingCheckReturn { Code = code }).OrderByDescending(it => it.Name);
                if (entry.Any())
                { 
                 data = JsonConvert.DeserializeObject<ZhixingCheckResponse.ZhixingCheckReturn>(entry.FirstOrDefault().Value);
                }
            }
            else
            {
                data = _redisCache.HashGet(new ZhixingCheckResponse.ZhixingCheckReturn { Code = code, LastDateWeek = lastDateWeek });
            }
            if (data != null || isOnlyCashe)
            {
                message.Data = data;
                message.Code = ResponseCode.Success;
                return message;
            }
            else
            {

                var url = _applicationConfig.Url + string.Format(QichachaPath.ZhixingCheck, _applicationConfig.Token, keyword);
                var result = HttpUtils.QichachaInvokeWebService(url, _applicationConfig.Token, _applicationConfig.Nonce);
                var response = JsonConvert.DeserializeObject<ZhixingCheckResponse>(result);
                if (response.Status == "200")
                {

                    message.Code = ResponseCode.Success;
                    message.Data = response.Result;

                }
                else
                {
                    message.Code = ResponseCode.ThirdpartyError;
                    message.ErrorMessage = response.Message;
                }

                if (message.Code == ResponseCode.Success)
                {
                    var result2 = message.Data;
                    result2.LastDateWeek = lastDateWeek;
                    result2.LastSelectDate = DateTime.Now;
                    result2.Code = code;
                    _redisCache.HashSet(result2);
                }
                return message;
            }
        }


        /// <summary>
        /// 立案信息核查
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<CaseFilingCheckResponse.CaseFilingCheckReturn> CaseFilingCheck(string keyword, string code, bool isOnlyCashe)
        {
            var lastDateWeek = GetLastDateWeek();
            ResponseMessage<CaseFilingCheckResponse.CaseFilingCheckReturn> message = new ResponseMessage<CaseFilingCheckResponse.CaseFilingCheckReturn>();
            if ((keyword == null || keyword.Length < 4) && !string.IsNullOrWhiteSpace(code))
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = "输入的关键字不能少于4位";
                return message;
            }
            var data = new CaseFilingCheckResponse.CaseFilingCheckReturn();
            //var data = _redisCache.HashGet(new CaseFilingCheckResponse.CaseFilingCheckReturn { Code = code, LastDate = DateTime.Now.ToString("yyyy-MM-dd") });
            if (isOnlyCashe)
            {
                var entry = _redisCache.HashGetEntrys(new CaseFilingCheckResponse.CaseFilingCheckReturn { Code = code }).OrderByDescending(it => it.Name);
                if (entry.Any())
                { 
                 data = JsonConvert.DeserializeObject<CaseFilingCheckResponse.CaseFilingCheckReturn>(entry.FirstOrDefault().Value);
                }
            }
            else
            {
                data = _redisCache.HashGet(new CaseFilingCheckResponse.CaseFilingCheckReturn { Code = code, LastDateWeek = lastDateWeek });
            }
            if (data != null || isOnlyCashe)
            {
                message.Data = data;
                message.Code = ResponseCode.Success;
                return message;
            }
            else
            {

                var url = _applicationConfig.Url + string.Format(QichachaPath.CaseFilingCheck, _applicationConfig.Token, keyword);
                var result = HttpUtils.QichachaInvokeWebService(url, _applicationConfig.Token, _applicationConfig.Nonce);
                var response = JsonConvert.DeserializeObject<CaseFilingCheckResponse>(result);
                if (response.Status == "200")
                {

                    message.Code = ResponseCode.Success;
                    message.Data = response.Result;

                }
                else
                {
                    message.Code = ResponseCode.ThirdpartyError;
                    message.ErrorMessage = response.Message;
                }

                if (message.Code == ResponseCode.Success)
                {
                    var result2 = message.Data;
                    result2.LastDateWeek = lastDateWeek;
                    result2.LastSelectDate = DateTime.Now;
                    result2.Code = code;
                    _redisCache.HashSet(result2);
                }
                return message;
            }

        }


        /// <summary>
        /// 税收违法核查
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<TaxIllegalCheckReturn> TaxIllegalCheck(string keyword, string code, bool isOnlyCashe)
        {
            var lastDateWeek = GetLastDateWeek();
            ResponseMessage<TaxIllegalCheckReturn> message = new ResponseMessage<TaxIllegalCheckReturn>();
            if ((keyword == null || keyword.Length < 4) && !string.IsNullOrWhiteSpace(code))
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = "输入的关键字不能少于4位";
                return message;
            }
            //var data = _redisCache.HashGet(new TaxIllegalCheckReturn { Code = code, LastDate = DateTime.Now.ToString("yyyy-MM-dd") });
            var data = new TaxIllegalCheckReturn();
            if (isOnlyCashe)
            {
                var entry = _redisCache.HashGetEntrys(new TaxIllegalCheckReturn { Code = code }).OrderByDescending(it => it.Name);
                if (entry.Any())
                { 
                data = JsonConvert.DeserializeObject<TaxIllegalCheckReturn>(entry.FirstOrDefault().Value);
                }
            }
            else
            {
                data = _redisCache.HashGet(new TaxIllegalCheckReturn { Code = code, lastDateWeek = lastDateWeek });
            }
            if (data != null || isOnlyCashe)
            {
                message.Data = data;
                message.Code = ResponseCode.Success;
                return message;
            }
            else
            {

                var url = _applicationConfig.Url + string.Format(QichachaPath.TaxIllegalCheck, _applicationConfig.Token, keyword);
                var result = HttpUtils.QichachaInvokeWebService(url, _applicationConfig.Token, _applicationConfig.Nonce);
                var response = JsonConvert.DeserializeObject<TaxIllegalCheckResponse>(result);
                if (response.Status == "200")
                {

                    message.Code = ResponseCode.Success;
                    message.Data = response.Result;

                }
                else
                {
                    message.Code = ResponseCode.ThirdpartyError;
                    message.ErrorMessage = response.Message;
                }

                if (message.Code == ResponseCode.Success)
                {
                    var result2 = message.Data;
                    result2.lastDateWeek = lastDateWeek;
                    result2.LastSelectDate = DateTime.Now;
                    result2.Code = code;
                    _redisCache.HashSet(result2);
                }
                return message;
            }

        }

        /// <summary>
        /// 严重违法核查
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ResponseMessage<SeriousIllegalCheckResponse.SeriousIllegalCheckReturn> SeriousIllegalCheck(string keyword, string code, bool isOnlyCashe)
        {
            var lastDateWeek = GetLastDateWeek();
            ResponseMessage<SeriousIllegalCheckResponse.SeriousIllegalCheckReturn> message = new ResponseMessage<SeriousIllegalCheckResponse.SeriousIllegalCheckReturn>();
            if ((keyword == null || keyword.Length < 4) && !string.IsNullOrWhiteSpace(code))
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = "输入的关键字不能少于4位";
                return message;
            }
            //var data = _redisCache.HashGet(new SeriousIllegalCheckResponse.SeriousIllegalCheckReturn { Code = code, LastDate = DateTime.Now.ToString("yyyy-MM-dd") });
            var data = new SeriousIllegalCheckResponse.SeriousIllegalCheckReturn();
            if (isOnlyCashe)
            {
                var entry = _redisCache.HashGetEntrys(new SeriousIllegalCheckResponse.SeriousIllegalCheckReturn { Code = code }).OrderByDescending(it => it.Name);
                if (entry.Any())
                { 
                
                data = JsonConvert.DeserializeObject<SeriousIllegalCheckResponse.SeriousIllegalCheckReturn>(entry.FirstOrDefault().Value);
                }
            }
            else
            {
                data = _redisCache.HashGet(new SeriousIllegalCheckResponse.SeriousIllegalCheckReturn { Code = code, LastDateWeek = lastDateWeek });
            }
            if (data != null || isOnlyCashe)
            {
                message.Data = data;
                message.Code = ResponseCode.Success;
                return message;
            }
            else
            {
                var url = _applicationConfig.Url + string.Format(QichachaPath.SeriousIllegalCheck, _applicationConfig.Token, keyword);
                var result = HttpUtils.QichachaInvokeWebService(url, _applicationConfig.Token, _applicationConfig.Nonce);
                var response = JsonConvert.DeserializeObject<SeriousIllegalCheckResponse>(result);
                if (response.Status == "200")
                {

                    message.Code = ResponseCode.Success;
                    message.Data = response.Result;
                }
                else
                {
                    message.Code = ResponseCode.ThirdpartyError;
                    message.ErrorMessage = response.Message;
                }
                if (message.Code == ResponseCode.Success)
                {
                    var result2 = message.Data;
                    result2.LastDateWeek = lastDateWeek;
                    result2.LastSelectDate = DateTime.Now;
                    result2.Code = code;
                    _redisCache.HashSet(result2);
                }
                return message;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>> Search(string url)
        {
            ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>> message = new ResponseMessage<List<ECIV4SearchResponse.ECIV4SearchResult>>();
            var result = HttpUtils.QichachaInvokeWebService(url, _applicationConfig.Token, _applicationConfig.Nonce);
            ECIV4SearchResponse response = JsonConvert.DeserializeObject<ECIV4SearchResponse>(result);
            if (response.Status == "200")
            {
                message.Code = ResponseCode.Success;
                message.Data = response.Result.ToList();
            }
            else
            {
                message.Code = ResponseCode.ThirdpartyError;
                message.ErrorMessage = response.Message;
            }
            return message;
        }

        private long GetLastDateWeek()
        {
            var date = DateTime.Now;
            //int weekOfYear = new GregorianCalendar().GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            //var lastDateWeek = Convert.ToInt64(date.Year.ToString() + weekOfYear);
            //return lastDateWeek;
            var month = date.Month < 10 ? 0 + date.Month.ToString() : date.Month.ToString();
            var lastDateWeek = Convert.ToInt64(date.Year.ToString()+ month + (date.Day / 15));
            return lastDateWeek;
        }
    }
}
