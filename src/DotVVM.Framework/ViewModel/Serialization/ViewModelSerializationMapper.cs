#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;

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
        }

        private readonly ConcurrentDictionary<Type, ViewModelSerializationMap> serializationMapCache = new ConcurrentDictionary<Type, ViewModelSerializationMap>();
        public ViewModelSerializationMap GetMap(Type type) => serializationMapCache.GetOrAdd(type, CreateMap);

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
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name))
            {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                var propertyMap = new ViewModelPropertyMap() {
                    PropertyInfo = property,
                    Name = propertySerialization.ResolveName(property),
                    ViewModelProtection = ProtectMode.None,
                    Type = property.PropertyType,
                    TransferAfterPostback = property.GetMethod != null && property.GetMethod.IsPublic,
                    TransferFirstRequest = property.GetMethod != null && property.GetMethod.IsPublic,
                    TransferToServer = property.SetMethod != null && property.SetMethod.IsPublic,
                    JsonConverter = GetJsonConverter(property),
                    Populate = ReflectionUtils.IsComplexType(property.PropertyType) && !ReflectionUtils.IsEnumerable(property.PropertyType) && !ReflectionUtils.IsObject(property.PropertyType) && property.GetMethod != null
                };

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

                propertyMap.ClientExtenders =
                    property.GetCustomAttributes<ClientExtenderAttribute>()
                    .OrderBy(c => c.Order)
                    .Select(extender => new ClientExtenderInfo() { Name = extender.Name, Parameter = extender.Parameter })
                    .ToList();

                var validationAttributes = validationMetadataProvider.GetAttributesForProperty(property);
                propertyMap.ValidationRules = validationRuleTranslator.TranslateValidationRules(property, validationAttributes).ToList();

                propertyMap.ValidateSettings();

                yield return propertyMap;
            }
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
    }
}
