using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Contains path that can be evaluated on the client side.
        /// E.g.: /Product/Suppliers/2/Name
        /// </summary>
        [JsonProperty("propertyPath")]
        public string? PropertyPath { get; internal set; }

        /// <summary>
        /// Object affected by this validation error
        /// </summary>
        [JsonIgnore]
        internal object? TargetObject { get; set; }

        /// <summary>
        /// Determines whether this error is fully processed
        /// </summary>
        [JsonIgnore]
        internal bool IsResolved { get; set; }

        [JsonConstructor]
        internal ViewModelValidationError(string errorMessage, string? propertyPath = null, object? targetObject = null)
        {
            this.PropertyPath = propertyPath;
            this.ErrorMessage = errorMessage;
            this.TargetObject = targetObject;
        }
    }
}
