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


        private Action<JObject, JsonSerializer, object, ViewModelProtectionHelper> readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object, ViewModelProtectionHelper> ReaderFactory
        {
            get { return readerFactory ?? (readerFactory = CreateReaderFactory()); }
        }

        private Action<JsonWriter, object, JsonSerializer, ViewModelProtectionHelper> writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, ViewModelProtectionHelper> WriterFactory
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
        public Action<JObject, JsonSerializer, object, ViewModelProtectionHelper> CreateReaderFactory()
        {
            var block = new List<Expression>();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var value = Expression.Variable(Type, "value");
            var protectionHelper = Expression.Parameter(typeof(ViewModelProtectionHelper), "protectionHelper");


            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties that should be read
            foreach (var property in Properties.Where(p => p.TransferToServer))
            {
                Expression jsonProp = null;
                // handle serialized properties
                Expression callDeserialize;
                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData)
                {
                    jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name + "$encrypted"], jobj);
                    callDeserialize = ExpressionUtils.Replace(
                        (ViewModelProtectionHelper ph, JObject j) =>
                        ph.DecryptAndDeserialize((string)j[property.Name + "$encrypted"], property.Type),
                        protectionHelper, jobj);
                    // protectionHelper.DecryptAndDeserialize(jobj["{p.Name}"].CreateReader().ReadAsString(), {p.Type})
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

                // check the signature
                if (property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    // protectionHelper.VerifyHmacSignature(value.{property.Name}, jobj[{property.Name + "$mac"}] as string)
                    block.Add(ExpressionUtils.Replace((ViewModelProtectionHelper ph, object vp, JObject jo) =>
                        ph.VerifyHmacSignature(vp, (string)jo[property.Name + "$mac"]),
                        protectionHelper, Expression.Property(value, property.Name), jobj));
                }
            }

            block.Add(value);

            // build the lambda expression
            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object, ViewModelProtectionHelper>>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value }, block),
                    typeof(object)).OptimizeConstants(),
                jobj, serializer, valueParam, protectionHelper);
            return ex.Compile();
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, ViewModelProtectionHelper> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var value = Expression.Variable(Type, "value");
            var protectionHelper = Expression.Parameter(typeof(ViewModelProtectionHelper), "protectionHelper");

            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));

            // go through all properties that should be serialized
            foreach (var property in Properties.Where(map => map.TransferToClient))
            {
                // writer.WritePropertyName("{property.Name"});

                var prop = Expression.Convert(Expression.Property(value, property.Name), typeof(object));

                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData)
                {
                    block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name + "$encrypted")));
                    // writer.WriteValue(protectionHelper.SerializeAndEncrypt(value.{property.Name}));
                    block.Add(ExpressionUtils.Replace((ViewModelProtectionHelper ph, JsonWriter w, object p) =>
                        w.WriteValue(ph.SerializeAndEncrypt(p)),
                        protectionHelper, writer, prop));
                }
                else
                {
                    block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name)));
                    // serializer.Serialize(writer, value.{property.Name});
                    block.Add(Expression.Call(serializer, "Serialize", Type.EmptyTypes, writer, prop));
                }

                // add the signature if needed
                if (property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    // writer.WritePropertyName({property.Name + "$mac"});
                    block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName(property.Name + "$mac"), writer));

                    // writer.WriteValue(protectionHelper.CalculateHmacSignature({prop});
                    block.Add(ExpressionUtils.Replace((ViewModelProtectionHelper ph, JsonWriter w, object p) =>
                        w.WriteValue(ph.CalculateHmacSignature(p)),
                        protectionHelper, writer, prop));

                }
            }

            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, ViewModelProtectionHelper>>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, protectionHelper);
            return ex.Compile();
        }

    }
}
