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
            var lastEVcount = Expression.Variable(typeof(int), "lastEncrypedValuesCount");


            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties that should be read
            foreach (var property in Properties.Where(p => p.TransferToServer))
            {
                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    var callDeserialize = ExpressionUtils.Replace(
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
                    var checkEVCount = ShouldCheckEncrypedValueCount(property.Type);
                    if (checkEVCount)
                    {
                        // lastEncrypedValuesCount = encrypedValues.Count
                        block.Add(Expression.Assign(lastEVcount, Expression.Property(encryptedValues, "Count")));
                    }

                    var jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name], jobj);
                    var callDeserialize = ExpressionUtils.Replace((JsonSerializer s, JObject j) =>
                        s.Deserialize(j[property.Name].CreateReader(), property.Type), serializer, jobj);

                    // if ({jsonProp} != null) value.{p.Name} = deserialize();
                    block.Add(
                        Expression.IfThen(Expression.NotEqual(jsonProp, Expression.Constant(null)),
                            Expression.Call(
                            value,
                            Type.GetProperty(property.Name).SetMethod,
                            Expression.Convert(callDeserialize, property.Type)
                    )));

                    if (checkEVCount)
                    {
                        block.Add(Expression.IfThen(
                            ExpressionUtils.Replace((int levc, JArray ev) =>
                                levc - ev.Count != (int)GetAndRemove(ev, 0), lastEVcount, encryptedValues),
                            Expression.Throw(Expression.New(typeof(System.Security.SecurityException)))
                        ));
                    }
                }
            }

            block.Add(value);

            // build the lambda expression
            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object, JArray>>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value, lastEVcount }, block),
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
        public Action<JsonWriter, object, JsonSerializer, JArray> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var encryptedValues = Expression.Parameter(typeof(JArray), "encryptedValues");
            var value = Expression.Variable(Type, "value");
            var lastEVcount = Expression.Variable(typeof(int), "lastEncrypedValuesCount");

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
                    // encryptedValues.Add(JsonConvert.SerializeObject({value}));
                    block.Add(ExpressionUtils.Replace((JArray ev, object p) => ev.Add(JToken.FromObject(p)), encryptedValues, prop));
                }

                if (property.ViewModelProtection == ViewModelProtectionSettings.None || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
                    var checkEVCount = ShouldCheckEncrypedValueCount(property.Type);
                    if (checkEVCount)
                    {
                        // lastEncrypedValuesCount = encrypedValues.Count
                        block.Add(Expression.Assign(lastEVcount, Expression.Property(encryptedValues, "Count")));
                    }

                    block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes, Expression.Constant(property.Name)));

                    // serializer.Serialize(writer, value.{property.Name});
                    block.Add(Expression.Call(serializer, "Serialize", Type.EmptyTypes, writer, prop));

                    if (checkEVCount)
                    {
                        // encryptedValues.Add(encryptedValues.Count - lastEVcount)
                        block.Add(ExpressionUtils.Replace((int lastC, JArray ev) =>
                            ev.Add(ev.Count - lastC),
                            lastEVcount, encryptedValues));
                    }
                }
            }

            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, JArray>>(
                Expression.Block(new[] { value, lastEVcount }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValues);
            return ex.Compile();
        }

        private static readonly string RedwoodAssemblyName = typeof(ViewModelSerializationMap).Assembly.FullName;
        private bool ShouldCheckEncrypedValueCount(Type type)
        {
            return !(
                type.IsPrimitive ||
                type == typeof(string) ||
                (typeof(IEnumerable<>).IsAssignableFrom(type) && ShouldCheckEncrypedValueCount(type.GenericTypeArguments[0])) ||
                (type.Assembly.GetReferencedAssemblies().All(a => a.FullName != RedwoodAssemblyName) &&
                    !type.GenericTypeArguments.Any(ShouldCheckEncrypedValueCount))
           );
        }

    }
}
