using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(DothtmlContentTypeDefinitions.DothtmlContentType)]
    internal class DothtmlClassifierProvider : IClassifierProvider
    {
        [Import] 
        internal IClassificationTypeRegistryService Registry = null;


        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new DothtmlClassifier(Registry, textBuffer));
        }
    }
}
