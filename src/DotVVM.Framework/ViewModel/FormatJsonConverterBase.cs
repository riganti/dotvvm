using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// A <see cref="JsonConverter"/> that helps with conversions of viewmodel properties to and from string on client side to another type on the server side.
    /// </summary>
    public abstract class FormatJsonConverterBase<T> : JsonConverter where T: struct
    {
        
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(T) || objectType == typeof(T?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            if (reader.Value == null)
            {
                return objectType == typeof(T?) ? default(T?) : default(T);
            }
            else if (reader.ValueType == typeof(string))
            {
                var stringValue = reader.Value as string;
                T result;
                if (TryConvertFromString(stringValue, out result))
                {
                    return result;
                }
                return ProvideEmptyValue(objectType);
            }
            throw new JsonException($"The converter {GetType()} could not convert the value from string!");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is T)
            {
                writer.WriteValue(ConvertToString(value));
            }
            else if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                throw new JsonException($"The converter {GetType()} could not convert the value to string!");
            }
        }

        /// <summary>
        /// Tries the convert the value from string and returns whether the attempt was successful.
        /// </summary>
        /// <param name="value">A value to convert.</param>
        /// <param name="result">The result value in case the conversion was successful.</param>
        protected abstract bool TryConvertFromString(string value, out T result);

        /// <summary>
        /// Provides a value that should be used when the value couldn't be parsed from string.
        /// </summary>
        protected abstract T? ProvideEmptyValue(Type objectType);

        /// <summary>
        /// Converts the value to a string representation before sending it to the client.
        /// </summary>
        protected abstract string ConvertToString(object value);
    }
}
