using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using System.Reflection;
using DotVVM.Framework.Configuration;
using FastExpressionCompiler;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Performs the JSON serialization for specified type.
    /// </summary>
    public class ViewModelSerializationMap
    {
        private readonly DotvvmConfiguration configuration;

        public delegate void ReaderDelegate(JsonReader reader, JsonSerializer serializer, object value, EncryptedValuesReader encryptedValuesReader);
        public delegate void WriterDelegate(JsonWriter writer, object obj, JsonSerializer serializer, EncryptedValuesWriter evWriter, bool isPostback);

        /// <summary>
        /// Gets or sets the object type for this serialization map.
        /// </summary>
        public Type Type { get; private set; }

        public IEnumerable<ViewModelPropertyMap> Properties { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelSerializationMap"/> class.
        /// </summary>
        public ViewModelSerializationMap(Type type, IEnumerable<ViewModelPropertyMap> properties, DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            Type = type;
            Properties = properties.ToList();
        }

        public void ResetFunctions()
        {
            readerFactory = null;
            writerFactory = null;
        }

        private ReaderDelegate? readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public ReaderDelegate ReaderFactory => readerFactory ?? (readerFactory = CreateReaderFactory());
        private WriterDelegate? writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public WriterDelegate WriterFactory => writerFactory ?? (writerFactory = CreateWriterFactory());
        private Func<IServiceProvider, object>? constructorFactory;
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
            return ex.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
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
            var readerTmp = Expression.Variable(typeof(JsonReader), "readerTmp");

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

                var existingValue =
                    property.Populate ?
                    (Expression)Expression.Convert(Expression.Property(value, property.PropertyInfo), typeof(object)) :
                    Expression.Constant(null, typeof(object));

                // when suppressed, we read from the standard properties, because the object is nested in the
                var isEVSuppressed = Expression.Property(encryptedValuesReader, "Suppressed");

                var readEncrypted =
                    property.ViewModelProtection == ProtectMode.EncryptData || property.ViewModelProtection == ProtectMode.SignData;

                if (readEncrypted)
                {
                    // encryptedValuesReader.Suppress()
                    // value.{property} = ({property.Type})Deserialize(serializer, encryptedValuesReader.ReadValue({propertyIndex}), {property}, (object)value.{PropertyInfo});
                    // encryptedValuesReader.EndSuppress()
                    Expression readEncryptedValue = Expression.Block(
                        Expression.Assign(
                            readerTmp,
                            ExpressionUtils.Replace(
                                (EncryptedValuesReader ev) => ev.ReadValue(propertyIndex).CreateReader(),
                                encryptedValuesReader).OptimizeConstants()
                        ),
                        Expression.Call(encryptedValuesReader, "Suppress", Type.EmptyTypes),
                        Expression.Assign(
                            Expression.Property(value, property.PropertyInfo),
                            Expression.Convert(
                                ExpressionUtils.Replace(
                                    (JsonSerializer s, JsonReader reader, object existing) => Deserialize(s, reader, property, existing),
                                    serializer, readerTmp, existingValue),
                                property.Type)
                            ).OptimizeConstants()
                    );

                    readEncryptedValue = Expression.TryFinally(
                        readEncryptedValue,
                        Expression.Call(encryptedValuesReader, "EndSuppress", Type.EmptyTypes)
                    );


                    // if (!encryptedValuesReader.Suppressed)
                    block.Add(Expression.IfThen(
                        Expression.Not(isEVSuppressed),
                        readEncryptedValue
                    ));
                }
                // propertyBlock is the body of this currentProperty's switch case
                var propertyblock = new List<Expression>();
                var checkEV = CanContainEncryptedValues(property.Type) && !readEncrypted;
                if (checkEV)
                {
                    // encryptedValuesReader.Nest({propertyIndex});
                    propertyblock.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes, Expression.Constant(propertyIndex)));
                }

                // existing value is either null or the value {property} depending on property.Populate
                // value.{property} = ({property.Type})Deserialize(serializer, reader, existing value);
                propertyblock.Add(
                    Expression.Assign(
                    Expression.Property(value, property.PropertyInfo),
                    Expression.Convert(
                        ExpressionUtils.Replace((JsonSerializer s, JsonReader j, object existingValue) =>
                            Deserialize(s, j, property, existingValue),
                            serializer, reader, existingValue),
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

                Expression body = Expression.Block(typeof(void), propertyblock);

                if (readEncrypted)
                {
                    // only read the property when the reader is suppressed, otherwise do nothing
                    body = Expression.IfThenElse(
                        isEVSuppressed,
                        body,
                        Expression.Block(
                            Expression.IfThen(
                                ExpressionUtils.Replace((JsonReader rdr) => rdr.TokenType == JsonToken.StartArray || rdr.TokenType == JsonToken.StartConstructor || rdr.TokenType == JsonToken.StartObject, reader),
                                Expression.Call(reader, "Skip", Type.EmptyTypes)),
                            Expression.Call(reader, "Read", Type.EmptyTypes))
                    );
                }


                // create this currentProperty's switch case
                // case {property.Name}:
                //     {propertyBlock}
                propertiesSwitch.Add(Expression.SwitchCase(
                    body,
                    Expression.Constant(property.Name)
                ));
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

            // return value
            block.Add(value);

            // build the lambda expression
            var ex = Expression.Lambda<ReaderDelegate>(
                Expression.Convert(
                    Expression.Block(Type, new[] { value, currentProperty, readerTmp }, block),
                    typeof(object)).OptimizeConstants(),
                reader, serializer, valueParam, encryptedValuesReader);
            return ex.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
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

        private static object? Deserialize(JsonSerializer serializer, JsonReader reader, ViewModelPropertyMap property, object existingValue)
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
                if (property.Type.IsValueType && reader.TokenType == JsonToken.Null)
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
        public bool RequiredTypeField() => true;            // possible optimization - types can be inferred from parent metadata in some cases

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
                
                if (property.TransferToClient && property.PropertyInfo.GetMethod != null)
                {
                    if (property.TransferFirstRequest != property.TransferAfterPostback)
                    {
                        if (property.ViewModelProtection != ProtectMode.None)
                        {
                            throw new NotSupportedException($"The {Type}.{property.Name} property cannot user viewmodel protection because it is sent to the client only in some requests.");
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

                    var writeEV = property.ViewModelProtection == ProtectMode.EncryptData ||
                        property.ViewModelProtection == ProtectMode.SignData;

                    if (writeEV)
                    {
                        // encryptedValuesWriter.WriteValue({propertyIndex}, (object)value.{property.PropertyInfo.Name});
                        block.Add(
                            Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.WriteValue), Type.EmptyTypes, Expression.Constant(propertyIndex), Expression.Convert(prop, typeof(object))));
                    }


                    if (property.ViewModelProtection == ProtectMode.None ||
                        property.ViewModelProtection == ProtectMode.SignData)
                    {
                        var propertyBlock = new List<Expression>();
                        var checkEV = CanContainEncryptedValues(property.Type);
                        if (checkEV)
                        {
                            if (writeEV)
                            {
                                // encryptedValuesWriter.Suppress();
                                propertyBlock.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Suppress), Type.EmptyTypes));
                            }
                            else
                            {
                                // encryptedValuesWriter.Nest({propertyIndex});
                                propertyBlock.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes, Expression.Constant(propertyIndex)));
                            }
                        }

                        // writer.WritePropertyName({property.Name});
                        propertyBlock.Add(Expression.Call(writer, nameof(JsonWriter.WritePropertyName), Type.EmptyTypes,
                            Expression.Constant(property.Name)));

                        // serializer.Serialize(serializer, writer, {property}, (object)value.{property.PropertyInfo.Name});
                        propertyBlock.Add(GetSerializeExpression(property, writer, prop, serializer));

                        Expression propertyFinally = Expression.Default(typeof(void));
                        if (checkEV)
                        {
                            if (writeEV)
                            {
                                // encryptedValuesWriter.EndSuppress();
                                propertyFinally = Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.EndSuppress), Type.EmptyTypes);
                            }
                            // encryption is worthless if the property is not being transferred both ways
                            // therefore ClearEmptyNest throws exception if the property contains encrypted values
                            else if (!property.IsFullyTransferred())
                            {
                                // encryptedValuesWriter.ClearEmptyNest();
                                propertyFinally = Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.ClearEmptyNest), Type.EmptyTypes);
                            }
                            else
                            {
                                // encryptedValuesWriter.End();
                                propertyFinally = Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes);
                            }
                        }

                        block.Add(
                            Expression.TryFinally(
                                Expression.Block(propertyBlock),
                                propertyFinally
                            )
                        );
                    }
                }

                block.Add(Expression.Label(endPropertyLabel));
            }

            // writer.WriteEndObject();
            block.Add(ExpressionUtils.Replace<JsonWriter>(w => w.WriteEndObject(), writer));

            // encryptedValuesWriter.End();
            block.Add(Expression.Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes));

            // compile the expression
            var ex = Expression.Lambda<WriterDelegate>(
                Expression.Block(new[] { value }, block).OptimizeConstants(), writer, valueParam, serializer, encryptedValuesWriter, isPostback);
            return ex.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
        }

        /// <summary>
        /// Determines whether type can contain encrypted fields
        /// </summary>
        private bool CanContainEncryptedValues(Type type)
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
