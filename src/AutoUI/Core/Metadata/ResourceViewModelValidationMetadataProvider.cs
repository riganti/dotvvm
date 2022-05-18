using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.AutoUI.Metadata
{
    /// <summary>
    /// Validation attribmetadata provider which loads missing error messages from the RESX file.
    /// </summary>
    public class ResourceViewModelValidationMetadataProvider : IViewModelValidationMetadataProvider
    {
        private readonly IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider;
        private readonly IViewModelValidationMetadataProvider baseValidationMetadataProvider;
        private ConcurrentDictionary<PropertyInfoCulturePair, List<ValidationAttribute>> cache = new ConcurrentDictionary<PropertyInfoCulturePair, List<ValidationAttribute>>();

        private ResourceManager errorMessages;

        /// <summary>
        /// Gets the type of the resource file that contains the default error message patterns.
        /// The resource key is the name of the attribute without the trailing Attribute (e.g. Required for RequiredAttribute etc.).
        /// </summary>
        public Type ErrorMessagesResourceFileType { get; }


        public ResourceViewModelValidationMetadataProvider(Type errorMessagesResourceFileType, IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider, IViewModelValidationMetadataProvider baseValidationMetadataProvider)
        {
            ErrorMessagesResourceFileType = errorMessagesResourceFileType;
            errorMessages = new ResourceManager(errorMessagesResourceFileType);

            this.propertyDisplayMetadataProvider = propertyDisplayMetadataProvider;
            this.baseValidationMetadataProvider = baseValidationMetadataProvider;
        }

        /// <summary>
        /// Gets validation attributes for the specified property.
        /// </summary>
        public IEnumerable<ValidationAttribute> GetAttributesForProperty(PropertyInfo property)
        {
            return cache.GetOrAdd(new PropertyInfoCulturePair(CultureInfo.CurrentUICulture, property), GetAttributesForPropertyCore);
        }

        /// <summary>
        /// Determines validation attributes for the specified property and loads missing error messages from the resource file.
        /// </summary>
        private List<ValidationAttribute> GetAttributesForPropertyCore(PropertyInfoCulturePair pair)
        {
            // determine property name
            var propertyDisplayName = propertyDisplayMetadataProvider.GetPropertyMetadata(pair.PropertyInfo).GetDisplayName().Localize();

            // process all validation attributes
            var results = new List<ValidationAttribute>();
            foreach (var attribute in baseValidationMetadataProvider.GetAttributesForProperty(pair.PropertyInfo))
            {
                if (string.IsNullOrEmpty(attribute.ErrorMessage) && string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
                {
                    // the error message has not been set, determine new error message
                    var clone = CloneAttribute(attribute);

                    // determine error message
                    var errorMessage = GetErrorMessage(attribute, propertyDisplayName);

                    // add the clone to the list
                    clone.ErrorMessage = errorMessage;
                    results.Add(clone);
                }
                else
                {
                    results.Add(attribute);
                }
            }

            return results;
        }

        /// <summary>
        /// Creates a copy of the specified validation attribute instance.
        /// </summary>
        protected virtual ValidationAttribute CloneAttribute(ValidationAttribute attribute)
        {
            return (ValidationAttribute)attribute.GetType()
                .GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(attribute, null)!;
        }
        
        /// <summary>
        /// Gets the error message for the specified attribute.
        /// </summary>
        public virtual string GetErrorMessage(ValidationAttribute attribute, string propertyDisplayName)
        {
            var attributeName = attribute.GetType().Name;
            if (attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase))
            {
                attributeName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);
            }

            return string.Format(GetDefaultErrorMessagePattern(attributeName), propertyDisplayName);
        }

        /// <summary>
        /// Gets the default error message pattern for the specified attribute.
        /// </summary>
        protected virtual string GetDefaultErrorMessagePattern(string attributeName)
        {
            return errorMessages.GetString(attributeName) ?? errorMessages.GetString("Unknown") ?? "Error";
        }
        

        private struct PropertyInfoCulturePair
        {
            public readonly CultureInfo Culture;
            public readonly PropertyInfo PropertyInfo;

            public PropertyInfoCulturePair(CultureInfo culture, PropertyInfo propertyInfo)
            {
                Culture = culture;
                PropertyInfo = propertyInfo;
            }
        }
    }
}
