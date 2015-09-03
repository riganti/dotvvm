using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.ViewModel
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
            var lastEVcount = Expression.Variable(typeof(int), "lastEncrypedValuesCount");


            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties that should be read
            foreach (var property in Properties.Where(p => p.TransferToServer))
            {
                if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData || property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                {
#if DEBUG
                    block.Add(ExpressionUtils.Replace((int levc, JArray ev) =>
                        System.Diagnostics.Debug.WriteLine("ev read " + ev.First.ToString(Formatting.None) + ": " + property.Name + " in " + Type.Name), lastEVcount, encryptedValues));
#endif
                    var callDeserialize = ExpressionUtils.Replace(
                        (JsonSerializer s, JArray ev, JObject j) => Deserialize(s, GetAndRemove(ev, 0).CreateReader(), property, j),
                        serializer, encryptedValues, jobj);
                    // encryptedValues[(int)jobj["{p.Name}"]]

                    block.Add(Expression.Call(
                        value,
                        Type.GetProperty(property.Name).SetMethod,
                        Expression.Convert(callDeserialize, property.Type)));
                }
                else
                {
                    var checkEVCount = property.TransferToClient && ShouldCheckEncrypedValueCount(property.Type);
                    if (checkEVCount)
                    {
                        // lastEncrypedValuesCount = encrypedValues.Count
                        block.Add(Expression.Assign(lastEVcount, Expression.Property(encryptedValues, "Count")));
                    }

                    var jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name], jobj);
                    var callDeserialize = ExpressionUtils.Replace((JsonSerializer s, JObject j) =>
                        Deserialize(s, j[property.Name].CreateReader(), property, j), serializer, jobj);

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
#if DEBUG
                        block.Add(ExpressionUtils.Replace((int levc, JArray ev) =>
                            System.Diagnostics.Debug.WriteLine("ev checksum expected " + (levc - ev.Count).ToString() + ", actual " + ev.First.ToString() + ": " + property.Name + " in " + Type.Name), lastEVcount, encryptedValues));
#endif
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

        private static void Serialize(JsonSerializer serializer, JsonWriter writer, ViewModelPropertyMap property, object value)
        {
            if (property.JsonConverter != null && property.JsonConverter.CanWrite && property.JsonConverter.CanConvert(property.Type))
            {
                property.JsonConverter.WriteJson(writer, value, serializer);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        private static object Deserialize(JsonSerializer serializer, JsonReader reader, ViewModelPropertyMap property, object existingValue)
        {
            if (property.JsonConverter != null && property.JsonConverter.CanRead && property.JsonConverter.CanConvert(property.Type))
            {
                return property.JsonConverter.ReadJson(reader, property.Type, existingValue, serializer);
            }
            else
            {
                return serializer.Deserialize(reader, property.Type);
            }
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
            var lastEVcount = Expression.Variable(typeof(int), "lastEncrypedValuesCount");

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
            foreach (var property in Properties)
            {
                var options = new Dictionary<string, object>();

                if (property.TransferToClient)
                {
                    // writer.WritePropertyName("{property.Name"});
                    var prop = Expression.Convert(Expression.Property(value, property.Name), typeof (object));

                    if (property.ViewModelProtection == ViewModelProtectionSettings.EnryptData ||
                        property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                    {
                        // encryptedValues.Add(JsonConvert.SerializeObject({value}));
#if DEBUG
                        block.Add(ExpressionUtils.Replace((JArray ev) =>
                            System.Diagnostics.Debug.WriteLine("ev[" + ev.Count + "]: " + property.Name + " in " +
                                                               Type.Name), encryptedValues));
#endif
                        block.Add(
                            ExpressionUtils.Replace(
                                (JArray ev, object p) => ev.Add(p != null ? JToken.FromObject(p) : JValue.CreateNull()),
                                encryptedValues, prop));
                    }

                    if (property.ViewModelProtection == ViewModelProtectionSettings.None ||
                        property.ViewModelProtection == ViewModelProtectionSettings.SignData)
                    {
                        var checkEVCount = property.ViewModelProtection == ViewModelProtectionSettings.None &&
                                           property.TransferToServer && ShouldCheckEncrypedValueCount(property.Type);
                        if (checkEVCount)
                        {
                            // lastEncrypedValuesCount = encrypedValues.Count
                            block.Add(Expression.Assign(lastEVcount, Expression.Property(encryptedValues, "Count")));
                        }

                        block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes,
                            Expression.Constant(property.Name)));

                        // serializer.Serialize(writer, value.{property.Name});
                        block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, object v) => Serialize(s, w, property, v), serializer, writer, prop));
                        
                        if (checkEVCount)
                        {
                            // encryptedValues.Add(encryptedValues.Count - lastEVcount)
#if DEBUG
                            block.Add(ExpressionUtils.Replace((JArray ev) =>
                                System.Diagnostics.Debug.WriteLine("ev[" + ev.Count + "]: checksum of " + property.Name +
                                                                   " in " + Type.Name), encryptedValues));
#endif
                            block.Add(ExpressionUtils.Replace((int lastC, JArray ev) =>
                                ev.Add(ev.Count - lastC),
                                lastEVcount, encryptedValues));
                        }
                    }

                    if (!property.TransferToServer)
                    {
                        // write an instruction into a viewmodel that the property may not be posted back
                        options["doNotPost"] = true;
                    }
                    else if(property.TransferToServerOnlyInPath)
                    {
                        options["pathOnly"] = true;
                    }
                }
                else if (property.TransferToServer)
                {
                    // render empty property options - we need to create the observable on the client, however we don't transfer the value
                    options["doNotUpdate"] = true;
                }

                if ((property.Type == typeof (DateTime) || property.Type == typeof (DateTime?)) && property.JsonConverter == null)      // TODO: allow customization using attributes
                {
                    options["isDate"] = true;
                }

                if (options.Any())
                {
                    block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName(property.Name + "$options"), writer));
                    block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartObject(), writer));
                    foreach (var option in options)
                    {
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName(option.Key), writer));
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(option.Value), writer));
                    }
                    block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));
                }
            }

            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, JArray, HashSet<ViewModelSerializationMap>, ViewModelSerializationMap>>(
                Expression.Block(new[] { value, lastEVcount }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValues, usedTypes, serializationMap);
            return ex.Compile();
        }

        private static readonly string DotvvmAssemblyName = typeof(ViewModelSerializationMap).Assembly.FullName;
        /// <summary>
        /// Determines whether type can contain encrypted fields
        /// </summary>
        private bool ShouldCheckEncrypedValueCount(Type type)
        {
            return !(
                // primitives can't contain encrypted fields
                type.IsPrimitive ||
                type == typeof(string) ||
                // types in assemblies than don't reference dotvvm also can't contain encryped values (as long as generic arguments also met the conditions)
                (type.Assembly.GetReferencedAssemblies().All(a => a.FullName != DotvvmAssemblyName) &&
                    !type.GenericTypeArguments.Any(ShouldCheckEncrypedValueCount))
           );
        }

    }
}
