using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

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
        public CommandBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties)
        {
            AddNullResolvers();
        }

        private protected MaybePropValue<ParametrizedCode> commandJs; // CommandJavascriptBindingProperty
        private protected MaybePropValue<string> id; // IdBindingProperty
        private protected MaybePropValue<ActionFiltersBindingProperty> actionFilters;

        private protected CommandJavascriptBindingProperty GetCommandJavascript(out ErrorWrapper? error) =>
            commandJs.GetValue(this).TryGet(out var value, out error)
                ? new CommandJavascriptBindingProperty(value)
                : default;

        private protected IdBindingProperty GetId(out ErrorWrapper? error) =>
            id.GetValue(this).TryGet(out var value, out error)
                ? new IdBindingProperty(value)
                : default;

        private protected override void StoreProperty(object p)
        {
            if (p is CommandJavascriptBindingProperty commandJs)
                this.commandJs.SetValue(new(commandJs.Code, null));
            if (p is IdBindingProperty id)
                this.id.SetValue(new(id.Id, null));
            if (p is ActionFiltersBindingProperty actionFilters)
                this.actionFilters.SetValue(new(actionFilters, null));
            else
                base.StoreProperty(p);
        }

        private protected override bool TryGetPropertyVirtual(Type type, out PropValue<object> value)
        {
            ErrorWrapper? error;
            if (type == typeof(CommandJavascriptBindingProperty))
            {
                value = new(GetCommandJavascript(out error), error);
                return true;
            }
            if (type == typeof(IdBindingProperty))
            {
                value = new(GetId(out error), error);
                return true;
            }
            if (type == typeof(ActionFiltersBindingProperty))
            {
                value = actionFilters.GetValue(this).AsObject();
                return true;
            }

            value = default;
            return false;
        }
        private protected override bool TryGetPropertyVirtual<T>([MaybeNull] out T value, out ErrorWrapper? error)
        {
            if (typeof(T) == typeof(CommandJavascriptBindingProperty))
            {
                value = (T)(object)GetCommandJavascript(out error);
                return true;
            }
            if (typeof(T) == typeof(IdBindingProperty))
            {
                value = (T)(object)GetId(out error);
                return false;
            }
            value = default;
            error = null;
            return false;
        }

        private protected override IEnumerable<object?> GetOutOfDictionaryProperties() =>
            base.GetOutOfDictionaryProperties().Concat(new object?[] {
                commandJs.Value?.Apply(v => new CommandJavascriptBindingProperty(v)),
                id.Value?.Apply(v => new IdBindingProperty(v)),
                actionFilters.Value,
            });


        public ImmutableArray<IActionFilter> ActionFilters =>
            actionFilters.GetValueOrNull(this)?.Filters ?? ImmutableArray<IActionFilter>.Empty;

        public ParametrizedCode CommandJavascript => commandJs.GetValueOrThrow(this);

        public string BindingId => id.GetValueOrThrow(this);

        public BindingDelegate BindingDelegate => this.bindingDelegate.GetValueOrThrow(this);

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => BindingCompilationService.GetDelegates([ new CommonCommandMethods(),  new Methods() ]);

            public class Methods
            {
                public CommandJavascriptBindingProperty CreateJs(IdBindingProperty id, CastedExpressionBindingProperty? expression = null) =>
                    new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(
                        id.Id,
                        needsCommandArgs: expression?.Expression.Type?.GetDelegateArguments()?.Length.Apply(len => len != 0)
                    ));
            }

            /// <summary> resolvers shared by command and static command binding </summary>
            public class CommonCommandMethods
            {
                public ExpectedTypeBindingProperty GetExpectedType(AssignedPropertyBindingProperty? property = null)
                {
                    var prop = property?.DotvvmProperty;

                    var type = prop is null ? null :
                               prop.IsBindingProperty ? prop.PropertyType.GenericTypeArguments.SingleOrDefault() :
                               prop.PropertyType;

                    // replace object with Command, we can't produce anything else than a delegate from a command binding
                    if (type is null || type == typeof(object))
                        type = typeof(Command);
                    
                    if (!type.IsDelegate())
                    {
                        // can I just throw an exception here?
                        throw new Exception($"Command binding can only be used in properties of a delegate type (or ICommandBinding). Property {prop} has type {prop?.PropertyType.ToCode()}.");
                    }

                    return new ExpectedTypeBindingProperty(type);
                }
                public CastedExpressionBindingProperty ConvertExpressionToType(ParsedExpressionBindingProperty expr, ExpectedTypeBindingProperty? expectedType = null)
                {
                    var destType = expectedType?.Type ?? typeof(object);
                    var convertedExpr = TypeConversion.ImplicitConversion(expr.Expression, destType, throwException: false, allowToString: true);
                    return new CastedExpressionBindingProperty(
                        // if the expression is of type object (i.e. null literal) try the lambda conversion.
                        convertedExpr != null && expr.Expression.Type != typeof(object) ? convertedExpr :
                        TypeConversion.MagicLambdaConversion(expr.Expression, destType) ?? convertedExpr ??
                        TypeConversion.EnsureImplicitConversion(expr.Expression, destType, allowToString: true)!
                    );
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
                p == CommandIdParameter ? new(KnockoutHelper.MakeStringLiteral(id, htmlSafe: false), OperatorPrecedence.Max) :
                default);

        public CommandBindingExpression(BindingCompilationService service, Action<object[]> command, string id)
            : this(service, c => (Action)(() => command(BindingHelper.GetDataContexts(c).ToArray()!)), id)
        { }

        public CommandBindingExpression(BindingCompilationService service, Func<object[], Task> command, string id)
            : this(service, c => (Command)(() => command(BindingHelper.GetDataContexts(c).ToArray()!)), id)
        { }

        public CommandBindingExpression(BindingCompilationService service, Delegate command, string id)
            : this(service, c => command, id)
        { }

        public CommandBindingExpression(BindingCompilationService service, BindingDelegate command, string id)
            : base(service, new object[] { command, new IdBindingProperty(id), new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(id)) })
        { }
    }

    public class CommandBindingExpression<T> : CommandBindingExpression, ICommandBinding<T>
    {
        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
        public CommandBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }
    }
}
