using System;

namespace DotVVM.Framework.Routing
{
    public class GenericConvertedRouteParameterType : GenericRouteParameterType, IConvertedRouteParameterConstraint
    {
        Func<object, string, ParameterParseResult> convertedParser;

        public GenericConvertedRouteParameterType(Func<string, string> partRegex, Func<string, string, ParameterParseResult> parser = null, Func<object, string, ParameterParseResult> convertedParser = null) : base(partRegex, parser)
        {
            this.convertedParser = convertedParser;
        }

        public ParameterParseResult ParseObject(object value, string parameter)
        {
            if (value is string || value == null)
            {
                return ParseString((string)value, parameter);
            }

            if (convertedParser != null)
            {
                return convertedParser(value, parameter);
            }

            throw new NotSupportedException($"The route constraint doesn't support the value of {value.GetType()}.");  // TODO: add route constraint name to the error message
        }
    }
}
