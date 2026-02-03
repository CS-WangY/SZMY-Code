using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 创建一个记录实体
        /// </summary>
        private static log4net.ILog _log = log4net.LogManager.GetLogger("CommonLogger");

        /// <summary>
        /// 注册Log4配置文件
        /// </summary>
        public static void RegisterLog4Config()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(System.Web.HttpContext.Current.Server.MapPath("Config/log4net.config")));
        }

        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public static bool IsDebugEnabled
        {
            get
            {
                return _log.IsDebugEnabled;
            }
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            _log.Debug(message);
        }

        /// <summary>
        /// 记录消息日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            _log.Info(message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(string message)
        {
            _log.Warn(message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            _log.Error(message);
        }

        /// <summary>
        /// 记录指定的一个Exception的日志
        /// </summary>
        /// <param name="exception"></param>
        public static void Exception(Exception exception)
        {
            _log.Error(exception.Message, exception);
        }

        /// <summary>
        /// 记录指定的一个Exception的日志
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Exception(string message, Exception exception)
        {
            _log.Error(message, exception);
        }
    }
}
