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

        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy, string prefix)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphXmlItem, StandardGlyphItem.GlyphItemPublic);

            foreach (var n in context.MetadataControlResolver.GetElementNames(context, tagNameHierarchy))
            {
                yield return new SimpleRwHtmlCompletion(n.DisplayText.Substring(prefix.Length), n.CompletionText.Substring(prefix.Length) + " ", glyph);
            }
                
            if (tagNameHierarchy.Any())
            {
                var tagName = tagNameHierarchy[tagNameHierarchy.Count - 1];
                yield return new SimpleRwHtmlCompletion("/" + tagName, "/" + tagName + ">", null);
            }
        }

    }
}
