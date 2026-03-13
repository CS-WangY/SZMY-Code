using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Enum
{
    /// <summary>
    /// 导入处理类型
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WebSocketImportType
    {
      AdressBook
    }
}
