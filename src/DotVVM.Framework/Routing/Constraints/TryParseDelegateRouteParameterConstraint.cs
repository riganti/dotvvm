using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotVVM.Framework.Routing.Constraints
{
    public class TryParseDelegateRouteParameterConstraint<T> : IRouteParameterConstraint
    {
        private readonly string constraintName;
        private readonly string partRegex;

        private readonly TryParseDelegate<T> tryParseDelegate;


        public delegate bool TryParseDelegate<TOutput>(string val, out TOutput value);


        public TryParseDelegateRouteParameterConstraint(string constraintName, string partRegex, TryParseDelegate<T> tryParseDelegate)
        {
            this.constraintName = constraintName;
            this.partRegex = partRegex;
            this.tryParseDelegate = tryParseDelegate;
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

            if (tryParseDelegate.Invoke((string)value, out var result))
            {
                return ParameterParseResult.Create(result);
            }
            else
            {
                return ParameterParseResult.Failed;
            }
        }

        public Type PredictType(Type valueType)
        {
            if (valueType == typeof(string))
            {
                return typeof(T);
            }
            else
            {
                throw new NotSupportedException($"The route constraint '{constraintName}' accepts only string values! Make sure the constraint is specified as a parameter's first constraint.");
            }
        }

    }
}
