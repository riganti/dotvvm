using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public abstract class DirectiveValueHtmlCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.DirectiveValue; }
        }

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