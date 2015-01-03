using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        private Action<JsonWriter, object, JsonSerializer, JArray> writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, JArray> WriterFactory
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
                    jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name + "$encrypted"], jobj);
                    callDeserialize = ExpressionUtils.Replace(
                        (JsonSerializer s, JArray ev, JObject j) => s.Deserialize(ev[j[property.Name + "$encrypted"].Value<int>()].CreateReader(), property.Type),
                        serializer, encryptedValues, jobj);
                    // encryptedValues[(int)jobj["{p.Name}"]]
                }
                else
                {
                    jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name], jobj);
                    callDeserialize = ExpressionUtils.Replace((JsonSerializer s, JObject j) =>
                        s.Deserialize(j[property.Name].CreateReader(), property.Type), serializer, jobj);
                }

                // if ({jsonProp} != null) value.{p.Name} = deserialize();
                block.Add(
                    Expression.IfThen(Expression.NotEqual(jsonProp, Expression.Constant(null)),
                        Expression.Call(
                        value,
                        Type.GetProperty(property.Name).SetMethod,
                        Expression.Convert(callDeserialize, property.Type)
                )));
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

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, JArray> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var encryptedValues = Expression.Parameter(typeof(JArray), "encryptedValues");
            var value = Expression.Variable(Type, "value");

            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));

            // go through all properties that should be serialized
            foreach (var property in Properties.Where(map => map.TransferToClient))
            {
                // writer.WritePropertyName("{property.Name"});
                var prop = Expression.Convert(Expression.Property(value, property.Name), typeof(object));

                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name + "$encrypted")));

                    // writer.WriteValue(encryptedValues.Count);
                    block.Add(ExpressionUtils.Replace((JArray ev, JsonWriter w) => w.WriteValue(ev.Count),
                        encryptedValues, writer));

                    // encryptedValues.Add(JsonConvert.SerializeObject({value}));
                    block.Add(ExpressionUtils.Replace((JArray ev, object p) => ev.Add(JToken.FromObject(p)),
                        encryptedValues, prop));
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
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, JArray>>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValues);
            return ex.Compile();
        }

    }
}
