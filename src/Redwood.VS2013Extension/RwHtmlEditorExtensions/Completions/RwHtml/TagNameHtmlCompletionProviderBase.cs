using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public abstract class TagNameHtmlCompletionProviderBase : IRwHtmlCompletionProvider
    {
        public TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagName; }
        }

        public abstract IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context);
    }
}