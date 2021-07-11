using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.Binding
{
    class CommandActionFilterVisitor: ExpressionVisitor
    {
        public CommandActionFilterVisitor(ICommandBinding binding, bool isControlCommand)
        {
            Binding = binding;
            IsControlCommand = isControlCommand;
        }

        public ICommandBinding Binding { get; }
        public bool IsControlCommand { get; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var filters = node.Method.GetCustomAttributes<ICommandActionFilter>().ToArray();
            if (filters.Length == 0)
                return node;

            var resultType = node.Type;

            var innerCall =
                resultType == typeof(void) ?
                Expression.Block(node, Expression.Constant(true)) :
                (Expression)node;

            // void can't be in generic argument, so we use bool
            var fakeResultType = resultType == typeof(void) ? typeof(bool) : resultType;

            var invokeMethod = GetType().GetMethod(nameof(InvokeWithFilters));

            var call = Expression.Call(
                invokeMethod.MakeGenericMethod(fakeResultType),
                Expression.Constant(filters),
                Expression.Constant(IsControlCommand),
                Expression.Constant(Binding),
                BindingCompiler.CurrentControlParameter,
                Expression.Constant(resultType),
                Expression.Lambda(innerCall)
            );

            if (resultType == typeof(void))
                // we have the bool, return void
                return Expression.Block(
                    call,
                    Expression.Default(typeof(void))
                );
            else
                return call;
        }

        public static async Task<T> InvokeWithFilters<T>(
            ICommandActionFilter[] filters,
            bool isControlCommand,
            ICommandBinding commandBinding,
            DotvvmBindableObject control,
            Type resultType,
            Func<Task<T>> command)
        {
            var context = control.GetValue(Internal.RequestContextProperty) as IDotvvmRequestContext;
            if (context is null)
                throw new NotSupportedException($"Can not invoke {commandBinding} on {control.DebugString()}. The control is not in control tree.");
            var actionInfo = new ActionInfo(commandBinding, () => command(), isControlCommand);
            foreach (var f in filters)
            {
                await f.OnCommandExecutingAsync(context, actionInfo);
            }

            try
            {
                var result = await command();
                foreach (var f in filters)
                {
                    await f.OnCommandExecutedAsync(context, actionInfo, null);
                }
                return result;
            }
            catch (Exception e)
            {
                context.IsCommandExceptionHandled = false;
                foreach (var f in filters)
                {
                    await f.OnCommandExecutedAsync(context, actionInfo, e);
                }
                if (context.IsCommandExceptionHandled)
                {
                    if (resultType != typeof(void))
                    {
                        context.Services.GetService<RuntimeWarningCollector>().Warn(new DotvvmRuntimeWarning(
                            $"Command {commandBinding} has thrown an exception and it was handled, but the command does not return void and no result value was specified.",
                            relatedException: e,
                            relatedControl: control
                        ));
                    }
                    return default;
                }
                throw;
            }
        }
    }
}
