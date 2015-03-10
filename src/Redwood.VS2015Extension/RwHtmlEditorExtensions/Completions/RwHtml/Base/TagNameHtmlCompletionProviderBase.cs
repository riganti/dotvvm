using Redwood.Framework.Parser.RwHtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public abstract class TagNameHtmlCompletionProviderBase : RwHtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagName; }
        }

        public sealed override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            if (context.CurrentNode is RwHtmlElementNode)
            {
                var tagNameHierarchy = GetTagHierarchy(context);
                
                if (tagNameHierarchy.Any())
                {
                    tagNameHierarchy.RemoveAt(tagNameHierarchy.Count - 1);
                }

                return GetItemsCore(context, tagNameHierarchy);
            }
            return Enumerable.Empty<SimpleRwHtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy);
    }
}