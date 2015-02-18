using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public class MainDirectiveNameHtmlCompletionProviderBase : DirectiveNameHtmlCompletionProviderBase
    {
        public override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            var directives = new[] { Constants.BaseTypeDirective, Constants.MasterPageDirective, Constants.ViewModelDirectiveName };
            return directives.Select(d => new SimpleRwHtmlCompletion(d, d + " "));
        }
    }
}