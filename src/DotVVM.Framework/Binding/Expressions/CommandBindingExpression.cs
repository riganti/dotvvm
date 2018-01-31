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
    /// Represents typical command binding delegate, quivalent to Func&lt;Task&gt;
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
                public CommandJavascriptBindingProperty CreateJs(IdBindingProperty id) =>
                    new CommandJavascriptBindingProperty(CreateJsPostbackInvocation(id.Id));

                public ExpectedTypeBindingProperty GetExpectedType(AssignedPropertyBindingProperty property = null)
                {
                    var prop = property?.DotvvmProperty;
                    if (prop == null) return new ExpectedTypeBindingProperty(typeof(Command));

                    return new ExpectedTypeBindingProperty(prop.IsBindingProperty ? (prop.PropertyType.GenericTypeArguments.SingleOrDefault() ?? typeof(Command)) : prop.PropertyType);
                }
            }
        }

        public static object ViewModelNameParameter = new object();
        public static object SenderElementParameter = new object();
        public static object CurrentPathParameter = new object();
        public static object CommandIdParameter = new object();
        public static object ControlUniqueIdParameter = new object();
        public static object OptionalKnockoutContextParameter = new object();
        public static object PostbackHandlersParameter = new object();
        public static object CommandArgumentsParameter = new object();
        private static ParametrizedCode javascriptPostbackInvocation =
            new JsIdentifierExpression("dotvvm").Member("postBack").Invoke(
                new JsSymbolicParameter(ViewModelNameParameter),
                new JsSymbolicParameter(SenderElementParameter),
                new JsSymbolicParameter(CurrentPathParameter),
                new JsSymbolicParameter(CommandIdParameter),
                new JsSymbolicParameter(ControlUniqueIdParameter),
                new JsSymbolicParameter(OptionalKnockoutContextParameter),
                new JsSymbolicParameter(PostbackHandlersParameter),
                new JsSymbolicParameter(CommandArgumentsParameter)
            ).FormatParametrizedScript();
        public static ParametrizedCode CreateJsPostbackInvocation(string id) =>
            javascriptPostbackInvocation.AssignParameters(p =>
                p == CommandIdParameter ? CodeParameterAssignment.FromLiteral(id) :
                default(CodeParameterAssignment));

        public CommandBindingExpression(BindingCompilationService service, Action<object[]> command, string id)
            : this(service, (h, o) => (Action)(() => command(h)), id)
        { }

        public CommandBindingExpression(BindingCompilationService service, Func<object[], Task> command, string id)
            : this(service, (h, o) => (Command)(() => command(h)), id)
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