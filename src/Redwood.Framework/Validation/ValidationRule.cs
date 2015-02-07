using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace Redwood.Framework.Validation
{
    public class ValidationRule
    {
        [JsonProperty("propertyName")]
        public string PropertyName { get; set; }

        [JsonIgnore]
        public PropertyInfo Property { get; set; }

        [JsonProperty("ruleName")]
        public string RuleName { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("parameters")]
        public object[] Parameters { get; set; }

        [JsonIgnore]
        public Func<RedwoodValidationContext, bool> ValidationFunc { get; set; }
    }
}