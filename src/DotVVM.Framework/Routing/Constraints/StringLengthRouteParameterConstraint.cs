using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Routing.Constraints
{

    public class LengthRouteParameterConstraint : StringRouteParameterConstraintBase
    {
        public LengthRouteParameterConstraint(string constraintName) : base(constraintName)
        {
        }

        public override string GetPartRegex(string parameter)
        {
            if (!int.TryParse(parameter, out var requiredLength))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return "[^/]{" + requiredLength + "}";
        }

        protected override bool ValidateValue(string value, string parameter)
        {
            if (!int.TryParse(parameter, out var requiredLength))
            {
                throw new ArgumentException($"The parameter of the route constraint '{constraintName}' must be a number!");
            }

            return value?.Length == requiredLength;
        }
    }
}
