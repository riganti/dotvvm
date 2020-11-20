using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class ValidationSummaryControlGenerator : SeleniumGenerator<ValidationSummary>
    {
        public override DotvvmProperty[] NameProperties { get; } = { };
        public override bool CanUseControlContentForName { get; } = false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "ValidationSummaryProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
