using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public abstract class TagAttributeValueHtmlCompletionProviderBase : RwHtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagAttributeValue; }
        }

        public sealed override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            if (context.CurrentNode is RwHtmlAttributeNode)
            {
                var tagNameHierarchy = GetTagHierarchy(context);

                string attributeName = null;
                for (int i = context.CurrentTokenIndex - 1; i >= 0; i--)
                {
                    if (context.Tokens[i].Type == RwHtmlTokenType.Text)
                    {
                        attributeName = context.Tokens[i].Text;
                        break;
                    }
                }
                if (attributeName != null)
                {
                    return GetItemsCore(context, tagNameHierarchy, attributeName);
                }
            }
            return Enumerable.Empty<SimpleRwHtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy, string attributeName);
    }
}