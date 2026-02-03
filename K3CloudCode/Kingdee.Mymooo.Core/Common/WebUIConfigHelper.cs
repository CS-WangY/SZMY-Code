using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    /// <summary>
    /// WebUI配置辅助类
    /// </summary>
    public static class WebUIConfigHelper
    {
        #region 公共

        #region AppSettings相关配置文件

        /// <summary>
        /// 统一平台提供给ERP调用，mq消息队列mes系统AppId
        /// </summary>
        public readonly static string WorkbenchForMqMesAppId = ConfigurationManager.AppSettings["WorkbenchForMqMesAppId"].AppSettingWithEmpty();

        /// <summary>
        /// erp派产云平台
        /// </summary>
        //public readonly static string DispatchToCloudUrl = ConfigurationManager.AppSettings["DispatchToCloudUrl"].AppSettingWithEmpty();

        /// <summary>
        /// 统一平台提供给ERP调用，mq消息队列mes系统地址
        /// </summary>
        public readonly static string WorkbenchForMqMesUrl = ConfigurationManager.AppSettings["WorkbenchForMqMesUrl"].AppSettingWithEmpty();

        #endregion

        #endregion

        #region 私有

        /// <summary>
        /// 获得指定配置节的字符串，如果不存在则返回空字符串
        /// </summary>
        /// <param name="appSetting">配置节</param>
        /// <returns>字符串</returns>
        private static string AppSettingWithEmpty(this string appSetting)
        {
            return appSetting != null ? appSetting : string.Empty;
        }

        #endregion
    }
}
