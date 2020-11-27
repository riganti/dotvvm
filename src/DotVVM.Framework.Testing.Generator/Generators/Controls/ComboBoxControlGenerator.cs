using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class ComboBoxControlGenerator : SeleniumGenerator<ComboBox>
    {
        private static readonly DotvvmProperty[] nameProperties = { Selector.SelectedValueProperty, SelectorBase.SelectionChangedProperty, ItemsControl.DataSourceProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "ComboBoxProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
