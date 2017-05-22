using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace DotVVM.Framework.Binding
{
    public interface IExpressionToDelegateCompiler
    {
        Delegate Compile(LambdaExpression expression);
    }

    public class DefaultExpressionToDelegateCompiler: IExpressionToDelegateCompiler
    {
        public Delegate Compile(LambdaExpression expression) => expression.Compile();
    }
}
