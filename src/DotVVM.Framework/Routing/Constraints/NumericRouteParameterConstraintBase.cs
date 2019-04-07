using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Routing.Constraints
{
    public abstract class NumericRouteParameterConstraintBase : IRouteParameterConstraint
    {
        protected readonly string constraintName;

        public NumericRouteParameterConstraintBase(string constraintName)
        {
            this.constraintName = constraintName;
        }

        public string GetPartRegex(string parameter) => "-?[0-9.e]*?";

        public ParameterParseResult ParseValue(object value, string parameter)
        {
            if (value is string)
            {
                if (!Invariant.TryParse((string)value, out double convertedValue))
                {
                    return ParameterParseResult.Failed;
                }
                value = convertedValue;
            }
            else if (value == null || !ReflectionUtils.IsNumericType(value.GetType()))
            {
                return ParameterParseResult.Failed;
            }

            if (ValidateValue(value, parameter))
            {
                return ParameterParseResult.Create(value);
            }
            else
            {
                return ParameterParseResult.Failed;
            }
        }

        protected abstract bool ValidateValue(object value, string parameter);


        public Type PredictType(Type valueType)
        {
            if (ReflectionUtils.IsNumericType(valueType))
            { 
                return valueType;
            }
            else if (valueType == typeof(string))
            {
                return typeof(double);
            }
            else
            {
                throw new NotSupportedException($"The route constraint '{constraintName}' accepts only numeric or string values!");
            }
        }
    }
}
