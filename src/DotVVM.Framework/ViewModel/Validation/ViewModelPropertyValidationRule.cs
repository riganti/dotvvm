using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelPropertyValidationRule
    {
        [JsonProperty("ruleName")]
        public string? ClientRuleName { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage => SourceValidationAttribute.FormatErrorMessage(PropertyName);

        [JsonProperty("parameters")]
        public object?[] Parameters { get; set; }

        [JsonIgnore]
        public ValidationAttribute SourceValidationAttribute { get; set; }

        [JsonIgnore]
        public string PropertyName => PropertyNameResolver?.Invoke() ?? StaticPropertyName;

        [JsonIgnore]
        public string StaticPropertyName { get; set; }

        [JsonIgnore]
        public Func<string>? PropertyNameResolver { get; set; }

        public ViewModelPropertyValidationRule(ValidationAttribute sourceValidationAttribute, string staticPropertyName,
            string? clientRuleName = null, params object?[] parameters)
        {
            SourceValidationAttribute = sourceValidationAttribute ?? throw new ArgumentNullException(nameof(sourceValidationAttribute));
            StaticPropertyName = staticPropertyName ?? throw new ArgumentNullException(nameof(staticPropertyName));
            ClientRuleName = clientRuleName;
            Parameters = parameters;
        }
    }
}
