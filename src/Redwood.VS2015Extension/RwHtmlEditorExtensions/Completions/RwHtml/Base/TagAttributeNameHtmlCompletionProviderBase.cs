using Redwood.Framework.Parser.RwHtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public abstract class TagAttributeNameHtmlCompletionProviderBase : RwHtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagAttributeName; }
        }

        public sealed override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            if (context.CurrentNode is RwHtmlElementNode || context.CurrentNode is RwHtmlAttributeNode)
            {
                var tagNameHierarchy = GetTagHierarchy(context);

                // if the tag has already some attributes, don't show them in the IntelliSense
                var tag = context.CurrentNode as RwHtmlElementNode ?? ((RwHtmlAttributeNode)context.CurrentNode).ParentElement;
                var existingAttributeNames = tag.Attributes.Select(a => a.AttributeName).ToList();

                return GetItemsCore(context, tagNameHierarchy)
                    .Where(n => !existingAttributeNames.Contains(n.DisplayText));
            }
            return Enumerable.Empty<SimpleRwHtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy);
    }
}