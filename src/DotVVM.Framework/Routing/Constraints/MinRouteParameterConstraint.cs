using System;

namespace DotVVM.Framework.Routing.Constraints
{
    public class MinRouteParameterConstraint : NumericRouteParameterConstraintBase
    {
        public MinRouteParameterConstraint(string constraintName) : base(constraintName)
        {
        }

        protected override bool ValidateValue(object value, string parameter)
        {
            if (!double.TryParse(parameter, out var minValue))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return Convert.ToDouble(value) >= minValue;
        }
    }
}
