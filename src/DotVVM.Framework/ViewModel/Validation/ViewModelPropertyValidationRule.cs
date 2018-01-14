using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelPropertyValidationRule
    {
        [JsonProperty("ruleName")]
        public string ClientRuleName { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage => SourceValidationAttribute.FormatErrorMessage(PropertyName);

        [JsonProperty("parameters")]
        public object[] Parameters { get; set; }

        [JsonIgnore]
        public ValidationAttribute SourceValidationAttribute { get; set; }

        [JsonIgnore]
        public string PropertyName { get; set; }

        public ViewModelPropertyValidationRule(ValidationAttribute sourceValidationAttribute, string propertyName,
            string clientRuleName = null, params object[] parameters)
        {
            SourceValidationAttribute = sourceValidationAttribute ?? throw new ArgumentNullException(nameof(sourceValidationAttribute));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            ClientRuleName = clientRuleName;
            Parameters = parameters;
        }
    }
}
