using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmDictionaryConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var dict = value as IDictionary;

                var attrs = value.GetType().GetGenericArguments();
                var keyValuePair = typeof(KeyValuePair<,>).MakeGenericType(attrs);
                var listType = typeof(List<>).MakeGenericType(keyValuePair);


                var keyEnumerator = dict.Keys.GetEnumerator();
                var valuesEnumerator = dict.Values.GetEnumerator();


                var list = Activator.CreateInstance(listType);
                var invokeMethod = listType.GetMethod(nameof(List<object>.Add));
                while (keyEnumerator.MoveNext() && valuesEnumerator.MoveNext())
                {
                    var item = Activator.CreateInstance(keyValuePair, keyEnumerator.Current, valuesEnumerator.Current);
                    invokeMethod.Invoke(list, new[] { item });
                }

                serializer.Serialize(writer, list);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {

                var attrs = objectType.GetGenericArguments();
                var keyValuePair = typeof(KeyValuePair<,>).MakeGenericType(attrs);
                var listType = typeof(List<>).MakeGenericType(keyValuePair);

                var dict = existingValue as IDictionary;
                dict ??= (IDictionary)Activator.CreateInstance(objectType);

                var keyProp = keyValuePair.GetProperty(nameof(KeyValuePair<object, object>.Key));
                var valueProp = keyValuePair.GetProperty(nameof(KeyValuePair<object, object>.Value));

                var value = serializer.Deserialize(reader, listType) as IEnumerable;
                if (value is null) throw new Exception($"Could not deserialize object with path '{reader.Path}' as IEnumerable.");
                foreach (var item in value)
                {
                    dict.Add(keyProp.GetValue(item), valueProp.GetValue(item));
                }
                return dict;
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return ReflectionUtils.ImplementsGenericDefinition(objectType, typeof(IDictionary<,>));
        }
    }

}
