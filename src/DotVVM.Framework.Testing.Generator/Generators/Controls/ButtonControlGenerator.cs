using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class ButtonControlGenerator : SeleniumGenerator<Button>
    {
        private static readonly DotvvmProperty[] nameProperties 
            = { ButtonBase.TextProperty, ButtonBase.ClickProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;
        public override bool CanUseControlContentForName => true;

        protected override void AddDeclarationsCore(
            PageObjectDefinition pageObject, 
            SeleniumGeneratorContext context)
        {
            const string type = "ButtonProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
