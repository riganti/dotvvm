using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class UpdateProgressControlGenerator : SeleniumGenerator<UpdateProgress>
    {
        private static readonly DotvvmProperty[] nameProperties = { HtmlGenericControl.InnerTextProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;
        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "UpdateProgressProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
