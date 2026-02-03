using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Kingdee.Mymooo.Core.NewtonsoftJsonConverter
{
    /// <summary>
    /// Bigint类型转换处理
    /// </summary>
    public class BigintConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(long) || objectType == typeof(ulong) || objectType == typeof(long?) || objectType == typeof(ulong?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return 0L;
            }
            else
            {
                IConvertible convertible = reader.Value as IConvertible;
                if (objectType == typeof(long) || objectType == typeof(long?))
                {
                    return convertible.ToInt64(CultureInfo.InvariantCulture);
                }
                else if (objectType == typeof(ulong) || objectType == typeof(ulong?))
                {
                    return convertible.ToUInt64(CultureInfo.InvariantCulture);
                }
                return 0;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(0);
            }
            else if (value is long || value is ulong || value is long? || value is ulong?)
            {
                writer.WriteValue(value);
            }
            else
            {
                throw new Exception("Expected Bigint value");
            }
        }
    }
}
