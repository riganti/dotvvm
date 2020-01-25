#nullable enable
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation
{
    public interface IBindingExpressionBuilder
    {
        Expression Parse(string expression, DataContextStack dataContexts, BindingParserOptions options, params KeyValuePair<string, Expression>[] additionalSymbols);
    }
}
