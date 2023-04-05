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
    /// <summary> Inserts <see cref="BodyResourceLinks" /> and <see cref="HeadResourceLinks"/> into head or body element </summary>
    public class ResourceLinksVisitor : ResolvedControlTreeVisitor
    {
        private readonly IControlResolver controlResolver;

        private bool headLinksFound = false;
        private bool bodyLinksFound = false;

        public ResourceLinksVisitor(IControlResolver controlResolver)
        {
            this.controlResolver = controlResolver;
        }


        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            // skip
        }

        public override void VisitControl(ResolvedControl control)
        {
            if (typeof(HeadResourceLinks).IsAssignableFrom(control.Metadata.Type))
            {
                headLinksFound = true;
            }
            else if (typeof(BodyResourceLinks).IsAssignableFrom(control.Metadata.Type))
            {
                bodyLinksFound = true;
            }
            else
            {
                base.VisitControl(control);
            }
        }

        private ResolvedControl? TryFindElement(ResolvedControl control, string tagName)
        {
            // BFS to find the outermost element
            // in case somebody mistypes <body> element into a <table>, we don't want to insert resources into it
            var queue = new Queue<ResolvedControl>();
            queue.Enqueue(control);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Metadata.Type == typeof(HtmlGenericControl) && current.ConstructorParameters?[0] is string controlTagName && tagName.Equals(controlTagName, StringComparison.OrdinalIgnoreCase))
                {
                    return current;
                }
                foreach (var child in current.Content)
                {
                    queue.Enqueue(child);
                }
            }
            return null;
        }

        private ResolvedControl CreateLinksControl(Type type, DataContextStack dataContext)
        {
            var metadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(type));
            var control = new ResolvedControl((ControlResolverMetadata)metadata, null, dataContext);
            control.SetProperty(new ResolvedPropertyValue(Internal.UniqueIDProperty, "cAuto" + type.Name));
            return control;
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            if (view.MasterPage is {})
            {
                // if there is a masterpage, this visitor has already inserted the links into it
                return;
            }
            if (!typeof(Controls.Infrastructure.DotvvmView).IsAssignableFrom(view.ControlBuilderDescriptor.ControlType))
            {
                // markup controls
                return;
            }
            if (!headLinksFound)
            {
                var headElement = TryFindElement(view, "head");
                if (headElement is {})
                {
                    headElement.Content.Add(CreateLinksControl(typeof(HeadResourceLinks), headElement.DataContextTypeStack));
                }
                else
                {
                    // no head element found, and no masterpage -> insert it at the document start
                    view.Content.Insert(0, CreateLinksControl(typeof(HeadResourceLinks), view.DataContextTypeStack));
                }
            }
            if (!bodyLinksFound)
            {
                var bodyElement = TryFindElement(view, "body");
                if (bodyElement is {})
                {
                    bodyElement.Content.Add(CreateLinksControl(typeof(BodyResourceLinks), bodyElement.DataContextTypeStack));
                }
                else
                {
                    // no body element found, and no masterpage -> insert it at the document end
                    view.Content.Add(CreateLinksControl(typeof(BodyResourceLinks), view.DataContextTypeStack));
                }
            }
            base.VisitView(view);
        }
    }
}
