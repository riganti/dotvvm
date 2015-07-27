using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    [DebuggerDisplay("{Type.Name}: {Value}")]
    public class ResolvedBinding
    {
        public Type BindingType { get; set; }
        public string Value { get; set; }
        public Expression Expression { get; set; }
        public DataContextStack DataContextTypeStack { get; set; }
        public Exception ParsingError { get; set; }

        public Expression GetExpression()
        {
            if (ParsingError != null) throw new Exception($"can't get expression, parsing of binding {{{ BindingType.Name }: { Value }}} failed", ParsingError);
            return Expression;
        }
    }
}
