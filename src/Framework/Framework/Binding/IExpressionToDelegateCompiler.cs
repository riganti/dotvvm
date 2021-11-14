using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Configuration;
using FastExpressionCompiler;

namespace DotVVM.Framework.Binding
{
    public interface IExpressionToDelegateCompiler
    {
        Delegate Compile(LambdaExpression expression);
    }

    public class DefaultExpressionToDelegateCompiler : IExpressionToDelegateCompiler
    {
        readonly bool interpret;
        public DefaultExpressionToDelegateCompiler(DotvvmConfiguration config)
        {
            interpret = config.Debug;
        }
        public Delegate Compile(LambdaExpression expression) =>
            interpret ? expression.Compile(preferInterpretation: interpret) :
            expression.Compile();
        // TODO: use FastExpressionCompiler
        // we can't do that atm since it still has some bugs, when these are fixed we should use that for all bindings
        // {
        //     var x = expression.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression | CompilerFlags.EnableDelegateDebugInfo);
        //     var di = x.Target as IDelegateDebugInfo;

        //     Console.WriteLine(di.CSharpString);
        //     Console.WriteLine(di.ExpressionString);

        //     return x;
        // }
    }
}
