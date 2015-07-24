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
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    [Export(typeof(ICompletionSourceProvider))]
    [Export(typeof(DothtmlCompletionSourceProvider))]
    [Name("DotVVM IntelliSense")]
    [ContentType(DothtmlContentTypeDefinitions.DothtmlContentType)]
    public class DothtmlCompletionSourceProvider : ICompletionSourceProvider
    {

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IClassificationTypeRegistryService Registry;

        [Import]
        internal IGlyphService GlyphService;

        [ImportMany(typeof(IDothtmlCompletionProvider))]
        public IDothtmlCompletionProvider[] CompletionProviders { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        public VisualStudioWorkspace Workspace { get; set; }


        public DotvvmConfigurationProvider ConfigurationProvider { get; private set; }

        public MetadataControlResolver MetadataControlResolver { get; private set; }


        public DothtmlParser Parser { get; private set; }

        public DothtmlClassifier Classifier { get; private set; }
        

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var classifierProvider = new DothtmlClassifierProvider()
                {
                    Registry = Registry
                };

                ConfigurationProvider = new DotvvmConfigurationProvider();
                MetadataControlResolver = new MetadataControlResolver();

                WatchWorkspaceChanges();

                Parser = new DothtmlParser();
                Classifier = (DothtmlClassifier)classifierProvider.GetClassifier(textBuffer);

                return new DothtmlCompletionSource(this, Parser, Classifier, textBuffer,
                    Workspace, GlyphService, CompletionHelper.DTE, ConfigurationProvider, MetadataControlResolver);
            });
        }




        private void WatchWorkspaceChanges()
        {
            CompletionRefreshHandler.Instance.RefreshCompletion += (sender, args) => FireWorkspaceChanged();

            Workspace.WorkspaceChanged += (sender, args) => CompletionRefreshHandler.Instance.NotifyRefreshNeeded();
            ConfigurationProvider.WorkspaceChanged += (sender, args) => CompletionRefreshHandler.Instance.NotifyRefreshNeeded();
        } 

        private void FireWorkspaceChanged()
        {
            foreach (var provider in CompletionProviders)
            {
                provider.OnWorkspaceChanged();
            }
            MetadataControlResolver.OnWorkspaceChanged();
        }
    }
}