using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.ViewModel
{
    /// <summary>
    /// Performs the JSON serialization for specified type.
    /// </summary>
    public class ViewModelSerializationMap
    {
        /// <summary>
        /// Gets or sets the object type for this serialization map.
        /// </summary>
        public Type Type { get; private set; }

        public IEnumerable<ViewModelPropertyMap> Properties { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelSerializationMap"/> class.
        /// </summary>
        public ViewModelSerializationMap(Type type, IEnumerable<ViewModelPropertyMap> properties)
        {
            Type = type;
            Properties = properties.ToList();
        }


        private Action<JObject, JsonSerializer, object, JArray> readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object, JArray> ReaderFactory
        {
            get { return readerFactory ?? (readerFactory = CreateReaderFactory()); }
        }

        private Action<JsonWriter, object, JsonSerializer, JArray, HashSet<ViewModelSerializationMap>, ViewModelSerializationMap> writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, JArray, HashSet<ViewModelSerializationMap>, ViewModelSerializationMap> WriterFactory
        {
            get { return writerFactory ?? (writerFactory = CreateWriterFactory()); }
        }

        private Func<object> constructorFactory;
        /// <summary>
        /// Gets the constructor factory.
        /// </summary>
        public Func<object> ConstructorFactory
        {
            get { return constructorFactory ?? (constructorFactory = CreateConstructorFactory()); }
        }

        /// <summary>
        /// Creates the constructor for this object.
        /// </summary>
        public Func<object> CreateConstructorFactory()
        {
            var ex = Expression.Lambda<Func<object>>(Expression.New(Type));
            return ex.Compile();
        }

        /// <summary>
        /// Creates the reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object, JArray> CreateReaderFactory()
        {
            var block = new List<Expression>();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var encryptedValues = Expression.Parameter(typeof(JArray), "encryptedValues");
            var value = Expression.Variable(Type, "value");

            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties that should be read
            foreach (var property in Properties.Where(p => p.TransferToServer))
            {
                Expression jsonProp = null;

                // handle serialized properties
                Expression callDeserialize;
                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    callDeserialize = ExpressionUtils.Replace(
                        (JsonSerializer s, JArray ev, JObject j) => s.Deserialize(GetAndRemove(ev, 0).CreateReader(), property.Type),
                        serializer, encryptedValues, jobj);
                    // encryptedValues[(int)jobj["{p.Name}"]]

                    block.Add(Expression.Call(
                        value,
                        Type.GetProperty(property.Name).SetMethod,
                        Expression.Convert(callDeserialize, property.Type)));
                }
                else
                {
                    jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name], jobj);
                    callDeserialize = ExpressionUtils.Replace((JsonSerializer s, JObject j) =>
                        s.Deserialize(j[property.Name].CreateReader(), property.Type), serializer, jobj);

                    // if ({jsonProp} != null) value.{p.Name} = deserialize();
                    block.Add(
                        Expression.IfThen(Expression.NotEqual(jsonProp, Expression.Constant(null)),
                            Expression.Call(
                            value,
                            Type.GetProperty(property.Name).SetMethod,
                            Expression.Convert(callDeserialize, property.Type)
                    )));
                }
            }

            block.Add(value);

            // build the lambda expression
            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object, JArray>>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value }, block),
                    typeof(object)).OptimizeConstants(),
                jobj, serializer, valueParam, encryptedValues);
            return ex.Compile();
        }

        private JToken GetAndRemove(JArray array, int index)
        {
            var value = array[index];
            array.RemoveAt(index);
            return value;
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, JArray, HashSet<ViewModelSerializationMap>, ViewModelSerializationMap> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var encryptedValues = Expression.Parameter(typeof(JArray), "encryptedValues");
            var usedTypes = Expression.Parameter(typeof(HashSet<ViewModelSerializationMap>), "usedTypes");
            var serializationMap = Expression.Parameter(typeof(ViewModelSerializationMap), "serializationMap");
            var value = Expression.Variable(Type, "value");

            // usedMaps.Add(serializationMap);
            block.Add(ExpressionUtils.Replace((HashSet<ViewModelSerializationMap> ut, ViewModelSerializationMap sm) => ut.Add(sm), usedTypes, serializationMap));
            
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));
            
            // writer.WritePropertyName("$validationErrors")
            // writer.WriteStartArray()
            // writer.WriteEndArray()
            block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName("$validationErrors"), writer));
            block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WriteStartArray(), writer));
            block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WriteEndArray(), writer));

            // writer.WritePropertyName("$type");
            // serializer.Serialize(writer, value.GetType().FullName)
            block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName("$type"), writer));
            block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, string t) => s.Serialize(w, t), serializer, writer, Expression.Constant(Type.FullName)));

            // go through all properties that should be serialized
            foreach (var property in Properties.Where(map => map.TransferToClient))
            {
                // writer.WritePropertyName("{property.Name"});
                var prop = Expression.Convert(Expression.Property(value, property.Name), typeof(object));

                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    // encryptedValues.Add(JsonConvert.SerializeObject({value}));
                    block.Add(ExpressionUtils.Replace((JArray ev, object p) => ev.Add(JToken.FromObject(p)), encryptedValues, prop));
                }

                if (property.ViewModelProtection == ViewModelProtectionSettings.None || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name)));

                    // serializer.Serialize(writer, value.{property.Name});
                    block.Add(Expression.Call(serializer, "Serialize", Type.EmptyTypes, writer, prop));
                }
            }

            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, JArray, HashSet<ViewModelSerializationMap>, ViewModelSerializationMap>>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValues, usedTypes, serializationMap);
            return ex.Compile();
        }

    }
}
