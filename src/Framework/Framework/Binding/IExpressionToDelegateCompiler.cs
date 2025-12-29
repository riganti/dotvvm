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
        Delegate Compile(LambdaExpression expression, DotvvmExpressionCompilerType? preferredExpressionCompiler = null);

        T Compile<T>(Expression<T> expression, DotvvmExpressionCompilerType? preferredExpressionCompiler = null) where T : Delegate;
    }

    public class DefaultExpressionToDelegateCompiler(DotvvmConfiguration config) : IExpressionToDelegateCompiler
    {
        public Delegate Compile(LambdaExpression expression, DotvvmExpressionCompilerType? preferredExpressionCompiler = null)
        {
            return (preferredExpressionCompiler ?? config.Runtime.ExpressionCompiler) switch {

                // in Debug, we wanted to use interpretation
                // the interpreter is broken: https://github.com/dotnet/runtime/issues/96385
                // interpret ? expression.Compile(preferInterpretation: interpret) :
                DotvvmExpressionCompilerType.Standard => expression.Compile(),

                DotvvmExpressionCompilerType.FastExpressionCompiler => expression.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression),

                _ => throw new NotSupportedException($"Expression compiler {config.Runtime.ExpressionCompiler} is not supported!")
            };
        }

        public T Compile<T>(Expression<T> expression, DotvvmExpressionCompilerType? preferredExpressionCompiler = null) where T : Delegate
        {
            return (preferredExpressionCompiler ?? config.Runtime.ExpressionCompiler) switch {

                // in Debug, we wanted to use interpretation
                // the interpreter is broken: https://github.com/dotnet/runtime/issues/96385
                // interpret ? expression.Compile(preferInterpretation: interpret) :
                DotvvmExpressionCompilerType.Standard => expression.Compile(),

                DotvvmExpressionCompilerType.FastExpressionCompiler => expression.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression),

                _ => throw new NotSupportedException($"Expression compiler {config.Runtime.ExpressionCompiler} is not supported!")
            };
        }
    }
}
