using Kingdee.Mymooo.Core.NewtonsoftJsonConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Kingdee.Mymooo.Core.Utils
{
	public class JsonConvertUtils
    {
        private static JsonSerializerSettings _jsonSerializerSettings;
        static JsonConvertUtils()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>()
                {
                    new DecimalConverter(),
                    new DateTimeConverter(),
                    new BigintConverter(),
                    new BoolConverter(),
                }
            };
        }

        public static T DeserializeObject<T>(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException("data");
            }
            return JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings);
        }

        public static string SerializeObject<T>(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            return JsonConvert.SerializeObject(data, Formatting.None, _jsonSerializerSettings);
        }
    }
}
