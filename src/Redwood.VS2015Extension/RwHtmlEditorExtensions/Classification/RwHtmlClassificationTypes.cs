using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification
{
    internal static class RwHtmlClassificationTypes
    {
        public const string Name = "RWHTML";
        public const string BindingBrace = "RwHtml_BindingBrace";
        public const string BindingContent = "RwHtml_BindingContent";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(BindingBrace)]
        internal static ClassificationTypeDefinition RwHtmlBindingBraceClassificationTypeDefinition = null;

        [Export(typeof (ClassificationTypeDefinition))] 
        [Name(BindingContent)] 
        internal static ClassificationTypeDefinition RwHtmlBindingContentClassificationTypeDefinition = null;
    }
} 
