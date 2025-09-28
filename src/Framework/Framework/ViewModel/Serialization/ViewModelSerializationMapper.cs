using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.ViewModel.Validation;
using System.Collections.Concurrent;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Runtime;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using DotVVM.Framework.Compilation.Javascript;
using FastExpressionCompiler;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Builds serialization maps that are used during the JSON serialization.
    /// </summary>
    public class ViewModelSerializationMapper : IViewModelSerializationMapper
    {
        private readonly IValidationRuleTranslator validationRuleTranslator;
        private readonly IViewModelValidationMetadataProvider validationMetadataProvider;
        private readonly IPropertySerialization propertySerialization;
        private readonly DotvvmConfiguration configuration;
        private readonly IDotvvmJsonOptionsProvider jsonOptions;
        private readonly ILogger<ViewModelSerializationMapper>? logger;

        public ViewModelSerializationMapper(IValidationRuleTranslator validationRuleTranslator, IViewModelValidationMetadataProvider validationMetadataProvider,
            IPropertySerialization propertySerialization, DotvvmConfiguration configuration, IDotvvmJsonOptionsProvider jsonOptions, ILogger<ViewModelSerializationMapper>? logger = null)
        {
            this.validationRuleTranslator = validationRuleTranslator;
            this.validationMetadataProvider = validationMetadataProvider;
            this.propertySerialization = propertySerialization;
            this.configuration = configuration;
            this.jsonOptions = jsonOptions;
            this.logger = logger;

            HotReloadMetadataUpdateHandler.SerializationMappers.Add(new(this));
        }

        private readonly ConcurrentDictionary<string, ViewModelSerializationMap> serializationMapCache = new();
        public ViewModelSerializationMap GetMap(Type type) => serializationMapCache.GetOrAdd(type.GetTypeHash(), t => CreateMap(type));
        public ViewModelSerializationMap<T> GetMap<T>() => (ViewModelSerializationMap<T>)serializationMapCache.GetOrAdd(typeof(T).GetTypeHash(), _ => CreateMap<T>());
        public ViewModelSerializationMap GetMapByTypeId(string typeId) => serializationMapCache[typeId];

        /// <summary>
        /// Creates the serialization map for specified type.
        /// </summary>
        protected virtual ViewModelSerializationMap CreateMap(Type type) =>
            (ViewModelSerializationMap)CreateMapGenericMethod.MakeGenericMethod(type).Invoke(this, Array.Empty<object>())!;
        static MethodInfo CreateMapGenericMethod =
            (MethodInfo)MethodFindingHelper.GetMethodFromExpression(() => default(ViewModelSerializationMapper)!.CreateMap<MethodFindingHelper.Generic.T>());
        protected virtual ViewModelSerializationMap<T> CreateMap<T>()
        {
            var type = typeof(T);
            // constructor which takes properties as parameters
            // if it exists, we always need to recreate the viewmodel
            var valueConstructor = GetConstructor(type);
            return new ViewModelSerializationMap<T>(GetProperties(type, valueConstructor), valueConstructor, jsonOptions.ViewModelJsonOptions, configuration);
        }

        protected virtual MethodBase? GetConstructor(Type type)
        {
            if (ReflectionUtils.IsPrimitiveType(type) || ReflectionUtils.IsEnumerable(type))
                return null;

            if (type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(c => SerialiationMapperAttributeHelper.IsJsonConstructor(c)) is {} factory)
                return factory;

            if (type.IsAbstract)
                return null;

            if (type.GetConstructors().FirstOrDefault(c => SerialiationMapperAttributeHelper.IsJsonConstructor(c)) is {} ctor)
                return ctor;
            
            if (type.GetConstructor(Type.EmptyTypes) is {} emptyCtor)
                return emptyCtor;
            if (ReflectionUtils.IsTupleLike(type))
            {
                var ctors = type.GetConstructors();
                if (ctors.Length == 1)
                    return ctors[0];
                else
                    throw new NotSupportedException($"Type {type.FullName} is a tuple-like type, but it has {ctors.Length} constructors.");
            }
            return GetRecordConstructor(type);
        }

        protected static MethodBase? GetRecordConstructor(Type t)
        {
            if (t.IsAbstract)
                return null;
            // https://sharplab.io/#v2:EYLgxg9gTgpgtADwGwBYA0AbEAzAzmgFxCgFcA7AHwgAcYyACAZQE9cCYBbAWACga6mrdhwB0AGQCWZAI68CzWvQDC9ALz0A3vQBCIelIIBuZXoPGwpsgXoBfOQpj0AqmvoANehXoBNehGz6Vry81FAG2AwARACCkcE8GDDW1urytP4APEoAfPGJ1gCGBARQrgQiAOJJSiRsEBzRxWHAJOy4ACJFBQAUAJQi0WTM3djk9AX0cNnjA00SLewAKg4iAGIkGBgAcgUcjuqRALISYFAQuP7lq4wAFgVQ1CJK0DBP9dQSGEUSEGSHBdQPmQAOaNErzVowSL0ABkMOUvwAbjAoOVFhAAJJWADMACZugU3mQ2KQwARoNEoMCSHsrLgANoABgAuiIAGoFDAkGC9Vy43ohMJWCL0SJFEp6ACksXGTSAA=
            // F# record or single case discriminated union (multi case is abstract)
            if (t.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute" && Convert.ToInt32(a.ConstructorArguments[0].Value) is 1 or 2))
            {
                return t.GetConstructors().Single();
            }
            // TODO: F# normal type, it's not possible AFAIK to add attribute to the default constructor

            // find constructor which matches Deconstruct method
            var deconstruct = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.Name == "Deconstruct").ToArray();
            if (deconstruct.Length == 0)
                return null;
            var constructors =
               (from c in t.GetConstructors()
                from d in deconstruct
                where c.GetParameters().Select(p => p.Name).SequenceEqual(d.GetParameters().Select(p => p.Name)) &&
                      c.GetParameters().Select(p => unwrapByRef(p.ParameterType)).SequenceEqual(d.GetParameters().Select(p => unwrapByRef(p.ParameterType)))
                select c).ToArray();

            if (constructors.Length == 1)
                return constructors[0];

            return null;

            static Type unwrapByRef(Type t) => t.IsByRef ? t.GetElementType()! : t;
        }

        protected virtual MemberInfo[] ResolveShadowing(Type type, MemberInfo[] members)
        {
            var shadowed = new Dictionary<string, MemberInfo>();
            foreach (var member in members)
            {
                if (!shadowed.ContainsKey(member.Name))
                {
                    shadowed.Add(member.Name, member);
                    continue;
                }
                var previous = shadowed[member.Name];
                if (member.DeclaringType == previous.DeclaringType)
                    throw new InvalidOperationException($"Two or more members named '{member.Name}' on type '{member.DeclaringType!.ToCode()}' are not allowed.");
                var (inherited, replacing) = member.DeclaringType!.IsAssignableFrom(previous.DeclaringType!) ? (member, previous) : (previous, member);
                var inheritedType = inherited.GetResultType();
                var replacingType = replacing.GetResultType();

                // collections are special case, since everything is serialized as array, we don't have to care about the actual type, only the element type
                // this is neccessary for IGridViewDataSet hierarchy to work...
                while (ReflectionUtils.IsCollection(inheritedType) && ReflectionUtils.IsCollection(replacingType))
                {
                    inheritedType = ReflectionUtils.GetEnumerableType(inheritedType) ?? typeof(object);
                    replacingType = ReflectionUtils.GetEnumerableType(replacingType) ?? typeof(object);
                }

                if (inheritedType.IsAssignableFrom(replacingType))
                {
                    shadowed[member.Name] = replacing;
                }
                else
                {
                    throw new InvalidOperationException($"Detected forbidden member shadowing of '{inherited.DeclaringType.ToCode(stripNamespace: true)}.{inherited.Name}: {inherited.GetResultType().ToCode(stripNamespace: true)}' by '{replacing.DeclaringType.ToCode(stripNamespace: true)}.{replacing.Name}: {replacing.GetResultType().ToCode(stripNamespace: true)}' while building serialization map for '{type.ToCode(stripNamespace: true)}'");
                }
            }
            return shadowed.Values.ToArray();
        }

        private static bool TryGetDotvvmConverter(Type propertyType, IDotvvmJsonOptionsProvider jsonOptions)
        {
            try
            {
                var converter = jsonOptions.ViewModelJsonOptions.GetConverter(propertyType);
                return converter is IDotvvmJsonConverter;
            }
            catch
            {
                // GetConverter might throw for types without metadata
                return false;
            }
        }

        /// <summary>
        /// Gets the properties of the specified type.
        /// </summary>
        protected virtual IEnumerable<ViewModelPropertyMap> GetProperties(Type type, MethodBase? constructor)
        {
            var ctorParams = constructor?.GetParameters().ToDictionary(p => p.Name.NotNull(), StringComparer.OrdinalIgnoreCase);

            var properties = type.GetAllMembers(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(m => m is PropertyInfo or FieldInfo)
                                 .ToArray();
            properties = ResolveShadowing(type, properties);
            Array.Sort(properties, (a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
            foreach (MemberInfo property in properties)
            {
                var bindAttribute = property.GetCustomAttribute<BindAttribute>();
                var include = !SerialiationMapperAttributeHelper.IsJsonIgnore(property) && bindAttribute is not { Direction: Direction.None };
                if (property is FieldInfo)
                {
                    // fields are ignored by default, unless marked with [Bind(not None)], [JsonInclude] or defined in ValueTuple<...>
                    include = include && (
                        !(bindAttribute is null or { Direction: Direction.None }) ||
                        property.IsDefined(typeof(JsonIncludeAttribute)) ||
                        (type.IsGenericType && type.FullName!.StartsWith("System.ValueTuple`"))
                    );
                }
                if (!include) continue;

                var (propertyType, canGet, canSet) = property switch {
                    PropertyInfo p => (p.PropertyType, p.GetMethod is { IsPublic: true }, p.SetMethod is { IsPublic: true }),
                    FieldInfo f => (f.FieldType, true, !f.IsInitOnly && !f.IsLiteral),
                    _ => throw new NotSupportedException()
                };

                var ctorParam = ctorParams?.GetValueOrDefault(property.Name);

                var propertyMap = new ViewModelPropertyMap(
                    property,
                    propertySerialization.ResolveName(property),
                    ProtectMode.None,
                    propertyType,
                    transferToServer: ctorParam is {} || canSet,
                    transferAfterPostback: canGet,
                    transferFirstRequest: canGet,
                    populate: ((ViewModelJsonConverter.CanConvertType(propertyType) || propertyType == typeof(object)) && canGet) ||
                             (canGet && TryGetDotvvmConverter(propertyType, jsonOptions))
                );
                propertyMap.ConstructorParameter = ctorParam;
                propertyMap.JsonConverter = GetJsonConverter(property);
                propertyMap.AllowDynamicDispatch = propertyMap.JsonConverter is null && (propertyType.IsAbstract || propertyType == typeof(object));

                if (type.IsDefined(typeof(DotvvmSerializationAttribute), true))
                {
                    var typeSerializationAttribute = type.GetCustomAttribute<DotvvmSerializationAttribute>()!;
                    propertyMap.AllowDynamicDispatch = typeSerializationAttribute.AllowsDynamicDispatch(propertyMap.AllowDynamicDispatch);
                }

                foreach (ISerializationInfoAttribute attr in property.GetCustomAttributes().OfType<ISerializationInfoAttribute>())
                {
                    attr.SetOptions(propertyMap);
                }

                if (bindAttribute != null)
                {
                    propertyMap.Bind(bindAttribute.Direction);
                    propertyMap.AllowDynamicDispatch = bindAttribute.AllowsDynamicDispatch(propertyMap.AllowDynamicDispatch);

                    if (propertyMap.AllowDynamicDispatch && propertyMap.JsonConverter is {})
                        throw new NotSupportedException($"Property '{property.DeclaringType?.ToCode()}.{property.Name}' cannot use dynamic dispatch, because it has an explicit JsonConverter.");
                }

                var viewModelProtectionAttribute = property.GetCustomAttribute<ProtectAttribute>();
                if (viewModelProtectionAttribute != null)
                {
                    propertyMap.ViewModelProtection = viewModelProtectionAttribute.Settings;
                }

                propertyMap.ClientExtenders.AddRange(
                    property.GetCustomAttributes<ClientExtenderAttribute>()
                    .OrderBy(c => c.Order)
                    .Select(extender => new ClientExtenderInfo(extender.Name, extender.Parameter))
                );

                var validationAttributes = validationMetadataProvider.GetAttributesForProperty(property);
                propertyMap.ValidationRules.AddRange(validationRuleTranslator.TranslateValidationRules(property, validationAttributes));

                propertyMap.ValidateSettings();

                yield return propertyMap;
            }
        }

        protected virtual JsonConverter? GetJsonConverter(MemberInfo property)
        {
            var converterType = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType;
            if (converterType == null)
            {
                if (SerialiationMapperAttributeHelper.HasNewtonsoftJsonConvert(property))
                {
                    this.logger?.LogWarning($"Property {property.DeclaringType?.FullName}.{property.Name} has Newtonsoft.Json.JsonConverterAttribute, which is not supported by DotVVM anymore. Use System.Text.Json.Serialization.JsonConverterAttribute instead.");
                }
                return null;
            }
            try
            {
                return (JsonConverter?)Activator.CreateInstance(converterType);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Cannot create an instance of {converterType} converter! Please check that this class have a public parameterless constructor.", ex);
            }
        }

        /// <summary> Remove this type from cache, unless it has been significantly changed. Used only for hot-reload, nothing should rely on this working properly. </summary>
        /// <returns>true if the cached was cleared successfully, false if the cached could not be cleared for some reason. </returns>
        internal bool ClearCache(Type t)
        {
            var hash = t.GetTypeHash();
            if (!this.serializationMapCache.TryGetValue(hash, out var cachedItem))
                return true;
            
            // we want to check if the cached metadata was changed manually
            // it there are changes, we don't clear the cache as that would cause more trouble than leaving outdated metadata there
            var freshProperties = GetProperties(t, GetConstructor(t)).Select(p => (p.Name, p.Type, p.BindDirection, p.ViewModelProtection)).ToHashSet();

            // if freshly mapping the type produces the same result, no need to clear the cache
            if (freshProperties.SetEquals(cachedItem.OriginalProperties))
                return true;

            var currentProperties = cachedItem.Properties.Select(p => (p.Name, p.Type, p.BindDirection, p.ViewModelProtection)).ToHashSet();
            if (currentProperties.SetEquals(cachedItem.OriginalProperties))
            {
                this.serializationMapCache.TryRemove(hash, out _);
                return true;
            }
            return false;
        }
    }
}
