using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class MainTagNameCompletionProvider : TagNameHtmlCompletionProviderBase
    {

        public override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            // TODO: get control tag names
            return Enumerable.Empty<SimpleRwHtmlCompletion>();
        }

    }
}
