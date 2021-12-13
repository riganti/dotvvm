using System;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.ControlSamples.TemplateHost
{
    public class CompositeControlWithTemplate : CompositeControl 
    {

        public static DotvvmControl GetContents(
            ValueOrBinding<string> headerText,
            ITemplate contentTemplate
        )
        {
            return new HtmlGenericControl("fieldset")
                .AppendChildren(
                    new HtmlGenericControl("legend", new TextOrContentCapability(headerText)),
                    new Framework.Controls.TemplateHost() { Template = contentTemplate }
                );
        }

    }
}
