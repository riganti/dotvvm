using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class RouteLinkControlGenerator : SeleniumGenerator<RouteLink>
    {
        public override DotvvmProperty[] NameProperties { get; } = { RouteLink.TextProperty, HtmlGenericControl.InnerTextProperty, RouteLink.RouteNameProperty };

        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "RouteLinkProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
