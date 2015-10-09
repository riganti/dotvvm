using DotVVM.VS2015Extension.Configuration;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(ContentTypeDefinitions.DothtmlContentType)]
    internal class DothtmlClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService Registry = null;

        [ImportMany(typeof(IClassifierProvider))]
        internal IClassifierProvider[] AllClassifierProviders { get; set; }

        [ImportMany(typeof(ITaggerProvider))]
        internal ITaggerProvider[] AllTaggerProviders { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new DothtmlClassifier(Registry, textBuffer, AllClassifierProviders, AllTaggerProviders));
        }
    }
}