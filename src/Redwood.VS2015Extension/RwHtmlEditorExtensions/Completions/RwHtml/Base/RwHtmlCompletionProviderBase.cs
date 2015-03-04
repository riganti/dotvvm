using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

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