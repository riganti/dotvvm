using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.Compilation.Binding;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class CommandBindingCompilationAttribute : BindingCompilationAttribute
    {
        public override string CompileToJs(ResolvedBinding binding, CompiledBindingExpression expression)
        {
            return $"dotvvm.postbackScript('{ expression.Id }')";
        }

        protected override Expression ConvertExpressionToType(Expression expr, Type expectedType)
        {
            if (!typeof(Delegate).IsAssignableFrom(expectedType)) throw new Exception($"Command bindings must be assigned to properties with Delegate type, not { expectedType }");
            var normalConvert = TypeConversion.ImplicitConversion(expr, expectedType);
            if (normalConvert != null && expr.Type != typeof(object)) return normalConvert;
            if (typeof(Delegate).IsAssignableFrom(expectedType) && !typeof(Delegate).IsAssignableFrom(expr.Type))
            {
                var invokeMethod = expectedType.GetMethod("Invoke");
                return Expression.Lambda(
                    expectedType,
                    base.ConvertExpressionToType(expr, invokeMethod.ReturnType),
                    invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name))
                );
            }
            // TODO: convert delegates to another delegates
            throw new Exception($"can't convert {expr.Type} to {expectedType}");
        }
    }
}
