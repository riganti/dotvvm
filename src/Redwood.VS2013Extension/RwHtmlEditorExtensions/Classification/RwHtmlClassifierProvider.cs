using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("htmlx")]
    internal class RwHtmlClassifierProvider : IClassifierProvider
    {
        [Import] 
        internal IClassificationTypeRegistryService Registry = null;


        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new RwHtmlClassifier(Registry, textBuffer));
        }
    }
}
