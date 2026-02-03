using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.NewtonsoftJsonConverter
{
    public class BoolConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool) || objectType == typeof(bool?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return false;
            }
            else
            {
                IConvertible convertible = reader.Value as IConvertible;
                return convertible.ToBoolean(CultureInfo.InvariantCulture);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(false);
            }
            else if (value is bool || value is bool?)
            {
                writer.WriteValue(value);
            }
            else
            {
                throw new Exception("Expected bool value");
            }
        }
    }
}
