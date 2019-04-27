using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class DotvvmControlGenerator : SeleniumGenerator<DotvvmMarkupControl>
    {
        public override DotvvmProperty[] NameProperties { get; } = { };
        public override bool CanUseControlContentForName => false;

        public override bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            // check if node is user control
            // todo: odstranit
            return context.Control.DothtmlNode is DothtmlElementNode htmlNode 
                   && htmlNode.TagPrefix != null 
                   && htmlNode.TagPrefix != "dot";
        }

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = $"{context.UniqueName}PageObject";

            AddControlPageObjectProperty(pageObject, context, type);
        }
    }
}
