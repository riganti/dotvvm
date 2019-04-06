using System;

namespace DotVVM.Framework.Routing.Constraints
{
    public class MaxRouteParameterConstraint : NumericRouteParameterConstraintBase
    {
        public MaxRouteParameterConstraint(string constraintName) : base(constraintName)
        {
        }

        protected override bool ValidateValue(object value, string parameter)
        {
            if (!double.TryParse(parameter, out var maxValue))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return Convert.ToDouble(value) <= maxValue;
        }
    }
}
