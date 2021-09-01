using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class ButtonParameter
    {
        public string MyProperty { get; set; }
    }

    public class ParametrizedButton : HtmlGenericControl
    {
        public ParametrizedButton() : base("button") { }

        public IValueBinding<ButtonParameter> Parameter
        {
            get { return (IValueBinding<ButtonParameter>)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }
        public static readonly DotvvmProperty ParameterProperty =
            DotvvmProperty.Register<IValueBinding<ButtonParameter>, ParametrizedButton>(nameof(Parameter));

        /// <summary>
        /// Gets or sets the command that will be triggered when the button is clicked.
        /// </summary>
        public Action<ButtonParameter> Click
        {
            get { return (Action<ButtonParameter>)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Action<ButtonParameter>, ParametrizedButton>(nameof(Click));

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var parameter = Parameter.GetParametrizedKnockoutExpression(this);
            writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(nameof(Click), this.GetCommandBinding(ClickProperty), this, new PostbackScriptOptions(
                commandArgs: new ParametrizedCode.Builder {
                    "[",
                    parameter,
                    "]"
                }.Build(OperatorPrecedence.Max)
            )));
        }
    }
}
