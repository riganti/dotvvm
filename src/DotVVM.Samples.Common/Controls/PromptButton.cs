using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class PromptButton : HtmlGenericControl
    {
        public PromptButton() : base("button") { }

        /// <summary>
        /// Gets or sets the command that will be triggered when the button is clicked.
        /// </summary>
        public Action<string> Click
        {
            get { return (Action<string>)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Action<string>, PromptButton>(nameof(Click));

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(nameof(Click), this.GetCommandBinding(ClickProperty), this, new PostbackScriptOptions(
                commandArgs: CodeParameterAssignment.FromExpression(
                    new JsArrayExpression(
                        new JsIdentifierExpression("prompt").Invoke(new JsLiteral("type something"))
                    )
                )
            )));
        }
    }
}
