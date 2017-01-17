using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary>
    /// Represents typical command binding delegate, quivalent to Func&lt;Task&gt;
    /// </summary>
    public delegate Task Command();

    [BindingCompilationRequirements(
        required: new[] { typeof(CompiledBindingExpression.BindingDelegate), typeof(CommandJavascriptBindingProperty) },
        optional: new[] { typeof(ActionFiltersBindingProperty) }
        )]
    [Options]
    public class CommandBindingExpression : BindingExpression, ICommandBinding
    {
        public CommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public ImmutableArray<IActionFilter> ActionFilters =>
            this.GetProperty<ActionFiltersBindingProperty>(ErrorHandlingMode.ReturnNull)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public ParametrizedCode CommandJavascript => this.GetProperty<CommandJavascriptBindingProperty>().Code;

        public string BindingId => this.GetProperty<IdBindingProperty>().Id;

        public CompiledBindingExpression.BindingDelegate BindingDelegate => this.GetProperty<CompiledBindingExpression.BindingDelegate>();

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => BindingCompilationService.GetDelegates(new Methods());

            public class Methods
            {
                public CommandJavascriptBindingProperty CreateJs(IdBindingProperty id) =>
                    new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(id.Id));
                public CastedExpressionBindingProperty ConvertExpressionToType(ParsedExpressionBindingProperty exprP, ExpectedTypeBindingProperty expectedTypeP = null)
                {
                    var expr = exprP.Expression;
                    var expectedType = expectedTypeP?.Type ?? typeof(object);
                    if (expectedType == typeof(object)) expectedType = typeof(Command);
                    if (!typeof(Delegate).IsAssignableFrom(expectedType)) throw new Exception($"Command bindings must be assigned to properties with Delegate type, not { expectedType }");
                    var normalConvert = TypeConversion.ImplicitConversion(expr, expectedType);
                    if (normalConvert != null && expr.Type != typeof(object)) return new CastedExpressionBindingProperty(normalConvert);
                    if (typeof(Delegate).IsAssignableFrom(expectedType) && !typeof(Delegate).IsAssignableFrom(expr.Type))
                    {
                        var invokeMethod = expectedType.GetMethod("Invoke");
                        expr = TaskConversion(expr, invokeMethod.ReturnType);
                        return new CastedExpressionBindingProperty(Expression.Lambda(
                            expectedType,
                            expr,//base.ConvertExpressionToType(expr, invokeMethod.ReturnType),
                            invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name))
                        ));
                    }
                    // TODO: convert delegates to another delegates
                    throw new Exception($"Can not convert expression '{ expr }' to '{expectedType}'.");
                }

                private static Type GetTaskType(Type taskType)
                    => taskType.GetProperty("Result")?.PropertyType ?? typeof(void);

                private Expression TaskConversion(Expression expr, Type expectedType)
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
                                return Expression.Block(expr, Expression.Call(typeof(Task), "FromResult", new[] { typeof(int) }, Expression.Constant(0)));
                            }
                            else if (typeof(Task<>).IsAssignableFrom(expectedType))
                            {
                                var taskType = GetTaskType(expectedType);
                                var converted = expr;//base.ConvertExpressionToType(expr, taskType);
                                if (converted != null) return Expression.Call(typeof(Task), "FromResult", new Type[] { taskType }, converted);
                            }
                        }
                        // TODO: convert Task<> to another Task<>
                    }
                    return expr;
                }
            }
        }

        public static object ViewModelNameParameter = new object();
        public static object SenderElementParameter = new object();
        public static object CurrentPathParameter = new object();
        public static object CommandIdParameter = new object();
        public static object ControlUniqueIdParameter = new object();
        public static object UseObjectSetTimeoutParameter = new object();
        public static object ValidationPathParameter = new object();
        public static object OptionalKnockoutContextParameter = new object();
        public static object PostbackHandlersParameters = new object();
        private static ParametrizedCode javascriptPostbackInvocation =
            new JsIdentifierExpression("dotvvm").Member("postBack").Invoke(
                new JsSymbolicParameter(ViewModelNameParameter),
                new JsSymbolicParameter(SenderElementParameter),
                new JsSymbolicParameter(CurrentPathParameter),
                new JsSymbolicParameter(CommandIdParameter),
                new JsSymbolicParameter(ControlUniqueIdParameter),
                new JsSymbolicParameter(UseObjectSetTimeoutParameter),
                new JsSymbolicParameter(ValidationPathParameter),
                new JsSymbolicParameter(OptionalKnockoutContextParameter),
                new JsSymbolicParameter(PostbackHandlersParameters)
            ).FormatParametrizedScript();
        public static ParametrizedCode CreateJsPostbackInvocation(string id) =>
            javascriptPostbackInvocation.AssignParameters(p =>
                p == CommandIdParameter ? CodeParameterAssignment.FromExpression(new JsLiteral(id)) :
                default(CodeParameterAssignment));

        public CommandBindingExpression(BindingCompilationService service, Action<object[]> command, string id)
            : this(service, (h, o) => (Action)(() => command(h)), id)
        { }

        public CommandBindingExpression(BindingCompilationService service, Delegate command, string id)
            : this(service, (h, o) => command, id)
        { }

        public CommandBindingExpression(BindingCompilationService service, CompiledBindingExpression.BindingDelegate command, string id)
            : base(service, new object[] { command, new IdBindingProperty(id), new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(id)) })
        { }
    }
}