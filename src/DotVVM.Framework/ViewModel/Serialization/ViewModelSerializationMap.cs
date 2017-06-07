using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Performs the JSON serialization for specified type.
    /// </summary>
    public class ViewModelSerializationMap
    {
        public delegate void ReaderDelegate(JsonReader reader, JsonSerializer serializer, object value, EncryptedValuesReader encryptedValuesReader);
        public delegate void WriterDelegate(JsonWriter writer, object obj, JsonSerializer serializer, EncryptedValuesWriter evWriter, bool isPostback);

        private const string CLIENT_EXTENDERS_KEY = "clientExtenders";

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

        public void ResetFunctions()
        {
            readerFactory = null;
            writerFactory = null;
        }

        private ReaderDelegate readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public ReaderDelegate ReaderFactory => readerFactory ?? (readerFactory = CreateReaderFactory());
        private WriterDelegate writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public WriterDelegate WriterFactory => writerFactory ?? (writerFactory = CreateWriterFactory());
        private Func<object> constructorFactory;
        /// <summary>
        /// Gets the constructor factory.
        /// </summary>
        public Func<object> ConstructorFactory => constructorFactory ?? (constructorFactory = CreateConstructorFactory());
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
        public ReaderDelegate CreateReaderFactory()
        {
            var block = new List<Expression>();
            var reader = Expression.Parameter(typeof(JsonReader), "reader");
            var serializer = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var valueParam = Expression.Parameter(typeof(object), "valueParam");
            var encryptedValuesReader = Expression.Parameter(typeof(EncryptedValuesReader), "encryptedValuesReader");
            var value = Expression.Variable(Type, "value");
            var currentProperty = Expression.Variable(typeof(string), "currentProperty");

            // add current object to encrypted values, this is needed because one property can potentionaly contain more objects (is a collection)
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));

            // value = new {Type}();
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            block.Add(ExpressionUtils.Replace((JsonReader rdr) => rdr.TokenType == JsonToken.StartObject ? rdr.Read() : ExpressionUtils.Stub.Throw<bool>(new Exception($"TokenType = StartObject was expected.")), reader));

            var propertiesSwitch = new List<SwitchCase>();

            // go through all properties that should be read
            var encryptedPropertyIndex = 0;
            foreach (var property in Properties.Where(p => p.TransferToServer && p.PropertyInfo.SetMethod != null)) {
                if (property.ViewModelProtection == ProtectMode.EncryptData || property.ViewModelProtection == ProtectMode.SignData) {
                    // encryptedValues[(int)jobj["{p.Name}"]]
                    block.Add(Expression.Call(
                        value,
                        property.PropertyInfo.SetMethod,
                        Expression.Convert(
                            ExpressionUtils.Replace(
                                (JsonSerializer s, EncryptedValuesReader ev, object existing) => Deserialize(s, ev.ReadValue(encryptedPropertyIndex).CreateReader(), property, existing),
                                serializer, encryptedValuesReader,
                                    Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object))),
                            property.Type)
                        ).OptimizeConstants());
                    encryptedPropertyIndex++;
                } else {
                    var propertyblock = new List<Expression>();
                    var checkEV = property.TransferAfterPostback && property.TransferFirstRequest && ShouldCheckEncrypedValues(property.Type);
                    if (checkEV) {
                        // lastEncrypedValuesCount = encrypedValues.Count
                        propertyblock.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes, Expression.Constant(encryptedPropertyIndex)));
                    }

                    // if ({jsonProp} != null) value.{p.Name} = deserialize();
                    propertyblock.Add(
                        Expression.Call(
                        value,
                        property.PropertyInfo.SetMethod,
                        Expression.Convert(
                            ExpressionUtils.Replace((JsonSerializer s, JsonReader j, object existingValue) =>
                                Deserialize(s, j, property, existingValue),
                                serializer, reader,
                                property.Populate ?
                                    (Expression)Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object)) :
                                    Expression.Constant(null, typeof(object))),
                            property.Type)
                    ));

                    propertyblock.Add(
                        Expression.Call(reader, "Read", Type.EmptyTypes));

                    if (checkEV) {
                        propertyblock.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));
                        encryptedPropertyIndex++;
                    }

                    propertiesSwitch.Add(Expression.SwitchCase(
                        Expression.Block(typeof(void), propertyblock),
                        Expression.Constant(property.Name)
                    ));
                }
            }

            block.Add(ExpressionUtils.While(
                ExpressionUtils.Replace((JsonReader rdr, string val) => rdr.TokenType == JsonToken.PropertyName &&
                                                                        ExpressionUtils.Stub.Assign(val, rdr.Value as string) != null &&
                                                                        rdr.Read(), reader, currentProperty),
                ExpressionUtils.Switch(currentProperty,
                    Expression.Block(typeof(void),
                        Expression.IfThen(
                            ExpressionUtils.Replace((JsonReader rdr) => rdr.TokenType == JsonToken.StartArray || rdr.TokenType == JsonToken.StartConstructor || rdr.TokenType == JsonToken.StartObject, reader),
                            Expression.Call(reader, "Skip", Type.EmptyTypes)),
                        Expression.Call(reader, "Read", Type.EmptyTypes)),
                    propertiesSwitch.ToArray())
                ));

            // close encrypted values
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));

            block.Add(value);


            // build the lambda expression
            var ex = Expression.Lambda<ReaderDelegate>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value, currentProperty }, block),
                    typeof(object)).OptimizeConstants(),
                reader, serializer, valueParam, encryptedValuesReader);
            return ex.Compile();
            //return null;
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
            else if (existingValue != null && property.Populate)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    serializer.Converters.OfType<ViewModelJsonConverter>().First().Populate(reader, serializer, existingValue);
                    return existingValue;
                }
                else
                {
                    serializer.Populate(reader, existingValue);
                    return existingValue;
                }
            }
            else
            {
                if (property.Type.GetTypeInfo().IsValueType && reader.TokenType == JTokenType.Null)
                {
                    return Activator.CreateInstance(property.Type);
                }
                else
                {
                    return serializer.Deserialize(reader, property.Type);
                }
            }
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public WriterDelegate CreateWriterFactory()
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
            //block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName("$validationErrors"), writer));
            //block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WriteStartArray(), writer));
            //block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WriteEndArray(), writer));

            // writer.WritePropertyName("$type");
            // serializer.Serialize(writer, value.GetType().FullName)
            block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName("$type"), writer));
            block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, string t) => s.Serialize(w, t), serializer, writer, Expression.Constant(Type.GetTypeHash())));

            // go through all properties that should be serialized
            foreach (var property in Properties)
            {
                var endPropertyLabel = Expression.Label("end_property_" + property.Name);
                var options = new Dictionary<string, object>();
                if (property.TransferToClient && property.PropertyInfo.GetMethod != null)
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

                    if (property.ViewModelProtection == ProtectMode.EncryptData ||
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
                
                if (property.ClientExtenders.Any())
                {
                    options[CLIENT_EXTENDERS_KEY] = property.ClientExtenders.ToArray();
                }

                AddTypeOptions(options, property);
                
                block.Add(Expression.Label(endPropertyLabel));
                if (options.Any())
                {
                    GenerateOptionsBlock(block, property, options, writer);
                }
            }

            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));
            // compile the expression
            var ex = Expression.Lambda<WriterDelegate>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValuesWriter, isPostback);
            return ex.Compile();
        }

        private void GenerateOptionsBlock(IList<Expression> block,  ViewModelPropertyMap property, Dictionary<string, object> options, ParameterExpression writer)
        {
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName(property.Name + "$options"), writer));
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartObject(), writer));
            foreach (var option in options)
            {
                block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName(option.Key), writer));
                switch (option.Key)
                {
                    case CLIENT_EXTENDERS_KEY:
                    {
                        // declare 'clientExtenders' as the array of objects
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartArray(), writer));
                        foreach (var extender in property.ClientExtenders)
                        {
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteStartObject(), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName("name"), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(extender.Name), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WritePropertyName("parameter"), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(extender.Parameter), writer));
                            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

                        }
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndArray(), writer));
                        break;
                    }
                    default:
                        // legacy code - direct { property : value }
                        block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteValue(option.Value), writer));
                        break;
                }
            }
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));
        }

        private void AddTypeOptions(Dictionary<string, object> options, ViewModelPropertyMap property)
        {
            if (property.TransferToClient || property.TransferToServer)
            {
                if ((property.Type == typeof(DateTime) || property.Type == typeof(DateTime?)) && property.JsonConverter == null) // TODO: allow customization using attributes
                {
                    options["isDate"] = true;
                }
                else if (property.Type.IsNumericType())
                {
                    options["type"] = property.Type.Name.ToLower();
                }
                else if (Nullable.GetUnderlyingType(property.Type)?.IsNumericType() == true)
                {
                    options["type"] = Nullable.GetUnderlyingType(property.Type).Name.ToLower() + "?";
                }
            }
        }

        /// <summary>
        /// Determines whether type can contain encrypted fields
        /// </summary>
        private bool ShouldCheckEncrypedValues(Type type)
        {
            return !(
                // primitives can't contain encrypted fields
                type.GetTypeInfo().IsPrimitive ||
                type.GetTypeInfo().IsEnum ||
                type == typeof(string)
           );
        }

    }

}
