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
    public class MainTagAttributeValueCompletionProvider : TagAttributeValueHtmlCompletionProviderBase
    {
        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, List<string> tagNameHierarchy, string attributeName)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupEnum, StandardGlyphItem.GlyphItemPublic);
            return context.MetadataControlResolver.GetControlAttributeValues(context, tagNameHierarchy, attributeName)
                .Select(n => new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText, glyph));
        }
    }
}