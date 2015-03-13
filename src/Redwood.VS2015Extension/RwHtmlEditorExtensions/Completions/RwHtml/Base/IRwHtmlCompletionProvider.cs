using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public interface IRwHtmlCompletionProvider
    {

        TriggerPoint TriggerPoint { get; }

        IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context);
        

        void OnWorkspaceChanged();
    }
}