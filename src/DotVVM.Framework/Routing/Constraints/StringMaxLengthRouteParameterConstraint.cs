using System;

namespace DotVVM.Framework.Routing.Constraints
{
    public class MaxLengthRouteParameterConstraint : StringRouteParameterConstraintBase
    {
        public MaxLengthRouteParameterConstraint(string constraintName) : base(constraintName)
        {
        }

        protected override bool ValidateValue(string value, string parameter)
        {
            if (!int.TryParse(parameter, out var maxLength))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return value?.Length <= maxLength;
        }
    }
}
