using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Contains path that can be evaluated on the client side.
        /// E.g.: /Product/Suppliers/2/Name
        /// </summary>
        [JsonPropertyName("propertyPath")]
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

        public override string ToString() => 
            $"ViewModelValidationError({ErrorMessage}, {PropertyPath}{(IsResolved ? "" : ", relative in object " + TargetObject)})";
    }
}
