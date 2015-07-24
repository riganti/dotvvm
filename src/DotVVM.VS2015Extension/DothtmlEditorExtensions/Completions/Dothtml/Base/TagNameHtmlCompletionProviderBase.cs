using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public abstract class TagNameHtmlCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.TagName; }
        }

        public sealed override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            if (context.CurrentNode is DothtmlElementNode)
            {
                var tagNameHierarchy = GetTagHierarchy(context);
                
                if (tagNameHierarchy.Any())
                {
                    tagNameHierarchy.RemoveAt(tagNameHierarchy.Count - 1);
                }

                return GetItemsCore(context, tagNameHierarchy);
            }
            return Enumerable.Empty<SimpleDothtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, List<string> tagNameHierarchy);
    }
}