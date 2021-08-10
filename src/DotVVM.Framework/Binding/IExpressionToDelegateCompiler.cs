#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace DotVVM.Framework.Binding
{
    public interface IExpressionToDelegateCompiler
    {
        Delegate Compile(LambdaExpression expression);
    }

    public class DefaultExpressionToDelegateCompiler : IExpressionToDelegateCompiler
    {
        public Delegate Compile(LambdaExpression expression) => expression.Compile();
    }
}
