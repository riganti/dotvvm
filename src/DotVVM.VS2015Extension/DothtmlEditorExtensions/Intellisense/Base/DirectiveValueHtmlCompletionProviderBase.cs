using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public abstract class DirectiveValueHtmlCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint => TriggerPoint.DirectiveValue;

        public override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            if (context.CurrentNode is DothtmlDirectiveNode)
            {
                var directiveName = ((DothtmlDirectiveNode)context.CurrentNode).Name;
                return GetItemsCore(context, directiveName);
            }
            return Enumerable.Empty<SimpleDothtmlCompletion>();
        }

        protected abstract IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, string directiveName);
    }
}