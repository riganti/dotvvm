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

        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, List<string> tagNameHierarchy)
        {
            var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);
            return context.MetadataControlResolver.GetControlAttributeNames(context, tagNameHierarchy)
                .Select(n => new SimpleRwHtmlCompletion(n.DisplayText, n.CompletionText, glyph));
        }

    }
}