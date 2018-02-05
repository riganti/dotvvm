using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// Builds serialization maps that are used during the JSON serialization.
    /// </summary>
    public class ViewModelSerializationMapper : IViewModelSerializationMapper
    {
        private readonly IValidationRuleTranslator validationRuleTranslator;
        private readonly IViewModelValidationMetadataProvider validationMetadataProvider;

        public ViewModelSerializationMapper(IValidationRuleTranslator validationRuleTranslator, IViewModelValidationMetadataProvider validationMetadataProvider)
        {
            this.validationRuleTranslator = validationRuleTranslator;
            this.validationMetadataProvider = validationMetadataProvider;
        }

        private readonly ConcurrentDictionary<Type, ViewModelSerializationMap> serializationMapCache = new ConcurrentDictionary<Type, ViewModelSerializationMap>();
        public ViewModelSerializationMap GetMap(Type type) => serializationMapCache.GetOrAdd(type, CreateMap);

        /// <summary>
        /// Creates the serialization map for specified type.
        /// </summary>
        protected virtual ViewModelSerializationMap CreateMap(Type type)
        {
            return new ViewModelSerializationMap(type, GetProperties(type));
        }

        /// <summary>
        /// Gets the properties of the specified type.
        /// </summary>
        protected virtual IEnumerable<ViewModelPropertyMap> GetProperties(Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name))
            {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                var propertyMap = new ViewModelPropertyMap()
                {
                    PropertyInfo = property,
                    Name = property.Name,
                    ViewModelProtection = ProtectMode.None,
                    Type = property.PropertyType,
                    TransferAfterPostback = property.GetMethod != null && property.GetMethod.IsPublic,
                    TransferFirstRequest = property.GetMethod != null && property.GetMethod.IsPublic,
                    TransferToServer = property.SetMethod != null && property.SetMethod.IsPublic,
                    JsonConverter = GetJsonConverter(property),
                    Populate = ViewModelJsonConverter.IsComplexType(property.PropertyType) && !ViewModelJsonConverter.IsEnumerable(property.PropertyType) && property.GetMethod != null
                };

                foreach (ISerializationInfoAttribute attr in property.GetCustomAttributes().OfType<ISerializationInfoAttribute>())
                {
                    attr.SetOptions(propertyMap);
                }

                var bindAttribute = property.GetCustomAttribute<BindAttribute>();
                if (bindAttribute != null)
                {
                    propertyMap.Bind(bindAttribute.Direction);
                    if (!string.IsNullOrEmpty(bindAttribute.Name))
                    {
                        propertyMap.Name = bindAttribute.Name;
                    }
                }

                if (string.IsNullOrEmpty(bindAttribute?.Name))
                {
                    // use JsonProperty name if Bind attribute is not present or doesn't specify it
                    var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                    if (!string.IsNullOrEmpty(jsonPropertyAttribute?.PropertyName))
                    {
                        propertyMap.Name = jsonPropertyAttribute.PropertyName;
                    }
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

        protected virtual JsonConverter GetJsonConverter(PropertyInfo property)
        {
            var converterType = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType;
            if (converterType == null)
            {
                return null;
            }
            try
            {
                return (JsonConverter)Activator.CreateInstance(converterType);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Cannot create an instance of {converterType} converter! Please check that this class have a public parameterless constructor.", ex);
            }
        }
    }
}
