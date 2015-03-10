using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Redwood.Framework.Parser.RwHtml.Parser;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public abstract class RwHtmlCompletionProviderBase : IRwHtmlCompletionProvider
    {
        public abstract TriggerPoint TriggerPoint { get; }

        public abstract IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context);
        


        protected List<string> GetTagHierarchy(RwHtmlCompletionContext context)
        {
            var hierarchy = new List<string>();

            var node = context.CurrentNode as RwHtmlElementNode;
            if (node == null && context.CurrentNode is RwHtmlAttributeNode)
            {
                node = ((RwHtmlAttributeNode)context.CurrentNode).ParentElement;
            }

            while (node != null)
            {
                hierarchy.Add(node.FullTagName);
                node = node.ParentElement;
            }

            hierarchy.Reverse();
            return hierarchy;
        }

        public virtual void OnWorkspaceChanged()
        {
        }
    }
}