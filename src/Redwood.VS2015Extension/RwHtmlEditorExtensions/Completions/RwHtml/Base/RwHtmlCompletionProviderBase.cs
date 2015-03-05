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

        public event EventHandler<WorkspaceChangeEventArgs> WorkspaceChanged;


        public RwHtmlCompletionProviderBase()
        {
            WorkspaceChanged += OnWorkspaceChanged; 
        }


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

        protected virtual void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs workspaceChangeEventArgs)
        {
        }

        public virtual void OnWorkspaceChanged(WorkspaceChangeEventArgs e)
        {
            if (WorkspaceChanged != null)
            {
                WorkspaceChanged(this, e);
            }
        }
    }
}