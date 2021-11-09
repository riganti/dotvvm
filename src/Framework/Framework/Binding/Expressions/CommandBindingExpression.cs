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
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary>
    /// Represents typical command binding delegate, equivalent to Func&lt;Task&gt;
    /// </summary>
    public delegate Task Command();

    [BindingCompilationRequirements(
        required: new[] { typeof(BindingDelegate) },
        optional: new[] { typeof(ActionFiltersBindingProperty), typeof(IdBindingProperty), typeof(CommandJavascriptBindingProperty) }
        )]
    [Options]
    public class CommandBindingExpression : BindingExpression, ICommandBinding
    {
        public CommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties)
        {
            AddNullResolvers();
        }

        public ImmutableArray<IActionFilter> ActionFilters =>
            this.GetProperty<ActionFiltersBindingProperty>(ErrorHandlingMode.ReturnNull)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public ParametrizedCode CommandJavascript => this.GetProperty<CommandJavascriptBindingProperty>().Code;

        public string BindingId => this.GetProperty<IdBindingProperty>().Id;

        public BindingDelegate BindingDelegate => this.GetProperty<BindingDelegate>();

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => BindingCompilationService.GetDelegates(new[] { new Methods() });

            public class Methods
            {
                public CommandJavascriptBindingProperty CreateJs(IdBindingProperty id, CastedExpressionBindingProperty? expression = null) =>
                    new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(
                        id.Id,
                        needsCommandArgs: expression?.Expression.Type?.GetDelegateArguments()?.Length.Apply(len => len != 0)
                    ));

                public ExpectedTypeBindingProperty GetExpectedType(AssignedPropertyBindingProperty? property = null)
                {
                    var prop = property?.DotvvmProperty;
                    if (prop == null) return new ExpectedTypeBindingProperty(typeof(Command));

                    return new ExpectedTypeBindingProperty(prop.IsBindingProperty ? (prop.PropertyType.GenericTypeArguments.SingleOrDefault() ?? typeof(Command)) : prop.PropertyType);
                }
            }
        }

        public static CodeSymbolicParameter PostbackOptionsParameter = new CodeSymbolicParameter("CommandBindingExpression.PostbackOptionsParameter");
        public static CodeSymbolicParameter SenderElementParameter = new CodeSymbolicParameter("CommandBindingExpression.SenderElementParameter");
        public static CodeSymbolicParameter CurrentPathParameter = new CodeSymbolicParameter("CommandBindingExpression.CurrentPathParameter");
        public static CodeSymbolicParameter CommandIdParameter = new CodeSymbolicParameter("CommandBindingExpression.CommandIdParameter");
        public static CodeSymbolicParameter ControlUniqueIdParameter = new CodeSymbolicParameter("CommandBindingExpression.ControlUniqueIdParameter");
        /// Knockout context passed as postback argument. May be null, when it's the same as ko.contextFor(element).
        public static CodeSymbolicParameter OptionalKnockoutContextParameter = new CodeSymbolicParameter("CommandBindingExpression.OptionalKnockoutContextParameter", CodeParameterAssignment.FromIdentifier("null"));
        public static CodeSymbolicParameter PostbackHandlersParameter = new CodeSymbolicParameter("CommandBindingExpression.PostbackHandlersParameter");
        public static CodeSymbolicParameter CommandArgumentsParameter = new CodeSymbolicParameter("CommandBindingExpression.CommandArgumentsParameter");
        public static CodeSymbolicParameter AbortSignalParameter = new CodeSymbolicParameter("CommandBindingExpression.AbortSignalParameter");

        private static ParametrizedCode createJavascriptPostbackInvocation(JsExpression? commandArgs) =>
            new JsIdentifierExpression("dotvvm").Member("postBack").Invoke(
                new JsSymbolicParameter(SenderElementParameter),
                new JsSymbolicParameter(CurrentPathParameter),
                new JsSymbolicParameter(CommandIdParameter),
                new JsSymbolicParameter(ControlUniqueIdParameter),
                new JsSymbolicParameter(OptionalKnockoutContextParameter),
                new JsSymbolicParameter(PostbackHandlersParameter),
                commandArgs ?? new JsLiteral(new object[] { }),
                new JsSymbolicParameter(AbortSignalParameter)
            ).FormatParametrizedScript();

        private static ParametrizedCode javascriptPostbackInvocation = createJavascriptPostbackInvocation(
            new JsSymbolicParameter(CommandArgumentsParameter, new CodeParameterAssignment("undefined", OperatorPrecedence.Max)));

        private static ParametrizedCode javascriptPostbackInvocation_requiredCommandArgs = createJavascriptPostbackInvocation(new JsSymbolicParameter(CommandArgumentsParameter));

        private static ParametrizedCode javascriptPostbackInvocation_noCommandArgs = createJavascriptPostbackInvocation(null);

        /// <param name="needsCommandArgs">Whether the Javascript will contain commandArgs (true - it will be required, false - the symbolic parameter will not be available, null - it will be optional)</param>
        public static ParametrizedCode CreateJsPostbackInvocation(string id, bool? needsCommandArgs = null) =>
            (needsCommandArgs == true ? javascriptPostbackInvocation_requiredCommandArgs :
             needsCommandArgs == false ? javascriptPostbackInvocation_noCommandArgs :
             javascriptPostbackInvocation)
            .AssignParameters(p =>
                p == CommandIdParameter ? CodeParameterAssignment.FromLiteral(id) :
                default);

        public CommandBindingExpression(BindingCompilationService service, Action<object[]> command, string id)
            : this(service, (h, o) => (Action)(() => command(h!)), id)
        { }

        public CommandBindingExpression(BindingCompilationService service, Func<object[], Task> command, string id)
            : this(service, (h, o) => (Command)(() => command(h!)), id)
        { }

        public CommandBindingExpression(BindingCompilationService service, Delegate command, string id)
            : this(service, (h, o) => command, id)
        { }

        public CommandBindingExpression(BindingCompilationService service, BindingDelegate command, string id)
            : base(service, new object[] { command, new IdBindingProperty(id), new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(id)) })
        { }
    }

    public class CommandBindingExpression<T> : CommandBindingExpression, ICommandBinding<T>
    {
        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
        public CommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }
    }
}
