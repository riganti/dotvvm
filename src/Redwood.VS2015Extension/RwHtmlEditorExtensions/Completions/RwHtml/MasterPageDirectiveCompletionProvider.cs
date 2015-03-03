using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Redwood.Framework.Parser;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class MasterPageDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {
        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.MasterPageDirective)
            {
                // TODO: retrieve list of *.RWMASTER files in the project
                return Enumerable.Empty<SimpleRwHtmlCompletion>();
            }
            else
            {
                return Enumerable.Empty<SimpleRwHtmlCompletion>();
            }
        }
    }
}