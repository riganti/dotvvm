using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;
using System.Reflection;
using DotVVM.Framework.Configuration;
using FastExpressionCompiler;
using static System.Linq.Expressions.Expression;
using System.Collections.Immutable;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Performs the JSON serialization for specified type.
    /// </summary>
    public class ViewModelSerializationMap
    {
        private readonly DotvvmConfiguration configuration;

        public delegate object ReaderDelegate(JsonReader reader, JsonSerializer serializer, object? existingValue, EncryptedValuesReader encryptedValuesReader, IServiceProvider services);
        public delegate void WriterDelegate(JsonWriter writer, object obj, JsonSerializer serializer, EncryptedValuesWriter evWriter, bool isPostback);

        /// <summary>
        /// Gets or sets the object type for this serialization map.
        /// </summary>
        public Type Type { get; private set; }
        public MethodBase? Constructor { get; set; }
        public ImmutableArray<ViewModelPropertyMap> Properties { get; private set; }


        /// <summary> Rough structure of Properties when the object was initialized. This is used for hot reload to judge if it can be flushed from the cache. </summary>
        internal (string name, Type t, Direction direction, ProtectMode protection)[] OriginalProperties { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelSerializationMap"/> class.
        /// </summary>
        public ViewModelSerializationMap(Type type, IEnumerable<ViewModelPropertyMap> properties, MethodBase? constructor, DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            Type = type;
            Constructor = constructor;
            Properties = properties.ToImmutableArray();
            OriginalProperties = Properties.Select(p => (p.Name, p.Type, p.BindDirection, p.ViewModelProtection)).ToArray();
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

        public void SetConstructor(Func<IServiceProvider, object> constructor) => constructorFactory = constructor;

        /// <summary>
        /// Creates the constructor for this object.
        /// </summary>
        private Expression CallConstructor(Expression services, Expression[] properties)
        {
            if (constructorFactory != null)
                return Convert(Invoke(Constant(constructorFactory), services), Type);

            if (Constructor is null && Type.IsValueType)
            {
                // structs don't need default constructors
                return Default(Type);
            }

            if (Constructor is null)
                throw new Exception($"Can not deserialize {Type.FullName}, no constructor or multiple constructors found. Use the [JsonConstructor] attribute to specify the constructor used for deserialization.");

            var parameters = Constructor.GetParameters().Select(p => properties[Properties.FindIndex(pp => pp.ConstructorParameter == p)]).ToArray();
            return Constructor switch {
                ConstructorInfo c =>
                    New(c, parameters),
                MethodInfo m =>
                    Call(m, parameters),
                _ => throw new NotSupportedException()
            };
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
            var servicesParameter = Expression.Parameter(typeof(IServiceProvider), "services");
            var value = Expression.Variable(Type, "value");
            var currentProperty = Expression.Variable(typeof(string), "currentProperty");
            var readerTmp = Expression.Variable(typeof(JsonReader), "readerTmp");

            // we first read all values into local variables and only then we either call the constructor or set the properties on the object
            var propertyVars = Properties.Select(p => Variable(p.Type, "prop_" + p.Name)).ToArray();

            // curly brackets are used for variables and methods from the context of this factory method
            // value = ({Type})valueParam;
            block.Add(Expression.Assign(value,
                Type.IsValueType
                    ? Condition(Equal(valueParam, Constant(null)),
                        Default(Type),
                        Expression.Convert(valueParam, Type)
                    )
                    : Expression.Convert(valueParam, Type)));

            // get existing values into the local variables
            block.Add(IfThen(
                Expression.NotEqual(valueParam, Expression.Constant(null)),
                Expression.Block(
                    Properties
                        .Zip(propertyVars, (p, v) => p.PropertyInfo.GetMethod != null ? Expression.Assign(v, Expression.Property(value, p.PropertyInfo)) : null)
                        .Where(e => e != null)!
                )
            ));

            // add current object to encrypted values, this is needed because one property can potentially contain more objects (is a collection)
            block.Add(Expression.Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));


            // if the reader is in an invalid state, throw an exception
            // TODO: Change exception type, just Exception is not exactly descriptive
            block.Add(ExpressionUtils.Replace((JsonReader rdr) => rdr.TokenType == JsonToken.StartObject ? rdr.Read() : ExpressionUtils.Stub.Throw<bool>(new Exception($"TokenType = StartObject was expected.")), reader));

            var propertiesSwitch = new List<SwitchCase>();

            // iterate through all properties even if they're gonna get skipped
            // it's important for the index to count with all the properties that viewModel contains because the client will send some of them only sometimes
            for (int propertyIndex = 0; propertyIndex < Properties.Length; propertyIndex++)
            {
                var property = Properties[propertyIndex];
                var propertyVar = propertyVars[propertyIndex];
                if (!property.TransferToServer)
                {
                    continue;
                }

                var existingValue =
                    property.Populate ?
                    (Expression)Convert(propertyVar, typeof(object)) :
                    Constant(null, typeof(object));

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
                            propertyVar,
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
                    propertyVar,
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

            // call the constructor
            var hasConstructorProperties = Properties.Any(p => p.ConstructorParameter is {});

            var constructorCall = CallConstructor(servicesParameter, propertyVars);
            if (hasConstructorProperties)
            {
                block.Add(Assign(value, constructorCall));
            }
            else
            {
                block.Add(IfThen(
                    Equal(valueParam, Constant(null)),
                    Expression.Assign(value, constructorCall))
                );
            }

            var setProperties = Expression.Block(
                Properties
                    .Zip(propertyVars, (p, v) =>
                        p is { PropertyInfo.SetMethod: not null, ConstructorParameter: null, TransferToServer: true }
                            ? Expression.Assign(Expression.Property(value, p.PropertyInfo), v)
                            : null)
                    .Where(e => e != null)!
            );
            block.Add(setProperties);

            // return value
            block.Add(Convert(value, typeof(object)));

            // build the lambda expression
            var ex = Expression.Lambda<ReaderDelegate>(
                Expression.Block(typeof(object), new[] { value, currentProperty, readerTmp }.Concat(propertyVars), block).OptimizeConstants(),
                reader, serializer, valueParam, encryptedValuesReader, servicesParameter);
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
                    return serializer.Converters.OfType<ViewModelJsonConverter>().First().Populate(reader, serializer, existingValue);
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
            for (int propertyIndex = 0; propertyIndex < Properties.Length; propertyIndex++)
            {
                var property = Properties[propertyIndex];
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
