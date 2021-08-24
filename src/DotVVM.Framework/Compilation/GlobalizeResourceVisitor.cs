#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class GlobalizeResourceVisitor : ResolvedControlTreeVisitor
    {
        private readonly ControlResolverMetadata globalizeResourceControl;
        private bool isGlobalizeRequired = false;
        private bool isResourcePlaced = false;

        public GlobalizeResourceVisitor(ControlResolverMetadata globalizeResourceControl)
        {
            this.globalizeResourceControl = globalizeResourceControl;
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            Visit(view, view.Content, base.VisitView);
        }

        public override void VisitControl(ResolvedControl control)
        {
            if(control.Metadata.Type == typeof(Content))
            {
                Visit(control, control.Content, base.VisitControl);
                return;
            }
            base.VisitControl(control);
        }

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            Visit(propertyTemplate, propertyTemplate.Content, base.VisitPropertyTemplate);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            var requiredGlobalizeProperty = propertyBinding.Binding.Binding
                .GetProperty<GlobalizeResourceBindingProperty>(ErrorHandlingMode.ReturnNull);
            if (!isGlobalizeRequired)
            {
                isGlobalizeRequired = requiredGlobalizeProperty != null;
            }
            base.VisitPropertyBinding(propertyBinding);
        }

        private void Visit<TNode>(TNode node,
            List<ResolvedControl> content,
            Action<TNode> visitBase)
            where TNode : ResolvedTreeNode
        {
            visitBase(node);
            if (isGlobalizeRequired && !isResourcePlaced)
            {
                // the just-visited subtree requires globalize
                content.Add(new ResolvedControl(
                    globalizeResourceControl,
                    node.DothtmlNode,
                    node.TreeRoot.DataContextTypeStack) { Parent = node });
                isResourcePlaced = true;
            }
        }
    }
}
