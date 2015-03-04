using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
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

            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
            return directives.Select(d => new SimpleRwHtmlCompletion(d, d + " ", glyph));
        }
    }
}