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
    public class RefreshTextButton : HtmlGenericControl
    {
        public RefreshTextButton() : base("button") { }

        /// <summary>
        /// Gets or sets the command that will be triggered when the button is clicked.
        /// </summary>
        public Func<string> Click
        {
            get { return (Func<string>)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Func<string>, RefreshTextButton>(nameof(Click));

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var script = KnockoutHelper.GenerateClientPostBackScript(nameof(Click), this.GetCommandBinding(ClickProperty), this, new PostbackScriptOptions(returnValue: null));
            writer.AddAttribute("onclick", $"var promise = {script}; var el = this; promise.then(function(a) {{ el.innerText = a.commandResult || a; }})");

            base.AddAttributesToRender(writer, context);
        }
    }
}
