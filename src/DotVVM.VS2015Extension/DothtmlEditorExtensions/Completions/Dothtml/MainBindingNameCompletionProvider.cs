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
    public class MainBindingNameCompletionProvider : BindingNameCompletionProviderBase
    {
        public override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            var bindingTypes = new[] { Constants.CommandBinding, Constants.ValueBinding, Constants.ResourceBinding };
            var userControlBindingTypes = new[] { Constants.ControlStateBinding, Constants.ControlCommandBinding, Constants.ControlPropertyBinding };

            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
            return Enumerable.Concat(bindingTypes, userControlBindingTypes).Select(b => new SimpleDothtmlCompletion(b, b + ": ", glyph));
        }
    }
}