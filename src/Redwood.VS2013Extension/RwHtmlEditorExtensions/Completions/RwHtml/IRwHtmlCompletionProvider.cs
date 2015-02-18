using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public interface IRwHtmlCompletionProvider
    {

        TriggerPoint TriggerPoint { get; }

        IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context);

    }
}