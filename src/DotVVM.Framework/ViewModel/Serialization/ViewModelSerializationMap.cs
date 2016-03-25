using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
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


        private Action<JObject, JsonSerializer, object, EncryptedValuesReader> readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public Action<JObject, JsonSerializer, object, EncryptedValuesReader> ReaderFactory
        {
            get { return readerFactory ?? (readerFactory = CreateReaderFactory()); }
        }

        private Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool> writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool> WriterFactory
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
        public Action<JObject, JsonSerializer, object, EncryptedValuesReader> CreateReaderFactory()
        {
            var block = new List<Expression>();
            var jobj = Expression.Parameter(typeof(JObject), "jobj");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var encryptedValuesReader = Expression.Parameter(typeof(EncryptedValuesReader), "encryptedValuesReader");
            var value = Expression.Variable(Type, "value");

            // add current object to encrypted values, this is needed because one property can potentionaly contain more objects (is a collection)
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));

            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // go through all properties that should be read
            foreach (var property in Properties.Where(p => p.TransferToServer))
            {
                if (property.ViewModelProtection == ProtectMode.EnryptData || property.ViewModelProtection == ProtectMode.SignData)
                {
                    // encryptedValues[(int)jobj["{p.Name}"]]

                    block.Add(Expression.Call(
                        value,
                        property.PropertyInfo.SetMethod,
                        Expression.Convert(
                            ExpressionUtils.Replace(
                                (JsonSerializer s, EncryptedValuesReader ev, object existing) => Deserialize(s, ev.ReadValue(), property, existing),
                                serializer, encryptedValuesReader,
                                    Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object))),
                            property.Type)
                        ));
                }
                else
                {
                    var checkEV = property.TransferAfterPostback && property.TransferFirstRequest && ShouldCheckEncrypedValues(property.Type);
                    if (checkEV)
                    {
                        // lastEncrypedValuesCount = encrypedValues.Count
                        block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));
                    }

                    var jsonProp = ExpressionUtils.Replace((JObject j) => j[property.Name], jobj);

                    // if ({jsonProp} != null) value.{p.Name} = deserialize();
                    block.Add(
                        Expression.IfThen(Expression.NotEqual(jsonProp, Expression.Constant(null)),
                            Expression.Call(
                            value,
                            property.PropertyInfo.SetMethod,
                            Expression.Convert(
                                ExpressionUtils.Replace((JsonSerializer s, JObject j, object existingValue) =>
                                    Deserialize(s, j[property.Name], property, existingValue),
                                    serializer, jobj,
                                    Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object))),
                                property.Type)
                    )));

                    if (checkEV)
                    {
                        block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));
                    }
                }
            }

            // close encrypted values
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));

            block.Add(value);


            // build the lambda expression
            var ex = Expression.Lambda<Action<JObject, JsonSerializer, object, EncryptedValuesReader>>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value }, block),
                    typeof(object)).OptimizeConstants(),
                jobj, serializer, valueParam, encryptedValuesReader);
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

        private static object Deserialize(JsonSerializer serializer, JToken jtoken, ViewModelPropertyMap property, object existingValue)
        {
            if (property.JsonConverter != null && property.JsonConverter.CanRead && property.JsonConverter.CanConvert(property.Type))
            {
                return property.JsonConverter.ReadJson(jtoken.CreateReader(), property.Type, existingValue, serializer);
            }
            else if (existingValue != null && property.Populate)
            {
                if (jtoken.Type == JTokenType.Null)
                    return null;
                else if (jtoken.Type == JTokenType.Object)
                {
                    serializer.Converters.OfType<ViewModelJsonConverter>().First().Populate((JObject)jtoken, serializer, existingValue);
                    return existingValue;
                }
                else
                {
                    serializer.Populate(jtoken.CreateReader(), existingValue);
                    return existingValue;
                }
            }
            else
            {
                return serializer.Deserialize(jtoken.CreateReader(), property.Type);
            }
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Expression.Parameter(typeof(JsonWriter), "writer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var encryptedValuesWriter = Expression.Parameter(typeof(EncryptedValuesWriter), "encryptedValuesWriter");
            var isPostback = Expression.Parameter(typeof(bool), "isPostback");
            var value = Expression.Variable(Type, "value");

            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));
            block.Add(Expression.Call(writer, "WriteStartObject", Type.EmptyTypes));

            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes));

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
                var endPropertyLabel = Expression.Label("end_property_" + property.Name);
                var options = new Dictionary<string, object>();
                if (property.TransferToClient)
                {
                    if (property.TransferFirstRequest != property.TransferAfterPostback)
                    {
                        if (property.ViewModelProtection != ProtectMode.None) throw new Exception("Property sent only on selected requests can use viewModel protection.");

                        Expression condition = isPostback;
                        if (property.TransferAfterPostback) condition = Expression.Not(condition);
                        block.Add(Expression.IfThen(condition, Expression.Goto(endPropertyLabel)));
                    }

                    // writer.WritePropertyName("{property.Name"});
                    var prop = Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object));

                    if (property.ViewModelProtection == ProtectMode.EnryptData ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        // encryptedValues.Add(JsonConvert.SerializeObject({value}));
                        block.Add(
                            Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Value), Type.EmptyTypes, prop));
                    }

                    if (property.ViewModelProtection == ProtectMode.None ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        var checkEV = property.ViewModelProtection == ProtectMode.None &&
                                           ShouldCheckEncrypedValues(property.Type);
                        if (checkEV)
                        {
                            // lastEncrypedValuesCount = encrypedValues.Count
                            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes));
                        }

                        block.Add(Expression.Call(writer, "WritePropertyName", Type.EmptyTypes,
                            Expression.Constant(property.Name)));

                        // serializer.Serialize(writer, value.{property.Name});
                        block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, object v) => Serialize(s, w, property, v), serializer, writer, prop));

                        if (checkEV)
                        {
                            // if not fully transported, ensure nothing happened
                            if (property.TransferAfterPostback != property.TransferFirstRequest && !property.TransferToServer)
                            {
                                block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.ClearEmptyNest), Type.EmptyTypes));
                            }
                            else
                            {
                                block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));
                            }
                        }
                    }

                    if (!property.TransferToServer)
                    {
                        // write an instruction into a viewmodel that the property may not be posted back
                        options["doNotPost"] = true;
                    }
                    else if (property.TransferToServerOnlyInPath)
                    {
                        options["pathOnly"] = true;
                    }
                }
                else if (property.TransferToServer)
                {
                    // render empty property options - we need to create the observable on the client, however we don't transfer the value
                    options["doNotUpdate"] = true;
                }

                if ((property.Type == typeof(DateTime) || property.Type == typeof(DateTime?)) && property.JsonConverter == null)      // TODO: allow customization using attributes
                {
                    options["isDate"] = true;
                }

                AddTypeOptions(options, property.Type);
                
                block.Add(Expression.Label(endPropertyLabel));
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
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));
            // compile the expression
            var ex = Expression.Lambda<Action<JsonWriter, object, JsonSerializer, EncryptedValuesWriter, bool>>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValuesWriter, isPostback);
            return ex.Compile();
        }

        private void AddTypeOptions(Dictionary<string, object> options, Type type)
        {
            if (type.IsNumericType())
            {
                options["type"] = type.Name.ToLower();
            }
            else if (Nullable.GetUnderlyingType(type)?.IsNumericType() == true)
            {
                options["type"] = Nullable.GetUnderlyingType(type).Name.ToLower();
            }
        }

        private static readonly string DotvvmAssemblyName = typeof(ViewModelSerializationMap).Assembly.FullName;
        /// <summary>
        /// Determines whether type can contain encrypted fields
        /// </summary>
        private bool ShouldCheckEncrypedValues(Type type)
        {
            return !(
                // primitives can't contain encrypted fields
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string)
           );
        }

    }

}
