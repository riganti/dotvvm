using System;
using System.Diagnostics;
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
#if !DotNetCore
// for some reason, STJ does not serialize these type automatically on .NET Framework
    public class DotvvmDateOnlyJsonConverter: JsonConverter<DateOnly>
    {
        public override void Write(Utf8JsonWriter writer, DateOnly date, JsonSerializerOptions options)
        {
            var dateWithoutTimezone = new DateOnly(date.Year, date.Month, date.Day);
            writer.WriteStringValue(dateWithoutTimezone.ToString("O", CultureInfo.InvariantCulture));
        }

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                DateOnly.TryParseExact(reader.GetString(), "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            throw new JsonException("The value speci!fied in the JSON could not be converted to DateTime!");
        }
    }
    public class DotvvmTimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        public override void Write(Utf8JsonWriter writer, TimeOnly time, JsonSerializerOptions serializer)
        {
            Span<byte> output = stackalloc byte[32];
 
            bool result = System.Buffers.Text.Utf8Formatter.TryFormat(time.ToTimeSpan(), output, out int bytesWritten, 'c');
            Debug.Assert(result);
 
            writer.WriteStringValue(output.Slice(0, bytesWritten));
        }

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                TimeSpan.TryParseExact(reader.GetString(), "c", CultureInfo.InvariantCulture, out var time))
            {
                return new TimeOnly(time.Ticks);
            }
            throw new JsonException("The value specified in the JSON could not be converted to TimeOnly!");
        }
    }
#endif
}
