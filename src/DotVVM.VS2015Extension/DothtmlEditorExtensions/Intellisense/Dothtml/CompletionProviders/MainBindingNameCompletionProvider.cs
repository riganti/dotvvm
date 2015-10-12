using DotVVM.Framework.Parser;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.CompletionProviders
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class MainBindingNameCompletionProvider : BindingNameCompletionProviderBase
    {
        public override IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context)
        {
            var bindingTypes = new[] { Constants.CommandBinding, Constants.ValueBinding, Constants.ResourceBinding };
            var userControlBindingTypes = new[] { Constants.StaticCommandBinding, Constants.ControlCommandBinding, Constants.ControlPropertyBinding };

            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
            return Enumerable.Concat(bindingTypes, userControlBindingTypes).Select(b => new SimpleDothtmlCompletion(b, b + ": ", glyph));
        }
    }
}