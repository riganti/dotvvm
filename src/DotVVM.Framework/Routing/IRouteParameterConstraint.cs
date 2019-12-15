using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public interface IRouteParameterConstraint
    {
        string GetPartRegex(string parameter);
        ParameterParseResult ParseString(string value, string parameter);
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
