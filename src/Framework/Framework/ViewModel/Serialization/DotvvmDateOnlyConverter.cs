using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmDateOnlyConverter : JsonConverter<DateOnly>
    {
        public override void Write(Utf8JsonWriter writer, DateOnly date, JsonSerializerOptions options)
        {
            var dateWithoutTimezone = new DateOnly(date.Year, date.Month, date.Day);
            writer.WriteStringValue(dateWithoutTimezone.ToString("O", CultureInfo.InvariantCulture)); // TODO: utf8
        }

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String
                     && DateOnly.TryParseExact(reader.GetString(), "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            throw new Exception("The value specified in the JSON could not be converted to DateTime!");
        }
    }
}
