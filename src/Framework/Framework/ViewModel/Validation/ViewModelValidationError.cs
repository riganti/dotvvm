#nullable disable
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationError
    {
        /// <summary>
        /// Object affected by this validation error
        /// </summary>
        [JsonIgnore]
        public object TargetObject { get; set; }

        /// <summary>
        /// Contains path that can be evaluated on the client side.
        /// E.g.: Product().Suppliers()[2].Name
        /// </summary>
        [JsonProperty("propertyPath")]
        public string PropertyPath { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
