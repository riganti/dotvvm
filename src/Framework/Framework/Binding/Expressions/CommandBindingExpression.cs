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
using FastExpressionCompiler;
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

        private protected MaybePropValue<CommandJavascriptBindingProperty> commandJs;
        private protected MaybePropValue<IdBindingProperty> id;
        private protected MaybePropValue<ActionFiltersBindingProperty> actionFilters;

        private protected override void StoreProperty(object p)
        {
            if (p is CommandJavascriptBindingProperty commandJs)
                this.commandJs.SetValue(new(commandJs));
            if (p is IdBindingProperty id)
                this.id.SetValue(new(id));
            if (p is ActionFiltersBindingProperty actionFilters)
                this.actionFilters.SetValue(new(actionFilters));
            else
                base.StoreProperty(p);
        }

        public override object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (type == typeof(CommandJavascriptBindingProperty))
                return commandJs.GetValue(this).GetValue(errorMode, this, type);
            if (type == typeof(IdBindingProperty))
                return id.GetValue(this).GetValue(errorMode, this, type);
            if (type == typeof(ActionFiltersBindingProperty))
                return actionFilters.GetValue(this).GetValue(errorMode, this, type);
            return base.GetProperty(type, errorMode);
        }

        private protected override IEnumerable<object?> GetOutOfDictionaryProperties() =>
            base.GetOutOfDictionaryProperties().Concat(new object?[] {
                commandJs.Value.Value,
                id.Value.Value,
                actionFilters.Value.Value,
            });


        public ImmutableArray<IActionFilter> ActionFilters =>
            actionFilters.GetValueOrNull(this)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public ParametrizedCode CommandJavascript => commandJs.GetValueOrThrow(this).Code;

        public string BindingId => id.GetValueOrThrow(this).Id;

        public BindingDelegate BindingDelegate => this.bindingDelegate.GetValueOrThrow(this);

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

                    var type = prop is null ? null :
                               prop.IsBindingProperty ? prop.PropertyType.GenericTypeArguments.SingleOrDefault() :
                               prop.PropertyType;

                    // replace object with Command, we can't produce anything else than a delegate from a command binding
                    if (type is null || type == typeof(object))
                        type = typeof(Delegate);
                    
                    if (!type.IsDelegate())
                    {
                        // can I just throw an exception here?
                        throw new Exception($"Command binding can only be used in properties of a delegate type (or ICommandBinding). Property {prop} has type {prop?.PropertyType.ToCode()}.");
                    }

                    return new ExpectedTypeBindingProperty(type);
                }
            }
        }

        public static CodeSymbolicParameter PostbackOptionsParameter = new CodeSymbolicParameter("CommandBindingExpression.PostbackOptionsParameter");
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
                new JsSymbolicParameter(JavascriptTranslator.CurrentElementParameter),
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
