using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class GridViewCheckBoxColumnControlGenerator : SeleniumGenerator<GridViewCheckBoxColumn>
    {
        public override DotvvmProperty[] NameProperties { get; } = { GridViewColumn.HeaderTextProperty, GridViewCheckBoxColumn.ValueBindingProperty };
        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "GridViewCheckBoxColumnProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
