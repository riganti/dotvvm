using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public abstract class TagAttributeValueHtmlCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagAttributeValue; }
        }

        public sealed override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            if (context.CurrentNode is DothtmlAttributeNode)
            {
                var tagNameHierarchy = GetTagHierarchy(context);

                string attributeName = null;
                for (int i = context.CurrentTokenIndex - 1; i >= 0; i--)
                {
                    if (context.Tokens[i].Type == DothtmlTokenType.Text)
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
            return Enumerable.Empty<SimpleDothtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, List<string> tagNameHierarchy, string attributeName);
    }
}