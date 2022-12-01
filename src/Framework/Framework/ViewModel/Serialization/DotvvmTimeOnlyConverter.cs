using System;
using System.Globalization;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmTimeOnlyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var date = (TimeOnly)value;
                var dateWithoutTimezone = new TimeOnly(date.Hour, date.Minute, date.Second);
                writer.WriteValue(dateWithoutTimezone.ToString("O", CultureInfo.InvariantCulture));
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType == typeof(TimeOnly))
                {
                    return TimeOnly.MinValue;
                }
                else
                {
                    return null;
                }
            }
            else if (reader.TokenType == JsonToken.Date)
            {
                return (TimeOnly)reader.Value;
            }
            else if (reader.TokenType == JsonToken.String
                     && TimeOnly.TryParseExact((string)reader.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            throw new JsonSerializationException("The value specified in the JSON could not be converted to DateTime!");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeOnly) || objectType == typeof(TimeOnly?);
        }
    }
}
