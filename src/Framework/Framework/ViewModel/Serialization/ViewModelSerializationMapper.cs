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
            return new ViewModelSerializationMap(type, GetProperties(type), configuration);
        }

        /// <summary>
        /// Gets the properties of the specified type.
        /// </summary>
        protected virtual IEnumerable<ViewModelPropertyMap> GetProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Array.Sort(properties, (a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
            foreach (var property in properties)
            {
                if (property.IsDefined(typeof(JsonIgnoreAttribute))) continue;

                var propertyMap = new ViewModelPropertyMap(
                    property,
                    propertySerialization.ResolveName(property),
                    ProtectMode.None,
                    property.PropertyType,
                    transferToServer: IsSetterSupported(property),
                    transferAfterPostback: property.GetMethod != null && property.GetMethod.IsPublic,
                    transferFirstRequest: property.GetMethod != null && property.GetMethod.IsPublic,
                    populate: ViewModelJsonConverter.CanConvertType(property.PropertyType) && property.GetMethod != null
                );
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
            if (property.DeclaringType.IsGenericType && property.DeclaringType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) return true;

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
            var freshProperties = GetProperties(t).Select(p => (p.Name, p.Type, p.BindDirection, p.ViewModelProtection)).ToHashSet();

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
