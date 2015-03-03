using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Redwood.Framework.Parser;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class MainDirectiveNameCompletionProvider : DirectiveNameHtmlCompletionProviderBase
    {
        public override IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context)
        {
            var directives = new[] { Constants.BaseTypeDirective, Constants.MasterPageDirective, Constants.ViewModelDirectiveName };
            return directives.Select(d => new SimpleRwHtmlCompletion(d, d + " "));
        }
    }
}