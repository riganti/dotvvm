using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using Microsoft.VisualStudio.Language.Intellisense;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class MainTagAttributeNameCompletionProvider : TagAttributeNameHtmlCompletionProviderBase
    {

        public bool CombineWithHtmlCompletions { get; set; }

        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
            var glyph2 = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemShortcut);

            var keepHtmlAttributes = false;
            var results = Enumerable.Concat(
                context.MetadataControlResolver.GetControlAttributeNames(context, tagNameHierarchy, out keepHtmlAttributes)
                    .Select(n => new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText, glyph)),
                context.MetadataControlResolver.GetAttachedPropertyNames(context)
                    .Select(n => new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText, glyph2))
            );

            CombineWithHtmlCompletions = keepHtmlAttributes;

            return results;
        }
    }

}