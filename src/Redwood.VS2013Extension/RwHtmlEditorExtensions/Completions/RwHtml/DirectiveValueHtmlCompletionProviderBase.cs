using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public abstract class DirectiveValueHtmlCompletionProviderBase : IRwHtmlCompletionProvider
    {
        public TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.DirectiveValue; }
        }

        public virtual IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            var directiveName = context.Tokens[context.CurrentTokenIndex - 2].Text.Trim();
            return GetItemsCore(context, directiveName);
        }

        protected abstract IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, string directiveName);
    }
}