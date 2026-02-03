using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.NewtonsoftJsonConverter
{
    public class DateTimeConverter : JsonConverter
    {
        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
        private CultureInfo _culture;
        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

        public CultureInfo Culture
        {
            get
            {
                return _culture ?? CultureInfo.CurrentCulture;
            }
            set
            {
                _culture = value;
            }
        }
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
            {
                return true;
            }

            if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
            {
                return true;
            }

            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string value2;
            if (value is DateTime)
            {
                DateTime dateTime = (DateTime)value;
                if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
                {
                    dateTime = dateTime.ToUniversalTime();
                }

                value2 = dateTime.ToString(DefaultDateTimeFormat, Culture);
            }
            else
            {
                if (!(value is DateTimeOffset))
                {
                    throw new Exception("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got .");
                }
                DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
                if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
                {
                    dateTimeOffset = dateTimeOffset.ToUniversalTime();
                }

                value2 = dateTimeOffset.ToString(DefaultDateTimeFormat, Culture);
            }

            writer.WriteValue(value2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool flag = IsNullableType(objectType);
            Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullableType(objectType))
                {
                    throw new Exception($"Cannot convert null value to {objectType}.");
                }

                return null;
            }

            if (reader.TokenType == JsonToken.Date)
            {
                return ((DateTime)reader.Value).ToLocalTime();
			}
            if (reader.TokenType != JsonToken.String)
            {
                throw new Exception($"Unexpected token parsing date. Expected String, got {objectType}.");
            }

            string text = reader.Value.ToString();
            if (string.IsNullOrEmpty(text) && flag)
            {
                return null;
            }

            if (type == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(text, Culture, _dateTimeStyles);
            }

            return DateTime.Parse(text, Culture, _dateTimeStyles);
        }

        private void ArgumentNotNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private bool IsNullableType(Type t)
        {
            ArgumentNotNull(t, "t");
            if (t.IsGenericType)
            {
                return t.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return false;
        }
    }
}
