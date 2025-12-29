using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;
using System.Reflection;
using DotVVM.Framework.Configuration;
using FastExpressionCompiler;
using static System.Linq.Expressions.Expression;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Performs the JSON serialization for specified type.
    /// </summary>
    public abstract class ViewModelSerializationMap
    {
        protected readonly JsonSerializerOptions jsonOptions;
        protected readonly DotvvmConfiguration configuration;
        protected readonly ViewModelJsonConverter viewModelJsonConverter;

        public delegate T ReaderDelegate<T>(ref Utf8JsonReader reader, JsonSerializerOptions options, T? existingValue, bool populate, EncryptedValuesReader encryptedValuesReader, DotvvmSerializationState state);
        public delegate void WriterDelegate<T>(Utf8JsonWriter writer, T obj, JsonSerializerOptions options, bool requireTypeField, EncryptedValuesWriter evWriter, DotvvmSerializationState state);

        /// <summary>
        /// Gets or sets the object type for this serialization map.
        /// </summary>
        public Type Type { get; }
        public MethodBase? Constructor { get; }
        public ImmutableArray<ViewModelPropertyMap> Properties { get; }
        public string ClientTypeId { get; }


        /// <summary> Rough structure of Properties when the object was initialized. This is used for hot reload to judge if it can be flushed from the cache. </summary>
        internal (string name, Type t, Direction direction, ProtectMode protection)[] OriginalProperties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelSerializationMap"/> class.
        /// </summary>
        internal ViewModelSerializationMap(Type type, IEnumerable<ViewModelPropertyMap> properties, MethodBase? constructor, JsonSerializerOptions jsonOptions, DotvvmConfiguration configuration)
        {
            this.jsonOptions = jsonOptions;
            this.configuration = configuration;
            this.viewModelJsonConverter = configuration.ServiceProvider.GetRequiredService<ViewModelJsonConverter>();
            Type = type;
            ClientTypeId = type.GetTypeHash();
            Constructor = constructor;
            Properties = properties.ToImmutableArray();
            OriginalProperties = Properties.Select(p => (p.Name, p.Type, p.BindDirection, p.ViewModelProtection)).ToArray();
            ValidatePropertyMap();
        }

        public static ViewModelSerializationMap Create(Type type, IEnumerable<ViewModelPropertyMap> properties, MethodBase? constructor, DotvvmConfiguration configuration) =>
            (ViewModelSerializationMap)Activator.CreateInstance(typeof(ViewModelSerializationMap<>).MakeGenericType(type), properties, constructor, configuration)!;

        private void ValidatePropertyMap()
        {
            var dict = new Dictionary<string, ViewModelPropertyMap>(capacity: Properties.Length);
            foreach (var propertyMap in Properties)
            {
                if (!dict.ContainsKey(propertyMap.Name))
                {
                    dict.Add(propertyMap.Name, propertyMap);
                }
                else
                {
                    var other = dict[propertyMap.Name];
                    throw new InvalidOperationException($"Serialization map for '{Type.ToCode()}' has a name conflict between a {(propertyMap.PropertyInfo is FieldInfo ? "field" : "property")} '{propertyMap.PropertyInfo.Name}' and {(other.PropertyInfo is FieldInfo ? "field" : "property")} '{other.PropertyInfo.Name}' — both are named '{propertyMap.Name}' in JSON.");
                }
            }
        }

        public abstract void ResetFunctions();
        public abstract void SetConstructorUntyped(Func<IServiceProvider, object> constructor);

    }
    public sealed class ViewModelSerializationMap<T> : ViewModelSerializationMap
    {
        public ViewModelSerializationMap(IEnumerable<ViewModelPropertyMap> properties, MethodBase? constructor, JsonSerializerOptions jsonOptions, DotvvmConfiguration configuration): 
            base(typeof(T), properties, constructor, jsonOptions, configuration)
        {
        }
        public override void ResetFunctions()
        {
            readerFactory = null;
            writerFactory = null;
        }

        private ReaderDelegate<T>? readerFactory;
        /// <summary>
        /// Gets the JSON reader factory.
        /// </summary>
        public ReaderDelegate<T> ReaderFactory => readerFactory ??= CreateReaderFactory();
        private WriterDelegate<T>? writerFactory;
        /// <summary>
        /// Gets the JSON writer factory.
        /// </summary>
        public WriterDelegate<T> WriterFactory => writerFactory ??= CreateWriterFactory();
        private Func<IServiceProvider, T>? constructorFactory;

        public void SetConstructor(Func<IServiceProvider, T> constructor) => constructorFactory = constructor;
        public override void SetConstructorUntyped(Func<IServiceProvider, object> constructor) => constructorFactory = s => (T)constructor(s);

        /// <summary>
        /// Creates the constructor for this object.
        /// </summary>
        private Expression CallConstructor(Expression services, Dictionary<ViewModelPropertyMap, ParameterExpression> propertyVariables, bool throwImmediately = false)
        {
            if (constructorFactory != null)
                return Invoke(Constant(constructorFactory), services);

            if (Constructor is null && Type.IsValueType)
            {
                // structs don't need default constructors
                return Default(Type);
            }

            if (Constructor is null && (Type.IsInterface || Type.IsAbstract))
                return jitException($"Can not deserialize {Type.ToCode()} because it's abstract. Please avoid using abstract types in view model. If you really mean it, you can add a static factory method and mark it with [JsonConstructor] attribute.");

            if (Constructor is null)
                return jitException($"Can not deserialize {Type.ToCode()}, no parameterless constructor found. Use the [JsonConstructor] attribute to specify the constructor used for deserialization.");

            var parameters = Constructor.GetParameters().Select(p => {
                var prop = Properties.FirstOrDefault(pp => pp.ConstructorParameter == p);
                if (prop is null)
                {
                    var mayBeService = !ReflectionUtils.IsPrimitiveType(p.ParameterType);
                    if (!mayBeService)
                        throw new Exception($"Can not deserialize {Type.ToCode()}, constructor parameter {p.Name} is not mapped to any property.");

                    var errorMessage = $"Can not deserialize {Type.ToCode()}, constructor parameter {p.Name} is not mapped to any property and service {p.ParameterType.ToCode(stripNamespace: true)} was not found in ServiceProvider.";
                    return ExpressionUtils.WrapException(
                        Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new [] { p.ParameterType }, services),
                        e => throw new Exception(errorMessage, e)
                    );
                }

                if (!prop.TransferToServer)
                    throw new Exception($"Can not deserialize {Type.ToCode()}, property {prop.Name} is not transferred to server, but it's used in constructor.");

                Debug.Assert(propertyVariables.ContainsKey(prop));

                return propertyVariables[prop];
            }).ToArray();
            return Constructor switch {
                ConstructorInfo c =>
                    New(c, parameters),
                MethodInfo m =>
                    Call(m, parameters),
                _ => throw new NotSupportedException()
            };

            // Since the old serializer didn't care about constructor problems until it was actually needed,
            // we can't throw exception during compilation, so we wait until this code will run.
            Expression jitException(string message) =>
                throwImmediately
                    ? throw new Exception(message)
                    : Throw(New(
                        typeof(Exception).GetConstructor(new [] { typeof(string) })!,
                        Constant(message)
                    ), this.Type);
        }

        /// <summary>
        /// Creates the reader factory.
        /// </summary>
        public ReaderDelegate<T> CreateReaderFactory()
        {
            var block = new List<Expression>();
            var reader = Parameter(typeof(Utf8JsonReader).MakeByRefType(), "reader");
            var jsonOptions = Parameter(typeof(JsonSerializerOptions), "jsonOptions");
            var value = Parameter(typeof(T), "value");
            var allowPopulate = Parameter(typeof(bool), "allowPopulate");
            var encryptedValuesReader = Parameter(typeof(EncryptedValuesReader), "encryptedValuesReader");
            var state = Parameter(typeof(DotvvmSerializationState), "state");
            var currentProperty = Variable(typeof(string), "currentProperty");
            var readerTmp = Variable(typeof(Utf8JsonReader), "readerTmp");

            // we first read all values into local variables and only then we either call the constructor or set the properties on the object
            var propertyVars = Properties
                .Where(p => p.TransferToServer)
                .ToDictionary(
                    p => p,
                    p => Variable(p.Type, "prop_" + p.Name)
                );

            // If we have constructor property or if we have { get; init; } property, we always create new instance
            var alwaysCallConstructor = Properties.Any(p => p.TransferToServer && (
                p.ConstructorParameter is {} ||
                (p.PropertyInfo as PropertyInfo)?.IsInitOnly() == true));

            // We don't want to clone IDotvvmViewModel automatically, because the user is likely to register this specific instance somewhere
            if (alwaysCallConstructor && typeof(IDotvvmViewModel).IsAssignableFrom(Type) && Constructor is {} && !SerialiationMapperAttributeHelper.IsJsonConstructor(Constructor))
            {
                var cloneReason =
                    Properties.FirstOrDefault(p => p.TransferToServer && (p.PropertyInfo as PropertyInfo)?.IsInitOnly() == true) is {} initProperty
                        ? $"init-only property {initProperty.Name} is transferred client → server" :
                    Properties.FirstOrDefault(p => p.TransferToServer && p.ConstructorParameter is {}) is {} ctorProperty
                        ? $"property {ctorProperty.Name} must be injected into constructor parameter {ctorProperty.ConstructorParameter!.Name}" : "internal bug";
                throw new Exception($"Deserialization of {Type.ToCode()} is not allowed, because it implements IDotvvmViewModel and {cloneReason}. To allow cloning the object on deserialization, mark a constructor with [JsonConstructor].");
            }
            var constructorCall = CallConstructor(Property(state, "Services"), propertyVars, throwImmediately: alwaysCallConstructor);

            // curly brackets are used for variables and methods from the context of this factory method
            // if (!allowPopulate && !alwaysCallConstructor)
            //     value = new T()
            if (!alwaysCallConstructor)
                block.Add(IfThen(
                    Not(allowPopulate),
                    Assign(value, constructorCall)
                ));

            // get existing values into the local variables
            if (propertyVars.Count > 0)
            {
                // if (value != null)
                //     prop_X = value.X; ...
                block.Add(IfThen(
                    Type.IsValueType ? Constant(true) : NotEqual(value, Constant(null)),
                    Block(
                        propertyVars
                            .Where(p => p.Key.PropertyInfo is not PropertyInfo { GetMethod: null })
                            .Select(p => Assign(p.Value, MemberAccess(value, p.Key)))
                    )
                ));
            }

            // add current object to encrypted values, this is needed because one property can potentially contain more objects (is a collection)
            block.Add(Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes));

            var propertiesSwitch = new List<(string fieldName, Expression readExpression)>();

            // iterate through all properties even if they're gonna get skipped
            // it's important for the index to count with all the properties that viewModel contains because the client will send some of them only sometimes
            for (int propertyIndex = 0; propertyIndex < Properties.Length; propertyIndex++)
            {
                var property = Properties[propertyIndex];
                if (!property.TransferToServer)
                {
                    continue;
                }
                var propertyVar = propertyVars[property];

                var existingValue =
                    property.Populate ?
                    (Expression)Convert(propertyVar, typeof(object)) :
                    Constant(null, typeof(object));

                // when suppressed, we read from the standard properties, because the object is nested in the
                var isEVSuppressed = Property(encryptedValuesReader, "Suppressed");

                var readEncrypted =
                    property.ViewModelProtection == ProtectMode.EncryptData || property.ViewModelProtection == ProtectMode.SignData;

                if (readEncrypted)
                {
                    // encryptedValuesReader.Suppress()
                    // value.{property} = ({property.Type})Deserialize(serializer, encryptedValuesReader.ReadValue({propertyIndex}), {property}, (object)value.{PropertyInfo});
                    // encryptedValuesReader.EndSuppress()
                    Expression readEncryptedValue = Block(
                        Assign(
                            readerTmp,
                            Call(JsonSerializationCodegenFragments.ReadEncryptedValueMethod, Call(encryptedValuesReader, "ReadValue", Type.EmptyTypes, Constant(propertyIndex)))
                        ),
                        Call(encryptedValuesReader, "Suppress", Type.EmptyTypes),

                        Assign(
                            propertyVar,
                            DeserializePropertyValue(property, readerTmp, propertyVar, jsonOptions, state))
                    );

                    readEncryptedValue = TryFinally(
                        readEncryptedValue,
                        Call(encryptedValuesReader, "EndSuppress", Type.EmptyTypes)
                    );

                    // if (!encryptedValuesReader.Suppressed)
                    //     ...readEncryptedValue
                    block.Add(IfThen(Not(isEVSuppressed), readEncryptedValue));
                }
                // propertyBlock is the body of this currentProperty's switch case
                var propertyblock = new List<Expression>();
                var checkEV = CanContainEncryptedValues(property.Type) && !readEncrypted;
                if (checkEV)
                {
                    // encryptedValuesReader.Nest({propertyIndex});
                    propertyblock.Add(Call(encryptedValuesReader, nameof(EncryptedValuesReader.Nest), Type.EmptyTypes, Constant(propertyIndex)));
                }

                // existing value is either null or the value {property} depending on property.Populate
                // value.{property} = ({property.Type})Deserialize(serializer, reader, existing value);
                propertyblock.Add(Assign(
                    propertyVar,
                    DeserializePropertyValue(property, reader, propertyVar, jsonOptions, state)
                ));

                // reader.Read();
                propertyblock.Add(Call(reader, "Read", Type.EmptyTypes));

                if (checkEV)
                {
                    // encryptedValuesReader.AssertEnd();
                    propertyblock.Add(Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));
                }

                Expression body = Block(typeof(void), propertyblock);

                if (readEncrypted)
                {
                    // only read the property when the reader is suppressed, otherwise do nothing
                    body = IfThenElse(
                        isEVSuppressed,
                        body,
                        Call(JsonSerializationCodegenFragments.IgnoreValueMethod, reader)
                    );
                }


                propertiesSwitch.Add((property.Name, body));
            }

            // while((currentProperty = JsonSerializationCodegenFragments.TryReadNextProperty(ref reader)) != null)
            block.Add(ExpressionUtils.While(
                condition: ReferenceNotEqual(Assign(currentProperty, Call(JsonSerializationCodegenFragments.TryReadNextPropertyMethod, reader)), Constant(null, typeof(string))),
            //     switch(currentProperty)
                body: ExpressionUtils.Switch(
                    condition: currentProperty,
            //     case "{property}": see above
                    cases: propertiesSwitch.Select(p => SwitchCase(p.readExpression, Constant(p.fieldName))).ToArray(),
            //     default:
            //         JsonSerializationCodegenFragments.IgnoreValue(ref reader)
                    defaultCase: Call(JsonSerializationCodegenFragments.IgnoreValueMethod, reader)
                )
            ));

            // close encrypted values
            // encryptedValuesReader.AssertEnd();
            block.Add(Call(encryptedValuesReader, nameof(EncryptedValuesReader.AssertEnd), Type.EmptyTypes));

            // call the constructor
            if (alwaysCallConstructor)
            {
                block.Add(Assign(value, constructorCall));
            }

            if (propertyVars.Count > 0)
            {
                var propertySettingExpressions =
                    Properties
                        .Where(p => p is { ConstructorParameter: null, TransferToServer: true, PropertyInfo: PropertyInfo { SetMethod: not null } or FieldInfo { IsInitOnly: false } })
                        .Select(p => Assign(MemberAccess(value, p), propertyVars[p]))
                        .ToList();

                if (propertySettingExpressions.Any())
                {
                    block.Add(Block(propertySettingExpressions!));
                }
            }

            // return value
            block.Add(value);

            // build the lambda expression
            var ex = Lambda<ReaderDelegate<T>>(
                Block(typeof(T), [ currentProperty, readerTmp, ..propertyVars.Values ], block).OptimizeConstants(),
                reader, jsonOptions, value, allowPopulate, encryptedValuesReader, state);
            return ex.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
            //return ex.Compile();
        }

        Expression MemberAccess(Expression obj, ViewModelPropertyMap property)
        {
            if (property.PropertyInfo is PropertyInfo pi)
                return Property(obj, pi);
            if (property.PropertyInfo is FieldInfo fi)
                return Field(obj, fi);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates the writer factory.
        /// </summary>
        public WriterDelegate<T> CreateWriterFactory()
        {
            var block = new List<Expression>();
            var writer = Parameter(typeof(Utf8JsonWriter), "writer");
            var value = Parameter(Type, "value");
            var jsonOptions = Parameter(typeof(JsonSerializerOptions), "options");
            var requireTypeField = Parameter(typeof(bool), "requireTypeField");
            var encryptedValuesWriter = Parameter(typeof(EncryptedValuesWriter), "encryptedValuesWriter");
            var dotvvmState = Parameter(typeof(DotvvmSerializationState), "dotvvmState");

            var isPostback = Property(dotvvmState, "IsPostback");

            // curly brackets are used for variables and methods from the scope of this factory method
            // if (requireTypeField)
            //     writer.WriteString("$type", "{Type}");
            block.Add(IfThen(requireTypeField,
                ExpressionUtils.Replace((Utf8JsonWriter w, string t) => w.WriteString("$type", t), writer, Constant(Type.GetTypeHash()))
            ));

            // go through all properties that should be serialized
            for (int propertyIndex = 0; propertyIndex < Properties.Length; propertyIndex++)
            {
                var property = Properties[propertyIndex];
                var endPropertyLabel = Label("end_property_" + property.Name);

                if (property.TransferToClient && property.PropertyInfo is not PropertyInfo { GetMethod: null })
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
                            condition = Not(condition);
                        }

                        block.Add(IfThen(condition, Goto(endPropertyLabel)));
                    }

                    // (object)value.{property.PropertyInfo.Name}
                    var prop = MemberAccess(value, property);

                    var writeEV = property.ViewModelProtection == ProtectMode.EncryptData ||
                        property.ViewModelProtection == ProtectMode.SignData;

                    if (writeEV)
                    {
                        // encryptedValuesWriter.WriteValue({propertyIndex}, (object)value.{property.PropertyInfo.Name});
                        block.Add(
                            Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.WriteValue), Type.EmptyTypes, Constant(propertyIndex), Convert(prop, typeof(object))));
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
                                propertyBlock.Add(Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Suppress), Type.EmptyTypes));
                            }
                            else
                            {
                                // encryptedValuesWriter.Nest({propertyIndex});
                                propertyBlock.Add(Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.Nest), Type.EmptyTypes, Constant(propertyIndex)));
                            }
                        }

                        // writer.WritePropertyName({property.Name});
                        propertyBlock.Add(Call(writer, nameof(Utf8JsonWriter.WritePropertyName), Type.EmptyTypes,
                            Constant(property.Name)));

                        // serializer.Serialize(serializer, writer, {property}, (object)value.{property.PropertyInfo.Name});
                        propertyBlock.Add(GetSerializeExpression(property, writer, prop, jsonOptions, dotvvmState));

                        Expression propertyFinally = Default(typeof(void));
                        if (checkEV)
                        {
                            if (writeEV)
                            {
                                // encryptedValuesWriter.EndSuppress();
                                propertyFinally = Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.EndSuppress), Type.EmptyTypes);
                            }
                            // encryption is worthless if the property is not being transferred both ways
                            // therefore ClearEmptyNest throws exception if the property contains encrypted values
                            else if (!property.IsFullyTransferred())
                            {
                                // encryptedValuesWriter.ClearEmptyNest();
                                propertyFinally = Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.ClearEmptyNest), Type.EmptyTypes);
                            }
                            else
                            {
                                // encryptedValuesWriter.End();
                                propertyFinally = Call(encryptedValuesWriter, nameof(EncryptedValuesWriter.End), Type.EmptyTypes);
                            }
                        }

                        block.Add(
                            TryFinally(
                                Block(propertyBlock),
                                propertyFinally
                            )
                        );
                    }
                }

                block.Add(Label(endPropertyLabel));
            }

            // compile the expression
            var ex = Lambda<WriterDelegate<T>>(
                Block(block).OptimizeConstants(), writer, value, jsonOptions, requireTypeField, encryptedValuesWriter, dotvvmState);
            return ex.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
            //return ex.Compile();
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

        private JsonConverter? GetPropertyConverter(ViewModelPropertyMap property, Type type)
        {
            if (property.JsonConverter is null)
                return null;
            if (property.JsonConverter is JsonConverterFactory factory)
                return factory.CreateConverter(type, jsonOptions);
            return property.JsonConverter;
        }

        private Expression CallPropertyConverterRead(JsonConverter converter, Type type, Expression reader, Expression jsonOptions, Expression dotvvmState, Expression? existingValue)
        {
            Debug.Assert(reader.Type == typeof(Utf8JsonReader).MakeByRefType() || reader.Type == typeof(Utf8JsonReader), $"{reader.Type} != {typeof(Utf8JsonReader).MakeByRefType()}");
            Debug.Assert(jsonOptions.Type == typeof(JsonSerializerOptions));
            Debug.Assert(dotvvmState.Type == typeof(DotvvmSerializationState));


            if (converter is IDotvvmJsonConverter)
            {
                // T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state)
                // T Populate(ref Utf8JsonReader reader, Type typeToConvert, T value, JsonSerializerOptions options, DotvvmSerializationState state);
                if (existingValue is null)
                    return Call(Constant(converter), "Read", Type.EmptyTypes, reader, Constant(type), jsonOptions, dotvvmState);
                else
                    return Call(Constant(converter), "Populate", Type.EmptyTypes, reader, Constant(type), existingValue, jsonOptions, dotvvmState);
            }
            else
            {
                var read = Call(Constant(converter), "Read", Type.EmptyTypes, reader, Constant(type), jsonOptions);
                if (read.Type.IsValueType)
                    return read;
                else
                    return Condition(
                        test: Call(JsonSerializationCodegenFragments.IsNullTokenTypeMethod, reader),
                        ifTrue: Default(read.Type),
                        ifFalse: read
                    );
            }
        }

        private Expression CallPropertyConverterWrite(JsonConverter converter, Expression writer, Expression value, Expression jsonOptions, Expression dotvvmState)
        {
            Debug.Assert(writer.Type == typeof(Utf8JsonWriter));
            Debug.Assert(jsonOptions.Type == typeof(JsonSerializerOptions));
            Debug.Assert(dotvvmState.Type == typeof(DotvvmSerializationState));

            // void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true)
            if (converter is IDotvvmJsonConverter)
            {
                return Call(Constant(converter), nameof(IDotvvmJsonConverter<object>.Write), Type.EmptyTypes, writer, value, jsonOptions, dotvvmState, Constant(true), Constant(true));
            }
            else
            {
                return Call(Constant(converter), nameof(IDotvvmJsonConverter<object>.Write), Type.EmptyTypes, writer, value, jsonOptions);
            }
        }

        private Expression? TryDeserializePrimitive(Expression reader, Type type)
        {
            // Utf8JsonReader readerTest = default;
            if (type == typeof(bool))
                return Call(reader, "GetBoolean", Type.EmptyTypes);
            if (type == typeof(byte))
                return Call(reader, "GetByte", Type.EmptyTypes);
            if (type == typeof(decimal))
                return Call(reader, "GetDecimal", Type.EmptyTypes);
            if (type == typeof(double))
                return Call(
                    typeof(SystemTextJsonUtils).GetMethod(nameof(SystemTextJsonUtils.GetFloat64Value))!,
                    reader
                );
            if (type == typeof(Guid))
                return Call(reader, "GetGuid", Type.EmptyTypes);
            if (type == typeof(short))
                return Call(reader, "GetInt16", Type.EmptyTypes);
            if (type == typeof(int))
                return Call(reader, "GetInt32", Type.EmptyTypes);
            if (type == typeof(long))
                return Call(reader, "GetInt64", Type.EmptyTypes);
            if (type == typeof(sbyte))
                return Call(reader, "GetSByte", Type.EmptyTypes);
            if (type == typeof(float))
                return Call(
                    typeof(SystemTextJsonUtils).GetMethod(nameof(SystemTextJsonUtils.GetFloat32Value))!,
                    reader
                );
#if NET6_0_OR_GREATER
            if (type == typeof(Half))
                return Convert(Call(
                    typeof(SystemTextJsonUtils).GetMethod(nameof(SystemTextJsonUtils.GetFloat32Value))!,
                    reader
                ), typeof(Half));
#endif
            if (type == typeof(string))
                return Call(reader, "GetString", Type.EmptyTypes);
            if (type == typeof(ushort))
                return Call(reader, "GetUInt16", Type.EmptyTypes);
            if (type == typeof(uint))
                return Call(reader, "GetUInt32", Type.EmptyTypes);
            if (type == typeof(ulong))
                return Call(reader, "GetUInt64", Type.EmptyTypes);
            return null;
        }

        private Expression? TrySerializePrimitive(Expression writer, Expression value)
        {
            var type = value.Type;
            Debug.Assert(!ReflectionUtils.IsNullableType(type));
            // Utf8JsonWriter writer = default;
            // writer.WriteValue
            // Newtonsoft.Json.JsonWriter nj = default;
            if (type == typeof(bool))
                return Call(writer, "WriteBooleanValue", Type.EmptyTypes, value);
            if (type == typeof(float) || type == typeof(double))
                return Call(
                    typeof(SystemTextJsonUtils).GetMethod(nameof(SystemTextJsonUtils.WriteFloatValue), [ typeof(Utf8JsonWriter), type ])!,
                    writer, value
                );
#if NET6_0_OR_GREATER
            if (type == typeof(Half))
                return Call(
                    typeof(SystemTextJsonUtils).GetMethod(nameof(SystemTextJsonUtils.WriteFloatValue), [ typeof(Utf8JsonWriter), typeof(float) ])!,
                    writer, Convert(value, typeof(float))
                );
#endif
            if (type == typeof(decimal) ||
                type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
                return Call(writer, "WriteNumberValue", Type.EmptyTypes, value);
            if (type == typeof(short) || type == typeof(ushort) || type == typeof(sbyte) || type == typeof(byte))
                return Call(writer, "WriteNumberValue", Type.EmptyTypes, Convert(value, typeof(int)));
            if (type == typeof(string) || type == typeof(Guid)) // TODO: datetime too?
                return Call(writer, "WriteStringValue", Type.EmptyTypes, value);

            return null;
        }

        private Expression DeserializePropertyValue(ViewModelPropertyMap property, Expression reader, Expression existingValue, Expression jsonOptions, Expression dotvvmState)
        {
            var type = existingValue.Type;
            Debug.Assert(type.UnwrapNullableType() == property.Type.UnwrapNullableType(), $"{type} != {property.Type}, property: {property.PropertyInfo.DeclaringType}.{property.Name}");

            if (ReflectionUtils.IsNullable(existingValue.Type))
            {
                return Condition(
                    test: Call(JsonSerializationCodegenFragments.IsNullTokenTypeMethod, reader),
                    ifTrue: Default(type),
                    ifFalse: Convert(DeserializePropertyValue(property, reader, existingValue.UnwrapNullable(throwOnNull: false), jsonOptions, dotvvmState), type)
                );
            }
            if (GetPropertyConverter(property, type) is {} customConverter)
            {
                return CallPropertyConverterRead(customConverter, type, reader, jsonOptions, dotvvmState, property.Populate ? existingValue : null);
            }

            if (TryDeserializePrimitive(reader, type) is {} primitive)
            {
                return primitive;
            }

            var converter = this.jsonOptions.GetConverter(type);
            if (!converter.CanConvert(type))
            {
                throw new Exception($"JsonOptions returned an invalid converter {converter} for type {type}.");
            }
            if (property.AllowDynamicDispatch && !type.IsSealed)
            {
                if (converter is IDotvvmJsonConverter)
                {
                    return Call(
                        JsonSerializationCodegenFragments.DeserializeViewModelDynamicMethod.MakeGenericMethod(type),
                        reader, jsonOptions, existingValue, Constant(property.Populate), // ref Utf8JsonReader reader, JsonSerializerOptions options, TVM? existingValue, bool populate
                        Constant(this.viewModelJsonConverter), // ViewModelJsonConverter factory
                        Constant(converter), // ViewModelJsonConverter.VMConverter<TVM>? defaultConverter
                        dotvvmState); // DotvvmSerializationState state
                }
                else
                {
                    return Call(
                        JsonSerializationCodegenFragments.DeserializeValueDynamicMethod.MakeGenericMethod(property.Type),
                        reader, jsonOptions, existingValue, // ref Utf8JsonReader reader, JsonSerializerOptions options, TValue? existingValue
                        Constant(property.Populate), // bool populate
                        Constant(this.viewModelJsonConverter), // ViewModelJsonConverter factory
                        dotvvmState); // DotvvmSerializationState state
                }
            }
            else
            {
                return CallPropertyConverterRead(converter, type, reader, jsonOptions, dotvvmState, property.Populate ? existingValue : null);
            }
        }

        private Expression GetSerializeExpression(ViewModelPropertyMap property, Expression writer, Expression value, Expression jsonOptions, Expression dotvvmState)
        {
            Debug.Assert(jsonOptions.Type == typeof(JsonSerializerOptions));
            Debug.Assert(dotvvmState.Type == typeof(DotvvmSerializationState));
            Debug.Assert(value.Type.UnwrapNullableType() == property.Type.UnwrapNullableType(), $"{value.Type} != {property.Type}");

            if (ReflectionUtils.IsNullableType(value.Type))
            {
                return IfThenElse(
                    Property(value, "HasValue"),
                    GetSerializeExpression(property, writer, Property(value, "Value"), jsonOptions, dotvvmState),
                    Call(writer, "WriteNullValue", Type.EmptyTypes)
                );
            }

            if (GetPropertyConverter(property, value.Type) is {} converter)
            {
                return CallPropertyConverterWrite(converter, writer, value, jsonOptions, dotvvmState);
            }
            if (TrySerializePrimitive(writer, value) is {} primitive)
            {
                return primitive;
            }
            if (this.viewModelJsonConverter.CanConvert(value.Type))
            {
                if (property.AllowDynamicDispatch && !value.Type.IsSealed)
                {
                    if (value.Type.IsAbstract)
                    {
                        // Always doing dynamic dispatch to an unknown type
                        return Call(
                            (MethodInfo)MethodFindingHelper.GetMethodFromExpression(() =>
                                JsonSerializer.Serialize<object?>(default(Utf8JsonWriter)!, null, default(JsonSerializerOptions)!)),
                            writer,
                            Convert(value, typeof(object)),
                            jsonOptions
                        );
                    }
                    else
                    {
                        // We use cached converter for T, if value.GetType() == T
                        var defaultConverter = this.viewModelJsonConverter.GetConverter(value.Type);
                        return Call(
                            JsonSerializationCodegenFragments.SerializeViewModelDynamicMethod.MakeGenericMethod(value.Type),
                            writer, jsonOptions, value, Constant(defaultConverter), dotvvmState);
                    }
                }
                else
                {
                    var viewModelConverter = this.viewModelJsonConverter.GetConverter(value.Type);
                    return CallPropertyConverterWrite(viewModelConverter, writer, value, jsonOptions, dotvvmState);
                }
            }

            return Call(JsonSerializationCodegenFragments.SerializeValueMethod.MakeGenericMethod(value.Type), writer, jsonOptions, value, Constant(property.AllowDynamicDispatch && !value.Type.IsSealed));
        }
    }

    internal static class JsonSerializationCodegenFragments
    {
        public static readonly MethodInfo ReadAssertMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(ReadAssert), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadAssert(ref Utf8JsonReader reader, JsonTokenType tokenType)
        {
            if (reader.TokenType != tokenType)
                ThrowToken(tokenType, reader.TokenType);
            else
                reader.Read();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowToken(JsonTokenType expected, JsonTokenType actual)
        {
            throw new Exception($"TokenType = {expected} was expected, but {actual} was found.");
        }

        public static readonly MethodInfo SerializeValueMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(SerializeValue), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        private static void SerializeValue<TValue>(Utf8JsonWriter writer, JsonSerializerOptions options, TValue? value, bool dynamic)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else if (dynamic)
            {
                JsonSerializer.Serialize(writer, (object)value, options);
            }
            else
            {
                JsonSerializer.Serialize<TValue>(writer, value, options);
            }
        }

        public static readonly MethodInfo SerializeViewModelDynamicMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(SerializeViewModelDynamic), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        private static void SerializeViewModelDynamic<TVM>(Utf8JsonWriter writer, JsonSerializerOptions options, TVM? value, IDotvvmJsonConverter<TVM> defaultConverter, DotvvmSerializationState state)
            where TVM: class
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            var type = value.GetType();
            if (defaultConverter is {} && type == typeof(TVM))
            {
                defaultConverter.Write(writer, value, options, state);
                return;
            }
            else
            {
                JsonSerializer.Serialize(writer, value, type, options);
            }
        }

        public static readonly MethodInfo DeserializeValueStaticMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(DeserializeValueStatic), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        private static TValue? DeserializeValueStatic<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return SystemTextJsonUtils.Deserialize<TValue>(ref reader, options);
        }

        public static readonly MethodInfo DeserializeValueDynamicMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(DeserializeValueDynamic), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        private static TValue? DeserializeValueDynamic<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options, TValue? existingValue, bool populate, ViewModelJsonConverter factory, DotvvmSerializationState state)
            where TValue: class
        {
            Debug.Assert(!typeof(TValue).IsSealed);
            var type = existingValue?.GetType();
            if (type is null || type == typeof(TValue))
                return SystemTextJsonUtils.Deserialize<TValue>(ref reader, options);

            // we actually have to do the dynamic dispatch
            // if ViewModelJsonConverter wants to handle the type, we call it directly to support Populate
            // otherwise, just JsonSerializer.Deserialize with the specific type
            if (factory.CanConvert(type))
            {
                var converter = factory.GetDotvvmConverter(type);
                return populate ? (TValue?)converter.PopulateUntyped(ref reader, type, existingValue, options, state)
                                : (TValue?)converter.ReadUntyped(ref reader, type, options, state);
            }

            return (TValue?)JsonSerializer.Deserialize(ref reader, type!, options);
        }

        public static readonly MethodInfo DeserializeViewModelDynamicMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(DeserializeViewModelDynamic), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        private static TVM? DeserializeViewModelDynamic<TVM>(ref Utf8JsonReader reader, JsonSerializerOptions options, TVM? existingValue, bool populate, ViewModelJsonConverter factory, IDotvvmJsonConverter<TVM> defaultConverter, DotvvmSerializationState state)
            where TVM: class
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (existingValue is null)
            {
                return defaultConverter.Read(ref reader, typeof(TVM), options, state);
            }

            var realType = existingValue?.GetType() ?? typeof(TVM);
            if (defaultConverter is {} && realType == typeof(TVM))
            {
                return populate && existingValue is {}
                        ? defaultConverter.Populate(ref reader, typeof(TVM), existingValue, options, state)
                        : defaultConverter.Read(ref reader, typeof(TVM), options, state);
            }

            var converter = factory.GetDotvvmConverter(realType);
            return populate ? (TVM?)converter.PopulateUntyped(ref reader, realType, existingValue, options, state)
                            : (TVM?)converter.ReadUntyped(ref reader, realType, options, state);
        }

        public static readonly MethodInfo ReadEncryptedValueMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(ReadEncryptedValue), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        static Utf8JsonReader ReadEncryptedValue(JsonNode? node)
        {
            var data = new MemoryStream();
            using (var writer = new Utf8JsonWriter(data, new JsonWriterOptions { Indented = false, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
            {
                if (node is {})
                    node.WriteTo(writer);
                else
                    writer.WriteNullValue();
            }
            var reader = new Utf8JsonReader(data.ToSpan());
            reader.Read();
            return reader;
        }

        public static readonly MethodInfo IgnoreValueMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(IgnoreValue), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        static void IgnoreValue(ref Utf8JsonReader reader)
        {
            if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.StartObject)
            {
                reader.Skip();
            }
            reader.Read();
        }

        public static readonly MethodInfo TryReadNextPropertyMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(TryReadNextProperty), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        static string? TryReadNextProperty(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return null;
            }
            var currentProperty = reader.GetString();
            if (currentProperty == null)
            {
                return null;
            }
            reader.Read();
            return currentProperty;
        }

        public static readonly MethodInfo IsNullTokenTypeMethod = typeof(JsonSerializationCodegenFragments).GetMethod(nameof(IsNullTokenType), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        static bool IsNullTokenType(ref Utf8JsonReader reader)
        {
            return reader.TokenType == JsonTokenType.Null;
        }
    }
}
