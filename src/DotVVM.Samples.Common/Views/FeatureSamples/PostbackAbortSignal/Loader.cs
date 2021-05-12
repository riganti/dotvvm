using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.Views.FeatureSamples.PostbackAbortSignal
{
    public class Loader : HtmlGenericControl
    {
        public Command Load
        {
            get { return (Command)GetValue(LoadProperty); }
            set { SetValue(LoadProperty, value); }
        }
        public static readonly DotvvmProperty LoadProperty
            = DotvvmProperty.Register<Command, Loader>(c => c.Load, null);

        public Loader() : base("input")
        {

        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);

            var loadBinding = GetCommandBinding(LoadProperty);
            if (loadBinding != null)
            {
                writer.AddAttribute("onclick", "window.abortController=new AbortController(); "+ KnockoutHelper.GenerateClientPostBackScript(
                    nameof(Load),
                    loadBinding,
                    this,
                    new PostbackScriptOptions(abortSignal: new CodeParameterAssignment("window.abortController.signal", OperatorPrecedence.Max))), true, ";");
            }

            writer.AddAttribute("value", "Load data");
            writer.AddAttribute("type", "button");
        }
    }
}
