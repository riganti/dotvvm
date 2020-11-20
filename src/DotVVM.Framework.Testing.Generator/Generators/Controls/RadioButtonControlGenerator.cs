using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class RadioButtonControlGenerator : SeleniumGenerator<RadioButton>
    {
        public override DotvvmProperty[] NameProperties { get; } = {CheckableControlBase.TextProperty, CheckableControlBase.CheckedValueProperty, RadioButton.CheckedProperty};
        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "RadioButtonProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
