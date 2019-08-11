using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Routing.Constraints
{
    public class RegexRouteParameterConstraint : IRouteParameterConstraint
    {
        private readonly string constraintName;

        public RegexRouteParameterConstraint(string constraintName)
        {
            this.constraintName = constraintName;
        }

        public string GetPartRegex(string parameter)
        {
            if (parameter.StartsWith("^"))
            {
                throw new ArgumentException("Regex in route constraint should not start with `^`, it's always looking for full-match.");
            }
            if (parameter.EndsWith("$"))
            {
                throw new ArgumentException("Regex in route constraint should not end with `$`, it's always looking for full-match.");
            }
            return parameter;
        }

        public ParameterParseResult ParseValue(object value, string parameter)
        {
            if (!(value is string))
            {
                return ParameterParseResult.Failed;
            }

            return ParameterParseResult.Create(value);
        }

        public Type PredictType(Type valueType)
        {
            if (valueType == typeof(string))
            {
                return typeof(string);
            }
            else
            {
                throw new NotSupportedException($"The route constraint '{constraintName}' accepts only string values! Make sure the constraint is specified as a parameter's first constraint.");
            }
        }
    }
}
