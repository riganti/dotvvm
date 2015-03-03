using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(RwHtmlCompletionProviderBase))]
    public class MainTagNameCompletionProvider : TagNameHtmlCompletionProviderBase
    {

        public override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            // TODO: get control tag names
            return Enumerable.Empty<SimpleRwHtmlCompletion>();
        }

    }
}
