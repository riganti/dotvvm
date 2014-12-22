using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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


        private Action<JObject, JsonSerializer, object> readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object> ReaderFactory
        {
            get { return readerFactory ?? (readerFactory = CreateReaderFactory()); }
        }

        private Action<JsonWriter, object, JsonSerializer> writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer> WriterFactory
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
        public Action<JObject, JsonSerializer, object> CreateReaderFactory()
        {
            var block = new List<Expression>();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var value = Expression.Variable(Type, "value");

            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties
            foreach (var property in Properties.Where(p => p.TransferToServer))
            {
                // jobj["{property.Name}"]
                var jsonProp = Expression.Property(jobj, 
                            typeof(JObject).GetProperty("Item", typeof(JObject), new[] { typeof(string) }),
                            Expression.Constant(property.Name));
                // jobj["{property.Name}"].CreateReader();
                var propReader = Expression.Call(jsonProp, "CreateReader", Type.EmptyTypes);

                // handle serialized properties
                Expression callDeserialize;
                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData)
                {
                    // ViewModelProtectionHelper.DecryptAndDeserialize(jobj["{p.Name}"].CreateReader().ReadAsString(), {p.Type})
                    callDeserialize = Expression.Call(
                        typeof (ViewModelProtectionHelper).GetMethod("DecryptAndDeserialize"),
                        Expression.Call(propReader, "ReadAsString", Type.EmptyTypes),
                        Expression.Constant(property.Type));
                }
                else
                {
                    callDeserialize = Expression.Call(serializer,
                        typeof (JsonSerializer).GetMethod("Deserialize", new[] { typeof (JsonReader), typeof (Type) }),
                        propReader, Expression.Constant(property.Type));
                }

                // if (jobj["{p.Name}"] != null) value.{p.Name} = deserialize();
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
                    block.Add(Expression.Call(typeof(ViewModelProtectionHelper).GetMethod("VerifyHmacSignature"),
                        Expression.Property(value, property.Name),
                        Expression.Convert(Expression.Property(jobj,
                            typeof(JObject).GetProperty("Item", typeof(JObject), new[] { typeof(string) }),
                            Expression.Constant(property.Name + "$mac")), typeof(string))

                    ));
                }
            }

            block.Add(value);

            // build the lambda expression
            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object>>(Expression.Convert(Expression.Block(Type, new[] { value }, block), typeof(object)), jobj, serializer, valueParam);
            return ex.Compile();
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var value = Expression.Variable(Type, "value");

            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));
            
            // go through all properties that should be serialized
            foreach (var property in Properties.Where(map => map.TransferToClient))
            {
                // writer.WritePropertyName("{property.Name"});
                block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name)));

                var prop = Expression.Convert(Expression.Property(value, property.Name), typeof(object));

                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData)
                    // writer.WriteValue(ViewModelProtectionHelper.SerializeAndEncrypt(value.{property.Name}));
                    block.Add(Expression.Call(writer, 
                        typeof(JsonWriter).GetMethod("WriteValue", new[] { typeof(string) }), 
                        Expression.Call(typeof(ViewModelProtectionHelper).GetMethod("SerializeAndEncrypt"), prop)));
                else
                    // serializer.Serialize(writer, value.{property.Name});
                    block.Add(Expression.Call(serializer, "Serialize", Type.EmptyTypes, writer, prop));

                // add the signature if needed
                if (property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name + "$mac")));
                    block.Add(Expression.Call(writer,
                        typeof(JsonWriter).GetMethod("WriteValue", new[] { typeof(string) }),
                        Expression.Call(typeof(ViewModelProtectionHelper).GetMethod("CalculateHmacSignature"),
                        prop)));
                }
            }

            block.Add(Expression.Call(writer, "WriteEndObject", Type.EmptyTypes));

            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer>>(Expression.Block(new[] { value }, block), writer, valueParam, serializer);
            return ex.Compile();
        }

    }
}
