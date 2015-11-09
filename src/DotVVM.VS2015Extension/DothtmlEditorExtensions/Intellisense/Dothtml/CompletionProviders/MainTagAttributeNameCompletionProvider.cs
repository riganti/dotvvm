using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions.CustomCommit;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.CompletionProviders
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
                    .Select(n => new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText, glyph)
                    {
                        CustomCommit = new MainTagAttributeNameCustomCommit(context)
                    }),
                context.MetadataControlResolver.GetAttachedPropertyNames(context)
                    .Select(n => new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText, glyph2)
                    {
                        CustomCommit = new MainTagAttributeNameCustomCommit(context)
                    })
            );

            CombineWithHtmlCompletions = keepHtmlAttributes;

            return results;
        }
    }
}