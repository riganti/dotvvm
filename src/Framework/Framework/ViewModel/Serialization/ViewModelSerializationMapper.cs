using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Runtime;

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

        public ViewModelSerializationMapper(IValidationRuleTranslator validationRuleTranslator, IViewModelValidationMetadataProvider validationMetadataProvider,
            IPropertySerialization propertySerialization, DotvvmConfiguration configuration)
        {
            this.validationRuleTranslator = validationRuleTranslator;
            this.validationMetadataProvider = validationMetadataProvider;
            this.propertySerialization = propertySerialization;
            this.configuration = configuration;

            HotReloadMetadataUpdateHandler.SerializationMappers.Add(new(this));
        }

        private readonly ConcurrentDictionary<string, ViewModelSerializationMap> serializationMapCache = new();
        public ViewModelSerializationMap GetMap(Type type) => serializationMapCache.GetOrAdd(type.GetTypeHash(), t => CreateMap(type));
        public ViewModelSerializationMap GetMapByTypeId(string typeId) => serializationMapCache[typeId];

        /// <summary>
        /// Creates the serialization map for specified type.
        /// </summary>
        protected virtual ViewModelSerializationMap CreateMap(Type type)
        {
            // constructor which takes properties as parameters
            // if it exists, we always need to recreate the viewmodel
            var valueConstructor = GetConstructor(type);
            return new ViewModelSerializationMap(type, GetProperties(type, valueConstructor), valueConstructor, configuration);
        }

        protected virtual MethodBase? GetConstructor(Type type)
        {
            if (ReflectionUtils.IsPrimitiveType(type) || ReflectionUtils.IsEnumerable(type))
                return null;

            if (type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(c => c.IsDefined(typeof(JsonConstructorAttribute))) is {} factory)
                return factory;

            if (type.IsAbstract)
                return null;

            if (type.GetConstructors().FirstOrDefault(c => c.IsDefined(typeof(JsonConstructorAttribute))) is {} ctor)
                return ctor;
            
            if (type.GetConstructor(Type.EmptyTypes) is {} emptyCtor)
                return emptyCtor;
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

        /// <summary>
        /// Gets the properties of the specified type.
        /// </summary>
        protected virtual IEnumerable<ViewModelPropertyMap> GetProperties(Type type, MethodBase? constructor)
        {
            var ctorParams = constructor?.GetParameters().ToDictionary(p => p.Name.NotNull(), StringComparer.OrdinalIgnoreCase);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Array.Sort(properties, (a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
            foreach (var property in properties)
            {
                if (property.IsDefined(typeof(JsonIgnoreAttribute))) continue;

                var ctorParam = ctorParams?.GetValueOrDefault(property.Name);

                var propertyMap = new ViewModelPropertyMap(
                    property,
                    propertySerialization.ResolveName(property),
                    ProtectMode.None,
                    property.PropertyType,
                    transferToServer: ctorParam is {} || IsSetterSupported(property),
                    transferAfterPostback: property.GetMethod != null && property.GetMethod.IsPublic,
                    transferFirstRequest: property.GetMethod != null && property.GetMethod.IsPublic,
                    populate: ViewModelJsonConverter.CanConvertType(property.PropertyType) && property.GetMethod != null
                );
                propertyMap.ConstructorParameter = ctorParam;
                propertyMap.JsonConverter = GetJsonConverter(property);

                foreach (ISerializationInfoAttribute attr in property.GetCustomAttributes().OfType<ISerializationInfoAttribute>())
                {
                    attr.SetOptions(propertyMap);
                }

                var bindAttribute = property.GetCustomAttribute<BindAttribute>();
                if (bindAttribute != null)
                {
                    propertyMap.Bind(bindAttribute.Direction);
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
        /// <summary>
        /// Returns whether DotVVM serialization supports setter of given property. 
        /// </summary>
        private static bool IsSetterSupported(PropertyInfo property)
        {
            // support all properties of KeyValuePair<,>
            if (property.DeclaringType!.IsGenericType && property.DeclaringType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) return true;

            return property.SetMethod != null && property.SetMethod.IsPublic;
        }

        protected virtual JsonConverter? GetJsonConverter(PropertyInfo property)
        {
            var converterType = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType;
            if (converterType == null)
            {
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
