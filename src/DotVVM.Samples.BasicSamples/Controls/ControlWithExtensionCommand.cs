using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotVVM.Framework.Runtime;
using DotVVM.Framework;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class ControlWithExtensionCommand: HtmlGenericControl
    {
        public List<int> List
        {
            get { return (List<int>)GetValue(ListProperty); }
            set { SetValue(ListProperty, value); }
        }
        public static readonly DotvvmProperty ListProperty =
            DotvvmProperty.Register<List<int>, ControlWithExtensionCommand>(t => t.List);


        public ControlWithExtensionCommand():base("span")
        {
            Children.Add(new Literal("add to list"));
        }

        protected override void OnInit(IDotvvmRequestContext context)
        {
            this.RegisterExtensionCommand(() => List.Add(List.Count), "AddToList");
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            var postbackScript = KnockoutHelper.GenerateClientPostBackScript("AddToList", this.GetExtensionCommand("AddToList"), context, this);
            writer.AddAttribute("onclick", postbackScript);
            base.AddAttributesToRender(writer, context);
        }
    }
}