using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE80;
using Microsoft.Html.Editor.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Classification;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions
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

        [ImportMany(typeof(IRwHtmlCompletionProvider))]
        public IRwHtmlCompletionProvider[] CompletionProviders { get; set; }


        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var dte2 = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.12.0");

            var classifierProvider = new RwHtmlClassifierProvider()
            {
                Registry = Registry
            };
            
            var classifier = (RwHtmlClassifier)classifierProvider.GetClassifier(textBuffer);
            return new RwHtmlCompletionSource(this, new RwHtmlParser(), classifier, textBuffer, dte2);
        }
    }
}