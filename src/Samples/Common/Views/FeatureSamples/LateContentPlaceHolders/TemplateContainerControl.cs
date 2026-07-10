using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.LateContentPlaceHolders
{
    /// <summary>
    /// A simple CompositeControl with an ITemplate property.
    /// The template is instantiated in GetContents (Load phase), allowing ContentPlaceHolder
    /// controls defined inside the template to be created after the initial master page composition.
    /// </summary>
    public class TemplateContainerControl : CompositeControl
    {
        public static DotvvmControl GetContents(
            ITemplate contentTemplate
        )
        {
            return new HtmlGenericControl("div")
                .AppendChildren(
                    new TemplateHost() { Template = contentTemplate }
                );
        }
    }
}
