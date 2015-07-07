using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public abstract class DothtmlCompletionProviderBase : IDothtmlCompletionProvider
    {
        public abstract TriggerPoint TriggerPoint { get; }

        public abstract IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context);
        


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

        public virtual void OnWorkspaceChanged()
        {
        }
    }
}