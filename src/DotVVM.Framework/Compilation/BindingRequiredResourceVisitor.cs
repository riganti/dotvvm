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
    public class BindingRequiredResourceVisitor: ResolvedControlTreeVisitor
    {
        private readonly ControlResolverMetadata requiredResourceConrolMetadata;
        public BindingRequiredResourceVisitor(ControlResolverMetadata requiredResourceConrolMetadata)
        {
            this.requiredResourceConrolMetadata = requiredResourceConrolMetadata;
        }

        ImmutableHashSet<string> requiredResources = ImmutableHashSet<string>.Empty;

        public override void VisitView(ResolvedTreeRoot view)
        {
            var original = requiredResources;
            base.VisitView(view);
            if (original != requiredResources)
            {
                view.Content.AddRange(
                    requiredResources.Except(original).Select(r => CreateRequiredResourceControl(r, view.DothtmlNode, view.Content.First().DataContextTypeStack)));
                requiredResources = original;
            }
        }

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            var original = requiredResources;
            base.VisitPropertyTemplate(propertyTemplate);
            if (original != requiredResources)
            {
                propertyTemplate.Content.AddRange(
                    requiredResources.Except(original).Select(r => CreateRequiredResourceControl(r, propertyTemplate.DothtmlNode, propertyTemplate.Content.First().DataContextTypeStack)));
                requiredResources = original;
            }
        }

        public override void VisitControl(ResolvedControl control)
        {
            if (control.Metadata.Type == typeof(Content))
            {
                var original = requiredResources;
                base.VisitControl(control);
                if (original != requiredResources)
                {
                    control.Content.AddRange(
                        requiredResources.Except(original).Select(r => CreateRequiredResourceControl(r, control.DothtmlNode, control.Content.First().DataContextTypeStack)));
                    requiredResources = original;
                }
            }
            else
            {
                base.VisitControl(control);
            }
        }

        private ResolvedControl CreateRequiredResourceControl(string resource, Parser.Dothtml.Parser.DothtmlNode node, DataContextStack dataContext)
        {
            var c = new ResolvedControl(requiredResourceConrolMetadata, node, dataContext);
            c.SetProperty(new ResolvedPropertyValue(RequiredResource.NameProperty, resource));
            return c;
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            var r = propertyBinding.Binding.Binding.GetProperty<RequiredRuntimeResourcesBindingProperty>(ErrorHandlingMode.ReturnNull);
            if (r != null)
                requiredResources = requiredResources.Union(r.Resources);
            base.VisitPropertyBinding(propertyBinding);
        }
    }
}
