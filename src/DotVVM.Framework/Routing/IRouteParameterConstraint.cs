using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Routing
{
    public interface IRouteParameterConstraint
    {
        /// <summary> Gets a regular expression that matches the route parameter value. </summary>
        /// <param name="parameter">Argument passed to the constraint (like for example `min(1)`). Null, if there is no parameter specified.</param>
        string GetPartRegex(string? parameter);
        ParameterParseResult ParseString(string value, string? parameter);
    }

    public struct ParameterParseResult
    {
        public readonly bool IsOK;
        public readonly object? Value;
        public ParameterParseResult(bool ok, object? value)
        {
            IsOK = ok;
            Value = value;
        }

        public static readonly ParameterParseResult Failed = new ParameterParseResult(false, null);
        public static ParameterParseResult Create(object obj) => new ParameterParseResult(true, obj.NotNull());
    }
}
