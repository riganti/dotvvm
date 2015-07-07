using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using Constants = DotVVM.Framework.Parser.Constants;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class MasterPageDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {
        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.MasterPageDirective)
            {
                var documents = CompletionHelper.GetCurrentProjectFiles(context)
                    .Where(i => i.Name.EndsWith(".dothtml", StringComparison.CurrentCultureIgnoreCase))
                    .Select(CompletionHelper.GetProjectItemRelativePath)
                    .ToList();

                var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphJSharpDocument, StandardGlyphItem.GlyphItemPublic);
                return documents.Select(d => new SimpleDothtmlCompletion(d, d, glyph));
            }
            else
            {
                return Enumerable.Empty<SimpleDothtmlCompletion>();
            }
        }
    }
}