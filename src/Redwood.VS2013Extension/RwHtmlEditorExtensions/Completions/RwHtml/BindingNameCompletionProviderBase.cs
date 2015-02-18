using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public abstract class BindingNameCompletionProviderBase : IRwHtmlCompletionProvider
    {
        public TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.BindingName; }
        }

        public abstract IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context);
    }
}