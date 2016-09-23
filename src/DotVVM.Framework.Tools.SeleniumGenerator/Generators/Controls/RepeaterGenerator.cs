using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class RepeaterGenerator : SeleniumGenerator<Repeater>
    {
        private static readonly DotvvmProperty[] nameProperties = new[] { ItemsControl.DataSourceProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;



        protected override void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            IAbstractPropertySetter itemTemplate;
            if (context.Control.TryGetProperty(Repeater.ItemTemplateProperty, out itemTemplate))
            {
                var template = (ResolvedPropertyTemplate) itemTemplate;

                // generate child helper class
                var itemHelperName = context.UniqueName + "RepeaterHelper";
                context.Visitor.PushScope(new HelperDefinition() { Name = itemHelperName });
                context.Visitor.VisitPropertyTemplate(template);
                helper.Children.Add(context.Visitor.PopScope());

                // generate property
                var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.RepeaterProxy";
                helper.Members.Add(GeneratePropertyForProxy(context, type, itemHelperName));
                helper.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type, itemHelperName));
            }
        }
    }
}
