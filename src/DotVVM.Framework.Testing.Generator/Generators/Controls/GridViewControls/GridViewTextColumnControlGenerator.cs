using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls.GridViewControls
{
    public class GridViewTextColumnControlGenerator : SeleniumGenerator<GridViewTextColumn>
    {
        public override DotvvmProperty[] NameProperties { get; } = { GridViewTextColumn.ValueBindingProperty, GridViewColumn.HeaderTextProperty };
        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "GridViewTextColumnProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
