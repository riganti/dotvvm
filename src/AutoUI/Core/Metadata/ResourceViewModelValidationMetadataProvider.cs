using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.AutoUI.Metadata
{
    /// <summary>
    /// Validation metadata provider which loads missing error messages from the RESX file.
    /// </summary>
    public class ResourceViewModelValidationMetadataProvider : IViewModelValidationMetadataProvider
    {
        private readonly IViewModelValidationMetadataProvider baseValidationMetadataProvider;
        private readonly ConcurrentDictionary<PropertyInfo, List<ValidationAttribute>> cache = new();
        private readonly ResourceManager errorMessages;
        private static readonly FieldInfo internalErrorMessageField;

        /// <summary>
        /// Gets the type of the resource file that contains the default error message patterns.
        /// The resource key is the name of the attribute without the trailing Attribute (e.g. Required for RequiredAttribute etc.).
        /// </summary>
        public Type ErrorMessagesResourceFileType { get; }


        public ResourceViewModelValidationMetadataProvider(Type errorMessagesResourceFileType, IViewModelValidationMetadataProvider baseValidationMetadataProvider)
        {
            ErrorMessagesResourceFileType = errorMessagesResourceFileType;
            errorMessages = new ResourceManager(errorMessagesResourceFileType);

            this.baseValidationMetadataProvider = baseValidationMetadataProvider;
        }

        static ResourceViewModelValidationMetadataProvider()
        {
            internalErrorMessageField = typeof(ValidationAttribute).GetField("_errorMessage", BindingFlags.Instance | BindingFlags.NonPublic).NotNull();
        }

        /// <summary>
        /// Gets validation attributes for the specified property.
        /// </summary>
        public IEnumerable<ValidationAttribute> GetAttributesForProperty(PropertyInfo property)
        {
            return cache.GetOrAdd(property, GetAttributesForPropertyCore);
        }

        /// <summary>
        /// Determines validation attributes for the specified property and loads missing error messages from the resource file.
        /// </summary>
        private List<ValidationAttribute> GetAttributesForPropertyCore(PropertyInfo property)
        {
            // process all validation attributes
            var results = new List<ValidationAttribute>();
            foreach (var attribute in baseValidationMetadataProvider.GetAttributesForProperty(property))
            {
                if (HasDefaultErrorMessage(attribute) && GetErrorMessageKey(attribute) is {} errorMessageKey)
                {
                    var clone = CloneAttribute(attribute);

                    // update the attribute
                    clone.ErrorMessageResourceType = ErrorMessagesResourceFileType;
                    clone.ErrorMessageResourceName = errorMessageKey;

                    results.Add(clone);
                }
                else
                {
                    results.Add(attribute);
                }
            }

            return results;
        }

        private bool HasDefaultErrorMessage(ValidationAttribute attribute)
        {
            return string.IsNullOrEmpty((string)internalErrorMessageField.GetValue(attribute))
                && attribute.ErrorMessageResourceType == null
                && string.IsNullOrEmpty(attribute.ErrorMessageResourceName);
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
        public virtual string? GetErrorMessageKey(ValidationAttribute attribute)
        {
            var attributeName = attribute.GetType().Name;
            if (attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase))
            {
                attributeName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);
            }

            return errorMessages.GetString(attributeName) != null ? attributeName :
                    errorMessages.GetString("Unknown") != null ? "Unknown" :
                    null;
        }
    }
}
