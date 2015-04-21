using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class MainTagAttributeNameCompletionProvider : TagAttributeNameHtmlCompletionProviderBase
    {

        public bool CombineWithHtmlCompletions { get; set; }

        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
            var glyph2 = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemShortcut);

            var keepHtmlAttributes = false;
            var results = Enumerable.Concat(
                context.MetadataControlResolver.GetControlAttributeNames(context, tagNameHierarchy, out keepHtmlAttributes)
                    .Select(n => new SimpleRwHtmlCompletion(n.DisplayText, n.CompletionText, glyph)),
                context.MetadataControlResolver.GetAttachedPropertyNames(context)
                    .Select(n => new SimpleRwHtmlCompletion(n.DisplayText, n.CompletionText, glyph2))
            );

            CombineWithHtmlCompletions = keepHtmlAttributes;

            return results;
        }
    }

}