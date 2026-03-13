using System;

namespace com.mymooo.workbench.qichacha
{
    public class QichachaPath
    {
        /// <summary>
        /// 模糊搜索
        /// </summary>
        public const string ECIV4_Search = "FuzzySearch/GetList?key={0}&searchKey={1}";

        /// <summary>
        /// 多维度查询
        /// </summary>
        public const string ECIV4_SearchWide = "ECIV4/SearchWide?key={0}&keyword={1}&type={2}&pageSize=20";

        /// <summary>
        /// 企业工商详情
        /// </summary>
        public const string ECIV4_GetBasicDetailsByName = "ECIV4/GetBasicDetailsByName?key={0}&keyword={1}";

        /// <summary>
        /// 被执行人核查
        /// </summary>
        public const string ZhixingCheck = "ZhixingCheck/GetList?key={0}&searchKey={1}&pageIndex=1&pageSize=20";


        /// <summary>
        /// 立案信息调查
        /// </summary>
        public const string CaseFilingCheck = "CaseFilingCheck/GetList?key={0}&searchKey={1}&pageIndex=1&pageSize=20";

        /// <summary>
        /// 严重违法调查
        /// </summary>
        public const string SeriousIllegalCheck = "SeriousIllegalCheck/GetList?key={0}&searchKey={1}";


        /// <summary>
        /// 税收违法核查
        /// </summary>
        public const string TaxIllegalCheck = "TaxIllegalCheck/GetList?key={0}&searchKey={1}&pageIndex=1&pageSize=20";

    }
}
