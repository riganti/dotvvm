using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class LiteralGenerator : SeleniumGenerator<Literal>
    {
        private static readonly DotvvmProperty[] nameProperties = new[] { Literal.TextProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => true;


        public override bool CanAddDeclarations(HelperDefinition helperDefinition, SeleniumGeneratorContext context)
        {
            IAbstractPropertySetter setter;
            if (context.Control.TryGetProperty(Literal.RenderSpanElementProperty, out setter))
            {
                if (((ResolvedPropertyValue) setter).Value as bool? == false)
                {
                    return false;
                }
            }

            return base.CanAddDeclarations(helperDefinition, context);
        }

        protected override void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.LiteralProxy";
            helper.Members.Add(GeneratePropertyForProxy(context, type));
            helper.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }

    }
}