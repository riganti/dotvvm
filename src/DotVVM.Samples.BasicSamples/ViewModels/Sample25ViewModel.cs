using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample25ViewModel : DotvvmViewModelBase
    {

        [JsonConverter(typeof(DateTimeConverter1))]
        public DateTime Date1 { get; set; }

        public string Date1String { get; set; }

        public void VerifyDate1()
        {
            Date1String = Date1.ToString("g");
        }

        public void SetDate1()
        {
            Date1 = DateTime.Now;
        }


        [JsonConverter(typeof(DateTimeConverter2))]
        public DateTime? Date2 { get; set; }

        public string Date2String { get; set; } = "null";

        public void VerifyDate2()
        {
            Date2String = Date2?.ToString("g") ?? "null";
        }
        public void SetDate2()
        {
            Date2 = DateTime.Now;
        }

    }

    public class DateTimeConverter1 : CustomDateTimeConverterBase
    {
        public DateTimeConverter1()
        {
            DateTimeFormat = "d.M.yyyy";
        }
    }

    public class DateTimeConverter2 : CustomDateTimeConverterBase
    {
        public DateTimeConverter2()
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }

    public abstract class CustomDateTimeConverterBase : JsonConverter
    {
        public string DateTimeFormat { get; set; }

        public DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.None;

        public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (string) || objectType == typeof (DateTime) || objectType == typeof(DateTime?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            if (reader.Value == null)
            {
                return objectType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
            }
            else if (reader.ValueType == typeof (string))
            {
                var stringValue = reader.Value as string;
                DateTime result;
                if (DateTime.TryParseExact(stringValue, DateTimeFormat, FormatProvider, DateTimeStyles, out result))
                {
                    return result;
                }
                return objectType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
            }
            throw new JsonException("Unable to parse DateTime value!");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                writer.WriteValue(((DateTime)value).ToString(DateTimeFormat, FormatProvider));
            }
            else if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                throw new JsonException("Unable to serialize DateTime value!");
            }
        }
    }
}
