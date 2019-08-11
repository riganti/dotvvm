using System;

namespace DotVVM.Framework.Routing.Constraints
{
    public class RangeRouteParameterConstraint : NumericRouteParameterConstraintBase
    {
        public RangeRouteParameterConstraint(string constraintName) : base(constraintName)
        {
        }

        protected override bool ValidateValue(object value, string parameter)
        {
            var split = parameter.Split(',');
            if (split.Length != 2)
            {
                throw new ArgumentException($"The route constraint '{constraintName}' must have two parameters!");
            }

            if (!double.TryParse(split[0], out var minValue))
            {
                throw new ArgumentException($"The first parameter of the route constraint '{constraintName}' must be a number!");
            }
            if (!double.TryParse(split[1], out var maxValue))
            {
                throw new ArgumentException($"The second parameter of the route constraint '{constraintName}' must be a number!");
            }

            return Convert.ToDouble(value) >= minValue && Convert.ToDouble(value) <= maxValue;
        }
    }
}
