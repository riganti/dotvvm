using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class StaticCommandArgumentValidationError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Gets or sets the argument name
        /// </summary>
        [JsonProperty("argumentName")]
        public string ArgumentName { get; internal set; }

        /// <summary>
        /// Contains path that can be evaluated on the client side.
        /// E.g.: /Product/Suppliers/2/Name
        /// </summary>
        [JsonProperty("propertyPath")]
        public string? PropertyPath { get; internal set; }

        /// <summary>
        /// Determines whether this error is fully processed
        /// </summary>
        [JsonIgnore]
        internal bool IsResolved { get; set; }

        [JsonConstructor]
        internal StaticCommandArgumentValidationError(string errorMessage, string argumentName, string? propertyPath = null)
        {
            this.PropertyPath = propertyPath;
            this.ArgumentName = argumentName;
            this.ErrorMessage = errorMessage;
        }
    }
}
