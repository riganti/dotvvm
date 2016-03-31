using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.ServerRendering
{
	public class TestControl : DotvvmMarkupControl
	{
        [MarkupOptions(Required = true)]
        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }
        public static readonly DotvvmProperty DateProperty
            = DotvvmProperty.Register<DateTime, TestControl>(c => c.Date, default(DateTime));

        [MarkupOptions(Required = true)]
        public string OriginalMessage
        {
            get { return (string)GetValue(OriginalMessageProperty); }
            set { SetValue(OriginalMessageProperty, value); }
        }
        public static readonly DotvvmProperty OriginalMessageProperty
            = DotvvmProperty.Register<string, TestControl>(c => c.OriginalMessage, "");

        public string SanitizedMessage
        {
            get { return (string)GetValue(SanitizedMessageProperty); }
        }
        public static readonly DotvvmProperty SanitizedMessageProperty
            = DotvvmProperty.Register<string, TestControl>(c => c.SanitizedMessage, "");

        protected override void OnPreRender(IDotvvmRequestContext context)
        {

            //SetValue(SanitizedMessageProperty, OriginalMessage.Replace("<","") ); //FIXME: this causes null it should not since default of OriginalMessage is set to ""

            //let's say there is something more sophisticated IRL
            SetValue(SanitizedMessageProperty, OriginalMessage?.Replace("<", "") ?? "");
            base.OnPreRender(context);
        }

    }
}

