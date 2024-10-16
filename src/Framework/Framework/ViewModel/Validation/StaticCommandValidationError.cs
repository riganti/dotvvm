using System;
using System.Text.Json.Serialization;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class StaticCommandValidationError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Gets or sets the argument name
        /// </summary>
        [JsonPropertyName("argumentName")]
        public string? ArgumentName { get; internal set; }

        /// <summary>
        /// Contains path that can be evaluated on the client side.
        /// E.g.: /Product/Suppliers/2/Name
        /// </summary>
        [JsonPropertyName("propertyPath")]
        public string? PropertyPath { get; internal set; }

        /// <summary>
        /// Determines property path from either argument name
        /// </summary>
        [JsonIgnore]
        internal Func<DotvvmConfiguration, string>? PropertyPathExtractor { get; set; }

        /// <summary>
        /// Determines whether this error is fully processed
        /// </summary>
        [JsonIgnore]
        internal bool IsResolved { get; set; }

        [JsonConstructor]
        internal StaticCommandValidationError(
            string errorMessage,
            string? argumentName = null,
            string? propertyPath = null,
            Func<DotvvmConfiguration, string>? propertyPathExtractor = null)
        {
            this.PropertyPath = propertyPath;
            this.ArgumentName = argumentName;
            this.ErrorMessage = errorMessage;
            this.PropertyPathExtractor = propertyPathExtractor;
        }
        public override string ToString() => 
            $"StaticCommandValidationError({ErrorMessage}, {ArgumentName}, {(IsResolved ? "" : "un")}resolved {PropertyPath})";
}
}
