using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class MainTagNameCompletionProvider : TagNameHtmlCompletionProviderBase
    {

        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphXmlItem, StandardGlyphItem.GlyphItemPublic);

            var tagNames = context.MetadataControlResolver.GetElementNames(context, tagNameHierarchy).ToList();
            foreach (var n in tagNames)
            {
                yield return new SimpleRwHtmlCompletion(n.DisplayText, n.CompletionText, glyph);
            }
                
            if (tagNameHierarchy.Any())
            {
                var tagName = tagNameHierarchy[tagNameHierarchy.Count - 1];
                yield return new SimpleRwHtmlCompletion("/" + tagName, "/" + tagName + ">", null);
            }
        }

    }
}
