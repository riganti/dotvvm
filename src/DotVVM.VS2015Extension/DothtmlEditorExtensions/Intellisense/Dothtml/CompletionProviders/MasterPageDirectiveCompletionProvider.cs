using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Constants = DotVVM.Framework.Parser.Constants;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.CompletionProviders
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class MasterPageDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {
        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.MasterPageDirective)
            {
                var documents = CompletionHelper.GetCurrentProjectFiles(context)
                    .Where(i => i.Name.EndsWith(".dotmaster", StringComparison.CurrentCultureIgnoreCase))
                    .Select(DTEHelper.GetProjectItemRelativePath)
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