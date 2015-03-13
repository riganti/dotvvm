using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Parser;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public abstract class DirectiveValueHtmlCompletionProviderBase : RwHtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.DirectiveValue; }
        }

        public override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            if (context.CurrentNode is RwHtmlDirectiveNode)
            {
                var directiveName = ((RwHtmlDirectiveNode)context.CurrentNode).Name;
                return GetItemsCore(context, directiveName);
            }
            return Enumerable.Empty<SimpleRwHtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, string directiveName);
    }
}