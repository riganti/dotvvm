using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Binding.Expressions;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree;
using System.Linq;

namespace DotVVM.Framework.Compilation
{
    public class BindingRequiredResourceVisitor : ResolvedControlTreeVisitor
    {
        private readonly ControlResolverMetadata requiredResourceControlMetadata;
        public BindingRequiredResourceVisitor(ControlResolverMetadata requiredResourceConrolMetadata)
        {
            this.requiredResourceControlMetadata = requiredResourceConrolMetadata;
        }

        ImmutableHashSet<string> requiredResources = ImmutableHashSet<string>.Empty;

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            Visit(propertyTemplate, propertyTemplate.Content, base.VisitPropertyTemplate);
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            Visit(view, view.Content, base.VisitView);
        }

        public override void VisitControl(ResolvedControl control)
        {
            if (control.Metadata.Type == typeof(Content))
            {
                Visit(control, control.Content, base.VisitControl);
            }
            else
            {
                base.VisitControl(control);
            }
        }


        private void Visit<TNodeType>(TNodeType node, List<ResolvedControl> nodeContent, Action<TNodeType> visitBase) where TNodeType : ResolvedTreeNode
        {
            var original = requiredResources;
            visitBase(node);
            if (original != requiredResources)
            {
                nodeContent.AddRange(
                    requiredResources
                        .Except(original)
                        .Select(name => CreateRequiredResourceControl(name, node.DothtmlNode, nodeContent.First().DataContextTypeStack)));
                requiredResources = original;
            }
        }

        private ResolvedControl CreateRequiredResourceControl(string resource, Parser.Dothtml.Parser.DothtmlNode node, DataContextStack dataContext)
        {
            var control = new ResolvedControl(requiredResourceControlMetadata, node, dataContext);
            control.SetProperty(new ResolvedPropertyValue(RequiredResource.NameProperty, resource));
            return control;
        }

        private void AddResourcesFromProperties(ResolvedControl control)
        {
            if (control.TryGetProperty(Controls.Styles.RequiredResourcesProperty, out var value))
            {
                var newResources = (string[])((ResolvedPropertyValue)value).Value;
                if (newResources != null)
                    requiredResources = requiredResources.Union(newResources);
            }
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            var requiredResourceProperty = propertyBinding.Binding.Binding.GetProperty<RequiredRuntimeResourcesBindingProperty>(ErrorHandlingMode.ReturnNull);
            if (requiredResourceProperty != null)
            {
                requiredResources = requiredResources.Union(requiredResourceProperty.Resources);
            }

            base.VisitPropertyBinding(propertyBinding);
        }
    }
}
