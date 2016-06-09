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
    public class ViewModelSerializationMapper: IViewModelSerializationMapper
    {

        private readonly IValidationRuleTranslator validationRuleTranslator;
        private readonly IViewModelValidationMetadataProvider validationMetadataProvider;

        public ViewModelSerializationMapper(DotvvmConfiguration configuration)
        {
            validationRuleTranslator = configuration.ServiceLocator.GetService<IValidationRuleTranslator>();
            validationMetadataProvider = configuration.ServiceLocator.GetService<IViewModelValidationMetadataProvider>();
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

                var bindAttribute = property.GetCustomAttribute<BindAttribute>();
                if (bindAttribute != null)
                {
                    propertyMap.TransferAfterPostback = bindAttribute.Direction.HasFlag(Direction.ServerToClientPostback);
                    propertyMap.TransferFirstRequest = bindAttribute.Direction.HasFlag(Direction.ServerToClientFirstRequest);
                    propertyMap.TransferToServer = bindAttribute.Direction.HasFlag(Direction.ClientToServerNotInPostbackPath) || bindAttribute.Direction.HasFlag(Direction.ClientToServerInPostbackPath);
                    propertyMap.TransferToServerOnlyInPath = !bindAttribute.Direction.HasFlag(Direction.ClientToServerNotInPostbackPath) && propertyMap.TransferToServer;
                }

                var viewModelProtectionAttribute = property.GetCustomAttribute<ProtectAttribute>();
                if (viewModelProtectionAttribute != null)
                {
                    propertyMap.ViewModelProtection = viewModelProtectionAttribute.Settings;
                }

                var clientExtenderList = property.GetCustomAttributes<ClientExtenderAttribute>();
                if (clientExtenderList.Any())
                {
                    clientExtenderList
                        .OrderBy(c => c.Order)
                        .ToList()
                        .ForEach(extender => propertyMap.ClientExtenders.Add(new ClientExtenderInfo() { Name = extender.Name, Parameter = extender.Parameter }));
                }

                var validationAttributes = validationMetadataProvider.GetAttributesForProperty(property);
                propertyMap.ValidationRules = validationRuleTranslator.TranslateValidationRules(property, validationAttributes).ToList();

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
