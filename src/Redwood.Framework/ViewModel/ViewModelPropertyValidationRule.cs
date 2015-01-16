using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelPropertyValidationRule
    {

        [JsonProperty("ruleName")]
        public string RuleName { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("parameters")]
        public object[] Parameters { get; set; }

    }
}