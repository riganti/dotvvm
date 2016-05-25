using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.ServerRendering
{
	public class ArticleDetail : ArticleBase
	{
        [MarkupOptions(Required = true)]
        public Command EditClick
        {
            get { return (Command)GetValue(EditClickProperty); }
            set { SetValue(EditClickProperty, value); }
        }
        public static readonly DotvvmProperty EditClickProperty
            = DotvvmProperty.Register<Command, ArticleDetail>(c => c.EditClick, null);

        public string SanitizedMessage
        {
            get { return (string)GetValue(SanitizedMessageProperty); }
        }
        public static readonly DotvvmProperty SanitizedMessageProperty
            = DotvvmProperty.Register<string, ArticleDetail>(c => c.SanitizedMessage, "");

        public void OnEditClick()
        {
            EditClick?.Invoke();
        }

        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            //SetValue(SanitizedMessageProperty, OriginalMessage.Replace("<","") ); //FIXME: this causes null it should not since default of OriginalMessage is set to ""

            //let's say there is something more sophisticated IRL
            SetValue(SanitizedMessageProperty, OriginalMessage?.Replace("<", "") ?? "");
            base.OnPreRender(context);
        }
    }
}

