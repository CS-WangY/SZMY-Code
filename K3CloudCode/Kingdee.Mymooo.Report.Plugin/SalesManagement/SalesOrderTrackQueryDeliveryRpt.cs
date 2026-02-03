using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单跟踪加急收货报表"), HotUpdate]
    public class SalesOrderTrackQueryDeliveryRpt : SysReportBaseService
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            //this.IsCreateTempTableByPlugin = false;
            //是否分组汇总
            this.ReportProperty.IsGroupSummary = false;
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            if (filter.CustomParams.ContainsKey("OpenParameter"))
            {
                Dictionary<string, object> OpenParameter = (Dictionary<string, object>)filter.CustomParams["OpenParameter"];

                if (OpenParameter.ContainsKey("FEntryIds"))
                {

                    var entryIds = OpenParameter["FEntryIds"].ToString();
                    string sql = $@"/*dialect*/select {KSQL_SEQ}
                             ,TE.FID as FSoId,TE.FENTRYID as FSoEntryId
                             ,T.FBILLNO FSoNo,TE.FSEQ FSoSeq
                             ,TE.FSupplyTargetOrgId
                             ,TE.FQTY FQty --销售数量
                             ,BDM.FNUMBER FMaterialCode,BDML.FNAME FMaterialName --物料
                             ,po.FID FPoId
                             ,TranmitStock.FSUPPLYENTRYID FPoEntryId --采购订单明细ID
                             ,po.FBILLNO FPoNo
                             ,poe.FSEQ FPoSeq
                             ,buy.FWECHATCODE FBuyWechatCode,buyl.FNAME FBuyName
                             ,asse.FNUMBER FTrackingCode,assel.FDATAVALUE FTrackingName,poe.FTrackingNumber,poe.FSupplierDescriptions
                             ,buy.FMOBILE,TranmitStock.FBaseQty
                             ,convert(nvarchar(2048),'') FTrackingResults
                             into {tableName}
                             from T_SAL_ORDERENTRY TE
                             inner join T_SAL_ORDERENTRY_R TER on TE.FENTRYID=TER.FENTRYID
                             inner join T_SAL_ORDER T on TE.FID=T.FID
                             left join T_BD_MATERIAL BDM on TE.FMATERIALID=BDM.FMATERIALID
                             left join T_BD_MATERIAL_L BDML on TE.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                             left join v_TranmitStockAll TranmitStock on TE.FENTRYID=TranmitStock.FSRCENTRYID and TranmitStock.FSUPPLYFORMID='PUR_PurchaseOrder'
                             left join T_PUR_POORDERENTRY poe on TranmitStock.FSUPPLYENTRYID=poe.FENTRYID
                             left join T_PUR_POORDER po on po.FID=poe.FID
                             left join V_BD_BUYER buy on po.FPURCHASERID=buy.fid
                             left join V_BD_BUYER_L buyl on buy.fid=buyl.fid
                             left join T_BAS_ASSISTANTDATAENTRY asse on asse.FENTRYID=poe.FTrackingID
                             left join T_BAS_ASSISTANTDATAENTRY_L assel on asse.FENTRYID=assel.FENTRYID and assel.FLOCALEID=2052
                             where TE.FENTRYID in ({entryIds}) ";
                    sql = string.Format(sql, " T.FCreateDate,T.FBILLNO,TE.FSEQ ");
                    DBUtils.Execute(this.Context, sql);
                    sql = $"select distinct FTrackingCode,FTrackingNumber,FMOBILE from {tableName} ";
                    //查询快递信息
                    var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                    List<string> keys = new List<string>();
                    foreach (var item in datas)
                    {
                        var trackingCode = Convert.ToString(item["FTrackingCode"]);
                        var trackingNumber = Convert.ToString(item["FTrackingNumber"]);
                        var mobile = Convert.ToString(item["FMOBILE"]);
                        var trackingResults = "";
                        if (!trackingCode.EqualsIgnoreCase("huolala") && !trackingCode.EqualsIgnoreCase("qita") && !string.IsNullOrEmpty(trackingCode))
                        {
                            //已经查询过，则不再查询，防止重复查询
                            if (keys.Where(x => x.Equals(trackingNumber)).Count() > 0)
                            {
                                continue;
                            }
                            var result = QueryTrack(trackingCode, trackingNumber, "");
                            if (result.Contains("验证码错误"))
                            {
                                //收、寄件人的电话号码（手机和固定电话均可，只能填写一个，顺丰速运、顺丰快运、中通快递必填
                                if (trackingCode.EqualsIgnoreCase("shunfeng") || trackingCode.EqualsIgnoreCase("shunfengkuaiyun") || trackingCode.EqualsIgnoreCase("zhongtong"))
                                {
                                    result = QueryTrack(trackingCode, trackingNumber, mobile);
                                }
                            }
                            var subscribeResponse = JsonConvert.DeserializeObject<SubscribeData>(result);
                            if (!string.IsNullOrEmpty(subscribeResponse?.status))
                            {
                                //成功
                                if (subscribeResponse.status.Equals("200"))
                                {
                                    trackingResults = subscribeResponse.data.OrderByDescending(p => p.time).FirstOrDefault()?.context;
                                }
                                else
                                {
                                    trackingResults = subscribeResponse.message;
                                }
                            }
                            else
                            {
                                trackingResults = subscribeResponse.message;
                            }
                            keys.Add(trackingNumber);
                            DBUtils.Execute(this.Context, $"update {tableName} set FTrackingResults=@FTrackingResults where FTrackingNumber=@FTrackingNumber",
                                         new List<SqlParam>() {
                                             new SqlParam("@FTrackingNumber", KDDbType.String, trackingNumber),
                                             new SqlParam("@FTrackingResults", KDDbType.String, trackingResults) });
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 物流查询
        /// </summary>
        /// <param name="_com">快递公司名称</param>
        /// <param name="_num">订单号</param>
        /// <param name="_phone">手机号</param>
        public string QueryTrack(string _com, string _num, string _phone)
        {
            var customer = "A1EF5F351FA1189EE272F4BEBF38194D";
            var key = "zsPYhHJH964";
            var queryTrackParam = new QueryTrackParam()
            {
                com = _com,
                num = _num,
                phone = _phone,
                resultv2 = "4"
            };

            var result = ExpressHelper.query(new QueryTrackReq()
            {
                customer = customer,
                sign = ExpressHelper.GetMD5(queryTrackParam.ToString() + key + customer),
                param = queryTrackParam
            });

            return result;

        }

        public class DataItem
        {
            /// <summary>
            /// 
            /// </summary>
            public string time { get; set; }
            /// <summary>
            /// 【杭州市】您的包裹已存放至【驿站】，记得早点来取它回家！
            /// </summary>
            public string context { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ftime { get; set; }
            /// <summary>
            /// 本数据元对应的行政区域编码，resultv2=1或者resultv2=4才会展示
            /// </summary>
            public string areaCode { get; set; }
            /// <summary>
            /// //本数据元对应的行政区域名称，resultv2=1或者resultv2=4才会展示
            /// </summary>
            public string areaName { get; set; }
            /// <summary>
            /// //本数据元对应的物流状态名称或者高级物流状态名称，resultv2=1或者resultv2=4才会展示
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// 本数据元对应的快件当前地点，resultv2=4才会展示
            /// </summary>
            public string location { get; set; }
            /// <summary>
            /// 本数据元对应的行政区域经纬度，resultv2=4才会展示
            /// </summary>
            public string areaCenter { get; set; }
            /// <summary>
            /// 本数据元对应的行政区域拼音，resultv2=4才会展示
            /// </summary>
            public string areaPinYin { get; set; }
            /// <summary>
            /// 本数据元对应的高级物流状态值，resultv2=4才会展示
            /// </summary>
            public string statusCode { get; set; }
        }

        public class SubscribeData
        {
            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 单号
            /// </summary>
            public string nu { get; set; }
            /// <summary>
            /// 是否签收标记，0未签收，1已签收，请忽略，明细状态请参考state字段
            /// </summary>
            public string ischeck { get; set; }
            /// <summary>
            /// 快递公司编码,一律用小写字母
            /// </summary>
            public string com { get; set; }
            /// <summary>
            /// 快递单当前状态，默认为0在途，1揽收，2疑难，3签收，4退签，5派件，8清关，14拒签等10个基础物流状态，如需要返回高级物流状态，请参考 resultv2 传值
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<DataItem> data { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string state { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string condition { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string isLoop { get; set; }
        }

        public class QueryTrackParam
        {
            /**
             * 查询的快递公司的编码，一律用小写字母
             */
            public string com { get; set; }
            /**
             * 查询的快递单号， 单号的最大长度是32个字符
             */
            public string num { get; set; }
            /**
             * 收件人或寄件人的手机号或固话
             */
            public string phone { get; set; }
            /**
             * 出发地城市，省-市-区
             */
            public string from { get; set; }
            /**
             * 目的地城市，省-市-区
             */
            public string to { get; set; }
            /**
             * 添加此字段表示开通行政区域解析功能。0：关闭（默认），1：开通行政区域解析功能，2：开通行政解析功能并且返回出发、目的及当前城市信息
             */
            public string resultv2 { get; set; }
            /**
             * 返回数据格式。0：json（默认），1：xml，2：html，3：text
             */
            public string show { get; set; }
            /**
             * 返回结果排序方式。desc：降序（默认），asc：升序
             */
            public string order { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            }

        }

        public class QueryTrackReq
        {
            /**
             * 我方分配给贵司的的公司编号, 点击查看账号信息
             */
            public string customer { get; set; }
            /**
             * 签名， 用于验证身份， 按param + key + customer 的顺序进行MD5加密（注意加密后字符串要转大写）， 不需要“+”号
             */
            public string sign { get; set; }
            /**
             * 其他参数组合成的json对象
             */
            public QueryTrackParam param { get; set; }
        }

        public class SubscribeResponse
        {
            public string Result { get; set; }

            public string ReturnCode { get; set; }

            public string Message { get; set; }
        }

    }

    public static class ExpressHelper
    {
        public static string GetMD5(this string data)
        {
            using (var md5 = MD5.Create())
            {
                var hsah = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hsah).Replace("-", "");
            }
        }

        public static string query(SalesOrderTrackQueryDeliveryRpt.QueryTrackReq query)
        {

            var request = ObjectToMap(query);
            if (request == null)
            {
                return null;
            }
            var result = doPostForm("http://poll.kuaidi100.com/poll/query.do", request);
            return result;
        }

        public static Dictionary<string, string> ObjectToMap(object obj)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            Type t = obj.GetType(); // 获取对象对应的类， 对应的类型

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性

            foreach (PropertyInfo p in pi)
            {
                MethodInfo m = p.GetGetMethod();

                if (m != null && m.IsPublic)
                {
                    // 进行判NULL处理
                    if (m.Invoke(obj, new object[] { }) != null)
                    {
                        map.Add(p.Name, m.Invoke(obj, new object[] { })
                                         .ToString()); // 向字典添加元素
                    }
                }
            }
            return map;
        }

        public static string doPostForm(string url, Dictionary<string, string> param)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var multipartFormDataContent = new FormUrlEncodedContent(param))
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(param));
                        var result = client.PostAsync(url, multipartFormDataContent).Result.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(result);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
