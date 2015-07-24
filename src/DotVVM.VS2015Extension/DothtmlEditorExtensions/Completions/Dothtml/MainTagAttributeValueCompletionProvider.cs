using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using Microsoft.VisualStudio.Language.Intellisense;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
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