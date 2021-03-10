using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// This converter serializes Dictionary&lt;&gt; as List&lt;KeyValuePair&lt;,&gt;&gt; in order to make dictionaries work with knockout. 
    /// </summary>
    public class DotvvmDictionaryConverter : JsonConverter
    {
        private static Type keyValuePairGenericType = typeof(KeyValuePair<,>);
        private static Type listGenericType = typeof(List<>);
        private static Type dictionaryEntryType = typeof(DictionaryEntry);
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = value as IDictionary;
            if (dict == null)
            {
                writer.WriteNull();
            }
            else
            {
                var attrs = value.GetType().GetGenericArguments();
                var keyValuePair = keyValuePairGenericType.MakeGenericType(attrs);
                var listType = listGenericType.MakeGenericType(keyValuePair);

                var itemEnumerator = dict.GetEnumerator();

                var keyProp = dictionaryEntryType.GetProperty(nameof(DictionaryEntry.Key));
                var valueProp = dictionaryEntryType.GetProperty(nameof(DictionaryEntry.Value));

                var list = Activator.CreateInstance(listType);
                var invokeMethod = listType.GetMethod(nameof(List<object>.Add));
                while (itemEnumerator.MoveNext())
                {
                    var item = Activator.CreateInstance(keyValuePair, keyProp.GetValue(itemEnumerator.Current), valueProp.GetValue(itemEnumerator.Current));
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
                var keyValuePair = keyValuePairGenericType.MakeGenericType(attrs);
                var listType = listGenericType.MakeGenericType(keyValuePair);

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
            return typeof(IDictionary).IsAssignableFrom(objectType)
                && ReflectionUtils.ImplementsGenericDefinition(objectType, typeof(IDictionary<,>));

        }
    }

}
