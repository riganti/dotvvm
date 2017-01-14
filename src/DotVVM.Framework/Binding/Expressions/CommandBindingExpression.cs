using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
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
        required: new[] { typeof(CompiledBindingExpression.BindingDelegate), typeof(CommandJavascriptBindingProperty) }
        )]
    [Options]
    public class CommandBindingExpression : BindingExpression, ICommandBinding
    {
        public CommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public ImmutableArray<IActionFilter> ActionFilters => this.GetProperty<ActionFiltersBindingProperty>().Filters;

        public ParametrizedCode CommandJavascript => this.GetProperty<CommandJavascriptBindingProperty>().Code;

        public string BindingId => this.GetProperty<IdBindingProperty>().Id;

        public CompiledBindingExpression.BindingDelegate BindingDelegate => this.GetProperty<CompiledBindingExpression.BindingDelegate>();

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {

            };
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
            new JsIdentifierExpression("dotvvm").Member("postback").Invoke(
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
            : base(service, new object[]{ command, new IdBindingProperty(id), new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(id))})
        { }
    }
}