using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;
using Constants = Redwood.Framework.Parser.Constants;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class MasterPageDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {
        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.MasterPageDirective)
            {
                var documents = CompletionHelper.GetCurrentProjectFiles(context)
                    .Where(i => i.Name.EndsWith(".rwhtml", StringComparison.CurrentCultureIgnoreCase))
                    .Select(CompletionHelper.GetProjectItemRelativePath)
                    .ToList();

                var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphJSharpDocument, StandardGlyphItem.GlyphItemPublic);
                return documents.Select(d => new SimpleRwHtmlCompletion(d, d, glyph));
            }
            else
            {
                return Enumerable.Empty<SimpleRwHtmlCompletion>();
            }
        }
    }
}