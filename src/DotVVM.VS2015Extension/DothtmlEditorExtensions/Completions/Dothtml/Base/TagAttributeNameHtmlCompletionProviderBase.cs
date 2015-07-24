using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public abstract class TagAttributeNameHtmlCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagAttributeName; }
        }

        public sealed override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            if (context.CurrentNode is DothtmlElementNode || context.CurrentNode is DothtmlAttributeNode)
            {
                var tagNameHierarchy = GetTagHierarchy(context);

                // if the tag has already some attributes, don't show them in the IntelliSense
                var tag = context.CurrentNode as DothtmlElementNode ?? ((DothtmlAttributeNode)context.CurrentNode).ParentElement;
                var existingAttributeNames = tag.Attributes.Select(a => a.AttributeName).ToList();

                return GetItemsCore(context, tagNameHierarchy)
                    .Where(n => !existingAttributeNames.Contains(n.DisplayText));
            }
            return Enumerable.Empty<SimpleDothtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, List<string> tagNameHierarchy);
    }
}