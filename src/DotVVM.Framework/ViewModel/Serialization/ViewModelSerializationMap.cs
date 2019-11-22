using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using System.Reflection;

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
        private Func<IServiceProvider, object> constructorFactory;
        /// <summary>
        /// Gets the constructor factory.
        /// </summary>
        public Func<IServiceProvider, object> ConstructorFactory => constructorFactory ?? (constructorFactory = CreateConstructorFactory());

        public void SetConstructor(Func<IServiceProvider, object> constructor) => constructorFactory = constructor;

        /// <summary>
        /// Creates the constructor for this object.
        /// </summary>
        public Func<IServiceProvider, object> CreateConstructorFactory()
        {
            var ex = Expression.Lambda<Func<IServiceProvider, object>>(Expression.New(Type), new [] { Expression.Parameter(typeof(IServiceProvider)) });
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

            // add current object to encrypted values, this is needed because one property can potentially contain more objects (is a collection)
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));

            // curly brackets are used for variables and methods from the context of this factory method
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // if the reader is in an invalid state, throw an exception
            // TODO: Change exception type, just Exception is not exactly descriptive
            block.Add(ExpressionUtils.Replace((JsonReader rdr) => rdr.TokenType == JsonToken.StartObject ? rdr.Read() : ExpressionUtils.Stub.Throw<bool>(new Exception($"TokenType = StartObject was expected.")), reader));

            var propertiesSwitch = new List<SwitchCase>();

            // iterate through all properties even if they're gonna get skipped
            // it's important for the index to count with all the properties that viewModel contains because the client will send some of them only sometimes
            for (int propertyIndex = 0; propertyIndex < Properties.Count(); propertyIndex++)
            {
                var property = Properties.ElementAt(propertyIndex);
                if (!property.TransferToServer || property.PropertyInfo.SetMethod == null)
                {
                    continue;
                }

                if (property.ViewModelProtection == ProtectMode.EncryptData || property.ViewModelProtection == ProtectMode.SignData)
                {
                    // value.{property} = ({property.Type})Deserialize(serializer, encryptedValuesReader.ReadValue({propertyIndex}), {property}, (object)value.{PropertyInfo});
                    block.Add(Expression.Call(
                        value,
                        property.PropertyInfo.SetMethod,
                        Expression.Convert(
                            ExpressionUtils.Replace(
                                (JsonSerializer s, EncryptedValuesReader ev, object existing) => Deserialize(s, ev.ReadValue(propertyIndex).CreateReader(), property, existing),
                                serializer, encryptedValuesReader,
                                    Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object))),
                            property.Type)
                        ).OptimizeConstants());
                }
                else
                {
                    // propertyBlock is the body of this currentProperty's switch case
                    var propertyblock = new List<Expression>();
                    var checkEV = CanContainEncryptedValues(property.Type);
                    if (checkEV)
                    {
                        // encryptedValuesReader.Nest({propertyIndex});
                        propertyblock.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes, Expression.Constant(propertyIndex)));
                    }

                    // existing value is either null or the value {property} depending on property.Populate
                    // value.{property} = ({property.Type})Deserialize(serializer, reader, existing value);
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

                    // reader.Read();
                    propertyblock.Add(
                        Expression.Call(reader, "Read", Type.EmptyTypes));

                    if (checkEV)
                    {
                        // encryptedValuesReader.AssertEnd();
                        propertyblock.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));
                    }

                    // create this currentProperty's switch case
                    // case {property.Name}:
                    //     {propertyBlock}
                    propertiesSwitch.Add(Expression.SwitchCase(
                        Expression.Block(typeof(void), propertyblock),
                        Expression.Constant(property.Name)
                    ));
                }
            }

            // WARNING: the following code is not commented out. It's a transcription of the expression below it. Yes, it's long.
            // while(reader reads properties and assigns them to currentProperty)
            // {
            //     switch(currentProperty)
            //     {
            //     {propertiesSwitch}
            //     default:
            //         if(reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.Start)
            //         {
            //             reader.Skip();
            //         }
            //         reader.Read();
            //     }
            // }
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
            // encryptedValuesReader.AssertEnd();
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));

            //TODO: find out why this is here
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

        private static Dictionary<Type, MethodInfo> writeValueMethods =
            (from method in typeof(JsonWriter).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            where method.Name == nameof(JsonWriter.WriteValue)
            let parameters = method.GetParameters()
            where parameters.Length == 1
            let parameterType = parameters[0].ParameterType
            where parameterType != typeof(object) && parameterType != typeof(byte[])
            where parameterType != typeof(DateTime) && parameterType != typeof(DateTime?)
            where parameterType != typeof(DateTimeOffset) && parameterType != typeof(DateTimeOffset?)
            select new { key = parameterType, value = method }
            ).ToDictionary(x => x.key, x => x.value);

        private static Expression GetSerializeExpression(ViewModelPropertyMap property, Expression jsonWriter, Expression value, Expression serializer)
        {
            if (property.JsonConverter?.CanWrite == true)
            {
                // maybe use the converter. It can't be easily inlined because polymorphism
                return ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, object v) => Serialize(s, w, property, v), serializer, jsonWriter, Expression.Convert(value, typeof(object)));
            }
            else if (writeValueMethods.TryGetValue(value.Type, out var method))
            {
                return Expression.Call(jsonWriter, method, new [] { value });
            }
            else
            {
                return Expression.Call(serializer, "Serialize", Type.EmptyTypes, new [] { jsonWriter, Expression.Convert(value, typeof(object)) });
            }
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
                if (property.Type.GetTypeInfo().IsValueType && reader.TokenType == JsonToken.Null)
                {
                    return Activator.CreateInstance(property.Type);
                }
                else
                {
                    return serializer.Deserialize(reader, property.Type);
                }
            }
        }

        /// Gets if this object require $type to be serialized.
        public bool RequiredTypeField() =>
            this.Properties.Any(p => p.ClientValidationRules.Any()); // it is required for validation

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

            // curly brackets are used for variables and methods from the scope of this factory method
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value, Expression.Convert(valueParam, Type)));

            // writer.WriteStartObject();
            block.Add(Expression.Call(writer, nameof(JsonWriter.WriteStartObject), Type.EmptyTypes));

            // encryptedValuesWriter.Nest();
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes));

            if (this.RequiredTypeField())
            {
                // writer.WritePropertyName("$type");
                block.Add(ExpressionUtils.Replace((JsonWriter w) => w.WritePropertyName("$type"), writer));

                // serializer.Serialize(writer, value.GetType().FullName)
                block.Add(ExpressionUtils.Replace((JsonSerializer s, JsonWriter w, string t) => w.WriteValue(t), serializer, writer, Expression.Constant(Type.GetTypeHash())));
            }

            // go through all properties that should be serialized
            for (int propertyIndex = 0; propertyIndex < Properties.Count(); propertyIndex++)
            {
                var property = Properties.ElementAt(propertyIndex);
                var endPropertyLabel = Expression.Label("end_property_" + property.Name);
                var options = new Dictionary<string, object>();
                if (property.TransferToClient && property.PropertyInfo.GetMethod != null)
                {
                    if (property.TransferFirstRequest != property.TransferAfterPostback)
                    {
                        if (property.ViewModelProtection != ProtectMode.None)
                        {
                            throw new Exception("Property sent only on selected requests can use viewModel protection.");
                        }

                        Expression condition = isPostback;
                        if (property.TransferAfterPostback)
                        {
                            condition = Expression.Not(condition);
                        }

                        block.Add(Expression.IfThen(condition, Expression.Goto(endPropertyLabel)));
                    }

                    // (object)value.{property.PropertyInfo.Name}
                    var prop = Expression.Property(value, property.PropertyInfo);

                    if (property.ViewModelProtection == ProtectMode.EncryptData ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        // encryptedValuesWriter.Value({propertyIndex}, (object)value.{property.PropertyInfo.Name});
                        block.Add(
                            Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.WriteValue), Type.EmptyTypes, Expression.Constant(propertyIndex), Expression.Convert(prop, typeof(object))));
                    }

                    if (property.ViewModelProtection == ProtectMode.None ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        var checkEV = CanContainEncryptedValues(property.Type);
                        if (checkEV)
                        {
                            // encryptedValuesWriter.Nest({propertyIndex});
                            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes, Expression.Constant(propertyIndex)));
                        }

                        // writer.WritePropertyName({property.Name});
                        block.Add(Expression.Call(writer, nameof(JsonWriter.WritePropertyName), Type.EmptyTypes,
                            Expression.Constant(property.Name)));

                        // serializer.Serialize(serializer, writer, {property}, (object)value.{property.PropertyInfo.Name});
                        block.Add(GetSerializeExpression(property, writer, prop, serializer));

                        if (checkEV)
                        {
                            // encryption is worthless if the property is not being transfered both ways
                            // therefore ClearEmptyNest throws exception if the property contains encrypted values
                            if (!property.IsFullyTransferred())
                            {
                                // encryptedValuesWriter.ClearEmptyNest();
                                block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.ClearEmptyNest), Type.EmptyTypes));
                            }
                            else
                            {
                                // encryptedValuesWriter.End();
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

            // writer.WriteEndObject();
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // encryptedValuesWriter.End();
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));

            // compile the expression
            var ex = Expression.Lambda<WriterDelegate>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValuesWriter, isPostback);
            return ex.Compile();
        }

        private void GenerateOptionsBlock(IList<Expression> block, ViewModelPropertyMap property, Dictionary<string, object> options, ParameterExpression writer)
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
            if ((property.TransferToClient || property.TransferToServer) && property.ViewModelProtection != ProtectMode.EncryptData)
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
        private bool CanContainEncryptedValues(Type type)
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
