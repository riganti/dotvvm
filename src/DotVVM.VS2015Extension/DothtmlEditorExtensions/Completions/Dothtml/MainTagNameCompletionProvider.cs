using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using Microsoft.VisualStudio.Language.Intellisense;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
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
                yield return new SimpleDothtmlCompletion(n.DisplayText, n.CompletionText, glyph);
            }
                
            if (tagNameHierarchy.Any())
            {
                var tagName = tagNameHierarchy[tagNameHierarchy.Count - 1];
                yield return new SimpleDothtmlCompletion("/" + tagName, "/" + tagName + ">", null);
            }
        }

    }
}
