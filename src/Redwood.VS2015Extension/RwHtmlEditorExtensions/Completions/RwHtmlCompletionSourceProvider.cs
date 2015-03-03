using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name("RWHTML Element Name Completion")]
    [ContentType(RwHtmlContentTypeDefinitions.RwHtmlContentType)]
    public class RwHtmlCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IClassificationTypeRegistryService Registry = null;

        [ImportMany(typeof(RwHtmlCompletionProviderBase))]
        public RwHtmlCompletionProviderBase[] CompletionProviders { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        public VisualStudioWorkspace Workspace { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var classifierProvider = new RwHtmlClassifierProvider()
            {
                Registry = Registry
            };
            
            var classifier = (RwHtmlClassifier)classifierProvider.GetClassifier(textBuffer);
            return new RwHtmlCompletionSource(this, new RwHtmlParser(), classifier, textBuffer, Workspace);
        }
    }
}