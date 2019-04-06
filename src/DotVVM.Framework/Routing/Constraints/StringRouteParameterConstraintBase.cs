using System;

namespace DotVVM.Framework.Routing.Constraints
{
    public abstract class StringRouteParameterConstraintBase : IRouteParameterConstraint
    {
        protected readonly string constraintName;

        public StringRouteParameterConstraintBase(string constraintName)
        {
            this.constraintName = constraintName;
        }

        public string GetPartRegex(string parameter) => null;

        public ParameterParseResult ParseValue(object value, string parameter)
        {
            if (!(value is string))
            {
                return ParameterParseResult.Failed;
            }

            if (ValidateValue((string)value, parameter))
            {
                return ParameterParseResult.Create(value);
            }
            else
            {
                return ParameterParseResult.Failed;
            }
        }

        protected abstract bool ValidateValue(string value, string parameter);

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
