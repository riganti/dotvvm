using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public abstract class DothtmlCompletionProviderBase : IDothtmlCompletionProvider
    {
        public abstract TriggerPoint TriggerPoint { get; }

        public abstract IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context);

        public virtual void OnWorkspaceChanged()
        {
        }

        protected List<string> GetTagHierarchy(DothtmlCompletionContext context)
        {
            var hierarchy = new List<string>();

            var node = context.CurrentNode as DothtmlElementNode;
            if (node == null && context.CurrentNode is DothtmlAttributeNode)
            {
                node = ((DothtmlAttributeNode)context.CurrentNode).ParentElement;
            }

            while (node != null)
            {
                hierarchy.Add(node.FullTagName);
                node = node.ParentElement;
            }

            hierarchy.Reverse();
            return hierarchy;
        }
    }
}