using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class LinkButtonControlGenerator : SeleniumGenerator<LinkButton>
    {
        public override DotvvmProperty[] NameProperties { get; } = { ButtonBase.TextProperty, HtmlGenericControl.InnerTextProperty, ButtonBase.ClickProperty};
        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "LinkButtonProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}