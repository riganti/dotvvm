using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq.Expressions;

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
        
        [JsonProperty("groups")]
        public string Groups { get; set; }

        [JsonIgnore]
        public Func<RedwoodValidationContext, bool> ValidationFunc { get; set; }

        public ValidationRule()
        {
            Groups = "*";
        }

        public static ValidationRule Create<T, TProp>(Expression<Func<T, TProp>> prop, string errorMessage = null, string rule = null, object[] parameters = null, Func<RedwoodValidationContext, bool> valFunc = null, string groups = "*")
        {
            var p = prop.Body as MemberExpression;
            var r = new ValidationRule()
            {
                ErrorMessage = errorMessage,
                RuleName = rule,
                Parameters = parameters ?? new object[0],
                ValidationFunc = valFunc ?? (c => true),
                Groups = groups
            };
            if(p != null)
            {
                r.Property = p.Member as PropertyInfo;
                r.PropertyName = p.Member.Name;
            }
            return r;
        }
    }
}