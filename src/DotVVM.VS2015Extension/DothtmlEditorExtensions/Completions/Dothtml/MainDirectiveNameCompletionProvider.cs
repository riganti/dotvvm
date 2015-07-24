using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using DotVVM.Framework.Parser;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class MainDirectiveNameCompletionProvider : DirectiveNameHtmlCompletionProviderBase
    {
        public override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            var directives = new[] { Constants.BaseTypeDirective, Constants.MasterPageDirective, Constants.ViewModelDirectiveName };

            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
            return directives.Select(d => new SimpleDothtmlCompletion(d, d + " ", glyph));
        }
    }
}