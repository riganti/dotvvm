using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmDateTimeConverter : JsonConverter<DateTime>
    {
        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
        {
            writer.WriteStringValue(new DateTime(date.Ticks, DateTimeKind.Unspecified)); // remove timezone information
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var date = reader.GetDateTime();
                return new DateTime(date.Ticks, DateTimeKind.Unspecified);
            }

            throw new JsonException("The value specified in the JSON could not be converted to DateTime!");
        }
    }
}
