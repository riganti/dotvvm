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

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty
            = DotvvmProperty.Register<string, Loader>(c => c.Text, null);


        public Loader() : base("input")
        {

        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);

            var loadBinding = GetCommandBinding(LoadProperty);
            if (loadBinding != null)
            {
                var call = KnockoutHelper.GenerateClientPostBackExpression(
                    nameof(Load),
                    loadBinding,
                    this,
                    new PostbackScriptOptions(abortSignal: new CodeParameterAssignment("window.abortController.signal", OperatorPrecedence.Max)));

                var @catch = "function (error) {if(error.reason.type===\"abort\"){document.getElementsByClassName(\"message\")[0].innerText=\"aborted\";}}";

                writer.AddAttribute("onclick", $"window.abortController=new AbortController(); {call}.catch({@catch}); event.stopPropagation();return false;", true, ";");
            }

            writer.AddAttribute("value", Text);
            writer.AddAttribute("type", "button");
        }
    }
}
