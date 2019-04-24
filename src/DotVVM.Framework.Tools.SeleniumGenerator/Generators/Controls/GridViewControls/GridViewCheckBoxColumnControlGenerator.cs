using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls.GridViewControls
{
    public class GridViewCheckBoxColumnControlGenerator : SeleniumGenerator<GridViewCheckBoxColumn>
    {
        public override DotvvmProperty[] NameProperties { get; } = { GridViewColumn.HeaderTextProperty, GridViewCheckBoxColumn.ValueBindingProperty };
        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "GridViewColumns.GridViewCheckBoxColumnProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
