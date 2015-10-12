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
    public class MainTagNameCompletionProvider : TagNameHtmlCompletionProviderBase
    {
        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphXmlItem, StandardGlyphItem.GlyphItemPublic);

            var tagNames = context.MetadataControlResolver.GetElementNames(context, tagNameHierarchy).ToList();
            foreach (var n in tagNames)
            {
                yield return new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText + " ", glyph);
            }

            if (tagNameHierarchy.Any())
            {
                var tagName = tagNameHierarchy[tagNameHierarchy.Count - 1];
                yield return new SimpleDothtmlCompletion("/" + tagName, "/" + tagName + ">", null);
            }
        }
    }
}