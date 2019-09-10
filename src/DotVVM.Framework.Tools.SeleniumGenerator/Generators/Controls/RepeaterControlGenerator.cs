using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class RepeaterControlGenerator : SeleniumGenerator<Repeater>
    {
        private static readonly DotvvmProperty[] nameProperties = { ItemsControl.DataSourceProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;


        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            if (context.Control.TryGetProperty(Repeater.ItemTemplateProperty, out var itemTemplate))
            {
                var template = (ResolvedPropertyTemplate) itemTemplate;

                // generate child helper class
                var itemHelperName = context.UniqueName + "RepeaterPageObject";
                context.Visitor.PushScope(new PageObjectDefinitionImpl(itemHelperName, pageObject.Namespace));
                context.Visitor.VisitPropertyTemplate(template);
                pageObject.Children.Add(context.Visitor.PopScope());

                // generate property
                const string type = "RepeaterProxy";
                AddGenericPageObjectProperties(pageObject, context, type, itemHelperName);
            }
        }
    }
}
