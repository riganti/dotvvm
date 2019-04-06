using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    /// <summary>
    /// Provides the implementation for a route constraint.
    /// </summary>
    public interface IRouteParameterConstraint
    {

        /// <summary>
        /// Returns a regular expression that can be used to quickly match the parameter in the URL.
        /// If the parameter does not match the regular expression, the <see cref="ParseValue(object, string)"/> method will not be called at all.
        /// If the parameter matches the regulare expression, the <see cref="ParseValue(object, string)"/> will be called to perform additional validation and potential conversions.
        /// </summary>
        /// <param name="parameter">The parameter of the route constraint specified in route definition.</param>
        string GetPartRegex(string parameter);

        /// <summary>
        /// Parses the value of the route parameter, and return a result. Can convert the parameter to a different type (e.g. int).
        /// </summary>
        /// <param name="value">The value found in the URL.</param>
        /// <param name="parameter">The parameter of the route constraint specified in route definition.</param>
        ParameterParseResult ParseValue(object value, string parameter);

        /// <summary>
        /// Returns the type of the value that will be returned when the route is matched. Use this method to indicate whether the constraint performs any value conversions. The first route contraint always gets string, but can convert int to any other type and pass it to the next constraint in the chain.
        /// </summary>
        /// <param name="valueType">The type of the value on the input.</param>
        /// <returns>The type of the value that will be returned.</returns>
        Type PredictType(Type valueType);
    }

    public struct ParameterParseResult
    {
        public readonly bool IsOK;
        public readonly object Value;
        public ParameterParseResult(bool ok, object value)
        {
            IsOK = ok;
            Value = value;
        }

        public static readonly ParameterParseResult Failed = new ParameterParseResult(false, null);
        public static ParameterParseResult Create(object obj) => new ParameterParseResult(true, obj);
    }
}
