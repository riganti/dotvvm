using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name("Redwood IntelliSense")]
    [ContentType(RwHtmlContentTypeDefinitions.RwHtmlContentType)]
    public class RwHtmlCompletionSourceProvider : ICompletionSourceProvider
    {

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IClassificationTypeRegistryService Registry;

        [Import]
        internal IGlyphService GlyphService;

        [ImportMany(typeof(IRwHtmlCompletionProvider))]
        public IRwHtmlCompletionProvider[] CompletionProviders { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        public VisualStudioWorkspace Workspace { get; set; }
        

        private RedwoodConfigurationProvider configurationProvider;
        private MetadataControlResolver metadataControlResolver;



        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var classifierProvider = new RwHtmlClassifierProvider()
            {
                Registry = Registry
            };
            
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
            configurationProvider = new RedwoodConfigurationProvider();
            metadataControlResolver = new MetadataControlResolver();

            WatchWorkspaceChanges();
            
            var classifier = (RwHtmlClassifier)classifierProvider.GetClassifier(textBuffer);
            return new RwHtmlCompletionSource(this, new RwHtmlParser(), classifier, textBuffer, 
                Workspace, GlyphService, dte, configurationProvider, metadataControlResolver);
        }




        private void WatchWorkspaceChanges()
        {
            CompletionRefreshHandler.Instance.RefreshCompletion += (sender, args) => FireWorkspaceChanged();

            Workspace.WorkspaceChanged += (sender, args) => CompletionRefreshHandler.Instance.NotifyRefreshNeeded();
            configurationProvider.WorkspaceChanged += (sender, args) => CompletionRefreshHandler.Instance.NotifyRefreshNeeded();
        } 

        private void FireWorkspaceChanged()
        {
            foreach (var provider in CompletionProviders)
            {
                provider.OnWorkspaceChanged();
            }
            metadataControlResolver.OnWorkspaceChanged();
        }
    }
}