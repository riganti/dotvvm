using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.Compilation.Binding;
using DotVVM.Framework.Utils;

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
            if (expectedType == typeof(object)) expectedType = typeof(Command);
            if (!typeof(Delegate).IsAssignableFrom(expectedType)) throw new Exception($"Command bindings must be assigned to properties with Delegate type, not { expectedType }");
            var normalConvert = TypeConversion.ImplicitConversion(expr, expectedType);
            if (normalConvert != null && expr.Type != typeof(object)) return normalConvert;
            if (typeof(Delegate).IsAssignableFrom(expectedType) && !typeof(Delegate).IsAssignableFrom(expr.Type))
            {
                var invokeMethod = expectedType.GetMethod("Invoke");
                expr = TaskConversion(expr, invokeMethod.ReturnType);
                return Expression.Lambda(
                    expectedType,
                    base.ConvertExpressionToType(expr, invokeMethod.ReturnType),
                    invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name))
                );
            }
            // TODO: convert delegates to another delegates
            throw new Exception($"can not convert '{expr.Type}' to '{expectedType}'");
        }

        public static Type GetTaskType(Type taskType)
            => taskType.GetProperty("Result")?.PropertyType ?? typeof(void);

        public Expression TaskConversion(Expression expr, Type expectedType)
        {
            if (typeof(Task).IsAssignableFrom(expr.Type) && !typeof(Task).IsAssignableFrom(expectedType))
            {
                // wait for task
                if (expectedType == typeof(void))
                {
                    return Expression.Call(expr, "Wait", Type.EmptyTypes);
                }
                else
                {
                    var taskResult = GetTaskType(expectedType);
                    if (taskResult != typeof(void) && expectedType.IsAssignableFrom(taskResult))
                    {
                        return Expression.Property(expr, "Result");
                    }
                }
            }
            else if (typeof(Task).IsAssignableFrom(expectedType))
            {
                if (!typeof(Task).IsAssignableFrom(expr.Type))
                {
                    // return dummy completed task
                    if (expectedType == typeof(Task))
                    {
                        return Expression.Block(expr, Expression.Call(typeof(TaskUtils), "GetCompletedTask", Type.EmptyTypes));
                    }
                    else if (typeof(Task<>).IsAssignableFrom(expectedType))
                    {
                        var taskType = GetTaskType(expectedType);
                        var converted = base.ConvertExpressionToType(expr, taskType);
                        if (converted != null) return Expression.Call(typeof(Task), "FromResult", new Type[] { taskType }, converted);
                    }
                }
                // TODO: convert Task<> to another Task<>
            }
            return expr;
        }
    }
}
