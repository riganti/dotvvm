using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.Generator;

namespace DotVVM.Framework.Testing.Generator.Generators.Controls
{
    public class LiteralControlGenerator : SeleniumGenerator<Literal>
    {
        private static readonly DotvvmProperty[] nameProperties = { Literal.TextProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => true;


        public override bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            if (context.Control.TryGetProperty(Literal.RenderSpanElementProperty, out var setter))
            {
                if (((ResolvedPropertyValue) setter).Value as bool? == false)
                {
                    return false;
                }
            }

            return base.CanAddDeclarations(pageObject, context);
        }

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "LiteralProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}
