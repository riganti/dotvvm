using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class ListBoxControlGenerator : SeleniumGenerator<ListBox>
    {
        public override DotvvmProperty[] NameProperties { get; } = { Selector.SelectedValueProperty, ItemsControl.DataSourceProperty, SelectorBase.ItemTextBindingProperty };
        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "ListBoxProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
