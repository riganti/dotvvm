using System;

namespace DotVVM.Framework.Routing.Constraints
{
    public class MinLengthRouteParameterConstraint : StringRouteParameterConstraintBase
    {
        public MinLengthRouteParameterConstraint(string constraintName) : base(constraintName)
        {
        }

        public override string GetPartRegex(string parameter)
        {
            if (!int.TryParse(parameter, out var requiredLength))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return "[^/]{" + requiredLength + ",}";
        }

        protected override bool ValidateValue(string value, string parameter)
        {
            if (!int.TryParse(parameter, out var minLength))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return value?.Length >= minLength;
        }
    }
}
