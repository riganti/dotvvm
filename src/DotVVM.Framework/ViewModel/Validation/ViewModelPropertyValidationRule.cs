using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelPropertyValidationRule
    {

        [JsonProperty("ruleName")]
        public string ClientRuleName { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage =>  SourceValidationAttribute.FormatErrorMessage(PropertyName);

        [JsonProperty("parameters")]
        public object[] Parameters { get; set; }

        [JsonIgnore]
        public ValidationAttribute SourceValidationAttribute { get; set; }

        [JsonIgnore]
        public string PropertyName { get; set; }
    }
}
