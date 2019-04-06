using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Routing.Constraints
{
    public class PatternRouteParameterConstraint : IRouteParameterConstraint
    {
        private readonly string constraintName;
        private readonly string partRegex;

        public PatternRouteParameterConstraint(string constraintName, string partRegex)
        {
            this.constraintName = constraintName;
            this.partRegex = partRegex;
        }

        public string GetPartRegex(string parameter)
        {
            return partRegex;
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
