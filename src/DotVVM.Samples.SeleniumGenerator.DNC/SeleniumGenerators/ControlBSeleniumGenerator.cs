using DotVVM.Framework.Binding;
using DotVVM.Framework.Testing.SeleniumGenerator;
using SampleApp1.Controls;

namespace SampleApp1.SeleniumGenerators
{
    public class ControlBSeleniumGenerator : SeleniumGenerator<ControlB>
    {
        public override DotvvmProperty[] NameProperties { get; } = { };
        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            // this is generator for user control so it's using PageObject not Proxy
            // you need to create this class in your UI test project in folder PageObjects
            const string type = "PageObjects.MyControlBPageObject";
            AddControlPageObjectProperty(pageObject, context, type);
        }
    }
}
